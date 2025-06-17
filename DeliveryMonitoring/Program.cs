using CNET_ERP_V7.WebConstants;
using DeliveryMonitoring.Controllers;
using DeliveryMonitoring.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllersWithViews();
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
         options.LoginPath = "/login";
     });
builder.Services.AddSession();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<AuthenticationManager>();

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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Login}/{id?}");

app.Run();
