using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;

namespace Extension.EventSystems
{
    public class SaveGameEvent : EventBase
    {
        public override string Name => "SaveGame";
        public override string Description => "Raised when saving game";
    }
    public class LoadGameEvent : EventBase
    {
        public override string Name => "SaveGame";
        public override string Description => "Raised when saving game";
    }

    public class SaveGameEventArgs : EventArgs
    {
        public SaveGameEventArgs(IStream stream, bool isStart)
        {
            Stream = stream;
            IsStart = isStart;
        }

        public IStream Stream { get; }
        public bool IsStart { get; }
        public bool IsEnd => !IsStart;
    }

    public class LoadGameEventArgs : EventArgs
    {
        public LoadGameEventArgs(IStream stream, bool isStart)
        {
            Stream = stream;
            IsStart = isStart;
        }

        public IStream Stream { get; }
        public bool IsStart { get; }
        public bool IsEnd => !IsStart;
    }

    public class SaveGameEventSystem : EventSystem
    {
        public SaveGameEventSystem()
        {
            SaveGameEvent = new SaveGameEvent();
            LoadGameEvent = new LoadGameEvent();
        }

        public SaveGameEvent SaveGameEvent { get; }
        public LoadGameEvent LoadGameEvent { get; }

    }
}
