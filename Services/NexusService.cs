using Microsoft.Data.SqlClient;
using NexusTelecommunicationManagementSystem.Models;
using System.Data;

namespace NexusTelecommunicationManagementSystem.Services;

public sealed class NexusService
{
    private readonly string _cs;
    public NexusService(IConfiguration config) => _cs = config.GetConnectionString("DefaultConnection")!;

    private SqlConnection Cn() => new(_cs);
    private static Plan PlanFrom(SqlDataReader r) => new() { Id = r.GetInt32(r.GetOrdinal("PlanId")), Name = r.GetString(r.GetOrdinal("PlanName")), ConnectionType = Enum.Parse<ConnectionType>(r.GetString(r.GetOrdinal("ConnectionType"))), Price = r.GetDecimal(r.GetOrdinal("Price")), BillingCycle = r.GetString(r.GetOrdinal("BillingCycle")), Speed = r.GetString(r.GetOrdinal("Speed")), Description = r.IsDBNull(r.GetOrdinal("Description")) ? "" : r.GetString(r.GetOrdinal("Description")) };

    public IReadOnlyList<Plan> Plans => QueryPlans(true);
    public IReadOnlyList<ServiceOrder> Orders => GetOrders();
    public IReadOnlyList<Connection> Connections => GetConnections();
    public IReadOnlyList<Bill> Bills => GetBills();
    public Plan? GetPlan(int id) => QueryPlans(false).FirstOrDefault(x => x.Id == id);
    private List<Plan> QueryPlans(bool activeOnly)
    {
        var list = new List<Plan>(); using var cn = Cn(); cn.Open(); using var cmd = new SqlCommand("SELECT Id AS PlanId,Name AS PlanName,ConnectionType,Price,BillingCycle,Speed,Description FROM dbo.Plans WHERE IsActive=1 OR @all=1 ORDER BY Id", cn); cmd.Parameters.AddWithValue("@all", activeOnly ? 0 : 1); using var r = cmd.ExecuteReader(); while (r.Read()) list.Add(PlanFrom(r)); return list;
    }

    public List<ServiceOrder> GetOrders(int? userId = null)
    {
        var list = new List<ServiceOrder>(); using var cn = Cn(); cn.Open(); using var cmd = new SqlCommand(@"SELECT o.*,p.Name PlanName,p.ConnectionType,p.Price,p.BillingCycle,p.Speed,p.Description FROM dbo.Orders o JOIN dbo.Plans p ON p.Id=o.PlanId WHERE (@uid IS NULL OR o.UserId=@uid) ORDER BY o.ApplicationDate DESC", cn); cmd.Parameters.AddWithValue("@uid", (object?)userId ?? DBNull.Value); using var r = cmd.ExecuteReader(); while (r.Read()) list.Add(new ServiceOrder { Id=r.GetInt32(r.GetOrdinal("Id")),OrderCode=r.GetString(r.GetOrdinal("OrderCode")),UserId=r.IsDBNull(r.GetOrdinal("UserId"))?null:r.GetInt32(r.GetOrdinal("UserId")),CustomerName=r.GetString(r.GetOrdinal("CustomerName")),Email=r.IsDBNull(r.GetOrdinal("Email"))?"":r.GetString(r.GetOrdinal("Email")),Contact=r.GetString(r.GetOrdinal("Contact")),Address=r.GetString(r.GetOrdinal("Address")),Status=Enum.Parse<OrderStatus>(r.GetString(r.GetOrdinal("Status"))),ApplicationDate=r.GetDateTime(r.GetOrdinal("ApplicationDate")),FeasibilityNote=r.IsDBNull(r.GetOrdinal("FeasibilityNote"))?"":r.GetString(r.GetOrdinal("FeasibilityNote")),Plan=new Plan { Id=r.GetInt32(r.GetOrdinal("PlanId")),Name=r.GetString(r.GetOrdinal("PlanName")),ConnectionType=Enum.Parse<ConnectionType>(r.GetString(r.GetOrdinal("ConnectionType"))),Price=r.GetDecimal(r.GetOrdinal("Price")),BillingCycle=r.GetString(r.GetOrdinal("BillingCycle")),Speed=r.GetString(r.GetOrdinal("Speed")),Description=r.IsDBNull(r.GetOrdinal("Description"))?"":r.GetString(r.GetOrdinal("Description"))} }); return list;
    }

