using System;
using System.Collections.Generic;
using RS.COMMON;
using RS.COMMON.DTO;
using RS.DAL;

namespace RS.MVC.Utilities
{

    public class StorageUtilities : IStorageUtilities
    {
        private readonly string _connectionString;
        public StorageUtilities(string connectionString)
        {
            _connectionString = connectionString;
        } 
        public static T GetValue<T>(String value)
        {
            return (T)Convert.ChangeType(value, typeof(T));
        }
        public IEnumerable<IdNamePair> GetLookup(string tableName)
        {
            using (var _uow = new UnitOfWork(_connectionString))
            {
                return _uow.LookupRepository.GetAll(tableName);
            }
        }
        public IEnumerable<IdNamePair> GetDistricts(int cityId)
        {
            using (var _uow = new UnitOfWork(_connectionString))
            {
                return _uow.LookupRepository.GetDistricts(cityId);
            }
        }
    }
}