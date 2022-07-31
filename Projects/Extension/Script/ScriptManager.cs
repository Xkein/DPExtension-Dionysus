using DynamicPatcher;
using Extension.Components;
using Extension.Ext;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Extension.Script
{
    public partial class ScriptManager
    {
        // type name -> script
        static Dictionary<string, Script> Scripts = new Dictionary<string, Script>();

        /// <summary>
        /// create script or get a exist script by script name
        /// </summary>
        /// <param name="scriptName"></param>
        /// <returns></returns>
        public static Script GetScript(string scriptName)
        {
            if(scriptName == null)
                return null; 

            if (Scripts.TryGetValue(scriptName, out Script script))
            {
                return script;
            }
            else
            {
                Script newScript = new Script(scriptName);
                try
                {
                    Assembly assembly = FindScriptAssembly(scriptName);

                    RefreshScript(newScript, assembly);

                    Scripts.Add(scriptName, newScript);
                    return newScript;
                }
                catch (Exception e)
                {
                    Logger.LogError("ScriptManager could not find script: {0}", scriptName);
                    Logger.PrintException(e);
                    return null;
                }
            }
        }
        /// <summary>
        /// get all scripts in cs file
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static List<Script> GetScripts(string fileName)
        {
            List<Script> scripts = new List<Script>();

            var pair = Program.Patcher.FileAssembly.First((pair) => pair.Key.EndsWith(fileName));
            Assembly assembly = pair.Value;

            Type[] types = FindScriptTypes(assembly);

            foreach (var type in types)
            {
                Script script = GetScript(type.Name);
                scripts.Add(script);
            }

            return scripts;
        }
        /// <summary>
        /// get scripts by script names or cs file names
        /// </summary>
        /// <param name="scriptList"></param>
        /// <returns></returns>
        public static List<Script> GetScripts(IEnumerable<string> scriptList)
        {
            List<Script> scripts = new List<Script>();

            foreach (string item in scriptList)
            {
                if (item.EndsWith(".cs"))
                {
                    scripts.AddRange(GetScripts(item));
                }
                else
                {
                    scripts.Add(GetScript(item));
                }
            }

            return scripts;
        }

        public static void CreateScriptableTo(Component root, IEnumerable<Script> scripts, params object[] parameters)
        {
            if (scripts == null)
                return;

            foreach (var script in scripts)
            {
                CreateScriptableTo(root, script, parameters);
            }
        }

        public static ScriptComponent CreateScriptableTo(Component root, Script script, params object[] parameters)
        {
            if (script == null)
                return null;

            var scriptComponent = CreateScriptable<ScriptComponent>(script, parameters);
            scriptComponent.AttachToComponent(root);
            return scriptComponent;
        }

        public static TScriptable CreateScriptable<TScriptable>(Script script, params object[] parameters) where TScriptable : ScriptComponent
        {
            if (script == null)
                return null;

            var scriptable = Activator.CreateInstance(script.ScriptableType, parameters) as TScriptable;
            scriptable.Script = script;
            return scriptable;
        }

        private static Type[] FindScriptTypes(Assembly assembly)
        {
            Type[] types = assembly.GetTypes();
            if (types == null || types.Length == 0)
                return new Type[0];

            return types.Where(t => typeof(IScriptable).IsAssignableFrom(t)).ToArray();
        }

        private static Assembly FindScriptAssembly(string scriptName)
        {
            foreach (var pair in Program.Patcher.FileAssembly)
            {
                Assembly assembly = pair.Value;
                Type[] types = FindScriptTypes(assembly);
                foreach (Type type in types)
                {
                    if (type.Name == scriptName)
                    {
                        return assembly;
                    }
                }
            }

            return null;
        }

        private static void RefreshScript(Script script, Assembly assembly)
        {
            Type[] types = FindScriptTypes(assembly);
            foreach (Type type in types)
            {
                if (type.Name == script.Name)
                {
                    script.ScriptableType = type;
                    break;
                }
            }
        }

        private static void RefreshScripts(Assembly assembly)
        {
            Type[] types = FindScriptTypes(assembly);

            foreach (Type type in types)
            {
                string scriptName = type.Name;
                if (Scripts.TryGetValue(scriptName, out Script script))
                {
                    RefreshScript(script, assembly);
                }
                else
                {
                    script = GetScript(scriptName);
                }

                Logger.Log("refresh script: {0}", script.Name);
            }

        }


        private static void Patcher_AssemblyRefresh(object sender, AssemblyRefreshEventArgs args)
        {
            Assembly assembly = args.RefreshedAssembly;
            RefreshScripts(assembly);

            Type[] types = FindScriptTypes(assembly);
            foreach (Type type in types)
            {

                // [warning!] unsafe change to scriptable
                unsafe
                {
                    // refresh modified scripts only
                    void RefreshScriptComponents<TExt, TBase>(ECSInstanceExtension<TExt, TBase> ext) where TExt : Extension<TBase>
                    {
                        ScriptComponent[] components = ext.GameObject.GetComponentsInChildren(c => c.GetType().Name == type.Name).Cast<ScriptComponent>().ToArray();
                        if (components.Length > 0)
                        {
                            foreach (var component in components)
                            {
                                var root = component.Parent;
                                var script = component.Script;

                                component.DetachFromParent();

                                CreateScriptableTo(root, script, ext);
                            }
                        }
                    }

                    void Refresh<TExt, TBase>(Container<TExt, TBase> container, ref DynamicVectorClass<Pointer<TBase>> dvc) where TExt : ECSInstanceExtension<TExt, TBase>
                    {
                        Logger.Log("refreshing {0}'s ScriptComponents...", typeof(TExt).Name);
                        foreach (var pItem in dvc)
                        {
                            var ext = container.Find(pItem);
                            RefreshScriptComponents(ext);
                        }
                    }

                    Refresh(TechnoExt.ExtMap, ref TechnoClass.Array);
                    Refresh(BulletExt.ExtMap, ref BulletClass.Array);
#if USE_ANIM_EXT
                    Refresh(AnimExt.ExtMap, ref AnimClass.Array);
#endif
                }
            }
        }

        private static void RefreshScriptable<TExt, TBase>() where TExt : IScriptable
        {

        }

        static ScriptManager()
        {
            Program.Patcher.AssemblyRefresh += Patcher_AssemblyRefresh;
        }


        #region 使用lambda表达式树创建对象
        public static TScriptable CreateScriptable<TScriptable>(Script script) where TScriptable : ScriptComponent
        {
            if (script == null)
                return null;

            if (!ScriptCtors.ContainsKey(script.ScriptableType.Name))
            {
                List<ParameterExpression> parameterExpressions = new List<ParameterExpression>();
                var ctor = script.ScriptableType.GetConstructors()[0];
                NewExpression ctorExpression = Expression.New(ctor, parameterExpressions);
                var expression = Expression.Lambda<Func<TScriptable>>(ctorExpression, parameterExpressions);
                var lambda = expression.Compile();
                ScriptCtors.Add(script.ScriptableType.Name, lambda);
            }

            var func = ScriptCtors[script.ScriptableType.Name] as Func<TScriptable>;

            var scriptable = func();
            scriptable.Script = script;
            return scriptable;
        }

        public static TScriptable CreateScriptable<T1, TScriptable>(Script script, T1 p1) where TScriptable : ScriptComponent
        {
            if (script == null)
                return null;

            if (!ScriptCtors.ContainsKey(script.ScriptableType.Name))
            {
                List<ParameterExpression> parameterExpressions = new List<ParameterExpression>()
                {
                    Expression.Parameter(typeof(T1), "t1")
                };

                var ctor = script.ScriptableType.GetConstructors()[0];
                NewExpression ctorExpression = Expression.New(ctor, parameterExpressions);
                var expression = Expression.Lambda<Func<T1, TScriptable>>(ctorExpression, parameterExpressions);
                var lambda = expression.Compile();
                ScriptCtors.Add(script.ScriptableType.Name, lambda);
            }

            var func = ScriptCtors[script.ScriptableType.Name] as Func<T1, TScriptable>;

            var scriptable = func(p1);
            scriptable.Script = script;
            return scriptable;
        }

        public static TScriptable CreateScriptable<T1, T2, TScriptable>(Script script, T1 p1, T2 p2) where TScriptable : ScriptComponent
        {
            if (script == null)
                return null;

            if (!ScriptCtors.ContainsKey(script.ScriptableType.Name))
            {
                List<ParameterExpression> parameterExpressions = new List<ParameterExpression>()
                {
                    Expression.Parameter(typeof(T1), "t1"),
                    Expression.Parameter(typeof(T1), "t2")
                };

                var ctor = script.ScriptableType.GetConstructors()[0];
                NewExpression ctorExpression = Expression.New(ctor, parameterExpressions);
                var expression = Expression.Lambda<Func<T1, T2, TScriptable>>(ctorExpression, parameterExpressions);
                var lambda = expression.Compile();
                ScriptCtors.Add(script.ScriptableType.Name, lambda);
            }

            var func = ScriptCtors[script.ScriptableType.Name] as Func<T1, T2, TScriptable>;

            var scriptable = func(p1, p2);
            scriptable.Script = script;
            return scriptable;
        }

        private static Dictionary<string, object> ScriptCtors = new Dictionary<string, object>();
        #endregion


    }
}
