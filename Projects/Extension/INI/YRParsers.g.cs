
using PatcherYRpp;


namespace Extension.INI
{
    internal partial class YRParsers
    {
        public static partial void Register()
        {
            new SuperWeaponTypeClassParser().Register();
            new TechnoTypeClassParser().Register();
            new WeaponTypeClassParser().Register();
        }
    }
    
    public class SuperWeaponTypeClassParser : FindTypeParser<SuperWeaponTypeClass>
    {
        public SuperWeaponTypeClassParser() : base(SuperWeaponTypeClass.ABSTRACTTYPE_ARRAY)
        {

        }
    }
    public class TechnoTypeClassParser : FindTypeParser<TechnoTypeClass>
    {
        public TechnoTypeClassParser() : base(TechnoTypeClass.ABSTRACTTYPE_ARRAY)
        {

        }
    }
    public class WeaponTypeClassParser : FindTypeParser<WeaponTypeClass>
    {
        public WeaponTypeClassParser() : base(WeaponTypeClass.ABSTRACTTYPE_ARRAY)
        {

        }
    }
}
