
using System;
using System.Collections;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;
using Extension.Script;
using System.Threading.Tasks;
using Extension.Components;
using Extension.Coroutines;
using Extension.Decorators;
using Extension.Utilities;
using PatcherYRpp.Utilities;
using Extension.INI;

namespace Scripts
{
    [Serializable]
    public class SuperWeaponKeeper : TechnoScriptable
    {
        public SuperWeaponKeeper(TechnoExt owner) : base(owner) {}

        public override void Awake()
        {
            INI = INIComponent.CreateRulesIniComponent(Owner.OwnerTypeRef.BaseAbstractType.ID);
        }

        INIComponent INI;

        private bool ShowStorage => INI.Get("SuperWeaponKeeper.ShowStorage", true);
        private int[] TimesToKeep => INI.GetList<int>("SuperWeaponKeeper.TimesToKeep");
        Pointer<SuperWeaponTypeClass>[] SuperWeaponTypesToKeep => INI.GetList<Pointer<SuperWeaponTypeClass>>("SuperWeaponKeeper.ToKeep");
        Pointer<SuperClass>[] SuperWeaponToKeep => SuperWeaponTypesToKeep.Select(type => Owner.OwnerObject.Ref.Owner.Ref.FindSuperWeapon(type)).ToArray();

        private int[] storages;

        public override void Start()
        {
            if (TimesToKeep == null || SuperWeaponTypesToKeep == null)
                return;

            if (TimesToKeep.Length != SuperWeaponToKeep.Length)
            {
                string section = DebugUtilities.GetAbstractID(Owner.OwnerObject.Convert<AbstractClass>());
                Logger.LogError("[{0}] has wrong SuperWeaponKeeper configuration!", section);
            }
            
            storages = new int[SuperWeaponTypesToKeep.Length];

            for (int idx = 0; idx < SuperWeaponTypesToKeep.Length; idx++)
            {
                GameObject.StartCoroutine(Keeper(idx));
            }
        }

        const string LevelTag = "Keeper::Level";
        const string MaxLevelTag = "Keeper::MaxLevel";
        const string LaunchedTag = "Keeper::Launched";
        private const string BaseChargedTag = "Keeper::BaseCharged";

        [Serializable]
        class SWLaunchListener : SuperWeaponScriptable
        {
            public SWLaunchListener(SuperWeaponExt owner) : base(owner)
            {
                AltUIName = owner.OwnerObject.Ref.Type.Ref.Base.UIName;
            }

            public override void OnLaunch(CellStruct cell, bool isPlayer)
            {
                Pointer<SuperClass> pSuper = Owner.OwnerObject;
                DecoratorComponent decorator = Owner.DecoratorComponent;

                if (pSuper.Ref.IsCharged)
                {
                    if (decorator.GetValue<int>(LevelTag) > 0)
                    {
                        decorator.SetValue(LaunchedTag, true);
                    }
                    else
                    {
                        decorator.SetValue(BaseChargedTag, false);
                        pSuper.Ref.UIName = pSuper.Ref.Type.Ref.Base.UIName;
                    }
                }
            }

            public override void LoadFromStream(IStream stream)
            {
                Pointer<SuperClass> pSuper = Owner.OwnerObject;

                pSuper.Ref.UIName = AltUIName;
            }

            public UniString AltUIName;
        }
        
