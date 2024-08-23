using KMU.HisOrder.MVC.Models;
using KMU.HisOrder.MVC.Controllers;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using NuGet.Packaging;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using KMU.HisOrder.MVC.Hubs;
using System.Buffers.Text;
using System.Text;
using System.Security.Cryptography;

//var builder = WebApplication.CreateBuilder(args);
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddRazorPages();

var target = builder.Configuration.GetConnectionString("Target");
var connectionString = builder.Configuration.GetConnectionString("LocalServer");
switch (target)
{
    case "LocalServer":
        connectionString = builder.Configuration.GetConnectionString("LocalServer");
        break;
    case "HGHTestServer":
        connectionString = builder.Configuration.GetConnectionString("HGHTestServer");
        break;

    case "HGHProductionServer":
        connectionString = builder.Configuration.GetConnectionString("HGHProductionServer");
        break;


    case "BOLocalServer":
        connectionString = builder.Configuration.GetConnectionString("BOLocalServer");
        break;

    case "BOProductionServer":
        connectionString = builder.Configuration.GetConnectionString("BOProductionServer");
        break;


    case "Backup":
        connectionString = builder.Configuration.GetConnectionString("Backup");
        break;
    case "test":
        connectionString = builder.Configuration.GetConnectionString("test");
        break;
    case "MoHD":
        connectionString = builder.Configuration.GetConnectionString("MoHD");
        break;

    default:
        break;
}

var EncryptionType = builder.Configuration.GetConnectionString("EncryptionType");

switch (EncryptionType) 
{
    case "Base64":
        connectionString = Encoding.UTF8.GetString(Convert.FromBase64String(connectionString));
        break;


    case "AES256":
        //參考資料 [Day14] 資料使用安全(保護連接字串)上 - iT 邦幫忙::一起幫忙解決難題，拯救 IT 人的一天 https://ithelp.ithome.com.tw/articles/10187947
        var AES256_Key = builder.Configuration.GetSection("ConnectionStrings")["Key"];//加密金鑰(32 Byte)
        var AES256_IV = builder.Configuration.GetSection("ConnectionStrings")["IV"];//初始向量(Initial Vector, iv) 類似雜湊演算法中的加密鹽(16 Byte)

        connectionString = LoginController.AES256(connectionString, AES256_Key, AES256_IV, false);//Do AES256 Decryption
        break;


    default:
        break;
}



builder.Services.AddDbContext<KMUContext>(options =>
    options.UseNpgsql(connectionString));

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

builder.Services.AddAutoMapper(typeof(Program));

builder.Services.AddControllersWithViews();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(360);
});

//builder.Services.ConfigureApplicationCookie(options => {

//    options.Cookie.Name = ".AspNetCore.Session";
//    options.ExpireTimeSpan = TimeSpan.FromSeconds(10);
//    options.LoginPath = new PathString("/Login/NotLogin");

//});

builder.Services.AddLocalization(options =>
{
    options.ResourcesPath = "Resources";
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(option =>
{
    //未登入時會自動導到這個網址
    option.LoginPath = new PathString("/Login/NotLogin");

    //登入成功，但瀏覽到無權限網址時導入
    option.AccessDeniedPath = new PathString("/Login/NotAuth");
    //參考資料 
    //【12.身分驗證】ASP.NET Core Web API 入門教學(12_2) - 身分驗證和登入期限 - YouTube
    //https://www.youtube.com/watch?v=TCHqjhGvclU
    //設定登入後cookie有效時間【秒數】
    //option.ExpireTimeSpan = TimeSpan.FromSeconds(5);//5秒後自動失效登出
    //參考資料
    //在沒有 ASP.NET Core的情況下使用 cookie 驗證 Identity | Microsoft Learn
    //https://learn.microsoft.com/zh-tw/aspnet/core/security/authentication/cookie?view=aspnetcore-6.0
});

//全專案都適用登入驗證
//參考資料
//ASP.NET Core Web API 入門教學 - 使用 cookie 驗證但不使用 ASP.NET Core Identity（實作登入登出） | 凱哥寫程式's Blog | TalllKai
//https://blog.talllkai.com/ASPNETCore/2021/08/22/CookieAuthentication
builder.Services.AddMvc(options =>
{
    //如須例外排除不需要驗證，請加上[AllowAnonymous]
    options.Filters.Add(new AuthorizeFilter());
});
//全專案都適用登入驗證

//加入 SignalR 2023.01.03 add by elain
builder.Services.AddSignalR();


if (!builder.Environment.IsDevelopment())
{
    builder.Services.AddHttpsRedirection(options =>
    {
        options.RedirectStatusCode = (int)HttpStatusCode.PermanentRedirect;
        options.HttpsPort = 443;
    });
}

builder.WebHost.UseSetting("https_port", "443");


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    //app.UseHsts();
}


//2022.11.07 add by 1050325
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});


var supportedCultures = new List<CultureInfo>()
            {
                new CultureInfo("zh"),
                new CultureInfo("en"),
            };

app.UseRequestLocalization(new RequestLocalizationOptions()
{

    DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture("zh"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures,
});



app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

//-----------------------
app.UseSession();
//順序要一樣
app.UseCookiePolicy();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();
//-----------------------

//app.Urls.AddRange(new List<string>() { "http://*:5000","https://localhost:9037/" });

//app.Urls.AddRange(new List<string>() { "http://*:5001", "https://*:443/" });


app.MapControllerRoute(
    name: "MyArea",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}");

//加入 Hub 2023.01.03 add by elain
app.MapHub<ChatHub>("/chatHub");

app.Run();

