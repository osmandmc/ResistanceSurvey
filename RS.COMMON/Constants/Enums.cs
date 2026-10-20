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
            [Description("Tam Kazanım")]
            FullGain = 1,
            [Description("Yarım Kazanım")]
            PartialGain = 2,
            [Description("Sıfır Kazanım")]
            ZeroGain = 3
        }
        

    }
    public static class Role
    {
        public const string Admin = "Admin";
        public const string User = "User";
    }
}