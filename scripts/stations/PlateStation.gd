extends Node2D

var plate: Node2D = null
@onready var progress: ProgressBar = $CanvasLayer/ProgressBar

func _ready() -> void:
	if plate == null:
		_spawn_plate()
	# S'assurer que la barre de progression est invisible au début
	progress.visible = false

func _spawn_plate() -> void:
	var plate_scene: PackedScene = load("res://scenes/items/Plate.tscn")
	plate = plate_scene.instantiate() as Node2D
	add_child(plate)
	plate.position = Vector2.ZERO

func _process(_delta: float) -> void:
	pass

func try_use_station(player: Node) -> void:
	if player == null:
		return
	var player_item = player.held_item
	
	if player_item != null and plate != null:
		_handle_ingredient_placement(player, player_item)
	elif player_item == null and _can_give_plate(player):
		_give_plate_to_player(player)

func _handle_ingredient_placement(player: Node, item: Node2D) -> void:
	if player.agent_role != player.AgentRole.ASSEMBLER:
		return
	if _is_ingredient_correct(player, item) and plate.try_place_food(item):
		player.held_item = null
		player.on_ingredient_deposited(item)
		if _is_recipe_complete_on_plate(player):
			_start_cooking()

func _can_give_plate(player: Node) -> bool:
	if plate == null or not plate.is_cooked:
		return false
	if player == null:
		return false
	return player.agent_role == player.AgentRole.RUNNER

func _give_plate_to_player(player: Node) -> void:
	plate.reparent(player)
	plate.position = player.position + Vector2(0, -16)
	player.held_item = plate
	plate = null

func _is_recipe_complete_on_plate(player: Node) -> bool:
	if not player.current_recipe:
		return false
	# Vérifier que tous les ingrédients de la recette sont sur l'assiette
	return plate != null and not plate.is_empty() and plate.held_foods.size() >= player.current_recipe.get_ingredient_count()

func _start_cooking() -> void:
	# Cuisson instantanée
	if plate:
		plate.cook_plate()

func has_ready_plate() -> bool:
	return plate != null and plate.is_cooked

func _is_ingredient_correct(player: Node, item: Node2D) -> bool:
	# Pour l'architecture multi-agent, on vérifie si l'ingrédient fait partie de la recette
	# sans se soucier de l'ordre (l'assembleur peut placer les ingrédients dans n'importe quel ordre)
	var current_recipe = player.current_recipe
	
	if not current_recipe:
		return false
	
	var held_ingredient = item.get_ingredient()
	if not held_ingredient:
		return false
	
	# Vérifier si cet ingrédient fait partie de la recette
	for needed_ingredient in current_recipe.ingredients:
		if held_ingredient.is_same_as(needed_ingredient):
			# Vérifier qu'on n'a pas déjà cet ingrédient sur l'assiette
			if plate and not plate.is_empty():
				var already_has = false
				for food in plate.held_foods:
					var food_ing = food.get_ingredient() if food.has_method("get_ingredient") else null
					if food_ing and food_ing.is_same_as(needed_ingredient):
						already_has = true
						break
				if not already_has:
					return true
			else:
				return true
	
	return false
