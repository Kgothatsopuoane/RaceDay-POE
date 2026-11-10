

var builder = WebApplication.CreateBuilder(args);

// Add MVC services
builder.Services.AddControllersWithViews();

builder.Services.AddScoped<RaceDayMVC.Services.ApiService>();

// Allow ApiService to access the current MVC session
builder.Services.AddHttpContextAccessor();

// Configure session services
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Configure connection to RaceDayAPI
builder.Services.AddHttpClient("RaceDayAPI", client =>
{
    var apiUrl = builder.Configuration["ApiSettings:BaseUrl"];

    client.BaseAddress = new Uri(apiUrl!);
});

var app = builder.Build();

// Configure application pipeline
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
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
