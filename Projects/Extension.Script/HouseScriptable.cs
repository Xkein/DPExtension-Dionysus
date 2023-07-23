using Extension.Ext;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extension.Script
{
    public interface IHouseScriptable : IObjectScriptable
    {
    }


    [Serializable]
    public abstract class HouseScriptable : Scriptable<HouseExt>, IHouseScriptable
    {
        public HouseScriptable(HouseExt owner) : base(owner)
        {
        }
        
        [Obsolete("not support OnPut in HouseScriptable yet", true)]
        public void OnPut(CoordStruct coord, Direction faceDir)
        {
            throw new NotSupportedException("not support OnPut in HouseScriptable yet");
        }
        [Obsolete("not support OnRemove in HouseScriptable yet", true)]
        public void OnRemove()
        {
            throw new NotSupportedException("not support OnRemove in HouseScriptable yet");
        }
        [Obsolete("not support OnReceiveDamage in HouseScriptable yet", true)]
        public void OnReceiveDamage(Pointer<int> pDamage, int DistanceFromEpicenter, Pointer<WarheadTypeClass> pWH,
            Pointer<ObjectClass> pAttacker, bool IgnoreDefenses, bool PreventPassengerEscape, Pointer<HouseClass> pAttackingHouse)
        {
            throw new NotSupportedException("not support OnReceiveDamage in HouseScriptable yet");
        }

    }
}
