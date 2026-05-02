# Liste de commandes `curl` permettant d'intéragir avec l'API

## Création d'utilisateur.ice
```
curl -X POST http://localhost:5285/register \
    -H "Content-Type: application/json" \
    -d '{"username":"Camille","email":"acme@felisse.fr","password":"password"}'
```

## Login avec les identifiants créés ci-dessus
```
curl -X POST http://localhost:5000/api/users/login \
  -H "Content-Type: application/json" \
  -d '{"email": "acme@felisse.fr", "password": "password"}'
```
