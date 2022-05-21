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
            _coroutines = new List<Coroutine>();
        }

        public int CoroutineCount => _coroutines.Count;

        public Coroutine StartCoroutine(Coroutine coroutine)
        {
            if (coroutine == null || coroutine.Enumerator == null)
                return null;
            _coroutines.Add(coroutine);
            RunCoroutine(coroutine);
            return coroutine;
        }
        public Coroutine StartCoroutine(IEnumerator enumerator)
        {
            if(enumerator == null)
                return null;
            var coroutine = new Coroutine(enumerator) { Waiter = NullWaiter };
            return StartCoroutine(coroutine);
        }

        public void StopCoroutine(Coroutine coroutine)
        {
            if (coroutine == null)
                return;
            _coroutines.Remove(coroutine);
        }
        public void StopCoroutine(IEnumerator enumerator)
        {
            if (enumerator == null)
                return;
            StopCoroutine(_coroutines.Find(c => c.Enumerator.Equals(enumerator)));
        }

        public void Update()
        {
            if (_coroutines.Count == 0)
                return;

            foreach (var coroutine in _coroutines.ToList())
            {
                RunCoroutine(coroutine);
            }
        }

        private void RunCoroutine(Coroutine coroutine)
        {
            try
            {
                CoroutineWaiter waiter = coroutine.Waiter;
                if (waiter.CanRun)
                {
                    var enumerator = coroutine.Enumerator;
                    if (enumerator.MoveNext())
                    {
                        object result = enumerator.Current;
                        switch (result)
                        {
                            case WaitForFrames f:
                                coroutine.Waiter = new FramesCoroutineWaiter(f);
                                break;
                            case CustomYieldInstruction c:
                                coroutine.Waiter = new CustomCoroutineWaiter(c);
                                break;
                            case IEnumerator e:
                                coroutine.Waiter = new EnumeratorCoroutineWaiter(e);
                                break;
                            case IAsyncResult a:
                                coroutine.Waiter = new AsyncResultCoroutineWaiter(a);
                                break;
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
                                coroutine.Waiter = NullWaiter;
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
            catch (Exception e)
            {
                Logger.PrintException(e);
            }
        }

        private void NotifyNotSupported(Coroutine co, object result)
        {
            Logger.LogWarning("coroutine {0} return an unsupported result - {1}!", co, result);
        }

        private List<Coroutine> _coroutines;
    }
}
