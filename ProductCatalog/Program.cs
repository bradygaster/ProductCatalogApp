var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Register HTTP client for ProductService API
builder.Services.AddHttpClient("ProductService", client =>
{
    var productServiceUrl = builder.Configuration["ProductServiceUrl"] ?? "http://localhost:5000";
    client.BaseAddress = new Uri(productServiceUrl);
});

builder.Services.AddScoped<ProductCatalog.Services.ProductServiceClient>();

// Register Azure Service Bus sender
builder.Services.AddSingleton(sp =>
{
    var connectionString = builder.Configuration["ServiceBus:ConnectionString"];
    if (string.IsNullOrEmpty(connectionString))
    {
        throw new InvalidOperationException("ServiceBus:ConnectionString configuration is required");
    }
    return new Azure.Messaging.ServiceBus.ServiceBusClient(connectionString);
});

builder.Services.AddSingleton<ProductCatalog.Services.OrderQueueService>();

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
