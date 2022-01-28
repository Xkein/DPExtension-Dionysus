using DynamicPatcher;
using PatcherYRpp.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Misc.DynamicPatcher.GeneralHooks
{
    public class General
    {
        [Hook(HookType.AresHook, Address = 0x52BA60, Size = 5)]
        public static unsafe UInt32 GameInit(REGISTERS* R)
        {
            // ensure network synchronization
            MathEx.SetRandomSeed(0);

            return 0;
        }
    }
}
