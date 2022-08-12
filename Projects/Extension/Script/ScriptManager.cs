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
                    void RefreshScriptComponents<TExt, TBase>(GOInstanceExtension<TExt, TBase> ext) where TExt : Extension<TBase>
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

                    void Refresh<TExt, TBase>(Container<TExt, TBase> container, ref DynamicVectorClass<Pointer<TBase>> dvc) where TExt : GOInstanceExtension<TExt, TBase>
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

        public static TScriptable CreateScriptable<T1, T2, T3, TScriptable>(Script script, T1 p1, T2 p2, T3 p3) where TScriptable : ScriptComponent
        {
            if (script == null)
                return null;

            if (!ScriptCtors.ContainsKey(script.ScriptableType.Name))
            {
                List<ParameterExpression> parameterExpressions = new List<ParameterExpression>()
            {
                    Expression.Parameter(typeof(T1), "t1"),
                    Expression.Parameter(typeof(T2), "t2"),
                    Expression.Parameter(typeof(T3), "t3")
            };

                var ctor = script.ScriptableType.GetConstructors()[0];
                NewExpression ctorExpression = Expression.New(ctor, parameterExpressions);
                var expression = Expression.Lambda<Func<T1, T2, T3, TScriptable>>(ctorExpression, parameterExpressions);
                var lambda = expression.Compile();
                ScriptCtors.Add(script.ScriptableType.Name, lambda);
            }

            var func = ScriptCtors[script.ScriptableType.Name] as Func<T1, T2, T3, TScriptable>;

            var scriptable = func(p1, p2, p3);
            scriptable.Script = script;
            return scriptable;
        }
        public static TScriptable CreateScriptable<T1, T2, T3, T4, TScriptable>(Script script, T1 p1, T2 p2, T3 p3, T4 p4) where TScriptable : ScriptComponent
        {
            if (script == null)
                return null;

            if (!ScriptCtors.ContainsKey(script.ScriptableType.Name))
            {
                List<ParameterExpression> parameterExpressions = new List<ParameterExpression>()
            {
                    Expression.Parameter(typeof(T1), "t1"),
                    Expression.Parameter(typeof(T2), "t2"),
                    Expression.Parameter(typeof(T3), "t3"),
                    Expression.Parameter(typeof(T4), "t4")
            };

                var ctor = script.ScriptableType.GetConstructors()[0];
                NewExpression ctorExpression = Expression.New(ctor, parameterExpressions);
                var expression = Expression.Lambda<Func<T1, T2, T3, T4, TScriptable>>(ctorExpression, parameterExpressions);
                var lambda = expression.Compile();
                ScriptCtors.Add(script.ScriptableType.Name, lambda);
            }

            var func = ScriptCtors[script.ScriptableType.Name] as Func<T1, T2, T3, T4, TScriptable>;

            var scriptable = func(p1, p2, p3, p4);
            scriptable.Script = script;
            return scriptable;
        }
        public static TScriptable CreateScriptable<T1, T2, T3, T4, T5, TScriptable>(Script script, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5) where TScriptable : ScriptComponent
        {
            if (script == null)
                return null;

            if (!ScriptCtors.ContainsKey(script.ScriptableType.Name))
            {
                List<ParameterExpression> parameterExpressions = new List<ParameterExpression>()
            {
                    Expression.Parameter(typeof(T1), "t1"),
                    Expression.Parameter(typeof(T2), "t2"),
                    Expression.Parameter(typeof(T3), "t3"),
                    Expression.Parameter(typeof(T4), "t4"),
                    Expression.Parameter(typeof(T5), "t5")
            };

                var ctor = script.ScriptableType.GetConstructors()[0];
                NewExpression ctorExpression = Expression.New(ctor, parameterExpressions);
                var expression = Expression.Lambda<Func<T1, T2, T3, T4, T5, TScriptable>>(ctorExpression, parameterExpressions);
                var lambda = expression.Compile();
                ScriptCtors.Add(script.ScriptableType.Name, lambda);
            }

            var func = ScriptCtors[script.ScriptableType.Name] as Func<T1, T2, T3, T4, T5, TScriptable>;

            var scriptable = func(p1, p2, p3, p4, p5);
            scriptable.Script = script;
            return scriptable;
        }
        public static TScriptable CreateScriptable<T1, T2, T3, T4, T5, T6, TScriptable>(Script script, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6) where TScriptable : ScriptComponent
        {
            if (script == null)
                return null;

            if (!ScriptCtors.ContainsKey(script.ScriptableType.Name))
            {
                List<ParameterExpression> parameterExpressions = new List<ParameterExpression>()
            {
                    Expression.Parameter(typeof(T1), "t1"),
                    Expression.Parameter(typeof(T2), "t2"),
                    Expression.Parameter(typeof(T3), "t3"),
                    Expression.Parameter(typeof(T4), "t4"),
                    Expression.Parameter(typeof(T5), "t5"),
                    Expression.Parameter(typeof(T6), "t6")
            };

                var ctor = script.ScriptableType.GetConstructors()[0];
                NewExpression ctorExpression = Expression.New(ctor, parameterExpressions);
                var expression = Expression.Lambda<Func<T1, T2, T3, T4, T5, T6, TScriptable>>(ctorExpression, parameterExpressions);
                var lambda = expression.Compile();
                ScriptCtors.Add(script.ScriptableType.Name, lambda);
            }

            var func = ScriptCtors[script.ScriptableType.Name] as Func<T1, T2, T3, T4, T5, T6, TScriptable>;

            var scriptable = func(p1, p2, p3, p4, p5, p6);
            scriptable.Script = script;
            return scriptable;
        }
        public static TScriptable CreateScriptable<T1, T2, T3, T4, T5, T6, T7, TScriptable>(Script script, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7) where TScriptable : ScriptComponent
        {
            if (script == null)
                return null;

            if (!ScriptCtors.ContainsKey(script.ScriptableType.Name))
            {
                List<ParameterExpression> parameterExpressions = new List<ParameterExpression>()
            {
                    Expression.Parameter(typeof(T1), "t1"),
                    Expression.Parameter(typeof(T2), "t2"),
                    Expression.Parameter(typeof(T3), "t3"),
                    Expression.Parameter(typeof(T4), "t4"),
                    Expression.Parameter(typeof(T5), "t5"),
                    Expression.Parameter(typeof(T6), "t6"),
                    Expression.Parameter(typeof(T7), "t7")
            };

                var ctor = script.ScriptableType.GetConstructors()[0];
                NewExpression ctorExpression = Expression.New(ctor, parameterExpressions);
                var expression = Expression.Lambda<Func<T1, T2, T3, T4, T5, T6, T7, TScriptable>>(ctorExpression, parameterExpressions);
                var lambda = expression.Compile();
                ScriptCtors.Add(script.ScriptableType.Name, lambda);
            }

            var func = ScriptCtors[script.ScriptableType.Name] as Func<T1, T2, T3, T4, T5, T6, T7, TScriptable>;

            var scriptable = func(p1, p2, p3, p4, p5, p6, p7);
            scriptable.Script = script;
            return scriptable;
        }
        public static TScriptable CreateScriptable<T1, T2, T3, T4, T5, T6, T7, T8, TScriptable>(Script script, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8) where TScriptable : ScriptComponent
        {
            if (script == null)
                return null;

            if (!ScriptCtors.ContainsKey(script.ScriptableType.Name))
            {
                List<ParameterExpression> parameterExpressions = new List<ParameterExpression>()
            {
                    Expression.Parameter(typeof(T1), "t1"),
                    Expression.Parameter(typeof(T2), "t2"),
                    Expression.Parameter(typeof(T3), "t3"),
                    Expression.Parameter(typeof(T4), "t4"),
                    Expression.Parameter(typeof(T5), "t5"),
                    Expression.Parameter(typeof(T6), "t6"),
                    Expression.Parameter(typeof(T7), "t7"),
                    Expression.Parameter(typeof(T8), "t8")
            };

                var ctor = script.ScriptableType.GetConstructors()[0];
                NewExpression ctorExpression = Expression.New(ctor, parameterExpressions);
                var expression = Expression.Lambda<Func<T1, T2, T3, T4, T5, T6, T7, T8, TScriptable>>(ctorExpression, parameterExpressions);
                var lambda = expression.Compile();
                ScriptCtors.Add(script.ScriptableType.Name, lambda);
            }

            var func = ScriptCtors[script.ScriptableType.Name] as Func<T1, T2, T3, T4, T5, T6, T7, T8, TScriptable>;

            var scriptable = func(p1, p2, p3, p4, p5, p6, p7, p8);
            scriptable.Script = script;
            return scriptable;
        }
        public static TScriptable CreateScriptable<T1, T2, T3, T4, T5, T6, T7, T8, T9, TScriptable>(Script script, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9) where TScriptable : ScriptComponent
        {
            if (script == null)
                return null;

            if (!ScriptCtors.ContainsKey(script.ScriptableType.Name))
            {
                List<ParameterExpression> parameterExpressions = new List<ParameterExpression>()
            {
                    Expression.Parameter(typeof(T1), "t1"),
                    Expression.Parameter(typeof(T2), "t2"),
                    Expression.Parameter(typeof(T3), "t3"),
                    Expression.Parameter(typeof(T4), "t4"),
                    Expression.Parameter(typeof(T5), "t5"),
                    Expression.Parameter(typeof(T6), "t6"),
                    Expression.Parameter(typeof(T7), "t7"),
                    Expression.Parameter(typeof(T8), "t8"),
                    Expression.Parameter(typeof(T9), "t9")
            };

                var ctor = script.ScriptableType.GetConstructors()[0];
                NewExpression ctorExpression = Expression.New(ctor, parameterExpressions);
                var expression = Expression.Lambda<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TScriptable>>(ctorExpression, parameterExpressions);
                var lambda = expression.Compile();
                ScriptCtors.Add(script.ScriptableType.Name, lambda);
            }

            var func = ScriptCtors[script.ScriptableType.Name] as Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TScriptable>;

            var scriptable = func(p1, p2, p3, p4, p5, p6, p7, p8, p9);
            scriptable.Script = script;
            return scriptable;
        }
        public static TScriptable CreateScriptable<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TScriptable>(Script script, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10) where TScriptable : ScriptComponent
        {
            if (script == null)
                return null;

            if (!ScriptCtors.ContainsKey(script.ScriptableType.Name))
            {
                List<ParameterExpression> parameterExpressions = new List<ParameterExpression>()
            {
                    Expression.Parameter(typeof(T1), "t1"),
                    Expression.Parameter(typeof(T2), "t2"),
                    Expression.Parameter(typeof(T3), "t3"),
                    Expression.Parameter(typeof(T4), "t4"),
                    Expression.Parameter(typeof(T5), "t5"),
                    Expression.Parameter(typeof(T6), "t6"),
                    Expression.Parameter(typeof(T7), "t7"),
                    Expression.Parameter(typeof(T8), "t8"),
                    Expression.Parameter(typeof(T9), "t9"),
                    Expression.Parameter(typeof(T10), "t10")
            };

                var ctor = script.ScriptableType.GetConstructors()[0];
                NewExpression ctorExpression = Expression.New(ctor, parameterExpressions);
                var expression = Expression.Lambda<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TScriptable>>(ctorExpression, parameterExpressions);
                var lambda = expression.Compile();
                ScriptCtors.Add(script.ScriptableType.Name, lambda);
            }

            var func = ScriptCtors[script.ScriptableType.Name] as Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TScriptable>;

            var scriptable = func(p1, p2, p3, p4, p5, p6, p7, p8, p9, p10);
            scriptable.Script = script;
            return scriptable;
        }
        public static TScriptable CreateScriptable<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TScriptable>(Script script, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10, T11 p11) where TScriptable : ScriptComponent
        {
            if (script == null)
                return null;

            if (!ScriptCtors.ContainsKey(script.ScriptableType.Name))
            {
                List<ParameterExpression> parameterExpressions = new List<ParameterExpression>()
            {
                    Expression.Parameter(typeof(T1), "t1"),
                    Expression.Parameter(typeof(T2), "t2"),
                    Expression.Parameter(typeof(T3), "t3"),
                    Expression.Parameter(typeof(T4), "t4"),
                    Expression.Parameter(typeof(T5), "t5"),
                    Expression.Parameter(typeof(T6), "t6"),
                    Expression.Parameter(typeof(T7), "t7"),
                    Expression.Parameter(typeof(T8), "t8"),
                    Expression.Parameter(typeof(T9), "t9"),
                    Expression.Parameter(typeof(T10), "t10"),
                    Expression.Parameter(typeof(T11), "t11")
            };

                var ctor = script.ScriptableType.GetConstructors()[0];
                NewExpression ctorExpression = Expression.New(ctor, parameterExpressions);
                var expression = Expression.Lambda<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TScriptable>>(ctorExpression, parameterExpressions);
                var lambda = expression.Compile();
                ScriptCtors.Add(script.ScriptableType.Name, lambda);
            }

            var func = ScriptCtors[script.ScriptableType.Name] as Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TScriptable>;

            var scriptable = func(p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11);
            scriptable.Script = script;
            return scriptable;
        }
        public static TScriptable CreateScriptable<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TScriptable>(Script script, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10, T11 p11, T12 p12) where TScriptable : ScriptComponent
        {
            if (script == null)
                return null;

            if (!ScriptCtors.ContainsKey(script.ScriptableType.Name))
            {
                List<ParameterExpression> parameterExpressions = new List<ParameterExpression>()
            {
                    Expression.Parameter(typeof(T1), "t1"),
                    Expression.Parameter(typeof(T2), "t2"),
                    Expression.Parameter(typeof(T3), "t3"),
                    Expression.Parameter(typeof(T4), "t4"),
                    Expression.Parameter(typeof(T5), "t5"),
                    Expression.Parameter(typeof(T6), "t6"),
                    Expression.Parameter(typeof(T7), "t7"),
                    Expression.Parameter(typeof(T8), "t8"),
                    Expression.Parameter(typeof(T9), "t9"),
                    Expression.Parameter(typeof(T10), "t10"),
                    Expression.Parameter(typeof(T11), "t11"),
                    Expression.Parameter(typeof(T12), "t12")
            };

                var ctor = script.ScriptableType.GetConstructors()[0];
                NewExpression ctorExpression = Expression.New(ctor, parameterExpressions);
                var expression = Expression.Lambda<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TScriptable>>(ctorExpression, parameterExpressions);
                var lambda = expression.Compile();
                ScriptCtors.Add(script.ScriptableType.Name, lambda);
            }

            var func = ScriptCtors[script.ScriptableType.Name] as Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TScriptable>;

            var scriptable = func(p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12);
            scriptable.Script = script;
            return scriptable;
        }
        public static TScriptable CreateScriptable<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TScriptable>(Script script, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10, T11 p11, T12 p12, T13 p13) where TScriptable : ScriptComponent
        {
            if (script == null)
                return null;

            if (!ScriptCtors.ContainsKey(script.ScriptableType.Name))
            {
                List<ParameterExpression> parameterExpressions = new List<ParameterExpression>()
            {
                    Expression.Parameter(typeof(T1), "t1"),
                    Expression.Parameter(typeof(T2), "t2"),
                    Expression.Parameter(typeof(T3), "t3"),
                    Expression.Parameter(typeof(T4), "t4"),
                    Expression.Parameter(typeof(T5), "t5"),
                    Expression.Parameter(typeof(T6), "t6"),
                    Expression.Parameter(typeof(T7), "t7"),
                    Expression.Parameter(typeof(T8), "t8"),
                    Expression.Parameter(typeof(T9), "t9"),
                    Expression.Parameter(typeof(T10), "t10"),
                    Expression.Parameter(typeof(T11), "t11"),
                    Expression.Parameter(typeof(T12), "t12"),
                    Expression.Parameter(typeof(T13), "t13")
            };

                var ctor = script.ScriptableType.GetConstructors()[0];
                NewExpression ctorExpression = Expression.New(ctor, parameterExpressions);
                var expression = Expression.Lambda<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TScriptable>>(ctorExpression, parameterExpressions);
                var lambda = expression.Compile();
                ScriptCtors.Add(script.ScriptableType.Name, lambda);
            }

            var func = ScriptCtors[script.ScriptableType.Name] as Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TScriptable>;

            var scriptable = func(p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13);
            scriptable.Script = script;
            return scriptable;
        }
        public static TScriptable CreateScriptable<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TScriptable>(Script script, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10, T11 p11, T12 p12, T13 p13, T14 p14) where TScriptable : ScriptComponent
        {
            if (script == null)
                return null;

            if (!ScriptCtors.ContainsKey(script.ScriptableType.Name))
            {
                List<ParameterExpression> parameterExpressions = new List<ParameterExpression>()
            {
                    Expression.Parameter(typeof(T1), "t1"),
                    Expression.Parameter(typeof(T2), "t2"),
                    Expression.Parameter(typeof(T3), "t3"),
                    Expression.Parameter(typeof(T4), "t4"),
                    Expression.Parameter(typeof(T5), "t5"),
                    Expression.Parameter(typeof(T6), "t6"),
                    Expression.Parameter(typeof(T7), "t7"),
                    Expression.Parameter(typeof(T8), "t8"),
                    Expression.Parameter(typeof(T9), "t9"),
                    Expression.Parameter(typeof(T10), "t10"),
                    Expression.Parameter(typeof(T11), "t11"),
                    Expression.Parameter(typeof(T12), "t12"),
                    Expression.Parameter(typeof(T13), "t13"),
                    Expression.Parameter(typeof(T14), "t14")
            };

                var ctor = script.ScriptableType.GetConstructors()[0];
                NewExpression ctorExpression = Expression.New(ctor, parameterExpressions);
                var expression = Expression.Lambda<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TScriptable>>(ctorExpression, parameterExpressions);
                var lambda = expression.Compile();
                ScriptCtors.Add(script.ScriptableType.Name, lambda);
            }

            var func = ScriptCtors[script.ScriptableType.Name] as Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TScriptable>;

            var scriptable = func(p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13, p14);
            scriptable.Script = script;
            return scriptable;
        }
        public static TScriptable CreateScriptable<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TScriptable>(Script script, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10, T11 p11, T12 p12, T13 p13, T14 p14, T15 p15) where TScriptable : ScriptComponent
        {
            if (script == null)
                return null;

            if (!ScriptCtors.ContainsKey(script.ScriptableType.Name))
            {
                List<ParameterExpression> parameterExpressions = new List<ParameterExpression>()
            {
                    Expression.Parameter(typeof(T1), "t1"),
                    Expression.Parameter(typeof(T2), "t2"),
                    Expression.Parameter(typeof(T3), "t3"),
                    Expression.Parameter(typeof(T4), "t4"),
                    Expression.Parameter(typeof(T5), "t5"),
                    Expression.Parameter(typeof(T6), "t6"),
                    Expression.Parameter(typeof(T7), "t7"),
                    Expression.Parameter(typeof(T8), "t8"),
                    Expression.Parameter(typeof(T9), "t9"),
                    Expression.Parameter(typeof(T10), "t10"),
                    Expression.Parameter(typeof(T11), "t11"),
                    Expression.Parameter(typeof(T12), "t12"),
                    Expression.Parameter(typeof(T13), "t13"),
                    Expression.Parameter(typeof(T14), "t14"),
                    Expression.Parameter(typeof(T15), "t15")
            };

                var ctor = script.ScriptableType.GetConstructors()[0];
                NewExpression ctorExpression = Expression.New(ctor, parameterExpressions);
                var expression = Expression.Lambda<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TScriptable>>(ctorExpression, parameterExpressions);
                var lambda = expression.Compile();
                ScriptCtors.Add(script.ScriptableType.Name, lambda);
            }

            var func = ScriptCtors[script.ScriptableType.Name] as Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TScriptable>;

            var scriptable = func(p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13, p14, p15);
            scriptable.Script = script;
            return scriptable;
        }



        private static Dictionary<string, object> ScriptCtors = new Dictionary<string, object>();
        #endregion


    }
}
