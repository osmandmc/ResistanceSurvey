namespace RS.COMMON.Entities.LookupEntity
{
    /// <summary>An işkolu (workers, Law 6356) or, with IsCivilService, a hizmet kolu (civil servants, Law 4688; “MEMUR - …”).</summary>
    public class CompanyWorkLine : LookupEntity
    {
        public bool IsCivilService { get; set; }
    }
}