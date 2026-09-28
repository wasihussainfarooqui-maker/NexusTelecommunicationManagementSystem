using Microsoft.AspNetCore.Mvc;
using NexusTelecommunicationManagementSystem.Services;
namespace NexusTelecommunicationManagementSystem.Controllers;
public class HomeController : Controller { private readonly NexusService _nexus; public HomeController(NexusService n)=>_nexus=n; public IActionResult Index()=>View(_nexus.Plans); public IActionResult Error()=>View(); }
