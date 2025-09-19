extends CharacterBody2D

@export var speed: float = 140.0
@export var autonomous: bool = true

var held_item: Node2D = null
var _auto_dir: Vector2 = Vector2(1, 0)
var _interact_cooldown: float = 0.0

func _ready() -> void:
	if autonomous:
		_auto_dir = Vector2(1, 0).rotated(randf() * TAU).normalized()

func _physics_process(delta: float) -> void:
	if autonomous:
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
		if autonomous:
			_auto_try_use()

func _autonomous_move(delta: float) -> void:
	velocity = _auto_dir * speed
	var collision := move_and_collide(velocity * delta)
	if collision:
		_auto_dir = _auto_dir.bounce(collision.get_normal()).normalized()
		velocity = _auto_dir * speed

	var vp: Rect2 = get_viewport_rect()
	var pos := global_position
	var bounced := false
	if pos.x < 16:
		pos.x = 16
		_auto_dir.x = abs(_auto_dir.x)
		bounced = true
	elif pos.x > vp.size.x - 16:
		pos.x = vp.size.x - 16
		_auto_dir.x = -abs(_auto_dir.x)
		bounced = true
	if pos.y < 32:
		pos.y = 32
		_auto_dir.y = abs(_auto_dir.y)
		bounced = true
	elif pos.y > vp.size.y - 16:
		pos.y = vp.size.y - 16
		_auto_dir.y = -abs(_auto_dir.y)
		bounced = true
	if bounced:
		_auto_dir = _auto_dir.normalized()
	global_position = pos

func _auto_try_use() -> void:
	var areas: Array = ($InteractArea as Area2D).get_overlapping_areas()
	for a in areas:
		var owner: Node = (a as Node).get_parent()
		if owner and owner.has_method("try_use_station"):
			owner.try_use_station(self)
			_interact_cooldown = 0.2
			return

func _input(event: InputEvent) -> void:
	if autonomous:
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
