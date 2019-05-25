using System.Collections.Generic;
using RS.COMMON;
using RS.COMMON.DTO;

namespace RS.MVC.Utilities
{
    public class StorageUtilities : IStorageUtilities
    {
        private readonly IUnitOfWork _uow;
        public StorageUtilities(IUnitOfWork uow)
        {
            _uow = uow;
        } 
        public IEnumerable<IdNamePair> GetLookup(string tableName)
        {
            var items =  _uow.LookupRepository.GetAll(tableName);
            return items;
        }
    }
}