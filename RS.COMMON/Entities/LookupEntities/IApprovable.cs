namespace RS.COMMON.Entities.LookupEntity
{
    /// <summary>A list value researchers can add while entering a case; it waits for approval in Tanımlar until checked.</summary>
    public interface IApprovable
    {
        int Id { get; }
        string Name { get; set; }
        bool Approved { get; set; }
    }
}
