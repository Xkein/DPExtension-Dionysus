using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using DynamicPatcher;

namespace Extension.Coroutines
{
    [Serializable]
    public class CoroutineSystem
    {
        private static readonly NullCoroutineWaiter NullWaiter = new NullCoroutineWaiter();

        public CoroutineSystem()
        {
            _coroutines = new Dictionary<IEnumerator, CoroutineWaiter>();
        }

        public int CoroutineCount => _coroutines.Count;

        public void StartCoroutine(IEnumerator coroutine)
        {
            if(coroutine == null)
                return;
            _coroutines.Add(coroutine, NullWaiter);
            RunCoroutine(coroutine);
        }

        public void StopCoroutine(IEnumerator coroutine)
        {
            if (coroutine == null)
                return;
            _coroutines.Remove(coroutine);
        }

        public void Update()
        {
            if (_coroutines.Count == 0)
                return;

            foreach (var coroutine in _coroutines.Keys.ToList())
            {
                RunCoroutine(coroutine);
            }
        }

        private void RunCoroutine(IEnumerator coroutine)
        {
            CoroutineWaiter waiter = _coroutines[coroutine];
            if (waiter.CanRun)
            {
                if (coroutine.MoveNext())
                {
                    object result = coroutine.Current;
                    switch (result)
                    {
                        case null:
                        case bool:
                        case byte:
                        case sbyte:
                        case short:
                        case ushort:
                        case int:
                        case uint:
                        case long:
                        case ulong:
                        case char:
                        case string:
                            SetCoroutineWaiter(coroutine, NullWaiter);
                            break;
                        case WaitForFrames f:
                            SetCoroutineWaiter(coroutine, new FramesCoroutineWaiter(f));
                            break;
                        case IEnumerator e:
                            SetCoroutineWaiter(coroutine, new EnumeratorCoroutineWaiter(e));
                            break;
                        default:
                            NotifyNotSupported(coroutine, result);
                            break;
                    }
                }
                else
                {
                    StopCoroutine(coroutine);
                }
            }
        }

        private void SetCoroutineWaiter(IEnumerator coroutine, CoroutineWaiter waiter)
        {
            _coroutines[coroutine] = waiter;
        }

        private void NotifyNotSupported(IEnumerator co, object result)
        {
            Logger.LogWarning("coroutine {0} return an unsupported result - {1}!", co, result);
        }

        private Dictionary<IEnumerator, CoroutineWaiter> _coroutines;
    }
}
