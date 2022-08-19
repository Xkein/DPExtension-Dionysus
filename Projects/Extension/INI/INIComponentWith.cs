using Extension.Components;
using PatcherYRpp;
using System;

namespace Extension.INI
{
    /// <summary>
    /// Component used to get ini value lazily
    /// </summary>
    [Serializable]
    public class INIComponentWith<T> : INIComponent where T : INIConfig, new()
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">the ini filename</param>
        /// <param name="section">the section name in ini</param>
        /// <param name="nextIniComponent">next INIComponent to read if key not found in current INIComponent</param>
        public INIComponentWith(string name, string section, INIComponent nextIniComponent = null) : base(name, section, nextIniComponent)
        {
        }

        public T Data
        {
            get
            {
                if (m_Data == null)
                {
                    ReReadData();
                }

                return m_Data;
            }
        }

        public override void ReRead()
        {
            m_Data = null;

            base.ReRead();
        }

        private void ReReadData()
        {
            T data = new();

            data.Read(this);

            INIComponentManager.SetData(Name, INISection, m_Data = data);
        }


        /// <summary>
        /// create INIComponent that read Ai
        /// </summary>
        /// <param name="section"></param>
        /// <returns></returns>
        public static new INIComponentWith<T> CreateAiIniComponent(string section)
        {
            var art = new INIComponentWith<T>(INIConstant.AiName, section);
            return art;
        }

        /// <summary>
        /// create INIComponent that read Art
        /// </summary>
        /// <param name="section"></param>
        /// <returns></returns>
        public static new INIComponentWith<T> CreateArtIniComponent(string section)
        {
            var art = new INIComponentWith<T>(INIConstant.ArtName, section);
            return art;
        }

        /// <summary>
        /// create INIComponent that read Rules, GameMode and Map
        /// </summary>
        /// <param name="section"></param>
        /// <returns></returns>
        public static new INIComponentWith<T> CreateRulesIniComponent(string section)
        {
            var rules = new INIComponentWith<T>(INIConstant.RulesName, section);
            INIComponentWith<T> map;
            if (SessionClass.Instance.GameMode != GameMode.Campaign)
            {
                var mode = new INIComponentWith<T>(INIConstant.GameModeName, section, rules);
                map = new INIComponentWith<T>(INIConstant.MapName, section, mode);
            }
            else
            {
                map = new INIComponentWith<T>(INIConstant.MapName, section, rules);
            }
            return map;
        }


        [NonSerialized] private T m_Data;
    }

    public static class INIComponentWithHelpers
    {
        public static INIComponentWith<T> CreateAiIniComponentWith<T>(this Component component, string section) where T : INIConfig, new()
        {
            var ini = INIComponentWith<T>.CreateAiIniComponent(section);
            ini.AttachToComponent(component);
            return ini;
        }
        public static INIComponentWith<T> CreateArtIniComponentWith<T>(this Component component, string section) where T : INIConfig, new()
        {
            var ini = INIComponentWith<T>.CreateArtIniComponent(section);
            ini.AttachToComponent(component);
            return ini;
        }
        public static INIComponentWith<T> CreateRulesIniComponentWith<T>(this Component component, string section) where T : INIConfig, new()
        {
            var ini = INIComponentWith<T>.CreateRulesIniComponent(section);
            ini.AttachToComponent(component);
            return ini;
        }
    }
}
