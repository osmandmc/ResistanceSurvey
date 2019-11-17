using System.Collections.Generic;
using System.Data;
using Dapper;
using RS.COMMON;
using RS.COMMON.DTO;

namespace RS.DAL.Repositories
{
    public class LookupRepository: BaseRepository, ILookupRepository
    {
        public LookupRepository(IDbTransaction transaction): base(transaction){}

        public IEnumerable<IdNamePair> GetAll(string tableName)
        {
            var foo = _connection.Query<IdNamePair>($"select * from {tableName}", transaction:_transaction).AsList();
            return foo;
        }
        public IEnumerable<IdNamePair> GetDistricts(int cityId)
        {
            var districts = _connection.Query<IdNamePair>($"select * from District where CityId = @cityId", new { cityId },  transaction:_transaction).AsList();
            return districts;
        }
    }
}