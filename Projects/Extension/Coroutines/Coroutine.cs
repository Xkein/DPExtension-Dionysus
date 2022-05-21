using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extension.Coroutines
{
    [Serializable]
    public sealed class Coroutine : YieldInstruction
    {
        internal Coroutine(IEnumerator enumerator)
        {
            _enumerator = enumerator;
        }

        public string Name => _enumerator.GetType().Name;

        internal CoroutineWaiter Waiter { get; set; }
        internal IEnumerator Enumerator => _enumerator;

        private IEnumerator _enumerator;
    }
}
