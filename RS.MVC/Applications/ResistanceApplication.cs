using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using RS.COMMON;
using RS.COMMON.DTO;
using RS.COMMON.Entities;
using RS.COMMON.Entities.LookupEntity;
using RS.EF;
using RS.MVC.Models;

namespace RS.MVC.Applications
{
    public interface IResistanceApplication
    {
        PagedResult<ResistanceIndexDto> GetPaged(ResistanceFilterModel filter);
        IEnumerable<ResistanceIndexDto> GetAll();
        void Create(ResistanceCreateModel model);
        Result<CompanyReturnDto> CreateCompany(CompanyCreateViewModel model);
        OutsourceCompanyReturnDto CreateOutsourceCompany(OutsourceCompanyCreateViewModel model);
        Resistance ExistingResistance(int companyId, int categoryId);
        ResistanceEditModel GetResistanceDetail(int id);
        void AddProtesto(ProtestoAddModel viewModel);
        ProtestoEditModel GetProtestoDetail(int id);
        void UpdateResistance(ResistanceEditModel viewModel);
        void UpdateProtesto(ProtestoEditModel model);
        IEnumerable<NewsItem> GetNewsList(int year, int month);
        string GetNewsContent(int newsId);
        string GetResistanceName(int id);
        void DeleteProtesto(ProtestoDeleteModel model);
        void DeleteResistance(ResistanceDeleteModel viewModel);
    }
    public class ResistanceApplication : IResistanceApplication
    {
        public readonly RSDBContext _db;
        public ResistanceApplication(RSDBContext db)
        {
            _db = db;
        }
        public PagedResult<ResistanceIndexDto> GetPaged(ResistanceFilterModel filter)
        {
            var query = _db.Resistance
                    .Where(rs => !rs.Deleted && (filter.CompanyId == null || rs.CompanyId == filter.CompanyId)
                    && filter.CategoryId == null || rs.CategoryId == filter.CategoryId)
                    .Select(r => new ResistanceIndexDto
                    {
                        Id = r.Id,
                        CategoryName = r.Category.Name,
                        CompanyName = r.Company.Name,
                        Code = r.Code,
                        StartDate = r.StartDate
                    })
                    .OrderByDescending(x => x.StartDate);

            var result = new PagedResult<ResistanceIndexDto>();
            result.CurrentPage = filter.PageNumber;
            result.PageSize = filter.PageSize;
            result.RowCount = query.Count();


            var pageCount = (double)result.RowCount / filter.PageSize;
            result.PageCount = (int)Math.Ceiling(pageCount);

            var skip = (filter.PageNumber - 1) * filter.PageSize;
            result.Results = query.AsNoTracking().Skip(skip).Take(filter.PageSize).ToList();

            return result;

        }
        public IEnumerable<ResistanceIndexDto> GetAll()
        {
            var resistances = _db.Resistance.Select(r => new ResistanceIndexDto
            {
                Id = r.Id,
                CategoryName = r.Category.Name,
                CompanyName = r.Company.Name,
                Code = r.Code,
                StartDate = r.Protestos.OrderByDescending(o => o.StartDate).Select(p => p.StartDate).First()
            }).ToList();
            return resistances;
        }
        public void Create(ResistanceCreateModel model)
        {
            Resistance resistance = model.MapToResistanceDto();
            BindCorporation(resistance, model.CorporationIds);
            BindResistanceReason(resistance, model.ResistanceReasonIds);
            BindTradeUnionId(resistance, model.TradeUnionId);

            if (model.ResistanceNewsIds != null)
                model.ResistanceNewsIds.Where(n => !n.IsDeleted).ToList().ForEach(news => resistance.ResistanceNews.Add(new ResistanceNews { NewsId = news.Id }));

            Protesto protesto = model.MapToProtestoDto();
            BindProtestoType(protesto, model.ProtestoTypeIds);
            BindProtestoPlace(protesto, model.ProtestoPlaceIds);
            _db.Resistance.Add(resistance);
            protesto.ResistanceId = resistance.Id;
            _db.Protesto.Add(protesto);
            _db.SaveChanges();
        }
        public Result<CompanyReturnDto> CreateCompany(CompanyCreateViewModel model)
        {
            var result = new Result<CompanyReturnDto>();
            if (_db.Company.Any(s => s.Name == model.Name && !s.Deleted))
            {
                result.Success = false;
            }
            else
            {

                var comapny = _db.Company.Add(new Company { Name = model.Name, CompanyScaleId = model.ScaleId, CompanyTypeId = model.TypeId, CompanyWorkLineId = model.WorklineId, IsOutsource = false });
                _db.SaveChanges();
                result.Success = true;
                var returnModel = new CompanyReturnDto(comapny.Entity);
                result.Response = returnModel;
            }

            return result;
        }
        public OutsourceCompanyReturnDto CreateOutsourceCompany(OutsourceCompanyCreateViewModel model)
        {
            var company = _db.Company.Add(new Company
            {
                Name = model.Name,
                IsOutsource = true,
                CompanyScaleId = model.ScaleId,
                CompanyTypeId = model.TypeId,
                CompanyWorkLineId = model.WorklineId
            });
            var companyOutsource = _db.CompanyOutsourceCompany.Add(new CompanyOutsourceCompany
            {
                CompanyId = model.MainCompanyId,
                OutsourceCompanyId = company.Entity.Id
            });

            _db.SaveChanges();
            var returnModel = new OutsourceCompanyReturnDto(company.Entity, model.MainCompanyId);
            return returnModel;
        }
        public ResistanceEditModel GetResistanceDetail(int id)
        {
            var data = (from s in _db.Resistance
                        join coc in _db.CompanyOutsourceCompany on s.CompanyId equals coc.OutsourceCompanyId into oc
                        from o in oc.DefaultIfEmpty()
                        where s.Id == id && !s.Deleted
                        select new ResistanceEditModel
                        {
                            Id = s.Id,
                            Code = s.Code,
                            CompanyId = o != null ? o.CompanyId : s.CompanyId,
                            OutsourceCompanyId = o != null ? o.OutsourceCompanyId : 0,
                            IsOutsource = s.Company.IsOutsource,
                            CategoryId = s.CategoryId,
                            CorporationIds = s.ResistanceCorporations.Select(rc => rc.CorporationId.ToString()).ToList(),
                            EmploymentTypeIds = s.ResistanceEmploymentTypes.Select(emp => emp.EmploymentTypeId).ToList(),
                            ResistanceReasonIds = s.ResistanceResistanceReasons.Select(emp => emp.ResistanceReasonId.ToString()).ToList(),
                            ResistanceNewsIds = s.ResistanceNews.Select(news => new ResistanceNewsModel { Id = news.NewsId, IsDeleted = false }).ToList(),
                            EmployeeCount = s.EmployeeCountNumber,
                            EmployeeCountId = s.EmployeeCountId,
                            HasTradeUnion = s.HasTradeUnion,
                            DevelopRight = s.DevelopRight,
                            TradeUnionAuthorityId = s.TradeUnionAuthorityId,
                            TradeUnionId = s.TradeUnionId.ToString(),
                            LegalInterventionDesc = s.LegalInterventionDesc,
                            ResistanceDescription = s.Description,
                            ResistanceNote = s.Note,
                            FiredEmployeeCountByProtesto = s.FiredEmployeeCountByProtesto,
                            ResistanceNews = s.ResistanceNews.OrderBy(s => s.News.Date).Select(r => r.News).ToList(),

                            Protestos = s.Protestos.Where(s => !s.Deleted).Select(p => new ProtestoListModel
                            {
                                ProtestoId = p.Id,
                                ProtestoStartDate = p.StartDate,
                                ProtestoTypes = p.ProtestoProtestoTypes.Select(pt => pt.ProtestoType.Name).ToList()
                            }).ToList()
                        })
            .FirstOrDefault();
            return data;
        }
        public Resistance ExistingResistance(int companyId, int categoryId)
        {
            return _db.Resistance.FirstOrDefault(c => c.CompanyId == companyId && c.CategoryId == categoryId);
        }
        public void UpdateResistance(ResistanceEditModel viewModel)
        {
            var resistance = _db.Resistance.SingleOrDefault(r => r.Id == viewModel.Id);
            resistance.Id = viewModel.Id;
            resistance.CategoryId = viewModel.CategoryId;
            resistance.CompanyId = viewModel.OutsourceCompanyId ?? viewModel.CompanyId;
            resistance.Code = viewModel.Code;
            resistance.EmployeeCountId = viewModel.EmployeeCountId;
            resistance.EmployeeCountNumber = viewModel.EmployeeCount;
            resistance.HasTradeUnion = viewModel.HasTradeUnion;
            resistance.TradeUnionAuthorityId = viewModel.TradeUnionAuthorityId;
            resistance.Description = viewModel.ResistanceDescription;
            resistance.Note = viewModel.ResistanceNote;
            resistance.LegalInterventionDesc = viewModel.LegalInterventionDesc;
            resistance.FiredEmployeeCountByProtesto = viewModel.FiredEmployeeCountByProtesto;
            resistance.Updater = viewModel.UserName;
            resistance.UpdateDate = DateTime.Now;
            resistance.ResistanceCorporations = new List<ResistanceCorporation>();
            resistance.ResistanceEmploymentTypes = new List<ResistanceEmploymentType>();
            resistance.ResistanceResistanceReasons = new List<ResistanceResistanceReason>();
            resistance.ResistanceNews = new List<ResistanceNews>();
            if (viewModel.AnyLegalIntervention == 1) resistance.AnyLegalIntervention = true;
            if (viewModel.AnyLegalIntervention == 2) resistance.AnyLegalIntervention = false;
            var rCorporations = _db.ResistanceCorporation.Where(r => r.ResistanceId == resistance.Id).ToList();
            _db.ResistanceCorporation.RemoveRange(rCorporations);
            if (viewModel.EmploymentTypeIds != null)
                viewModel.EmploymentTypeIds.ForEach(emp => resistance.ResistanceEmploymentTypes.Add(new ResistanceEmploymentType
                {
                    EmploymentTypeId = emp
                }));
            var rEmpl = _db.ResistanceEmploymentType.Where(r => r.ResistanceId == resistance.Id).ToList();
            _db.ResistanceEmploymentType.RemoveRange(rEmpl);

            var rReason = _db.ResistanceResistanceReason.Where(r => r.ResistanceId == resistance.Id).ToList();
            _db.ResistanceResistanceReason.RemoveRange(rReason);
            if (viewModel.ResistanceNewsIds != null)
                viewModel.ResistanceNewsIds.Where(n => !n.IsDeleted).ToList().ForEach(news => resistance.ResistanceNews.Add(new ResistanceNews
                {
                    NewsId = news.Id
                }));
            var rNews = _db.ResistanceNews.Where(r => r.ResistanceId == resistance.Id).ToList();
            _db.ResistanceNews.RemoveRange(rNews);

            BindCorporation(resistance, viewModel.CorporationIds);
            BindResistanceReason(resistance, viewModel.ResistanceReasonIds);
            _db.Resistance.Update(resistance);
            _db.SaveChanges();
        }
        public void UpdateProtesto(ProtestoEditModel model)
        {
            var protesto = model.ToEntity();
            BindProtestoType(protesto, model.ProtestoTypeIds);
            BindProtestoPlace(protesto, model.ProtestoPlaceIds);
            _db.ProtestoInterventionType.RemoveRange(_db.ProtestoInterventionType.Where(i => i.ProtestoId == model.ProtestoId).ToList());
            _db.ProtestoProtestoPlace.RemoveRange(_db.ProtestoProtestoPlace.Where(i => i.ProtestoId == model.ProtestoId).ToList());
            _db.ProtestoProtestoType.RemoveRange(_db.ProtestoProtestoType.Where(i => i.ProtestoId == model.ProtestoId).ToList());
            _db.ProtestoCity.RemoveRange(_db.ProtestoCity.Where(i => i.ProtestoId == model.ProtestoId).ToList());
            _db.ProtestoDistrict.RemoveRange(_db.ProtestoDistrict.Where(i => i.ProtestoId == model.ProtestoId).ToList());
            _db.ProtestoLocation.RemoveRange(_db.ProtestoLocation.Where(i => i.ProtestoId == model.ProtestoId).ToList());
            _db.ProtestoLocation.AddRange(protesto.Locations);
            _db.Protesto.Update(protesto);
            _db.SaveChanges();
        }
        public ProtestoEditModel GetProtestoDetail(int id)
        {
            var protesto = _db.Protesto.Where(p => p.Id == id).Select(s => new ProtestoEditModel
            {
                ProtestoId = s.Id,
                ResistanceId = s.ResistanceId,
                CustodyCount = s.CustodyCount,
                EmployeeCountInProtesto = s.EmployeeCountNumber,
                GenderId = s.GenderId,
                EmployeeCountInProtestoId = s.ProtestoEmployeeCountId,
                InterventionTypeIds = s.ProtestoInterventionTypes.Select(i => i.InterventionTypeId).ToList(),
                ProtestoPlaceIds = s.ProtestoProtestoPlaces.Select(pp => pp.ProtestoPlaceId.ToString()).ToList(),
                ProtestoTypeIds = s.ProtestoProtestoTypes.Select(pt => pt.ProtestoTypeId.ToString()).ToList(),
                ProtestoCityIds = s.Cities.Select(c => c.CityId).ToList(),
                Locations = s.Locations.Select(loc=> new ProtestoLocationModel { ProtestoId = loc.ProtestoId, CityId = loc.CityId, DistrictId = loc.DistrictId, Place = loc.Place, Deleted = false}).ToList(),
                ProtestoDistrictIds = s.Districts != null ? s.Districts.Select(c => (int?)c.DistrictId).ToList() : null,
                ProtestoStartDate = s.StartDate,
                ProtestoEndDate = s.EndDate,
                ResistanceName = s.Resistance.Company.Name,
                Note = s.Note
            })
            .FirstOrDefault();
            return protesto;
        }
        public void AddProtesto(ProtestoAddModel model)
        {
            var protesto = model.ToEntity();
            _db.Protesto.Add(protesto);
            _db.SaveChanges();
        }
        public IEnumerable<NewsItem> GetNewsList(int year, int month)
        {
            var data = (from n in _db.News
                        join r in _db.ResistanceNews on n.Id equals r.NewsId into rnd
                        from rn in rnd.DefaultIfEmpty()
                        where n.Date.Year == year && n.Date.Month == month && n.Status != COMMON.Constants.Enums.Status.Passive
                        select new NewsItem
                        {
                            Id = n.Id,
                            Header = n.Header,
                            Content = n.Content,
                            Date = n.Date,
                            Link = n.Link,
                            Added = rn != null
                        }
            )
            .OrderBy(s => s.Date)
            .Distinct()
            .ToList();
            return data;
        }
        public string GetNewsContent(int newsId)
        {
            return _db.News.Where(n => n.Id == newsId).Select(s => s.Content).Single();
        }
        public string GetResistanceName(int id)
        {
            return _db.Resistance.Where(r => r.Id == id).Select(s => s.Company.Name).FirstOrDefault();
        }


