/* NEXUS Telecommunication Management System - SQL Server / SSMS database */
IF DB_ID(N'NexusTelecommunicationManagementSystem') IS NULL CREATE DATABASE NexusTelecommunicationManagementSystem;
GO
USE NexusTelecommunicationManagementSystem;
GO
IF OBJECT_ID('dbo.Users','U') IS NULL CREATE TABLE dbo.Users(Id INT IDENTITY(1,1) PRIMARY KEY,Username NVARCHAR(80) NOT NULL UNIQUE,PasswordHash VARBINARY(64) NOT NULL,PasswordSalt VARBINARY(32) NOT NULL,FullName NVARCHAR(160) NOT NULL,Email NVARCHAR(160) NOT NULL,Contact NVARCHAR(40),Address NVARCHAR(250),City NVARCHAR(100),Role NVARCHAR(100) NOT NULL,IsActive BIT NOT NULL DEFAULT 1,CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME());
GO
IF COL_LENGTH('dbo.Users','Role') IS NOT NULL AND COL_LENGTH('dbo.Users','Role') < 100 ALTER TABLE dbo.Users ALTER COLUMN Role NVARCHAR(100) NOT NULL;
GO
IF OBJECT_ID('dbo.Plans','U') IS NULL CREATE TABLE dbo.Plans(Id INT IDENTITY(1,1) PRIMARY KEY,Name NVARCHAR(160) NOT NULL,ConnectionType NVARCHAR(30) NOT NULL,Price DECIMAL(12,2) NOT NULL,BillingCycle NVARCHAR(40) NOT NULL,Speed NVARCHAR(80) NOT NULL,Description NVARCHAR(500),IsActive BIT NOT NULL DEFAULT 1);
GO
IF OBJECT_ID('dbo.Orders','U') IS NULL CREATE TABLE dbo.Orders(Id INT IDENTITY(1,1) PRIMARY KEY,OrderCode NVARCHAR(40) NOT NULL UNIQUE,UserId INT NULL,CustomerName NVARCHAR(160) NOT NULL,Email NVARCHAR(160),Contact NVARCHAR(40) NOT NULL,Address NVARCHAR(250) NOT NULL,PlanId INT NOT NULL,Status NVARCHAR(40) NOT NULL,ApplicationDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),FeasibilityNote NVARCHAR(500),CONSTRAINT FK_Orders_Users FOREIGN KEY(UserId) REFERENCES dbo.Users(Id),CONSTRAINT FK_Orders_Plans FOREIGN KEY(PlanId) REFERENCES dbo.Plans(Id));
GO
IF OBJECT_ID('dbo.Connections','U') IS NULL CREATE TABLE dbo.Connections(Id INT IDENTITY PRIMARY KEY,AccountId NVARCHAR(40) NOT NULL UNIQUE,UserId INT NULL,CustomerName NVARCHAR(160) NOT NULL,PlanId INT NOT NULL,Status NVARCHAR(40) NOT NULL,ConnectedAt DATETIME2 NOT NULL,AmountDue DECIMAL(12,2) NOT NULL DEFAULT 0,CONSTRAINT FK_Connections_Users FOREIGN KEY(UserId) REFERENCES dbo.Users(Id),CONSTRAINT FK_Connections_Plans FOREIGN KEY(PlanId) REFERENCES dbo.Plans(Id));
GO
IF OBJECT_ID('dbo.Bills','U') IS NULL CREATE TABLE dbo.Bills(Id INT IDENTITY PRIMARY KEY,BillNumber NVARCHAR(50) NOT NULL UNIQUE,AccountId NVARCHAR(40) NOT NULL,UserId INT NULL,Amount DECIMAL(12,2) NOT NULL,Paid DECIMAL(12,2) NOT NULL DEFAULT 0,DueDate DATE NOT NULL,CONSTRAINT FK_Bills_Users FOREIGN KEY(UserId) REFERENCES dbo.Users(Id));
GO
IF OBJECT_ID('dbo.Employees','U') IS NULL CREATE TABLE dbo.Employees(Id INT IDENTITY PRIMARY KEY,Name NVARCHAR(160) NOT NULL,Role NVARCHAR(100) NOT NULL,Email NVARCHAR(160) NOT NULL,Status NVARCHAR(30) NOT NULL DEFAULT 'Active');
GO
IF COL_LENGTH('dbo.Employees','UserId') IS NULL ALTER TABLE dbo.Employees ADD UserId INT NULL;
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name='FK_Employees_Users') ALTER TABLE dbo.Employees ADD CONSTRAINT FK_Employees_Users FOREIGN KEY(UserId) REFERENCES dbo.Users(Id);
GO
GO
IF OBJECT_ID('dbo.Vendors','U') IS NULL CREATE TABLE dbo.Vendors(Id INT IDENTITY PRIMARY KEY,Name NVARCHAR(160) NOT NULL,Contact NVARCHAR(80),Category NVARCHAR(100),Status NVARCHAR(30) NOT NULL DEFAULT 'Active');
GO
IF OBJECT_ID('dbo.RetailShops','U') IS NULL CREATE TABLE dbo.RetailShops(Id INT IDENTITY PRIMARY KEY,Name NVARCHAR(160) NOT NULL,City NVARCHAR(100) NOT NULL,Contact NVARCHAR(80),Status NVARCHAR(30) NOT NULL DEFAULT 'Open');
GO
IF OBJECT_ID('dbo.Equipment','U') IS NULL CREATE TABLE dbo.Equipment(Id INT IDENTITY PRIMARY KEY,Name NVARCHAR(160) NOT NULL,Category NVARCHAR(100) NOT NULL,Quantity INT NOT NULL,ReorderLevel INT NOT NULL,Vendor NVARCHAR(160));
GO
IF OBJECT_ID('dbo.SupportTickets','U') IS NULL CREATE TABLE dbo.SupportTickets(Id INT IDENTITY PRIMARY KEY,UserId INT NULL,Name NVARCHAR(160) NOT NULL,Email NVARCHAR(160) NOT NULL,Message NVARCHAR(2000) NOT NULL,Status NVARCHAR(30) NOT NULL DEFAULT 'Open',CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),CONSTRAINT FK_Support_Users FOREIGN KEY(UserId) REFERENCES dbo.Users(Id));
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_SupportTickets_Status') CREATE INDEX IX_SupportTickets_Status ON dbo.SupportTickets(Status);
GO
GO
-- Seed credentials. Passwords are PBKDF2-SHA256 (120,000 iterations), matching PasswordService.cs.
IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Username='admin') INSERT dbo.Users(Username,PasswordHash,PasswordSalt,FullName,Email,Contact,Address,City,Role) VALUES('admin',0xD686614A7CF7E4C95B17D901A0EBD55C1B990F5951ADC376D6B6163C5B645C20,0xA67AF3D5EE11DABDDA56E694BA435232,'NEXUS Administrator','admin@nexus.local','021-1110000','NEXUS HQ','Karachi','Admin');
-- STAFF LOGIN CREDENTIALS (change them from Admin > Employees after first setup)
-- Technical: tech.usman / Tech@12345 | Accounts: accounts.maham / Accounts@12345 | Retail: retail.bilal / Retail@12345 | Administrator: sara.admin / Admin@12345
IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Username='tech.usman') INSERT dbo.Users(Username,PasswordHash,PasswordSalt,FullName,Email,Contact,Role) VALUES('tech.usman',0x29DA1383D12DAC00AEEBE3FF9A61EF24F9B5666D814E7BCC69E16B668FC1B3C6,0xE5F76A60830647D4FFD227AE5A39B6FD,'Usman Ali','usman@nexus.local','0300-0000001','Technical');
IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Username='accounts.maham') INSERT dbo.Users(Username,PasswordHash,PasswordSalt,FullName,Email,Contact,Role) VALUES('accounts.maham',0x8EEEC8C02C8366B8CE00E8C20D7C7B5C31C90FAC9E91175C4F24CB98722C96E4,0x9C5630AAC2D33AB373741AF9014425EC,'Maham Raza','maham@nexus.local','0300-0000002','Accounts');
IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Username='retail.bilal') INSERT dbo.Users(Username,PasswordHash,PasswordSalt,FullName,Email,Contact,Role) VALUES('retail.bilal',0xBA632E5F912E952F12B1BB0BE4B8AAF4A383700E27428D95A3011784B529FA6F,0x320D7984BBB497A08F3F900C03928862,'Bilal Shah','bilal@nexus.local','0300-0000003','Retail Executive');
IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Username='sara.admin') INSERT dbo.Users(Username,PasswordHash,PasswordSalt,FullName,Email,Contact,Role) VALUES('sara.admin',0x7335F290BE51446998F6330F7C13EFC4F8929A9FB08380121B043C041BF27978,0xD360370FD388F52D6CED917FFB757EA5,'Sara Ahmed','sara@nexus.local','0300-0000004','Admin');
UPDATE e SET UserId=u.Id FROM dbo.Employees e JOIN dbo.Users u ON u.Email=e.Email WHERE e.UserId IS NULL;
IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Username='demo') INSERT dbo.Users(Username,PasswordHash,PasswordSalt,FullName,Email,Contact,Address,City,Role) VALUES('demo',0xDDC37398F2FDBFFE1512C607B30ADA0A9A89358F2269395E90EB3780DF5D2F6D,0x35449F963A7D357EC6A3DDE401E95D88,'Demo Customer','customer@example.com','0300-0000000','Clifton, Karachi','Karachi','Customer');
IF NOT EXISTS (SELECT 1 FROM dbo.Plans) INSERT dbo.Plans(Name,ConnectionType,Price,BillingCycle,Speed,Description) VALUES('Broadband 64 Kbps Unlimited','Broadband',225,'Monthly','64 Kbps','Unlimited broadband connectivity.'),('Broadband 128 Kbps Unlimited','Broadband',350,'Monthly','128 Kbps','Higher-speed unlimited broadband.'),('Dial-Up 28 Kbps Unlimited','DialUp',75,'Monthly','28 Kbps','Unlimited dial-up service.'),('Telephone Local Monthly','Telephone',35,'Monthly','Landline','Monthly telephone rental plus calls.');
IF NOT EXISTS (SELECT 1 FROM dbo.Employees) INSERT dbo.Employees(Name,Role,Email) VALUES('Sara Ahmed','Admin','sara@nexus.local'),('Usman Ali','Technical','usman@nexus.local'),('Maham Raza','Accounts','maham@nexus.local'),('Bilal Shah','Retail Executive','bilal@nexus.local');
IF NOT EXISTS (SELECT 1 FROM dbo.Vendors) INSERT dbo.Vendors(Name,Contact,Category) VALUES('Nexus Hardware Supply','021-5551001','Routers & Modems'),('Network Devices Ltd.','042-5552002','Switches');
IF NOT EXISTS (SELECT 1 FROM dbo.RetailShops) INSERT dbo.RetailShops(Name,City,Contact) VALUES('NEXUS Clifton','Karachi','021-1110001'),('NEXUS Gulberg','Lahore','042-1110002'),('NEXUS Blue Area','Islamabad','051-1110003');
IF NOT EXISTS (SELECT 1 FROM dbo.Equipment) INSERT dbo.Equipment(Name,Category,Quantity,ReorderLevel,Vendor) VALUES('Broadband Router','Router',42,15,'Nexus Hardware Supply'),('ADSL Modem','Modem',9,10,'Nexus Hardware Supply'),('24-Port Switch','Switch',17,5,'Network Devices Ltd.');
IF NOT EXISTS (SELECT 1 FROM dbo.Orders WHERE OrderCode='B0000000001') INSERT dbo.Orders(OrderCode,UserId,CustomerName,Email,Contact,Address,PlanId,Status,ApplicationDate,FeasibilityNote) SELECT 'B0000000001',u.Id,u.FullName,u.Email,u.Contact,u.Address,1,'Feasible',DATEADD(day,-2,SYSUTCDATETIME()),'Area is serviceable. Installation can be scheduled.' FROM dbo.Users u WHERE u.Username='demo';
IF NOT EXISTS (SELECT 1 FROM dbo.Connections WHERE AccountId='B001000000000001') INSERT dbo.Connections(AccountId,UserId,CustomerName,PlanId,Status,ConnectedAt,AmountDue) SELECT 'B001000000000001',u.Id,u.FullName,1,'Active',DATEADD(month,-2,SYSUTCDATETIME()),225 FROM dbo.Users u WHERE u.Username='demo';
IF NOT EXISTS (SELECT 1 FROM dbo.Bills WHERE BillNumber='NEX-2026-0001') INSERT dbo.Bills(BillNumber,AccountId,UserId,Amount,Paid,DueDate) SELECT 'NEX-2026-0001','B001000000000001',u.Id,225,0,DATEADD(day,10,CAST(GETDATE() AS date)) FROM dbo.Users u WHERE u.Username='demo';
GO
