using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Data;

// Constructeur d'appli (ou API) web
var builder = WebApplication.CreateBuilder(args);

// Les différents controlleurs 
builder.Services.AddControllers();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=taskflow.db")
  ); // Pour l'instant on va dire que ça passe ¬_¬"

// Documentation Swagger/OpenAPI, comme dans le 3API
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Auth JWT - ça permet de vérifier si une requête peut accéder à un endpoint
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => {
        options.TokenValidationParameters = new TokenValidationParameters {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)
            ),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

// Middleware d'authentification, on utilisera des JWT
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Message d'erreur si la requête finit sur un endpoint qu'existe pô
app.MapFallback(() => Results.NotFound(new {
  code = "404",
  message = "Cette route n'existe pas *_*"
}));

app.Run();
