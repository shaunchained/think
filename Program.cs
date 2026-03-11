using thinking1.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// Session requires a distributed cache — in-memory for single-server hosting
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

builder.Services.AddSingleton<ScenarioService>();

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

// Scenario routes must be registered before the default route
// Step needs two segments: id + stepNumber
app.MapControllerRoute(
    name: "scenario_step",
    pattern: "Scenario/Step/{id}/{stepNumber:int}",
    defaults: new { controller = "Scenario", action = "Step" });

app.MapControllerRoute(
    name: "scenario_result",
    pattern: "Scenario/Result/{id}",
    defaults: new { controller = "Scenario", action = "Result" });

app.MapControllerRoute(
    name: "scenario_start",
    pattern: "Scenario/Start/{id}",
    defaults: new { controller = "Scenario", action = "Start" });

app.MapControllerRoute(
    name: "scenario_submit",
    pattern: "Scenario/Submit",
    defaults: new { controller = "Scenario", action = "Submit" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
