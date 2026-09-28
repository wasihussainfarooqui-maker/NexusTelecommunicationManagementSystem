using Microsoft.AspNetCore.Mvc; using NexusTelecommunicationManagementSystem.Services;
namespace NexusTelecommunicationManagementSystem.Controllers;
public class ProfileController:Controller{private readonly NexusService _nexus;public ProfileController(NexusService n)=>_nexus=n;public IActionResult Index(){var id=HttpContext.Session.GetInt32("UserId");if(id is null)return RedirectToAction("Login","Account");return View(_nexus.Customer(id.Value));}}
