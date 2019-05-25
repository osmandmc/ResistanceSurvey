using System.Data;
using System.Linq;
using Dapper;
using RS.COMMON;
using RS.COMMON.DTO;

namespace RS.DAL.Repositories
{
    public class CompanyRepository : BaseRepository, ICompanyRepository
    {
        public CompanyRepository(IDbTransaction transaction) : base(transaction) { }
        public int AddCompany(CompanyDto company)
        {

            var companyQuery = "insert into Company(Name, WorkLineId, ScaleId, TypeId, IsOutsource) " +
                        "values(@Name, @WorkLineId, @ScaleId, @TypeId, @IsOutsource); " +
                        "SELECT LAST_INSERT_ID()";

            var companyId = _connection.Query<int>(companyQuery, new
            {
                company.Name,
                company.WorklineId,
                company.ScaleId,
                company.TypeId,
                IsOutsource = false
            }).Single();
            return companyId;

        }
        public int AddOutsourceCompany(CompanyDto company)
        {
            var outsourceCompanyQuery = "insert into Company(Name, WorkLineId, ScaleId, TypeId, IsOutsource) " +
            "values(@Name, @WorkLineId, @ScaleId, @TypeId, @IsOutsource); " +
            "SELECT LAST_INSERT_ID()";

            var outsourceCompanyId = _connection.Query<int>(outsourceCompanyQuery, new
            {
                company.Name,
                company.WorklineId,
                company.ScaleId,
                company.TypeId,
                IsOutsource = true
            }).Single();

            var crossCompanyQuery = "insert into CompanyOutsourceCompany(CompanyId, OutsourceCompanyId) " +
                       "values(@CompanyId, @OutsourceCompanyId);";

            _connection.Query<int>(crossCompanyQuery, new
            {
                companyId = company.OutsourceEmployerMainCompanyId,
                outsourceCompanyId,
            });

            return outsourceCompanyId;
        }
    }
}