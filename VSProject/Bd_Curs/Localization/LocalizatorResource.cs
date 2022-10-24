using System.Resources;

namespace Bd_Curs
{
    public static class LocalizatorResource
    {
        public static ResourceManager Localize;
        public static void INIT(System.Reflection.Assembly type)=>
            Localize = new ResourceManager("Bd_Curs.Localization.Language", type);
    }
}
