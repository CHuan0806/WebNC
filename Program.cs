using Microsoft.EntityFrameworkCore;
using QLNhaSach1;
using QLNhaSach1.Data;
using QLNhaSach1.Hubs;
using QLNhaSach1.Service;
using StackExchange.Redis;
using Microsoft.AspNetCore.SignalR;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Đăng ký DatabaseInitializer
builder.Services.AddScoped<DatabaseInitializer>();

// Đăng ký HttpClient và PaypalService
builder.Services.AddHttpClient();
builder.Services.AddTransient<PaypalService>();

// Thêm session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromDays(1);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = "MySession";
});

// Thêm Authentication với Cookie
builder.Services.AddAuthentication("MyCookieAuth")
    .AddCookie("MyCookieAuth", options =>
    {
        options.LoginPath = "/User/Login";
        options.AccessDeniedPath = "/User/AccessDenied";
        options.Cookie.Name = "MyAuthCookie";
        options.ReturnUrlParameter = "returnUrl";
    });

// Đăng ký Redis
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var config = builder.Configuration.GetSection("Redis")["ConnectionString"];
    var options = ConfigurationOptions.Parse(config, true);
    options.AbortOnConnectFail = false;
    return ConnectionMultiplexer.Connect(options);
});

builder.Services.AddSingleton<CacheService>();

// Đăng ký CustomUserIdProvider cho SignalR
builder.Services.AddSingleton<IUserIdProvider, CustomUserIdProvider>();

// Cấu hình SignalR với CustomUserIdProvider
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapHub<ChatHub>("/chathub");

    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
});

// Khởi tạo tài khoản admin
using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
    await initializer.SeedAdminUserAsync();
}

// Gọi seed data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<AppDbContext>();
    SeedData.Initialize(context);
}

app.Run();
