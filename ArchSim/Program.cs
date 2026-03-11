using ArchSim.Services;

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

builder.Services.AddSingleton<ScenarioLoader>();
builder.Services.AddSingleton<ScoreCalculator>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
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
