using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using Extension.Components;
using Extension.Ext;
using XLua;

namespace Extension.Lua
{
    [CSharpCallLua]
    public class LuaComponent : Component
    {
        public LuaComponent(string script, bool useSharedEnv = false)
        {
            m_Script = script;
            m_useSharedEnv = useSharedEnv;
        }

        public IExtension Owner;

        [NonSerialized]
        private LuaEnv m_LuaEnv;
        private string m_Script;
        private bool m_useSharedEnv;


        public override void OnDestroy()
        {
            Call("OnDestroy");

            m_LuaEnv?.Dispose();
        }

        // low performance
        public void Call(string name, params object[] args)
        {
            var function = m_LuaEnv.Global.Get<LuaFunction>(name);
            if (function != null)
            {
                m_LuaEnv.Global.Set("Owner", Owner);
                m_LuaEnv.Global.Set("this", this);

                function.Call(args);
            }
        }

        public override void Awake()
        {
            Setup();

            Call("Awake");
        }
        public override void OnUpdate()
        {
            Call("OnUpdate");
            m_LuaEnv.Tick();
        }

        public override void Start() => Call("Start");
        public override void OnLateUpdate() => Call("OnLateUpdate");
        public override void OnRender() => Call("OnRender");
        public override void SaveToStream(IStream stream) => Call("SaveToStream", stream);
        public override void LoadFromStream(IStream stream)
        {
            Setup();

            Call("LoadFromStream", stream);
        }


        private void Setup()
        {
            m_LuaEnv = new LuaEnv();

            m_LuaEnv.DoString(m_Script);
        }
    }
}