    public List<Connection> GetConnections(int? userId=null)
    { var list=new List<Connection>(); using var cn=Cn(); cn.Open(); using var cmd=new SqlCommand(@"SELECT c.*,p.Name PlanName,p.ConnectionType,p.Price,p.BillingCycle,p.Speed,p.Description FROM dbo.Connections c JOIN dbo.Plans p ON p.Id=c.PlanId WHERE (@uid IS NULL OR c.UserId=@uid) ORDER BY c.ConnectedAt DESC",cn); cmd.Parameters.AddWithValue("@uid",(object?)userId??DBNull.Value); using var r=cmd.ExecuteReader(); while(r.Read()) list.Add(new Connection{Id=r.GetInt32(r.GetOrdinal("Id")),AccountId=r.GetString(r.GetOrdinal("AccountId")),UserId=r.IsDBNull(r.GetOrdinal("UserId"))?null:r.GetInt32(r.GetOrdinal("UserId")),CustomerName=r.GetString(r.GetOrdinal("CustomerName")),Status=Enum.Parse<ConnectionStatus>(r.GetString(r.GetOrdinal("Status"))),ConnectedAt=r.GetDateTime(r.GetOrdinal("ConnectedAt")),AmountDue=r.GetDecimal(r.GetOrdinal("AmountDue")),Plan=new Plan{Id=r.GetInt32(r.GetOrdinal("PlanId")),Name=r.GetString(r.GetOrdinal("PlanName")),ConnectionType=Enum.Parse<ConnectionType>(r.GetString(r.GetOrdinal("ConnectionType"))),Price=r.GetDecimal(r.GetOrdinal("Price")),BillingCycle=r.GetString(r.GetOrdinal("BillingCycle")),Speed=r.GetString(r.GetOrdinal("Speed")),Description=r.IsDBNull(r.GetOrdinal("Description"))?"":r.GetString(r.GetOrdinal("Description"))}}); return list; }

    public List<Bill> GetBills(int? userId=null)
    { var list=new List<Bill>(); using var cn=Cn(); cn.Open(); using var cmd=new SqlCommand("SELECT * FROM dbo.Bills WHERE (@uid IS NULL OR UserId=@uid) ORDER BY DueDate DESC",cn); cmd.Parameters.AddWithValue("@uid",(object?)userId??DBNull.Value); using var r=cmd.ExecuteReader(); while(r.Read()) list.Add(new Bill{Id=r.GetInt32(r.GetOrdinal("Id")),BillNumber=r.GetString(r.GetOrdinal("BillNumber")),AccountId=r.GetString(r.GetOrdinal("AccountId")),UserId=r.IsDBNull(r.GetOrdinal("UserId"))?null:r.GetInt32(r.GetOrdinal("UserId")),Amount=r.GetDecimal(r.GetOrdinal("Amount")),Paid=r.GetDecimal(r.GetOrdinal("Paid")),DueDate=r.GetDateTime(r.GetOrdinal("DueDate"))}); return list; }

    public ServiceOrder? TrackOrder(string code) => GetOrders().FirstOrDefault(x=>x.OrderCode.Equals(code.Trim(),StringComparison.OrdinalIgnoreCase));
    public Connection? TrackConnection(string code) => GetConnections().FirstOrDefault(x=>x.AccountId.Equals(code.Trim(),StringComparison.OrdinalIgnoreCase));

