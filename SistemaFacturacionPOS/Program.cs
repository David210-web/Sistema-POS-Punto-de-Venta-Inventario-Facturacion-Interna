using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using SistemaFacturacionPOS.Contexto;
using SistemaFacturacionPOS.Services;
using SistemaFacturacionPOS.Services.Interfaces;
using SistemaFacturacionPOS.Repositories;
using SistemaFacturacionPOS.Repositories.Interfaces;
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

// ── Repositorios ────────────────────────────────────────────────────────────
builder.Services.AddScoped<ILoginRepository,          LoginRepository>();
builder.Services.AddScoped<IPOSRepository,            POSRepository>();
builder.Services.AddScoped<ICajaRepository,           CajaRepository>();
builder.Services.AddScoped<IFacturacionRepository,    FacturacionRepository>();
builder.Services.AddScoped<IProductosRepository,      ProductosRepository>();
builder.Services.AddScoped<IBodegasRepository,        BodegasRepository>();
builder.Services.AddScoped<IProductoBodegaRepository, ProductoBodegaRepository>();
builder.Services.AddScoped<IUsuarioRepository,        UsuarioRepository>();
builder.Services.AddScoped<IRolesRepository,          RolesRepository>();
builder.Services.AddScoped<IHomeRepository,           HomeRepository>();
builder.Services.AddScoped<ILogsRepository,           LogsRepository>();
builder.Services.AddScoped<IConfiguracionRepository,  ConfiguracionRepository>();

// ── Servicios de dominio ─────────────────────────────────────────────────────
builder.Services.AddScoped<ILoginService,          LoginService>();
builder.Services.AddScoped<IPOSService,            POSService>();
builder.Services.AddScoped<ICajaService,           CajaService>();
builder.Services.AddScoped<IFacturacionService,    FacturacionService>();
builder.Services.AddScoped<IProductosService,      ProductosService>();
builder.Services.AddScoped<IBodegasService,        BodegasService>();
builder.Services.AddScoped<IProductoBodegaService, ProductoBodegaService>();
builder.Services.AddScoped<IUsuarioService,        UsuarioService>();
builder.Services.AddScoped<IRolesService,          RolesService>();
builder.Services.AddScoped<IHomeService,           HomeService>();
builder.Services.AddScoped<ILogsService,           LogsService>();
builder.Services.AddScoped<IConfiguracionService,  ConfiguracionService>();

builder.Services.AddDbContext<SistemaFacturacionPOSContext>((serviceProvider, options) =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
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
