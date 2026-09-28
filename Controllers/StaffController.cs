using Microsoft.AspNetCore.Mvc;
using NexusTelecommunicationManagementSystem.Models;
using NexusTelecommunicationManagementSystem.Services;

namespace NexusTelecommunicationManagementSystem.Controllers;

public class StaffController : Controller
{
    private readonly NexusService _s;
    public StaffController(NexusService s) => _s = s;

    private string Role => HttpContext.Session.GetString("Role") ?? "";
    private int UserId => HttpContext.Session.GetInt32("UserId") ?? 0;
    private bool Guard(params string[] roles) => roles.Length == 0 || roles.Any(x => Role.Equals(x, StringComparison.OrdinalIgnoreCase));
    private IActionResult Denied() => RedirectToAction("AccessDenied", "Account");

    [HttpGet]
    public IActionResult Dashboard()
    {
        if (string.IsNullOrWhiteSpace(Role) || Role.Equals("Customer", StringComparison.OrdinalIgnoreCase) || Role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            return RedirectToAction("StaffLogin", "Account");

        var orders = _s.GetOrders();
        var connections = _s.GetConnections();
        var bills = _s.GetBills();
        var customers = _s.Customers();
        var tickets = _s.GetSupportTickets();
        var equipment = _s.Equipment();
        var employee = _s.EmployeeByUserId(UserId);
        var r = Role.ToLowerInvariant();

        var vm = new StaffDashboardViewModel
        {
            EmployeeId = employee?.Id ?? 0,
            Name = HttpContext.Session.GetString("FullName") ?? "Staff",
            Username = HttpContext.Session.GetString("Username") ?? "",
            Role = Role,
            Email = employee?.Email ?? "",
            OpenOrders = orders.Count(x => x.Status is OrderStatus.Pending or OrderStatus.Feasible or OrderStatus.InstallationScheduled),
            PendingOrders = orders.Count(x => x.Status == OrderStatus.Pending),
            ScheduledInstallations = orders.Count(x => x.Status == OrderStatus.InstallationScheduled),
            ActiveConnections = connections.Count(x => x.Status == ConnectionStatus.Active),
            OutstandingBills = bills.Count(x => x.Due > 0),
            OutstandingAmount = bills.Sum(x => x.Due),
            OpenTickets = tickets.Count(x => !x.Status.Equals("Closed", StringComparison.OrdinalIgnoreCase)),
            LowStock = equipment.Count(x => x.Quantity <= x.ReorderLevel),
            Customers = customers.Count,
            Orders = orders,
            Connections = connections,
            Bills = bills,
            CustomersList = customers,
            Tickets = tickets,
            Equipment = equipment
        };

        // Each role receives a deliberately different work queue.
        if (r.Contains("technical")) vm.Orders = orders.Where(x => x.Status is OrderStatus.Pending or OrderStatus.Feasible or OrderStatus.InstallationScheduled).ToList();
        else if (r.Contains("account")) vm.Bills = bills.Where(x => x.Due > 0 || x.DueDate >= DateTime.Today.AddDays(-30)).ToList();
        else if (r.Contains("retail") || r.Contains("sales")) vm.Orders = orders.Where(x => x.Status != OrderStatus.Connected && x.Status != OrderStatus.Cancelled).ToList();
        else if (r.Contains("support")) vm.Tickets = tickets.Where(x => !x.Status.Equals("Closed", StringComparison.OrdinalIgnoreCase)).ToList();
        else if (r.Contains("inventory")) vm.Equipment = equipment;

        return View(vm);
    }

    [HttpPost][ValidateAntiForgeryToken]
    public IActionResult UpdateOrder(string code, OrderStatus status, string note)
    {
        if (!Guard("Technical", "Retail Executive", "Sales", "Support")) return Denied();
        _s.UpdateOrderDetails(code, status, string.IsNullOrWhiteSpace(note) ? $"Updated by {Role}." : note.Trim());
        TempData["StaffMessage"] = $"Order {code} updated.";
        return RedirectToAction(nameof(Dashboard));
    }

    [HttpPost][ValidateAntiForgeryToken]
    public IActionResult UpdateConnection(string accountId, ConnectionStatus status, decimal amountDue)
    {
        if (!Guard("Technical", "Accounts", "Support")) return Denied();
        _s.UpdateConnectionDetails(accountId, status, amountDue);
        TempData["StaffMessage"] = $"Connection {accountId} updated.";
        return RedirectToAction(nameof(Dashboard));
    }

    [HttpPost][ValidateAntiForgeryToken]
    public IActionResult UpdateBill(int id, decimal amount, decimal paid, DateTime dueDate)
    {
        if (!Guard("Accounts")) return Denied();
        _s.UpdateBill(id, amount, paid, dueDate);
        TempData["StaffMessage"] = "Billing record updated.";
        return RedirectToAction(nameof(Dashboard));
    }

    [HttpPost][ValidateAntiForgeryToken]
    public IActionResult UpdateTicket(int id, string status)
    {
        if (!Guard("Support", "Retail Executive", "Technical")) return Denied();
        _s.UpdateSupportTicket(id, status);
        TempData["StaffMessage"] = "Support ticket updated.";
        return RedirectToAction(nameof(Dashboard));
    }

    [HttpPost][ValidateAntiForgeryToken]
    public IActionResult UpdateEquipment(int id, string name, string category, int quantity, int reorderLevel, string vendor)
    {
        if (!Guard("Inventory", "Technical")) return Denied();
        _s.UpdateEquipment(id, name.Trim(), category.Trim(), quantity, reorderLevel, vendor.Trim());
        TempData["StaffMessage"] = "Equipment record updated.";
        return RedirectToAction(nameof(Dashboard));
    }

    [HttpPost][ValidateAntiForgeryToken]
    public IActionResult UpdateCustomer(int id, string name, string email, string contact, string address, string city)
    {
        if (!Guard("Retail Executive", "Sales", "Support", "Accounts")) return Denied();
        _s.UpdateCustomer(id, name.Trim(), email.Trim(), contact.Trim(), address.Trim(), city.Trim());
        TempData["StaffMessage"] = "Customer profile updated.";
        return RedirectToAction(nameof(Dashboard));
    }
}
