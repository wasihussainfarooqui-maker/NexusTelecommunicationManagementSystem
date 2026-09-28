namespace NexusTelecommunicationManagementSystem.Models;

public sealed class AdminDashboardViewModel
{
    public int Customers { get; set; }
    public int Orders { get; set; }
    public int ActiveConnections { get; set; }
    public int PendingFeasibility { get; set; }
    public int Employees { get; set; }
    public int LowStock { get; set; }
    public decimal Outstanding { get; set; }
    public IReadOnlyList<ServiceOrder> RecentOrders { get; set; } = [];
}

public sealed class Customer
{
    public int Id { get; set; }
    public string Username { get; set; } = "";
    public string Name { get; set; } = "";
    public string Contact { get; set; } = "";
    public string Email { get; set; } = "";
    public string City { get; set; } = "";
    public string Address { get; set; } = "";
    public string AccountId { get; set; } = "";
    public bool IsActive { get; set; } = true;
}

public sealed class Employee {
    public int Id { get; set; } public string Name { get; set; } = ""; public string Role { get; set; } = "";
    public string Email { get; set; } = ""; public string Status { get; set; } = "Active";
    public string Username { get; set; } = ""; public bool HasLogin { get; set; }
}
public sealed class Vendor { public int Id { get; set; } public string Name { get; set; } = ""; public string Contact { get; set; } = ""; public string Category { get; set; } = ""; public string Status { get; set; } = "Active"; }
public sealed class RetailShop { public int Id { get; set; } public string Name { get; set; } = ""; public string City { get; set; } = ""; public string Contact { get; set; } = ""; public string Status { get; set; } = "Open"; }
public sealed class Equipment { public int Id { get; set; } public string Name { get; set; } = ""; public string Category { get; set; } = ""; public int Quantity { get; set; } public int ReorderLevel { get; set; } public string Vendor { get; set; } = ""; public string Status => Quantity <= ReorderLevel ? "Low stock" : "In stock"; }
