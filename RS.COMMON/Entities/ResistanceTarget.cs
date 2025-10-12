using System.Collections.Generic;

namespace RS.COMMON.Entities;

public abstract class ResistanceTarget
{
    public int Id { get; set; }
    public string Name { get; set; }
    
    public ICollection<Resistance> Resistances { get; set; }

}