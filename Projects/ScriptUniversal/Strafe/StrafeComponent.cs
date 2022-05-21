using Extension.Components;
using PatcherYRpp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Extension.Coroutines;
using ScriptUniversal.Strategy;

namespace ScriptUniversal.Strafe
{
    [Serializable]
    public abstract class StrafeComponent : WeaponFireStrategy
    {
        public StrafeComponent(Pointer<TechnoClass> techno, Pointer<WeaponTypeClass> weapon) : base(techno, weapon)
        {

        }


    }
}
