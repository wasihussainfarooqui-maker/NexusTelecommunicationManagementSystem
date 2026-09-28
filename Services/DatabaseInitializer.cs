using Microsoft.Data.SqlClient;
using System.Security.Cryptography;

namespace NexusTelecommunicationManagementSystem.Services;

public sealed class DatabaseInitializer
{
    private readonly string _connectionString;
    public DatabaseInitializer(IConfiguration configuration) => _connectionString = configuration.GetConnectionString("DefaultConnection")!;

    public async Task InitializeAsync()
    {
        var builder = new SqlConnectionStringBuilder(_connectionString);
        var database = builder.InitialCatalog;
        builder.InitialCatalog = "master";
        await using (var master = new SqlConnection(builder.ConnectionString))
        {
            await master.OpenAsync();
            var safeName = database.Replace("]", "]]" );
            await using var cmd = new SqlCommand($"IF DB_ID(N'{database.Replace("'", "''")}') IS NULL CREATE DATABASE [{safeName}]", master);
            await cmd.ExecuteNonQueryAsync();
        }

        await using var cn = new SqlConnection(_connectionString);
        await cn.OpenAsync();
        foreach (var statement in SchemaStatements())
        {
            await using var cmd = new SqlCommand(statement, cn);
            await cmd.ExecuteNonQueryAsync();
        }
        await SeedAsync(cn);
        await EnsureStaffAsync(cn);
    }

