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
	var player_item = player.held_item
	
	if player_item != null and plate != null:
		_handle_ingredient_placement(player, player_item)
	elif player_item == null and _can_give_plate():
		_give_plate_to_player(player)

func _handle_ingredient_placement(player: Node, item: Node2D) -> void:
	if _is_ingredient_correct(player, item) and plate.try_place_food(item):
		player.held_item = null
		player.on_ingredient_deposited(item)
		if _is_recipe_complete_on_plate(player):
			_start_cooking()

func _can_give_plate() -> bool:
	return plate != null and plate.is_cooked

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

func _is_ingredient_correct(player: Node, item: Node2D) -> bool:
	var current_recipe = player.current_recipe
	var step_index = player.step_index
	
	if not current_recipe or step_index >= current_recipe.get_ingredient_count():
		return false
	
	var needed_ingredient = current_recipe.get_ingredient_at(step_index)
	var held_ingredient = item.get_ingredient()
	
	if not needed_ingredient or not held_ingredient:
		return false
	
	return held_ingredient.is_same_as(needed_ingredient)
