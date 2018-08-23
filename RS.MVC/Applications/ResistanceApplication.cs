using System.Collections.Generic;
using RS.COMMON;
using RS.COMMON.DTO;

namespace RS.MVC.Applications
{
    public class ResistanceApplication : IResistanceApplication
    {
        public readonly IUnitOfWork _uow;
        public ResistanceApplication(IUnitOfWork uow)
        {
            _uow = uow;

        }
        public IEnumerable<ResistanceIndexDto> GetAll()
        {
            return _uow.ResistanceRepository.GetAll();
        }
        public IEnumerable<KeyValuePair<int, string>> GetCategories()
        {
            return _uow.ResistanceRepository.GetCategories();
        }
        
    }
}