using Microsoft.EntityFrameworkCore;
using MinimarketApp.Models;
using Microsoft.AspNetCore.Authentication.Cookies; // 1. IMPORTANTE: Namespace para Login

var builder = WebApplication.CreateBuilder(args);

// Conexión a SQL Server
builder.Services.AddDbContext<MinimarketContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. CONFIGURAR EL SERVICIO DE AUTENTICACIÓN (COOKIES)
// Esto le dice al sistema: "Usa cookies para recordar al usuario, y si no tiene permiso, mándalo al Login"
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(option => {
        option.LoginPath = "/Acceso/Login"; // Ruta de la pantalla de Login
        option.ExpireTimeSpan = TimeSpan.FromMinutes(60); // Tiempo que dura la sesión abierta
        option.AccessDeniedPath = "/Acceso/SinPermiso"; // (Opcional) Si intenta entrar a algo prohibido
    });

builder.Services.AddControllersWithViews();

// Configuración de Sesión (Carrito de Compras)
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// 3. ACTIVAR LOS MOTORES DE SEGURIDAD (EN ORDEN ESTRICTO)
app.UseAuthentication(); // ¿Quién es? (Lee la cookie)
app.UseAuthorization();  // ¿Qué permiso tiene? (Admin o Cajero)
app.UseSession();        // Memoria temporal (Carrito)

app.MapStaticAssets();

// 4. CAMBIAR LA RUTA INICIAL
// Ahora el sistema arranca en el Login, no en el Home.
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Acceso}/{action=Login}/{id?}");

app.Run();