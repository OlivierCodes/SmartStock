# SmartStock API

> **Système Intelligent de Gestion des Stocks et des Ventes**  
> Plateforme destinée aux commerçants du Grand Marché de Lomé.

## Architecture du projet

```
SmartStock/
├── Controllers/                # Endpoints API REST
│   ├── AuthController.cs       # Authentification JWT
│   ├── UsersController.cs      # Gestion utilisateurs (Responsable)
│   ├── CategoriesController.cs # Catégories de produits
│   ├── ProductsController.cs   # Catalogue & stock
│   ├── StockMovementsController.cs # Entrées/sorties stock
│   ├── SalesController.cs      # Ventes
│   └── DashboardController.cs  # Tableau de bord
├── Data/
│   ├── SmartStockDbContext.cs  # Contexte EF Core
│   └── Migrations/             # Migrations EF Core
├── Models/
│   ├── Entities/               # Entités de domaine
│   │   ├── User.cs
│   │   ├── Category.cs
│   │   ├── Product.cs
│   │   ├── StockMovement.cs
│   │   ├── Sale.cs
│   │   └── SaleItem.cs
│   └── DTOs/                   # Objets de transfert de données
├── Services/                   # Logique métier
│   ├── Interfaces/
│   ├── AuthService.cs
│   ├── ProductService.cs
│   └── StockSaleService.cs
├── Middlewares/
│   └── ExceptionHandlingMiddleware.cs
├── appsettings.json
└── Program.cs
```

## Démarrage rapide

### Prérequis
- .NET 9.0 SDK
- SQL Server ou PostgreSQL

### Configuration

1. Modifier `appsettings.json` :
   - `ConnectionStrings:DefaultConnection` – Chaîne SQL Server
   - `ConnectionStrings:PostgreSQLConnection` – Chaîne PostgreSQL
   - `DatabaseProvider` – `"SqlServer"` ou `"PostgreSQL"`
   - `Jwt:Key` – Clé secrète (≥ 32 caractères)

2. Lancer l'application :
```bash
dotnet run
```

La migration est appliquée automatiquement au démarrage en développement.

3. Accéder à Swagger : `https://localhost:{port}/swagger`

### Commandes EF Core utiles

```bash
# Créer une nouvelle migration
dotnet ef migrations add <NomMigration> --output-dir Data/Migrations

# Appliquer les migrations manuellement
dotnet ef database update

# Revenir à une migration précédente
dotnet ef database update <NomMigration>
```

## Compte administrateur par défaut

| Champ    | Valeur                  |
|----------|-------------------------|
| Email    | admin@smartstock.com    |
| Mot passe| Admin@1234              |
| Rôle     | Responsable             |

> ⚠️ Changez ce mot de passe immédiatement en production !

## Rôles et permissions

| Fonctionnalité                | Responsable | Magasinier | Vendeur |
|-------------------------------|:-----------:|:----------:|:-------:|
| Gestion des utilisateurs      | ✅           | ❌          | ❌       |
| Catalogue produits (lecture)  | ✅           | ✅          | ✅       |
| Créer/Modifier produits       | ✅           | ✅          | ❌       |
| Supprimer produits            | ✅           | ❌          | ❌       |
| Entrées/Sorties de stock      | ✅           | ✅          | ❌       |
| Rapport journalier stock      | ✅           | ❌          | ❌       |
| Enregistrer une vente         | ✅           | ❌          | ✅       |
| Voir toutes les ventes        | ✅           | ✅          | ❌       |
| Voir ses propres ventes       | ✅           | ✅          | ✅       |
| Annuler une vente             | ✅           | ❌          | ❌       |
| Rapport de ventes             | ✅           | ❌          | ❌       |
| Tableau de bord               | ✅           | ❌          | ❌       |

## Endpoints API

### Auth – `/api/auth`
| Méthode | Route                   | Description                    | Accès         |
|---------|-------------------------|--------------------------------|---------------|
| POST    | `/login`                | Connexion (retourne JWT)       | Public        |
| POST    | `/register`             | Créer un utilisateur           | Responsable   |
| PUT     | `/change-password`      | Changer son mot de passe       | Authentifié   |
| GET     | `/me`                   | Profil utilisateur connecté    | Authentifié   |

### Produits – `/api/products`
| Méthode | Route            | Description                         | Accès                    |
|---------|------------------|-------------------------------------|--------------------------|
| GET     | `/`              | Liste paginée (filtres disponibles) | Tous                     |
| GET     | `/{id}`          | Détail d'un produit                 | Tous                     |
| GET     | `/low-stock`     | Produits en stock faible            | Tous                     |
| POST    | `/`              | Créer un produit                    | Responsable, Magasinier  |
| PUT     | `/{id}`          | Modifier un produit                 | Responsable, Magasinier  |
| DELETE  | `/{id}`          | Désactiver un produit               | Responsable              |

### Mouvements de Stock – `/api/stockmovements`
| Méthode | Route              | Description                   | Accès                   |
|---------|--------------------|-------------------------------|-------------------------|
| GET     | `/`                | Historique paginé             | Responsable, Magasinier |
| GET     | `/{id}`            | Détail d'un mouvement         | Responsable, Magasinier |
| POST    | `/`                | Enregistrer entrée/sortie     | Responsable, Magasinier |
| GET     | `/daily-report`    | Rapport journalier            | Responsable             |

### Ventes – `/api/sales`
| Méthode | Route            | Description                   | Accès                   |
|---------|------------------|-------------------------------|-------------------------|
| GET     | `/`              | Toutes les ventes             | Responsable, Magasinier |
| GET     | `/my-sales`      | Ventes du vendeur connecté    | Vendeur                 |
| GET     | `/{id}`          | Détail d'une vente            | Tous                    |
| POST    | `/`              | Enregistrer une vente         | Vendeur, Responsable    |
| POST    | `/{id}/cancel`   | Annuler une vente             | Responsable             |
| GET     | `/report`        | Rapport période               | Responsable             |

### Tableau de Bord – `/api/dashboard`
| Méthode | Route       | Description                      | Accès       |
|---------|-------------|----------------------------------|-------------|
| GET     | `/summary`  | KPIs + alertes + top produits    | Responsable |

## Variables d'environnement (Production)

```bash
ConnectionStrings__DefaultConnection="Server=...;Database=SmartStockDb;..."
DatabaseProvider="SqlServer"
Jwt__Key="VotreCleSuperSecreteDeProduction"
Jwt__Issuer="SmartStock"
Jwt__Audience="SmartStockApp"
```
