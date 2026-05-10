using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using SistemaFacturacionPOS.Contexto;
using SistemaFacturacionPOS.Services;
using SistemaFacturacionPOS.Interceptors;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<AuditInterceptor>();

builder.Services.AddDbContext<SistemaFacturacionPOSContext>((serviceProvider, options) =>
{
    options.UseSqlServer("Server=localhost;Database=SistemaPOS;User Id=sa;Password=FCb@rcelona2002;TrustServerCertificate=True;");
    options.AddInterceptors(serviceProvider.GetRequiredService<AuditInterceptor>());
});
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login/Index";
        options.AccessDeniedPath = "/Login/AccesoDenegado";
        options.ExpireTimeSpan = TimeSpan.FromHours(8); // Sesión de 8 horas
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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
