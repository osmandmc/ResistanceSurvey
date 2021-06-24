using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RS.COMMON.DTO;
using RS.MVC.Models;

namespace RS.COMMON
{
    public interface IUnitOfWork: IDisposable
    {
        IResistanceRepository ResistanceRepository { get; }
        ICompanyRepository CompanyRepository { get; }
        ILookupRepository LookupRepository { get; }
        void Commit();
    }
    public interface ILookupRepository
    {
        IEnumerable<IdNamePair> GetAll(string tableName);
        IEnumerable<IdNamePair> GetDistricts(int cityId);
    }
    public interface IStorageUtilities
    {
        IEnumerable<IdNamePair> GetLookup(string tableName);
        IEnumerable<IdNamePair> GetDistricts(int cityId);
    }
    // public interface IBaseRepository
    // {
    //     int Execute(string sql, object param = null);
    //     object ExecuteScalar(string sql, object param = null);
    //     IList<T> Query<T>(string sql, object param = null) where T : class;
    //     IList<TReturn> Query<T1, T2, TReturn>(string sql, Func<T1, T2, TReturn> map, object param = null) where T1 : class where T2 : class;
    //     Task<IEnumerable<T>> QueryAsync<T>(string sql, object param = null) where T : class;
    // }
    public interface IResistanceRepository
    {
        IEnumerable<ResistanceIndexDto> GetAll();
        ResistanceDto ExistingResistance(int companyId, int categoryId);
        void AddResistance(ResistanceCreateDto model, ProtestoCreateDto protesto);
        void AddResistance(ResistanceCreateDto model, ProtestoCreateDto protesto, CompanyDto company);
        ResistanceDetailDto GetResistanceDetail(int id);
    }
    public interface ICompanyRepository
    {
        int AddCompany(CompanyDto company);
        int AddOutsourceCompany(CompanyDto company);
    }

}
