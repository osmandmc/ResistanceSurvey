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
        private readonly IDbTransaction _transaction;
        private bool _disposed;
        public UnitOfWork(string connectionString)
        {
            
            _connection = new SqlConnection(connectionString);
            _connection.Open();
            _transaction = _connection.BeginTransaction();
        }
        public void Dispose()
        {
            dispose(true);
            GC.SuppressFinalize(this);
        }

        private void dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (_transaction != null)
                    {
                        _transaction.Dispose();
                    }
                    if (_connection != null)
                    {
                        _connection.Close();
                        _connection.Dispose();
                    }
                }
                _disposed = true;
            }
        }

        ~UnitOfWork()
        {
            dispose(false);
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
