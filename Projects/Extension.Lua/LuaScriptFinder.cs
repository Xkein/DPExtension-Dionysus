using NLua;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extension.lua
{
    public static class LuaScriptFinder
    {
        public const string LUA_DIRECTORY_NAME = "lua";
        public static string LuaSearchDirectory = Path.Combine(Directory.GetCurrentDirectory(), LUA_DIRECTORY_NAME);

        public static Lua Find(string luaScriptName)
        {

            return null;
        }

    }
}
