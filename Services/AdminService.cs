using NexusTelecommunicationManagementSystem.Models;

namespace NexusTelecommunicationManagementSystem.Services;

// Compatibility facade retained from the original Admin project. The integrated
// application now persists all records through NexusService/SQL Server.
public sealed class AdminService
{
    private readonly NexusService _db;
    public AdminService(NexusService db) => _db = db;
    public AdminDashboardViewModel Dashboard() => _db.Dashboard();
    public IReadOnlyList<ServiceOrder> Orders => _db.GetOrders();
    public IReadOnlyList<Connection> Connections => _db.GetConnections();
    public IReadOnlyList<Bill> Bills => _db.GetBills();
    public IReadOnlyList<Customer> Customers => _db.Customers();
    public IReadOnlyList<Employee> Employees => _db.Employees();
    public IReadOnlyList<Vendor> Vendors => _db.Vendors();
    public IReadOnlyList<RetailShop> Shops => _db.Shops();
    public IReadOnlyList<Equipment> Equipment => _db.Equipment();
    public IReadOnlyList<Plan> Plans => _db.Plans;
}
