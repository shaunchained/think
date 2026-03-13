using ArchSim.Data;
using ArchSim.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddMemoryCache();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Azure SQL Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Authentication: Cookie + Google OAuth
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.LoginPath = "/account/login";
    options.LogoutPath = "/account/logout";
    options.AccessDeniedPath = "/account/access-denied";
    options.Cookie.Name = "think_auth";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
    options.SlidingExpiration = true;
})
.AddGoogle(options =>
{
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"] ?? "";
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? "";
    options.SaveTokens = true;
    options.Scope.Add("profile");
    options.Scope.Add("email");
    options.ClaimActions.MapJsonKey("picture", "picture");
});

builder.Services.AddHttpClient();
builder.Services.AddSingleton<ScenarioLoader>();
builder.Services.AddSingleton<ScoreCalculator>();

var app = builder.Build();

// Auto-migrate database on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();


app.MapControllerRoute(
    name: "scenario_answer",
    pattern: "scenario/{id}/answer",
    defaults: new { controller = "Scenario", action = "Answer" });

app.MapControllerRoute(
    name: "scenario_step",
    pattern: "scenario/{id}/step/{n:int}",
    defaults: new { controller = "Scenario", action = "Step" });

app.MapControllerRoute(
    name: "scenario_score",
    pattern: "scenario/{id}/score",
    defaults: new { controller = "Scenario", action = "Score" });

app.MapControllerRoute(
    name: "scenario_start",
    pattern: "scenario/{id}",
    defaults: new { controller = "Scenario", action = "Start" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
