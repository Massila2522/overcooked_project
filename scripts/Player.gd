extends CharacterBody2D

@export var speed: float = 140.0
@export var autonomous: bool = true

var held_item: Node2D = null
var _interact_cooldown: float = 0.0

# Cibles des stations (assignées depuis Main.gd)
var target_source: Node2D = null
var target_cutting: Node2D = null
var target_cooking: Node2D = null
var target_rendu: Node2D = null
var target_plate: Node2D = null

func _ready() -> void:
	if autonomous:
		pass

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
	var target := _select_autonomous_target()
	if target != null:
		var to_target: Vector2 = (target.global_position - global_position)
		if to_target.length() > 4.0:
			velocity = to_target.normalized() * speed
			move_and_slide()
			# Si proche de la cible, tenter d'interagir
			if to_target.length() < 24.0 and _interact_cooldown <= 0.0:
				_auto_try_use()
		else:
			velocity = Vector2.ZERO
	else:
		velocity = Vector2.ZERO

func _auto_try_use() -> void:
	var areas: Array = ($InteractArea as Area2D).get_overlapping_areas()
	for a in areas:
		var owner: Node = (a as Node).get_parent()
		if owner and owner.has_method("try_use_station"):
			owner.try_use_station(self)
			_interact_cooldown = 0.2
			return

func _select_autonomous_target() -> Node2D:
	# 1) Si un traitement est en cours sur une station, rester à proximité
	if held_item == null:
		# Priorité: rester près d'une station en cours
		if target_cooking != null and target_cooking.get("is_cooking") == true:
			return target_cooking
		if target_cutting != null and target_cutting.get("is_cutting") == true:
			return target_cutting
		# Ensuite: récupérer un item prêt sur une station
		if target_cooking != null and target_cooking.get("current_item") != null:
			return target_cooking
		if target_cutting != null and target_cutting.get("current_item") != null:
			return target_cutting

	# 2) Sinon, décider selon l'état de l'objet tenu
	if held_item == null:
		# Aller à la source seulement si elle a encore des ingrédients
		if target_source != null and target_source.get("is_empty") == false:
			return target_source
		# Sinon rester près de la chaîne de production (préférence cuisson > découpe)
		if target_cooking != null:
			return target_cooking
		if target_cutting != null:
			return target_cutting
		return target_source
	
	if held_item.has_method("get_display_name"):
		var name: String = held_item.get_display_name()
		if "assiette" in name:
			# Si on tient déjà une assiette, aller rendre
			return target_rendu
		elif "coupé et cuit" in name:
			if target_plate != null:
				return target_plate
			else:
				return target_rendu
		elif "coupé" in name:
			return target_cooking
		else:
			return target_cutting

	return target_source

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
