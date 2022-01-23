using Extension.Ext;
using Extension.Script;
using Extension.Utilities;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace Scripts
{
    [Serializable]
    class JJClusterSupply
    {
        public JJClusterSupply(int supply, int current)
        {
            this.supply = supply;
            this.current = current;
        }

        public bool CanSupply()
        {
            return current <= 0;
        }

        public int GetNextDelay() => 10;

        public int GetSupply()
        {
            if (CanSupply())
            {
                current = GetNextDelay();
                return supply;
            }

            return 0;
        }

        internal void Elapse()
        {
            current--;
        }

        int supply;
        int current;
    }

    class JJCluster
    {
        public const int NEED_ENERGY = 500;
        public const int ATTACK_RANGE = Game.CellSize * 15;

        public JJCluster()
        {
            Jumpjets = new List<JUMPJETScript>();
        }

        public JJCluster(int energy)
        {
            Jumpjets = new List<JUMPJETScript>();
            this.energy = energy;
        }

        public int Energy => energy;

        public CoordStruct Mean => Points.Aggregate((sum, next) => sum + next) * (1.0 / Points.Count);
        // for small cluster, it is ok
        public List<CoordStruct> Points => Jumpjets.Select(j => j.Owner.OwnerObject.Ref.Base.Base.GetCoords()).ToList();
        public JUMPJETScript Leader
        {
            get => Jumpjets.FirstOrDefault();
            set
            {
                if (Jumpjets.Remove(value))
                {
                    Jumpjets.Insert(0, value);
                }
            }
        }
        public List<JUMPJETScript> Jumpjets { get; }

        static Pointer<AnimTypeClass> ChargeAnimType => AnimTypeClass.ABSTRACTTYPE_ARRAY.Find("INITFIRE");
        public void Charge()
        {
            foreach (var jj in Jumpjets)
            {
                int supply = jj.Supply.GetSupply();

                if (supply > 0)
                {
                    CoordStruct mean = Mean;

                    ColorStruct chargeInnerColor = new ColorStruct(104, 10, 10);
                    ColorStruct chargeOuterColor = new ColorStruct(44, 0, 5);
                    ColorStruct chargeOuterSpread = new ColorStruct(10, 10, 10);

                    // draw laser from jumpjet to mean point
                    Pointer<LaserDrawClass> pLaser = YRMemory.Create<LaserDrawClass>(
                        jj.Owner.OwnerObject.Ref.Base.Base.GetCoords(), mean, chargeInnerColor, chargeOuterColor, chargeOuterSpread, 1);
                    pLaser.Ref.Thickness = 1;
                    pLaser.Ref.IsHouseColor = true;

                    // create merge anim on mean point
                    Pointer<AnimClass> pAnim = YRMemory.Create<AnimClass>(ChargeAnimType, mean);

                    energy += supply;
                }
            }
        }

        static Pointer<WeaponTypeClass> Weapon => WeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("RedEye2");
        static Pointer<AnimTypeClass> AttackAnimType => AnimTypeClass.ABSTRACTTYPE_ARRAY.Find("TWLT026");
        public bool Attack(Pointer<AbstractClass> pTarget)
        {
            if (!IsCharged() || pTarget.IsNull)
            {
                return false;
            }

            CoordStruct mean = Mean;

            // draw laser from jumpjets to mean point
            ColorStruct attackInnerColor = new ColorStruct(208, 10, 10);
            ColorStruct attackOuterColor = new ColorStruct(88, 0, 5);
            ColorStruct attackOuterSpread = new ColorStruct(10, 10, 10);

            foreach (CoordStruct point in Points)
            {
                Pointer<LaserDrawClass> pMergeLaser = YRMemory.Create<LaserDrawClass>(point, mean, attackInnerColor, attackOuterColor, attackOuterSpread, 10);
                pMergeLaser.Ref.Thickness = 4;
                pMergeLaser.Ref.IsHouseColor = true;
            }

            // create merge anim on mean point
            Pointer<AnimClass> pAnim = YRMemory.Create<AnimClass>(AttackAnimType, mean);

            // draw laser from mean to target
            Pointer<LaserDrawClass> pLaser = YRMemory.Create<LaserDrawClass>(mean, pTarget.Ref.GetCoords(), attackInnerColor, attackOuterColor, attackOuterSpread, 10);
            pLaser.Ref.Thickness = MathEx.Clamp(3 * Jumpjets.Count, 6, 15);
            pLaser.Ref.IsHouseColor = true;

            // launch bullet from mean point
            Pointer<BulletClass> pBullet = BulletFactory.CreateBullet(pTarget, Weapon, Leader.Owner.OwnerObject);
            pBullet.Ref.MoveTo(mean, new BulletVelocity(0, 0, 200));

            energy -= NEED_ENERGY;
            return true;
        }

        public void Attack()
        {
            if (!IsCharged())
            {
                return;
            }

            CoordStruct mean = Mean;

            foreach (var jj in Jumpjets)
            {
                var pTarget = jj.Owner.OwnerObject.Ref.Target;
                if (!pTarget.IsNull && mean.DistanceFrom(pTarget.Ref.GetCoords()) <= ATTACK_RANGE)
                {
                    Attack(pTarget);
                    return;
                }
            }

            List<Pointer<ObjectClass>> list = ObjectFinder.FindObjectsNear(mean, ATTACK_RANGE);

            foreach (var pTarget in list)
            {
                //DebugUtilities.MarkTarget(pTarget.Convert<AbstractClass>(), new ColorStruct(0, 0, 200), 500);
            }

            if (list.Count > 0)
            {
                Pointer<HouseClass> pOwnerHouse = Leader.Owner.OwnerObject.Ref.Owner;

                list = list
                    .Where(pObject => pObject.Ref.IsAttackable() && !pOwnerHouse.Ref.IsAlliedWith(pObject) && CanAttack(pObject.Ref.Base.WhatAmI()))
                    .OrderByDescending(pObject => GetAttackPriority(pObject.Ref.Base.WhatAmI()) + GetHousePriority(pObject.Ref.Base.GetOwningHouse()))
                    .ToList();

                foreach (Pointer<ObjectClass> pTarget in list)
                {
                    var targetLoc = pTarget.Ref.Base.GetCoords();
                    var pTargetOwner = pTarget.Ref.Base.GetOwningHouse();

                    if (MapClass.Instance.TryGetCellAt(targetLoc, out var pCell))
                    {
                        //DebugUtilities.HighlightCell(pCell, new ColorStruct(200, 0, 0));
                    }

                    if (!Attack(pTarget.Convert<AbstractClass>()))
                        return;

                    //DynamicPatcher.Logger.Log("attack {0}(owned by {1}) located at {2}, {3}, {4}",
                    //    DebugUtilities.GetAbstractID(pTarget.Convert<AbstractClass>()),
                    //    pTargetOwner.IsNull ? "nullptr" : DebugUtilities.GetAbstractID(pTargetOwner.Convert<AbstractClass>()),
                    //    targetLoc.X, targetLoc.Y, targetLoc.Z);
                }
            }
        }

        public bool CanAttack(AbstractType type)
        {
            return type switch
            {
                AbstractType.Unit => true,
                AbstractType.Aircraft => true,
                AbstractType.Anim => false,
                AbstractType.Building => true,
                AbstractType.Bullet => false,
                AbstractType.Infantry => true,
                AbstractType.BuildingLight => false,
                AbstractType.Particle => false,
                AbstractType.ParticleSystem => false,
                AbstractType.Terrain => true,
                AbstractType.VoxelAnim => false,
                AbstractType.Wave => false,
                _ => false,
            };
        }

        public int GetAttackPriority(AbstractType type)
        {
            return type switch
            {
                AbstractType.Unit => 100,
                AbstractType.Aircraft => 120,
                AbstractType.Anim => 0,
                AbstractType.Building => 90,
                AbstractType.Bullet => 0,
                AbstractType.Infantry => 110,
                AbstractType.BuildingLight => 0,
                AbstractType.Particle => 0,
                AbstractType.ParticleSystem => 0,
                AbstractType.Terrain => 50,
                AbstractType.VoxelAnim => 0,
                AbstractType.Wave => 0,
                _ => 0,
            };
        }

        public int GetHousePriority(Pointer<HouseClass> pHouse)
        {
            if (pHouse.IsNull)
            {
                return 100;
            }

            if (pHouse == HouseClass.FindNeutral())
            {
                return 150;
            }

            return 200;
        }

        public bool IsCharged()
        {
            return Jumpjets.Count > 0 && energy >= NEED_ENERGY;
        }

        public void Add(JUMPJETScript jj)
        {
            Jumpjets.Add(jj);
            jj.Cluster = this;
        }
        public void Add(IEnumerable<JUMPJETScript> list)
        {
            Jumpjets.AddRange(list);
            foreach (JUMPJETScript jj in list)
            {
                jj.Cluster = this;
            }
        }

        public void Remove(JUMPJETScript jj)
        {
            Jumpjets.Remove(jj);
            jj.Cluster = null;
        }

        public void Clear()
        {
            foreach (JUMPJETScript jj in Jumpjets)
            {
                jj.Cluster = null;
            }

            Jumpjets.Clear();
        }

        public bool IsOutOfRange(JUMPJETScript jj, double range)
        {
            return Jumpjets.Count > 0 && jj.Owner.OwnerObject.Ref.Base.Base.GetCoords().DistanceFrom(Mean) > range;
        }

        private int energy;
    }

    static class JJClusterDiscoverer
    {
        public static int ClusterRange { get; } = Game.CellSize * 5;
        public static int ClusterCapacity { get; } = 5;
        public static int ClusterStartNum { get; } = 2;

        private static List<JUMPJETScript> GetJJList()
        {
            var list = new List<JUMPJETScript>();

            foreach (Pointer<TechnoClass> pTechno in TechnoClass.Array)
            {
                var ext = TechnoExt.ExtMap.Find(pTechno);
                if (ext.Scriptable is JUMPJETScript jj)
                {
                    list.Add(jj);
                }
            }

            return list;
        }

        private static void UpdateClusters()
        {
            List<JUMPJETScript> list = cacheList;

            foreach (JUMPJETScript jj in list)
            {
                jj.Supply.Elapse();

                JJCluster cluster = jj.Cluster;

                if (cluster != null && cluster.Leader == jj)
                {
                    cluster.Charge();
                }
            }
        }

        private static void CleanClusters()
        {
            List<JUMPJETScript> list = cacheList;
            foreach (JUMPJETScript jj in list)
            {
                JJCluster cluster = jj.Cluster;

                if (cluster != null)
                {
                    if (cluster.IsOutOfRange(jj, ClusterRange))
                    {
                        cluster.Remove(jj);
                    }

                    if (cluster.Jumpjets.Count < ClusterStartNum)
                    {
                        cluster.Clear();
                    }
                }
            }
        }

        private static void DiscoverClusters()
        {
            List<JUMPJETScript> list = cacheList;
            List<CoordStruct> points = list.Select(jj => jj.Owner.OwnerObject.Ref.Base.Base.GetCoords()).ToList();

            List<int> restPoints = Enumerable.Range(0, list.Count).ToList();

            while (restPoints.Count > 0)
            {
                // pick first point as leader
                int leaderIdx = restPoints[0];
                var leaderOwner = list[leaderIdx].Owner.OwnerObject.Ref.Owner;
                // include leader
                List<int> nearPoints = restPoints.Where(
                    idx => points[idx].DistanceFrom(points[leaderIdx]) <= ClusterRange
                    && leaderOwner.Ref.IsAlliedWith(list[idx].Owner.OwnerObject.Ref.Owner)).ToList();

                if (nearPoints.Count >= ClusterStartNum)
                {
                    if (nearPoints.Count > ClusterCapacity)
                    {
                        // nearPoints = nearPoints[:ClusterCapacity]
                        nearPoints = nearPoints.GetRange(0, ClusterCapacity);
                    }

                    var jumpjets = nearPoints.Select(p => list[p]).ToArray();
                    // inherit from last max energy cluster
                    var cluster = new JJCluster(jumpjets.Max(jj => jj.Cluster?.Leader == jj ? jj.Cluster.Energy : 0));
                    cluster.Add(jumpjets);
                }
                else
                {
                    // clean cluster for small group
                    foreach (var idx in nearPoints)
                    {
                        list[idx].Cluster = null;
                    }
                }

                // restPoints = restPoints - nearPoints
                restPoints = restPoints.Except(nearPoints).ToList();
            }
        }

        public static void Update()
        {
            if (currentFrame == Game.CurrentFrame)
            {
                return;
            }

            cacheList = GetJJList();
            UpdateClusters();
            CleanClusters();
            DiscoverClusters();

            currentFrame = Game.CurrentFrame;
        }

        private static int currentFrame = 0;
        private static List<JUMPJETScript> cacheList;
    }

    [Serializable]
    public class JUMPJETScript : TechnoScriptable
    {
        public JUMPJETScript(TechnoExt owner) : base(owner)
        {
            Supply = new JJClusterSupply(50, 0);
        }

        internal JJClusterSupply Supply { get; }
        internal JJCluster Cluster { get => cluster; set => cluster = value; }

        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
        }

        public override void OnUpdate()
        {
            JJClusterDiscoverer.Update();

            Cluster?.Attack();
        }

        public override void OnRemove()
        {
            Cluster?.Remove(this);
        }

        public override void SaveToStream(IStream stream)
        {
            bool isLeader = cluster != null && cluster.Leader == this;
            stream.Write(isLeader);
            if (isLeader)
            {
                stream.Write(cluster.Energy);
            }
        }

        public override void LoadFromStream(IStream stream)
        {
            bool isLeader = false;
            stream.Read(ref isLeader);
            if (isLeader)
            {
                int energy = 0;
                stream.Read(ref energy);
                cluster = new JJCluster(energy);
            }
        }

        [NonSerialized]
        JJCluster cluster;

        static JUMPJETScript()
        {
            //var random = new Random();
            //var e1 = TechnoTypeClass.ABSTRACTTYPE_ARRAY.Find("E1");
            //for (int i = 0; i < 10000; i++)
            //{
            //    var pTechno = e1.Ref.Base.CreateObject(HouseClass.Player).Convert<TechnoClass>();
            //    pTechno.Ref.Base.Put(new CoordStruct(random.Next(40000), random.Next(40000), random.Next(40000)), Direction.N);

            //    DynamicPatcher.Logger.Log("current e1 idx :{0}", i);
            //}

            //HouseClass.Player.Ref.GiveMoney(114514);
        }
    }
}
