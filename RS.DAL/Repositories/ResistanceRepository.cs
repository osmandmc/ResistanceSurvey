using System;
using System.Collections.Generic;
using Dapper;
using RS.COMMON;
using RS.COMMON.DTO;
using System.Data;
using System.Linq;

namespace RS.DAL.Repositories
{
    public class ResistanceRepository : BaseRepository, IResistanceRepository
    {
        public ResistanceRepository(IDbTransaction transaction) : base(transaction) { }

        public IEnumerable<ResistanceIndexDto> GetAll()
        {
            var allResistanceQuery = "select c.Name as CompanyName, ct.Name as CategoryName, p.ProtestoStartDate from Resistance r " +
            "inner join Company c on r.CompanyId = c.Id " +
            "inner join Category ct on r.CategoryId = ct.Id " +
            "left outer join Protesto p on r.Id = p.ResistanceId";

            var response = _connection.Query<ResistanceIndexDto>(allResistanceQuery);
            return response;
        }
        public void AddResistance(ResistanceCreateDto resistance, ProtestoCreateDto protesto)
        {
            var resistanceQuery = "insert into Resistance(CompanyId, CategoryId, EmployeeCountId, EmployeeCount, HasTradeUnion, TradeUnionAuthorityId, TradeUnionId, StartDate, EndDate)" +
            "values(@CompanyId, @CategoryId, @EmployeeCountId, @EmployeeCount, @HasTradeUnion, @TradeUnionAuthorityId, @TradeUnionId, @StartDate, @EndDate);" +
            "SELECT LAST_INSERT_ID()";

            var resistanceId = _connection.Query<int>(resistanceQuery, new
            {
                resistance.CompanyId,
                resistance.CategoryId,
                resistance.EmployeeCountId,
                resistance.EmployeeCount,
                resistance.HasTradeUnion,
                resistance.TradeUnionAuthorityId,
                resistance.TradeUnionId,
                resistance.StartDate,
                resistance.EndDate
            }).Single();

            foreach (var item in resistance.CorporationIds)
            {
                var corporationsQuery = "insert into resistancetocorporation(resistanceId, corporationId)" +
                "values(@resistanceId, @corporationId)";
                _connection.Query<int>(corporationsQuery, new
                {
                    resistanceId,
                    corporationId = item
                });
            }

            foreach (var item in resistance.EmploymentTypeIds)
            {
                var corporationsQuery = "insert into resistancetoemploymenttype(resistanceId, employmenttypeId)" +
                "values(@resistanceId, @employmenttypeId)";
                _connection.Query<int>(corporationsQuery, new
                {
                    resistanceId,
                    employmenttypeId = item
                });
            }

            var protestoQuery = "insert into Protesto(ResistanceId, EmployeeCountInProtestoId, EmployeeCountInProtesto, CustodyCount, GenderId, IsAgainstProduction, ProtestoStartDate, ProtestoEndDate)" +
           "values(@ResistanceId, @EmployeeCountInProtestoId, @EmployeeCountInProtesto, @CustodyCount, @GenderId, @IsAgainstProduction, @ProtestoStartDate, @ProtestoEndDate);" +
           "SELECT LAST_INSERT_ID()";
            var protestoId = _connection.Query<int>(protestoQuery, new
            {
                resistanceId,
                protesto.EmployeeCountInProtestoId,
                protesto.EmployeeCountInProtesto,
                protesto.CustodyCount,
                protesto.GenderId,
                protesto.IsAgainstProduction,
                protesto.ProtestoEndDate,
                protesto.ProtestoStartDate
            }).Single();

            foreach (var item in protesto.ProtestoTypeIds)
            {
                var protestotypeQuery = "insert into Protestototype(protestoId, protestotypeId)" +
         "values(@ProtestoId, @protestotypeId)";
                _connection.Query<int>(protestotypeQuery, new
                {
                    protestoId,
                    protestotypeId = item
                });
            }
            foreach (var item in protesto.ProtestoReasonIds)
            {
                var protestoreasonQuery = "insert into Protestotoreason(protestoId, protestoreasonId)" +
         "values(@ProtestoId, @protestoreasonId)";
                _connection.Query<int>(protestoreasonQuery, new
                {
                    protestoId,
                    protestoreasonId = item
                });
            }
            foreach (var item in protesto.ProtestoPlaceIds)
            {
                var protestoplaceQuery = "insert into Protestotoplace(protestoId, protestoplaceId)" +
         "values(@ProtestoId, @protestoplaceId)";
                _connection.Query<int>(protestoplaceQuery, new
                {
                    protestoId,
                    protestoplaceId = item
                });
            }
            foreach (var item in protesto.InterventionTypeIds)
            {
                var protestoplaceQuery = "insert into Protestotointerventiontype(protestoId, interventiontypeId)" +
         "values(@ProtestoId, @interventiontypeId)";
                _connection.Query<int>(protestoplaceQuery, new
                {
                    protestoId,
                    interventiontypeId = item
                });
            }

        }
        public void AddResistance(ResistanceCreateDto resistance, ProtestoCreateDto protesto, CompanyDto company)
        {
            _connection.Execute("insert into Resistance() values()", new
            {

            });
        }

        public ResistanceDto ExistingResistance(int companyId, int categoryId)
        {
            var query = $"select * from resistance where categoryId = " + categoryId +
            " and companyId = " + companyId;

            var resistance = _connection.Query<ResistanceDto>(query).FirstOrDefault();
            return resistance;
        }
    }
}