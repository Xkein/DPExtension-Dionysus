using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using Extension.Components;
using Extension.Decorators;
using Extension.INI;
using Extension.Script;
using Extension.Utilities;
using PatcherYRpp;

namespace Extension.Ext
{
    [Serializable]
    public abstract class CommonTypeExtension<TExt, TBase> : TypeExtension<TExt, TBase>, IHaveScript where TExt : Extension<TBase>
    {
        protected CommonTypeExtension(Pointer<TBase> OwnerObject) : base(OwnerObject)
        {

        }


        public List<Script.Script> Scripts => _scripts;

        protected override void LoadFromINIFile(Pointer<CCINIClass> pINI)
        {
            base.LoadFromINIFile(pINI);

            INIReader reader = new INIReader(pINI);
            string section = OwnerObject.Convert<AbstractTypeClass>().Ref.ID;

            reader.Read(section, "Scripts", ref _scripts);
        }

        private List<Script.Script> _scripts;
    }

    [Serializable]
    public abstract class CommonInstanceExtension<TExt, TBase, TTypeExt, TTypeBase> : InstanceExtension<TExt, TBase>, IHaveComponent
        where TExt : Extension<TBase>
        where TBase : IOwnAbstractType<TTypeBase>
        where TTypeExt : CommonTypeExtension<TTypeExt, TTypeBase>
    {
        private static string _extComponentName = $"{typeof(TExt).Name} root component";

        protected CommonInstanceExtension(Pointer<TBase> OwnerObject) : base(OwnerObject)
        {
            _extComponent = new ExtComponent<TExt>(this as TExt, 0, _extComponentName);
            _decoratorComponent = new DecoratorComponent();

            _extComponent.OnAwake += () => Type = CommonTypeExtension<TTypeExt, TTypeBase>.ExtMap.Find(this.OwnerObject.Ref.OwnType);
            
            _extComponent.OnAwake += () => ScriptManager.CreateScriptableTo(_extComponent, Type.Scripts, this as TExt);
            _extComponent.OnAwake += () => _decoratorComponent.AttachToComponent(_extComponent);
        }



        public TTypeExt Type { get; internal set; }
        public ref TTypeBase OwnerTypeRef => ref Type.OwnerRef;
        //ExtensionReference<TTypeExt> type;
        //public TTypeExt Type
        //{
        //    get
        //    {
        //        if (type.TryGet(out TTypeExt ext) == false)
        //        {
        //            type.Set(OwnerObject.Ref.OwnType);
        //            ext = type.Get();
        //        }
        //        return ext;
        //    }
        //}

        public ExtComponent<TExt> ExtComponent => _extComponent.GetAwaked();
        public DecoratorComponent DecoratorComponent => _decoratorComponent;
        public Component AttachedComponent => ExtComponent;

        public override void SaveToStream(IStream stream)
        {
            base.SaveToStream(stream);
            _extComponent.Foreach(c => c.SaveToStream(stream));
        }

        public override void LoadFromStream(IStream stream)
        {
            base.LoadFromStream(stream);
            _extComponent.Foreach(c => c.LoadFromStream(stream));
        }

        private ExtComponent<TExt> _extComponent;
        private DecoratorComponent _decoratorComponent;
    }
}
