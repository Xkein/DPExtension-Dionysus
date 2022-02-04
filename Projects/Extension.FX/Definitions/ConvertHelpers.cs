using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extension.FX.Definitions
{
    public static class ConvertHelpers
    {
        public static CoordStruct ToCoordStruct(this Vector3 vector)
        {
            return new CoordStruct((int)vector.X, (int)vector.Y, (int)vector.Z);
        }

        public static ColorStruct ToColorStruct(this Vector3 vector)
        {
            return new ColorStruct((int)vector.X, (int)vector.Y, (int)vector.Z);
        }
    }
}
