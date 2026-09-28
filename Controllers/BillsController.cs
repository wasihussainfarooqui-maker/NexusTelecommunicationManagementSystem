using Microsoft.AspNetCore.Mvc; using NexusTelecommunicationManagementSystem.Services;
namespace NexusTelecommunicationManagementSystem.Controllers;
public class BillsController:Controller{private readonly NexusService _nexus;public BillsController(NexusService n)=>_nexus=n;public IActionResult Index(){if(HttpContext.Session.GetString("Role")!="Customer")return RedirectToAction("Login","Account");return View(_nexus.GetBills(HttpContext.Session.GetInt32("UserId")));}}
