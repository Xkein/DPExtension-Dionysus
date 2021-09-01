using Extension.Ext;
using Extension.FX;
using Extension.FX.Definitions;
using Extension.FX.Parameters;
using Extension.FX.Renders;
using Extension.FX.Scripts.Emitter;
using Extension.FX.Scripts.Particle;
using Extension.FX.Scripts.System;
using Extension.Script;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scripts
{
    [Serializable]
    public class CAPARS01Script : TechnoScriptable
    {
        public CAPARS01Script(TechnoExt owner) : base(owner) { }

        static CAPARS01Script()
        {
            FXSystem system = new FXSystem();

            system.MSystemUpdate.Add(
                new FXSystemState(system, null) { LoopDelay = 1f, LoopDuration = 100f }
                );

            FXEmitter emitter = new FXEmitter(system);

            emitter.MEmitterUpdate.Add(
                new FXEmitterState(system, emitter) { LoopCount = 10, LoopDelay = 1f, LoopDuration = 10f },
                new FXSpawnBurstInstantaneous(system, emitter) { SpawnCount = 1000, SpawnProbability = 0.8f },
                new FXSpawnRate(system, emitter) { SpawnRate = 1000 }
                );
            emitter.MParticleSpawn.Add(
                new FXInitializeParticle(system, emitter) { LifetimeMin = 1f, LifetimeMax = 2f },
                new FXSphereLocation(system, emitter) { SphereRadius = 256 },
                new FXAddVelocityFromPoint(system, emitter) { Offset = new Vector3(0, 0 ,100), VelocityStrength = new FXRandomRangeFloat(75, 100) },
                new FXBoxLocation(system, emitter) { BoxSize = new Vector3(10000, 10000, 10) }
                );
            emitter.MParticleUpdate.Add(
                new FXParticleState(system, emitter),
                new FXGravityForce(system, emitter),
                new FXCollision(system, emitter),
                new FXSolveForcesAndVelocity(system, emitter)
                );
            emitter.MRender.Add(
                new FXSHPRenderer(system, emitter) { SHP = "TWLT070.shp", PAL = "Anim.pal" }
                );

            system.Emitters.Add(emitter);

            Prototype = system;
        }

        public static FXSystem Prototype;

        FXSystem fxSystem;
        public override void OnUpdate()
        {
            if(fxSystem == null)
            {
                Pointer<TechnoClass> pTechno = Owner.OwnerObject;
                TechnoTypeExt extType = Owner.Type;

                fxSystem = Prototype.Clone();
                FXEngine.AddSystem(fxSystem);

                var location = pTechno.Ref.Base.Location;
                fxSystem.Spawn(new Vector3(location.X, location.Y, location.Z + 2000));
                Console.WriteLine($"{fxSystem} spawn!");
            }

        }

        public override void OnRemove()
        {
            if(fxSystem != null)
            {
                FXEngine.RemoveSystem(fxSystem);
                Console.WriteLine($"{fxSystem} remove!");
            }
        }
    }
}
