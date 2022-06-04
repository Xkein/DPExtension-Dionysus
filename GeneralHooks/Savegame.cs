using DynamicPatcher;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Extension.EventSystems;
using System.Runtime.InteropServices.ComTypes;
using PatcherYRpp;

namespace GeneralHooks
{
    public class Savegame
    {
        [Hook(HookType.AresHook, Address = 0x67D300, Size = 5)]
        public static unsafe UInt32 SaveGame_Start(REGISTERS* R)
        {
            var pStm = (Pointer<IStream>)R->ECX;
            IStream stream = Marshal.GetObjectForIUnknown(pStm) as IStream;

            EventSystem.SaveGame.Broadcast(
                EventSystem.SaveGame.SaveGameEvent,
                new SaveGameEventArgs(stream, true));

            return 0;
        }

        [Hook(HookType.AresHook, Address = 0x67E42E, Size = 0xD)]
        public static unsafe UInt32 SaveGame_End(REGISTERS* R)
        {
            var pStm = (Pointer<IStream>)R->ESI;
            IStream stream = Marshal.GetObjectForIUnknown(pStm) as IStream;

            EventSystem.SaveGame.Broadcast(
                EventSystem.SaveGame.SaveGameEvent,
                new SaveGameEventArgs(stream, false));

            return 0;
        }

        [Hook(HookType.AresHook, Address = 0x67E730, Size = 5)]
        public static unsafe UInt32 LoadGame_Start(REGISTERS* R)
        {
            var pStm = (Pointer<IStream>)R->ECX;
            IStream stream = Marshal.GetObjectForIUnknown(pStm) as IStream;

            EventSystem.SaveGame.Broadcast(
                EventSystem.SaveGame.LoadGameEvent,
                new LoadGameEventArgs(stream, true));

            return 0;
        }

        [Hook(HookType.AresHook, Address = 0x67F7C8, Size = 5)]
        public static unsafe UInt32 LoadGame_End(REGISTERS* R)
        {
            var pStm = (Pointer<IStream>)R->ESI;
            IStream stream = Marshal.GetObjectForIUnknown(pStm) as IStream;

            EventSystem.SaveGame.Broadcast(
                EventSystem.SaveGame.LoadGameEvent,
                new LoadGameEventArgs(stream, false));

            return 0;
        }
    }
}
