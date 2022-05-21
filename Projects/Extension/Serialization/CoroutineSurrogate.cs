using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Extension.Serialization
{
    // https://stackoverflow.com/questions/3294224/serialization-and-the-yield-statement

    class CoroutineSurrogateSelector : ISurrogateSelector
    {
        ISurrogateSelector _next;

        public void ChainSelector(ISurrogateSelector selector)
        {
            _next = selector;
        }

        public ISurrogateSelector GetNextSelector()
        {
            return _next;
        }

        public ISerializationSurrogate GetSurrogate(Type type, StreamingContext context, out ISurrogateSelector selector)
        {
            if (typeof(System.Collections.IEnumerator).IsAssignableFrom(type)
                && type.IsSerializable == false
                && type.GetCustomAttribute<CompilerGeneratedAttribute>() != null)
            {
                selector = this;
                return new CoroutineSerializationSurrogate();
            }
            
            if (_next == null)
            {
                selector = null;
                return null;
            }

            return _next.GetSurrogate(type, context, out selector);
        }
    }


    class CoroutineSerializationSurrogate : DefaultSerializationSurrogate
    {

    }
}
