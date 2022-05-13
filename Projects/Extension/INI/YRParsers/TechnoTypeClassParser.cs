using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Extension.Utilities;
using PatcherYRpp;

namespace Extension.INI.YRParsers
{
    public class TechnoTypeClassParser : FindTypeParser<TechnoTypeClass>
    {
        public TechnoTypeClassParser() : base(TechnoTypeClass.ABSTRACTTYPE_ARRAY)
        {

        }
    }
}