    public string CreateOrder(NewOrderViewModel model, int userId)
    { var plan=GetPlan(model.PlanId)??Plans.First(); var code=$"{plan.CodePrefix}{DateTime.UtcNow:yyyyMMddHHmmssfff}"; using var cn=Cn(); cn.Open(); using var cmd=new SqlCommand(@"INSERT dbo.Orders(OrderCode,UserId,CustomerName,Email,Contact,Address,PlanId,Status,ApplicationDate,FeasibilityNote) OUTPUT INSERTED.OrderCode VALUES(@code,@uid,@name,@email,@contact,@address,@plan,'Pending',SYSUTCDATETIME(),'Feasibility review is pending.')",cn); Add(cmd,"@code",code);Add(cmd,"@uid",userId);Add(cmd,"@name",model.FullName.Trim());Add(cmd,"@email",model.Email.Trim());Add(cmd,"@contact",model.Contact.Trim());Add(cmd,"@address",model.Address.Trim());Add(cmd,"@plan",plan.Id); return (string)cmd.ExecuteScalar()!; }

    public void SetOrderStatus(string code, OrderStatus status) { using var cn=Cn();cn.Open();using var cmd=new SqlCommand("UPDATE dbo.Orders SET Status=@s,FeasibilityNote=@n WHERE OrderCode=@c",cn);Add(cmd,"@s",status.ToString());Add(cmd,"@n",status==OrderStatus.Feasible?"Area is serviceable. Installation can be scheduled.":status==OrderStatus.Connected?"Connection activated.":status.ToString());Add(cmd,"@c",code);cmd.ExecuteNonQuery(); if(status==OrderStatus.Connected) ActivateConnection(code); }
    private void ActivateConnection(string code) { using var cn=Cn();cn.Open();using var cmd=new SqlCommand(@"IF NOT EXISTS(SELECT 1 FROM dbo.Connections c JOIN dbo.Orders o ON o.UserId=c.UserId AND o.PlanId=c.PlanId WHERE o.OrderCode=@code) BEGIN INSERT dbo.Connections(AccountId,UserId,CustomerName,PlanId,Status,ConnectedAt,AmountDue) SELECT CONCAT(p.ConnectionType,'-',o.Id,'-',RIGHT('0000000000'+CAST(o.UserId AS varchar(10)),10)),o.UserId,o.CustomerName,o.PlanId,'Active',SYSUTCDATETIME(),p.Price FROM dbo.Orders o JOIN dbo.Plans p ON p.Id=o.PlanId WHERE o.OrderCode=@code; END",cn);Add(cmd,"@code",code);cmd.ExecuteNonQuery(); }

    public void CreateSupportTicket(string name,string email,string message,int? userId){using var cn=Cn();cn.Open();using var cmd=new SqlCommand("INSERT dbo.SupportTickets(UserId,Name,Email,Message) VALUES(@u,@n,@e,@m)",cn);Add(cmd,"@u",(object?)userId??DBNull.Value);Add(cmd,"@n",name);Add(cmd,"@e",email);Add(cmd,"@m",message);cmd.ExecuteNonQuery();}
    public Customer? Customer(int id){using var cn=Cn();cn.Open();using var cmd=new SqlCommand("SELECT u.*,COALESCE((SELECT TOP 1 AccountId FROM dbo.Connections c WHERE c.UserId=u.Id ORDER BY c.Id DESC),'') AccountId FROM dbo.Users u WHERE u.Id=@id AND u.Role='Customer'",cn);Add(cmd,"@id",id);using var r=cmd.ExecuteReader();return r.Read()?MapCustomer(r):null;}
    public List<Customer> Customers(){using var cn=Cn();cn.Open();using var cmd=new SqlCommand("SELECT u.*,COALESCE((SELECT TOP 1 AccountId FROM dbo.Connections c WHERE c.UserId=u.Id ORDER BY c.Id DESC),'') AccountId FROM dbo.Users u WHERE u.Role='Customer' ORDER BY u.CreatedAt DESC",cn);using var r=cmd.ExecuteReader();var list=new List<Customer>();while(r.Read())list.Add(MapCustomer(r));return list;}
    private static Customer MapCustomer(SqlDataReader r)=>new(){Id=r.GetInt32(r.GetOrdinal("Id")),Username=r.GetString(r.GetOrdinal("Username")),Name=r.GetString(r.GetOrdinal("FullName")),Email=r.GetString(r.GetOrdinal("Email")),Contact=r.IsDBNull(r.GetOrdinal("Contact"))?"":r.GetString(r.GetOrdinal("Contact")),Address=r.IsDBNull(r.GetOrdinal("Address"))?"":r.GetString(r.GetOrdinal("Address")),City=r.IsDBNull(r.GetOrdinal("City"))?"":r.GetString(r.GetOrdinal("City")),AccountId=r.GetString(r.GetOrdinal("AccountId")),IsActive=r.GetBoolean(r.GetOrdinal("IsActive"))};
    public void SetCustomerActive(int id,bool active){using var cn=Cn();cn.Open();using var cmd=new SqlCommand("UPDATE dbo.Users SET IsActive=@a WHERE Id=@id AND Role='Customer'",cn);Add(cmd,"@a",active);Add(cmd,"@id",id);cmd.ExecuteNonQuery();}

