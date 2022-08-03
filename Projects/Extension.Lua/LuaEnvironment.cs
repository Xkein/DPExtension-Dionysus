using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XLua;

namespace Extension.Lua
{
    public class LuaEnvironment
    {
        private static LuaEnv s_LuaEnv;

        void Setup()
        {

            LuaEnv luaenv = new LuaEnv();

            s_LuaEnv = luaenv;
        }

        void Shutdown()
        {
            s_LuaEnv.Dispose();
            s_LuaEnv = null;
        }

    }
}
