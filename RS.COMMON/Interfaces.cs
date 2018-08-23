using System;
using System.Collections.Generic;
using RS.COMMON.DTO;

namespace RS.COMMON
{
    public interface IUnitOfWork
    {
        IResistanceRepository ResistanceRepository {get;}
    }
    public interface IResistanceRepository
    {
        IEnumerable<ResistanceIndexDto> GetAll();
        IEnumerable<KeyValuePair<int,string>> GetCategories();
    }
    public interface IResistanceApplication
    {
        IEnumerable<KeyValuePair<int,string>> GetCategories();
        IEnumerable<ResistanceIndexDto> GetAll();
    }
}
