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
        public ScriptComponent()
        {

        }

        public ScriptComponent(Script script)
        {
            Script = script;
        }

        public Script Script { get; internal set; }
    }

    public static class ScriptComponentHelpers
    {
        public static void CreateScriptComponent(this Component component, string scriptName, int id, string description, object[] parameters)
        {
            var script = ScriptManager.GetScript(scriptName);
            var scriptComponent = ScriptManager.CreateScriptable<ScriptComponent>(script, parameters);
            scriptComponent.ID = id;
            scriptComponent.Name = description;

            scriptComponent.AttachToComponent(component);
        }
        public static void CreateScriptComponent(this Component component, string scriptName, string description, object[] parameters)
        {
            component.CreateScriptComponent(scriptName, Component.NO_ID, description, parameters);
        }
    }
}
