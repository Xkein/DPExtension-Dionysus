using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Extension.Serialization
{
    public class MainSerializer
    {
        private static IFormatter GetFormatter()
        {
            return new EnhancedFormatter();
        }

        public static void Serialize(Stream serializationStream, object graph)
        {
            var formatter = GetFormatter();

            formatter.Serialize(serializationStream, graph);
        }

        public static object Deserialize(Stream serializationStream)
        {
            var formatter = GetFormatter();
            object graph = formatter.Deserialize(serializationStream);

            return graph;
        }

        public static T Deserialize<T>(Stream serializationStream)
        {
            return (T)Deserialize(serializationStream);
        }
    }
}
