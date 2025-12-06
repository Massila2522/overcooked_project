# 📸 GUIDE DE CONFIGURATION DES SPRITES

## 🎯 SYSTÈME DE GESTION DES SPRITES

Le projet utilise maintenant un système centralisé pour gérer les sprites via l'inspecteur Unity.

---

## 📋 ÉTAPE 1 : CRÉER LE SPRITE MANAGER

1. **Créer un GameObject vide** dans la scène (par exemple "SpriteManager")
2. **Attacher le script** `IngredientSpriteManager.cs` à ce GameObject
3. Ce GameObject sera automatiquement configuré comme Singleton (Instance)

---

## 📋 ÉTAPE 2 : CRÉER LES SCRIPTABLE OBJECTS POUR LES INGRÉDIENTS

Pour chaque type d'ingrédient, créez un ScriptableObject :

1. Dans Unity : **Right-click** dans le Project → **Create** → **Cuisine** → **Ingredient Sprite Data**
2. Nommez-le (ex: `OnionSpriteData`, `TomatoSpriteData`, etc.)
3. Pour chaque ScriptableObject, assignez les sprites :
   - **Raw Sprite** : Sprite de l'ingrédient brut
   - **Cut Sprite** : Sprite de l'ingrédient découpé
   - **Chopped Sprite** : (Uniquement pour la viande) Sprite de la viande hachée
   - **Cooked Sprite** : Sprite de l'ingrédient cuit

### Liste des ScriptableObjects à créer :
- `OnionSpriteData` (onion_raw, onion_cut, onion_cooked)
- `TomatoSpriteData` (tomato_raw, tomato_cut, tomato_cooked)
- `MushroomSpriteData` (mushroom_raw, mushroom_cut, mushroom_cooked)
- `LettuceSpriteData` (lettuce_raw, lettuce_cut)
- `MeatSpriteData` (meat_raw, meat_chopped, meat_cooked)
- `BurgerBunSpriteData` (pain - seulement raw)

---

## 📋 ÉTAPE 3 : CONFIGURER IngredientSpriteManager

Dans l'inspecteur du GameObject "SpriteManager" :

### Section "Sprites d'ingrédients"
- **Onion Sprites** : Glissez `OnionSpriteData`
- **Tomato Sprites** : Glissez `TomatoSpriteData`
- **Mushroom Sprites** : Glissez `MushroomSpriteData`
- **Lettuce Sprites** : Glissez `LettuceSpriteData`
- **Meat Sprites** : Glissez `MeatSpriteData`
- **Burger Bun Sprites** : Glissez `BurgerBunSpriteData`

### Section "Sprites d'ustensiles"
- **Marmite Sprite** : Glissez le sprite `marmite.png`
- **Pan Sprite** : Glissez le sprite `pan.png`
- **Plate Sprite** : Glissez le sprite `assiette.png`

### Section "Sprites de plats finis"
- **Soup Sprite** : Glissez le sprite `soupe.png`
- **Burger Sprite** : Glissez le sprite `burger.png`

---

## 📋 ÉTAPE 4 : CONFIGURER LES RÉSERVES (OPTIONNEL)

Pour chaque `ReserveStation` dans la scène :
- Vous pouvez optionnellement assigner un **Raw Ingredient Sprite** directement dans l'inspecteur
- Si non assigné, le système utilisera automatiquement `IngredientSpriteManager`

---

## 📋 ÉTAPE 5 : CONFIGURER LES AGENTS DE CUISINE (OPTIONNEL)

Pour les `DressingAgent` :
- Vous pouvez optionnellement assigner des sprites dans la section "Sprites (optionnel)"
- Si non assignés, le système utilisera automatiquement `IngredientSpriteManager`

---

## ✅ AVANTAGES DE CE SYSTÈME

1. **Centralisé** : Tous les sprites sont gérés au même endroit
2. **Flexible** : Possibilité d'override par GameObject si nécessaire
3. **Facile à maintenir** : Pas besoin de modifier le code pour changer les sprites
4. **Pas de chemins hardcodés** : Plus de problèmes de chemins de fichiers

---

## 🔍 TROUBLESHOOTING

### Les sprites ne s'affichent pas
1. Vérifiez que `IngredientSpriteManager` est présent dans la scène
2. Vérifiez que tous les ScriptableObjects sont assignés
3. Vérifiez que les sprites sont bien importés dans Unity (pas juste des fichiers PNG)

### Warning "Sprite non trouvé"
- Le système essaie d'abord l'override local, puis `IngredientSpriteManager`
- Assurez-vous que les ScriptableObjects sont bien assignés dans `IngredientSpriteManager`

---

## 📝 NOTES

- Les sprites sont chargés automatiquement au runtime
- Les changements d'état (raw → cut → cooked) mettent à jour automatiquement les sprites
- Le système fonctionne même si certains sprites ne sont pas assignés (avec warnings)

