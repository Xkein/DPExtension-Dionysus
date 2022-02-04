
using System;
using System.Threading;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;
using Extension.Script;

namespace ExtensionHooks
{
    public class TechnoExtHooks
    {
        [Hook(HookType.AresHook, Address = 0x6F3260, Size = 5)]
        static public unsafe UInt32 TechnoClass_CTOR(REGISTERS* R)
        {
            return TechnoExt.TechnoClass_CTOR(R);
        }

        [Hook(HookType.AresHook, Address = 0x6F4500, Size = 5)]
        static public unsafe UInt32 TechnoClass_DTOR(REGISTERS* R)
        {
            return TechnoExt.TechnoClass_DTOR(R);
        }
        
        [Hook(HookType.AresHook, Address = 0x70C250, Size = 8)]
        [Hook(HookType.AresHook, Address = 0x70BF50, Size = 5)]
        static public unsafe UInt32 TechnoClass_SaveLoad_Prefix(REGISTERS* R)
        {
            return TechnoExt.TechnoClass_SaveLoad_Prefix(R);
        }

        [Hook(HookType.AresHook, Address = 0x70C249, Size = 5)]
        static public unsafe UInt32 TechnoClass_Load_Suffix(REGISTERS* R)
        {
            return TechnoExt.TechnoClass_Load_Suffix(R);
        }

        [Hook(HookType.AresHook, Address = 0x70C264, Size = 5)]
        static public unsafe UInt32 TechnoClass_Save_Suffix(REGISTERS* R)
        {
            return TechnoExt.TechnoClass_Save_Suffix(R);
        }
        
        
        [Hook(HookType.AresHook, Address = 0x6F9E50, Size = 5)]
        static public unsafe UInt32 TechnoClass_Update(REGISTERS* R)
        {
            try{
                return ScriptManager.TechnoClass_Update_Script(R);
            }
			catch (Exception e)
			{
                Logger.PrintException(e);
				return (uint)0;
			}
        }
        [Hook(HookType.AresHook, Address = 0x6F6CA0, Size = 7)]
        static public unsafe UInt32 TechnoClass_Put_Script(REGISTERS* R)
        {
            try
            {
                return ScriptManager.TechnoClass_Put_Script(R);
            }
            catch (Exception e)
            {
                Logger.PrintException(e);
                return (uint)0;
            }
        }
        // avoid hook conflict with phobos feature -- shield
        //[Hook(HookType.AresHook, Address = 0x6F6AC0, Size = 5)]
        [Hook(HookType.AresHook, Address = 0x6F6AC4, Size = 5)]
        static public unsafe UInt32 TechnoClass_Remove_Script(REGISTERS* R)
        {
            try
            {
                return ScriptManager.TechnoClass_Remove_Script(R);
            }
            catch (Exception e)
            {
                Logger.PrintException(e);
                return (uint)0;
            }
        }
        [Hook(HookType.AresHook, Address = 0x701900, Size = 6)]
        static public unsafe UInt32 TechnoClass_ReceiveDamage_Script(REGISTERS* R)
        {
            try
            {
                return ScriptManager.TechnoClass_ReceiveDamage_Script(R);
            }
            catch (Exception e)
            {
                Logger.PrintException(e);
                return (uint)0;
            }
        }
        [Hook(HookType.AresHook, Address = 0x6FDD50, Size = 6)]
        static public unsafe UInt32 TechnoClass_Fire(REGISTERS* R)
        {
            try{
                return ScriptManager.TechnoClass_Fire_Script(R);
            }
			catch (Exception e)
			{
                Logger.PrintException(e);
				return (uint)0;
			}
        }
    }
}