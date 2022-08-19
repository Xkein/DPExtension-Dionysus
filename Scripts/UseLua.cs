using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using DynamicPatcher;
using Extension.Ext;
using Extension.INI;
using Extension.Lua;
using Extension.Script;
using PatcherYRpp;
using PatcherYRpp.Utilities;

namespace Scripts
{
    public class UseLuaConfig : INIConfig
    {
        public override void Read(INIComponent ini)
        {
            string luaScriptFile = ini.Get<string>("UseLua.File");
            string luaScriptString = ini.Get<string>("UseLua.String");

            string script = luaScriptString;
            if (string.IsNullOrEmpty(script))
            {
                if (string.IsNullOrEmpty(luaScriptFile))
                {
                    Logger.LogError("[{0}] has empty lua script file path!", ini.INISection);
                }
                else
                {
                    string pakName = luaScriptFile.Replace(".lua", "").Replace('/', '.').Replace('\\', '.');
                    script = $"require '{pakName}'";
                }
            }

            Script = script;
        }

        public string Script;
    }

    [Serializable]
    public class UseLua : ScriptComponent, ITechnoScriptable, IBulletScriptable, ISuperWeaponScriptable, IAnimScriptable
    {
        public UseLua() : base()
        {
            throw new NotSupportedException("UseLua prohibited due to value copy problem with struct. It will be fix in future.");
        }

        public UseLua(IExtension owner) : this()
        {
            Owner = owner;
        }

        private IExtension Owner;
        INIComponentWith<UseLuaConfig> INI;

        public override void Awake()
        {
            string section = DebugUtilities.GetAbstractID(Owner.OwnerObject);
            INI = this.CreateRulesIniComponentWith<UseLuaConfig>(section);

            Lua = new LuaComponent(INI.Data.Script) { Owner = Owner };
            Lua.AttachToComponent(this);
        }

        private LuaComponent Lua;

        public void OnPut(CoordStruct coord, Direction faceDir) => Lua.Call("OnPut", coord, faceDir);
        public void OnRemove() => Lua.Call("OnRemove");
        public void OnReceiveDamage(Pointer<int> pDamage, int DistanceFromEpicenter, Pointer<WarheadTypeClass> pWH,
            Pointer<ObjectClass> pAttacker, bool IgnoreDefenses,
            bool PreventPassengerEscape, Pointer<HouseClass> pAttackingHouse)
        {
            Lua.Call("OnReceiveDamage", pDamage, DistanceFromEpicenter, pWH, pAttacker, IgnoreDefenses, PreventPassengerEscape, pAttackingHouse);
        }
        public void OnDetonate(Pointer<CoordStruct> pCoords) => Lua.Call("OnDetonate", pCoords);
        public void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex) => Lua.Call("OnFire", pTarget, weaponIndex);
        public void OnLaunch(CellStruct cell, bool isPlayer) => Lua.Call("OnFire", cell, isPlayer);

        static UseLua()
        {
            Logger.LogWarning("Detected that you are using lua script.");
            Logger.LogWarning("Lua is working with low performance now but it will be improved in future versions.");
        }
    }
}
