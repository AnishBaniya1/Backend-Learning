using System.Text;
using Backend.Data;
using Backend.Models;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;


//This initializes the web application and gives access to configuration, services, etc.
var builder = WebApplication.CreateBuilder(args);
Env.Load();

//Get JWT settings from .env
var key = Environment.GetEnvironmentVariable("JWT__SecurityKey");

//This registers controller support in the dependency injection (DI) container.
//Without this, your app won’t know how to handle routes like /api/products.
builder.Services.AddControllers();


//Configures Entity Framework to use SQL Server with a connection string called "DefaultConnection".
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


//Sets up ASP.NET Core Identity (for user authentication/registration) and links it to your database (AppDbContext).
builder.Services.AddIdentityCore<AppUser>().AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders();

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
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key!))
    };
});//This is how ASP.NET ensures only tokens you generate are accepted.


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Connectify API", Description = "Chatting App", Version = "v1" });
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

app.UseHttpsRedirection();
app.UseAuthentication();//Validates JWT tokens on incoming requests.
app.UseAuthorization();//Checks if the authenticated user has permission to access the endpoint.

app.MapControllers();//map API routes
app.Run();


