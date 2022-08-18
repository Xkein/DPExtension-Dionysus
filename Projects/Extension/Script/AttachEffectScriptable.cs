using Extension.Ext;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extension.Script
{
    [Serializable]
    public class AttachEffectScriptable : TechnoScriptable
    {
        public int Duration { get; set; }

        public string ScriptName { get; private set; }

        public AttachEffectScriptable(TechnoExt owner) : base(owner)
        {
            ScriptName = this.GetType().Name;
        }

        /// <summary>
        /// 当AE效果被附加（从无到有）被触发
        /// </summary>
        public virtual void OnAttachEffectPut(Pointer<int> pDamage, Pointer<WarheadTypeClass> pWH,
         Pointer<ObjectClass> pAttacker, Pointer<HouseClass> pAttackingHouse)
        {

        }

        /// <summary>
        /// 当AE效果将要被移除前触发的效果
        /// </summary>
        public virtual void OnAttachEffectRemove()
        {

        }

        /// <summary>
        /// 当已经处于AE效果中，并且接收到新的同类AE时，可以在这里维护持续时间比如接收到一个新的AE时刷新/叠加/减少持续时间，默认为重置持续时间
        /// </summary>
        /// <param name="duration"></param>
        public virtual void OnAttachEffectRecieveNew(int duration, Pointer<int> pDamage, Pointer<WarheadTypeClass> pWH,
         Pointer<ObjectClass> pAttacker, Pointer<HouseClass> pAttackingHouse)
        {
            Duration = duration;
        }

    }
}
