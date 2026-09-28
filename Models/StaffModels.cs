namespace NexusTelecommunicationManagementSystem.Models;

public sealed class StaffDashboardViewModel
{
    public int EmployeeId { get; set; }
    public string Name { get; set; } = "";
    public string Username { get; set; } = "";
    public string Role { get; set; } = "";
    public string Email { get; set; } = "";
    public int OpenOrders { get; set; }
    public int PendingOrders { get; set; }
    public int ScheduledInstallations { get; set; }
    public int ActiveConnections { get; set; }
    public int OutstandingBills { get; set; }
    public decimal OutstandingAmount { get; set; }
    public int OpenTickets { get; set; }
    public int LowStock { get; set; }
    public int Customers { get; set; }
    public IReadOnlyList<ServiceOrder> Orders { get; set; } = [];
    public IReadOnlyList<Connection> Connections { get; set; } = [];
    public IReadOnlyList<Bill> Bills { get; set; } = [];
    public IReadOnlyList<Customer> CustomersList { get; set; } = [];
    public IReadOnlyList<SupportTicket> Tickets { get; set; } = [];
    public IReadOnlyList<Equipment> Equipment { get; set; } = [];
}

public sealed class SupportTicket
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string Message { get; set; } = "";
    public string Status { get; set; } = "Open";
    public DateTime CreatedAt { get; set; }
}
