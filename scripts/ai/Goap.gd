extends Node
class_name GoapAgent

signal goal_added(text: String)
signal subgoal_enqueued(text: String)

# Classes internes
class GoapAction:
	extends RefCounted
	var kind: String
	var name: String
	var params: Dictionary
	
	func _init(action_kind: String, action_name: String, action_params: Dictionary = {}):
		kind = action_kind
		name = action_name
		params = action_params.duplicate()
	
	func as_string() -> String:
		return "%s(%s)" % [name, params]

# État de l'agent
var agent_owner: Node = null
var agent_role: int = 0  # Player.AgentRole
var blackboard: Blackboard = null
var plan: Array[GoapAction] = []
var current_index: int = 0
var active: bool = false
var subgoals_queue: Array[String] = []
var current_action_step_index: int = -1

# Configuration
func setup(owner_node: Node, role: int = 0) -> void:
	agent_owner = owner_node
	agent_role = role
	
	# Trouver la blackboard
	var main = owner_node.get_tree().get_first_node_in_group("root")
	if main and main.has_node("Blackboard"):
		blackboard = main.get_node("Blackboard")

func start(recipe: Recipe) -> void:
	# Cette méthode est conservée pour compatibilité mais n'est plus utilisée
	# Les agents démarrent maintenant avec start_runner_ai, start_cutter_ai, etc.
	pass

func stop() -> void:
	active = false
	plan.clear()
	current_index = 0

func is_done() -> bool:
	return active == false or current_index >= plan.size()

# Boucle principale
func tick() -> void:
	if not active or agent_owner == null:
		return
	
	# Comportement selon le rôle
	match agent_role:
		agent_owner.AgentRole.RUNNER:
			_tick_runner()
		agent_owner.AgentRole.CUTTER:
			_tick_cutter()
		agent_owner.AgentRole.ASSEMBLER:
			_tick_assembler()

# ========== RUNNER (COUREUR) ==========
func start_runner_ai() -> void:
	active = true
	current_index = 0
	plan.clear()
	emit_signal("goal_added", "Coureur: Ramasser les ingrédients")

func _tick_runner() -> void:
	# Le coureur ramasse les ingrédients, et livre le plat final
	var assembly_station = agent_owner.stations.get("assembly")
	var plate_station = agent_owner.stations.get("plate")
	
	if agent_owner.held_item == null:
		# Vérifier si une assiette cuite est prête à la station d'assiettes
		if plate_station and plate_station.has_method("has_ready_plate") and plate_station.has_ready_plate():
			agent_owner.set_goap_target_node(plate_station)
			return
		
		# Vérifier si un plat fini attend à la table d'assemblage
		if assembly_station and assembly_station.has_method("has_cooked_plate") and assembly_station.has_cooked_plate():
			agent_owner.set_goap_target_node(assembly_station)
			return
		
		# Chercher un ingrédient manquant selon la recette
		var recipe = blackboard.get_current_recipe() if blackboard else null
		if recipe:
			var assembly_items = blackboard.get_assembly_items() if blackboard else []
			
			var needed_counts: Dictionary = {}
			var required_states: Dictionary = {}
			for ing in recipe.ingredients:
				var key = ing.base_type
				needed_counts[key] = needed_counts.get(key, 0) + 1
				required_states[key] = int(ing.state)
			
			var fulfilled_counts: Dictionary = {}
			for item in assembly_items:
				var ing = item.get_ingredient() if item.has_method("get_ingredient") else null
				if not ing:
					continue
				var required_state = required_states.get(ing.base_type, -1)
				if required_state >= 0 and ing.state >= required_state:
					fulfilled_counts[ing.base_type] = fulfilled_counts.get(ing.base_type, 0) + 1
			
			var processing_counts = blackboard.get_processing_counts() if blackboard else {}
			for base_type in processing_counts.keys():
				fulfilled_counts[base_type] = fulfilled_counts.get(base_type, 0) + processing_counts[base_type]
			
			for base_type in needed_counts.keys():
				var needed_count = needed_counts.get(base_type, 0)
				var current_count = fulfilled_counts.get(base_type, 0)
				if current_count < needed_count:
					var source = _find_source_for_ingredient(base_type)
					if source and not source.get("is_empty"):
						agent_owner.set_goap_target_node(source)
						return
		
		# Si tous les ingrédients sont déjà à la table, attendre près de la table d'assemblage
		if assembly_station:
			var distance = agent_owner.global_position.distance_to(assembly_station.global_position)
			if distance > 50.0:
				agent_owner.set_goap_target_node(assembly_station)
			else:
				agent_owner.target_set = false
		return
	else:
		var item = agent_owner.held_item
		if item and item.has_method("is_plate") and item.is_plate() and item.is_cooked:
			var rendu_station = agent_owner.stations.get("rendu")
			if rendu_station:
				agent_owner.set_goap_target_node(rendu_station)
				return
		
		if assembly_station:
			agent_owner.set_goap_target_node(assembly_station)
			return

