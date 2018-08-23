using System;
using System.Collections.Generic;
using Dapper;
using RS.COMMON;
using RS.COMMON.DTO;
using System.Data;

namespace RS.DAL.Repositories
{
    public class ResistanceRepository : IResistanceRepository
    {
         public IDbConnection _connection;
        public ResistanceRepository(IDbConnection connection)
        {
            _connection = connection;
        }

        public IEnumerable<ResistanceIndexDto> GetAll()
        {
            var res = new List<ResistanceIndexDto>();
            res.Add(new ResistanceIndexDto { Id = 1, CompanyName = "Flormar", CategoryName = "İşletme/Kurum", StartDate = new DateTime(2018, 3, 3) });
            return res;
        }
        public IEnumerable<KeyValuePair<int,string>> GetCategories()
        {
            var foo = _connection.Query<KeyValuePair<int,string>>("select * from category").AsList();
            return foo;
        
        }

    }
}