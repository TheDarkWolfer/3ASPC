# Documentation pour le projet 3ASPC
## Téléchargement
Le projet peut être téléchargé comme suit, depuis ce dépôt git :
```
git clone https://github.com/TheDarkWolfer/3ASPC
```
Suite à cela, les dépendances peuvent être téléchargées ainsi :
```
cd 3ASPC

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

- Création du fichier de DB
```dotnet ef database update
```
