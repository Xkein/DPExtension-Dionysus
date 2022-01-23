using Microsoft.VisualStudio.TestTools.UnitTesting;
using PatcherYRpp.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PatcherYRpp.Utilities.Tests
{
    [TestClass()]
    public class ObjectBlockContainerTests
    {
        public static IntPtr GetCoords(IntPtr pThis, IntPtr pCrd)
        {
            Pointer<ObjectClass> pObject = pThis;
            Pointer<CoordStruct> pCoord = pCrd;
            pCoord.Ref = pObject.Ref.Location;
            return pCrd;
        }
        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        public delegate IntPtr GetCoordsType(IntPtr pThis, IntPtr pCrd);
        [TestMethod()]
        public void GetCoveredBlocksTest()
        {
            const int TEST_OBJECT_MAX_NUMBER = 10000;
            const int TEST_FIND_MAX_COUNT = 1000;

            var random = new Random();
            var stopwatch = new Stopwatch();

            Game.pCurrentFrame = Marshal.AllocHGlobal(sizeof(int));
            DynamicVectorClass<Pointer<ObjectClass>> dvc = default;
            dvc.Items = Marshal.AllocHGlobal(Unsafe.SizeOf<Pointer<ObjectClass>>() * TEST_OBJECT_MAX_NUMBER);
            dvc.Count = TEST_OBJECT_MAX_NUMBER;
            Pointer<ObjectClass> testObjects = Marshal.AllocHGlobal(Unsafe.SizeOf<ObjectClass>() * TEST_OBJECT_MAX_NUMBER);
            Pointer<IntPtr> vfptr = Marshal.AllocHGlobal(Unsafe.SizeOf<IntPtr>() * 200);

            CoordStruct GetRandomCoords() => new CoordStruct(random.Next(200000), random.Next(200000), random.Next(200000));

            for (int i = 0; i < TEST_OBJECT_MAX_NUMBER; i++)
            {
                var pObject = testObjects + i;
                dvc[i] = pObject;

                pObject.Ref.Base.Vfptr = vfptr;
                pObject.Ref.Location = GetRandomCoords();
            }

            // GetCoords
            GetCoordsType dlg = GetCoords;
            dvc[0].Ref.SetVirtualFunctionPointer(18, Marshal.GetFunctionPointerForDelegate(dlg));
            var ret = dvc[0].Ref.Base.GetCoords();

            ObjectBlockContainer container = new ObjectBlockContainer(dvc.GetThisPointer(), 11, 5000, 5000);

            List<Pointer<ObjectClass>> BruteFindObjectsNear(CoordStruct location, int range)
            {
                ref var objects = ref container.ObjectArray;
                var list = new List<Pointer<ObjectClass>>();

                foreach (Pointer<ObjectClass> pObject in objects)
                {
                    if (pObject.Ref.Base.GetCoords().DistanceFrom(location) <= range)
                    {
                        list.Add(pObject);
                    }
                }

                return list;
            }
            List<Pointer<ObjectClass>> BlockFindObjectsNear(CoordStruct location, int range)
            {
                var blocks = container.GetCoveredBlocks(location, range);
                var list = new List<Pointer<ObjectClass>>();

                foreach (var block in blocks)
                {
                    foreach (var pObject in block.Objects)
                    {
                        if (pObject.Ref.Base.GetCoords().DistanceFrom(location) <= range)
                        {
                            list.Add(pObject);
                        }
                    }
                }

                return list;
            }
            var testList = Enumerable.Range(0, TEST_FIND_MAX_COUNT).Select(_ => GetRandomCoords()).ToList();

            object gameClosed = false;
            void UpdateGameFrame()
            {
                Console.WriteLine("game simulate begin.");
                //Thread.Sleep(100);
                while (!(bool)gameClosed)
                {
                    SpinWait.SpinUntil(() => false, 1);
                    Game.CurrentFrame++;
                }
                Console.WriteLine("game simulate end.");
            }

            var gameLoop = Task.Run(UpdateGameFrame);

            
            int TEST_OBJECT_NUMBER = TEST_OBJECT_MAX_NUMBER;
            int TEST_FIND_COUNT = TEST_FIND_MAX_COUNT;
            const int TEST_FIND_RANGE = 256 * 15;
            int TEST_OBJECT_NUMBER_DEC = 100;
            int TEST_FIND_COUNT_DEC = 10;

            Console.WriteLine("---------------------------------------");
            for (int j = 0; j < 100; j++)
            {
                Console.WriteLine($"find {TEST_FIND_COUNT} times in {TEST_OBJECT_NUMBER} objects within {TEST_FIND_RANGE}");

                stopwatch.Restart();
                for (int i = 0; i < TEST_FIND_COUNT; i++)
                {
                    BlockFindObjectsNear(testList[i], TEST_FIND_RANGE);
                }
                stopwatch.Stop();
                Console.WriteLine("block find method use {0}ms", stopwatch.ElapsedMilliseconds);

                stopwatch.Restart();
                for (int i = 0; i < TEST_FIND_COUNT; i++)
                {
                    BruteFindObjectsNear(testList[i], TEST_FIND_RANGE);
                }
                stopwatch.Stop();
                Console.WriteLine("brute find method use {0}ms", stopwatch.ElapsedMilliseconds);

                Console.WriteLine("---------------------------------------");
                TEST_OBJECT_NUMBER -= TEST_OBJECT_NUMBER_DEC;
                TEST_FIND_COUNT -= TEST_FIND_COUNT_DEC;
                dvc.Count = TEST_OBJECT_NUMBER;
            }

            gameClosed = true;
            gameLoop.Wait();

            Marshal.FreeHGlobal(dvc.Items);
            Marshal.FreeHGlobal(testObjects);
            Marshal.FreeHGlobal(Game.pCurrentFrame);
        }
    }
}