func _ingredient_needs_processing(ing: Ingredient) -> bool:
	# Vérifier dans la recette si cet ingrédient doit être découpé ou cuit
	var recipe = blackboard.get_current_recipe() if blackboard else null
	if not recipe:
		return false
	
	for recipe_ing in recipe.ingredients:
		if recipe_ing.base_type == ing.base_type:
			return recipe_ing.state != Ingredient.State.RAW
	
	# Aucun ingrédient correspondant trouvé dans la recette
	return false
	
# ========== CUTTER (DÉCOUPEUR) ==========
func start_cutter_ai() -> void:
	active = true
	current_index = 0
	plan.clear()
	emit_signal("goal_added", "Découpeur: Traiter les ingrédients")

func _tick_cutter() -> void:
	var recipe = blackboard.get_current_recipe() if blackboard else null
	var assembly_station = agent_owner.stations.get("assembly")
	var cutting_station = agent_owner.stations.get("cutting")
	var cooking_station = agent_owner.stations.get("cooking")
	
	if agent_owner.held_item == null:
		# Priorité : récupérer les items prêts sur les stations de traitement
		if _processing_station_has_ready_item(cutting_station, "is_cutting"):
			agent_owner.set_goap_target_node(cutting_station)
			return
		if _processing_station_has_ready_item(cooking_station, "is_cooking"):
			agent_owner.set_goap_target_node(cooking_station)
			return
		
		# Sinon, aller chercher un ingrédient nécessitant un traitement
		if recipe and assembly_station and _assembly_has_item_requiring_processing(assembly_station, recipe):
			agent_owner.set_goap_target_node(assembly_station)
			return
		
		# Sinon attendre près de la station de découpe
		_wait_near_station(cutting_station)
		return
	
	# Le découpeur transporte un ingrédient, déterminer l'étape suivante
	var item = agent_owner.held_item
	var ing = item.get_ingredient() if item.has_method("get_ingredient") else null
	if ing == null or recipe == null:
		if assembly_station:
			agent_owner.set_goap_target_node(assembly_station)
		return
	
	var required_state = _get_required_state_for(ing.base_type, recipe)
	if required_state == -1:
		if assembly_station:
			agent_owner.set_goap_target_node(assembly_station)
		return
	
	# Vérifier si l'ingrédient doit être découpé
	if required_state >= Ingredient.State.CUT and ing.state < Ingredient.State.CUT:
		if cutting_station:
			agent_owner.set_goap_target_node(cutting_station)
			return
	
	# Vérifier si l'ingrédient doit être cuit
	# On ne cuit que si l'état requis est exactement COOKED
	# ET que l'ingrédient n'est pas déjà dans l'état requis
	if required_state == Ingredient.State.COOKED:
		if ing.state < Ingredient.State.COOKED:
			if cooking_station:
				agent_owner.set_goap_target_node(cooking_station)
				return
		else:
			# Déjà cuit, retour à la table
			if assembly_station:
				agent_owner.set_goap_target_node(assembly_station)
				return
	
	# Si l'état requis est CUT et que l'ingrédient est déjà CUT, il est prêt
	if required_state == Ingredient.State.CUT and ing.state >= Ingredient.State.CUT:
		if assembly_station:
			agent_owner.set_goap_target_node(assembly_station)
			return
	
	# Si l'état requis est RAW et que l'ingrédient est RAW, il est prêt
	if required_state == Ingredient.State.RAW and ing.state == Ingredient.State.RAW:
		if assembly_station:
			agent_owner.set_goap_target_node(assembly_station)
			return
	
	# Par défaut, retour à la table d'assemblage
	if assembly_station:
		agent_owner.set_goap_target_node(assembly_station)

func _needs_cutting(ing: Ingredient) -> bool:
	var recipe = blackboard.get_current_recipe() if blackboard else null
	if not recipe:
		return false
	
	for recipe_ing in recipe.ingredients:
		if recipe_ing.base_type == ing.base_type:
			return recipe_ing.state == Ingredient.State.CUT or recipe_ing.state == Ingredient.State.COOKED
	
	return false

func _needs_cooking(ing: Ingredient) -> bool:
	var recipe = blackboard.get_current_recipe() if blackboard else null
	if not recipe:
		return false
	
	for recipe_ing in recipe.ingredients:
		if recipe_ing.base_type == ing.base_type:
			return recipe_ing.state == Ingredient.State.COOKED
	
	return false

