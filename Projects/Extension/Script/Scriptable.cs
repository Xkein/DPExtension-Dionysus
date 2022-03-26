using Extension.Ext;
using Extension.Utilities;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;

namespace Extension.Script
{
    public interface IAbstractScriptable
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

    public interface IScriptable : IReloadable
    {
    }

    [Serializable]
    public abstract class Scriptable<T> : ScriptComponent, IScriptable
    {
        public T Owner { get; protected set; }
        public Scriptable(T owner) : base()
        {
            Owner = owner;
        }
    }

    [Serializable]
    public class TechnoScriptable : Scriptable<TechnoExt>, ITechnoScriptable
    {
        public TechnoScriptable(TechnoExt owner) : base(owner)
        {
        }

        public virtual void OnPut(CoordStruct coord, Direction faceDir) { }
        public virtual void OnRemove() { }
        public virtual void OnReceiveDamage(Pointer<int> pDamage, int DistanceFromEpicenter, Pointer<WarheadTypeClass> pWH,
            Pointer<ObjectClass> pAttacker, bool IgnoreDefenses, bool PreventPassengerEscape, Pointer<HouseClass> pAttackingHouse)
        { }

        public virtual void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex) { }
    }

    [Serializable]
    public class BulletScriptable : Scriptable<BulletExt>, IBulletScriptable
    {
        public BulletScriptable(BulletExt owner) : base(owner)
        {
        }

        public virtual void OnPut(CoordStruct coord, Direction faceDir) { }
        public virtual void OnRemove() { }
        public virtual void OnReceiveDamage(Pointer<int> pDamage, int DistanceFromEpicenter, Pointer<WarheadTypeClass> pWH,
            Pointer<ObjectClass> pAttacker, bool IgnoreDefenses, bool PreventPassengerEscape, Pointer<HouseClass> pAttackingHouse)
        { }

        public virtual void OnDetonate(Pointer<CoordStruct> pCoords) { }
    }
}
