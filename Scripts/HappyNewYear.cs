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
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Scripts
{
    [Serializable]
    public class HappyNewYear : BulletScriptable
    {
        // 起飞需要的帧数
        const int _RAISE_FRAME = 1 * 60;
        // 开始爆炸需要的间隔
        const int _START_DELAY = _RAISE_FRAME - 1;
        // 系统持续时间
        const int _SYSTEM_DURATION = 10 * 60;
        // 爆炸间隔
        const int _BURST_DELAY = 50;
        // 爆炸粒子数量
        const int BURST_COUNT = 1000;
        // 上升高度
        const int RAISE_HEIGHT = 2000;

        public const float START_DELAY = _START_DELAY / 60f;
        public const float SYSTEM_DURATION = _SYSTEM_DURATION / 60f;
        public const float BURST_DELAY = _BURST_DELAY / 60f;
        public const float RAISE_DURATION = _RAISE_FRAME / 60f;

        public HappyNewYear(BulletExt owner) : base(owner) { }

        static HappyNewYear()
        {
            FXEngine.EnableParallel = true;

            FXSystem system = new FXSystem();

            system.MSystemUpdate.Add(
                new FXSystemState(system, null) { LoopDelay = 0, LoopDuration = SYSTEM_DURATION }
                );

            // 起飞过程的尾焰
            FXEmitter emitter = new FXEmitter(system);

            emitter.MEmitterUpdate.Add(
                new FXEmitterState(system, emitter) { LoopDuration = RAISE_DURATION },
                new FXSpawnRate(system, emitter) { SpawnRate = 100 }
                );
            emitter.MParticleSpawn.Add(
                new FXInitializeParticle(system, emitter) { LifetimeMin = 0.5f, LifetimeMax = 0.6f },
                new FXSphereLocation(system, emitter) { SphereRadius = 64 },
                new FXAddVelocityFromPoint(system, emitter) { Offset = new Vector3(0, 0, 0), VelocityStrength = new FXRandomRangeFloat(75, 100) }
                );
            emitter.MParticleUpdate.Add(
                new FXParticleState(system, emitter),
                new FXGravityForce(system, emitter),
                new FXCollision(system, emitter),
                new FXSolveForcesAndVelocity(system, emitter)
                );
            emitter.MRender.Add(
                new FXLaserRender(system, emitter) { Color = new Vector3(0.5f, 0, 0.5f), Duration = 3, Thickness = 1 }
                );

            system.Emitters.Add(emitter);

            // 球形爆炸
            emitter = new FXEmitter(system);

            emitter.MEmitterUpdate.Add(
                new FXEmitterState(system, emitter) { LoopCount = 1, LoopDuration = BURST_DELAY, LoopDelay = START_DELAY, DelayFirstLoopOnly = true },
                new FXSpawnBurstInstantaneous(system, emitter) { SpawnCount = BURST_COUNT, SpawnProbability = 0.8f }
                );
            emitter.MParticleSpawn.Add(
                new FXInitializeParticle(system, emitter) { LifetimeMin = 0.5f, LifetimeMax = 1f },
                new FXSphereLocation(system, emitter) { SphereRadius = 256 },
                new FXAddVelocityFromPoint(system, emitter) { Offset = new Vector3(0, 0, 0), VelocityStrength = new FXRandomRangeFloat(4000, 4150) }
                );
            emitter.MParticleUpdate.Add(
                new FXParticleState(system, emitter),
                new FXDrag(system, emitter) { Drag = 3 },
                new FXCollision(system, emitter),
                new FXSolveForcesAndVelocity(system, emitter)
                );
            emitter.MRender.Add(
                new FXLaserRender(system, emitter) { Color = new Vector3(0.5f, 0, 0.5f), Duration = 2, Thickness = 2 }
                );

            system.Emitters.Add(emitter);

            Prototype = system;
        }

        public static FXSystem Prototype;

        const int FX_SYSTEM_COUNT = 3;
        FXSystem[] fxSystems = new FXSystem[FX_SYSTEM_COUNT];
        Vector3[] colors = new[]
        {
            new Vector3(1f, 0f, 0f),
            new Vector3(0f, 1f, 0f),
            new Vector3(0f, 0f, 1f),
        };

        double factor = 1.0;
        CoordStruct currentLocation;
        CoordStruct fixedLocation;
        public override void OnUpdate()
        {
            Pointer<BulletClass> pBullet = Owner.OwnerObject;
            CoordStruct location = factor > 0.0 ? pBullet.Ref.Base.Base.GetCoords() : fixedLocation;

            // 竖直上升，减速至0
            var velocity = new BulletVelocity(0, 0, factor * 2 * RAISE_HEIGHT / _RAISE_FRAME);
            pBullet.Ref.Velocity = velocity;

            if (factor == 0.0)
            {
                fixedLocation = location;
                pBullet.Ref.Base.Location = fixedLocation;
            }
            else if (factor == 1.0)
            {
                currentLocation = location;
            }
            else
            {
                pBullet.Ref.Base.Location = currentLocation += new CoordStruct(0, 0, (int)velocity.Z);
            }

            factor = Math.Max(factor - 1.0 / _RAISE_FRAME, 0);
            //Console.WriteLine("{1:0.00} speed: {0}, location.z: {2}", (int)velocity.Z, factor, currentLocation.Z);

            for (int i = 0; i < FX_SYSTEM_COUNT; i++)
            {
                if (fxSystems[i] == null)
                {
                    var fxSystem = Prototype.Clone();
                    var render = fxSystem.Emitters[0].MRender.Scripts[0] as FXLaserRender;
                    render.Color = colors[i % colors.Length];
                    render = fxSystem.Emitters[1].MRender.Scripts[0] as FXLaserRender;
                    render.Color = colors[i % colors.Length];

                    FXEngine.AddSystem(fxSystem);
                    fxSystems[i] = fxSystem;

                    fxSystem.Spawn(new Vector3(location.X, location.Y, location.Z));
                    Console.WriteLine($"{fxSystem} spawn!");
                }
                if (fxSystems[i] != null)
                {
                    fxSystems[i].Position = new Vector3(location.X, location.Y, location.Z);
                }
            }

            if (fxSystems.All(fx => fx.ExecutionState == FXExecutionState.Complete))
            {
                pBullet.Ref.Range = 0;
            }
        }

        //public override void OnPut(CoordStruct coord, Direction faceDir)
        //{
        //    Pointer<BulletClass> pBullet = Owner.OwnerObject;
        //    pBullet.Ref.Velocity = new BulletVelocity(0, 0, 1000);
        //}

        //public override void OnRemove()
        //{
        //    for (int i = 0; i < FX_SYSTEM_COUNT; i++)
        //    {
        //        if (fxSystems[i] != null)
        //        {
        //            var fxSystem = fxSystems[i];
        //            FXEngine.RemoveSystem(fxSystem);
        //            Console.WriteLine($"{fxSystem} remove!");
        //        }
        //    }
        //}
    }
}
