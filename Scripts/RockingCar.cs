using DynamicPatcher;
using Extension.Ext;
using Extension.Script;
using Extension.Utilities;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using System;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace Scripts
{

    [ComVisible(true), Guid("5A740613-5414-4186-8FC4-3B648F13CCD3"), ProgId("Scripts.RockingCarLocomotion")]
    [RunClassConstructorFirst]
    public class RockingCarLocomotion : ExtendedLocomotionClass
    {
        public RockingCarLocomotion() : base(/*LocomotionClass.Drive*/)
        {
        }

        static RockingCarLocomotion()
        {
            //Logger.Log("UnregisterComObject<RockingCarLocomotion>()");
            COMHelpers.UnregisterComObject<RockingCarLocomotion>();
            //Logger.Log("RegisterComObject<RockingCarLocomotion>()");
            COMHelpers.RegisterComObject<RockingCarLocomotion>();

        }

        public Pointer<FootClass> Owner { get => _data.Owner; set => _data.Owner.Pointer = value; }
        public Pointer<FootClass> LinkedTo { get => _data.LinkedTo; set => _data.LinkedTo.Pointer = value; }
        public float ForwardsToRock { get => _data.ForwardsToRock; set => _data.ForwardsToRock = value; }
        public float SidewaysToRock { get => _data.SidewaysToRock; set => _data.SidewaysToRock = value; }
        public float AngleRotatedForwards { get => _data.AngleRotatedForwards; set => _data.AngleRotatedForwards = value; }
        public float AngleRotatedSideways { get => _data.AngleRotatedSideways; set => _data.AngleRotatedSideways = value; }

        public Vector3 Direction => MathEx.GetForwardVector(LinkedTo.Convert<TechnoClass>());

        public override void Link_To_Object(IntPtr pointer)
        {
            (Owner, LinkedTo) = (pointer, pointer);
            Base = LinkedTo.Ref.Locomotor;
            //base.Link_To_Object(pointer);
        }

        public override IntPtr Draw_Matrix(IntPtr pMatrix, IntPtr pKey)
        {
            Pointer<Matrix4x4> pMatrix44 = base.Draw_Matrix(pMatrix, pKey);
            Matrix4x4 rotateMat = Matrix4x4.CreateFromQuaternion(_quaternion);
            Matrix4x4 tmp = pMatrix44.Data;
            pMatrix44.Ref = Matrix4x4.Lerp(tmp, tmp * rotateMat, 0.1f);
            pMatrix44.Ref = Matrix4x4.Transform(tmp, _quaternion);
            return pMatrix44;
        }

        public override IntPtr Draw_Point(IntPtr pPoint)
        {
            Pointer<Point2D> pPoint2D = base.Draw_Point(pPoint);
            return pPoint2D;
        }

        public override IntPtr Shadow_Matrix(IntPtr pMatrix, IntPtr pKey)
        {
            Pointer<Matrix4x4> pMatrix44 = base.Shadow_Matrix(pMatrix, pKey);
            return pMatrix44;
        }

        public override IntPtr Shadow_Point(IntPtr pPoint)
        {
            Pointer<Point2D> pPoint2D = base.Shadow_Point(pPoint);
            return pPoint2D;
        }

        public override bool Process()
        {
            if (!base.Process())
                return false;

            //Pointer<FootClass> linkedTo = LinkedTo;

            //Random random = MathEx.Random;

            //const double rockSpeed = Math.PI / 180;
            //const double deg_45 = Math.PI / 4;

            //if (MathEx.Approximately(ForwardsToRock, AngleRotatedForwards))
            //    ForwardsToRock = (float)(random.NextDouble() * deg_45) * random.Next(-1, 2);
            //if (MathEx.Approximately(SidewaysToRock, AngleRotatedSideways))
            //    SidewaysToRock = (float)(random.NextDouble() * deg_45) * random.Next(-1, 2);

            //float deltaAngleRotatedForwards = -AngleRotatedForwards + MathEx.Lerp(AngleRotatedForwards, ForwardsToRock, rockSpeed);
            //float deltaAngleRotatedSideways = -AngleRotatedSideways + MathEx.Lerp(AngleRotatedSideways, SidewaysToRock, rockSpeed);

            //deltaAngleRotatedForwards = (float)(Math.Abs(deltaAngleRotatedForwards) < rockSpeed ? Math.Sign(deltaAngleRotatedForwards) * rockSpeed : deltaAngleRotatedForwards);
            //deltaAngleRotatedSideways = (float)(Math.Abs(deltaAngleRotatedSideways) < rockSpeed ? Math.Sign(deltaAngleRotatedSideways) * rockSpeed : deltaAngleRotatedSideways);

            //float boundForwards = Math.Abs(ForwardsToRock);
            //float boundSideways = Math.Abs(SidewaysToRock);

            //AngleRotatedForwards = MathEx.Clamp(AngleRotatedForwards + deltaAngleRotatedForwards, -boundForwards, boundForwards);
            //AngleRotatedSideways = MathEx.Clamp(AngleRotatedSideways + deltaAngleRotatedSideways, -boundSideways, boundSideways);

            //linkedTo.Ref.Base.AngleRotatedForwards = AngleRotatedForwards;
            //linkedTo.Ref.Base.AngleRotatedSideways = AngleRotatedSideways;

            //Logger.Log("AngleRotatedForwards: {0:0.##}, AngleRotatedSideways: {1:0.##}", AngleRotatedForwards, AngleRotatedSideways);

            Vector3 randomVec = MathEx.CalculateRandomUnitVector();
            randomVec.Z = Math.Abs(randomVec.Z);
            _quaternion = MathEx.FromToRotation(Vector3.UnitZ, randomVec);

            return true;
        }

        public override void Load(IStream stream)
        {
            stream.ReadObject(out _data);
            base.Load(stream);
        }

        public override void Save(IStream stream, int fClearDirty)
        {
            stream.WriteObject(_data);
            base.Save(stream, fClearDirty);
        }

        public override int SaveSize() => Pointer<Data>.TypeSize() + base.SaveSize();

        public void Begin_Piggyback(ILocomotion locomotion)
        {
            _oldLocomotor = locomotion;
        }

        public void End_Piggyback(out ILocomotion locomotion)
        {
            locomotion = _oldLocomotor;
            _oldLocomotor = null;
        }

        public bool Is_Ok_To_End()
        {
            throw new NotImplementedException();
        }

        public void Piggyback_CLSID(out Guid classid)
        {
            throw new NotImplementedException();
        }

        public bool Is_Piggybacking()
        {
            return _oldLocomotor != null;
        }

        [Serializable]
        private class Data
        {
            public Data()
            {
                Owner = new SwizzleablePointer<FootClass>(IntPtr.Zero);
                LinkedTo = new SwizzleablePointer<FootClass>(IntPtr.Zero);
            }

            public SwizzleablePointer<FootClass> Owner;
            public SwizzleablePointer<FootClass> LinkedTo;
            public CoordStruct Destination;
            public float ForwardsToRock;
            public float SidewaysToRock;
            public float AngleRotatedForwards;
            public float AngleRotatedSideways;
        }
        private Data _data = new Data();
        Quaternion _quaternion;
        [NonSerialized]
        ILocomotion _oldLocomotor;
    }

    [Serializable]
    public class RockingCar : TechnoScriptable
    {
        public RockingCar(TechnoExt owner) : base(owner)
        {
        }

        public override void Start()
        {
            ChangeLocomotion();
        }

        private void ChangeLocomotion()
        {
            if (Owner.OwnerObject.CastToFoot(out Pointer<FootClass> pFoot))
            {
                var rcLocomotion = new RockingCarLocomotion();
                rcLocomotion.Link_To_Object(pFoot);
                pFoot.Ref.Locomotor = rcLocomotion;
                rcLocomotion.GetCOMPtr<ILocomotion>().AddRef();

                //var nfs = pFoot.Ref.Locomotor as NeedForSpeedLocomotion;

                //var pLoco = new COMPtr<ILocomotion>(nfs);
                //var pPersist = new COMPtr<Microsoft.VisualStudio.OLE.Interop.IPersistStream>(nfs);

                //ILocomotion locomotor = pLoco.Object;
                //locomotor.In_Which_Layer();
                //locomotor.Is_Really_Moving_Now();
                //locomotor.Link_To_Object(pFoot);
                //locomotor.Destination();

                //var persist = pPersist.Object;
                //persist.IsDirty();
            }
        }


    }
}
