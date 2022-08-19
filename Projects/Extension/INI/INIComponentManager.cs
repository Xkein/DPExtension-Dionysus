using Extension.EventSystems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extension.INI
{
    public static class INIComponentManager
    {


        static INIComponentManager()
        {
            EventSystem.General.AddPermanentHandler(EventSystem.General.ScenarioClearClassesEvent, ScenarioClearClassesEventHandler);
        }

        private static void ScenarioClearClassesEventHandler(object sender, EventArgs e)
        {
            ClearBuffer();
        }

        /// <summary>
        /// share buffer for all INIComponent to avoid redundant read
        /// </summary>
        private static Dictionary<(string name, string section), INIBuffer> s_Buffers = new();

        /// <summary>
        /// share buffer for all INIComponent to avoid redundant read
        /// </summary>
        private static Dictionary<(string name, string section), INIConfig> s_Config = new();

        internal static INIBuffer FindBuffer(string name, string section)
        {
            if (s_Buffers.TryGetValue((name, section), out INIBuffer buffer))
            {
                return buffer;
            }

            return null;
        }

        internal static void SetBuffer(string name, string section, INIBuffer buffer)
        {
            s_Buffers[(name, section)] = buffer;
        }


        internal static INIConfig FindData(string name, string section)
        {
            if (s_Config.TryGetValue((name, section), out INIConfig data))
            {
                return data;
            }

            return null;
        }

        internal static void SetData(string name, string section, INIConfig data)
        {
            s_Config[(name, section)] = data;
        }


        /// <summary>
        /// clear all parsed and unparsed buffer
        /// </summary>
        public static void ClearBuffer()
        {
            s_Buffers.Clear();
            s_Config.Clear();
        }
    }
}
