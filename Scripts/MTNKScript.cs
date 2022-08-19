
using System;
using System.Threading;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;
using Extension.Script;
using System.Threading.Tasks;
using Extension.Utilities;
using Extension.INI;

namespace Scripts
{
    public class MTNKConfig : INIAutoConfig
    {
        [INIField(Key = "FireSuperWeapon")]
        public Pointer<SuperWeaponTypeClass> SuperWeapon;

        public bool HasLaserTail = true;
    }

    [Serializable]
    public class MTNK : TechnoScriptable
    {
        public MTNK(TechnoExt owner) : base(owner) {}

        static MTNK()
        {
            // Task.Run(() =>
            // {
            //     while (true)
            //     {
            //         Logger.Log("Ticked.");
            //         Thread.Sleep(1000);
            //     }
            // });
        }
        
        static ColorStruct innerColor = new ColorStruct(208,10,203);
        static ColorStruct outerColor = new ColorStruct(88, 0, 88);
        static ColorStruct outerSpread = new ColorStruct(10, 10, 10);

        CoordStruct lastLocation;

        INIComponentWith<MTNKConfig> INI;

        public override void Awake()
        {
            INI = this.CreateRulesIniComponentWith<MTNKConfig>(Owner.OwnerTypeRef.BaseAbstractType.ID);
        }

        public override void OnUpdate()
        {
            if (INI.Data.HasLaserTail)
            {
                Pointer<TechnoClass> pTechno = Owner.OwnerObject;

                CoordStruct nextLocation = pTechno.Ref.Base.Base.GetCoords();
                nextLocation.Z += 50;
                if (lastLocation.DistanceFrom(nextLocation) > 100)
                {
                    Pointer<LaserDrawClass> pLaser = YRMemory.Allocate<LaserDrawClass>().Construct(lastLocation, nextLocation, innerColor, outerColor, outerSpread, 30);
                    pLaser.Ref.Thickness = 10;
                    pLaser.Ref.IsHouseColor = true;
                    //Logger.Log("laser [({0}, {1}, {2}) -> ({3}, {4}, {5})]", lastLocation.X, lastLocation.Y, lastLocation.Z, nextLocation.X, nextLocation.Y, nextLocation.Z);

                    lastLocation = nextLocation;
                }
            }
        }
        
        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex) 
        {
            Pointer<SuperWeaponTypeClass> pSWType = INI.Data.SuperWeapon;

            if (pSWType.IsNull == false) {
                Pointer<TechnoClass> pTechno = Owner.OwnerObject;
                Pointer<HouseClass> pOwner = pTechno.Ref.Owner;
                Pointer<SuperClass> pSuper = pOwner.Ref.FindSuperWeapon(pSWType);

                CellStruct targetCell = CellClass.Coord2Cell(pTarget.Ref.GetCoords());
                //Logger.Log("FireSuperWeapon({2}):0x({3:X}) -> ({0}, {1})", targetCell.X, targetCell.Y, pSWType.Ref.Base.ID, (int)pSuper);
                pSuper.Ref.IsCharged = true;
                pSuper.Ref.Launch(targetCell, true);
                pSuper.Ref.IsCharged = false;
            }
        }
    }
}