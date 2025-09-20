extends Node2D

var plate: Node2D = null
var is_cooking: bool = false
var cooking_duration: float = 3.0

@onready var progress: ProgressBar = $CanvasLayer/ProgressBar
@onready var timer: Timer = $Timer

func _ready() -> void:
	_update_visual()
	if plate == null:
		_spawn_plate()
	if timer:
		timer.timeout.connect(_on_Timer_timeout)
	# S'assurer que la barre de progression est invisible au début
	progress.visible = false

func _spawn_plate() -> void:
	var plate_scene: PackedScene = load("res://scenes/Plate.tscn")
	plate = plate_scene.instantiate() as Node2D
	add_child(plate)
	plate.position = Vector2.ZERO
	_update_visual()

func _process(delta: float) -> void:
	if is_cooking and timer.time_left > 0.0:
		progress.value = 1.0 - (timer.time_left / cooking_duration)

func try_use_station(player: Node) -> void:
	if is_cooking:
		return
	
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
	return plate != null and not plate.is_empty() and not is_cooking and plate.is_cooked

func _give_plate_to_player(player: Node) -> void:
	plate.reparent(player)
	plate.position = player.position + Vector2(0, -16)
	player.held_item = plate
	plate = null
	_update_visual()

func _is_recipe_complete_on_plate(player: Node) -> bool:
	if not player.current_recipe:
		return false
	# Vérifier que tous les ingrédients de la recette sont sur l'assiette
	return plate != null and not plate.is_empty() and plate.held_foods.size() >= player.current_recipe.get_ingredient_count()

func _start_cooking() -> void:
	is_cooking = true
	progress.visible = true
	progress.value = 0.0
	timer.start(cooking_duration)

func _on_Timer_timeout() -> void:
	is_cooking = false
	progress.visible = false
	if plate and plate.has_method("cook_plate"):
		plate.call("cook_plate")

func _is_ingredient_correct(player: Node, item: Node2D) -> bool:
	if not item.has_method("get_ingredient"):
		return false
	
	var current_recipe = player.current_recipe
	var step_index = player.step_index
	
	if not current_recipe or step_index >= current_recipe.get_ingredient_count():
		return false
	
	var needed_ingredient = current_recipe.get_ingredient_at(step_index)
	var held_ingredient = item.get_ingredient()
	
	if not needed_ingredient or not held_ingredient:
		return false
	
	return held_ingredient.is_same_as(needed_ingredient)

func _update_visual() -> void:
	$Sprite.modulate = Color(0.6,0.6,0.6,1) if plate == null else Color(1.0,0.9,0.6,1)
