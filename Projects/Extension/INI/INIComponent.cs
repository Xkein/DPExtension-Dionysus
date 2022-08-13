using Extension.Utilities;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using Extension.EventSystems;
using Extension.INI;

namespace Extension.Components
{
    /// <summary>
    /// Component used to get ini value lazily
    /// </summary>
    [Serializable]
    public class INIComponent : Component
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">the ini filename</param>
        /// <param name="section">the section name in ini</param>
        /// <param name="nextIniComponent">next INIComponent to read if key not found in current INIComponent</param>
        public INIComponent(string name, string section, INIComponent nextIniComponent = null)
        {
            _name = name;
            _section = section;
            _nextIniComponent = nextIniComponent;
            nextIniComponent?.AttachToComponent(this);
            ReRead();
        }

        /// <summary>
        /// create INIComponent that read Ai
        /// </summary>
        /// <param name="section"></param>
        /// <returns></returns>
        public static INIComponent CreateAiIniComponent(string section)
        {
            var art = new INIComponent(INIConstant.AiName, section);
            return art;
        }

        /// <summary>
        /// create INIComponent that read Art
        /// </summary>
        /// <param name="section"></param>
        /// <returns></returns>
        public static INIComponent CreateArtIniComponent(string section)
        {
            var art = new INIComponent(INIConstant.ArtName, section);
            return art;
        }

        /// <summary>
        /// create INIComponent that read Rules, GameMode and Map
        /// </summary>
        /// <param name="section"></param>
        /// <returns></returns>
        public static INIComponent CreateRulesIniComponent(string section)
        {
            var rules = new INIComponent(INIConstant.RulesName, section);
            INIComponent map;
            if (SessionClass.Instance.GameMode != GameMode.Campaign)
            {
                var mode = new INIComponent(INIConstant.GameModeName, section, rules);
                map = new INIComponent(INIConstant.MapName, section, mode);
            }
            else
            {
                map = new INIComponent(INIConstant.MapName, section, rules);
            }
            return map;
        }

        public string ININame
        {
            get => _name;
            set
            {
                _name = value;
                ReRead();
            }
        }

        public string INISection
        {
            get => _section;
            set
            {
                _section = value;
                ReRead();
            }
        }

        /// <summary>
        /// get key value from ini
        /// </summary>
        /// <remarks>you can only get basic type value</remarks>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="def"></param>
        /// <returns></returns>
        public T Get<T>(string key, T def = default)
        {
            if (_buffer == null)
            {
                return def;
            }

            if (_buffer.GetParsed(key, out T val))
            {
                return val;
            }

            if (_nextIniComponent != null)
            {
                return _nextIniComponent.Get(key, def);
            }

            return def;
        }

        /// <summary>
        /// get key values from ini
        /// </summary>
        /// <remarks>you can only get basic type value</remarks>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="def"></param>
        /// <returns></returns>
        public T[] GetList<T>(string key, T[] def = default)
        {
            if (_buffer == null)
            {
                return def;
            }

            if (_buffer.GetParsedList(key, out T[] val))
            {
                return val;
            }

            if (_nextIniComponent != null)
            {
                return _nextIniComponent.GetList(key, def);
            }

            return def;
        }

        /// <summary>
        /// find buffer of create buffer stored globally
        /// </summary>
        public void ReRead()
        {
            INIBuffer buffer = FindBuffer(_name, _section);

            if (buffer == null)
            {
                buffer = new INIBuffer(_name, _section);

                // read ini section
                var pINI = YRMemory.Allocate<CCINIClass>().Construct();
                var pFile = YRMemory.Allocate<CCFileClass>().Construct(_name);
                INIReader reader = new INIReader(pINI);
                pINI.Ref.ReadCCFile(pFile);
                YRMemory.Delete(pFile);

                // read all pairs as <string, string> first
                int keyCount = pINI.Ref.GetKeyCount(_section);
                for (int i = 0; i < keyCount; i++)
                {
                    string key = pINI.Ref.GetKeyName(_section, i);
                    string val = null;
                    reader.Read(_section, key, ref val);
                    buffer.Unparsed[key] = val;
                }

                YRMemory.Delete(pINI);

                SetBuffer(_name, _section, buffer);
            }

            _buffer = buffer;
        }

        static INIComponent()
        {
            EventSystem.General.AddPermanentHandler(EventSystem.General.ScenarioClearClassesEvent, ScenarioClearClassesEventHandler);
        }

        private static void ScenarioClearClassesEventHandler(object sender, EventArgs e)
        {
            INIComponent.ClearBuffer();
        }

        /// <summary>
        /// buffer to store parsed and unparsed pairs in ini
        /// </summary>
        class INIBuffer
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

        /// <summary>
        /// share buffer for all INIComponent to avoid redundant read
        /// </summary>
        private static Dictionary<(string name, string section), INIBuffer> buffers = new();

        private static INIBuffer FindBuffer(string name, string section)
        {
            if (buffers.TryGetValue((name, section), out INIBuffer buffer))
            {
                return buffer;
            }

            return null;
        }

        private static void SetBuffer(string name, string section, INIBuffer buffer)
        {
            buffers[(name, section)] = buffer;
        }


        /// <summary>
        /// clear all parsed and unparsed buffer
        /// </summary>
        public static void ClearBuffer()
        {
            buffers.Clear();
        }

        public override void LoadFromStream(IStream stream)
        {
            ReRead();
        }

        private string _name;
        private string _section;
        [NonSerialized]
        private INIBuffer _buffer;
        private INIComponent _nextIniComponent;
    }
}
