using System.Net;
using BoDi;
using Ductus.FluentDocker.Builders;
using Ductus.FluentDocker.Services;
using Microsoft.Extensions.Configuration;
using TechTalk.SpecFlow;

namespace AcceptanceTestPoc.Tests.Acceptance.Hooks;

[Binding]
public class DockerControllerHooks
{
    private static ICompositeService _compositeService;
    private IObjectContainer _objectContainer;

    public DockerControllerHooks(IObjectContainer objectContainer)
    {
        _objectContainer = objectContainer;
    }

    [BeforeTestRun]
    public static void DockerComposeUp()
    {
        var config = LoadConfig();
        var dockerComposeFilename = config["DockerComposeFilename"];
        var dockerComposePath = GetDockerComposeLocation(dockerComposeFilename!);
        var baseUri = config["WeatherForecast.Api:BaseAddress"];
        _compositeService = new Builder()
            .UseContainer()
            .UseCompose()
            .FromFile(dockerComposePath)
            .RemoveOrphans()
            .WaitForHttp("webapi", $"{baseUri}/WeatherForecast",
                continuation: (response, _) => response.Code != HttpStatusCode.OK ? 2000 : 0)
            .Build()
            .Start();
    }
    
    [AfterTestRun]
    public static void DockerComposeDown()
    {
        _compositeService.Stop();
        _compositeService.Dispose();
    }

    [BeforeScenario()]
    public void AddHttpClient()
    {
        var config = LoadConfig();
        var httpClient = new HttpClient()
        {
            BaseAddress = new Uri(config["WeatherForecast.Api:BaseAddress"]!)
        };
        _objectContainer.RegisterInstanceAs(httpClient);
    }

    private static string GetDockerComposeLocation(string dockerComposeFilename)
    {
        var dir = Directory.GetCurrentDirectory();
        while (!Directory.EnumerateFiles(dir, "*.yml").Any())//s => s.EndsWith(Path.DirectorySeparatorChar)))
        {
            dir = dir.Substring(0, dir.LastIndexOf(Path.DirectorySeparatorChar));
        }

        return Path.Combine(dir, dockerComposeFilename);
    }

    private static IConfiguration LoadConfig()
    {
        return new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();
    }
}