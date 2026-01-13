using ProductCatalog.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add session support for shopping cart
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Add HTTP context accessor for accessing session in views
builder.Services.AddHttpContextAccessor();

// Register OrderQueueService
builder.Services.AddScoped<OrderQueueService>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var queuePath = configuration["AppSettings:OrderQueuePath"] ?? @".\Private$\ProductCatalogOrders";
    return new OrderQueueService(queuePath);
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

app.UseAuthorization();

app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
