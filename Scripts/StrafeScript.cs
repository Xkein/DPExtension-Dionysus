
using System;
using System.Collections;
using System.Threading;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;
using Extension.Script;
using System.Threading.Tasks;
using Extension.Coroutines;
using PatcherYRpp.Utilities;
using ScriptUniversal.Components;
using ScriptUniversal.Strategy;

namespace Scripts
{
    [Serializable]
    public class StrafeScript : TechnoScriptable
    {
        public StrafeScript(TechnoExt owner) : base(owner) {}

        [Serializable]
        public class StrafeStrategy : WeaponFireStrategy
        {
            public StrafeStrategy(Pointer<TechnoClass> techno) : base(techno, techno.Ref.GetWeapon(0).Ref.WeaponType)
            {

            }
            public override int FireTime => 10;
            public override int CoolDownTime => 200;

            public override int GetDelay(int fireTime) => fireTime > 1 ? fireTime * 2 : 10;

            private bool spin = false;
            public override void Awake()
            {
                OnCreateBullet += (BulletExt ext) =>
                {
                    ext.OwnerObject.Ref.MoveTo(Target.Ref.GetCoords() + new CoordStruct(-500, -500, 4000 + MathEx.Random.Next(0, 1000)) + MathEx.CalculateRandomPointInSphere(256, 1024).ToCoordStruct(), default);
                    if (spin)
                    {
                        ext.ExtComponent.CreateScriptComponent("AAHeatSeeker2", "ss", ext);
                    }
                };

                GameObject.StartCoroutine(DelaySecondFiring());
            }

            IEnumerator DelaySecondFiring()
            {
                while (true)
                {
                    yield return new WaitUntil(() => _secondFiring is not null);
                    GameObject.StopCoroutine(_secondFiring);
                    yield return new WaitForFrames(50);
                    _secondFiring.CanRestart = true;
                    yield return new WaitWhile(() => _secondFiring is not null);
                }
            }
            
            protected override IEnumerator Firing()
            {
                spin = true;
                yield return base.Firing();
                spin = false;
                Weapon = WeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("Medusa");
                _secondFiring = GameObject.StartCoroutine(base.Firing());
                yield return _secondFiring;
                _secondFiring = null;
                Weapon = Techno.OwnerObject.Ref.GetWeapon(0).Ref.WeaponType;
            }

            private Coroutine _secondFiring;
        }

        public override void Awake()
        {
            GameObject.AddComponent(_fireComponent = new FireComponent());
            new StrafeStrategy(Owner.OwnerObject).AttachToComponent(_fireComponent);
        }

        public override void OnUpdate()
        {
            Pointer<TechnoClass> pTechno = Owner.OwnerObject;
            TechnoTypeExt extType = Owner.Type;


        }
        
        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex) 
        {
            _fireComponent.StartFire(pTarget);
        }

        private FireComponent _fireComponent;
    }
}