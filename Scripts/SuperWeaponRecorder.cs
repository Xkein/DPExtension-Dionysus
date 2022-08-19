using DynamicPatcher;
using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using System;
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
    }
}
