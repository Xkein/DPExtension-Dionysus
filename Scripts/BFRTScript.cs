using Extension.Decorators;
using Extension.Ext;
using Extension.Script;
using Extension.Utilities;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scripts
{
    [Serializable]
    class PatrolPath
    {
        public CoordStruct this[int index] => PatrolPoints[index];
        public int Length => PatrolPoints.Count;
        public List<CoordStruct> PatrolPoints { get; }
        public PatrolPath(List<CoordStruct> patrolPoints)
        {
            PatrolPoints = patrolPoints;
        }

    }
    
    [Serializable]
    class EscortGroup
    {
        public static readonly DecoratorId ID = new DecoratorId(typeof(EscortGroup).GetHashCode());
        public ExtensionReference<TechnoExt> Princess { get; set; }
        public List<ExtensionReference<TechnoExt>> Escorts { get; set; }
        public List<ExtensionReference<TechnoTypeExt>> EscortTypes { get; set; }
        public PatrolPath PatrolPath { get; set; }
        public int CloseEnough { get; set; } = Game.CellSize;
        public int RecreateDelay { get; set; } = 500;
        public int GuardRange { get; set; } = Game.CellSize * 15;

        public EscortGroup(TechnoExt princess, List<TechnoTypeExt> escortTypes, List<CoordStruct> patrolPoints)
        {
            Princess = princess;
            Escorts = new List<ExtensionReference<TechnoExt>>();
            EscortTypes = escortTypes.Select(t => new ExtensionReference<TechnoTypeExt>(t)).ToList();
            PatrolPath = new PatrolPath(patrolPoints);
            _delay = RecreateDelay;
        }

        public void Add(TechnoExt ext)
        {
            ext.DecoratorComponent.CreateDecorator<PairDecorator<string, int>>(ID, "current point index", "patrol_index", 0);
            Escorts.Add(ext);
        }

        public void Remove(TechnoExt ext)
        {
            ext.DecoratorComponent.Remove(ID);
            Escorts.Remove(ext);
        }

        public CoordStruct GetPatrolDestination(int patrolIndex)
        {
            return ((TechnoExt)Princess).OwnerObject.Ref.Base.Base.GetCoords() + PatrolPath[patrolIndex];
        }

        public List<TechnoTypeExt> GetWastedEscortTypes()
        {
            var escortTypes = EscortTypes.Select(t => (TechnoTypeExt)t).ToList();
            var existingTypes = Escorts.Select(e => ((TechnoExt)e).Type);

            foreach (var type in existingTypes)
            {
                escortTypes.Remove(type);
            }

            return escortTypes;
        }
        private void CreateEscorts()
        {
            if (_delay > 0)
            {
                _delay--;
                return;
            }

            if (EscortTypes.Count <= Escorts.Count)
                return;

            List<TechnoTypeExt> types = GetWastedEscortTypes();

            if (CreateEscort(types[MathEx.Random.Next(types.Count)]))
            {
                _delay += RecreateDelay;
            }
        }

        private bool CreateEscort(TechnoTypeExt type)
        {
            Pointer<HouseClass> pOwner = ((TechnoExt)Princess).OwnerObject.Ref.Owner;
            Pointer<TechnoClass> pTechno = type.OwnerObject.Ref.Base.CreateObject(pOwner).Convert<TechnoClass>();
            TechnoExt ext = TechnoExt.ExtMap.Find(pTechno);
            Add(ext);

            CellStruct loc = CellClass.Coord2Cell(GetPatrolDestination(ext.DecoratorComponent.GetValue<int>(ID)));
            if (pTechno.Ref.Base.Base.WhatAmI() == AbstractType.Aircraft)
            {
                if (TechnoPlacer.PlaceTechnoFromEdge(pTechno))
                {
                    pTechno.CastToFoot(out Pointer<FootClass> pFoot);
                    Pointer<FlyLocomotionClass> pFly = pFoot.Ref.Locomotor.ToLocomotionClass<FlyLocomotionClass>();

                    //pFly.Ref.IsLanding = false;
                    //pFly.Ref.IsTakingOff = 0;
                    pTechno.Ref.Ammo = 114514;
                    //pTechno.Ref.Base.Location += new CoordStruct(0, 0, 10000);

                    return true;
                }
            }
            else if (TechnoPlacer.PlaceTechnoNear(pTechno, loc))
            {
                return true;
            }

            Remove(ext);
            return false;
        }

        public void Update()
        {
            Clean();
            CreateEscorts();

            UpdateEscorts();

        }

        private void Clean()
        {
            Escorts.RemoveAll(e => e.Get() == null);
        }

        private void UpdateEscorts()
        {
            var pPrincessTechno = ((TechnoExt)Princess).OwnerObject;
            var princessLoc = pPrincessTechno.Ref.Base.Base.GetCoords();
            var princessTarget = pPrincessTechno.Ref.Target;

            foreach (var escort in Escorts)
            {
                TechnoExt ext = escort;
                int patrolIndex = ext.DecoratorComponent.GetValue<int>("patrol_index");

                ext.OwnerObject.CastToFoot(out Pointer<FootClass> pFoot);
                ILocomotion locomotor = pFoot.Ref.Locomotor;

                Pointer<AbstractClass> pTarget = ext.OwnerObject.Ref.Target;
                CoordStruct location = pFoot.Ref.Base.Base.Base.GetCoords();
                var dest = GetPatrolDestination(patrolIndex);

                // process patrol
                if (location.DistanceFrom(new CoordStruct(dest.X, dest.Y, location.Z)) <= CloseEnough)
                {
                    patrolIndex = (patrolIndex + 1) % PatrolPath.Length;
                    ext.DecoratorComponent.SetValue("patrol_index", patrolIndex);
                }
                else if(MapClass.Instance.TryGetCellAt(dest, out var pCell))
                {
                    if (pTarget.IsNull)
                    {
                        if (locomotor.Destination() != dest)
                        {
                            //pFoot.Ref.Base.SetTarget(Pointer<AbstractClass>.Zero);
                            //pFoot.Ref.Base.SetDestination(pCell.Convert<AbstractClass>(), true);
                            locomotor.Move_To(dest);
                            //DebugUtilities.MarkLocation(dest, new ColorStruct(0, 200, 0));
                            //DebugUtilities.HighlightCell(pCell, new ColorStruct(200, 0, 0));
                        }
                    }
                    else if (pFoot.Ref.Base.CanAttackMove())
                    {
                        Pointer<WeaponStruct> pWeaponStruct = pFoot.Ref.Base.GetWeapon(pFoot.Ref.Base.SelectWeapon(pTarget));
                        Pointer<WeaponTypeClass> pWeapon = pWeaponStruct.Ref.WeaponType;
                        if (pCell.Ref.Base.GetCoords().DistanceFrom(pTarget.Ref.GetCoords()) <= pWeapon.Ref.Range)
                        {
                            pFoot.Ref.AttackMove(pTarget, pCell);
                        }

                        var targetLoc = pTarget.Ref.GetCoords();
                        var pTargetOwner = pTarget.Ref.GetOwningHouse();
                        MapClass.Instance.TryGetCellAt(targetLoc, out var pTargetCell);
                        //DynamicPatcher.Logger.Log("attack {0}(owned by {1}) located at {2}, {3}, {4}",
                        //    DebugUtilities.GetAbstractID(pTarget.Convert<AbstractClass>()),
                        //    pTargetOwner.IsNull ? "nullptr" : DebugUtilities.GetAbstractID(pTargetOwner.Convert<AbstractClass>()),
                        //    targetLoc.X, targetLoc.Y, targetLoc.Z);
                        //DebugUtilities.MarkLocation(targetLoc, new ColorStruct(0, 200, 0), beamHeight: 10000);
                        //DebugUtilities.HighlightCell(pTargetCell, new ColorStruct(200, 0, 0));
                    }
                }

                // process attack
                if (!pTarget.IsNull)
                {
                    if (location.DistanceFrom(princessLoc) >= GuardRange
                        || (pTarget.Ref.WhatAmI() == AbstractType.Cell && princessTarget.IsNull))
                    {
                        pFoot.Ref.Base.SetTarget(Pointer<AbstractClass>.Zero);
                    }
                }
                else if (!princessTarget.IsNull && pFoot.Ref.Base.CanAttack(princessTarget))
                {
                    pFoot.Ref.Base.Attack(princessTarget);
                }
                else
                {
                    // find targets to attack
                    List<Pointer<ObjectClass>> list = ObjectFinder.FindTechnosNear(princessLoc, GuardRange);
                    //{ // debug
                    //    ObjectBlockContainer container = ObjectFinder.Container;
                    //    foreach (var pObject in list)
                    //    {
                    //        DebugUtilities.MarkTarget(pObject.Convert<AbstractClass>(), new ColorStruct(0, 0, 200), 500);
                    //    }
                    //    var blocks = container.GetCoveredBlocks(princessLoc, GuardRange);
                    //    foreach (var block in blocks)
                    //    {
                    //        DebugUtilities.HighlightObjectBlock(block, new ColorStruct(0, 200, 0));
                    //    }
                    //}
                    list = list.Where(o => !pFoot.Ref.Base.Owner.Ref.IsAlliedWith(o.Ref.Base.GetOwningHouse())).ToList();
                    list.Sort((l, r) => -location.DistanceFrom(l.Ref.Base.GetCoords()).CompareTo(location.DistanceFrom(r.Ref.Base.GetCoords())));
                    foreach (var pNewTarget in list)
                    {
                        if (pFoot.Ref.Base.CanAttack(pNewTarget))
                        {
                            pFoot.Ref.Base.Attack(pNewTarget.Convert<AbstractClass>());
                            break;
                        }
                    }
                }
            }
        }

        public void Attack(Pointer<AbstractClass> pTarget)
        {
            foreach (var escort in Escorts)
            {
                TechnoExt ext = escort;
                var pTechno = ext.OwnerObject;
                pTechno.Ref.Attack(pTarget);
            }
        }

        private int _delay;
    }

    [Serializable]
    public class BFRTScript : TechnoScriptable
    {
        static readonly string[] EscortNames = new[] {
                "ORCA",
                "BPLN",
                "HORNET",
                "JUMPJET",
                "JUMPJET",
                "E1",
                "HTK",
                "GHOST",
            };
        static readonly CoordStruct[] PatrolPoints = new[] {
            new CoordStruct(400, 400, 0),
            new CoordStruct(400, -400, 0),
            new CoordStruct(-400, -400, 0),
            new CoordStruct(-400, 400, 0),
             //new CoordStruct(0, 0, 0),
        };
        public BFRTScript(TechnoExt owner) : base(owner)
        {
            var escortTypes = TechnoTypeClass.ABSTRACTTYPE_ARRAY.Finds(EscortNames);
            _escortGroup = new EscortGroup(owner, escortTypes.Select(t => TechnoTypeExt.ExtMap.Find(t)).ToList(), PatrolPoints.ToList());
            //_escortGroup.CloseEnough = Game.CellSize * 3;
            //_escortGroup.RecreateDelay = 100;
            //_escortGroup.GuardRange = Game.CellSize * 10;
        }

        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            //_escortGroup.Attack(pTarget);
        }

        public override void OnUpdate()
        {
            _escortGroup.Update();
        }

        EscortGroup _escortGroup;
    }
}
