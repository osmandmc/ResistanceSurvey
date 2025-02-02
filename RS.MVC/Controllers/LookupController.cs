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
    // GET
    [Authorize]
    public IActionResult EmployeeCounts()
    {
        var employeeCount = _db.EmployeeCount
            .OrderBy(s => s.Name)
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
    

}
