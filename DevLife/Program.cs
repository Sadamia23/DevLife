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

// Existing services
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
builder.Services.AddScoped<IGitHubAnalysisService, GitHubAnalysisService>();
builder.Services.AddHttpClient<IGitHubAuthService, GitHubAuthService>();
builder.Services.AddHttpClient<IAIGitHubPersonalityService, OpenAIGitHubPersonalityService>();

// DevDating services
builder.Services.AddScoped<IDevDatingService, DevDatingService>();
builder.Services.AddHttpClient<IAIDatingService, OpenAIDatingService>();

builder.Services.AddLogging();
builder.Services.AddScoped<ILogger<GitHubAnalysisService>, Logger<GitHubAnalysisService>>();
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

// Database initialization and seeding
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        await context.Database.EnsureCreatedAsync();

        // Seed dating profiles for demo purposes
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<DatingDataSeeder>>();
        var seeder = new DatingDataSeeder(context, logger);
        await seeder.SeedDatingProfilesAsync();

        Console.WriteLine("✅ Database initialized and dating profiles seeded successfully!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error during database initialization: {ex.Message}");
        throw;
    }
}

app.Run();