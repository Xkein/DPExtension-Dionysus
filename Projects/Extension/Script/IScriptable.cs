using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Extension.Utilities;
using PatcherYRpp;

namespace Extension.Script
{
    public interface IScriptable : IReloadable
    {
    }

    public interface IAbstractScriptable : IScriptable
    {
        public void OnUpdate();
    }

    public interface IObjectScriptable : IAbstractScriptable
    {
        public void OnPut(CoordStruct coord, Direction faceDir);
        public void OnRemove();
        public void OnReceiveDamage(Pointer<int> pDamage, int DistanceFromEpicenter, Pointer<WarheadTypeClass> pWH,
            Pointer<ObjectClass> pAttacker, bool IgnoreDefenses, bool PreventPassengerEscape, Pointer<HouseClass> pAttackingHouse);
    }

    public interface ITechnoScriptable : IObjectScriptable
    {
        public void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex);
    }

    public interface IBulletScriptable : IObjectScriptable
    {
        public void OnDetonate(Pointer<CoordStruct> pCoords);
    }

    public interface ISuperWeaponScriptable : IAbstractScriptable
    {
        void OnLaunch(CellStruct cell, bool isPlayer);

    }

    public interface IAnimScriptable : IAbstractScriptable
    {

    }
}
