using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RS.COMMON.DTO;
using RS.COMMON.Entities;
using RS.EF;
using RS.MVC.Applications;
using RS.MVC.Models;

namespace RS.MVC.Controllers;

public class ResistanceApiController(IResistanceApplication rsApplication, RSDBContext db) : BaseController
{
    private readonly RSDBContext _db = db;
    private readonly IResistanceApplication _rsApplication = rsApplication;

     
    [Authorize]
    [HttpGet]
    public IActionResult List(ResistanceFilterModel filter)
    {
        var result = _rsApplication.GetPaged(filter);
        result.Filter = filter;
        return Json(result);
    }
    [Authorize]
    public IActionResult Get(int id)
    {
        var resistance = _rsApplication.GetResistanceDetail(id);
        return Json(resistance);
    }
    
    [Authorize]
    [HttpPost]
    public IActionResult CreateResistance([FromBody] ResistanceModel model)
    {
        if (ModelState.IsValid)
        {
            model.UserName = UserName;
            _rsApplication.Create(model);
            return Ok();
        }
        var values = ModelState.Values.Where(m => m.Errors.Count > 0).Select(s => s.Errors).ToList();
        return BadRequest(values);
    }
    
    [Authorize]
    [HttpPost]
    public IActionResult UpdateResitance([FromBody] ResistanceUpdateModel viewModel)
    {
        if (ModelState.IsValid)
        {
            viewModel.UserName = UserName;
            _rsApplication.UpdateResistance(viewModel);
            return Ok();
        }
        var values = ModelState.Values.Where(m => m.Errors.Count > 0).Select(s => s.Errors).ToList();
        return StatusCode(StatusCodes.Status400BadRequest, values);
    }
    [Authorize]
    [HttpDelete]
    public IActionResult DeleteResistance(int id)
    {
        var model = new ResistanceDeleteModel()
        {
            UserName = UserName,
            ResistanceId = id
        };
        _rsApplication.DeleteResistance(model);
        return Ok();
    }
    [Authorize]
    [HttpPost]
    public IActionResult CreateOrUpdateProtesto([FromBody] ProtestoEditModel viewModel)
    {
        if (ModelState.IsValid)
        {
            var protesto = _rsApplication.UpsertProtesto(viewModel);
            return Ok(protesto);
        }
        var values = ModelState.Values
            .Where(m => m.Errors.Count > 0)
            .Select(s => s.Errors)
            .ToList();
        return StatusCode(StatusCodes.Status400BadRequest, values);
    }
    
    [Authorize]
    [HttpDelete]
    public IActionResult DeleteProtesto(int id)
    {
        var viewModel = new ProtestoDeleteModel(){ ProtestoId = id, UserName = "TO DO" };
        _rsApplication.DeleteProtesto(viewModel);
        return Ok();
    }
    
