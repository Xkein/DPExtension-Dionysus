using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Extension.Serialization
{
    public class DefaultSurrogateSelector : ISurrogateSelector
    {
        ISurrogateSelector _next;

        public virtual void ChainSelector(ISurrogateSelector selector)
        {
            _next = selector;
        }

        public virtual ISurrogateSelector GetNextSelector()
        {
            return _next;
        }

        public virtual ISerializationSurrogate GetSurrogate(Type type, StreamingContext context, out ISurrogateSelector selector)
        {
            if (_next == null)
            {
                selector = null;
                return null;
            }

            return _next.GetSurrogate(type, context, out selector);
        }
    }

    public class DefaultSerializationSurrogate : ISerializationSurrogate
    {
        public virtual void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            foreach (FieldInfo f in obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                info.AddValue(f.Name, f.GetValue(obj));
            }
        }

        public virtual object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            foreach (FieldInfo f in obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                f.SetValue(obj, info.GetValue(f.Name, f.FieldType));
            }
            return obj;
        }
    }
}