    public List<Employee> Employees()=>SimpleList<Employee>(@"SELECT e.Id,e.Name,e.Role,e.Email,e.Status,COALESCE(u.Username,'') Username,CAST(CASE WHEN u.Id IS NULL THEN 0 ELSE 1 END AS bit) HasLogin
FROM dbo.Employees e LEFT JOIN dbo.Users u ON u.Id=e.UserId OR (e.UserId IS NULL AND u.Email=e.Email) ORDER BY e.Name",
r=>new Employee{Id=r.GetInt32(0),Name=r.GetString(1),Role=r.GetString(2),Email=r.GetString(3),Status=r.GetString(4),Username=r.GetString(5),HasLogin=r.GetBoolean(6)});
    public Employee? EmployeeByUserId(int userId)=>SimpleList<Employee>(@"SELECT TOP 1 e.Id,e.Name,e.Role,e.Email,e.Status,COALESCE(u.Username,'') Username,1 HasLogin FROM dbo.Employees e JOIN dbo.Users u ON u.Id=e.UserId WHERE u.Id=@id", r=>new Employee{Id=r.GetInt32(0),Name=r.GetString(1),Role=r.GetString(2),Email=r.GetString(3),Status=r.GetString(4),Username=r.GetString(5),HasLogin=true}, ("@id",userId)).FirstOrDefault();
    public void AddEmployee(string name,string role,string email,string username,string password)
    {
        var (hash,salt)=PasswordService.Hash(password); using var cn=Cn();cn.Open();using var tx=cn.BeginTransaction();
        try{
            using var u=new SqlCommand("INSERT dbo.Users(Username,PasswordHash,PasswordSalt,FullName,Email,Role) OUTPUT INSERTED.Id VALUES(@u,@h,@s,@n,@e,@r)",cn,tx);
            Add(u,"@u",username);Add(u,"@h",hash);Add(u,"@s",salt);Add(u,"@n",name);Add(u,"@e",email);Add(u,"@r",role);
            var uid=(int)u.ExecuteScalar()!;
            using var e=new SqlCommand("INSERT dbo.Employees(Name,Role,Email,Status,UserId) VALUES(@n,@r,@e,'Active',@uid)",cn,tx);
            Add(e,"@n",name);Add(e,"@r",role);Add(e,"@e",email);Add(e,"@uid",uid);e.ExecuteNonQuery();tx.Commit();
        }catch{tx.Rollback();throw;}
    }
    public void UpdateEmployee(int id,string name,string role,string email,string status,string? username)
    {
        using var cn=Cn();cn.Open();using var tx=cn.BeginTransaction();
        try{
            using var e=new SqlCommand("UPDATE dbo.Employees SET Name=@n,Role=@r,Email=@e,Status=@s WHERE Id=@id",cn,tx);Add(e,"@n",name);Add(e,"@r",role);Add(e,"@e",email);Add(e,"@s",status);Add(e,"@id",id);e.ExecuteNonQuery();
            using var u=new SqlCommand(@"UPDATE u SET FullName=@n,Role=@r,Email=@e,IsActive=CASE WHEN @s='Active' THEN 1 ELSE 0 END
FROM dbo.Users u JOIN dbo.Employees ep ON ep.UserId=u.Id WHERE ep.Id=@id",cn,tx);Add(u,"@n",name);Add(u,"@r",role);Add(u,"@e",email);Add(u,"@s",status);Add(u,"@id",id);u.ExecuteNonQuery();
            if(!string.IsNullOrWhiteSpace(username)){using var un=new SqlCommand(@"UPDATE u SET Username=@un FROM dbo.Users u JOIN dbo.Employees ep ON ep.UserId=u.Id WHERE ep.Id=@id",cn,tx);Add(un,"@un",username);Add(un,"@id",id);un.ExecuteNonQuery();}
            tx.Commit();
        }catch{tx.Rollback();throw;}
    }
    public void ResetEmployeePassword(int id,string password)
    {
        var (hash,salt)=PasswordService.Hash(password);using var cn=Cn();cn.Open();using var cmd=new SqlCommand(@"UPDATE u SET PasswordHash=@h,PasswordSalt=@s FROM dbo.Users u JOIN dbo.Employees e ON e.UserId=u.Id WHERE e.Id=@id",cn);Add(cmd,"@h",hash);Add(cmd,"@s",salt);Add(cmd,"@id",id);cmd.ExecuteNonQuery();
    }

    public List<SupportTicket> GetSupportTickets(string? status=null)
    {
        var list=new List<SupportTicket>(); using var cn=Cn();cn.Open();using var cmd=new SqlCommand(@"SELECT Id,UserId,Name,Email,Message,Status,CreatedAt FROM dbo.SupportTickets WHERE (@s='' OR Status=@s) ORDER BY CASE WHEN Status='Open' THEN 0 WHEN Status='In Progress' THEN 1 ELSE 2 END,CreatedAt DESC",cn);Add(cmd,"@s",status??"");using var r=cmd.ExecuteReader();while(r.Read())list.Add(new SupportTicket{Id=r.GetInt32(0),UserId=r.IsDBNull(1)?null:r.GetInt32(1),Name=r.GetString(2),Email=r.GetString(3),Message=r.GetString(4),Status=r.GetString(5),CreatedAt=r.GetDateTime(6)});return list;
    }
    public void UpdateSupportTicket(int id,string status){using var cn=Cn();cn.Open();using var cmd=new SqlCommand("UPDATE dbo.SupportTickets SET Status=@s WHERE Id=@id",cn);Add(cmd,"@s",status);Add(cmd,"@id",id);cmd.ExecuteNonQuery();}
    public void UpdateEquipment(int id,string name,string category,int quantity,int reorderLevel,string vendor){using var cn=Cn();cn.Open();using var cmd=new SqlCommand("UPDATE dbo.Equipment SET Name=@n,Category=@c,Quantity=@q,ReorderLevel=@r,Vendor=@v WHERE Id=@id",cn);Add(cmd,"@n",name);Add(cmd,"@c",category);Add(cmd,"@q",Math.Max(0,quantity));Add(cmd,"@r",Math.Max(0,reorderLevel));Add(cmd,"@v",vendor);Add(cmd,"@id",id);cmd.ExecuteNonQuery();}
    public void UpdateCustomer(int id,string name,string email,string contact,string address,string city){using var cn=Cn();cn.Open();using var cmd=new SqlCommand("UPDATE dbo.Users SET FullName=@n,Email=@e,Contact=@c,Address=@a,City=@city WHERE Id=@id AND Role='Customer'",cn);Add(cmd,"@n",name);Add(cmd,"@e",email);Add(cmd,"@c",contact);Add(cmd,"@a",address);Add(cmd,"@city",city);Add(cmd,"@id",id);cmd.ExecuteNonQuery();}
    public void UpdateOrderDetails(string code,OrderStatus status,string note){using var cn=Cn();cn.Open();using var cmd=new SqlCommand("UPDATE dbo.Orders SET Status=@s,FeasibilityNote=@n WHERE OrderCode=@c",cn);Add(cmd,"@s",status.ToString());Add(cmd,"@n",note);Add(cmd,"@c",code);cmd.ExecuteNonQuery();if(status==OrderStatus.Connected)ActivateConnection(code);}
    public void UpdateBill(int id,decimal amount,decimal paid,DateTime dueDate){using var cn=Cn();cn.Open();using var cmd=new SqlCommand("UPDATE dbo.Bills SET Amount=@a,Paid=CASE WHEN @p<0 THEN 0 WHEN @p>@a THEN @a ELSE @p END,DueDate=@d WHERE Id=@id",cn);Add(cmd,"@a",Math.Max(0,amount));Add(cmd,"@p",Math.Max(0,paid));Add(cmd,"@d",dueDate.Date);Add(cmd,"@id",id);cmd.ExecuteNonQuery();}
    public void UpdateConnectionDetails(string accountId,ConnectionStatus status,decimal amountDue){using var cn=Cn();cn.Open();using var cmd=new SqlCommand("UPDATE dbo.Connections SET Status=@s,AmountDue=@d WHERE AccountId=@a",cn);Add(cmd,"@s",status.ToString());Add(cmd,"@d",Math.Max(0,amountDue));Add(cmd,"@a",accountId);cmd.ExecuteNonQuery();}

    public List<Vendor> Vendors()=>SimpleList<Vendor>("SELECT Id,Name,Contact,Category,Status FROM dbo.Vendors ORDER BY Id",r=>new Vendor{Id=r.GetInt32(0),Name=r.GetString(1),Contact=r.IsDBNull(2)?"":r.GetString(2),Category=r.IsDBNull(3)?"":r.GetString(3),Status=r.GetString(4)});
    public List<RetailShop> Shops()=>SimpleList<RetailShop>("SELECT Id,Name,City,Contact,Status FROM dbo.RetailShops ORDER BY Id",r=>new RetailShop{Id=r.GetInt32(0),Name=r.GetString(1),City=r.GetString(2),Contact=r.IsDBNull(3)?"":r.GetString(3),Status=r.GetString(4)});
    public List<Equipment> Equipment()=>SimpleList<Equipment>("SELECT Id,Name,Category,Quantity,ReorderLevel,Vendor FROM dbo.Equipment ORDER BY Id",r=>new Equipment{Id=r.GetInt32(0),Name=r.GetString(1),Category=r.GetString(2),Quantity=r.GetInt32(3),ReorderLevel=r.GetInt32(4),Vendor=r.IsDBNull(5)?"":r.GetString(5)});
    private List<T> SimpleList<T>(string sql,Func<SqlDataReader,T> map, params (string Name,object Value)[] parameters){using var cn=Cn();cn.Open();using var cmd=new SqlCommand(sql,cn);foreach(var p in parameters)Add(cmd,p.Name,p.Value);using var r=cmd.ExecuteReader();var list=new List<T>();while(r.Read())list.Add(map(r));return list;}

    public void UpdatePlan(int id,string name,ConnectionType type,decimal price,string cycle,string speed,string description,bool active){using var cn=Cn();cn.Open();using var cmd=new SqlCommand("UPDATE dbo.Plans SET Name=@n,ConnectionType=@t,Price=@p,BillingCycle=@c,Speed=@s,Description=@d,IsActive=@a WHERE Id=@id",cn);Add(cmd,"@n",name);Add(cmd,"@t",type.ToString());Add(cmd,"@p",Math.Max(0,price));Add(cmd,"@c",cycle);Add(cmd,"@s",speed);Add(cmd,"@d",description);Add(cmd,"@a",active);Add(cmd,"@id",id);cmd.ExecuteNonQuery();}
    public void UpdateVendor(int id,string name,string contact,string category,string status){using var cn=Cn();cn.Open();using var cmd=new SqlCommand("UPDATE dbo.Vendors SET Name=@n,Contact=@c,Category=@cat,Status=@s WHERE Id=@id",cn);Add(cmd,"@n",name);Add(cmd,"@c",contact);Add(cmd,"@cat",category);Add(cmd,"@s",status);Add(cmd,"@id",id);cmd.ExecuteNonQuery();}
    public void UpdateShop(int id,string name,string city,string contact,string status){using var cn=Cn();cn.Open();using var cmd=new SqlCommand("UPDATE dbo.RetailShops SET Name=@n,City=@city,Contact=@c,Status=@s WHERE Id=@id",cn);Add(cmd,"@n",name);Add(cmd,"@city",city);Add(cmd,"@c",contact);Add(cmd,"@s",status);Add(cmd,"@id",id);cmd.ExecuteNonQuery();}
    public void AddPlan(string name,ConnectionType type,decimal price,string cycle,string speed){using var cn=Cn();cn.Open();using var cmd=new SqlCommand("INSERT dbo.Plans(Name,ConnectionType,Price,BillingCycle,Speed,Description) VALUES(@n,@t,@p,@c,@s,'Managed NEXUS service plan.')",cn);Add(cmd,"@n",name);Add(cmd,"@t",type.ToString());Add(cmd,"@p",price);Add(cmd,"@c",cycle);Add(cmd,"@s",speed);cmd.ExecuteNonQuery();}
    public void AddCustomer(string name,string contact,string email,string city){var username="customer"+DateTime.UtcNow.Ticks.ToString()[^6..];var (hash,salt)=PasswordService.Hash("ChangeMe@123");using var cn=Cn();cn.Open();using var cmd=new SqlCommand("INSERT dbo.Users(Username,PasswordHash,PasswordSalt,FullName,Email,Contact,City,Role) VALUES(@u,@h,@s,@n,@e,@c,@city,'Customer')",cn);Add(cmd,"@u",username);Add(cmd,"@h",hash);Add(cmd,"@s",salt);Add(cmd,"@n",name);Add(cmd,"@e",email);Add(cmd,"@c",contact);Add(cmd,"@city",city);cmd.ExecuteNonQuery();}
    public void MarkBillPaid(int id,decimal amount){using var cn=Cn();cn.Open();using var cmd=new SqlCommand("UPDATE dbo.Bills SET Paid=CASE WHEN @p<0 THEN 0 WHEN @p>Amount THEN Amount ELSE @p END WHERE Id=@id",cn);Add(cmd,"@p",amount);Add(cmd,"@id",id);cmd.ExecuteNonQuery();}
    public void SetConnectionStatus(string accountId,ConnectionStatus status){using var cn=Cn();cn.Open();using var cmd=new SqlCommand("UPDATE dbo.Connections SET Status=@s WHERE AccountId=@a",cn);Add(cmd,"@s",status.ToString());Add(cmd,"@a",accountId);cmd.ExecuteNonQuery();}
    public AdminDashboardViewModel Dashboard(){var customers=Customers();var orders=GetOrders();var con=GetConnections();var bills=GetBills();return new(){Customers=customers.Count,Orders=orders.Count,ActiveConnections=con.Count(x=>x.Status==ConnectionStatus.Active),PendingFeasibility=orders.Count(x=>x.Status==OrderStatus.Pending),Employees=Employees().Count(x=>x.Status=="Active"),LowStock=Equipment().Count(x=>x.Quantity<=x.ReorderLevel),Outstanding=bills.Sum(x=>x.Due),RecentOrders=orders.Take(6).ToList()};}
    public string? GetProfileField(int userId,string field){var allowed=new[]{"FullName","Email","Contact","Address","City"};if(!allowed.Contains(field))return null;using var cn=Cn();cn.Open();using var cmd=new SqlCommand($"SELECT {field} FROM dbo.Users WHERE Id=@id",cn);Add(cmd,"@id",userId);return cmd.ExecuteScalar()?.ToString();}
    private static void Add(SqlCommand c,string n,object v){c.Parameters.AddWithValue(n,v??DBNull.Value);}
}
