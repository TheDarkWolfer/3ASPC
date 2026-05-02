using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
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

// Ajout du bouton "Authorization" sur Swagger, pour les tests sur les différents endpoints
// Ça m'a pris une HEURE cette horreur, je vais partir élever des chèvres dans le Jura, par
// la Lune !!!
builder.Services.AddSwaggerGen(options => {
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Entre ton token JWT ici"
    });

    // On précise bien le type de token qu'on lui donne dans cette boite de dialogue
    options.AddSecurityRequirement(new OpenApiSecurityRequirement {
        {
            new OpenApiSecurityScheme {
                Reference = new OpenApiReference {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
	    // De base, string vide vu qu'on peut pas recréer le token à chaque lancement
            Array.Empty<string>()
        }
    });
});


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

// Swagger me sauve la vie quand aux tests d'endpoints en vrai, c'est TELLEMENT utile つ╥﹏╥つ
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
