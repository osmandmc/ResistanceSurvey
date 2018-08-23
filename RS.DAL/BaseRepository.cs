using Dapper;
using System.Security;
using System.Security.Permissions;
using System.Data.Common;
using System.Data;

namespace RS.DAL
{
    public class BaseRepository
    {
        public IDbConnection _connection;
        public BaseRepository(IDbConnection connection)
        {
            _connection = connection;
        }

        
    } 

}