using DynamicPatcher;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Extension.EventSystems;

namespace GeneralHooks
{
    public class PointerExpire
    {
        [Hook(HookType.AresHook, Address = 0x7258D0, Size = 6)]
        public static unsafe UInt32 AnnounceExpiredPointer(REGISTERS* R)
        {
            var pAbstract = (Pointer<AbstractClass>)R->ECX;
            bool removed = (Bool)R->DL;

            EventSystem.PointerExpire.Broadcast(
                EventSystem.PointerExpire.AnnounceExpiredPointerEvent,
                new AnnounceExpiredPointerEventArgs(pAbstract, removed));

            return 0;
        }


    }
}
