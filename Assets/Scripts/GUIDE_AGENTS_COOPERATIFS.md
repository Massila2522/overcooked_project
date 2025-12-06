# Guide : Configuration des Agents Coopératifs

Ce guide explique comment configurer deux agents coopératifs qui travaillent ensemble pour compléter 5 recettes.

## 🎯 Objectif

Les agents coopératifs (`CooperativeAgent`) travaillent en équipe pour :
- Se répartir automatiquement les recettes
- Éviter les conflits sur les stations partagées
- Optimiser le temps de préparation

## 📋 Étapes d'Installation

### 1. Supprimer l'ancien agent (optionnel)

Si tu as déjà un `UnifiedAgent` dans la scène :
- Sélectionne-le dans la Hiérarchie
- Supprime-le ou désactive-le

### 2. Créer le Premier Agent Coopératif

1. **Créer un GameObject vide** : `GameObject > Create Empty`
2. **Renommer** en "Agent1" ou "Chef1"
3. **Ajouter le script** : `Add Component > CooperativeAgent`
4. **Ajouter un SpriteRenderer** : `Add Component > SpriteRenderer`
5. **Configurer le sprite** : Assigne un sprite pour le personnage
6. **Configurer dans l'Inspector** :
   - `Agent Label` : "Chef 1" (ou autre nom)
   - `Agent Color` : Couleur distinctive (ex: blanc ou jaune)
   - `Move Speed` : 3-5 (vitesse de déplacement)

### 3. Créer le Deuxième Agent Coopératif

1. **Dupliquer Agent1** : Sélectionne Agent1, puis `Ctrl+D`
2. **Renommer** en "Agent2" ou "Chef2"
3. **Repositionner** : Déplace-le à un autre endroit sur la carte
4. **Configurer dans l'Inspector** :
   - `Agent Label` : "Chef 2"
   - `Agent Color` : Couleur différente (ex: bleu ou vert)

### 4. Vérifier la Configuration de la Scène

Assure-toi d'avoir au moins :
- **2 Stations de découpage** (CuttingStation) - pour éviter les files d'attente
- **2 Plaques de cuisson** (CookingStation)
- **2+ Places d'assiettes** (PlateStation)
- **1 Station de service** (ServeStation)
- **Réserves d'ingrédients** pour tous les types

### 5. (Optionnel) Ajouter une Deuxième Station de Découpage

Si tu n'as qu'une station de découpage :
1. Duplique la station existante (`Ctrl+D`)
2. Repositionne-la ailleurs dans la cuisine
3. Cela permettra aux deux agents de découper en parallèle

## 🎮 Fonctionnement

### Répartition des Recettes
- Chaque agent réserve automatiquement une recette disponible
- Pas de conflit : le système de réservation évite les doublons
- Les agents travaillent en parallèle sur des recettes différentes

### Gestion des Stations
- Recherche intelligente de la station la plus proche
- Attente automatique si une station est occupée
- Retry si échec de réservation

### Statistiques
- Chaque agent compte ses recettes complétées
- L'UI affiche les stats de chaque agent
- Le temps total est affiché à la fin

## 🔧 Configuration Avancée

### Dans l'Inspector du CooperativeAgent :

| Paramètre | Description |
|-----------|-------------|
| `Move Speed` | Vitesse de déplacement (défaut: 3) |
| `Agent Label` | Nom affiché au-dessus de l'agent |
| `Agent Color` | Couleur du sprite de l'agent |
| `Label Color` | Couleur du texte du label |

### Sprites Optionnels :

Tu peux assigner des sprites personnalisés :
- `Marmite Sprite Override` : Sprite de la marmite
- `Pan Sprite Override` : Sprite de la poêle
- `Plate Sprite Override` : Sprite de l'assiette
- `Soup Sprite Override` : Sprite du plat de soupe terminé
- `Burger Sprite Override` : Sprite du burger terminé

## 📊 Affichage UI

L'UIManager affiche maintenant :
- Le nombre d'agents coopératifs actifs
- L'état de chaque agent (libre, porte un ingrédient, etc.)
- Le nombre de recettes complétées par agent

## 🚀 Conseils d'Optimisation

1. **Plus de stations** = moins d'attente
2. **Positions initiales** : Place les agents à des endroits différents
3. **Vitesse** : Augmente `Move Speed` pour aller plus vite

## ❓ Dépannage

### Les agents ne bougent pas ?
- Vérifie que le `RecipeManager` est présent dans la scène
- Vérifie que les réserves d'ingrédients sont configurées

### Les agents se bloquent ?
- Ajoute plus de stations de découpage ou de cuisson
- Vérifie que toutes les stations ont le script approprié

### Erreurs de sprites ?
- Configure l'`IngredientSpriteManager` dans la scène
- Ou assigne les sprites directement dans l'Inspector des agents

