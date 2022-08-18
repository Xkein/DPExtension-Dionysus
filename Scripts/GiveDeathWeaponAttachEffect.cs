using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DpLib.Scripts.AE
{
    [Serializable]
    public class GiveDeathWeaponAttachEffect : AttachEffectScriptable
    {
        public GiveDeathWeaponAttachEffect(TechnoExt owner) : base(owner)
        {
        }

        private static Pointer<BulletTypeClass> inviso=> BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");
        private static Pointer<WarheadTypeClass> wh => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("NUKE");

        public override void OnUpdate()
        {
            base.OnUpdate();
        }

        public override void OnRemove()
        {
            base.OnRemove();
            if (Owner.OwnerObject.Ref.Base.Health <= 0)
            {
                var location = Owner.OwnerObject.Ref.Base.Base.GetCoords();
                var bullet = inviso.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 500, wh, 100, true);
                bullet.Ref.Detonate(location);
                bullet.Ref.Base.UnInit();
            }
        }

     
    }
}
