using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RS.COMMON.Entities.LookupEntity;
using RS.EF;

namespace RS.MVC.Controllers;

public class LookupController : Controller
{
    private readonly RSDBContext _db;
    public LookupController(RSDBContext db)
    {
        _db = db;
    }
    
    [Authorize]
    public IActionResult Categories()
    {
        var categories = _db.Category.OrderBy(s => s.Name).Select(s => new LookupEntity { Id = s.Id, Name = s.Name }).ToList();
        return Json(categories);
    }
    // GET
    [Authorize]
    public IActionResult EmployeeCounts()
    {
        var employeeCount = _db.EmployeeCount
            .Select(s => new LookupEntity { Id = s.Id, Name = s.Name })
            .ToList();
        return Json(employeeCount);
    }
    
    [Authorize]
    public IActionResult EmployeeCountInProtesto()
    {
        var employeeCount = _db.ProtestoEmployeeCount
            .Select(s => new LookupEntity { Id = s.Id, Name = s.Name })
            .ToList();
        return Json(employeeCount);
    }
    
    [Authorize]
    public IActionResult ResistanceReasons()
    {
        var resistanceReasons = _db.ResistanceReason
            .OrderBy(s=>s.Name)
            .ToList();
        return Json(resistanceReasons);
    }
    [Authorize]
    public IActionResult Genders()
    {
        var genders = _db.Gender
            .OrderBy(s=>s.Name)
            .ToList();
        return Json(genders);
    }
    
    [Authorize]
    public IActionResult CompanyTypes()
    {
        var lookup = _db.CompanyType
            .OrderBy(s=>s.Name)
            .ToList();
        return Json(lookup);
    }
    [Authorize]
    public IActionResult CompanyScales()
    {
        var lookup = _db.CompanyScale
            .OrderBy(s=>s.Name)
            .ToList();
        return Json(lookup);
    }
    [Authorize]
    public IActionResult CompanyWorklines()
    {
        var lookup = _db.CompanyWorkLine
            .OrderBy(s=>s.Name)
            .ToList();
        return Json(lookup);
    }
    [Authorize]
    public IActionResult TradeUnions()
    {
        var lookup = _db.TradeUnion
            .OrderBy(s=>s.Name)
            .ToList();
        return Json(lookup);
    }
    
    [Authorize]
    public IActionResult TradeUnionAuthorities()
    {
        var lookup = _db.TradeUnionAuthority
            .OrderBy(s=>s.Name)
            .ToList();
        return Json(lookup);
    }
    [Authorize]
    public IActionResult EmploymentTypes()
    {
        var lookup = _db.EmploymentType
            .OrderBy(s=>s.Name)
            .ToList();
        return Json(lookup);
    }
    [Authorize]
    public IActionResult Cities()
    {
        var lookup = _db.City
            .OrderBy(s=>s.Name)
            .ToList();
        return Json(lookup);
    }
    [Authorize]
    public IActionResult Districts()
    {
        var lookup = _db.District
            .OrderBy(s=>s.Name)
            .ToList();
        return Json(lookup);
    }
    [Authorize]
    public IActionResult InterventionTypes()
    {
        var lookup = _db.InterventionType
            .OrderBy(s=>s.Name)
            .ToList();
        return Json(lookup);
    }
   
   
}
