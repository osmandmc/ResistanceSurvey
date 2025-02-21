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
        PagedResult<ResistanceIndexDto, ResistanceFilterModel> GetPaged(ResistanceFilterModel filter);
        IEnumerable<ResistanceIndexDto> GetAll();
        void Create(ResistanceForm model);
        void Create(ResistanceModel model);
        Result<CompanyReturnDto> CreateCompany(CompanyCreateViewModel model);
        Resistance ExistingResistance(int companyId, int categoryId);
        ResistanceEditModel GetResistanceDetail(int id);
        void AddProtesto(ProtestoAddModel viewModel);
        ProtestoEditModel GetProtestoDetail(int id);
        void UpdateResistance(ResistanceEditForm viewModel);
        void UpdateResistance(ResistanceUpdateModel viewModel);
        int UpsertProtesto(ProtestoEditModel model);
        IEnumerable<NewsItem> GetNewsList(int year, int month);
        string GetNewsContent(int newsId);
        string GetResistanceName(int id);
        void DeleteProtesto(ProtestoDeleteModel model);
        void DeleteResistance(ResistanceDeleteModel viewModel);
    }
    public class ResistanceApplication(RSDBContext db) : IResistanceApplication
    {
        public PagedResult<ResistanceIndexDto, ResistanceFilterModel> GetPaged(ResistanceFilterModel filter)
        {
            var result = db.Resistance
                    .Where(rs => !rs.Deleted
                    && (filter.CompanyId == null || rs.CompanyId == filter.CompanyId)
                      && (filter.MainCompanyId == null || rs.MainCompanyId == filter.MainCompanyId)
                    && (filter.CategoryId == null || rs.CategoryId == filter.CategoryId)
                    && (filter.YearId == null || rs.Protestos.Any(s => !s.Deleted && s.StartDate.Year == filter.YearId && s.StartDate.Month == filter.MonthId))
                    && (filter.PersonalNote == null || String.IsNullOrEmpty(rs.Note) == !filter.PersonalNote)
                    )
                    .Select(r => new ResistanceIndexDto
                    {
                        Id = r.Id,
                        CategoryName = r.Category.Name,
                        CompanyName = r.Company.Name,
                        StartDate = r.StartDate
                    })
                    .OrderByDescending(x => x.StartDate)
                    .ToPagedFilteredResult(filter);
            return result;
        }
        public IEnumerable<ResistanceIndexDto> GetAll()
        {
            var resistances = db.Resistance.Select(r => new ResistanceIndexDto
            {
                Id = r.Id,
                CategoryName = r.Category.Name,
                CompanyName = r.Company.Name,
                StartDate = r.Protestos.OrderByDescending(o => o.StartDate).Select(p => p.StartDate).First()
            }).ToList();
            return resistances;
        }
        public void Create(ResistanceForm model)
        {
            if (model.Resistance.CompanyId == -1)
            {
                var newCompany = db.Company.Add(new Company { Name = model.Company.Name, CompanyScaleId = model.Company.ScaleId, CompanyTypeId = model.Company.TypeId, CompanyWorkLineId = model.Company.WorklineId });
                model.Resistance.CompanyId = newCompany.Entity.Id;
            }
            if (model.Resistance.MainCompanyId == -1)
            {
                var newCompany = db.Company.Add(new Company { Name = model.MainCompany.Name, CompanyScaleId = model.MainCompany.ScaleId, CompanyTypeId = model.MainCompany.TypeId, CompanyWorkLineId = model.MainCompany.WorklineId });
                model.Resistance.MainCompanyId = newCompany.Entity.Id;
                db.CompanyOutsourceCompany.Add(new CompanyOutsourceCompany { CompanyId = model.Resistance.MainCompanyId.Value, OutsourceCompanyId = model.Resistance.CompanyId });
            }
            Resistance resistance = model.Resistance.MapToResistanceDto();
            
            if (model.Resistance.ResistanceNewsIds != null)
                model.Resistance.ResistanceNewsIds.Where(n => !n.IsDeleted).ToList().ForEach(news => resistance.ResistanceNews.Add(new ResistanceNews { NewsId = news.Id }));

            Protesto protesto = model.Resistance.MapToProtestoDto();
            db.Resistance.Add(resistance);
            protesto.ResistanceId = resistance.Id;
            db.Protesto.Add(protesto);
            db.SaveChanges();
        }
        public void Create(ResistanceModel model)
        {
            if(model.MainCompanyId.HasValue)
                db.CompanyOutsourceCompany.Add(
                    new CompanyOutsourceCompany
                    {
                        CompanyId = model.MainCompanyId.Value, 
                        OutsourceCompanyId = model.CompanyId
                    });
            
            Resistance resistance = model.MapToResistance();
        
            BindCorporations(resistance, model.CorporationIds);
            BindResistanceReasons(resistance, model.ResistanceReasonIds);
            BindEmploymentTypes(resistance, model.EmploymentTypeIds);
            
            if (model.ResistanceNews != null)
                model.ResistanceNews
                    .ToList()
                    .ForEach(news => 
                        resistance.ResistanceNews.Add(new ResistanceNews { NewsId = news.Id }));

            Protesto protesto = model.MapToProtestoDto();
            BindProtestoTypes(protesto, model.Protesto.ProtestoTypeIds);
            BindProtestoPlaces(protesto, model.Protesto.ProtestoPlaceIds);
            BindProtestoInterventionTypes(protesto, model.Protesto.InterventionTypeIds);
          
            resistance.Protestos.Add(protesto);
            db.Resistance.Add(resistance);
            db.SaveChanges();
        }
        public Result<CompanyReturnDto> CreateCompany(CompanyCreateViewModel model)
        {
            var result = new Result<CompanyReturnDto>();
            if (db.Company.Any(s => s.Name == model.Name && !s.Deleted))
            {
                result.Success = false;
            }
            else
            {

                var comapny = db.Company.Add(new Company { Name = model.Name, CompanyScaleId = model.ScaleId, CompanyTypeId = model.TypeId, CompanyWorkLineId = model.WorklineId });
                db.SaveChanges();
                result.Success = true;
                var returnModel = new CompanyReturnDto(comapny.Entity);
                result.Response = returnModel;
            }

            return result;
        }
        public ResistanceEditModel GetResistanceDetail(int id)
        {
            var data = (from s in db.Resistance
                        join coc in db.CompanyOutsourceCompany on s.CompanyId equals coc.OutsourceCompanyId into oc
                        from o in oc.DefaultIfEmpty()
                        where s.Id == id && !s.Deleted
                        select new ResistanceEditModel
                        {
                            Id = s.Id,
                            Code = s.Code,
                            CompanyId = s.CompanyId,
                            MainCompanyId = s.MainCompanyId,
                            CategoryId = s.CategoryId,
                            CorporationIds = s.ResistanceCorporations.Select(emp => emp.Corporation).ToList(),
                            EmploymentTypeIds = s.ResistanceEmploymentTypes.Select(emp => emp.EmploymentType).ToList(),
                            ResistanceReasonIds = s.ResistanceResistanceReasons.Select(emp => emp.ResistanceReason).ToList(),
                            EmployeeCount = s.EmployeeCountNumber,
                            EmployeeCountId = s.EmployeeCountId,
                            HasTradeUnion = s.HasTradeUnion,
                            DevelopRight = s.DevelopRight,
                            TradeUnionAuthorityId = s.TradeUnionAuthorityId,
                            TradeUnionId = s.TradeUnionId,
                            LegalInterventionDesc = s.LegalInterventionDesc,
                            ResistanceDescription = s.Description,
                            ResistanceNote = s.Note,
                            FiredEmployeeCountByProtesto = s.FiredEmployeeCountByProtesto,
                            ResistanceResult = s.ResistanceResult,
                            ResistanceNews = s.ResistanceNews.OrderBy(s => s.News.Date).Select(r => r.News).ToList(),
                            ProtestoItems = s.Protestos.Where(s => !s.Deleted).OrderBy(s=>s.StartDate)
                                .Select(s => new ProtestoEditModel
                                {
                                    ProtestoId = s.Id,
                                    ResistanceId = s.ResistanceId,
                                    CustodyCount = s.CustodyCount,
                                    EmployeeCountInProtesto = s.EmployeeCountNumber,
                                    GenderId = s.GenderId,
                                    EmployeeCountInProtestoId = s.ProtestoEmployeeCountId,
                                    SimpleProtestoDescription = s.SimpleProtestoDescription,
                                    StrikeDuration = s.StrikeDuration,
                                    InterventionTypeIds = s.ProtestoInterventionTypes.Select(pt => pt.InterventionType).ToList(),
                                    ProtestoPlaceIds = s.ProtestoProtestoPlaces.Select(pt => pt.ProtestoPlace).ToList(),
                                    ProtestoTypeIds = s.ProtestoProtestoTypes.Select(pt => pt.ProtestoType).ToList(),
                                    Locations = s.Locations.Select(loc => new ProtestoLocationModel
                                    {
                                        Id = loc.Id,
                                        ProtestoId = loc.ProtestoId,
                                        CityId = loc.CityId,
                                        DistrictId = loc.DistrictId,
                                        Place = loc.Place,
                                        Deleted = false
                                    }).ToList(),
                                    ProtestoDistrictIds = s.Districts != null ? s.Districts.ToList() : null,
                                    ProtestoStartDate = s.StartDate,
                                    ProtestoEndDate = s.EndDate,
                                    ResistanceName = s.Resistance.Company.Name,
                                    Note = s.Note
                                })
                                .ToList()
                            
                        })
            .FirstOrDefault();
            return data;
        }
        public Resistance ExistingResistance(int companyId, int categoryId)
        {
            return db.Resistance.FirstOrDefault(c => c.CompanyId == companyId && c.CategoryId == categoryId);
        }
        public void UpdateResistance(ResistanceEditForm form)
        {
            var viewModel = form.Resistance;
            var resistance = db.Resistance.SingleOrDefault(r => r.Id == viewModel.Id);
            if (viewModel.CompanyId == -1)
            {
                var newCompany = db.Company.Add(new Company { Name = form.Company.Name, CompanyScaleId = form.Company.ScaleId, CompanyTypeId = form.Company.TypeId, CompanyWorkLineId = form.Company.WorklineId });
                viewModel.CompanyId = newCompany.Entity.Id;
            }
            if (viewModel.MainCompanyId == -1)
            {
                var newCompany = db.Company.Add(new Company { Name = form.MainCompany.Name, CompanyScaleId = form.MainCompany.ScaleId, CompanyTypeId = form.MainCompany.TypeId, CompanyWorkLineId = form.MainCompany.WorklineId });
                viewModel.MainCompanyId = newCompany.Entity.Id;
                db.CompanyOutsourceCompany.Add(new CompanyOutsourceCompany { CompanyId = viewModel.MainCompanyId.Value, OutsourceCompanyId = viewModel.CompanyId });
            }
            if (viewModel.MainCompanyId != null && viewModel.MainCompanyId != 0)
            {
                var exist = db.CompanyOutsourceCompany.Any(s => s.CompanyId == viewModel.MainCompanyId && s.OutsourceCompanyId == viewModel.CompanyId);
                if (!exist)
                {
                    db.CompanyOutsourceCompany.Add(new CompanyOutsourceCompany { CompanyId = viewModel.MainCompanyId.Value, OutsourceCompanyId = viewModel.CompanyId });
                }
            }
            resistance.Id = viewModel.Id;
            resistance.CategoryId = viewModel.CategoryId;
            resistance.CompanyId = viewModel.CompanyId;
            resistance.MainCompanyId = viewModel.MainCompanyId;
            resistance.Code = viewModel.Code;
            resistance.EmployeeCountId = viewModel.EmployeeCountId;
            resistance.EmployeeCountNumber = viewModel.EmployeeCount;
            resistance.HasTradeUnion = viewModel.HasTradeUnion;
            resistance.TradeUnionAuthorityId = viewModel.TradeUnionAuthorityId;
            resistance.TradeUnionId = viewModel.TradeUnionId;
            resistance.Description = viewModel.ResistanceDescription;
            resistance.Note = viewModel.ResistanceNote;
            resistance.LegalInterventionDesc = viewModel.LegalInterventionDesc;
            resistance.FiredEmployeeCountByProtesto = viewModel.FiredEmployeeCountByProtesto;
            resistance.DevelopRight = viewModel.DevelopRight;
            resistance.ResistanceResult = viewModel.ResistanceResult;
            resistance.Updater = viewModel.UserName;
            resistance.UpdateDate = DateTime.Now;
            resistance.ResistanceCorporations = new List<ResistanceCorporation>();
            resistance.ResistanceEmploymentTypes = new List<ResistanceEmploymentType>();
            resistance.ResistanceResistanceReasons = new List<ResistanceResistanceReason>();
            resistance.ResistanceNews = new List<ResistanceNews>();
            if (viewModel.AnyLegalIntervention == 1) resistance.AnyLegalIntervention = true;
            if (viewModel.AnyLegalIntervention == 2) resistance.AnyLegalIntervention = false;
            var rCorporations = db.ResistanceCorporation.Where(r => r.ResistanceId == resistance.Id).ToList();
            db.ResistanceCorporation.RemoveRange(rCorporations);
            if (viewModel.EmploymentTypeIds != null)
                viewModel.EmploymentTypeIds.ForEach(emp => resistance.ResistanceEmploymentTypes.Add(new ResistanceEmploymentType
                {
                    EmploymentType = emp
                }));
            var rEmpl = db.ResistanceEmploymentType.Where(r => r.ResistanceId == resistance.Id).ToList();
            db.ResistanceEmploymentType.RemoveRange(rEmpl);

            var rReason = db.ResistanceResistanceReason.Where(r => r.ResistanceId == resistance.Id).ToList();
            db.ResistanceResistanceReason.RemoveRange(rReason);
          
            var rNews = db.ResistanceNews.Where(r => r.ResistanceId == resistance.Id).ToList();
            db.ResistanceNews.RemoveRange(rNews);
            //BindTradeUnionId(resistance, viewModel.TradeUnionId);
            //BindCorporation(resistance, viewModel.CorporationIds);
            //BindResistanceReason(resistance, viewModel.ResistanceReasonIds);
            db.Resistance.Update(resistance);
            db.SaveChanges();
        }
        public void UpdateResistance(ResistanceUpdateModel viewModel)
        {
            var resistance = db.Resistance.SingleOrDefault(r => r.Id == viewModel.Id);
            resistance.CategoryId = viewModel.CategoryId;
            resistance.CompanyId = viewModel.CompanyId;
            resistance.MainCompanyId = viewModel.MainCompanyId;
            resistance.Code = viewModel.Code;
            resistance.EmployeeCountId = viewModel.EmployeeCountId;
            resistance.EmployeeCountNumber = viewModel.EmployeeCount;
            resistance.HasTradeUnion = viewModel.HasTradeUnion;
            resistance.TradeUnionAuthorityId = viewModel.TradeUnionAuthorityId;
            resistance.TradeUnionId = viewModel.TradeUnionId;
            resistance.Description = viewModel.ResistanceDescription;
            resistance.Note = viewModel.Note;
            //resistance.LegalInterventionDesc = viewModel.LegalInterventionDesc;
            //resistance.FiredEmployeeCountByProtesto = viewModel.FiredEmployeeCountByProtesto;
            //resistance.DevelopRight = viewModel.DevelopRight;
            // resistance.ResistanceResult = viewModel.ResistanceResult;
            resistance.Updater = viewModel.UserName;
            resistance.UpdateDate = DateTime.Now;
            resistance.ResistanceCorporations = new List<ResistanceCorporation>();
            resistance.ResistanceEmploymentTypes = new List<ResistanceEmploymentType>();
            resistance.ResistanceResistanceReasons = new List<ResistanceResistanceReason>();
            resistance.ResistanceNews = new List<ResistanceNews>();
            // if (viewModel.AnyLegalIntervention == 1) resistance.AnyLegalIntervention = true;
            // if (viewModel.AnyLegalIntervention == 2) resistance.AnyLegalIntervention = false;
            var rCorporations = db.ResistanceCorporation.Where(r => r.ResistanceId == resistance.Id).ToList();
            db.ResistanceCorporation.RemoveRange(rCorporations);
            
            
            if (viewModel.EmploymentTypeIds != null)
                viewModel.EmploymentTypeIds.ForEach(emp => resistance.ResistanceEmploymentTypes.Add(new ResistanceEmploymentType
                {
                    EmploymentTypeId = emp.Id
                }));
            var rEmpl = db.ResistanceEmploymentType.Where(r => r.ResistanceId == resistance.Id).ToList();
            db.ResistanceEmploymentType.RemoveRange(rEmpl);

            var rReason = db.ResistanceResistanceReason.Where(r => r.ResistanceId == resistance.Id).ToList();
            db.ResistanceResistanceReason.RemoveRange(rReason);
         
            var rNews = db.ResistanceNews.Where(r => r.ResistanceId == resistance.Id).ToList();
            db.ResistanceNews.RemoveRange(rNews);
            
            BindCorporations(resistance, viewModel.CorporationIds);
            BindResistanceReasons(resistance, viewModel.ResistanceReasonIds);
            if (viewModel.ResistanceNews != null)
                viewModel.ResistanceNews
                    .ToList()
                    .ForEach(news => 
                        resistance.ResistanceNews.Add(new ResistanceNews { NewsId = news.Id }));

            db.Resistance.Update(resistance);
            db.SaveChanges();
        }
        public int UpsertProtesto(ProtestoEditModel model)
        {
            var protesto = model.ToEntity();
          
            db.ProtestoInterventionType.RemoveRange(db.ProtestoInterventionType.Where(i => i.ProtestoId == model.ProtestoId).ToList());
            db.ProtestoProtestoPlace.RemoveRange(db.ProtestoProtestoPlace.Where(i => i.ProtestoId == model.ProtestoId).ToList());
            db.ProtestoProtestoType.RemoveRange(db.ProtestoProtestoType.Where(i => i.ProtestoId == model.ProtestoId).ToList());
            db.ProtestoLocation.RemoveRange(db.ProtestoLocation.Where(i => i.ProtestoId == model.ProtestoId).ToList());
            
            db.ProtestoLocation.AddRange(protesto.Locations);
            
            BindProtestoTypes(protesto, model.ProtestoTypeIds);
            BindProtestoPlaces(protesto, model.ProtestoPlaceIds);
            BindProtestoInterventionTypes(protesto, model.InterventionTypeIds);
            
            
            if (protesto.Id == 0)
            {
                db.Protesto.Add(protesto);    
            }
            else
            {
                db.Protesto.Update(protesto);    
            }

            var resistance = db.Resistance.Find(model.ResistanceId);
            var hasMinStartDate = db.Protesto.Where(s => !s.Deleted && s.ResistanceId == model.ResistanceId && s.Id != model.ProtestoId).Any();
            if (!hasMinStartDate || model.ProtestoStartDate < resistance.StartDate)
            {
                resistance.StartDate = protesto.StartDate;
                db.Resistance.Update(resistance);
            }
            
            db.SaveChanges();
            return protesto.Id;
        }
        public ProtestoEditModel GetProtestoDetail(int id)
        {
            var protesto = db.Protesto.Where(p => p.Id == id).Select(s => new ProtestoEditModel
            {
                ProtestoId = s.Id,
                ResistanceId = s.ResistanceId,
                CustodyCount = s.CustodyCount,
                EmployeeCountInProtesto = s.EmployeeCountNumber,
                GenderId = s.GenderId,
                EmployeeCountInProtestoId = s.ProtestoEmployeeCountId,
                SimpleProtestoDescription = s.SimpleProtestoDescription,
                StrikeDuration = s.StrikeDuration,
                InterventionTypeIds = s.ProtestoInterventionTypes.Select(pt => pt.InterventionType).ToList(),
                ProtestoPlaceIds = s.ProtestoProtestoPlaces.Select(pt => pt.ProtestoPlace).ToList(),
                ProtestoTypeIds = s.ProtestoProtestoTypes.Select(pt => pt.ProtestoType).ToList(),
                Locations = s.Locations.Select(loc => new ProtestoLocationModel
                {
                    ProtestoId = loc.ProtestoId,
                    CityId = loc.CityId,
                    DistrictId = loc.DistrictId,
                    // CityName = loc.City.Name,
                    // DistirctName = loc.District != null ? loc.District.Name : "",
                    Place = loc.Place,
                    Deleted = false
                }).ToList(),
                ProtestoDistrictIds = s.Districts != null ? s.Districts.ToList() : null,
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
            db.Protesto.Add(protesto);
            var res = db.Resistance.FirstOrDefault(s=>s.Id == model.ResistanceId);
            if (res.StartDate == default(DateTime) || model.ProtestoStartDate < res.StartDate) {
                res.StartDate = model.ProtestoStartDate;
                db.Resistance.Update(res);
            }
            db.SaveChanges();
        }
        public IEnumerable<NewsItem> GetNewsList(int year, int month)
        {
            var data = (from n in db.News
                        join r in db.ResistanceNews on n.Id equals r.NewsId into rnd
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
            return db.News.Where(n => n.Id == newsId).Select(s => s.Content).Single();
        }
        public string GetResistanceName(int id)
        {
            return db.Resistance.Where(r => r.Id == id).Select(s => s.Company.Name).FirstOrDefault();
        }
        
        private void BindProtestoTypes(Protesto protesto, List<ProtestoType> protestoTypes)
        {
            if (protestoTypes != null)
            {
                foreach (var typeItem in protestoTypes)
                {
                    if (typeItem.Id != -1)
                    {
                        protesto.ProtestoProtestoTypes.Add(new ProtestoProtestoType { ProtestoTypeId = typeItem.Id, ProtestoId = protesto.Id });
                    }
                    else
                    {
                        var pType = new ProtestoType { Name = typeItem.Name };
                        db.ProtestoType.Add(pType);
                        protesto.ProtestoProtestoTypes.Add(new ProtestoProtestoType { ProtestoType = pType });
                    }
                }
            }
        }
        private void BindProtestoPlaces(Protesto protesto, List<ProtestoPlace> protestoPlaces)
        {
            if (protestoPlaces != null)
            {
                foreach (var placeItem in protestoPlaces)
                {
                    if (placeItem.Id != -1)
                    {
                        protesto.ProtestoProtestoPlaces.Add(new ProtestoProtestoPlace { ProtestoPlaceId = placeItem.Id, ProtestoId = protesto.Id });
                    }
                    else
                    {
                        var pType = new ProtestoPlace { Name = placeItem.Name };
                        db.ProtestoPlace.Add(pType);
                        protesto.ProtestoProtestoPlaces.Add(new ProtestoProtestoPlace { ProtestoPlace = pType });
                    }
                }
            }
        }
        private void BindProtestoInterventionTypes(Protesto protesto, List<InterventionType> interventionTypes)
        {
            if (interventionTypes != null)
            {
                foreach (var item in interventionTypes)
                {
                    if (item.Id != -1)
                    {
                        protesto.ProtestoInterventionTypes.Add(new ProtestoInterventionType { InterventionTypeId = item.Id, ProtestoId = protesto.Id });
                    }
                    else
                    {
                        var iType = new InterventionType { Name = item.Name };
                        db.InterventionType.Add(iType);
                        protesto.ProtestoInterventionTypes.Add(new ProtestoInterventionType() { InterventionType = iType });
                    }
                }
            }
        }
        private void BindCorporations(Resistance resistance, List<Corporation> corporations)
        {
            if (corporations != null)
            {
                foreach (var corporationItem in corporations)
                {
                    if (corporationItem.Id != -1)
                    {
                        resistance.ResistanceCorporations.Add(new ResistanceCorporation { CorporationId = corporationItem.Id, ResistanceId = resistance.Id });
                    }
                    else
                    {
                        var corporation = new Corporation { Name = corporationItem.Name };
                        db.Corporation.Add(corporation);
                        resistance.ResistanceCorporations.Add(new ResistanceCorporation { Corporation = corporation });
                    }
                }
            }
        }
        private void BindResistanceReasons(Resistance resistance, List<ResistanceReason> reasons)
        {
            if (reasons != null)
            {
                foreach (var reasonItem in reasons)
                {
                    if (reasonItem.Id != -1)
                    {
                        resistance.ResistanceResistanceReasons.Add(new ResistanceResistanceReason { ResistanceReasonId = reasonItem.Id, ResistanceId = resistance.Id });
                    }
                    else
                    {
                        var reason =new ResistanceReason { Name = reasonItem.Name };
                        db.ResistanceReason.Add(reason);
                        resistance.ResistanceResistanceReasons.Add(new ResistanceResistanceReason{ ResistanceReason = reason});
                    }
                }
            }
        }
        private void BindEmploymentTypes(Resistance resistance, List<EmploymentType> employmentTypes)
        {
            if (employmentTypes != null)
            {
                foreach (var item in employmentTypes)
                {
                    if (item.Id != -1)
                    {
                        resistance.ResistanceEmploymentTypes.Add(new ResistanceEmploymentType { EmploymentTypeId = item.Id, ResistanceId = resistance.Id });
                    }
                    else
                    {
                        var employmentType =new EmploymentType { Name = item.Name };
                        db.EmploymentType.Add(employmentType);
                        resistance.ResistanceEmploymentTypes.Add(new ResistanceEmploymentType{ EmploymentType = employmentType});
                    }
                }
            }
        }

        
        public void DeleteProtesto(ProtestoDeleteModel model)
        {
            var protesto = db.Protesto.Find(model.ProtestoId);
            protesto.Updater = model.UserName;
            protesto.Deleted = true;
            var res = db.Resistance.FirstOrDefault(s => s.Id == protesto.ResistanceId);
            var minProtestoStartDate = db.Protesto.Where(x => !x.Deleted && x.Id != model.ProtestoId && x.ResistanceId == res.Id).Select(s => s.StartDate);
            res.StartDate = !minProtestoStartDate.Any() ? default(DateTime): minProtestoStartDate.Min();
            db.Resistance.Update(res);
            db.Entry(protesto).State = EntityState.Modified;
            db.SaveChanges();
        }
        public void DeleteResistance(ResistanceDeleteModel model)
        {
            var resistance = db.Resistance.Find(model.ResistanceId);
            resistance.Updater = model.UserName;
            resistance.Deleted = true;
            db.Entry(resistance).State = EntityState.Modified;
            db.SaveChanges();
        }
    }
}