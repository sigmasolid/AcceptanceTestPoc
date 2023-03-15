using System.Net.Http.Json;
using AcceptanceTestPoc.Api1;
using Shouldly;
using TechTalk.SpecFlow;

namespace AcceptanceTestPoc.Tests.Acceptance.StepDefinitions;

[Binding]
public class WeatherForecastSteps
{
    private readonly HttpClient _httpClient;
    private readonly ScenarioContext _scenarioContext;

    public WeatherForecastSteps(HttpClient httpClient, ScenarioContext scenarioContext)
    {
        _httpClient = httpClient;
        _scenarioContext = scenarioContext;
    }

    [When(@"I call the Weather forecast API")]
    public async Task WhenICallTheWeatherForecastApi()
    {
        var response = await _httpClient.GetAsync("WeatherForecast");
        var forecasts = await response.Content.ReadFromJsonAsync<WeatherForecast[]>();
        _scenarioContext.Add("forecasts", forecasts);
    }

    [Then(@"I get a 5 random weather forecasts")]
    public void ThenIGetAWeatherForecast()
    {
        var forecasts = _scenarioContext.Get<WeatherForecast[]>("forecasts");
        forecasts.Length.ShouldBe(5);
    }
}