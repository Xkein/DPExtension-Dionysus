using Extension.Ext;
using Extension.INI;
using Extension.Script;
using Extension.Utilities;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;

namespace Scripts
{
    public class DiffusionWaveGunConfig : INIConfig
    {
        public override void Read(IConfigReader ini)
        {
            FirstWeapon = ini.Get("DiffusionWaveGun.FirstWeapon", WeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("Medusa"));
            BurstWeapon = ini.Get("DiffusionWaveGun.BurstWeapon", WeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("Medusa"));

            Range = ini.Get("DiffusionWaveGun.SearchRange", Game.CellSize * 5);
            BrustCount = ini.Get("DiffusionWaveGun.BrustCount", 10);

            Speed = ini.Get("DiffusionWaveGun.FirstWeaponSpeed", 0.05f);
            FlyPercent = ini.Get("DiffusionWaveGun.FlyPercent", 0.4f);
            BurstMaxAngle = ini.Get("DiffusionWaveGun.BurstMaxAngle", 72f) / 90f;
        }

        public Pointer<WeaponTypeClass> FirstWeapon;
        public Pointer<WeaponTypeClass> BurstWeapon;

        // search range
        public int Range;
        // count of BurstWeapon
        public int BrustCount;
        // the speed of FirstWeapon
        public float Speed;
        // the length percent from owner to target. the FirstWeapon will burst when reach this
        public float FlyPercent;
        public float BurstMaxAngle;
    }

    [Serializable]
    public class DiffusionWaveGun : TechnoScriptable
    {
        public DiffusionWaveGun(TechnoExt owner) : base(owner)
        {
        }

        public override void Awake()
        {
            INI = this.CreateRulesIniComponentWith<DiffusionWaveGunConfig>(Owner.OwnerTypeRef.BaseAbstractType.ID);
        }

        INIComponentWith<DiffusionWaveGunConfig> INI;

        Pointer<WeaponTypeClass> FirstWeapon => INI.Data.FirstWeapon;
        Pointer<WeaponTypeClass> BurstWeapon => INI.Data.BurstWeapon;
        int Range => INI.Data.Range;
        int BrustCount => INI.Data.BrustCount;
        float Speed => INI.Data.Speed;
        float FlyPercent => INI.Data.FlyPercent;
        float BurstMaxAngle => INI.Data.BurstMaxAngle;

        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            var pOwner = Owner.OwnerObject;
            var pBullet = BulletFactory.CreateBullet(pTarget, FirstWeapon, pOwner);
            pBullet.Ref.MoveTo(pOwner.Ref.Base.GetFLH(weaponIndex, new CoordStruct()), new BulletVelocity(0, 0, 10000));
            BulletExt ext = BulletExt.ExtMap.Find(pBullet);


            int restCount = BrustCount;
            var targetLoc = pTarget.Ref.GetCoords();
            var targets = pOwner.Ref.FindAttackTechnos(targetLoc, Range).Select(t => t.Convert<AbstractClass>()).ToList();
            restCount -= targets.Count;
            var center = CellClass.Coord2Cell(targetLoc);
            foreach (var cell in new RandomCellEnumerator(Range, restCount))
            {
                if (MapClass.Instance.TryGetCellAt(center + cell, out var pCell))
                {
                    targets.Add(pCell.Convert<AbstractClass>());
                }
            }
            ext.GameObject.CreateScriptComponent(nameof(StraightRush), "first straight rush", ext, targets, BurstWeapon, Speed, FlyPercent, BurstMaxAngle);
        }

        [Serializable]
        public class StraightRush : BulletScriptable
        {
            public StraightRush(BulletExt owner, List<Pointer<AbstractClass>> targets, Pointer<WeaponTypeClass> burstWeapon, float speed, float flyPercent, float burstMaxAngle) : base(owner)
            {
                BurstWeapon = burstWeapon;
                this.speed = speed;
                this.flyPercent = flyPercent;
                this.burstMaxAngle = burstMaxAngle;

                BrustBullets = targets.Select(t => new SwizzleablePointer<BulletClass>(BulletFactory.CreateBullet(t, BurstWeapon, owner.OwnerObject.Ref.Owner))).ToList();
            }

