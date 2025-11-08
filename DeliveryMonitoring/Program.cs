using DeliveryMonitoring.Constants;
using DeliveryMonitoring.Controllers;
using DeliveryMonitoring.Models;
using DeliveryMonitoring.Services.Api;
using DeliveryMonitoring.Services.SecureCookie;
using DeliveryMonitoring.Services.SummaryReport;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient<IApiRequestService, ApiRequestService>();
var configuration = new ConfigurationBuilder()
       .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
       .Build();
var deliveryUri = configuration.GetValue<string>("Delivery");
var deliveryLoginUri = configuration.GetValue<string>("DeliveryLogin");
var CnetApiBaseUrl = configuration.GetValue<string>("CnetApiBaseUrl");
var ApiBaseUrl = configuration.GetValue<string>("ApiBaseUrl");

builder.Services.AddRazorPages().AddRazorRuntimeCompilation();
builder.Services.AddHttpClient();
builder.Services.AddHttpClient("Delivery", httpClient =>
{
    httpClient.BaseAddress = new Uri(deliveryUri);
    httpClient.DefaultRequestHeaders.Add("x-api-key", "c666e0e9-fnnm-5804-bbxo-144ad72ae730");
});
builder.Services.AddHttpClient("CnetApiBaseUrl", httpClient =>
{
    httpClient.BaseAddress = new Uri(CnetApiBaseUrl);
    httpClient.DefaultRequestHeaders.Add("x-api-key", "5D5EAFF4-D29A-485B-BDB9-785EF86FFFAE");
});
builder.Services.AddHttpClient("DeliveryLogin", httpClient =>
{
    httpClient.BaseAddress = new Uri(deliveryLoginUri);
});
builder.Services.AddHttpClient("ApiBaseUrl", httpClient =>
{
    httpClient.BaseAddress = new Uri(ApiBaseUrl);
});

builder.Services.AddAuthentication(CNET_WebConstantes.CookieScheme)
     .AddCookie(CNET_WebConstantes.CookieScheme, options =>
     {
         options.AccessDeniedPath = "/account/denied";
         options.LoginPath = "/verifyId";
     });
builder.Services.AddSession();

builder.Services.AddHttpContextAccessor();
builder.Services.AddDataProtection();
builder.Services.AddScoped<ISecureCookieService, SecureCookieService>();
builder.Services.AddScoped<AuthenticationManager>();
builder.Services.AddScoped<IApiRequestService, ApiRequestService>();
builder.Services.AddScoped<ISummaryReportService, SummaryReportService>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
//Added for Authentication
app.UseAuthentication();
app.UseAuthorization();

app.UseStatusCodePagesWithReExecute("/404", "?path={0}"); // Executes /404 without redirect

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Login}/{id?}");

app.Run();
