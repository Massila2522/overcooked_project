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

class GoapPlanner:
	extends RefCounted
	
	func plan_for_recipe(recipe: Recipe) -> Dictionary:
		var actions: Array[GoapAction] = []
		var subgoals: Array[String] = []
		
		if recipe == null:
			return {"actions": actions, "subgoals": subgoals}
		
		# Planifier pour chaque ingrédient
		for ing in recipe.ingredients:
			actions.append(GoapAction.new("pickup_from_source", "Pickup %s" % ing.base_type, {"ingredient": ing.base_type}))
			
			match ing.state:
				Ingredient.State.RAW:
					subgoals.append("Amener %s à la table" % ing.base_type)
					actions.append(GoapAction.new("place_on_plate", "Place %s on plate" % ing.base_type, {"ingredient": ing.base_type}))
				Ingredient.State.CUT:
					subgoals.append("Amener %s à découpage" % ing.base_type)
					actions.append(GoapAction.new("cut", "Cut %s" % ing.base_type, {"ingredient": ing.base_type}))
					subgoals.append("Amener %s découpée à la table" % ing.base_type)
					actions.append(GoapAction.new("place_on_plate", "Place %s on plate" % ing.base_type, {"ingredient": ing.base_type}))
				Ingredient.State.COOKED:
					subgoals.append("Amener %s à découpage" % ing.base_type)
					actions.append(GoapAction.new("maybe_cut", "Maybe Cut %s" % ing.base_type, {"ingredient": ing.base_type}))
					subgoals.append("Amener %s à cuisson" % ing.base_type)
					actions.append(GoapAction.new("cook", "Cook %s" % ing.base_type, {"ingredient": ing.base_type}))
					subgoals.append("Amener %s cuite à la table" % ing.base_type)
					actions.append(GoapAction.new("place_on_plate", "Place %s on plate" % ing.base_type, {"ingredient": ing.base_type}))
		
		# Finalisation
		subgoals.append("Cuire l'assiette")
		actions.append(GoapAction.new("cook_plate", "Cook Plate", {}))
		subgoals.append("Livrer le plat")
		actions.append(GoapAction.new("deliver", "Deliver", {}))
		
		return {"actions": actions, "subgoals": subgoals}

# État de l'agent
var agent_owner: Node = null
var planner: GoapPlanner = GoapPlanner.new()
var plan: Array[GoapAction] = []
var current_index: int = 0
var active: bool = false
var subgoals_queue: Array[String] = []
var current_action_step_index: int = -1

# Configuration
func setup(owner_node: Node) -> void:
	agent_owner = owner_node

func start(recipe: Recipe) -> void:
	var result: Dictionary = planner.plan_for_recipe(recipe)
	plan = result.get("actions", [])
	subgoals_queue = result.get("subgoals", [])
	current_index = 0
	active = true
	
	if recipe != null:
		var goal_text = "Faire %s" % recipe.get_display_name()
		emit_signal("goal_added", goal_text)
		for sg in subgoals_queue:
			emit_signal("subgoal_enqueued", sg)

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
	if current_index >= plan.size():
		active = false
		return
	
	var action: GoapAction = plan[current_index]
	if current_action_step_index == -1:
		current_action_step_index = agent_owner.step_index
	
	if _execute(action):
		current_index += 1
		current_action_step_index = -1

# Exécution des actions
func _execute(action: GoapAction) -> bool:
	match action.kind:
		"pickup_from_source":
			return _pickup_from_source(action.params.get("ingredient", ""))
		"cut":
			return _use_station("cutting")
		"maybe_cut":
			if agent_owner.held_item and "coupé" in agent_owner.held_item.get_display_name():
				return true
			return _use_station("cutting")
		"cook":
			return _use_station("cooking")
		"place_on_plate":
			return _use_station("plate")
		"cook_plate":
			return _handle_plate_cooking()
		"deliver":
			return _use_station("rendu")
		_:
			return true

func _pickup_from_source(ingredient_type: String) -> bool:
	if agent_owner.held_item != null:
		return true
	
	var target = null
	for station in agent_owner.get_tree().get_nodes_in_group("source"):
		if station.get("item_type") == ingredient_type and not station.get("is_empty"):
			target = station
			break
	
	if target == null:
		return false
	
	agent_owner.set_goap_target_node(target)
	return agent_owner.held_item != null

func _use_station(key: String) -> bool:
	var station = agent_owner.stations.get(key)
	if station == null:
		return true
	
	agent_owner.set_goap_target_node(station)
	
	match key:
		"cutting":
			return agent_owner.held_item != null and ("coupé" in agent_owner.held_item.get_display_name())
		"cooking":
			return agent_owner.held_item != null and ("cuit" in agent_owner.held_item.get_display_name())
		"plate":
			return current_action_step_index >= 0 and agent_owner.step_index > current_action_step_index
		"rendu":
			return agent_owner.held_item == null
		_:
			return true

func _handle_plate_cooking() -> bool:
	var ps = agent_owner.stations.get("plate")
	if ps == null:
		return true
	
	# Si on a déjà une assiette cuite
	if agent_owner.held_item and agent_owner.held_item.is_plate() and agent_owner.held_item.is_cooked:
		return true
	
	# Si l'assiette est cuite et on n'a rien, aller la prendre
	if ps.plate != null and ps.plate.is_cooked and agent_owner.held_item == null:
		agent_owner.set_goap_target_node(ps)
		return agent_owner.held_item != null and agent_owner.held_item.is_plate() and agent_owner.held_item.is_cooked
	
	return false
