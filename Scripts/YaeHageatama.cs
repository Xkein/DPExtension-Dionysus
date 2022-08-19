

using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using PatcherYRpp.Utilities.Clusters;

namespace Scripts
{
    [Serializable]
    public class SesshoSakura : ICanCluster<SesshoSakura>
    {
        public ICluster<SesshoSakura> Cluster { get; set; }
        
        public CoordStruct Point => throw new NotImplementedException();

        public Pointer<HouseClass> Owner => throw new NotImplementedException();

        public void Update()
        {
            throw new NotImplementedException();
        }
    }

    [Serializable]
    public class SesshoSakuraCluster : Cluster<SesshoSakura>
    {
        public override void Update()
        {
            throw new NotImplementedException();
        }
    }

    [Serializable]
    public class SesshoSakuraClusterDiscover : ClusterDiscoverer<SesshoSakura>
    {
        public override int ClusterRange => 10 * Game.CellSize;
        public override int ClusterCapacity => 3;
        public override int ClusterStartNum => 2;
        public override List<SesshoSakura> ObjectList { get; }
        public override ICluster<SesshoSakura> CreateCluster(IEnumerable<SesshoSakura> objects)
        {
            throw new NotImplementedException();
        }
    }

    [Serializable]
    public class YaeHageatama : TechnoScriptable
    {

        public YaeHageatama(TechnoExt owner) : base(owner) { }


        public IEnumerator Jump(CoordStruct dest)
        {
            if (Owner.OwnerObject.CastToFoot(out Pointer<FootClass> pFoot))
            {
                ILocomotion locomotion = pFoot.Ref.Locomotor;

                if (MapClass.Instance.TryGetCellAt(TechnoPlacer.FindPlaceableCellNear(Owner.OwnerObject, dest), out Pointer<CellClass> pDestCell))
                {
                    dest = pDestCell.Ref.FindInfantrySubposition(dest, true, false, false);
                }

                CoordStruct start = Owner.OwnerObject.Ref.BaseAbstract.GetCoords();
                if (MapClass.Instance.TryGetCellAt(start, out Pointer<CellClass> pStartCell))
                {
                    pStartCell.Ref.RemoveContent(Owner.OwnerObject.Convert<ObjectClass>(), false);
                }
                float percent = 0f;
                float speed = 0.1f;

                bool selected = Owner.OwnerObject.Ref.Base.IsSelected;
                Owner.OwnerObject.Ref.Base.Deselect();
                locomotion.Lock();

                while (percent <= 1f)
                {
                    CoordStruct cur = Vector3.Lerp(start.ToVector3(), dest.ToVector3(), percent).ToCoordStruct();
                    cur += new CoordStruct(0, 0, (int)(Math.Sin(percent * Math.PI) * 500));

                    Owner.OwnerObject.Ref.SetDestination(Pointer<AbstractClass>.Zero);
                    locomotion.Stop_Moving();
                    locomotion.Stop_Movement_Animation();
                    locomotion.Mark_All_Occupation_Bits(0);

                    if (MapClass.Instance.TryGetCellAt(Owner.OwnerObject.Ref.Base.Location, out Pointer<CellClass> pCurCell))
                    {
                        if (Owner.OwnerObject.Ref.Base.Location.Z - pCurCell.Ref.GetCenterCoords().Z < Game.BridgeHeight)
                        {
                            Owner.OwnerObject.Ref.Base.UnmarkAllOccupationBits(Owner.OwnerObject.Ref.Base.Location);
                        }
                    }
                    Owner.OwnerObject.Ref.Base.Location = cur;
                    if (MapClass.Instance.TryGetCellAt(cur, out pCurCell))
                    {
                        if (cur.Z - pCurCell.Ref.GetCenterCoords().Z < Game.BridgeHeight)
                        {
                            Owner.OwnerObject.Ref.Base.MarkAllOccupationBits(cur);
                        }
                    }

                    percent += speed;
                    yield return null;
                }

                locomotion.Unlock();
                Owner.OwnerObject.Ref.Base.UnmarkAllOccupationBits(Owner.OwnerObject.Ref.Base.Location);
                pDestCell.Ref.AddContent(Owner.OwnerObject.Convert<ObjectClass>(), false);

                if (selected)
                {
                    Owner.OwnerObject.Ref.Base.Select();
                }

                Create();
            }
        }

        void Create()
        {
            // TODO
        }

        void Brust()
        {
            // TODO
        }

        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            if (weaponIndex == 0)
            {
                if (pTarget.CastToCell(out Pointer<CellClass> pCell))
                {
                    GameObject.StartCoroutine(Jump(pCell.Ref.Base.GetCoords()));
                }
            }

            if (weaponIndex == 1)
            {
                Brust();
            }
        }
    }
}
