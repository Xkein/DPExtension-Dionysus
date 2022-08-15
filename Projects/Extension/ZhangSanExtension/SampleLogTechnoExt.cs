using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DynamicPatcher;
using Extension.Utilities;
using PatcherYRpp;

namespace Extension.Ext
{
    public partial class TechnoExt
    {
        [UpdateAction]
        public void LogOnUpdate() 
        {
            Logger.Log("ZhangSanUpdate");
        }

        [FireAction]
        public void LogOnFire(Pointer<AbstractClass> pTarget,int weaponIndex)
        {
            Logger.Log($"ZhangSan Fire With WeaponIndex {weaponIndex}");
        }
    }
}
