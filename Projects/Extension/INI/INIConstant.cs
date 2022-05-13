using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PatcherYRpp;

namespace Extension.INI
{
    public static class INIConstant
    {
        public static string AiName { get; set; } = "aimd.ini";
        public static string ArtName { get; set; } = "artmd.ini";
        public static string RulesName { get; set; } = "rulesmd.ini";
        public static string Ra2md { get; set; } = "ra2md.ini";

        public static string GameModeName => SessionClass.Instance.MPGameMode.Ref.INIFilename;
        public static string MapName => ScenarioClass.Instance.FileName;
    }
}
