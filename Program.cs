using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using CarBazzar.Models.Entity;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<CarBazaarContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<CarBazaarContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();
builder.Services.AddHttpClient("CarBazaarApi", client => 
{
    client.BaseAddress = new Uri("https://localhost:7291/");
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
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.MapHub<CarBazzar.Hubs.ChatHub>("/chatHub");

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CarBazaarContext>();
    db.Database.ExecuteSqlRaw(@"
        IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='RecentlyViewedCars' and xtype='U')
        CREATE TABLE [dbo].[RecentlyViewedCars] (
            [Id] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
            [UserId] [nvarchar](450) NOT NULL FOREIGN KEY REFERENCES [AspNetUsers]([Id]) ON DELETE NO ACTION,
            [CarId] [int] NOT NULL FOREIGN KEY REFERENCES [Cars]([Id]) ON DELETE NO ACTION,
            [ViewedAt] [datetime2](7) NOT NULL
        );
    ");
}

app.Run();
