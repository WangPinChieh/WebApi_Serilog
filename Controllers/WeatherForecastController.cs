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
    [ApiLogger]
    public void Add(AddModel model)
    {
        _models.Add(model);
    }

    [HttpGet("GetAddition", Name = "GetAddition")]
    [ApiLogger]
    public IEnumerable<AddModel> GetAddition()
    {
        return _models;
    }
}