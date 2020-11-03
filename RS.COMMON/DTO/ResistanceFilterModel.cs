namespace RS.COMMON.DTO
{
    public class ResistanceFilterModel: FilterModel
    {
        public int? CategoryId { get; set; }
        public int? CompanyId { get; set; }
        public int? YearId { get; set; }
        public int? MonthId { get; set; }
        public bool? PersonalNote { get; set; }

    }
}