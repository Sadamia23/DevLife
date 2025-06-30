using DevLife.Database;
using DevLife.Extensions;
using DevLife.Services.Implementation;
using DevLife.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          policy.WithOrigins("http://localhost:4200")
                                .AllowAnyHeader()
                                .AllowAnyMethod()
                                .AllowCredentials();
                      });
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddScoped<ICodeCasinoService, CodeCasinoService>();
builder.Services.AddScoped<IBugChaseService, BugChaseService>();
builder.Services.AddScoped<ICodeRoastService, CodeRoastService>();

builder.Services.AddScoped<ICodeFormatterService, CodeFormatterService>();

builder.Services.AddScoped<IMeetingExcuseService, MeetingExcuseService>();
builder.Services.AddScoped<IAIMeetingExcuseService, OpenAIMeetingExcuseService>();

builder.Services.AddHttpClient<IAIChallengeService, OpenAIChallengeService>();
builder.Services.AddHttpClient<IAICodeRoastService, OpenAICodeRoastService>();

builder.Services.AddHttpClient<IAIMeetingExcuseService, OpenAIMeetingExcuseService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.None; 
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; 
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseCors(MyAllowSpecificOrigins);

app.UseSession();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    try
    {
        await context.Database.EnsureCreatedAsync();

        Console.WriteLine("🗄️  Database connection established successfully!");

        Console.WriteLine("🌱 Starting database seeding...");

        await context.SeedDatabaseAsync();
        Console.WriteLine("✅ Basic data seeded successfully!");

        await context.SeedCodeRoastTasksAsync();
        Console.WriteLine("🔥 Code Roast tasks seeded successfully!");

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

        if (app.Environment.IsDevelopment())
        {
            Console.WriteLine($"Full exception: {ex}");
        }

        throw; 
    }
}


Console.WriteLine("\n🎉 DevLife Backend is running!");
Console.WriteLine("🌐 Frontend: http://localhost:4200");
Console.WriteLine("🔧 Backend API: https://localhost:7276");
Console.WriteLine("📖 Swagger UI: https://localhost:7276/swagger");
Console.WriteLine("\n🎮 Ready for some developer fun!");

app.Run();