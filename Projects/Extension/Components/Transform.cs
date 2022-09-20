using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;

namespace Extension.Components
{
    [Serializable]
    public abstract class Transform : Component
    {
        public Transform() : base()
        {
            _transform = this;
        }

        public virtual Vector3 Location { get; set; }
        [Obsolete("don't use before finish")]
        public virtual Quaternion Rotation { get; set; }
        [Obsolete("don't use before finish")]
        public virtual Vector3 Scale { get; set; }

        public Transform GetParent()
        {
            return Parent.Parent.Transform;
        }

        public new Transform GetRoot()
        {
            return base.GetRoot().Transform;
        }
    }
}

