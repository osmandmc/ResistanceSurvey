using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using RS.COMMON;
using RS.COMMON.DTO;
using RS.COMMON.Entities;
using RS.EF;
using RS.MVC.Models;

namespace RS.MVC.Applications
{
    public interface IResistanceApplication
    {
        PagedResult<ResistanceIndexDto> GetPaged(FilterModel filter);
        IEnumerable<ResistanceIndexDto> GetAll();
        void Create(ResistanceCreateModel model);
        CompanyReturnDto CreateCompany(CompanyCreateViewModel model);
        CompanyReturnDto CreateOutsourceCompany(CompanyCreateViewModel model);
        Resistance ExistingResistance(int companyId, int categoryId);
        ResistanceEditModel GetResistanceDetail(int id);
        void AddProtesto(ProtestoAddModel viewModel);
        ProtestoEditModel GetProtestoDetail(int id);
        void UpdateResistance(ResistanceEditModel viewModel);
        void UpdateProtesto(ProtestoEditModel model);
        IEnumerable<News> GetNewsList(string period);
        string GetNewsContent(int newsId);
    }
    public class ResistanceApplication : IResistanceApplication
    {
        public readonly RSDBContext _db;
        public ResistanceApplication(RSDBContext db)
        {
            _db = db;
        }
        public PagedResult<ResistanceIndexDto> GetPaged(FilterModel filter)
        {
            var query = _db.Resistance
                    .Select(r => new ResistanceIndexDto{
                        Id = r.Id,
                        CategoryName = r.Category.Name,
                        CompanyName = r.Company.Name,
                        Code = r.Code,
                        StartDate = r.Protestos.OrderByDescending(o=>o.StartDate).Select(p=>p.StartDate).First()
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
            var resistances = _db.Resistance.Select(r => new ResistanceIndexDto{
                Id = r.Id,
                CategoryName = r.Category.Name,
                CompanyName = r.Company.Name,
                Code = r.Code,
                StartDate = r.Protestos.OrderByDescending(o=>o.StartDate).Select(p=>p.StartDate).First()
            }).ToList();
            return resistances;
        }
        public void Create(ResistanceCreateModel model)
        {
            Resistance resistance = model.MapToResistanceDto();
            Protesto protesto = model.MapToProtestoDto();
            
            _db.Resistance.Add(resistance);
            protesto.ResistanceId = resistance.Id;
            _db.Protesto.Add(protesto);
            _db.SaveChanges();
        }

        public CompanyReturnDto CreateCompany(CompanyCreateViewModel model)
        {
            var comapny = _db.Company.Add(new Company{Name = model.Name, CompanyScaleId = model.ScaleId, CompanyTypeId = model.TypeId, CompanyWorkLineId = model.WorklineId, IsOutsource = false});    
            _db.SaveChanges();
            var returnModel = new CompanyReturnDto(comapny.Entity);
            return returnModel;
        }

        public CompanyReturnDto CreateOutsourceCompany(CompanyCreateViewModel model)
        {
            
            var company = _db.Company.Add(new Company
            {
                IsOutsource =true,
                CompanyScaleId = model.ScaleId,
                CompanyTypeId = model.TypeId,
                CompanyWorkLineId = model.WorklineId
            });
            var companyOutsource = _db.CompanyOutsourceCompany.Add(new CompanyOutsourceCompany
            {
                CompanyId = company.Entity.Id,
                OutsourceCompanyId = model.CompanyId.Value
            });
           
            _db.SaveChanges();
            var returnModel = new CompanyReturnDto(company.Entity);
            return returnModel;
        }

        public ResistanceEditModel GetResistanceDetail(int id)
        {
            var viewModel = _db.Resistance.Where(r=>r.Id == id).Select(s => new ResistanceEditModel
            {
                Id = s.Id,
                Code = s.Code,
                CompanyId = s.CompanyId, 
                CategoryId = s.CategoryId,
                CorporationIds = s.ResistanceCorporations.Select(rc=>rc.CorporationId).ToList(),
                EmploymentTypeIds = s.ResistanceEmploymentTypes.Select(emp=>emp.EmploymentTypeId).ToList(),
                EmployeeCount = s.EmployeeCountNumber,
                EmployeeCountId = s.EmployeeCountId,
                HasTradeUnion = s.HasTradeUnion,
                TradeUnionAuthorityId = s.TradeUnionAuthorityId,
                TradeUnionId = s.TradeUnionId,
                Protestos = s.Protestos.Select(p=> new ProtestoListModel {
                    ProtestoId = p.Id,
                    ProtestoStartDate = p.StartDate,
                    ProtestoTypes = p.ProtestoProtestoTypes.Select(pt=>pt.ProtestoType.Name).ToList()
                }).ToList()
            }).FirstOrDefault();
          
            return viewModel; 
        }
        public Resistance ExistingResistance(int companyId, int categoryId)
        {
            return _db.Resistance.FirstOrDefault(c=> c.CompanyId == companyId && c.CategoryId == categoryId);
        }

        public void UpdateResistance(ResistanceEditModel viewModel)
        {
            var resistance = new Resistance{
                Id = viewModel.Id,
                CategoryId = viewModel.CategoryId,
                CompanyId = viewModel.CompanyId,
                Code = viewModel.Code,
                EmployeeCountId = viewModel.EmployeeCountId,
                EmployeeCountNumber = viewModel.EmployeeCount,
                HasTradeUnion = viewModel.HasTradeUnion,
                TradeUnionAuthorityId = viewModel.TradeUnionAuthorityId,
                TradeUnionId = viewModel.TradeUnionId,
                ResistanceCorporations = new List<ResistanceCorporation>(),
                ResistanceEmploymentTypes = new List<ResistanceEmploymentType>()
            };
            viewModel.CorporationIds.ForEach(corp => resistance.ResistanceCorporations.Add(new ResistanceCorporation{CorporationId = corp}));
            viewModel.EmploymentTypeIds.ForEach(emp => resistance.ResistanceEmploymentTypes.Add(new ResistanceEmploymentType{EmploymentTypeId = emp}));

            _db.Resistance.Update(resistance);
            _db.SaveChanges();
        }
        public void UpdateProtesto(ProtestoEditModel model)
        {
            var protesto = model.ToEntity();
            _db.Protesto.Update(protesto);
            _db.SaveChanges();
        }

        public ProtestoEditModel GetProtestoDetail(int id)
        {
            var protesto = _db.Protesto.Where(p=>p.Id == id).Select(s=> new ProtestoEditModel{
                Id = s.Id,
                ResistanceId = s.ResistanceId,
                CustodyCount = s.CustodyCount,
                EmployeeCountInProtesto = s.EmployeeCountNumber,
                GenderId = s.GenderId,
                EmployeeCountInProtestoId = s.ProtestoEmployeeCountId,
                InterventionTypeIds = s.ProtestoInterventionTypes.Select(i=> i.InterventionTypeId).ToList(),
                ProtestoPlaceIds = s.ProtestoProtestoPlaces.Select(pp=>pp.ProtestoPlaceId).ToList(),
                ProtestoReasonIds = s.ProtestoProtestoResaons.Select(pr=>pr.ProtestoReasonId).ToList(),
                ProtestoTypeIds = s.ProtestoProtestoTypes.Select(pt=>pt.ProtestoTypeId).ToList(),
                IsAgainstProduction = s.IsAgainstProduction,
                ProtestoStartDate = s.StartDate,
                ProtestoEndDate = s.EndDate,
                DevelopRight = s.DevelopRight,
                AnyLegalIntervention = s.AnyLegalIntervention,
                LegalInterventionDesc = s.LegalInterventionDesc,
                FiredEmployeeCountByProtesto = s.FiredEmployeeCountByProtesto,
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

        public IEnumerable<News> GetNewsList(string period)
        {
           return _db.News.ToList();
        }
        public string GetNewsContent(int newsId)
        {
            return _db.News.Where(n=>n.Id == newsId).Select(s=>s.Content).Single();
        }
    }
}