    private static IEnumerable<string> SchemaStatements() => new[]
    {
        @"IF OBJECT_ID('dbo.Users','U') IS NULL CREATE TABLE dbo.Users(
            Id INT IDENTITY(1,1) PRIMARY KEY, Username NVARCHAR(80) NOT NULL UNIQUE, PasswordHash VARBINARY(64) NOT NULL,
            PasswordSalt VARBINARY(32) NOT NULL, FullName NVARCHAR(160) NOT NULL, Email NVARCHAR(160) NOT NULL,
            Contact NVARCHAR(40) NULL, Address NVARCHAR(250) NULL, City NVARCHAR(100) NULL, Role NVARCHAR(100) NOT NULL,
            IsActive BIT NOT NULL DEFAULT 1, CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME());",
        @"IF COL_LENGTH('dbo.Users','Role') IS NOT NULL AND COL_LENGTH('dbo.Users','Role') < 100 ALTER TABLE dbo.Users ALTER COLUMN Role NVARCHAR(100) NOT NULL;",
        @"IF OBJECT_ID('dbo.Plans','U') IS NULL CREATE TABLE dbo.Plans(
            Id INT IDENTITY(1,1) PRIMARY KEY, Name NVARCHAR(160) NOT NULL, ConnectionType NVARCHAR(30) NOT NULL,
            Price DECIMAL(12,2) NOT NULL, BillingCycle NVARCHAR(40) NOT NULL, Speed NVARCHAR(80) NOT NULL,
            Description NVARCHAR(500) NULL, IsActive BIT NOT NULL DEFAULT 1);",
        @"IF OBJECT_ID('dbo.Orders','U') IS NULL CREATE TABLE dbo.Orders(
            Id INT IDENTITY(1,1) PRIMARY KEY, OrderCode NVARCHAR(40) NOT NULL UNIQUE, UserId INT NULL,
            CustomerName NVARCHAR(160) NOT NULL, Email NVARCHAR(160) NULL, Contact NVARCHAR(40) NOT NULL,
            Address NVARCHAR(250) NOT NULL, PlanId INT NOT NULL, Status NVARCHAR(40) NOT NULL,
            ApplicationDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(), FeasibilityNote NVARCHAR(500) NULL,
            CONSTRAINT FK_Orders_Users FOREIGN KEY(UserId) REFERENCES dbo.Users(Id), CONSTRAINT FK_Orders_Plans FOREIGN KEY(PlanId) REFERENCES dbo.Plans(Id));",
        @"IF OBJECT_ID('dbo.Connections','U') IS NULL CREATE TABLE dbo.Connections(
            Id INT IDENTITY(1,1) PRIMARY KEY, AccountId NVARCHAR(40) NOT NULL UNIQUE, UserId INT NULL,
            CustomerName NVARCHAR(160) NOT NULL, PlanId INT NOT NULL, Status NVARCHAR(40) NOT NULL,
            ConnectedAt DATETIME2 NOT NULL, AmountDue DECIMAL(12,2) NOT NULL DEFAULT 0,
            CONSTRAINT FK_Connections_Users FOREIGN KEY(UserId) REFERENCES dbo.Users(Id), CONSTRAINT FK_Connections_Plans FOREIGN KEY(PlanId) REFERENCES dbo.Plans(Id));",
        @"IF OBJECT_ID('dbo.Bills','U') IS NULL CREATE TABLE dbo.Bills(
            Id INT IDENTITY(1,1) PRIMARY KEY, BillNumber NVARCHAR(50) NOT NULL UNIQUE, AccountId NVARCHAR(40) NOT NULL,
            UserId INT NULL, Amount DECIMAL(12,2) NOT NULL, Paid DECIMAL(12,2) NOT NULL DEFAULT 0, DueDate DATE NOT NULL,
            CONSTRAINT FK_Bills_Users FOREIGN KEY(UserId) REFERENCES dbo.Users(Id));",
        @"IF OBJECT_ID('dbo.Employees','U') IS NULL CREATE TABLE dbo.Employees(Id INT IDENTITY PRIMARY KEY, Name NVARCHAR(160) NOT NULL, Role NVARCHAR(100) NOT NULL, Email NVARCHAR(160) NOT NULL, Status NVARCHAR(30) NOT NULL DEFAULT 'Active');",
        @"IF COL_LENGTH('dbo.Employees','UserId') IS NULL ALTER TABLE dbo.Employees ADD UserId INT NULL;",
        @"IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name='FK_Employees_Users') ALTER TABLE dbo.Employees ADD CONSTRAINT FK_Employees_Users FOREIGN KEY(UserId) REFERENCES dbo.Users(Id);",
        @"IF OBJECT_ID('dbo.Vendors','U') IS NULL CREATE TABLE dbo.Vendors(Id INT IDENTITY PRIMARY KEY, Name NVARCHAR(160) NOT NULL, Contact NVARCHAR(80) NULL, Category NVARCHAR(100) NULL, Status NVARCHAR(30) NOT NULL DEFAULT 'Active');",
        @"IF OBJECT_ID('dbo.RetailShops','U') IS NULL CREATE TABLE dbo.RetailShops(Id INT IDENTITY PRIMARY KEY, Name NVARCHAR(160) NOT NULL, City NVARCHAR(100) NOT NULL, Contact NVARCHAR(80) NULL, Status NVARCHAR(30) NOT NULL DEFAULT 'Open');",
        @"IF OBJECT_ID('dbo.Equipment','U') IS NULL CREATE TABLE dbo.Equipment(Id INT IDENTITY PRIMARY KEY, Name NVARCHAR(160) NOT NULL, Category NVARCHAR(100) NOT NULL, Quantity INT NOT NULL, ReorderLevel INT NOT NULL, Vendor NVARCHAR(160) NULL);",
        @"IF OBJECT_ID('dbo.SupportTickets','U') IS NULL CREATE TABLE dbo.SupportTickets(Id INT IDENTITY PRIMARY KEY, UserId INT NULL, Name NVARCHAR(160) NOT NULL, Email NVARCHAR(160) NOT NULL, Message NVARCHAR(2000) NOT NULL, Status NVARCHAR(30) NOT NULL DEFAULT 'Open', CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(), CONSTRAINT FK_Support_Users FOREIGN KEY(UserId) REFERENCES dbo.Users(Id));",
        @"IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_Orders_UserId') CREATE INDEX IX_Orders_UserId ON dbo.Orders(UserId);",
        @"IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_Bills_UserId') CREATE INDEX IX_Bills_UserId ON dbo.Bills(UserId);"
    };

    private static async Task SeedAsync(SqlConnection cn)
    {
        var (adminHash, adminSalt) = PasswordService.Hash("NexusAdmin@2026!");
        var (userHash, userSalt) = PasswordService.Hash("User@12345");
        const string seed = @"
IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Username='admin')
INSERT dbo.Users(Username,PasswordHash,PasswordSalt,FullName,Email,Contact,Address,City,Role) VALUES('admin',@ah,@as,'NEXUS Administrator','admin@nexus.local','021-1110000','NEXUS HQ','Karachi','Admin');
IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Username='demo')
INSERT dbo.Users(Username,PasswordHash,PasswordSalt,FullName,Email,Contact,Address,City,Role) VALUES('demo',@uh,@us,'Demo Customer','customer@example.com','0300-0000000','Clifton, Karachi','Karachi','Customer');
IF NOT EXISTS (SELECT 1 FROM dbo.Plans)
BEGIN
INSERT dbo.Plans(Name,ConnectionType,Price,BillingCycle,Speed,Description) VALUES
('Broadband 64 Kbps Unlimited','Broadband',225,'Monthly','64 Kbps','Unlimited broadband connectivity for everyday browsing and communication.'),
('Broadband 128 Kbps Unlimited','Broadband',350,'Monthly','128 Kbps','Higher-speed unlimited broadband for homes and small offices.'),
('Dial-Up 28 Kbps Unlimited','DialUp',75,'Monthly','28 Kbps','Unlimited dial-up service with simple monthly management.'),
('Telephone Local Monthly','Telephone',35,'Monthly','Landline','Monthly telephone rental plus applicable call charges.');
END
IF NOT EXISTS (SELECT 1 FROM dbo.Employees) INSERT dbo.Employees(Name,Role,Email) VALUES('Sara Ahmed','Admin','sara@nexus.local'),('Usman Ali','Technical','usman@nexus.local'),('Maham Raza','Accounts','maham@nexus.local'),('Bilal Shah','Retail Executive','bilal@nexus.local');
IF NOT EXISTS (SELECT 1 FROM dbo.Vendors) INSERT dbo.Vendors(Name,Contact,Category) VALUES('Nexus Hardware Supply','021-5551001','Routers & Modems'),('Network Devices Ltd.','042-5552002','Switches');
IF NOT EXISTS (SELECT 1 FROM dbo.RetailShops) INSERT dbo.RetailShops(Name,City,Contact) VALUES('NEXUS Clifton','Karachi','021-1110001'),('NEXUS Gulberg','Lahore','042-1110002'),('NEXUS Blue Area','Islamabad','051-1110003');
IF NOT EXISTS (SELECT 1 FROM dbo.Equipment) INSERT dbo.Equipment(Name,Category,Quantity,ReorderLevel,Vendor) VALUES('Broadband Router','Router',42,15,'Nexus Hardware Supply'),('ADSL Modem','Modem',9,10,'Nexus Hardware Supply'),('24-Port Switch','Switch',17,5,'Network Devices Ltd.');
IF NOT EXISTS (SELECT 1 FROM dbo.Orders WHERE OrderCode='B0000000001')
INSERT dbo.Orders(OrderCode,UserId,CustomerName,Email,Contact,Address,PlanId,Status,ApplicationDate,FeasibilityNote)
SELECT 'B0000000001',u.Id,u.FullName,u.Email,u.Contact,u.Address,p.Id,'Feasible',DATEADD(day,-2,SYSUTCDATETIME()),'Area is serviceable. Installation can be scheduled.' FROM dbo.Users u CROSS JOIN dbo.Plans p WHERE u.Username='demo' AND p.Id=1;
IF NOT EXISTS (SELECT 1 FROM dbo.Connections WHERE AccountId='B001000000000001')
INSERT dbo.Connections(AccountId,UserId,CustomerName,PlanId,Status,ConnectedAt,AmountDue)
SELECT 'B001000000000001',u.Id,u.FullName,p.Id,'Active',DATEADD(month,-2,SYSUTCDATETIME()),225 FROM dbo.Users u CROSS JOIN dbo.Plans p WHERE u.Username='demo' AND p.Id=1;
IF NOT EXISTS (SELECT 1 FROM dbo.Bills WHERE BillNumber='NEX-2026-0001')
INSERT dbo.Bills(BillNumber,AccountId,UserId,Amount,Paid,DueDate) SELECT 'NEX-2026-0001','B001000000000001',u.Id,225,0,DATEADD(day,10,CAST(GETDATE() AS date)) FROM dbo.Users u WHERE u.Username='demo';";
        await using var cmd = new SqlCommand(seed, cn);
        cmd.Parameters.AddWithValue("@ah", adminHash); cmd.Parameters.AddWithValue("@as", adminSalt); cmd.Parameters.AddWithValue("@uh", userHash); cmd.Parameters.AddWithValue("@us", userSalt);
        await cmd.ExecuteNonQueryAsync();
    }
    private static async Task EnsureStaffAsync(SqlConnection cn)
    {
        var staff = new[]
        {
            ("tech.usman","Tech@12345","Usman Ali","usman@nexus.local","0300-0000001","Technical"),
            ("accounts.maham","Accounts@12345","Maham Raza","maham@nexus.local","0300-0000002","Accounts"),
            ("retail.bilal","Retail@12345","Bilal Shah","bilal@nexus.local","0300-0000003","Retail Executive"),
            ("sara.admin","Admin@12345","Sara Ahmed","sara@nexus.local","0300-0000004","Admin")
        };
        foreach(var x in staff)
        {
            if(!await ScalarExists(cn,"SELECT COUNT(*) FROM dbo.Users WHERE Username=@u",("@u",x.Item1)))
            {
                var (hash,salt)=PasswordService.Hash(x.Item2);
                await using var add=new SqlCommand("INSERT dbo.Users(Username,PasswordHash,PasswordSalt,FullName,Email,Contact,Role) VALUES(@u,@h,@s,@n,@e,@c,@r)",cn);
                add.Parameters.AddWithValue("@u",x.Item1);add.Parameters.AddWithValue("@h",hash);add.Parameters.AddWithValue("@s",salt);add.Parameters.AddWithValue("@n",x.Item3);add.Parameters.AddWithValue("@e",x.Item4);add.Parameters.AddWithValue("@c",x.Item5);add.Parameters.AddWithValue("@r",x.Item6);
                await add.ExecuteNonQueryAsync();
            }
        }
        await using var link=new SqlCommand("UPDATE e SET UserId=u.Id FROM dbo.Employees e JOIN dbo.Users u ON u.Email=e.Email WHERE e.UserId IS NULL",cn);
        await link.ExecuteNonQueryAsync();
    }
    private static async Task<bool> ScalarExists(SqlConnection cn,string sql,(string Name,object Value) p)
    {
        await using var cmd=new SqlCommand(sql,cn);cmd.Parameters.AddWithValue(p.Name,p.Value);return Convert.ToInt32(await cmd.ExecuteScalarAsync())>0;
    }

}