        IEnumerator Keeper(int idx)
        {
            if (TimesToKeep[idx] <= 0)
                yield break;

            // wait super weapon to reset
            yield return new WaitForFrames(2);
            
            SuperWeaponExt ext = SuperWeaponExt.ExtMap.Find(SuperWeaponToKeep[idx]);
            DecoratorComponent decorator = ext.DecoratorComponent;

            if (decorator.GetValue(LevelTag) == null)
            {
                decorator.CreateDecorator<PairDecorator<string, int>>("stored level from keepers", LevelTag, 0);
                decorator.CreateDecorator<PairDecorator<string, int>>("max stored level from keepers", MaxLevelTag, 0);
                decorator.CreateDecorator<PairDecorator<string, bool>>("super weapon is launched", LaunchedTag, false);
                decorator.CreateDecorator<PairDecorator<string, bool>>("super weapon is charged at least once", BaseChargedTag, false);

                ext.GameObject.CreateScriptComponent(nameof(SWLaunchListener), "super weapon launch listener for keepers", ext);
            }

            {
                int maxLevel = decorator.GetValue<int>(MaxLevelTag);
                maxLevel += TimesToKeep[idx];
                decorator.SetValue(MaxLevelTag, maxLevel);
            }

            int level = 0;
            TimerStruct oldTimer = new TimerStruct();

            void _SetStorageMessage()
            {
                SetStorageMessage(ext.OwnerObject, decorator.GetValue<int>(LevelTag));
            }

            if (ext.OwnerRef.RechargeTimer.Completed() && ext.OwnerRef.Granted)
            {
                decorator.SetValue(BaseChargedTag, true);
                _SetStorageMessage();

                // start next level
                ext.OwnerRef.SetCharge(0);
                ext.OwnerRef.SetReadiness(true);
            }

            while (true)
            {
                if (!ext.OwnerRef.Granted)
                {
                    decorator.SetValue(BaseChargedTag, false);
                    //Console.WriteLine("super weapon ungranted, waiting...");
                    //yield return new WaitUntil(() => ext.OwnerRef.Granted);
                    // use this ugly code due to serialize problem
                    while (!ext.OwnerRef.Granted)
                        yield return null;
                    if (decorator.GetValue<int>(LevelTag) > 0)
                    {
                        ext.OwnerRef.SetReadiness(true);
                    }
                    _SetStorageMessage();
                }

                int totalLevel = decorator.GetValue<int>(LevelTag);
                if (ext.OwnerRef.RechargeTimer.Completed())
                {
                    bool baseCharged = decorator.GetValue<bool>(BaseChargedTag);

                    if (!baseCharged)
                    {
                        decorator.SetValue(BaseChargedTag, true);
                        _SetStorageMessage();

                        if (totalLevel < decorator.GetValue<int>(MaxLevelTag))
                        {
                            // start next level
                            ext.OwnerRef.SetCharge(0);
                            ext.OwnerRef.SetReadiness(true);
                        }
                    }

                    if (level < TimesToKeep[idx])
                    {
                        if (baseCharged)
                        {
                            level++;
                            decorator.SetValue(LevelTag, ++totalLevel);
                        }

                        if (totalLevel < decorator.GetValue<int>(MaxLevelTag))
                        {
                            // start next level
                            ext.OwnerRef.SetCharge(0);
                            ext.OwnerRef.SetReadiness(true);
                        }

                        //Console.WriteLine("super weapon charged, current level: {0}", level);
                        _SetStorageMessage();
                    }
                }
                else if (level > 0 && decorator.GetValue<bool>(LaunchedTag))
                {
                    level--;
                    decorator.SetValue(LevelTag, --totalLevel);
                    decorator.SetValue(LaunchedTag, false);

                    //Console.WriteLine("super weapon fired, remaining level: {0}", level);
                    _SetStorageMessage();

                    if (decorator.GetValue<bool>(BaseChargedTag) || totalLevel > 0)
                    {
                        // continue the launch of current super weapon
                        ext.OwnerRef.SetReadiness(true);
                        Game.CurrentSWType = ext.OwnerRef.Type.Ref.ArrayIndex;
                    }

                    // do not break old timer
                    if (oldTimer.InProgress())
                    {
                        ext.OwnerRef.RechargeTimer = oldTimer;
                    }
                }
                else if (!ext.OwnerRef.IsCharged)
                {
                    //Console.WriteLine("super weapon uncharged, waiting...");
                    //yield return new WaitUntil(() => ext.OwnerRef.IsCharged);
                    // use this ugly code due to serialize problem
                    while (!ext.OwnerRef.IsCharged)
                        yield return null;
                }
                
                storages[idx] = level;
                oldTimer = ext.OwnerRef.RechargeTimer;
                yield return null;
            }
        }

        public override void OnRemove()
        {
            if (Owner.OwnerObject.Ref.Base.InLimbo)
            {
                return;
            }

            var pSWList = SuperWeaponToKeep;
            for (int idx = 0; idx < pSWList.Length; idx++)
            {
                Pointer<SuperClass> pSuper = pSWList[idx];
                SuperWeaponExt ext = SuperWeaponExt.ExtMap.Find(pSuper);
                DecoratorComponent decorator = ext.DecoratorComponent;

                int totalLevel = decorator.GetValue<int>(LevelTag);
                totalLevel -= storages[idx];
                decorator.SetValue(LevelTag, totalLevel);

                int maxLevel = decorator.GetValue<int>(MaxLevelTag);
                maxLevel -= TimesToKeep[idx];
                decorator.SetValue(MaxLevelTag, maxLevel);

                if (totalLevel >= maxLevel)
                {
                    pSuper.Ref.SetCharge(100);
                }

                SetStorageMessage(pSuper, totalLevel);
            }

        }

        void SetStorageMessage(Pointer<SuperClass> pSuper, int storage)
        {
            if (!ShowStorage)
                return;

            SuperWeaponExt ext = SuperWeaponExt.ExtMap.Find(pSuper);
            ref UniString altUIName = ref ext.GameObject.GetComponent<SWLaunchListener>().AltUIName;

            bool baseCharged = ext.DecoratorComponent.GetValue<bool>(BaseChargedTag);
            int maxLevel = ext.DecoratorComponent.GetValue<int>(MaxLevelTag);

            if (baseCharged)
            {
                storage++;
            }

            altUIName?.Dispose();
            altUIName = null;

            if (storage > 0 && maxLevel > 0)
            {
                altUIName = string.Format("{0}+{1}", pSuper.Ref.Type.Ref.Base.UIName, storage);
                pSuper.Ref.UIName = altUIName;
            }
            else
            {
                pSuper.Ref.UIName = pSuper.Ref.Type.Ref.Base.UIName;
                altUIName = pSuper.Ref.UIName;
            }
        }
        
        [Hook(HookType.AresHook, Address = 0x6D4A40, Size = 6)]
        public static unsafe UInt32 BeforeDrawSuperWeaponTimer(REGISTERS* R)
        {
            var pSuper = (Pointer<SuperClass>)R->ECX;
            
            R->EDX = (uint)(IntPtr)pSuper.Ref.UIName;
            R->ECX = (uint)pSuper.Ref.Owner;

            return 0x6D4A46;
        }

    }
}