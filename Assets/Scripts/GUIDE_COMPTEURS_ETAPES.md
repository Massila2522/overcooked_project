# Guide étape par étape - Création des compteurs

## ✅ Étape 1 : Import TMP Essential Resources (FAIT)
Vous avez déjà fait : Window > TextMeshPro > Import TMP Essential Resources

## 📝 Étape 2 : Créer les TextMeshPro dans le Canvas

### Créer le compteur de temps :
1. Dans la **Hiérarchie** (panneau de gauche), trouvez et sélectionnez le **Canvas**
2. **Clic droit** sur Canvas → **UI → Text - TextMeshPro**
3. Unity va vous demander d'importer les ressources TMP si ce n'est pas déjà fait → Cliquez sur **Import TMP Essentials**
4. Un nouveau TextMeshPro apparaît sous Canvas
5. **Renommez-le** : Clic droit sur le TextMeshPro → **Rename** → Tapez `TimeCounter`

### Créer le compteur de recettes :
1. **Clic droit** sur Canvas → **UI → Text - TextMeshPro**
2. **Renommez-le** : `RecipesCounter`

## 🎯 Étape 3 : Positionner les compteurs en haut à gauche

### Pour TimeCounter :
1. Sélectionnez **TimeCounter** dans la hiérarchie
2. Dans l'**Inspecteur** (panneau de droite), trouvez le composant **Rect Transform**
3. En haut du Rect Transform, vous verrez une icône avec 4 petits carrés (l'ancrage)
4. **Cliquez sur cette icône** → Une grille apparaît
5. **Cliquez sur le carré en haut à gauche** (Top-Left)
6. Ajustez la position :
   - **Pos X** : `10`
   - **Pos Y** : `-10`
7. Dans la section **Text (TextMeshPro)** :
   - **Text** : `Temps: 0:00`
   - **Font Size** : `24` (ou plus grand si vous voulez)
   - **Color** : Blanc (ou la couleur de votre choix)

### Pour RecipesCounter :
1. Sélectionnez **RecipesCounter** dans la hiérarchie
2. Même procédure : **Rect Transform** → Icône d'ancrage → **Top-Left**
3. Position :
   - **Pos X** : `10`
   - **Pos Y** : `-40` (juste en dessous du TimeCounter)
4. Dans **Text (TextMeshPro)** :
   - **Text** : `Recettes: 0`
   - **Font Size** : `24`
   - **Color** : Blanc (ou la couleur de votre choix)

## 🔗 Étape 4 : Assigner les références dans UIManager

1. Dans la **Hiérarchie**, trouvez le GameObject qui contient le script **UIManager**
   - C'est probablement le **Canvas** lui-même ou un GameObject enfant du Canvas
   - Si vous ne le trouvez pas, cherchez dans la hiérarchie un objet avec le composant **UIManager**

2. Sélectionnez ce GameObject

3. Dans l'**Inspecteur**, trouvez le composant **UIManager**

4. Dans la section **UI References**, vous verrez maintenant 3 champs :
   - **Blackboard Text** (déjà assigné probablement)
   - **Time Counter Text** (vide)
   - **Recipes Counter Text** (vide)

5. **Glissez-déposez** depuis la hiérarchie :
   - Glissez **TimeCounter** dans le champ **Time Counter Text**
   - Glissez **RecipesCounter** dans le champ **Recipes Counter Text**

## ✅ Étape 5 : Tester

1. Lancez le jeu (bouton Play)
2. Vous devriez voir en haut à gauche :
   ```
   Temps: 0:00
   Recettes: 0
   ```
3. Le temps devrait s'incrémenter chaque seconde
4. Le compteur de recettes devrait s'incrémenter à chaque recette servie

## 🎨 Optionnel : Personnaliser l'apparence

Si vous voulez rendre les compteurs plus visibles :
- Augmentez la **Font Size** (ex: 28 ou 32)
- Changez la **Color** pour un contraste plus fort
- Ajoutez un **Outline** dans les paramètres du TextMeshPro pour une meilleure lisibilité

