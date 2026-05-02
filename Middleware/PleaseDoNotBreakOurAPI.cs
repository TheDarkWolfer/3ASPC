using System.Net;
using System.Text.Json;

// L'idée c'est d'avoir un 500, mais un 500 qui va pas surprendre l'utilsiateur.ice avec 
// un stack trace incompréhensible, alors on fait un truc simple.
namespace TaskFlow3ASPC.Middlewares {
    public class ErrorHandlingMiddleware(RequestDelegate next) {
      public async Task Invoke(HttpContext context) {
	try {
	  await next(context);
	} catch (Exception ex) {
	  context.Response.StatusCode = 500; // Si la situation était un peu moins professionelle, j'aurais utilisé le 418 : am a teapot
	  await context.Response.WriteAsJsonAsync(new {
	    code    = 500,
	    message = "Une erreur interne est survenue *_*",
	    detail  = ex.Message, // le message sans jeter tous les logs à l'utilisateur.ice
	    cat = "https://http.cat/status/500"
	  });
	}
      }
    }
  }
