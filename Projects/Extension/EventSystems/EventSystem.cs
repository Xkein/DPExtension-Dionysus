using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamicPatcher;

namespace Extension.EventSystems
{
    public abstract class EventBase
    {
        public abstract string Name { get; }
        public abstract string Description { get; }
    }

    public abstract class EventSystem
    {
        
        public static GeneralEventSystem General { get; }
        public static PointerExpireEventSystem PointerExpire { get; }
        public static SaveGameEventSystem SaveGame { get; }


        static EventSystem()
        {
            General = new GeneralEventSystem();
            PointerExpire = new PointerExpireEventSystem();
            SaveGame = new SaveGameEventSystem();
        }

        /// <summary>
        /// add handler that exists permanently during game process
        /// </summary>
        /// <param name="e"></param>
        /// <param name="handler"></param>
        public virtual void AddPermanentHandler(EventBase e, EventHandler handler)
        {
            AddHandler(_permanentHandlers, e, handler);
        }
        /// <summary>
        /// remove handler that exists permanently during game process
        /// </summary>
        /// <param name="e"></param>
        /// <param name="handler"></param>
        public virtual void RemovePermanentHandler(EventBase e, EventHandler handler)
        {
            RemoveHandler(_permanentHandlers, e, handler);
        }

        /// <summary>
        /// add handler that only exists in single game scenario
        /// </summary>
        /// <remarks>UNFINISHED</remarks>
        /// <param name="e"></param>
        /// <param name="handler"></param>
        public virtual void AddTemporaryHandler(EventBase e, EventHandler handler)
        {
            AddHandler(_temporaryHandler, e, handler);
        }
        /// <summary>
        /// remove handler that only exists in single game scenario
        /// </summary>
        /// <remarks>UNFINISHED</remarks>
        /// <param name="e"></param>
        /// <param name="handler"></param>
        public virtual void RemoveTemporaryHandler(EventBase e, EventHandler handler)
        {
            RemoveHandler(_temporaryHandler, e, handler);
        }

        public virtual void Broadcast(EventBase e, EventArgs args)
        {
            Broadcast(_permanentHandlers, e, args);
            Broadcast(_temporaryHandler, e, args);
        }



        private void AddHandler(Dictionary<EventBase, EventHandler> handlers, EventBase e, EventHandler handler)
        {
            if (!handlers.ContainsKey(e))
            {
                handlers[e] = handler;
            }
            else
            {
                handlers[e] += handler;
            }

        }
        private void RemoveHandler(Dictionary<EventBase, EventHandler> handlers, EventBase e, EventHandler handler)
        {
            if (handlers.ContainsKey(e))
            {
                handlers[e] -= handler;
            }
        }
        private void Broadcast(Dictionary<EventBase, EventHandler> handlers, EventBase e, EventArgs args)
        {
            if (handlers.TryGetValue(e, out var handler))
            {
                try
                {
                    handler(this, args);
                }
                catch (Exception exception)
                {
                    Logger.PrintException(exception);
                }
            }
        }

        private Dictionary<EventBase, EventHandler> _permanentHandlers = new();
        private Dictionary<EventBase, EventHandler> _temporaryHandler = new();
    }
    
}
