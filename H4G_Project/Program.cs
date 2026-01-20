using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using H4G_Project.DAL;
using H4G_Project.Services;

var builder = WebApplication.CreateBuilder(args);

// ================================
// Firebase initialization (SECURE)
// ================================
if (FirebaseApp.DefaultInstance == null)
{
    var firebaseJson = Environment.GetEnvironmentVariable("FIREBASE_SERVICE_ACCOUNT_JSON");

    if (string.IsNullOrWhiteSpace(firebaseJson))
    {
        throw new Exception("FIREBASE_SERVICE_ACCOUNT_JSON environment variable is not set.");
    }

    FirebaseApp.Create(new AppOptions
    {
        Credential = GoogleCredential.FromJson(firebaseJson)
    });
}

// Services
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<EmailService>();

// Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// MVC
builder.Services.AddControllersWithViews();

builder.Services.AddScoped<UserDAL>();
builder.Services.AddScoped<StaffDAL>();

var app = builder.Build();

// HTTP pipeline
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
