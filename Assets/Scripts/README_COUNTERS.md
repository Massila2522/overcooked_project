# Guide d'ajout des compteurs UI

## 📊 Compteurs à ajouter

Deux compteurs doivent être ajoutés en haut à gauche de l'écran :
1. **Compteur de temps** : Affiche le temps écoulé (format minute:seconde, ex: 1:30)
2. **Compteur de recettes** : Affiche le nombre de recettes servies

## 🎨 Étapes dans Unity

### 1. Créer les TextMeshPro pour les compteurs

1. Dans la hiérarchie, sélectionnez le **Canvas**
2. Clic droit → **UI → Text - TextMeshPro**
3. Renommez-le `TimeCounter`
4. Répétez pour créer un deuxième TextMeshPro nommé `RecipesCounter`

### 2. Positionner les compteurs (en haut à gauche)

Pour chaque compteur :

1. Sélectionnez le TextMeshPro
2. Dans l'inspecteur, trouvez le composant **Rect Transform**
3. Configurez l'ancrage :
   - Cliquez sur l'icône d'ancrage en haut à gauche
   - Sélectionnez **Top-Left** (haut-gauche)
4. Ajustez la position :
   - **TimeCounter** : Position X = 10, Position Y = -10
   - **RecipesCounter** : Position X = 10, Position Y = -40

### 3. Configurer le texte

Pour **TimeCounter** :
- **Text** : `Temps: 0:00`
- **Font Size** : 24 (ou selon vos préférences)
- **Color** : Blanc ou couleur de votre choix
- **Alignment** : Left

Pour **RecipesCounter** :
- **Text** : `Recettes: 0`
- **Font Size** : 24 (ou selon vos préférences)
- **Color** : Blanc ou couleur de votre choix
- **Alignment** : Left

### 4. Assigner les références dans UIManager

1. Sélectionnez le GameObject qui contient le script **UIManager** (probablement le Canvas ou un enfant)
2. Dans l'inspecteur, trouvez le composant **UIManager**
3. Dans la section **UI References** :
   - Glissez `TimeCounter` dans le champ **Time Counter Text**
   - Glissez `RecipesCounter` dans le champ **Recipes Counter Text**

## ✅ Résultat attendu

Une fois configuré, vous devriez voir en haut à gauche :
```
Temps: 0:00
Recettes: 0
```

Les compteurs se mettront à jour automatiquement pendant le jeu :
- Le temps s'incrémente chaque seconde
- Le compteur de recettes s'incrémente à chaque recette servie

## 🔧 Fonctionnalités

- **Temps** : Format minute:seconde (ex: 1:30, 2:45, 10:15)
- **Recettes** : Compte le nombre total de recettes servies depuis le début de la partie
- **Pas d'impact sur le jeu** : Ces compteurs sont purement informatifs

