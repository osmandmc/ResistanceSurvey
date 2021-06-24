using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RS.MVC.Models
{
    public class CorporationListView
    {
        public string Name { get; internal set; }
        public string CorporationTypeName { get; internal set; }
        public bool Approved { get; internal set; }
    }
}
