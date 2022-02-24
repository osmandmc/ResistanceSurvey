using System.ComponentModel;

namespace RS.COMMON.Constants
{
    public class Enums
    {
        public enum Status
        {
            Pending,
            Linked,
            Passive
        }
        public enum ResistanceResult
        {
            [Description("Bilinmiyor")]
            Unknown = 0,
            [Description("Tam Kazaným")]
            FullGain = 1,
            [Description("Yarým Kazaným")]
            PartialGain = 2,
            [Description("Sýfýr Kazaným")]
            ZeroGain = 3
        }
        

    }
    public static class Role
    {
        public const string Admin = "Admin";
        public const string User = "User";
    }
}