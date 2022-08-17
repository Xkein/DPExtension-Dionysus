using Extension.Utilities;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using Extension.INI;
using System.Reflection;
using System.Linq.Expressions;

namespace Extension.Ext
{
    public partial class TechnoExt
    {

        public void OnPut(CoordStruct coord,Direction faceDir)
        {
            PartialHelper.TechnoPutAction(this,coord,faceDir);
        }

        public void OnUpdate()
        {
            PartialHelper.TechnoUpdateAction(this);
        }

        public void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            PartialHelper.TechnoFireAction(this,pTarget, weaponIndex);
        }

        public void OnRemove()
        {
            PartialHelper.TechnoRemoveAction(this);
        }

        public void OnReceiveDamage(Pointer<int> pDamage, int DistanceFromEpicenter, Pointer<WarheadTypeClass> pWH,
            Pointer<ObjectClass> pAttacker, bool IgnoreDefenses, bool PreventPassengerEscape, Pointer<HouseClass> pAttackingHouse)
        {
            PartialHelper.TechnoReceiveDamageAction(this,pDamage,DistanceFromEpicenter,pWH,pAttacker,IgnoreDefenses,PreventPassengerEscape,pAttackingHouse);
        }

        public string MyExtensionTest = nameof(MyExtensionTest);
    }

    public partial class TechnoTypeExt
    {
        public SwizzleablePointer<SuperWeaponTypeClass> FireSuperWeapon = new SwizzleablePointer<SuperWeaponTypeClass>(IntPtr.Zero);


        [INILoadAction]
        public void LoadINI(Pointer<CCINIClass> pINI)
        {
            INIReader reader = new INIReader(pINI);
            string section = OwnerObject.Ref.Base.Base.ID;

            reader.Read(section, nameof(FireSuperWeapon), ref FireSuperWeapon.Pointer);
        }

        [LoadAction]
        public void Load(IStream stream)
        {
        }
        [SaveAction]
        public void Save(IStream stream)
        {
        }
    }
}
