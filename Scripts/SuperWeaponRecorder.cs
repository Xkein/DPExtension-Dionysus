using DynamicPatcher;
using Extension.Coroutines;
using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scripts
{
    // !!!!!!!!!!!!!!!!!!!!
    // you can use GlobalScriptable everywhere unless your assembly outside DynamicPatcher folder


    [GlobalScriptable(typeof(SuperWeaponExt))]
    public class SuperWeaponRecorder : SuperWeaponScriptable
    {
        public SuperWeaponRecorder(SuperWeaponExt owner) : base(owner)
        {
        }

        public override void Awake()
        {
            Logger.Log("{0} start tick.", Owner.OwnerTypeRef.Base.ID);
        }
        public override void OnDestroy()
        {
            Logger.Log("{0} end tick", Owner.OwnerTypeRef.Base.ID);
        }
        public override void OnLaunch(CellStruct cell, bool isPlayer)
        {
            Logger.Log("{0} fire at ({1}, {2})", Owner.OwnerTypeRef.Base.ID, cell.X, cell.Y);
        }

        [GlobalScriptable(typeof(SuperWeaponExt))]
        [UpdateAfter(typeof(SuperWeaponConcentrator))]
        [UpdateBefore(typeof(SuperWeaponRecorder))]
        public class SuperWeaponIndicator : SuperWeaponScriptable
        {
            public SuperWeaponIndicator(SuperWeaponExt owner) : base(owner)
            {
            }

            public override void Awake()
            {
                //Logger.Log("SuperWeaponIndicator", Owner.OwnerTypeRef.Base.ID);
            }
            public override void OnLaunch(CellStruct cell, bool isPlayer)
            {
                if (MapClass.Instance.TryGetCellAt(cell, out var pCell))
                {
                    DebugUtilities.HighlightCell(pCell, new ColorStruct(255, 0, 0));
                }
            }
        }


        [GlobalScriptable(typeof(SuperWeaponExt))]
        [UpdateBefore(typeof(SuperWeaponRecorder))]
        public class SuperWeaponConcentrator : SuperWeaponScriptable
        {
            public SuperWeaponConcentrator(SuperWeaponExt owner) : base(owner)
            {
            }

            public override void Awake()
            {
                //Logger.Log("SuperWeaponConcentrator", Owner.OwnerTypeRef.Base.ID);
            }

            public override void OnLaunch(CellStruct cell, bool isPlayer)
            {
                if (MapClass.Instance.TryGetCellAt(cell, out var pCell))
                {
                    DebugUtilities.HighlightCircle(pCell.Ref.GetCenterCoords(), 1000, new ColorStruct(0, 255, 0));
                }
            }
        }
    }
}
