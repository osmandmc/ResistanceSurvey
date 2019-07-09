using System;
using System.Collections.Generic;

namespace RS.COMMON.DTO
{
    public class ProtestoListQueryModel
    {
        public int Id { get; set; }
        public List<string> ProtestoTypes { get; set; }
        public DateTime StartDate { get; set; }
    }

}