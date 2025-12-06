# 🎮 Guide d'Installation - Interface Améliorée

## Vue d'ensemble

Cette mise à jour ajoute une interface utilisateur moderne style "Overcooked" avec :
- ⏱️ Timer stylisé en haut à droite
- 🍽️ Compteur de recettes animé
- 📋 File d'attente des commandes visuelles
- 👥 Panneau de statut pour chaque agent
- 🏆 Écran de victoire animé

## 🚀 Installation Rapide

### Méthode 1 : Automatique (Recommandée)

1. **Ouvrez Unity** et chargez votre scène
2. Allez dans **Window > Kitchen Game > Scene Creator**
3. Configurez :
   - Nombre d'agents : **2**
   - Recettes à servir : **5**
   - Choisissez les couleurs de vos agents
4. Cliquez sur **"Configuration Complète"**
5. Lancez le jeu ! ▶️

### Méthode 2 : Manuelle

1. Créez un GameObject vide nommé "GameManagers"
2. Ajoutez les composants suivants :
   - `ImprovedUIManager`
   - `TaskDisplayUI`
   - `SceneSetup` (optionnel, pour configuration auto)

3. Vérifiez que vous avez :
   - `GameManager` avec `maxRecipes = 5`
   - `RecipeManager`
   - 2 GameObjects avec `CooperativeAgent`

## 📦 Scripts Ajoutés

| Script | Description |
|--------|-------------|
| `ImprovedUIManager.cs` | Interface principale avec timer, compteur et file de commandes |
| `TaskDisplayUI.cs` | Affiche les étapes en cours pour chaque agent |
| `ImprovedGameManager.cs` | (Optionnel) GameManager avec compte à rebours et événements |
| `SceneSetup.cs` | Configuration automatique de la scène |
| `EnhancedCooperativeAgent.cs` | Agent avec intégration UI des tâches |
| `KitchenSceneCreator.cs` | Outil d'édition (Window > Kitchen Game) |

## 🎨 Personnalisation

### Couleurs de l'interface

Dans `ImprovedUIManager.cs`, modifiez :

```csharp
[SerializeField] private Color panelColor = new Color(0.15f, 0.15f, 0.2f, 0.9f);
[SerializeField] private Color accentColor = new Color(1f, 0.6f, 0.2f, 1f); // Orange
[SerializeField] private Color successColor = new Color(0.3f, 0.9f, 0.4f, 1f);
```

### Position des panneaux

Les panneaux sont positionnés automatiquement :
- **Timer** : En haut à droite
- **Commandes** : En haut au centre
- **Agents** : En bas à gauche
- **Tâches** : En bas à droite (empilées par agent)

## 🔧 Configuration des 2 Agents

Dans la scène, configurez vos 2 agents :

### Agent 1 (Bleu)
- Position de départ : (-3, 0)
- Couleur : Bleu clair `(0.3, 0.7, 1.0)`

### Agent 2 (Orange)
- Position de départ : (3, 0)
- Couleur : Orange `(1.0, 0.5, 0.3)`

## 📊 Affichage des Tâches

Chaque agent affiche ses étapes en temps réel :

**Pour une Soupe :**
1. 🍽️ Assiette
2. 🍲 Marmite
3. 🧅 Ingrédient 1
4. 🧅 Ingrédient 2
5. 🧅 Ingrédient 3
6. 🔥 Cuisson
7. 🛎️ Servir

**Pour un Burger :**
1. 🍽️ Assiette
2. 🍞 Pain
3. 🥩 Viande
4. 🔥 Cuisson
5. 🥬 Salade
6. 🍅 Tomate
7. 🛎️ Servir

## ✅ Vérification

Avant de lancer, vérifiez :

- [ ] Canvas avec rendu Screen Space - Overlay
- [ ] GameManager avec `maxRecipes` configuré
- [ ] RecipeManager présent
- [ ] 2 agents avec `CooperativeAgent`
- [ ] Stations de travail (Réserves, Découpage, Cuisson, etc.)

## 🐛 Dépannage

### L'interface n'apparaît pas
- Vérifiez que `ImprovedUIManager` est dans la scène
- Le Canvas doit être en mode "Screen Space - Overlay"

### Les agents ne bougent pas
- Vérifiez que `RecipeManager` génère des recettes
- Assurez-vous que toutes les stations sont présentes

### Le jeu ne se termine pas
- Vérifiez `GameManager.maxRecipes`
- Les recettes doivent être servies sur une `ServeStation`

## 🎯 Conseils d'Optimisation

1. **Coordination** : Les agents évitent automatiquement les conflits
2. **Parallélisme** : Les 2 agents peuvent travailler sur des recettes différentes
3. **Efficacité** : Les agents choisissent les stations les plus proches

---

*Pour toute question, consultez les scripts pour plus de détails sur l'implémentation.*

