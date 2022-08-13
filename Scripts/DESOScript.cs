
using System;
using System.Threading;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;
using Extension.Script;
using Extension.Decorators;
using Extension.Utilities;
using System.Threading.Tasks;

namespace Scripts
{
    [Serializable]
    public class DESO : TechnoScriptable
    {
        public DESO(TechnoExt owner) : base(owner) { }

        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            if (weaponIndex == 1)
            {
                var id = 1919810;
                if (GameObject.GetComponent(id) == null)
                {
                    GameObject.CreateScriptComponent(nameof(NuclearLeakage), id, "Nuclear Leakage Decorator", this);
                }
            }
        }
    }

    [Serializable]
    public class NuclearLeakage : TechnoScriptable
    {
        public NuclearLeakage(DESO deso) : base(deso.Owner)
        {
        }

        int times = 100;

        static Pointer<WeaponTypeClass> Weapon => WeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("RadEruptionWeapon");
        static Pointer<WarheadTypeClass> Warhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("RadEruptionWarhead");
        
        static Pointer<WeaponTypeClass> Weapon2 => WeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("TerrorBomb");
        static Pointer<WarheadTypeClass> Warhead2 => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("TerrorBombWH");

        public override void OnReceiveDamage(Pointer<int> pDamage, int DistanceFromEpicenter, Pointer<WarheadTypeClass> pWH,
            Pointer<ObjectClass> pAttacker, bool IgnoreDefenses, bool PreventPassengerEscape, Pointer<HouseClass> pAttackingHouse)
        {
            if (times-- <= 0)
            {
                DetachFromParent();
                return;
            }

            TechnoExt owner = Owner;

            Pointer<WeaponTypeClass> pWeapon = Weapon;
            Pointer<WarheadTypeClass> pWarhead = Warhead;

            CoordStruct curLocation = owner.OwnerObject.Ref.Base.Base.GetCoords();

            Pointer<BulletClass> pBullet = pWeapon.Ref.Projectile.Ref.
                CreateBullet(owner.OwnerObject.Convert<AbstractClass>(), owner.OwnerObject,
                1, pWarhead, pWeapon.Ref.Speed, pWeapon.Ref.Bright);
            pBullet.Ref.WeaponType = pWeapon;

            pBullet.Ref.MoveTo(curLocation, new BulletVelocity(0, 0, 0));
            pBullet.Ref.Detonate(curLocation);
            pBullet.Ref.Base.UnInit();

            pWeapon = Weapon2;
            pWarhead = Warhead2;

            pBullet = pWeapon.Ref.Projectile.Ref.
                CreateBullet(owner.OwnerObject.Convert<AbstractClass>(), owner.OwnerObject,
                pWeapon.Ref.Damage, pWarhead, pWeapon.Ref.Speed, pWeapon.Ref.Bright);
            pBullet.Ref.WeaponType = pWeapon;

            pBullet.Ref.MoveTo(curLocation, new BulletVelocity(0, 0, 0));
            pBullet.Ref.Detonate(curLocation);
            pBullet.Ref.Base.UnInit();
        }
    }

}