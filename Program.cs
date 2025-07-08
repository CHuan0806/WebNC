using Microsoft.EntityFrameworkCore;
using QLNhaSach1;
using QLNhaSach1.Data; // thêm nếu cần

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
    options.IdleTimeout = TimeSpan.FromDays(1);     // Giữ session trong 7 ngày không hoạt động
    options.Cookie.HttpOnly = true;                 // Bảo mật: cookie không đọc được bằng JavaScript
    options.Cookie.IsEssential = true;              // Cho phép hoạt động kể cả khi người dùng không chấp nhận cookie
    options.Cookie.Name = "MySession";              // Tên cookie session
});

// ✅ Thêm Authentication với Cookie
builder.Services.AddAuthentication("MyCookieAuth")
    .AddCookie("MyCookieAuth", options =>
    {
        options.LoginPath = "/User/Login";              // Nếu chưa đăng nhập
        options.AccessDeniedPath = "/User/AccessDenied";
        options.Cookie.Name = "MyAuthCookie";
        options.ReturnUrlParameter = "returnUrl";       // Hỗ trợ redirect quay lại

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


// Khởi tạo tài khoản admin
using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
    await initializer.SeedAdminUserAsync();
}

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Gọi seed data tại đây
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<AppDbContext>();
    SeedData.Initialize(context);
}

app.Run();
