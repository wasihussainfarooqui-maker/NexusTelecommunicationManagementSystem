# NEXUS Telecommunication Management System — VS 2022 + SQL Server

This folder combines the original **Admin** and **User** projects into one ASP.NET Core MVC application while retaining the original Admin pages and customer pages.

## Requirements
- Visual Studio 2022
- .NET 8 SDK
- SQL Server LocalDB (`(localdb)\\MSSQLLocalDB`) or SQL Server/Express
- SQL Server Management Studio (SSMS), optional but recommended

## Database
The app uses **SQL Server**, not SQLite.

1. Open `Database/Database.sql` in SSMS.
2. Execute it against your SQL Server instance.
3. If you use LocalDB, the included connection string already points to `(localdb)\\MSSQLLocalDB`.
4. To use another SQL Server, edit `appsettings.json`.

The application also verifies/creates the database and schema at startup, so the database can be initialized automatically when the configured SQL Server is reachable.

## Credentials
### Administrator
- Username: `admin`
- Password: `NexusAdmin@2026!`
- Admin signup is intentionally disabled.
- Admin login: `/Account/AdminLogin`

### Demo customer
- Username: `demo`
- Password: `User@12345`

Customers can create accounts from `/Account/Signup`.

## Included original functionality
### Customer side
Home, Plans, Track, My Portal, Bills, Profile, Support, New Connection, order success page, responsive navigation and scroll-reveal animations.

### Admin side
Dashboard, Orders, Connections, Customers, Plans, Billing, Employees, Equipment/Stock, Vendors and Retail Shops. Admin actions include order status changes, connection status changes, customer activation/deactivation, adding customers, adding plans and updating bill payments.

## Authentication
- Server-side ASP.NET Core session authentication
- Separate Customer and Admin roles
- Admin cannot be created through public signup
- PBKDF2-SHA256 password hashing with per-user salt
- Session timeout configured for 8 hours
- Customer orders, bills, connections and support tickets are linked to the signed-in customer

## Visual Studio
Open `NexusTelecommunicationManagementSystem.sln`, restore NuGet packages, then press F5.


## Updated staff access

The project now has a dedicated **Staff Login** and role-based staff workspace.

### Seeded staff credentials
- Technical — `tech.usman` / `Tech@12345`
- Accounts — `accounts.maham` / `Accounts@12345`
- Retail Executive — `retail.bilal` / `Retail@12345`
- Administrator — `sara.admin` / `Admin@12345` (use the Administrator login, not Staff Login)

Change staff passwords from **Admin → Employees** after setup. Passwords are stored using the existing PBKDF2-SHA256 password service.

### AI customer chatbot
The customer portal includes **AI Help** at `/Chat`. It is implemented in C#/.NET 8 using the official OpenAI .NET SDK.

Set the API key as an environment variable:
`OPENAI_API_KEY`

or configure `OpenAI:ApiKey` in appsettings/user-secrets. Do not commit a real API key to source control.

The bot is intentionally scoped to customer-support guidance and does not claim access to private account information.

### Admin editing
The Employees page now supports:
- create staff account
- edit employee name/email/role/status/username
- reset password
- role-based access through Staff Login

Existing customer status, order status, connection status, plan creation and bill-payment controls remain unchanged.

### AI chatbot configuration
The customer chatbot works even when no external AI key is configured: it uses a built-in NEXUS support response engine for common questions. For full generative AI responses, configure `OpenAI:ApiKey` through Visual Studio User Secrets or the `OPENAI_API_KEY` environment variable. Never commit a real API key to source control.


## Role-based staff workspaces
Technical, Accounts, Retail Executive/Sales, Support and Inventory staff now receive separate operational dashboards with server-side role guards and editable work queues. Administrators can manage employee roles, status, usernames and passwords from Admin > Employees.
