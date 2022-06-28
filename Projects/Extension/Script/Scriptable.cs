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
    [Serializable]
    public abstract class Scriptable<T> : ScriptComponent, IScriptable
    {
        public T Owner { get; protected set; }

        protected Scriptable(T owner) : base()
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

        [Obsolete("not support OnPut in BulletScriptable yet", true)]
        public void OnPut(CoordStruct coord, Direction faceDir)
        {
            throw new NotSupportedException("not support OnPut in BulletScriptable yet");
        }
        [Obsolete("not support OnRemove in BulletScriptable yet", true)]
        public void OnRemove()
        {
            throw new NotSupportedException("not support OnRemove in BulletScriptable yet");
        }
        [Obsolete("not support OnReceiveDamage in BulletScriptable yet", true)]
        public void OnReceiveDamage(Pointer<int> pDamage, int DistanceFromEpicenter, Pointer<WarheadTypeClass> pWH,
            Pointer<ObjectClass> pAttacker, bool IgnoreDefenses, bool PreventPassengerEscape, Pointer<HouseClass> pAttackingHouse)
        {
            throw new NotSupportedException("not support OnReceiveDamage in BulletScriptable yet");
        }

        public virtual void OnDetonate(Pointer<CoordStruct> pCoords) { }
    }

    [Serializable]
    public class SuperWeaponScriptable : Scriptable<SuperWeaponExt>, ISuperWeaponScriptable
    {
        public SuperWeaponScriptable(SuperWeaponExt owner) : base(owner)
        {
        }

        public sealed override void OnUpdate()
        {
            throw new NotSupportedException("not support OnUpdate in SuperWeaponScriptable yet");
        }
        public sealed override void OnLateUpdate()
        {
            throw new NotSupportedException("not support OnLateUpdate in SuperWeaponScriptable yet");
        }
        public sealed override void OnRender()
        {
            throw new NotSupportedException("not support OnRender in SuperWeaponScriptable yet");
        }

        public virtual void OnLaunch(CellStruct cell, bool isPlayer) { }
    }

#if USE_ANIM_EXT
    [Serializable]
    public class AnimScriptable : Scriptable<AnimExt>, IAnimScriptable
    {
        public AnimScriptable(AnimExt owner) : base(owner)
        {
        }
        
        [Obsolete("not support OnPut in AnimScriptable yet", true)]
        public void OnPut(CoordStruct coord, Direction faceDir)
        {
            throw new NotSupportedException("not support OnPut in AnimScriptable yet");
        }
        public virtual void OnRemove() { }
        [Obsolete("not support OnReceiveDamage in AnimScriptable yet", true)]
        public void OnReceiveDamage(Pointer<int> pDamage, int DistanceFromEpicenter, Pointer<WarheadTypeClass> pWH,
            Pointer<ObjectClass> pAttacker, bool IgnoreDefenses, bool PreventPassengerEscape, Pointer<HouseClass> pAttackingHouse)
        {
            throw new NotSupportedException("not support OnReceiveDamage in AnimScriptable yet");
        }
    }
#endif
}
