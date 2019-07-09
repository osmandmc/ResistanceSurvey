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
            var allResistanceQuery = "select r.Id, r.Code, c.Name as CompanyName, ct.Name as CategoryName, p.ProtestoStartDate from Resistance r " +
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

            if (resistance.CorporationIds != null)
            {
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
            }
            if (resistance.EmploymentTypeIds != null)
            {

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
            if(protesto.InterventionTypeIds!=null)
            {
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
            

        }
        public void AddResistance(ResistanceCreateDto resistance, ProtestoCreateDto protesto, CompanyDto company)
        {
            _connection.Execute("insert into Resistance() values()", new
            {

            });
        }
        public ResistanceDetailDto GetResistanceDetail(int id)
        {
            var resistanceDictionary = new Dictionary<int, ResistanceDetailDto>();
            var protestoDictionary = new Dictionary<int, ProtestoListModel>();
            var query = "select distinct r.Id as Id, r.CompanyId, r.CategoryId, p.Id as ProtestoId, p.ProtestoStartDate as ProtestoStartDate, pt.Name as ProtestoType, " +
                "r.HasTradeUnion as HasTradeUnion, r.TradeUnionAuthorityId as TradeUnionAuthorityId, r.EmployeeCount as EmployeeCount, " + 
                "r.EmployeeCountId as EmployeeCountId, emp.Id as EmploymentTypeId, corp.Id as CorporationId " +
                "from Resistance r " +  
                "left join resistancetocorporation rto on rto.resistanceId = r.Id " +
                "left join corporation corp on corp.Id = rto.corporationId " +
                "left join resistancetoemploymenttype remp on remp.resistanceId " +
                "left join employmenttype emp on remp.employmenttypeId = emp.Id " +
                "left join Protesto p on r.Id = p.ResistanceId " +
                "left join ProtestotoType ptt on p.Id = ptt.ProtestoId " +
                "left join ProtestoType pt on pt.Id = ptt.protestoTypeId " + 
                "where r.Id = @id";
                var result3 = _connection.Query<ResistanceQueryModel, ProtestoListModel, string, int, int, ResistanceDetailDto>(query,
                (resistance, protesto, protestoType, employmentTypeId, corporationId) =>{
                    ResistanceDetailDto resistanceEntry;
                    ProtestoListModel protestoEntry;
                    
                    if (!resistanceDictionary.TryGetValue(resistance.Id, out resistanceEntry))
                    {
                        resistanceEntry = new ResistanceDetailDto(resistance.Id, resistance.CompanyId, resistance.CategoryId, resistance.HasTradeUnion, resistance.TradeUnionAuthorityId, resistance.TradeUnionId, resistance.EmployeeCount, resistance.EmployeeCountId);
                        resistanceDictionary.Add(resistanceEntry.Id, resistanceEntry);
                    }
                    if (!protestoDictionary.TryGetValue(protesto.ProtestoId, out protestoEntry))
                    {
                        protestoEntry = protesto;
                        protestoEntry.ProtestoTypes = new List<string>();
                        
                        protestoDictionary.Add(protesto.ProtestoId, protestoEntry);
                    }
                    if(!protestoEntry.ProtestoTypes.Contains(protestoType))
                        protestoEntry.ProtestoTypes.Add(protestoType); 
                    
                    if(!resistanceEntry.Protestos.Any(p=>p.ProtestoId == protestoEntry.ProtestoId))
                        resistanceEntry.Protestos.Add(protestoEntry);
                    if(!resistanceEntry.EmploymentTypeIds.Contains(employmentTypeId))
                        resistanceEntry.EmploymentTypeIds.Add(employmentTypeId);
                    if(!resistanceEntry.CorporationIds.Contains(corporationId))
                        resistanceEntry.CorporationIds.Add(corporationId);
                    return resistanceEntry;
                  
                }, new { id = @id }, splitOn:"ProtestoId, ProtestoType, EmploymentTypeId, CorporationId")
                .FirstOrDefault();
    
            

            return result3;
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