using DynamicPatcher;
using Extension.INI;
using Extension.Script;
using Extension.Utilities;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extension.Ext
{
    public partial class TechnoExt
    {
        private List<AttachEffectScriptable> _attachEffectScriptables = new List<AttachEffectScriptable>();

        [PutAction]
        public void TechnoClass_AttachEffectScript_Put(CoordStruct coordStruct, Direction faceDir)
        {
            foreach (var ae in _attachEffectScriptables)
            {
                ae.OnPut(coordStruct, faceDir);
            }
        }

        [UpdateAction]
        public void TechnoClass_AttachEffectScript_Update()
        {
            foreach (var ae in _attachEffectScriptables)
            {
                if (ae.Duration > 0)
                {
                    ae.Duration--;
                    ae.OnUpdate();
                }
            }
            ClearExpiredAttachEffect();
        }

        [RemoveAction]
        public void TechnoClass_AttachEffectScript_Remove()
        {
            foreach (var ae in _attachEffectScriptables)
            {
                ae.OnRemove();
            }
        }

        [FireAction]
        public void TechnoClass_AttachEffectScript_Fire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            foreach (var ae in _attachEffectScriptables)
            {
                ae.OnFire(pTarget,weaponIndex);
            }
        }

        [ReceiveDamageAction]
        public void TechnoClass_AttachEffectScript_ReceiveDamage(Pointer<int> pDamage, int DistanceFromEpicenter, Pointer<WarheadTypeClass> pWH,
         Pointer<ObjectClass> pAttacker, bool IgnoreDefenses, bool PreventPassengerEscape, Pointer<HouseClass> pAttackingHouse)
        {
            foreach (var ae in _attachEffectScriptables)
            {
                ae.OnReceiveDamage(pDamage, DistanceFromEpicenter, pWH, pAttacker, IgnoreDefenses, PreventPassengerEscape, pAttackingHouse);
            }

            if (pAttackingHouse.IsNull)
                return;


            var warheadTypeExt = WarheadTypeExt.ExtMap.Find(pWH);

            if (warheadTypeExt != null)
            {
                if (pAttackingHouse.Ref.IsAlliedWith(OwnerObject.Ref.Owner))
                {
                    if (!pWH.Ref.AffectsAllies)
                        return;
                }
                else
                {
                    if (!warheadTypeExt.AffectsEnemies)
                        return;
                }


                if (MapClass.GetTotalDamage(10000, pWH, OwnerObject.Ref.Type.Ref.Base.Armor, DistanceFromEpicenter) > 0 || warheadTypeExt.AllowZeroDamage)
                {

                    if (!string.IsNullOrEmpty(warheadTypeExt.AttachEffectScript))
                    {
                        var currentScript = _attachEffectScriptables.Where(s => s.ScriptName == warheadTypeExt.AttachEffectScript).FirstOrDefault();

                        if (currentScript != null && !warheadTypeExt.AttachEffectCumulative)
                        {
                            currentScript.OnAttachEffectRecieveNew(warheadTypeExt.AttachEffectDuration, pDamage, pWH, pAttacker, pAttackingHouse);
                        }
                        else
                        {
                            if (warheadTypeExt.AttachEffectDuration > 0)
                            {
                                var script = ScriptManager.GetScript(warheadTypeExt.AttachEffectScript);
                                currentScript = ScriptManager.CreateScriptableTo(GameObject, script, this) as AttachEffectScriptable;
                                currentScript.Duration = warheadTypeExt.AttachEffectDuration;
                                currentScript.OnAttachEffectPut(pDamage, pWH, pAttacker, pAttackingHouse);
                                _attachEffectScriptables.Add(currentScript);
                            }
                        }
                    }

                }

            }

        }

        /// <summary>
        /// 检查并清理过期AE
        /// </summary>
        private void ClearExpiredAttachEffect()
        {
            if (_attachEffectScriptables.Any())
            {
                for (var i = _attachEffectScriptables.Count() - 1; i >= 0; i--)
                {
                    var ae = _attachEffectScriptables[i];
                    if (ae.Duration <= 0)
                    {
                        ae.OnAttachEffectRemove();
                        _attachEffectScriptables.Remove(ae);
                    }
                }
            }
        }

    }

    public partial class WarheadTypeExt
    {
        public string AttachEffectScript;

        public int AttachEffectDuration;

        public bool AttachEffectCumulative = false;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="section"></param>
        [INILoadAction]
        public void ReadAttachEffectScript(Pointer<CCINIClass> pINI)
        {
            INIReader reader = new INIReader(pINI);
            string section = OwnerObject.Ref.Base.ID;
            if (string.IsNullOrEmpty(AttachEffectScript))
            {
                reader.Read(section, "AttachEffect.Scripts", ref AttachEffectScript);
                reader.Read(section, "AttachEffect.Scripts.Duration", ref AttachEffectDuration);
                reader.Read(section, "AttachEffect.Scripts.Cumulative", ref AttachEffectCumulative);
            }
        }
    }



}
