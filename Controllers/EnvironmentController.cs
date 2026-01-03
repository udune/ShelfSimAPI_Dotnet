using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShelfSimAPI.Data;
using ShelfSimAPI.DTOs;
using ShelfSimAPI.Models;

namespace ShelfSimAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EnvironmentController(AppDbContext context) : ControllerBase
{
    [HttpGet("config")]
    public async Task<ActionResult<EnvironmentConfigResponseDto>> GetConfig()
    {
        var configs = await context.EnvironmentConfigs.ToListAsync();

        var temp = configs.FirstOrDefault(c => c.ConfigKey == "Temperature");
        var humid = configs.FirstOrDefault(c => c.ConfigKey == "Humidity");
        var light = configs.FirstOrDefault(c => c.ConfigKey == "LightControl");

        return Ok(new EnvironmentConfigResponseDto
        {
            Temperature = new TemperatureConfig
            {
                Target = temp?.TargetValue ?? 23.0f,
                Tolerance = temp?.Tolerance ?? 2.0f
            },
            Humidity = new HumidityConfig
            {
                Target = humid?.TargetValue ?? 45.0f,
                Tolerance = humid?.Tolerance ?? 5.0f
            },
            LightControl = new LightControlConfig
            {
                AllowLight = light?.BoolValue ?? false
            }
        });
    }

    [HttpPut("config")]
    public async Task<ActionResult> UpdateConfig([FromBody] UpdateEnvironmentConfigDto dto)
    {
        await UpsertConfig("Temperature", dto.Temperature.Target, dto.Temperature.Tolerance, null);
        await UpsertConfig("Humidity", dto.Humidity.Target, dto.Humidity.Tolerance, null);
        await UpsertConfig("LightControl", 0, 0, dto.LightControl.AllowLight);

        await context.SaveChangesAsync();
        return Ok(new { message = "설정이 업데이트되었습니다." });
    }

    private async Task UpsertConfig(string key, float target, float tolerance, bool? boolValue)
    {
        var config = await context.EnvironmentConfigs.FindAsync(key);
        if (config == null)
        {
            context.EnvironmentConfigs.Add(new EnvironmentConfig
            {
                ConfigKey = key,
                TargetValue = target,
                Tolerance = tolerance,
                BoolValue = boolValue
            });
        }
        else
        {
            config.TargetValue = target;
            config.Tolerance = tolerance;
            config.BoolValue = boolValue;
            config.UpdatedAt = DateTime.UtcNow;
        }
    }
}