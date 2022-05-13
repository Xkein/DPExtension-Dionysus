using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Extension.Utilities;
using PatcherYRpp;

namespace Extension.INI.YRParsers
{
    public class WeaponTypeClassParser : FindTypeParser<WeaponTypeClass>
    {
        public WeaponTypeClassParser() : base(WeaponTypeClass.ABSTRACTTYPE_ARRAY)
        {

        }
    }
}
