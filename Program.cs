using QuestPDF.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
QuestPDF.Settings.License = LicenseType.Community;

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add session services
builder.Services.AddDistributedMemoryCache();
//add session function it is used to configure
//it allows you to remove, add
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // session timeout if the user is not active for 30 min the user must login again
    options.Cookie.HttpOnly = true;//done to prevent scripting
    options.Cookie.IsEssential = true;
});
//
var app = builder.Build();

// Use session middleware
app.UseSession();

// MVC pipeline
app.UseStaticFiles();
app.UseRouting();// determines how incoming requests are directed
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();// application can respond to the http host