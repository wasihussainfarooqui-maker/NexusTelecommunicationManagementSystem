using Microsoft.AspNetCore.Mvc;
using NexusTelecommunicationManagementSystem.Models;
using NexusTelecommunicationManagementSystem.Services;

namespace NexusTelecommunicationManagementSystem.Controllers;

public class AccountController : Controller
{
    private readonly AuthService _auth;
    public AccountController(AuthService auth) => _auth = auth;
    [HttpGet] public IActionResult Login(string? returnUrl=null) { ViewBag.ReturnUrl=returnUrl; return View(new LoginViewModel()); }
    [HttpPost][ValidateAntiForgeryToken] public IActionResult Login(LoginViewModel model,string? returnUrl=null)
    { model.IsAdmin=false; var user=_auth.Login(model.Username,model.Password); if(user is null || !user.Role.Equals("Customer",StringComparison.OrdinalIgnoreCase)){ModelState.AddModelError("","Invalid customer username or password.");return View(model);} SignIn(user); return RedirectToLocal(returnUrl,"/Dashboard/Index"); }
    [HttpGet] public IActionResult AdminLogin()=>View("AdminLogin",new LoginViewModel{IsAdmin=true});
    [HttpPost][ValidateAntiForgeryToken] public IActionResult AdminLogin(LoginViewModel model)
    {model.IsAdmin=true;var user=_auth.Login(model.Username,model.Password,true);if(user is null){ModelState.AddModelError("","Invalid administrator credentials.");return View("AdminLogin",model);} SignIn(user);return RedirectToAction("Index","Admin");}
    [HttpGet] public IActionResult StaffLogin()=>View(new LoginViewModel());
    [HttpPost][ValidateAntiForgeryToken] public IActionResult StaffLogin(LoginViewModel model)
    {var user=_auth.Login(model.Username,model.Password,staffOnly:true);if(user is null){ModelState.AddModelError("","Invalid or inactive staff credentials.");return View(model);}SignIn(user);return RedirectToAction("Dashboard","Staff");}
    [HttpGet] public IActionResult Signup()=>View(new SignupViewModel());
    [HttpPost][ValidateAntiForgeryToken] public IActionResult Signup(SignupViewModel model){if(!ModelState.IsValid)return View(model);if(!_auth.Signup(model,out var error)){ModelState.AddModelError("",error);return View(model);}var user=_auth.Login(model.Username,model.Password)!;SignIn(user);return RedirectToAction("Index","Dashboard");}
    [HttpPost][ValidateAntiForgeryToken] public IActionResult Logout(){HttpContext.Session.Clear();return RedirectToAction("Index","Home");}
    public IActionResult AccessDenied()=>View();
    private void SignIn(AuthUser u){HttpContext.Session.SetInt32("UserId",u.Id);HttpContext.Session.SetString("Username",u.Username);HttpContext.Session.SetString("FullName",u.FullName);HttpContext.Session.SetString("Role",u.Role);}
    private IActionResult RedirectToLocal(string? url,string fallback)=>!string.IsNullOrWhiteSpace(url)&&Url.IsLocalUrl(url)?Redirect(url):Redirect(fallback);
}