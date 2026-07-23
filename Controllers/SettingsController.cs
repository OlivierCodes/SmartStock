using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Text.Json;

namespace SmartStock.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SettingsController : ControllerBase
{
    private readonly string _settingsPath = Path.Combine(Directory.GetCurrentDirectory(), "shop-settings.json");

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Get()
    {
        if (!System.IO.File.Exists(_settingsPath))
            return Ok(new { shopName = "SMART STOCK", logoBase64 = "" });

        var content = System.IO.File.ReadAllText(_settingsPath);
        return Content(content, "application/json");
    }

    [HttpPost]
    [Authorize(Roles = "Responsable")]
    public IActionResult Save([FromBody] JsonElement settings)
    {
        System.IO.File.WriteAllText(_settingsPath, settings.GetRawText());
        return Ok(new { message = "Settings saved" });
    }
}