# Branche main

Cette branche contient la version finale de notre jeu Overcooked, avec 5 agents, développée avec Unity.  
La version de Unity utilisée est : 6000.0.63f1.

Ci-dessous une vidéo démonstrative de cette version du jeu :

[Regarde la vidéo sur YouTube](https://youtu.be/_t5vKw50WaQ)

## Résumé du jeu:
Ce jeu est une réinterprétation en 2D du gameplay d’Overcooked, développée avec Unity, où plusieurs agents coopèrent pour préparer des recettes. Les commandes arrivent aléatoirement (soupes ou hamburgers) et génèrent une file d’ingrédients à traiter dans l’ordre.

### Recettes disponibles

- Soupes : oignons / tomates / champignons  
  (3 ingrédients identiques coupés, puis cuits dans une marmite)
- Hamburger : pain, salade coupée, tomate coupée, viande coupée puis cuite

### Organisation du travail

- Une réserve fournit tous les ingrédients.
- Deux postes de découpe reçoivent les ingrédients bruts.
- Deux zones accueillent les ingrédients une fois coupés.
- Deux plaques de cuisson permettent d’utiliser soit une marmite (soupes), soit une poêle (viande du hamburger).
- Deux emplacements permettent de préparer les assiettes finales avant service.

### Agents

- Agent 1 : récupère les ingrédients dans l’ordre des commandes et les place aux postes de découpe.
- Agents 2 & 3 : coupent les ingrédients et les placent dans la zone dédiée.
- Agents 4 & 5 : cuisinent et assemblent les recettes du début à la fin, puis servent les plats.

Toutes les actions respectent strictement l’ordre des recettes et des ingrédients.
Un compteur de temps ainsi qu’un compteur du nombre de recettes exécutées ont été ajoutés pour suivre la performance de la cuisine.

## Résumés de toutes les branches principales pour ce projet:

- **main** : cette branche contient la dernière version du jeu, avec multi agents (5 agents) et codé avec Unity.
- **automaticalReceipe** : contient le projet 1, le jeu Overcooked avec un seul agent développé avec godot.
- **un_agent_unity** : cette branche contient la version du jeu overcooked avec un seul agent codé avec Unity.
- **double-agents** : cette branche contient la version du jeu overcooked avec deux agents codé avec Unity.
- **multi-agent-unity** : cette branche contient la version du jeu avec multi agents (5 agents) codé avec Unity.

## Note:
Dans le premier rendu nous avons oublié de faire un merge de la branche automaticalReceipe main ce qui fais que la dernière version était dans la branche automaticalReceipe et non pas dans main, mais actuellement main est à jour.