            CoordStruct StartCoord;
            List<SwizzleablePointer<BulletClass>> BrustBullets;
            SwizzleablePointer<WeaponTypeClass> BurstWeapon;
            float speed;
            float flyPercent;
            float burstMaxAngle;

            public override void Start()
            {
                var pMe = Owner.OwnerObject;
                StartCoord = pMe.Ref.Base.Location;
                pMe.Ref.Speed = 1;
            }

            public void Brust()
            {
                var pOwner = Owner.OwnerObject.Ref.Owner;
                var pMe = Owner.OwnerObject;

                var pWeapon = BurstWeapon;
                var location = pMe.Ref.Base.Location;

                foreach (var pBullet in BrustBullets)
                {
                    pBullet.Ref.MoveTo(location, new BulletVelocity(0, 0, 0));
                    BulletExt ext = BulletExt.ExtMap.Find(pBullet);
                    ext.GameObject.CreateScriptComponent(nameof(DiffusionEffect), "brust out", ext, burstMaxAngle);
                }
            }

            float accumulation = 0f;


            public override void OnDetonate(Pointer<CoordStruct> pCoords)
            {
                Brust();
            }
            public override void OnUpdate()
            {
                Vector3 targetLoc = Owner.OwnerObject.Ref.TargetCoords.ToVector3();
                Vector3 location = Vector3.Lerp(StartCoord.ToVector3(), targetLoc, accumulation += speed);
                Owner.OwnerObject.Ref.Base.Location = location.ToCoordStruct();
                Owner.OwnerObject.Ref.Velocity = new BulletVelocity(0, 0, 0);

                if (accumulation >= flyPercent)
                {
                    Owner.OwnerObject.Ref.Detonate(location.ToCoordStruct());
                    Owner.OwnerObject.Ref.Base.UnInit();
                    DetachFromParent();
                }
            }

            [Serializable]
            public class DiffusionEffect : BulletScriptable
            {
                public DiffusionEffect(BulletExt owner, float burstMaxAngle) : base(owner)
                {
                    this.burstMaxAngle = burstMaxAngle;
                }

                float burstMaxAngle;

                public override void Start()
                {
                    var random = MathEx.Random;
                    var pBullet = Owner.OwnerObject;
                    var arrow = Vector3.Normalize((pBullet.Ref.TargetCoords - pBullet.Ref.Base.Location).ToVector3());
                    float spinRad = (float)(random.NextDouble() * 2 * Math.PI);
                    var quat = Quaternion.CreateFromAxisAngle(arrow, spinRad);
                    flyDirection = MathEx.Slerp(arrow, -arrow, ((float)random.NextDouble() - 0.5f) * burstMaxAngle);
                    flyDirection = Vector3.Transform(flyDirection, quat);
                    flyDirection = Vector3.Normalize(flyDirection);
                    //DebugUtilities.HighlightDistance(pBullet.Ref.Base.Location, pBullet.Ref.Base.Location + (flyDirection * 1000).ToCoordStruct(), new ColorStruct(200, 0, 0));

                    //DebugUtilities.MarkLocation(pBullet.Ref.TargetCoords, new ColorStruct(0, 220, 0));
                    //DebugUtilities.HighlightDistance(pBullet.Ref.Base.Location, pBullet.Ref.TargetCoords, new ColorStruct(0, 0, 200));
                }

                [NonSerialized]
                Vector3 flyDirection;

                double force = 10.0;

                public override void OnUpdate()
                {
                    var pBullet = Owner.OwnerObject;
                    pBullet.Ref.Velocity += new BulletVelocity(flyDirection.X, flyDirection.Y, flyDirection.Z) * force;
                    force -= 1.0;

                    if (force <= 0)
                    {
                        DetachFromParent();
                    }
                }

                public override void SaveToStream(IStream stream)
                {
                    stream.Write(flyDirection);
                }
                public override void LoadFromStream(IStream stream)
                {
                    stream.Read(ref flyDirection);
                }
            }
        }
    }
}
