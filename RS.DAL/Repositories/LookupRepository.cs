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
            var foo = _connection.Query<IdNamePair>($"select * from {tableName}").AsList();
            return foo;
        }
    }
}