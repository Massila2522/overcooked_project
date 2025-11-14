extends Node2D

# Station d'assemblage où les ingrédients sont déposés en attente de traitement
# Le coureur y dépose les ingrédients qui nécessitent un traitement
# Le découpeur/cuisinier y récupère les ingrédients pour les traiter

var stored_items: Array[Node2D] = []
const PROCESSING_META_KEY := "_processing_base_type"

@onready var blackboard: Blackboard = get_node("/root/Blackboard") if has_node("/root/Blackboard") else null

func _ready() -> void:
	if not blackboard:
		# Si la blackboard n'est pas dans la scène, on la trouve via le groupe
		var main = get_tree().get_first_node_in_group("root")
		if main and main.has_node("Blackboard"):
			blackboard = main.get_node("Blackboard")

func try_use_station(agent: Node) -> void:
	if agent == null:
		return
	
	# Si l'agent a un item, le déposer
	if agent.held_item != null:
		_deposit_item(agent)
		return
	
	# Sinon, essayer de prendre un item selon son rôle
	if stored_items.size() == 0:
		return
	
	match agent.agent_role:
		agent.AgentRole.CUTTER:
			_pickup_item_for_cutter(agent)
		agent.AgentRole.ASSEMBLER:
			_pickup_item_for_assembler(agent)
		agent.AgentRole.RUNNER:
			_pickup_item_for_runner(agent)
		_:
			_pickup_item_default(agent)

func _deposit_item(agent: Node) -> void:
	var item = agent.held_item
	if not item:
		return
	var ingredient = item.get_ingredient() if item.has_method("get_ingredient") else null
	
	item.reparent(self)
	item.position = Vector2.ZERO + Vector2(randf_range(-15, 15), randf_range(-15, 15))
	agent.held_item = null
	stored_items.append(item)
	
	# Notifier la blackboard
	if blackboard and _should_track_item_in_blackboard(item):
		blackboard.add_item_to_assembly(item)
	
	if blackboard and item.has_meta(PROCESSING_META_KEY):
		var base_type = str(item.get_meta(PROCESSING_META_KEY))
		blackboard.register_processing_end(base_type)
		item.remove_meta(PROCESSING_META_KEY)

func _pickup_item_default(agent: Node) -> void:
	if stored_items.size() == 0 or agent.held_item != null:
		return
	_transfer_item_to_agent(stored_items[0], agent)

func _pickup_item_for_cutter(agent: Node) -> void:
	if stored_items.size() == 0 or agent.held_item != null:
		return
	
	var recipe: Recipe = agent.current_recipe
	if recipe == null:
		_pickup_item_default(agent)
		return
	
	for item in stored_items:
		var ing = item.get_ingredient() if item.has_method("get_ingredient") else null
		if ing and _ingredient_requires_processing(ing, recipe):
			if blackboard:
				blackboard.register_processing_start(ing.base_type)
			item.set_meta(PROCESSING_META_KEY, ing.base_type)
			_transfer_item_to_agent(item, agent)
			return
	# Aucun ingrédient à traiter, ne rien prendre

func _pickup_item_for_assembler(agent: Node) -> void:
	if stored_items.size() == 0 or agent.held_item != null:
		return
	
	# L'assembleur cherche un ingrédient spécifique qui manque sur l'assiette
	var recipe = agent.current_recipe
	var plate_station = agent.stations.get("plate")
	
	if recipe and plate_station and plate_station.plate:
		var plate = plate_station.plate
		var missing_ingredients = _get_missing_ingredients_for_plate(plate, recipe)
		
		for missing_ing in missing_ingredients:
			for item in stored_items:
				var item_ing = item.get_ingredient() if item.has_method("get_ingredient") else null
				if item_ing and item_ing.is_same_as(missing_ing):
					_transfer_item_to_agent(item, agent)
					return
	
	# Si aucun ingrédient spécifique trouvé, prendre le premier disponible
	_pickup_item_default(agent)

func _pickup_item_for_runner(agent: Node) -> void:
	if stored_items.size() == 0 or agent.held_item != null:
		return
	
	for item in stored_items:
		if _is_cooked_plate(item):
			_transfer_item_to_agent(item, agent)
			return

func _transfer_item_to_agent(item: Node2D, agent: Node) -> void:
	if item == null or agent == null:
		return
	var index = stored_items.find(item)
	if index < 0:
		return
	stored_items.remove_at(index)
	item.reparent(agent)
	item.position = agent.position + Vector2(0, -16)
	agent.held_item = item
	if blackboard and _should_track_item_in_blackboard(item):
		blackboard.remove_item_from_assembly(item)

func _ingredient_requires_processing(ing: Ingredient, recipe: Recipe) -> bool:
	var required_state = _get_required_state_for_ingredient(recipe, ing.base_type)
	if required_state < 0:
		return false
	return ing.state < required_state

func _get_missing_ingredients_for_plate(plate: Node2D, recipe: Recipe) -> Array[Ingredient]:
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

func has_items() -> bool:
	return stored_items.size() > 0

func get_first_item() -> Node2D:
	if stored_items.size() > 0:
		return stored_items[0]
	return null

func get_items() -> Array[Node2D]:
	return stored_items.duplicate()

func _get_required_state_for_ingredient(recipe: Recipe, base_type: String) -> int:
	if not recipe:
		return -1
	for ing in recipe.ingredients:
		if ing.base_type == base_type:
			return int(ing.state)
	return -1

func has_cooked_plate() -> bool:
	for item in stored_items:
		if _is_cooked_plate(item):
			return true
	return false

func _is_plate_item(item: Node2D) -> bool:
	return item != null and item.has_method("is_plate") and item.is_plate()

func _is_cooked_plate(item: Node2D) -> bool:
	return _is_plate_item(item) and item.is_cooked

func _should_track_item_in_blackboard(item: Node2D) -> bool:
	return item != null and item.has_method("get_ingredient")
