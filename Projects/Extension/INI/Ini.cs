using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extension.INI
{
    public static class Ini
    {
        /// <summary>
        /// Get dependency chain for ini
        /// </summary>
        /// <param name="iniName"></param>
        /// <returns></returns>
        public static string GetDependency(string iniName)
        {
            return INIComponentManager.GetDependency(iniName);
        }

        /// <summary>
        /// Get key value from ini
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dependency">Dependency chain for ini</param>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <param name="def"></param>
        /// <returns></returns>
        public static T Get<T>(string dependency, string section, string key, T def = default)
        {
            if (INIComponentManager.FindLinkedBuffer(dependency, section).GetParsed(key, out T val))
            {
                return val;
            }

            return def;
        }

        /// <summary>
        /// Get key values from ini
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dependency">Dependency chain for ini</param>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <param name="def"></param>
        /// <returns></returns>
        public static T[] GetList<T>(string dependency, string section, string key, T[] def = default)
        {
            if (INIComponentManager.FindLinkedBuffer(dependency, section).GetParsedList(key, out T[] val))
            {
                return val;
            }

            return def;
        }

        /// <summary>
        /// Get lazy reader for ini section
        /// </summary>
        /// <param name="dependency">dependency chain for ini</param>
        /// <param name="section"></param>
        /// <returns></returns>
        public static ISectionReader GerSection(string dependency, string section)
        {
            return new INIComponent(dependency, section);
        }

        /// <summary>
        /// Get lazy config wrapper for ini section
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dependency">Dependency chain for ini</param>
        /// <param name="section"></param>
        /// <returns></returns>
        public static IConfigWrapper<T> GetConfig<T>(string dependency, string section) where T : INIConfig, new()
        {
            return new INIComponentWith<T>(dependency, section);
        }

        public static void ClearBuffer()
        {
            INIComponentManager.ClearBuffer();
        }
    }
}
