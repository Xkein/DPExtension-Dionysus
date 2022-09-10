
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
using Extension.INI;

namespace Scripts
{
    public class DiskConfig : INIAutoConfig
    {
        [INIField(Key = "Disk.LaserInnerColor")]
        public ColorStruct InnerColor = new ColorStruct(11, 45, 14);

        [INIField(Key = "Disk.LaserOuterColor")]
        public ColorStruct OuterColor = new ColorStruct(19, 19, 810);

        [INIField(Key = "Disk.LaserOuterSpread")]
        public ColorStruct OuterSpread = new ColorStruct(10, 10, 10);

        [INIField(Key = "Disk.LaserThickness")]
        public int Thickness = 10;

        [INIField(Key = "Disk.LaserDuration")]
        public int Duration = 8;

        [INIField(Key = "Disk.LaserDamage")]
        public int Damage = 11;

        [INIField(Key = "Disk.PrepareTime")]
        public int PrepareTime = 300;

        [INIField(Key = "Disk.Radius")]
        public int Radius = 1024;

        [INIField(Key = "Disk.AimingSpeed")]
        public int AimingSpeed = 11;

        [INIField(Key = "Disk.LaserWarhead")]
        public Pointer<WarheadTypeClass> Warhead = WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("BlimpHEEffect");

        [INIField(Key = "Disk.DrainOnly")]
        public bool DrainOnly = false;

        [INIField(Key = "Disk.FireAtBottom")]
        public bool FireAtBottom = false;
    }

    [Serializable]
    public class Disk : TechnoScriptable
    {
        public Disk(TechnoExt owner) : base(owner) {}
        
        ColorStruct innerColor => config.Data.InnerColor;
        ColorStruct outerColor => config.Data.OuterColor;
        ColorStruct outerSpread => config.Data.OuterSpread;

        static Pointer<BulletTypeClass> pBulletType => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");
        int damage => config.Data.Damage;
        int thickness => config.Data.Thickness;
        int duration => config.Data.Duration;
        int prepareTime => config.Data.PrepareTime;
        int startRadius => config.Data.Radius;
        int aimingSpeed => config.Data.AimingSpeed;
        Pointer<WarheadTypeClass> pWH => config.Data.Warhead;
        bool drainOnly => config.Data.DrainOnly;
        bool fireAtBottom => config.Data.FireAtBottom;

        IConfigWrapper<DiskConfig> config;

        int angle;
        int frames;
        double radius;
        TechnoExt Target;

        public override void Awake()
        {
            config = Ini.GetConfig<DiskConfig>(Ini.GetDependency(INIConstant.RulesName), Owner.OwnerTypeRef.BaseAbstractType.ID);
        }

        private void KillStart(TechnoExt ext)
        {
            angle = 0;
            frames = 0;
            radius = startRadius;

            Target = ext;
        }

        private void KillUpdate()
        {
            if(Target is { Expired: false })
            {
                Pointer<TechnoClass> pTechno = Target.OwnerObject;

                int height = pTechno.Ref.Base.GetHeight();

                Action<int, int> Attack = (int start, int count) => {
                    int increasement = 360 / count;
                    CoordStruct curLocation;
                    CoordStruct from;

                    if (fireAtBottom)
                    {
                        curLocation = Owner.OwnerObject.Ref.Base.Base.GetCoords();
                        from = curLocation;
                    }
                    else
                    {
                        curLocation = pTechno.Ref.Base.Base.GetCoords();
                        from = curLocation + new CoordStruct(0, 0, 5000);
                    }

                    CoordStruct groundLocation = curLocation;
                    if (MapClass.Instance.TryGetCellAt(CellClass.Coord2Cell(curLocation), out var pCell))
                    {
                        groundLocation.Z = pCell.Ref.Base.GetCoords().Z;
                    }

                    for (int i = 0; i < count; i++) {
                        double x = radius * Math.Cos((start + i * increasement) * Math.PI / 180);
                        double y = radius * Math.Sin((start + i * increasement) * Math.PI / 180);
                        CoordStruct to = groundLocation + new CoordStruct((int)x, (int)y, -height);
                        Pointer<LaserDrawClass> pLaser = YRMemory.Allocate<LaserDrawClass>().Construct(from, to, innerColor, outerColor, outerSpread, duration);
                        pLaser.Ref.Thickness = thickness;
                        pLaser.Ref.IsHouseColor = true;
                        
                        if(frames > prepareTime) {
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
                radius -= aimingSpeed;
                if (radius < 0) {
                    KillStart(Target);
                } 
            }
        }

        public override void OnUpdate()
        {
            if (drainOnly)
            {
                if (Owner.OwnerRef.DrainAnim.IsNotNull)
                {
                    if (Target == null)
                    {
                        KillStart(TechnoExt.ExtMap.Find(Owner.OwnerRef.DrainTarget));
                    }
                }
                else
                {
                    Target = null;
                }
            }

            KillUpdate();
        }

        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            if (drainOnly)
                return;

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