        private void BindTradeUnionId(Resistance resistance, string tradeUnionId)
        {
            if (!String.IsNullOrEmpty(tradeUnionId))
            {
                int tid;
                var parseResult = int.TryParse(tradeUnionId, out tid);
                if (parseResult)
                {
                    resistance.TradeUnionId = tid;
                }
                else
                {
                    var tradeUnion = _db.TradeUnion.Add(new TradeUnion { Name = tradeUnionId });
                    resistance.TradeUnionId = tradeUnion.Entity.Id;
                }
            }
        }

        private void BindProtestoType(Protesto protesto, List<string> protestoTypeIds)
        {
            if (protestoTypeIds != null)
            {
                foreach (var typeItem in protestoTypeIds)
                {
                    int tid;
                    var parseResult = int.TryParse(typeItem, out tid);
                    if (parseResult)
                    {
                        protesto.ProtestoProtestoTypes.Add(new ProtestoProtestoType { ProtestoTypeId = tid, ProtestoId = protesto.Id });
                    }
                    else
                    {
                        var pType = _db.ProtestoType.Add(new ProtestoType { Name = typeItem });
                        protesto.ProtestoProtestoTypes.Add(new ProtestoProtestoType { ProtestoTypeId = pType.Entity.Id });
                    }
                }
            }
        }
        private void BindProtestoPlace(Protesto protesto, List<string> protestoPlaceIds)
        {
            if (protestoPlaceIds != null)
            {
                foreach (var placeItem in protestoPlaceIds)
                {
                    int pid;
                    var parseResult = int.TryParse(placeItem, out pid);
                    if (parseResult)
                    {
                        protesto.ProtestoProtestoPlaces.Add(new ProtestoProtestoPlace { ProtestoPlaceId = pid, ProtestoId = protesto.Id });
                    }
                    else
                    {
                        var pType = _db.ProtestoPlace.Add(new ProtestoPlace { Name = placeItem });
                        protesto.ProtestoProtestoPlaces.Add(new ProtestoProtestoPlace { ProtestoPlaceId = pType.Entity.Id });
                    }
                }
            }
        }
     

