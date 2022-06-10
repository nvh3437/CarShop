using CarShop.Services.Email;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ShopData;
using ShopData.Model;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
#region Services


builder.Services.AddControllersWithViews();

// Add DbContext
builder.Services.AddDbContext<Context>(option =>
    option.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

// Add Identity
builder.Services.AddIdentity<User, Role>()
    .AddEntityFrameworkStores<Context>()
    .AddDefaultTokenProviders();

//Config Identity
builder.Services.Configure<IdentityOptions>(option =>
{
    option.Password.RequireDigit = false;
    option.Password.RequireUppercase = false;
    option.Password.RequireLowercase = false;
    option.Password.RequiredLength = 8;
    option.Password.RequireNonAlphanumeric = false;

    option.User.RequireUniqueEmail = true;

    option.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(60);
    option.Lockout.MaxFailedAccessAttempts = 5;
    option.Lockout.AllowedForNewUsers = true;

    option.SignIn.RequireConfirmedPhoneNumber = false;
    option.SignIn.RequireConfirmedEmail = false;
});

//Add MailInfo EmailSender
builder.Services.AddOptions();
builder.Services.Configure<MailInfo>(builder.Configuration.GetSection("MailInfo"));
builder.Services.AddTransient<IEmailSender, EmailSender>();

builder.Services.ConfigureApplicationCookie(configure =>
{
    configure.LoginPath = "/Manage/Login";
    configure.LogoutPath = "/Manage/Login/Logout";
    configure.AccessDeniedPath = "/Denied";
});


#endregion
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

//app.UseHttpsRedirection(); ngrok http 5266

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();    // Xác Thực

app.UseAuthorization();   //Phân Quyền

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "Manage",
        pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
    endpoints.MapControllerRoute(
        name: "AutoMaker",
        pattern: "AutoMaker/{id?}",
        new {Controller = "AutoMaker", Action = "Index"});
        
});


app.Run();
