using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamicPatcher;
using PatcherYRpp;

namespace Extension.Ext
{
    public class MapContainer<TExt, TBase> : Container<TExt, TBase> where TExt : Extension<TBase>
    {
        Dictionary<Pointer<TBase>, TExt> Items;

        public MapContainer(string name) : base(name)
        {
            Items = new Dictionary<Pointer<TBase>, TExt>();
        }

        public override TExt Find(Pointer<TBase> key)
        {
            if (Items.TryGetValue(key, out TExt ext))
            {
                return ext;
            }
            return null;
        }

        protected override TExt Allocate(Pointer<TBase> key)
        {
            TExt val = Activator.CreateInstance(typeof(TExt), key) as TExt;
            Items.Add(key, val);

            return val;
        }

        protected override void SetItem(Pointer<TBase> key, TExt ext)
        {
            Items[key] = ext;
        }

        public override void RemoveItem(Pointer<TBase> key)
        {
            Items.Remove(key);
        }

        public override void Clear()
        {
            if (Items.Count > 0)
            {
                Logger.Log("Cleared {0} items from {1}.\n", Items.Count, Name);
                Items.Clear();
            }
        }

    }

}
