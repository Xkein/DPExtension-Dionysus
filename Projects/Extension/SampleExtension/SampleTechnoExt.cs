using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DynamicPatcher;
using Extension.Utilities;
using PatcherYRpp;

#if DEBUG
namespace Extension.Ext
{
    public partial class TechnoExt
    {
        [UpdateAction]
        public void LogOnUpdate() 
        {
            
        }

        [FireAction]
        public void SampleOnFire(Pointer<AbstractClass> pTarget,int weaponIndex)
        {
            
        }

        [RemoveAction]
        public void SampleOnRemove()
        {
            
        }

        [PutAction]
        public void SampleOnPut(CoordStruct coord,Direction faceDir)
        {
            
        }

        [ReceiveDamageAction]
        public void SampleOnDamge(Pointer<int> pDamage, int DistanceFromEpicenter, Pointer<WarheadTypeClass> pWH,
            Pointer<ObjectClass> pAttacker, bool IgnoreDefenses, bool PreventPassengerEscape, Pointer<HouseClass> pAttackingHouse)
        {
           
        }
    }
}
#endif
