using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Extension.INI
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class NotINIFieldAttribute : Attribute { }
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class INIFieldAttribute : Attribute
    {
        public string Key { get; set; }

        //public int MinCount { get; set; }
        //public int MaxCount { get; set; }

        //public object Min { get; set; }
        //public object Max { get; set; }
    }

    public abstract class INIConfig
    {
        /// <summary>
        /// read data from INIComponent
        /// </summary>
        /// <param name="ini"></param>
        public abstract void Read(INIComponent ini);
    }

    public abstract class INIAutoConfig : INIConfig
    {
        public sealed override void Read(INIComponent ini)
        {
            FieldInfo[] fields = this.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
            MethodInfo getList = ini.GetType().GetMethod("GetList");
            MethodInfo get = ini.GetType().GetMethod("Get");

            foreach (FieldInfo field in fields)
            {
                if (!field.IsDefined(typeof(NotINIFieldAttribute)))
                {
                    var iniField = field.GetCustomAttribute<INIFieldAttribute>();
                    MethodInfo getMethod = (field.FieldType.IsArray ? getList : get).MakeGenericMethod(field.FieldType);

                    string key = iniField?.Key ?? field.Name;
                    var val = getMethod.Invoke(ini, new object[] { key, field.GetValue(this) });
                    field.SetValue(this, val);
                }
            }
        }
    }

}
