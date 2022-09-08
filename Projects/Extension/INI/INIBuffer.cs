using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extension.INI
{
    /// <summary>
    /// buffer to store parsed and unparsed pairs in ini
    /// </summary>
    internal class INIBuffer
    {
        public INIBuffer(string name, string section)
        {
            Name = name;
            Section = section;
            Unparsed = new Dictionary<string, string>();
            Parsed = new Dictionary<string, object>();
        }

        public string Name;
        public string Section;
        public Dictionary<string, string> Unparsed;
        public Dictionary<string, object> Parsed;

        public bool GetParsed<T>(string key, out T val)
        {
            if (Parsed.TryGetValue(key, out object parsed))
            {
                val = (T)parsed;
                return true;
            }

            T tmp = default;
            if (Unparsed.TryGetValue(key, out string unparsed) && Parsers.GetParser<T>().Parse(unparsed, ref tmp))
            {
                Parsed[key] = val = tmp;
                return true;
            }

            val = default;
            return false;
        }

        public bool GetParsedList<T>(string key, out T[] val)
        {
            if (Parsed.TryGetValue(key, out object parsed))
            {
                val = (T[])parsed;
                return true;
            }

            List<T> tmp = new List<T>();
            if (Unparsed.TryGetValue(key, out string unparsed) && Parsers.GetParser<T>().ParseList(unparsed, ref tmp))
            {
                Parsed[key] = val = tmp.ToArray();
                return true;
            }

            val = default;
            return false;
        }
    }

    internal class INILinkedBuffer
    {
        public INILinkedBuffer(INIBuffer buffer, INILinkedBuffer nextBuffer = null)
        {
            m_Buffer = buffer;
            m_LinkedBuffer = nextBuffer;
        }

        public string Name => m_Buffer.Name;
        public string Dependency => m_LinkedBuffer != null ? Name + "->" + m_LinkedBuffer.Dependency : Name;
        public string Section => m_Buffer.Section;

        public bool Expired { get; set; }

        public bool GetUnparsed(string key, out string val)
        {
            if (m_Buffer.Unparsed.TryGetValue(key, out val))
                return true;

            if (m_LinkedBuffer != null)
            {
                return m_LinkedBuffer.GetUnparsed(key, out val);
            }

            return false;
        }

        public bool GetParsed<T>(string key, out T val)
        {
            if (m_Buffer.GetParsed(key, out val))
                return true;

            if (m_LinkedBuffer != null)
            {
                return m_LinkedBuffer.GetParsed(key, out val);
            }

            return false;
        }

        public bool GetParsedList<T>(string key, out T[] val)
        {
            if (m_Buffer.GetParsedList(key, out val))
                return true;

            if (m_LinkedBuffer != null)
            {
                return m_LinkedBuffer.GetParsedList(key, out val);
            }

            return false;
        }

        private INIBuffer m_Buffer;
        private INILinkedBuffer m_LinkedBuffer;
    }
}
