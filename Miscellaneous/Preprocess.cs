using DynamicPatcher;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Extension.Components;
using Extension.INI;
using Extension.Utilities;

namespace Miscellaneous
{
    [RunClassConstructorFirst]
    public class Preprocess
    {
        static Preprocess()
        {
            // add 500MB pressure
            GC.AddMemoryPressure(500 * 1024 * 1024);
            Logger.Log("Add 500MB pressure to GC.");

            RunSomeClassConstructor();
        }

        static void RunClassConstructor(Type type)
        {
            RuntimeHelpers.RunClassConstructor(type.TypeHandle);
            Logger.Log("Run class constructor for {0}.", type.FullName);
        }

        static void RunSomeClassConstructor()
        {
            RunClassConstructor(typeof(INIConstant));
            RunClassConstructor(typeof(INIComponent));
        }

    }
}
