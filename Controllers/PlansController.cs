using Microsoft.AspNetCore.Mvc; using NexusTelecommunicationManagementSystem.Services;
namespace NexusTelecommunicationManagementSystem.Controllers;
public class PlansController : Controller { private readonly NexusService _nexus; public PlansController(NexusService n)=>_nexus=n; public IActionResult Index()=>View(_nexus.Plans); }
