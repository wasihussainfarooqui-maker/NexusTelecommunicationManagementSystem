using Microsoft.Data.SqlClient;
using NexusTelecommunicationManagementSystem.Models;

namespace NexusTelecommunicationManagementSystem.Services;

public sealed record AuthUser(int Id,string Username,string FullName,string Role);
public sealed class AuthService
{
    private readonly string _cs;
    public AuthService(IConfiguration c)=>_cs=c.GetConnectionString("DefaultConnection")!;

    public AuthUser? Login(string username,string password,bool adminOnly=false,bool staffOnly=false)
    {
        using var cn=new SqlConnection(_cs); cn.Open();
        using var cmd=new SqlCommand("SELECT Id,Username,FullName,Role,PasswordHash,PasswordSalt,IsActive FROM dbo.Users WHERE Username=@u",cn);
        cmd.Parameters.AddWithValue("@u",username.Trim());
        using var r=cmd.ExecuteReader();
        if(!r.Read()||!r.GetBoolean(6)) return null;
        var role=r.GetString(3);
        if(adminOnly&&!role.Equals("Admin",StringComparison.OrdinalIgnoreCase)) return null;
        if(staffOnly&&(role.Equals("Customer",StringComparison.OrdinalIgnoreCase)||role.Equals("Admin",StringComparison.OrdinalIgnoreCase))) return null;
        if(!PasswordService.Verify(password,(byte[])r[4],(byte[])r[5])) return null;
        return new(r.GetInt32(0),r.GetString(1),r.GetString(2),role);
    }
    public bool Signup(SignupViewModel m,out string error)
    {
        error="";
        if(m.Password!=m.ConfirmPassword){error="Passwords do not match.";return false;}
        if(m.Password.Length<8){error="Password must be at least 8 characters.";return false;}
        using var cn=new SqlConnection(_cs);cn.Open();
        using(var check=new SqlCommand("SELECT COUNT(*) FROM dbo.Users WHERE Username=@u",cn)){check.Parameters.AddWithValue("@u",m.Username.Trim());if((int)check.ExecuteScalar()!>0){error="Username is already in use.";return false;}}
        var (hash,salt)=PasswordService.Hash(m.Password);
        using var cmd=new SqlCommand("INSERT dbo.Users(Username,PasswordHash,PasswordSalt,FullName,Email,Contact,Address,City,Role) OUTPUT INSERTED.Id VALUES(@u,@h,@s,@n,@e,@c,@a,@city,'Customer')",cn);
        cmd.Parameters.AddWithValue("@u",m.Username.Trim());cmd.Parameters.AddWithValue("@h",hash);cmd.Parameters.AddWithValue("@s",salt);cmd.Parameters.AddWithValue("@n",m.FullName.Trim());cmd.Parameters.AddWithValue("@e",m.Email.Trim());cmd.Parameters.AddWithValue("@c",m.Contact.Trim());cmd.Parameters.AddWithValue("@a",m.Address.Trim());cmd.Parameters.AddWithValue("@city",m.City.Trim());
        _=cmd.ExecuteScalar(); return true;
    }
}