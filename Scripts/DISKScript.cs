
using System;
using System.Threading;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;
using Extension.Script;
using Extension.Utilities;
using System.Threading.Tasks;

namespace Scripts
{
    [Serializable]
    public class Disk : TechnoScriptable
    {
        public Disk(TechnoExt owner) : base(owner) {}
        
        static ColorStruct innerColor = new ColorStruct(11,45,14);
        static ColorStruct outerColor = new ColorStruct(19, 19, 810);
        static ColorStruct outerSpread = new ColorStruct(10, 10, 10);

        int angle;
        int frames;
        double radius;
        static Pointer<BulletTypeClass> pBulletType => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");
        static Pointer<WarheadTypeClass> pWH => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("BlimpHEEffect");

        TechnoExt Target;

        private void KillStart(TechnoExt ext)
        {
            angle = 0;
            frames = 0;
            radius = 1024;

            Target = ext;
        }

        private void KillUpdate()
        {
            if(Target is { Expired: false })
            {
                Pointer<TechnoClass> pTechno = Target.OwnerObject;
                TechnoTypeExt extType = Target.Type;

                CoordStruct curLocation = pTechno.Ref.Base.Base.GetCoords();
                
                int height = pTechno.Ref.Base.GetHeight();

                Action<int, int> Attack = (int start, int count) => {
                    int increasement = 360 / count;
                    CoordStruct from = curLocation;
                        from.Z+=5000;
                    for (int i = 0; i < count; i++) {
                        double x = radius * Math.Cos((start + i * increasement) * Math.PI / 180);
                        double y = radius * Math.Sin((start + i * increasement) * Math.PI / 180);
                        CoordStruct to = curLocation + new CoordStruct((int)x, (int)y, -height);
                        Pointer<LaserDrawClass> pLaser = YRMemory.Allocate<LaserDrawClass>().Construct(from, to, innerColor, outerColor, outerSpread, 8);
                        pLaser.Ref.Thickness = 10;
                        pLaser.Ref.IsHouseColor = true;
                        
                        if(frames > 300) {
                            int damage = 11;
                            // MapClass.DamageArea(to, damage, Owner.OwnerObject, pWH, false, Owner.OwnerObject.Ref.Owner);
                            // MapClass.FlashbangWarheadAt(damage, pWH, to);
                            Pointer<BulletClass> pBullet = pBulletType.Ref.CreateBullet(pTechno.Convert<AbstractClass>(), Owner.OwnerObject, damage, pWH, 100, true);
                            pBullet.Ref.Detonate(to);
                            pBullet.Ref.Base.UnInit();
                        }
                        else {
                            frames++;
                        }
                    }
                };

                Attack(angle, 5);
                angle = (angle + 4) % 360;
                radius -= 11;
                if (radius < 0) {
                    KillStart(Target);
                } 
            }
        }

        public override void OnUpdate()
        {
            KillUpdate();
        }

        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            if (Target == null || Target.Expired)
            {
                if (pTarget.CastToTechno(out Pointer<TechnoClass> pTechno))
                {
                    KillStart(TechnoExt.ExtMap.Find(pTechno));
                }
            }
        }
    }
}