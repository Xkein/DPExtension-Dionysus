using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PatcherYRpp;

namespace Extension.Ext
{
    [Serializable]
    public abstract class TypeExtension<TExt, TBase> : Extension<TBase> where TExt : Extension<TBase>
    {
        public static Container<TExt, TBase> ExtMap = new Container<TExt, TBase>(typeof(TBase).Name);

        protected TypeExtension(Pointer<TBase> OwnerObject) : base(OwnerObject)
        {

        }

    }
}
