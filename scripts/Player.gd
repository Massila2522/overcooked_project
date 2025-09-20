extends CharacterBody2D

@export var speed: float = 200.0
@export var autonomous: bool = true

var held_item: Node2D = null
var current_recipe: Recipe = null
var step_index: int = 0
var ai_enabled: bool = false
var stations: Dictionary = {}
var current_target: Vector2 = Vector2.ZERO
var target_set: bool = false
var start_position: Vector2 = Vector2.ZERO
var has_delivered: bool = false

func start_ai() -> void:
	ai_enabled = true
	start_position = global_position
	has_delivered = false

func on_ingredient_accepted() -> void:
	pass

func on_ingredient_deposited(deposited_item: Node2D) -> void:
	if not current_recipe:
		return
	step_index = min(step_index + 1, current_recipe.get_ingredient_count())
	target_set = false

func is_recipe_complete() -> bool:
	return current_recipe != null and step_index >= current_recipe.get_ingredient_count()

func is_ingredient_required(ingredient_type: String) -> bool:
	if not current_recipe or step_index >= current_recipe.get_ingredient_count():
		return false
	var needed_ingredient = current_recipe.get_ingredient_at(step_index)
	return needed_ingredient != null and needed_ingredient.base_type == ingredient_type

func _physics_process(delta: float) -> void:
	if autonomous and ai_enabled:
		_autonomous_move(delta)
	else:
		var input_vector: Vector2 = Vector2(
			Input.get_action_strength("move_right") - Input.get_action_strength("move_left"),
			Input.get_action_strength("move_down") - Input.get_action_strength("move_up")
		)
		if input_vector.length() > 1.0:
			input_vector = input_vector.normalized()
		velocity = input_vector * speed
		move_and_slide()

func _input(event: InputEvent) -> void:
	if autonomous and ai_enabled:
		return
	if event.is_action_pressed("pickup") or event.is_action_pressed("place") or event.is_action_pressed("interact"):
		_use_station()
	elif event.is_action_pressed("pick_drop"):
		_pick_or_drop()

func _use_station() -> void:
	var areas: Array = ($InteractArea as Area2D).get_overlapping_areas()
	for area in areas:
		var owner: Node = area.get_parent()
		if not owner or not owner.has_method("try_use_station"):
			continue
		owner.try_use_station(self)
		return

func _autonomous_move(delta: float) -> void:
	var new_target = _get_target_position()
	if not target_set or current_target.distance_to(new_target) > 10.0:
		current_target = new_target
		target_set = true
	
	var distance = global_position.distance_to(current_target)
	
	if distance < 20.0:
		velocity = Vector2.ZERO
		_auto_try_use()
	else:
		velocity = (current_target - global_position).normalized() * speed
		move_and_slide()

func _get_target_position() -> Vector2:
	if not current_recipe or step_index >= current_recipe.get_ingredient_count():
		if has_delivered:
			return start_position
		elif _has_cooked_plate():
			return stations.get("rendu", global_position).global_position
		else:
			return stations.get("plate", global_position).global_position
	
	if held_item == null:
		return _find_source_station()
	else:
		return _find_transformation_station()

func _has_cooked_plate() -> bool:
	return held_item != null and held_item.has_method("is_plate") and held_item.call("is_plate") and held_item.get("is_cooked")

func _find_source_station() -> Vector2:
	var needed_ingredient = current_recipe.get_ingredient_at(step_index)
	if not needed_ingredient:
		return global_position
		
	for station in get_tree().get_nodes_in_group("source"):
		if station.get("item_type") == needed_ingredient.base_type and not station.get("is_empty"):
			return station.global_position
	return global_position

func _find_transformation_station() -> Vector2:
	var needed_ingredient = current_recipe.get_ingredient_at(step_index)
	if not needed_ingredient or not held_item.has_method("get_ingredient"):
		return stations.get("plate", global_position).global_position
		
	var held_ingredient = held_item.get_ingredient()
	if not held_ingredient or held_ingredient.base_type != needed_ingredient.base_type:
		return stations.get("plate", global_position).global_position
	
	match needed_ingredient.state:
		Ingredient.State.CUT:
			if held_ingredient.state == Ingredient.State.RAW:
				return stations.get("cutting", global_position).global_position
		Ingredient.State.COOKED:
			if held_ingredient.state in [Ingredient.State.RAW, Ingredient.State.CUT]:
				return stations.get("cooking", global_position).global_position
	
	return stations.get("plate", global_position).global_position

func _auto_try_use() -> void:
	var areas: Array = ($InteractArea as Area2D).get_overlapping_areas()
	for area in areas:
		var owner: Node = area.get_parent()
		if not owner or not owner.has_method("try_use_station"):
			continue
		if not _can_use_station(owner):
			continue
		owner.try_use_station(self)
		if owner == stations.get("rendu") and _has_cooked_plate():
			has_delivered = true
			target_set = false
		return

func _can_use_station(station: Node) -> bool:
	if station == stations.get("plate"):
		if station.get("is_cooking") or _has_cooked_plate():
			return false
	return true

func _pick_or_drop() -> void:
	var areas: Array = ($InteractArea as Area2D).get_overlapping_areas()
	if held_item == null:
		for area in areas:
			if not area.has_method("try_pick_item"):
				continue
			var item: Node2D = area.try_pick_item() as Node2D
			if not item:
				continue
			held_item = item
			held_item.position = position + Vector2(0, -16)
			held_item.reparent(self)
			return
	else:
		held_item.reparent(get_parent())
		held_item.position = position + Vector2(0, 16)
		held_item = null
