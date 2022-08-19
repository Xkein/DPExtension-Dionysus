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
}
