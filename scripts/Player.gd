extends CharacterBody2D

@export var speed: float = 200.0
@export var autonomous: bool = true

var held_item: Node2D = null
var _auto_dir: Vector2 = Vector2(1, 0)
var _interact_cooldown: float = 0.0

# Recipe targeting
var current_recipe: Array[String] = ["dough", "tomato", "cheese"]
var step_index: int = 0
var target_group: String = "source" # or "assembly"

# Cached targets
var ingredient_to_source: Dictionary = {}
var assembly_point: Vector2 = Vector2.ZERO

# Start paused until a recipe is chosen
var ai_enabled: bool = false

func start_ai() -> void:
	ai_enabled = true
	_cache_targets()

func _ready() -> void:
	if autonomous:
		_auto_dir = Vector2(1, 0).rotated(randf() * TAU).normalized()
	# Do not cache targets until recipe is chosen; Main will call start_ai()

func _cache_targets() -> void:
	# Cache one source per ingredient: choose the closest to the player start
	ingredient_to_source.clear()
	for ing in current_recipe:
		var best_node: Node2D = null
		var best: float = INF
		for n in get_tree().get_nodes_in_group("source"):
			if n.has_method("get") and n.get("item_type") == ing:
				var d: float = (n as Node2D).global_position.distance_to(global_position)
				if d < best:
					best = d
					best_node = n as Node2D
		if best_node:
			ingredient_to_source[ing] = best_node.global_position
	# Assembly point (first assembly found)
	for n in get_tree().get_nodes_in_group("assembly"):
		assembly_point = (n as Node2D).global_position
		break

func on_ingredient_accepted() -> void:
	# Called by AssemblyStation when an ingredient is accepted
	step_index = min(step_index + 1, current_recipe.size())
	# Force update target mode to go back to source
	target_group = "source"
	# Debug: show what we're looking for next (only if not finished)
	if step_index < current_recipe.size():
		var main = get_tree().get_first_node_in_group("root")
		if main and main.has_method("show_message"):
			main.show_message("Prochain: %s" % current_recipe[step_index])

func _physics_process(delta: float) -> void:
	if autonomous and ai_enabled:
		_autonomous_move(delta)
	else:
		var input_vector: Vector2 = Vector2(
			(Input.get_action_strength("move_right") + Input.get_action_strength("ui_right")) - (Input.get_action_strength("move_left") + Input.get_action_strength("ui_left")),
			(Input.get_action_strength("move_down") + Input.get_action_strength("ui_down")) - (Input.get_action_strength("move_up") + Input.get_action_strength("ui_up"))
		)
		if input_vector.length() > 1.0:
			input_vector = input_vector.normalized()
		velocity = input_vector * speed
		move_and_slide()

	if _interact_cooldown > 0.0:
		_interact_cooldown -= delta
	else:
		if autonomous and ai_enabled:
			_update_target_mode()
			_auto_try_use()

func _update_target_mode() -> void:
	if held_item == null:
		target_group = "source"
	else:
		target_group = "assembly"

func _autonomous_move(delta: float) -> void:
	var waypoint: Vector2 = _current_target_point()
	var to_target: Vector2 = waypoint - global_position
	var dist: float = to_target.length()
	var desired: Vector2 = to_target.normalized() if dist > 0.001 else Vector2.ZERO
	# arrive behavior near target
	var target_speed: float = speed if dist > 60.0 else lerp(20.0, speed, dist / 60.0)
	velocity = desired * target_speed
	move_and_slide()

func _current_target_point() -> Vector2:
	if target_group == "source":
		var needed := current_recipe[min(step_index, current_recipe.size()-1)]
		if ingredient_to_source.has(needed):
			return ingredient_to_source[needed]
		# fallback: nearest matching source
		var best_point: Vector2 = global_position
		var best: float = INF
		for n in get_tree().get_nodes_in_group("source"):
			if n.has_method("get") and n.get("item_type") == needed and n.get("is_empty") == false:
				var p: Vector2 = (n as Node2D).global_position
				var d: float = p.distance_to(global_position)
				if d < best:
					best = d
					best_point = p
		return best_point
	else:
		return assembly_point

func _auto_try_use() -> void:
	var areas: Array = ($InteractArea as Area2D).get_overlapping_areas()
	for a in areas:
		var owner: Node = (a as Node).get_parent()
		if owner and owner.has_method("try_use_station"):
			owner.try_use_station(self)
			_interact_cooldown = 0.08
			# If at assembly and still holding a wrong item, discard it to continue
			if target_group == "assembly" and held_item != null:
				var needed := current_recipe[min(step_index, current_recipe.size()-1)]
				var t: String = held_item.get("item_type") if held_item.has_method("get") else ""
				if t != needed:
					held_item.queue_free()
					held_item = null
					var main = get_tree().get_first_node_in_group("root")
					if main and main.has_method("show_message"):
						main.show_message("Ingrédient %s rejeté" % t)
			return

func _input(event: InputEvent) -> void:
	if autonomous and ai_enabled:
		return
	if event.is_action_pressed("pickup"):
		_use_station_by_parent()
	elif event.is_action_pressed("place"):
		_use_station_by_parent()
	elif event.is_action_pressed("interact"):
		_use_station_by_parent()
	elif event.is_action_pressed("pick_drop"):
		_pick_or_drop()

func _use_station_by_parent() -> void:
	var areas: Array = ($InteractArea as Area2D).get_overlapping_areas()
	for a in areas:
		var owner: Node = (a as Node).get_parent()
		if owner and owner.has_method("try_use_station"):
			owner.try_use_station(self)
			return

func _pick_or_drop() -> void:
	var areas: Array = ($InteractArea as Area2D).get_overlapping_areas()
	if held_item == null:
		for a in areas:
			if a.has_method("try_pick_item"):
				var item: Node2D = a.try_pick_item() as Node2D
				if item:
					held_item = item
					if held_item.has_method("set_freeze"):
						held_item.set("freeze", true)
					held_item.position = position + Vector2(0, -16)
					held_item.reparent(self)
					return
	else:
		held_item.reparent(get_parent())
		held_item.position = position + Vector2(0, 16)
		if held_item.has_method("set_freeze"):
			held_item.set("freeze", false)
		held_item = null
