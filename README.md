# Documentation pour le projet 3ASPC
## Téléchargement
Le projet peut être téléchargé comme suit, depuis ce dépôt git :
```
git clone https://github.com/TheDarkWolfer/3ASPC
```
Suite à cela, les dépendances peuvent être téléchargées ainsi :
```
cd 3ASPC

dotnet restore

# Installation des dépendances
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.Sqlite
dotnet add package Microsoft.EntityFrameworkCore.Tools
dotnet add package Swashbuckle.AspNetCore
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer # Authentification
dotnet add package BCrypt.Net-Next                               # Hashage des mots de passe
```

Il faut également téléchargers les outils EntityFramework pour manipuler la DB SQLite :

- Installation de l'outil
```dotnet tool install --global dotnet-ef
```

- Création de la DB
```dotnet ef migrations add InitialCreate
```

- Préparation de la DB
```dotnet ef database update
```

## Choix de certaines technologies
Nous avons choisi `EF Core` pour éviter d'avoir à intéragir en SQL directement avec la base de données : son rôle est celui d'une couche d'abstraction, et bien que moins optimisé que des requêtes SQL directes, cette perte de performances est complètement négligeable dans notre situation
De plus, les DTOs affichiées permettent d'éviter des bourdes comme de renvoyer le mot de passe lors de la création d'utilisateur.ice, par exemple. On les hashe avant de les stocker, mais faut quand même éviter les risques inutile ദ്ദി˵•̀ᴗ-˵ ✧ 

## Lancer l'API
```
dotnet run # Ou dotnet watch run pour relancer à chaque changement
```

L'interface Swagger se trouvera à [localhost:5285](http://localhost:5285/swagger/index.html)

De base, l'API est livrée avec un utilisateur admin, avec les identifiants suivants :

|Username|Email|Password|
|-|-|-|
|admin|admin@acme.fr|secret|

## Authentification
1. POST /api/users/register — créer un compte
2. POST /api/users/login    — récupérer le token JWT
3. Dans Swagger : bouton "Authorize" → Bearer <token>

