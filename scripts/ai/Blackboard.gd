extends Node
class_name Blackboard

# Architecture Blackboard pour la communication entre agents
# Singleton accessible globalement

# Ingrédients en attente à la station d'assemblage
var assembly_table_items: Array[Node2D] = []

# Recette actuelle en cours
var current_recipe: Recipe = null

# État de la recette
var recipe_ingredients_needed: Array[Ingredient] = []
var recipe_ingredients_ready: Array[String] = []  # IDs des ingrédients prêts
var recipe_ready: bool = false
var processing_counts: Dictionary = {}

# Signaux pour la communication
signal ingredient_added_to_assembly(item: Node2D)
signal ingredient_removed_from_assembly(item: Node2D)
signal recipe_ready_to_assemble
signal recipe_completed

func _ready() -> void:
	pass

# Gestion de la recette
func set_current_recipe(recipe: Recipe) -> void:
	current_recipe = recipe
	recipe_ingredients_needed = recipe.ingredients.duplicate() if recipe else []
	recipe_ingredients_ready.clear()
	assembly_table_items.clear()
	recipe_ready = false
	processing_counts.clear()

func get_current_recipe() -> Recipe:
	return current_recipe

# Gestion des ingrédients à la table d'assemblage
func add_item_to_assembly(item: Node2D) -> void:
	if item in assembly_table_items:
		return
	assembly_table_items.append(item)
	emit_signal("ingredient_added_to_assembly", item)
	_check_recipe_ready()

func remove_item_from_assembly(item: Node2D) -> void:
	var index = assembly_table_items.find(item)
	if index >= 0:
		assembly_table_items.remove_at(index)
		emit_signal("ingredient_removed_from_assembly", item)

func get_assembly_items() -> Array[Node2D]:
	return assembly_table_items.duplicate()

func has_items_at_assembly() -> bool:
	return assembly_table_items.size() > 0

# Vérifier si un ingrédient spécifique est disponible à l'assemblage
func get_item_at_assembly_by_type(ingredient_type: String, state: Ingredient.State) -> Node2D:
	for item in assembly_table_items:
		var ing = item.get_ingredient()
		if ing and ing.base_type == ingredient_type and ing.state == state:
			return item
	return null

func register_processing_start(base_type: String) -> void:
	if base_type == "":
		return
	processing_counts[base_type] = processing_counts.get(base_type, 0) + 1

func register_processing_end(base_type: String) -> void:
	if base_type == "":
		return
	if base_type in processing_counts:
		var new_value = max(0, processing_counts[base_type] - 1)
		if new_value <= 0:
			processing_counts.erase(base_type)
		else:
			processing_counts[base_type] = new_value

func get_processing_counts() -> Dictionary:
	return processing_counts.duplicate()

# Vérifier si la recette est prête à être assemblée
func _check_recipe_ready() -> void:
	if not current_recipe:
		return
	
	# Vérifier si tous les ingrédients requis sont présents
	var all_ready = true
	for needed_ing in recipe_ingredients_needed:
		var found = false
		for item in assembly_table_items:
			var ing = item.get_ingredient()
			if ing and ing.is_same_as(needed_ing):
				found = true
				break
		if not found:
			all_ready = false
			break
	
	if all_ready and recipe_ingredients_needed.size() > 0:
		if not recipe_ready:
			recipe_ready = true
			emit_signal("recipe_ready_to_assemble")

func is_recipe_ready() -> bool:
	return recipe_ready
