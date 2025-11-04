using System.Text;
using Backend.Data;
using Backend.Models;
using Backend.Services;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;


//This initializes the web application and gives access to configuration, services, etc.
var builder = WebApplication.CreateBuilder(args);
Env.Load();

// Register CORS (Cross-Origin Resource Sharing) service
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        // Allow requests coming from Angular's local development server
        builder.WithOrigins("http://localhost:4200", "https://localhost:4200")
        // Allow any HTTP header (like Authorization, Content-Type, etc.)
         .AllowAnyHeader()
         // Allow all HTTP methods (GET, POST, PUT, DELETE, etc.)
         .AllowAnyMethod()
         // Allow sending cookies or authorization headers across origins
         .AllowCredentials();
    });
});

//Get JWT settings from .env
var key = Environment.GetEnvironmentVariable("JWT__SecurityKey");
//var JwtSetting = builder.Configuration.GetSection("JWTSetting); 

//This registers controller support in the dependency injection (DI) container.
//Without this, your app won’t know how to handle routes like /api/products.
builder.Services.AddControllers();

//This loads all the value from .env file and can be used from IConfiguration
builder.Configuration.AddEnvironmentVariables();


//Configures Entity Framework to use SQL Server with a connection string called "DefaultConnection".
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


//Sets up ASP.NET Core Identity (for user authentication/registration) and links it to your database (AppDbContext).
builder.Services.AddIdentityCore<AppUser>().AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders();

//This allows you to inject TokenService anywhere (like in controllers).
builder.Services.AddScoped<TokenService>();

//This tells ASP.NET:
//“Use JWT Bearer Tokens as the default method for authentication.”
// Now, any [Authorize] attribute in your controllers will expect a valid JWT token.
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;

}).AddJwtBearer(options => //This configures how JWT tokens are validated:
{
    options.SaveToken = true;//Keeps the token after a user logs in (so you can access it later if needed).
    options.RequireHttpsMetadata = false;//Allows HTTP in development mode (otherwise HTTPS required).
    options.TokenValidationParameters = new TokenValidationParameters //Tells ASP.NET how to check if a token is valid:
    {
        ValidateIssuerSigningKey = true,//Ensures the token was signed with the correct secret key.

        //The actual key
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key!)),
        ValidateIssuer = false,
        ValidateAudience = false
    };

    options.Events = new JwtBearerEvents
    {
        // This event fires whenever ASP.NET receives a request and looks for a JWT token.
        OnMessageReceived = context =>
        {
            // Extract the "access_token" from the query string (used in SignalR).
            var accessToken = context.Request.Query["access_token"];
            // Get the request path (URL being accessed)
            var path = context.HttpContext.Request.Path;
            // Check if the token exists and the request is targeting the SignalR hub endpoint
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
            {
                // Assign the token manually so ASP.NET can authenticate the SignalR connection
                context.Token = accessToken;
            }
            // Complete the task (no async operation here)
            return Task.CompletedTask;
        }
    };
});//This is how ASP.NET ensures only tokens you generate are accepted.


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Connectify API", Description = "Chatting App", Version = "v1" });
    // ✅ Tell Swagger about JWT Bearer tokens
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' followed by your token. Example: Bearer eyJhbGciOi..."
    });

    // ✅ Apply it globally to all endpoints
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});
// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Connectify API V1");
    });
}
// Enable CORS (Cross-Origin Resource Sharing) for incoming HTTP requests
// This must be placed BEFORE app.UseAuthentication() and app.UseAuthorization()
app.UseCors(x => x.AllowAnyHeader().AllowAnyMethod().AllowCredentials().
WithOrigins("http://localhost:4200", "https://localhost:4200"));

app.UseHttpsRedirection();
app.UseAuthentication();//Validates JWT tokens on incoming requests.
app.UseAuthorization();//Checks if the authenticated user has permission to access the endpoint.
app.UseStaticFiles();
app.MapControllers();//map API routes
app.Run();


