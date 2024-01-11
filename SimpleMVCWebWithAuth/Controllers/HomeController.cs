using System.Diagnostics;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleMVCWebWithAuth.Data;
using SimpleMVCWebWithAuth.Models;

namespace SimpleMVCWebWithAuth.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly TelemetryClient _telemetryClient;
    private readonly IConfiguration _configuration;
    private readonly IUserRolesService _userRolesService;

    public HomeController(ILogger<HomeController> logger, TelemetryClient telemetryClient, IConfiguration configuration, IUserRolesService userRolesService)

    {
        _logger = logger;
        _telemetryClient = telemetryClient;
        _configuration = configuration;
        _userRolesService = userRolesService;
    }

    public IActionResult Index()
    {
        var simpleValue = _configuration["SimpleWebShared:MySimpleValue"] ?? "simpleValue is not set or accessible";
        var secretValue = _configuration["SimpleWebShared:MySecretValue"] ?? "secretValue is not set or accessible";
        ViewData["mysimplevalue"] = simpleValue;
        ViewData["mysecretvalue"] = secretValue;
        _telemetryClient.TrackTrace($"simpleValue in ViewData: {ViewData["mysimplevalue"]}");
        _telemetryClient.TrackTrace($"secretValue in ViewData: {ViewData["mysecretvalue"]}");
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult DemoLiveInsights()
    {
        //track event
        _telemetryClient.TrackEvent("EventTracked: Demo Live Insights Viewed");
        try
        {
            throw new Exception("All exceptions can be easily tracked!");
        }
        catch (Exception ex)
        {
            _telemetryClient.TrackException(ex);
        }
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    //Enable for creating admin role and user (chapter 7)
    /**
    [Authorize]
    public async Task<IActionResult> EnsureRolesAndUsers()
    {
        await _userRolesService.EnsureAdminUserRole();
        return RedirectToAction("Index");
    }
    **/
}
