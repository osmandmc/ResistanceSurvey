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
            Unknown = 0,
            FullGain = 1,
            PartialGain = 2,
            ZeroGain = 3
        }
        

    }
    public static class Role
    {
        public const string Admin = "Admin";
        public const string User = "User";
    }
}