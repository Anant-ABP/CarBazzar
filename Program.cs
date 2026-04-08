using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using CarBazzar.Models.Entity;
using CarBazzar.Services;

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
builder.Services.AddScoped<EmailService>(); // Email notification service
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

        IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Bookings' and xtype='U')
        BEGIN
            CREATE TABLE [dbo].[Bookings] (
                [Id] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
                [CarId] [int] NOT NULL FOREIGN KEY REFERENCES [Cars]([Id]) ON DELETE CASCADE,
                [UserId] [nvarchar](450) NOT NULL FOREIGN KEY REFERENCES [AspNetUsers]([Id]) ON DELETE NO ACTION,
                [BookingDate] [datetime2](7) NOT NULL
            );
            CREATE UNIQUE NONCLUSTERED INDEX [IX_Bookings_CarId] ON [dbo].[Bookings] ([CarId]);
            CREATE NONCLUSTERED INDEX [IX_Bookings_UserId] ON [dbo].[Bookings] ([UserId]);
        END
        
        IF COL_LENGTH('dbo.Bookings', 'MobileNumber') IS NULL
        BEGIN
            ALTER TABLE [dbo].[Bookings] ADD [MobileNumber] [nvarchar](max) NULL, [Message] [nvarchar](max) NULL;
        END

        IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Bookings_CarId' AND object_id = OBJECT_ID('dbo.Bookings') AND is_unique = 1)
        BEGIN
            DROP INDEX [IX_Bookings_CarId] ON [dbo].[Bookings];
            CREATE NONCLUSTERED INDEX [IX_Bookings_CarId] ON [dbo].[Bookings] ([CarId]);
        END
        
        IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Bookings_CarId_UserId' AND object_id = OBJECT_ID('dbo.Bookings'))
        BEGIN
            CREATE UNIQUE NONCLUSTERED INDEX [IX_Bookings_CarId_UserId] ON [dbo].[Bookings] ([CarId], [UserId]);
        END
    ");
    
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    string adminEmail = "kpaghadal936@rku.ac.in";
    string adminPass = "Kris@123";
    
    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        adminUser = new ApplicationUser 
        { 
            UserName = adminEmail, 
            Email = adminEmail, 
            FirstName = "Admin", 
            LastName = "User",
            EmailConfirmed = true
        };
        await userManager.CreateAsync(adminUser, adminPass);
    }
    else
    {
        var token = await userManager.GeneratePasswordResetTokenAsync(adminUser);
        await userManager.ResetPasswordAsync(adminUser, token, adminPass);
    }
}

app.Run();
