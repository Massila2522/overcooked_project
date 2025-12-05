# Guide - Positionner les compteurs en bas à gauche

## 🎯 Objectif
Déplacer les compteurs en bas à gauche (zone verte) et mettre le texte en noir

## 📝 Étapes dans Unity

### Pour TimeCounter :

1. **Sélectionnez TimeCounter** dans la hiérarchie

2. **Rect Transform** → Cliquez sur l'icône d'ancrage (4 carrés)
   - Sélectionnez **Bottom-Left** (bas-gauche)

3. **Ajustez la position** :
   - **Pos X** : `10` (ou `20` si vous voulez un peu plus d'espace)
   - **Pos Y** : `10` (ou `20` si vous voulez un peu plus d'espace depuis le bas)

4. **Text (TextMeshPro)** → **Color** :
   - Cliquez sur le carré de couleur
   - Choisissez **Noir** (ou RGB : R=0, G=0, B=0)

### Pour RecipesCounter :

1. **Sélectionnez RecipesCounter** dans la hiérarchie

2. **Rect Transform** → Cliquez sur l'icône d'ancrage
   - Sélectionnez **Bottom-Left**

3. **Ajustez la position** :
   - **Pos X** : `10` (même valeur que TimeCounter)
   - **Pos Y** : `40` (juste au-dessus de TimeCounter, environ 30 pixels d'écart)

4. **Text (TextMeshPro)** → **Color** :
   - Cliquez sur le carré de couleur
   - Choisissez **Noir** (RGB : R=0, G=0, B=0)

## 🎨 Optionnel : Améliorer la lisibilité

Si le texte noir n'est pas assez visible sur le vert, vous pouvez :
- Ajouter un **Outline** : Dans TextMeshPro → **Extra Settings** → **Outline Width** : `0.2` ou `0.3`
- Ou utiliser un **gris foncé** au lieu de noir pur (ex: RGB : R=30, G=30, B=30)

## ✅ Résultat attendu

Les compteurs devraient maintenant être en bas à gauche, dans la zone verte, avec du texte noir :
```
Recettes: 4
Temps: 2:01
```

(Recettes en haut, Temps en bas, car RecipesCounter a un Y plus élevé)

