# GUIDE D'INSTALLATION - SYSTÈME DE CUISINE

## ✅ SCRIPTS CRÉÉS

### Enums et Classes de Base
- `IngredientType.cs` - Types d'ingrédients
- `IngredientState.cs` - États des ingrédients
- `RecipeType.cs` - Types de recettes
- `Ingredient.cs` - Classe ingrédient
- `Recipe.cs` - Classe recette
- `SpriteLoader.cs` - Helper pour charger les sprites

### Stations
- `Station.cs` - Classe de base pour toutes les stations
- `ReserveStation.cs` - Stations de réserve (6 GameObjects)
- `CuttingStation.cs` - Stations de découpage (2 GameObjects)
- `CutIngredientsStation.cs` - Places pour ingrédients découpés (2 GameObjects)
- `CookingStation.cs` - Plaques de cuisson (2 GameObjects)
- `PlateStation.cs` - Places d'assiettes (2 GameObjects)
- `ServeStation.cs` - Stations de rendu/service (2 GameObjects)

### Agents
- `Agent.cs` - Classe de base pour tous les agents
- `Agent1.cs` - Agent récupération ingrédients
- `Agent2.cs` - Agent découpage
- `Agent3.cs` - Agent découpage
- `Agent4.cs` - Agent cuisine
- `Agent5.cs` - Agent cuisine

### Managers
- `RecipeManager.cs` - Gestion des recettes
- `GameManager.cs` - Gestionnaire principal
- `UIManager.cs` - Gestion de l'UI
- `IngredientQueue.cs` - File d'attente des ingrédients

---

## 📋 ÉTAPES D'INSTALLATION

### 1. ATTACHER LES SCRIPTS AUX GAMEOBJECTS

#### Réserves (6 GameObjects)
Pour chaque GameObject "Reserve - [Nom]" :
- Ajouter le script `ReserveStation.cs`
- Configurer `ingredientType` :
  - `Reserve - Tomates` → `Tomato`
  - `Reserve - Pain hamburger` → `BurgerBun`
  - `Reserve - Champignons` → `Mushroom`
  - `Reserve - Oignons` → `Onion`
  - `Reserve - Salade` → `Lettuce`
  - `Reserve - Viande` → `Meat`

#### Stations de Découpage (2 GameObjects)
Pour `Station - Couteau Gauche` et `Station - Couteau Droite` :
- Ajouter le script `CuttingStation.cs`
- Ajuster `cuttingRadius` si nécessaire (défaut : 1.5)
- Ajuster `cuttingTime` si nécessaire (défaut : 2 secondes)

#### Places Ingrédients Découpés (2 GameObjects)
Pour `Station - Place ingrédient découpé Gauche` et `Droite` :
- Ajouter le script `CutIngredientsStation.cs`

#### Plaques de Cuisson (2 GameObjects)
Pour `Station - Plaque de cuisson Gauche` et `Droite` :
- Ajouter le script `CookingStation.cs`
- Ajuster `cookingTime` si nécessaire (défaut : 5 secondes)

#### Places d'Assiettes (2 GameObjects)
Pour `Station - Place assiette Gauche` et `Droite` :
- Ajouter le script `PlateStation.cs`

#### Stations de Rendu (2 GameObjects)
Pour `Station - Rendu Gauche` et `Droite` :
- Ajouter le script `ServeStation.cs`

#### Agents (5 GameObjects)
Pour `Agent 1`, `Agent 2`, `Agent 3`, `Agent 4`, `Agent 5` :
- Ajouter les scripts correspondants :
  - `Agent 1` → `Agent1.cs`
  - `Agent 2` → `Agent2.cs`
  - `Agent 3` → `Agent3.cs`
  - `Agent 4` → `Agent4.cs`
  - `Agent 5` → `Agent5.cs`
- Ajouter un `SpriteRenderer` avec le sprite `agent.png` si pas déjà fait
- Ajuster `moveSpeed` si nécessaire (défaut : 3)

#### Managers
Créer un GameObject vide nommé "Managers" :
- Ajouter `GameManager.cs`
- Ajouter `RecipeManager.cs`
- Configurer `minDelay` et `maxDelay` dans RecipeManager (défaut : 2-5 secondes)

#### UI (Optionnel)
Créer un Canvas avec des TextMeshPro :
- Ajouter `UIManager.cs` au Canvas ou à un GameObject enfant
- Assigner les références aux TextMeshPro dans l'inspecteur

---

## ⚙️ CONFIGURATION

### RecipeManager
- `minDelay` : Délai minimum entre recettes (2 secondes)
- `maxDelay` : Délai maximum entre recettes (5 secondes)

### CuttingStation
- `cuttingRadius` : Distance pour déclencher le découpage (1.5)
- `cuttingTime` : Temps de découpage en secondes (2)

### CookingStation
- `cookingTime` : Temps de cuisson en secondes (5)

### Agent
- `moveSpeed` : Vitesse de déplacement (3)

---

## 🎮 TEST

1. Lancer la scène
2. Les recettes doivent commencer à arriver toutes les 2-5 secondes
3. Agent 1 doit récupérer les ingrédients des réserves
4. Agents 2 et 3 doivent découper les ingrédients
5. Agents 4 et 5 doivent cuisiner et servir

---

## ⚠️ NOTES IMPORTANTES

- Les sprites doivent être dans `Assets/Sprites/`
- Les chemins de sprites sont automatiquement gérés par `SpriteLoader`
- Les agents travaillent en coopération avec gestion des conflits
- Les recettes sont traitées dans l'ordre (FIFO)
- Les ingrédients sont traités dans l'ordre des recettes

---

## 🐛 DÉPANNAGE

Si les sprites ne se chargent pas :
- Vérifier que les sprites sont dans `Assets/Sprites/`
- Vérifier les noms des fichiers (doivent correspondre aux chemins dans le code)

Si les agents ne bougent pas :
- Vérifier que les scripts sont attachés
- Vérifier que les stations sont trouvées (FindObjectsOfType)

Si les recettes ne s'affichent pas :
- Vérifier que RecipeManager est attaché et actif
- Vérifier les délais min/max

