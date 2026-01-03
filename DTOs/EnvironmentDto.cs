namespace ShelfSimAPI.DTOs;

public class EnvironmentConfigResponseDto
{
    public TemperatureConfig Temperature { get; set; } = new();
    public HumidityConfig Humidity { get; set; } = new();
    public LightControlConfig LightControl { get; set; } = new();
}

public class TemperatureConfig
{
    public float Target { get; set; } = 23.0f;
    public float Tolerance { get; set; } = 2.0f;
}

public class HumidityConfig
{
    public float Target { get; set; } = 45.0f;
    public float Tolerance { get; set; } = 5.0f;
}

public class LightControlConfig
{
    public bool AllowLight { get; set; } = false;
}

public class UpdateEnvironmentConfigDto
{
    public TemperatureConfig Temperature { get; set; } = new();
    public HumidityConfig Humidity { get; set; } = new();
    public LightControlConfig LightControl { get; set; } = new();
}