namespace BusinessObjects.DTOs;

public class WeatherDTO
{
    public string Location { get; set; } = string.Empty;
    public double Temperature { get; set; }
    public string Condition { get; set; } = string.Empty;
    public double WindSpeed { get; set; }
    public int Humidity { get; set; }
    public string WeatherIconClass { get; set; } = "bi-cloud-sun";
    public DateTime FetchedAt { get; set; }
}
