using DynamicPatcher;
using Extension.INI;
using Extension.Utilities;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;


namespace Extension.Ext
{

    public partial class WarheadTypeExt
    {
        //Ares
        public bool AllowZeroDamage = false;
        public bool AffectsEnemies = true;

        [INILoadAction]
        public void LoadINI(Pointer<CCINIClass> pINI)
        {
            INIReader reader = new INIReader(pINI);
            string section = OwnerObject.Ref.Base.ID;

            //Ares
            reader.Read(section, "AffectsEnemies", ref AffectsEnemies);
            reader.Read(section, "AllowZeroDamage", ref AllowZeroDamage);

        }

        [LoadAction]
        public void Load(IStream stream) { }

        [SaveAction]
        public void Save(IStream stream) { }
    }
}
