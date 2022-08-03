using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using DynamicPatcher;
using Extension.Components;
using Extension.Ext;
using Extension.Lua;
using Extension.Script;
using PatcherYRpp;
using PatcherYRpp.Utilities;

namespace Scripts
{
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
        INIComponent INI;
        private string LuaScriptFile => INI.Get<string>("UseLua.File");
        private string LuaScriptString => INI.Get<string>("UseLua.String");

        public override void Awake()
        {
            string section = DebugUtilities.GetAbstractID(Owner.OwnerObject);
            INI = INIComponent.CreateRulesIniComponent(section);
            INI.AttachToComponent(this);

            string script = LuaScriptString;
            if (string.IsNullOrEmpty(script))
            {
                if (string.IsNullOrEmpty(LuaScriptFile))
                {
                    Logger.LogError("[{0}] has empty lua script file path!", section);
                }
                else
                {
                    string pakName = LuaScriptFile.Replace(".lua", "").Replace('/', '.').Replace('\\', '.');
                    script = $"require '{pakName}'";
                }
            }

            Lua = new LuaComponent(script);
            Lua.Owner = Owner;
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
