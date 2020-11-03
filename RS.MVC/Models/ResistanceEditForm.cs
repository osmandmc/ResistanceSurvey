using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RS.MVC.Models
{
    public class ResistanceEditForm
    {
        public ResistanceEditModel Resistance { get; set; }
        public CompanyCreateViewModel Company { get; set; }
        public CompanyCreateViewModel MainCompany { get; set; }
    }
}
