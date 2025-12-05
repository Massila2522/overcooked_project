extends CharacterBody2D

signal recipe_completed

@export var speed: float = 200.0

# État du joueur
var held_item: Node2D = null
var current_recipe: Recipe = null
var step_index: int = 0
var has_delivered: bool = false

# Navigation et IA
var stations: Dictionary = {}
var current_target: Vector2 = Vector2.ZERO
var target_set: bool = false
var goap_agent: Node = null
var nav: NavigationAgent2D

# Évitement d'obstacles
var target_node: Node = null
var target_obstacle_prev_enabled: bool = true

func _ready() -> void:
	# Configuration GOAP
	goap_agent = preload("res://scripts/ai/Goap.gd").new()
	goap_agent.setup(self)
	goap_agent.goal_added.connect(func(t: String): ($"../Label" as Label).text = t)
	goap_agent.subgoal_enqueued.connect(func(t: String): ($"../Label" as Label).text = t)
	
	# Configuration navigation
	nav = $NavigationAgent2D
	nav.velocity_computed.connect(_on_velocity_computed)

func start_ai() -> void:
	has_delivered = false
	if current_recipe != null:
		goap_agent.start(current_recipe)

# Callbacks pour les stations
func on_ingredient_accepted() -> void:
	pass

func on_ingredient_deposited(_deposited_item: Node2D) -> void:
	if not current_recipe:
		return
	step_index = min(step_index + 1, current_recipe.get_ingredient_count())
	target_set = false

# Vérifications d'état
func is_recipe_complete() -> bool:
	return current_recipe != null and step_index >= current_recipe.get_ingredient_count()

func is_ingredient_required(ingredient_type: String) -> bool:
	if not current_recipe or step_index >= current_recipe.get_ingredient_count():
		return false
	var needed_ingredient = current_recipe.get_ingredient_at(step_index)
	return needed_ingredient != null and needed_ingredient.base_type == ingredient_type

# Boucle principale
func _physics_process(delta: float) -> void:
	goap_agent.tick()
	_move(delta)

func _move(_delta: float) -> void:
	if not target_set:
		velocity = Vector2.ZERO
		return
	
	nav.target_position = current_target
	var next_pos: Vector2 = nav.get_next_path_position()
	var to_next: Vector2 = next_pos - global_position
	var distance_to_target = global_position.distance_to(current_target)
	
	# Interaction si proche de la cible
	if distance_to_target < 30.0:
		velocity = Vector2.ZERO
		_auto_try_use()
		return
	
	# Déplacement avec évitement d'obstacles
	nav.set_velocity(to_next.normalized() * speed)

func _on_velocity_computed(safe_velocity: Vector2) -> void:
	velocity = safe_velocity
	move_and_slide()

# Gestion des cibles
func set_goap_target(pos: Vector2) -> void:
	current_target = pos
	target_set = true
	nav.target_position = pos

func set_goap_target_node(node: Node) -> void:
	target_node = node
	# Désactiver l'évitement de la cible pour s'en approcher
	if target_node and target_node.has_node("NavObstacle"):
		var obs: Node = target_node.get_node("NavObstacle")
		target_obstacle_prev_enabled = obs.avoidance_enabled
		obs.avoidance_enabled = false
	set_goap_target(node.global_position)

func _restore_target_obstacle() -> void:
	if target_node and target_node.has_node("NavObstacle"):
		var obs: Node = target_node.get_node("NavObstacle")
		obs.avoidance_enabled = target_obstacle_prev_enabled
		target_node = null

# Utilitaires
func _has_cooked_plate() -> bool:
	return held_item != null and held_item.is_plate() and held_item.is_cooked

func _auto_try_use() -> void:
	var areas: Array = ($InteractArea as Area2D).get_overlapping_areas()
	for area in areas:
		var station: Node = area.get_parent()
		
		# Vérifier que c'est bien une station (pas un Item)
		if station.has_method("try_use_station"):
			station.try_use_station(self)
			
			# Gestion de la livraison
			if station == stations.get("rendu") and _has_cooked_plate():
				target_set = false


			# Wait for signal completed
			if is_recipe_complete() and not has_delivered:
				has_delivered = true
				emit_signal("recipe_completed")
			# Restaurer l'obstacle après utilisation
			if station == target_node:
				_restore_target_obstacle()
			return
