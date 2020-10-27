namespace RS.MVC.Models
{
    public class CompanyCreateViewModel
    {
        public string Name { get; set; }
        public int? TypeId { get; set; }
        public int? ScaleId { get; set; }
        public int? WorklineId { get; set; }
        
    }
    public class OutsourceCompanyCreateViewModel
    {
        public int MainCompanyId { get; set; }
        public string Name { get; set; }
        public int? TypeId { get; set; }
        public int? ScaleId { get; set; }
        public int? WorklineId { get; set; }

    }
}