using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SmartParcel.API.Data;
using System; // For InvalidOperationException
using System.Text;
using SmartParcel.API.Services.Implementations;
using SmartParcel.API.Services.Interfaces;
using System.Drawing;
using System.Runtime.InteropServices;

var builder = WebApplication.CreateBuilder(args);
if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
{
    AppContext.SetSwitch("System.Drawing.EnableUnixSupport", true);
}

// ✅ 1. Configure EF Core with PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ✅ 2. Configure Response Compression (Move this before app.Build())
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
});

// Add this line with your other service registrations
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ITamperHandler, TamperHandler>();
builder.Services.AddScoped<IPricingService, PricingService>();

// Add System.Drawing configuration for cross-platform support
if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
{
    AppContext.SetSwitch("System.Drawing.EnableUnixSupport", true);
}

// ✅ 2. Configure JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"], // FIX: Changed to Jwt:Audience
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]
                    ?? throw new InvalidOperationException("JWT key is missing"))),

            // ✅ Matches the updated AuthController claim format
            RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
        };
    });

// ✅ 3. Configure Controllers & Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter 'Bearer' [space] and then your valid token."
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// ✅ 4. Enable CORS
// Define a specific policy name
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          policy.WithOrigins("http://localhost:5173", // Your React app's development URL
                                             "http://127.0.0.1:5173") // Often useful to include both
                                .AllowAnyHeader()
                                .AllowAnyMethod()
                                .AllowCredentials(); // FIX: Added AllowCredentials for auth headers
                      });

    options.AddPolicy("AllowAll",
        builder =>
        {
            builder
                .SetIsOriginAllowed(origin => true) // Instead of AllowAnyOrigin
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials(); // Now compatib
        });
  
    });

    var app = builder.Build();

    // ✅ Enable Swagger only in development
    //if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();

    }

// ✅ Middleware order matters!
// FIX: Use the named CORS policy
app.UseResponseCompression();
app.UseHttpsRedirection(); // Move this before CORS
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// ✅ Apply EF Core migrations to the Render DB
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();
}

app.Run();


