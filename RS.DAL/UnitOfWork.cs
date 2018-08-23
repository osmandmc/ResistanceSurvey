using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.Common;
using Dapper;
using RS.COMMON;
using RS.DAL.Repositories;

namespace RS.DAL
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly IDbConnection _connection;
        public UnitOfWork()
        {

            var connection = SqlClientFactory.Instance.CreateConnection();

       //     connection.ConnectionString = "Server=127.0.0.1; Uid=root; Pwd=root; Database=resistance_survey;";
        //    _connection = new SqlConnection("Server=127.0.0.1; Uid=root; Pwd=root; Database = resistance_survey;");  
           _connection = connection;
          // _connection.Open();
        }

        private IResistanceRepository _resistanceRepository;
        public IResistanceRepository ResistanceRepository
        {
            get
            {
                if (_resistanceRepository == null)
                    _resistanceRepository = new ResistanceRepository(_connection);
                return _resistanceRepository;
            }
        }

    }
}
