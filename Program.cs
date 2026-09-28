using NexusTelecommunicationManagementSystem.Services;
var builder=WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(o=>{o.IdleTimeout=TimeSpan.FromHours(8);o.Cookie.HttpOnly=true;o.Cookie.IsEssential=true;});
builder.Services.AddSingleton<DatabaseInitializer>();
builder.Services.AddScoped<NexusService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddSingleton<AiChatService>();
var app=builder.Build();
using(var scope=app.Services.CreateScope()){await scope.ServiceProvider.GetRequiredService<DatabaseInitializer>().InitializeAsync();}
if(!app.Environment.IsDevelopment()){app.UseExceptionHandler("/Home/Error");app.UseHsts();}
app.UseHttpsRedirection();app.UseStaticFiles();app.UseRouting();app.UseSession();
app.MapControllerRoute(name:"default",pattern:"{controller=Home}/{action=Index}/{id?}");
app.Run();
