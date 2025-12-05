# Agent Converter - Guide d'utilisation

## 🎯 Objectif
Cet outil permet de convertir automatiquement tous les anciens agents (IngredientProviderAgent, CuttingAgent, DressingAgent) en un seul UnifiedAgent.

## 📋 Comment utiliser

### Méthode 1 : Via le menu Unity
1. Ouvrez Unity et votre scène
2. Allez dans le menu : **Tools → Convert to Unified Agent**
3. Une fenêtre s'ouvre avec les informations sur les agents présents
4. Cliquez sur **"Convertir vers UnifiedAgent"**
5. ✅ C'est fait ! Sauvegardez votre scène (Ctrl+S)

### Méthode 2 : Si aucun agent n'existe
1. Ouvrez la fenêtre **Tools → Convert to Unified Agent**
2. Cliquez sur **"Créer un UnifiedAgent"**
3. Positionnez-le dans la scène

## ⚠️ Important
- **Sauvegardez votre scène avant de convertir** (Ctrl+S)
- La conversion est réversible via Ctrl+Z (Undo)
- Le script préserve la position et la vitesse du premier agent trouvé
- Le SpriteRenderer est copié si présent

## 🔍 Ce que fait la conversion
1. ✅ Crée un nouveau GameObject "UnifiedAgent"
2. ✅ Ajoute le composant UnifiedAgent
3. ✅ Préserve la position du premier agent
4. ✅ Préserve la vitesse de déplacement (moveSpeed)
5. ✅ Copie le SpriteRenderer si présent
6. ✅ Supprime tous les anciens agents
7. ✅ Sélectionne automatiquement le nouvel agent

## 📝 Après la conversion
Vérifiez dans l'inspecteur que :
- Le `moveSpeed` est correct
- Le `agentLabel` est défini (optionnel)
- Le SpriteRenderer est présent si nécessaire

