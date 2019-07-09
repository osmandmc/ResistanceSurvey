using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using RS.COMMON;
using RS.COMMON.DTO;
using RS.COMMON.Entities;
using RS.EF;
using RS.MVC.Models;

namespace RS.MVC.Applications
{
    public interface IResistanceApplication
    {
        IEnumerable<ResistanceIndexDto> GetAll();
        void Create(ResistanceCreateModel model);
        CompanyReturnDto CreateCompany(CompanyCreateViewModel model);
        CompanyReturnDto CreateOutsourceCompany(CompanyCreateViewModel model);
        ResistanceDto ExistingResistance(int companyId, int categoryId);
        ResistanceEditModel GetResistanceDetail(int id);
    }
    public class ResistanceApplication : IResistanceApplication
    {
        public readonly IUnitOfWork _uow;
        public readonly RSDBContext _db;
        public ResistanceApplication(IUnitOfWork uow, RSDBContext db)
        {
            _uow = uow;
            _db = db;
        }

        public IEnumerable<ResistanceIndexDto> GetAll()
        {
            var foo = _db.City.ToList();
            return _uow.ResistanceRepository.GetAll();
        }

        public void Create(ResistanceCreateModel model)
        {
            Resistance resistance = model.MapToResistanceDto();
            Protesto protesto = model.MapToProtestoDto();
            protesto.Resistance = resistance;
            _db.Resistance.Add(resistance);
            _db.Protesto.Add(protesto);
            _db.SaveChanges();

            // _uow.ResistanceRepository.AddResistance(resistance, protesto);
            // _uow.Commit();
        }

        public CompanyReturnDto CreateCompany(CompanyCreateViewModel model)
        {
            var returnModel = new CompanyReturnDto();
          
                CompanyDto company = new CompanyDto
                {
                    Name = model.Name,
                    ScaleId = model.ScaleId,
                    TypeId = model.TypeId,
                    WorklineId = model.WorklineId
                };
                var foo = _uow.CompanyRepository.AddCompany(company);
                returnModel.CompanyId = foo;
                returnModel.CompanyName = model.Name;
            
            _uow.Commit();
            return returnModel;
        }

        public CompanyReturnDto CreateOutsourceCompany(CompanyCreateViewModel model)
        {
             var returnModel = new CompanyReturnDto();
          
                CompanyDto company = new CompanyDto
                {
                    OutsourceEmployerMainCompanyId = model.CompanyId,
                    ScaleId = model.ScaleId,
                    TypeId = model.TypeId,
                    WorklineId = model.WorklineId
                };
                var foo = _uow.CompanyRepository.AddOutsourceCompany(company);
                returnModel.CompanyId = foo;
                returnModel.CompanyName = model.Name;
            
            
            _uow.Commit();
            return returnModel;
        }

        public ResistanceEditModel GetResistanceDetail(int id)
        {
            var dto = _uow.ResistanceRepository.GetResistanceDetail(id);
            var editModel = new ResistanceEditModel(dto); 
            return editModel; 
        }
        public ResistanceDto ExistingResistance(int companyId, int categoryId)
        {
            return _uow.ResistanceRepository.ExistingResistance(companyId, categoryId);
        }
    }
}