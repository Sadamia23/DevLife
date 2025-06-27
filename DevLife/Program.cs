// Complete Program.cs with Meeting Excuse Generator and AI Integration
using DevLife.Database;
using DevLife.Extensions;
using DevLife.Services.Implementation;
using DevLife.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Define a specific CORS policy name
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

// --- Add services to the container ---
// =================================================================
// === CONFIGURE CORS (Cross-Origin Resource Sharing) ==============
// =================================================================
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          policy.WithOrigins("http://localhost:4200") // Allow your Angular app's origin
                                .AllowAnyHeader()
                                .AllowAnyMethod()
                                .AllowCredentials(); // Important for session cookies
                      });
});

// =================================================================
// Configure DbContext (assuming SQL Server, update connection string in appsettings.json)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// =================================================================
// === REGISTER CUSTOM SERVICES FOR DEPENDENCY INJECTION ==========
// =================================================================

// Authentication Service
builder.Services.AddScoped<IAuthService, AuthService>();

// Game Services
builder.Services.AddScoped<ICodeCasinoService, CodeCasinoService>();
builder.Services.AddScoped<IBugChaseService, BugChaseService>();
builder.Services.AddScoped<ICodeRoastService, CodeRoastService>();

// Utility Services
builder.Services.AddScoped<ICodeFormatterService, CodeFormatterService>();

// NEW: Meeting Excuse Services
builder.Services.AddScoped<IMeetingExcuseService, MeetingExcuseService>();
builder.Services.AddScoped<IAIMeetingExcuseService, OpenAIMeetingExcuseService>();

// =================================================================
// === REGISTER HTTP CLIENTS FOR AI SERVICES ======================
// =================================================================

// Register HttpClient for existing OpenAI services
builder.Services.AddHttpClient<IAIChallengeService, OpenAIChallengeService>();
builder.Services.AddHttpClient<IAICodeRoastService, OpenAICodeRoastService>();

// NEW: Register HttpClient for AI Meeting Excuse Service
builder.Services.AddHttpClient<IAIMeetingExcuseService, OpenAIMeetingExcuseService>();

// =================================================================
// === CONFIGURE WEB API SERVICES ==================================
// =================================================================

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// === CONFIGURE SESSION STATE SERVICES ============================
// 1. Add a distributed memory cache, which is required for in-memory session storage.
builder.Services.AddDistributedMemoryCache();

// 2. Add the session service to the dependency injection container and configure its options.
builder.Services.AddSession(options =>
{
    // Set a timeout for the session. The session will expire if idle for this duration.
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    // === FIX: CONFIGURE SESSION COOKIE FOR CROSS-ORIGIN (CORS) ===
    // This is crucial for the browser to send the session cookie from the
    // Angular app (localhost:4200) to the .NET API (localhost:7276).
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.None; // Allow cross-site cookie
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Requires HTTPS
});

var app = builder.Build();

// --- Configure the HTTP request pipeline ---
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    // Use the developer exception page to see detailed errors during development.
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseRouting();

// =================================================================
// === ENABLE THE CORS MIDDLEWARE ==================================
// =================================================================
//
// Apply the CORS policy. This should be placed after UseRouting()
// and before UseAuthorization() and MapControllers().
app.UseCors(MyAllowSpecificOrigins);

// =================================================================
// === ENABLE THE SESSION MIDDLEWARE ===============================
// This MUST be called after UseRouting() and before MapControllers().
app.UseSession();

app.MapControllers();

// =================================================================
// === DATABASE SEEDING ============================================
// =================================================================
// Seed the database with initial data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    try
    {
        // Ensure database is created
        await context.Database.EnsureCreatedAsync();

        Console.WriteLine("🗄️  Database connection established successfully!");

        // Seed existing data
        Console.WriteLine("🌱 Starting database seeding...");

        await context.SeedDatabaseAsync();
        Console.WriteLine("✅ Basic data seeded successfully!");

        // Seed Code Roast tasks
        await context.SeedCodeRoastTasksAsync();
        Console.WriteLine("🔥 Code Roast tasks seeded successfully!");

        // NEW: Seed Meeting Excuses
        await context.SeedMeetingExcusesAsync();
        Console.WriteLine("🎭 Meeting Excuses seeded successfully!");

        Console.WriteLine("🚀 DevLife application ready to roll!");
        Console.WriteLine("📝 Available features:");
        Console.WriteLine("   • Code Casino - Gambling with code challenges");
        Console.WriteLine("   • Bug Chase - Run from the bugs!");
        Console.WriteLine("   • Code Roast - Get your code brutally reviewed");
        Console.WriteLine("   • Meeting Excuse Generator - Escape boring meetings! (NEW)");
        Console.WriteLine("   • AI-Powered Excuses - Creative, personalized humor! (NEW)");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error during database setup: {ex.Message}");

        // Log the full exception in development
        if (app.Environment.IsDevelopment())
        {
            Console.WriteLine($"Full exception: {ex}");
        }

        throw; // Re-throw to prevent app startup with broken database
    }
}

// =================================================================
// === APPLICATION STARTUP MESSAGE =================================
// =================================================================

Console.WriteLine("\n🎉 DevLife Backend is running!");
Console.WriteLine("🌐 Frontend: http://localhost:4200");
Console.WriteLine("🔧 Backend API: https://localhost:7276");
Console.WriteLine("📖 Swagger UI: https://localhost:7276/swagger");
Console.WriteLine("\n🎮 Ready for some developer fun!");

app.Run();