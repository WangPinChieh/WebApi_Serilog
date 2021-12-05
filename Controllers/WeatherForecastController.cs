using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Serilog;
using WebApi_Serilog;
using WebApi_Serilog.Models;

namespace WebApi_Serilog.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static List<AddModel> _models = new List<AddModel>();

    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<WeatherForecastController> _logger;
    private readonly IDiagnosticContext _diagnosticContext;

    public WeatherForecastController(ILogger<WeatherForecastController> logger, IDiagnosticContext diagnosticContext)
    {
        _logger = logger;
        _diagnosticContext = diagnosticContext;
    }

    [HttpGet(Name = "GetWeatherForecast")]
    [ApiLogger]
    public IEnumerable<WeatherForecast> Get()
    {
        _logger.LogInformation("Test logger");
        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
    }

    [HttpPost(Name = "PostAddition")]
    public Guid Add(AddModel model)
    {
        _models.Add(model);
        return model.Id;

    }

    [HttpGet("GetAddition", Name = "GetAddition")]
    public IEnumerable<AddModel> GetAddition()
    {
        return _models;
    }

    [HttpPatch("PatchAddition/{id}", Name = "PatchAddition")]
    public IActionResult Update(Guid id, string lastName)
    {
        var data = _models.FirstOrDefault(m => m.Id == id);
        data.LastName = lastName;
        return Ok(new {data.Id});
    }

    [HttpDelete("DeleteAddition/{id}", Name = "DeleteAddition")]
    public IActionResult Delete(Guid id)
    {
        _models.Remove(_models.FirstOrDefault(m => m.Id == id));
        return Ok();
    }
}