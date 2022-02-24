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
            [Description("Tam Kazan�m")]
            FullGain = 1,
            [Description("Yar�m Kazan�m")]
            PartialGain = 2,
            [Description("S�f�r Kazan�m")]
            ZeroGain = 3
        }
        

    }
    public static class Role
    {
        public const string Admin = "Admin";
        public const string User = "User";
    }
}