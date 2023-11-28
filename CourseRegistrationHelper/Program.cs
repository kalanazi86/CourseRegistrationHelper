using CourseRegistrationHelper.Data;
using CourseRegistrationHelper.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();

// Add session services to the container
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Set a long timeout for demonstration purposes
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});




var app = builder.Build();

// Seed the Admin role
await SeedRoles(app.Services);

var adminUserName = builder.Configuration.GetValue<string>("AdminUser:UserName");
var adminPassword = builder.Configuration.GetValue<string>("AdminUser:Password");

// Seed the Admin user
await SeedAdminUser(app.Services, adminUserName, adminPassword);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
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

app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
async Task SeedRoles(IServiceProvider serviceProvider)
{
    using var scope = serviceProvider.CreateScope();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    if (!await roleManager.RoleExistsAsync("Admin"))
    {
        await roleManager.CreateAsync(new IdentityRole("Admin"));
    }
}

async Task SeedAdminUser(IServiceProvider serviceProvider, string adminUserName, string adminPassword)
{
    using var scope = serviceProvider.CreateScope();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    // Ensure the Admin role exists
    if (!await roleManager.RoleExistsAsync("Admin"))
    {
        await roleManager.CreateAsync(new IdentityRole("Admin"));
    }

    // Check if the admin user already exists
    var adminUser = await userManager.FindByNameAsync(adminUserName);
    if (adminUser == null)
    {
        // Create the admin user
        adminUser = new ApplicationUser { UserName = adminUserName, Email = adminUserName };
        var result = await userManager.CreateAsync(adminUser, adminPassword);
        if (!result.Succeeded)
        {
            // Log each error
            foreach (var error in result.Errors)
            {
                Console.WriteLine($"Error creating admin user: {error.Description}");
            }
            return;
        }

        // Assign the admin user to the Admin role
        var roleResult = await userManager.AddToRoleAsync(adminUser, "Admin");
        if (!roleResult.Succeeded)
        {
            // Log each error
            foreach (var error in roleResult.Errors)
            {
                Console.WriteLine($"Error assigning role to admin user: {error.Description}");
            }
        }
    }
}
