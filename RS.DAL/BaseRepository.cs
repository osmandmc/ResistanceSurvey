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

        // public int Execute(string sql, object param = null)
        // {
        //     int temp = _connection.Execute(sql, param, _transaction);
        //     return temp;
        // }
        // public object ExecuteScalar(string sql, object param = null)
        // {
        //     var temp = _connection.ExecuteScalar(sql, param, _transaction);
        //     return temp;
        // }
        // public IList<T> Query<T>(string sql, object param = null) where T : class
        // {
        //     var temp = _connection.Query<T>(sql, param, _transaction);
        //     return temp.ToList();
        // }
        // public IList<TReturn> Query<T1, T2, TReturn>(string sql, Func<T1, T2, TReturn> map, object param = null) where T1 : class where T2 : class
        // {
        //     var temp = _connection.Query<TReturn>(sql, param, _transaction);
        //     return temp.ToList();
        // }
        // public Task<IEnumerable<T>> QueryAsync<T>(string sql, object param = null) where T : class
        // {
        //     var temp = _connection.QueryAsync<T>(sql, param, _transaction);
        //     return temp;
        // }

    }

}