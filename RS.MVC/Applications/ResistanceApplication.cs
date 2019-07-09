using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using RS.COMMON;
using RS.COMMON.DTO;
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
        public ResistanceApplication(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public IEnumerable<ResistanceIndexDto> GetAll()
        {
            return _uow.ResistanceRepository.GetAll();
        }

        public void Create(ResistanceCreateModel model)
        {
            ResistanceCreateDto resistance = model.MapToResistanceDto();
            ProtestoCreateDto protesto = model.MapToProtestoDto();

            _uow.ResistanceRepository.AddResistance(resistance, protesto);
            _uow.Commit();
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