using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PatcherYRpp;

namespace Extension.Coroutines
{
    [Serializable]
    public abstract class CoroutineWaiter
    {
        public abstract bool CanRun { get; }
    }

    [Serializable]
    sealed class NullCoroutineWaiter : CoroutineWaiter
    {
        public override bool CanRun => true;
    }

    [Serializable]
    sealed class FramesCoroutineWaiter : CoroutineWaiter
    {
        public FramesCoroutineWaiter(WaitForFrames waitForFrames)
        {
            _waitForFrames = waitForFrames;
            _startFrame = Game.CurrentFrame;
        }

        public override bool CanRun => _waitForFrames.Frames + _startFrame <= Game.CurrentFrame;

        private WaitForFrames _waitForFrames;
        private int _startFrame;
    }

    [Serializable]
    class EnumeratorCoroutineWaiter : CoroutineWaiter
    {
        public EnumeratorCoroutineWaiter(IEnumerator enumerator)
        {
            _enumerator = enumerator;
            _coroutineSystem.StartCoroutine(enumerator);
            _lastUpdateFrame = Game.CurrentFrame;
        }

        public override bool CanRun
        {
            get
            {
                if (_lastUpdateFrame != Game.CurrentFrame)
                {
                    _lastUpdateFrame = Game.CurrentFrame;
                    _coroutineSystem.Update();
                }
                return _coroutineSystem.CoroutineCount == 0;
            }
        }

        private IEnumerator _enumerator;
        private CoroutineSystem _coroutineSystem = new();
        private int _lastUpdateFrame;
    }

    [Serializable]
    class AsyncResultCoroutineWaiter : CoroutineWaiter
    {
        public AsyncResultCoroutineWaiter(IAsyncResult asyncResult)
        {
            _asyncResult = asyncResult;
        }

        public override bool CanRun => _asyncResult == null || _asyncResult.IsCompleted;

        [NonSerialized]
        private IAsyncResult _asyncResult;
    }

    [Serializable]
    class CustomCoroutineWaiter : CoroutineWaiter
    {
        public CustomCoroutineWaiter(CustomYieldInstruction custom)
        {
            _custom = custom;
        }

        public override bool CanRun => !_custom.KeepWaiting;

        private CustomYieldInstruction _custom;
    }
}
