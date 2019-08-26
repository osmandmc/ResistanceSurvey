using Dapper;
using System.Security;
using System.Security.Permissions;
using System.Data.Common;
using System.Data;
using RS.COMMON;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Threading.Tasks;

namespace RS.DAL
{
    public class BaseRepository
    {
        public IDbConnection _connection;
        protected readonly IDbTransaction _transaction;
        public BaseRepository(IDbTransaction transaction)
        {
            _transaction = transaction;
            _connection = transaction.Connection;
        }

     

    }

}