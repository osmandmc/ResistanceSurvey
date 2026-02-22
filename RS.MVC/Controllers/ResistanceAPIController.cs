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
}