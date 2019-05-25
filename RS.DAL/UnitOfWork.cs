using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.Common;
using Dapper;
using RS.COMMON;
using RS.DAL.Repositories;
using MySql.Data;
using MySql.Data.MySqlClient;

namespace RS.DAL
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly IDbConnection _connection;
        private readonly IDbTransaction _transaction;
        public UnitOfWork()
        {
            _connection = new MySqlConnection();
            _connection.ConnectionString = "Server=127.0.0.1; Uid=root; Pwd=root; Database=resistance_survey;";
            _connection.Open();
            _transaction = _connection.BeginTransaction();
        }

        private IResistanceRepository _resistanceRepository;
        public IResistanceRepository ResistanceRepository
        {
             get { return _resistanceRepository ?? (_resistanceRepository = new ResistanceRepository(_transaction)); }
        }
        private ICompanyRepository _companyRepository;
        public ICompanyRepository CompanyRepository
        {
             get { return _companyRepository ?? (_companyRepository = new CompanyRepository(_transaction)); }
        }
        private ILookupRepository _lookupRepository;
        public ILookupRepository LookupRepository
        {
            get
            {
                if (_lookupRepository == null)
                    _lookupRepository = new LookupRepository(_transaction);
                return _lookupRepository;
            }
        }

        public void Commit()
        {
            try
            {
                _transaction.Commit();
            }
            catch
            {
                _transaction.Rollback();
            }
            finally
            {
                _transaction.Connection.Close();
                _transaction.Dispose();
            }


        }
    }
}
