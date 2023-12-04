using DeliveryMonitoring.Models;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages().AddRazorRuntimeCompilation();
builder.Services.AddHttpClient();
builder.Services.AddHttpClient("Delivery", httpClient =>
{
    httpClient.BaseAddress = new Uri("http://196.189.21.67:8084/api");
    httpClient.DefaultRequestHeaders.Add("x-api-key", "c666e0e9-fnnm-5804-bbxo-144ad72ae730");
});
builder.Services.AddHttpClient("DeliveryLogin", httpClient =>
{
    httpClient.BaseAddress = new Uri("");
    //httpClient.DefaultRequestHeaders.Add("x-api-key", "c666e0e9-fnnm-5804-bbxo-144ad72ae730");
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login/Login";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    });

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