# ========== ASSEMBLER (ASSEMBLEUR) ==========
func start_assembler_ai() -> void:
	active = true
	current_index = 0
	plan.clear()
	emit_signal("goal_added", "Assembleur: Assembler et servir")

func _tick_assembler() -> void:
	var plate_station = agent_owner.stations.get("plate")
	var assembly_station = agent_owner.stations.get("assembly")
	var recipe = blackboard.get_current_recipe() if blackboard else null
	
	if plate_station == null or recipe == null:
		agent_owner.target_set = false
		return
	
	var plate = plate_station.plate
	var ingredient_count = recipe.get_ingredient_count()
	
	if agent_owner.held_item:
		var held = agent_owner.held_item
		if held and held.has_method("is_plate") and held.is_plate():
			if held.is_cooked:
				if assembly_station:
					agent_owner.set_goap_target_node(assembly_station)
			else:
				if assembly_station:
					agent_owner.set_goap_target_node(assembly_station)
				else:
					_wait_near_station(plate_station)
		else:
			if plate_station:
				agent_owner.set_goap_target_node(plate_station)
			else:
				agent_owner.target_set = false
		return
	
	# Aucune tenue : surveiller l'assiette cuite et laisser le runner la récupérer
	if plate and plate.is_cooked:
		_wait_near_station(plate_station)
		return
	
	# Attendre que tous les ingrédients soient prêts avant d'assembler
	if not (blackboard and blackboard.is_recipe_ready()):
		_wait_near_station(plate_station)
		return
	
	if plate == null:
		_wait_near_station(plate_station)
		return
	
	var missing = _get_missing_ingredients(plate, recipe)
	if missing.size() > 0:
		if assembly_station and assembly_station.has_method("has_items") and assembly_station.has_items():
			agent_owner.set_goap_target_node(assembly_station)
		else:
			_wait_near_station(assembly_station if assembly_station else plate_station)
		return
	
	# Tous les ingrédients sont sur l'assiette, attendre la cuisson
	if not plate.is_cooked:
		_wait_near_station(plate_station)
	return

# Utilitaires
func _find_source_for_ingredient(ingredient_type: String) -> Node:
	for station in agent_owner.get_tree().get_nodes_in_group("source"):
		if station.get("item_type") == ingredient_type:
			return station
	return null

func _get_missing_ingredients(plate: Node2D, recipe: Recipe) -> Array[Ingredient]:
	# Retourner la liste des ingrédients manquants sur l'assiette
	var missing: Array[Ingredient] = []
	
	if not recipe or not plate:
		return missing
	
	# Créer une liste des ingrédients requis
	var required_ingredients = recipe.ingredients.duplicate()
	
	# Créer une liste des ingrédients déjà sur l'assiette
	var plate_ingredients: Array[Ingredient] = []
	if plate.has_method("get"):
		var foods = plate.get("held_foods")
		if foods is Array:
			for food in foods:
				var food_ing = food.get_ingredient() if food.has_method("get_ingredient") else null
				if food_ing:
					plate_ingredients.append(food_ing)
	
	# Pour chaque ingrédient requis, vérifier s'il est sur l'assiette
	for required_ing in required_ingredients:
		var found = false
		for plate_ing in plate_ingredients:
			if plate_ing.is_same_as(required_ing):
				found = true
				break
		if not found:
			missing.append(required_ing)
	
	return missing

func _processing_station_has_ready_item(station: Node, processing_flag: String) -> bool:
	if station == null:
		return false
	var current_item = station.get("current_item")
	var is_processing = bool(station.get(processing_flag))
	return current_item != null and not is_processing

func _assembly_has_item_requiring_processing(assembly_station: Node, recipe: Recipe) -> bool:
	if assembly_station == null or recipe == null:
		return false
	if not assembly_station.has_method("get_items"):
		return false
	var items = assembly_station.get_items()
	if items is Array:
		for item in items:
			var ing = item.get_ingredient() if item.has_method("get_ingredient") else null
			if ing:
				var required_state = _get_required_state_for(ing.base_type, recipe)
				if required_state >= 0 and ing.state < required_state:
					return true
	return false

func _get_required_state_for(base_type: String, recipe: Recipe) -> int:
	if recipe == null:
		return -1
	for ing in recipe.ingredients:
		if ing.base_type == base_type:
			return int(ing.state)
	return -1

func _wait_near_station(station: Node) -> void:
	if station == null:
		agent_owner.target_set = false
		return
	var distance = agent_owner.global_position.distance_to(station.global_position)
	if distance > 40.0:
		agent_owner.set_goap_target_node(station)
	else:
		agent_owner.target_set = false
