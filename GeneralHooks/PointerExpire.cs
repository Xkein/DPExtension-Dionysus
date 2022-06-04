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
        static PointerExpire()
        {
            EventSystem.PointerExpire.AddPermanentHandler(EventSystem.PointerExpire.AnnounceExpiredPointerEvent, ObjectFinderHandler);
        }

        private static void ObjectFinderHandler(object sender, EventArgs e)
        {
            var args = (AnnounceExpiredPointerEventArgs)e;
            var pAbstract = args.ExpiredPointer;

            if (pAbstract.CastToObject(out Pointer<ObjectClass> pObject))
            {
                ObjectFinder.ObjectContainer.PointerExpired(pObject);
                if (pAbstract.CastToTechno(out var _))
                {
                    ObjectFinder.TechnoContainer.PointerExpired(pObject);
                }
            }

            //Logger.Log("invoke AnnounceExpiredPointer({0}, {1})", DebugUtilities.GetAbstractID(pAbstract), removed);
        }

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
