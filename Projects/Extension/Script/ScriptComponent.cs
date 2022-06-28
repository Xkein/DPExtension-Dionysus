using Extension.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extension.Script
{
    [Serializable]
    public abstract class ScriptComponent : Component
    {
        protected ScriptComponent()
        {

        }

        protected ScriptComponent(Script script)
        {
            Script = script;
        }

        public Script Script { get; internal set; }
    }

    public static class ScriptComponentHelpers
    {
        public static void CreateScriptComponent(this Component component, string scriptName, int id, string description, params object[] parameters)
        {
            var script = ScriptManager.GetScript(scriptName);
            var scriptComponent = ScriptManager.CreateScriptableTo(component, script, parameters);
            scriptComponent.ID = id;
            scriptComponent.Name = description;
        }
        public static void CreateScriptComponent(this Component component, string scriptName, string description, params object[] parameters)
        {
            component.CreateScriptComponent(scriptName, Component.NO_ID, description, parameters);
        }

        public static void CreateScriptComponent<T>(this Component component, int id, string description, params object[] parameters) where T : ScriptComponent
        {
            component.CreateScriptComponent(typeof(T).Name, id, description, parameters);
        }

        public static void CreateScriptComponent<T>(this Component component, string description, params object[] parameters) where T : ScriptComponent
        {
            component.CreateScriptComponent<T>(Component.NO_ID, description, parameters);
        }
    }
}