        private void BindCorporation(Resistance resistance, List<string> corporationIds)
        {
            if (corporationIds != null)
            {
                foreach (var corporationItem in corporationIds)
                {
                    int cid;
                    var parseResult = int.TryParse(corporationItem, out cid);
                    if (parseResult)
                    {
                        resistance.ResistanceCorporations.Add(new ResistanceCorporation { CorporationId = cid, ResistanceId = resistance.Id });
                    }
                    else
                    {
                        var corporation = _db.Corporation.Add(new Corporation { Name = corporationItem });
                        resistance.ResistanceCorporations.Add(new ResistanceCorporation { CorporationId = corporation.Entity.Id });
                    }
                }
            }
        }
        private void BindResistanceReason(Resistance resistance, List<string> reasonIds)
        {
            if (reasonIds != null)
            {
                foreach (var reasonItem in reasonIds)
                {
                    int rid;
                    var parseResult = int.TryParse(reasonItem, out rid);
                    if (parseResult)
                    {
                        resistance.ResistanceResistanceReasons.Add(new ResistanceResistanceReason { ResistanceReasonId = rid, ResistanceId = resistance.Id });
                    }
                    else
                    {
                        var reason = _db.ResistanceReason.Add(new ResistanceReason { Name = reasonItem });
                        resistance.ResistanceResistanceReasons.Add(new ResistanceResistanceReason { ResistanceReasonId = reason.Entity.Id });
                    }
                }
            }
        }

        public void DeleteProtesto(ProtestoDeleteModel model)
        {
            var protesto = _db.Protesto.Find(model.ProtestoId);
            protesto.Updater = model.UserName;
            protesto.Deleted = true;
            _db.Entry(protesto).State = EntityState.Modified;
            _db.SaveChanges();
        }

        public void DeleteResistance(ResistanceDeleteModel model)
        {
            var protesto = _db.Resistance.Find(model.ResistanceId);
            protesto.Updater = model.UserName;
            protesto.Deleted = true;
            _db.Entry(protesto).State = EntityState.Modified;
            _db.SaveChanges();
        }
    }
}