    public ActionResult Export(ResistanceFilterModel filter)
    {
        var protestos = _db.Protesto
            .Include(item => item.Resistance)
            .ThenInclude(resistance => resistance.EmployeeCount)
            .Include("Resistance.Category")
            .Include("Resistance.Company.CompanyWorkLine")
            .Include("Resistance.MainCompany.CompanyWorkLine")
            .Include("Resistance.ResistanceEmploymentTypes")
            .Include("Resistance.ResistanceEmploymentTypes.EmploymentType")
            .Include("Resistance.TradeUnion")
            .Include("Resistance.TradeUnionAuthority")
            .Include("Resistance.ResistanceResistanceReasons")
            .Include("Resistance.ResistanceResistanceReasons.ResistanceReason")
            .Include("Resistance.ResistanceCorporations")
            .Include("Resistance.ResistanceCorporations.Corporation")
            .Include("Resistance.ResistanceCorporations.Corporation.CorporationType")
            .Include("Gender")
            .Include("ProtestoEmployeeCount")
            .Include("ProtestoProtestoTypes")
            .Include("ProtestoProtestoTypes.ProtestoType")
            .Include("ProtestoProtestoPlaces")
            .Include("ProtestoProtestoPlaces.ProtestoPlace")
            .Include("ProtestoInterventionTypes")
            .Include("ProtestoInterventionTypes.InterventionType")
            .Include("Locations")
            .Include("Locations.City")
            .Include("Locations.District")
            .Include("Resistance.ResistanceNews")
            .Include("Resistance.ResistanceNews.News")
            .Where(rs => !rs.Deleted && !rs.Resistance.Deleted
                        && (filter.CompanyId == null || rs.Resistance.CompanyId == filter.CompanyId)
                        && (filter.MainCompanyId == null || rs.Resistance.MainCompanyId == filter.MainCompanyId)
                        && (filter.CategoryId == null || rs.Resistance.CategoryId == filter.CategoryId)
                        && (filter.YearId == null || rs.StartDate.Year == filter.YearId && rs.StartDate.Month == filter.MonthId)
                        && (filter.PersonalNote == null || String.IsNullOrEmpty(rs.Resistance.Note) == !filter.PersonalNote)
                        )
            .ToList();

        var rows = new List<dynamic>();
        foreach (var item in protestos)
        {
            var row = new
            {
                ResistanceId = item.Resistance.Id,
                Description = item.Resistance.Description,
                CategoryName = item.Resistance.Category.Name,
                CompanyName = item.Resistance.Target.Name,
                CompanyWorkline = ((Company)item.Resistance.Target).CompanyWorkLine != null ? 
                    ((Company)item.Resistance.Target).CompanyWorkLine.Name : 
                    "",
                MainCompanyName = item.Resistance.MainCompany != null ? item.Resistance.MainCompany.Name : "",
                MainCompanyWorkline = item.Resistance.MainCompany != null ? item.Resistance.MainCompany.CompanyWorkLine != null ? item.Resistance.MainCompany.CompanyWorkLine.Name : "" : "",
                AgainstProduction = item.ProtestoProtestoTypes.Any(s => s.ProtestoType.AgainstProduction) ? "EVET" : "HAYIR",
                DevelopRight = item.Resistance.DevelopRight ? "EVET" : "HAYIR",
                EmployeeCount = item.Resistance.EmployeeCount != null ? item.Resistance.EmployeeCount.Name : "",
                EmployeeCountNumber = item.Resistance.EmployeeCountNumber,
                HasTradeUnion = item.Resistance.ResistanceCorporations.Any(s => s.Corporation.CorporationTypeId == 1) ? "EVET" : "HAYIR",
                TradeUnionAuthority = item.Resistance.TradeUnionAuthority != null ? item.Resistance.TradeUnionAuthority.Name : "",
                ResistanceReasons = item.Resistance.ResistanceResistanceReasons.Select(x => x.ResistanceReason.Name).ToList(),
                Corporations = item.Resistance.ResistanceCorporations?.Select(x => x.Corporation.Name).ToList() ?? new List<string>(),
                EmploymentTypes = item.Resistance.ResistanceEmploymentTypes?.Select(x => x.EmploymentType.Name).ToList() ?? new List<string>(),
                TradeUnionReacted = item.Resistance.TradeUnion != null ? item.Resistance.TradeUnion.Name : "",
                Gender = item.Gender != null ? item.Gender.Name : "",
                FiredEmployeeCount = item.Resistance.FiredEmployeeCountByProtesto,
                AnyLegalIntervention = item.Resistance.AnyLegalIntervention.HasValue ? (item.Resistance.AnyLegalIntervention.Value ? "EVET" : "HAYIR") : "BİLİNMİYOR",
                LegalInterventionDesc = item.Resistance.LegalInterventionDesc,
                ProtestoId = item.Id,
                ProtestoCount = item.Locations.Count,
                ProtestoTypes = item.ProtestoProtestoTypes?.Select(x => x.ProtestoType.Name).ToList() ?? new List<string>(),
                ProtestoPlaces = item.ProtestoProtestoPlaces?.Select(x => x.ProtestoPlace.Name).ToList() ?? new List<string>(),
                StartDate = item.StartDate.ToShortDateString(),
                EndDate = item.EndDate != null ? item.EndDate.Value.ToShortDateString() : "",
                StrikeDuration = item.StrikeDuration,
                Locations = item.Locations.Select(x => x.District != null ? $"{x.City.Name} / {x.District.Name} {x.Place}" : $"{x.City.Name} {x.Place}").ToList(),
                EmployeeCountInProtesto = item.ProtestoEmployeeCount != null ? item.ProtestoEmployeeCount.Name : "",
                EmployeeCountInProtestoNumber = item.EmployeeCountNumber,
                InterventionTypes = item.ProtestoInterventionTypes?.Select(x => x.InterventionType.Name).ToList() ?? new List<string>(),
                CustodyCount = item.CustodyCount,
                Note = item.Resistance.Note,
                ProtestoNote = item.Note,
                ResistanceResult = item.Resistance.ResistanceResult.ToString(),
            };
            rows.Add(row);
        }

        return Json(rows);
    }
}