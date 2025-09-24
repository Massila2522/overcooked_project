class_name Item
extends RigidBody2D

@export var item_type: String = "tomato"
var ingredient: Ingredient

func _ready() -> void:
	ingredient = Ingredient.new(item_type, Ingredient.State.RAW)
	_update_texture()

func set_ingredient(new_ingredient: Ingredient) -> void:
	ingredient = new_ingredient
	item_type = ingredient.base_type
	_update_texture()

func get_ingredient() -> Ingredient:
	return ingredient

func mark_cut() -> bool:
	var result = ingredient and ingredient.cut()
	if result:
		_update_texture()
	return result

func mark_cooked() -> bool:
	var result = ingredient and ingredient.cook()
	if result:
		_update_texture()
	return result

func get_display_name() -> String:
	return ingredient.display_name if ingredient else item_type

func get_item_type() -> String:
	return ingredient.base_type if ingredient else item_type

func is_same_as(other_ingredient: Ingredient) -> bool:
	if not ingredient or not other_ingredient:
		return false
	return ingredient.is_same_as(other_ingredient)

func _update_texture() -> void:
	var sprite = $Sprite
	var texture_path = _get_texture_path()
	if texture_path != "":
		var texture = load(texture_path)
		if texture:
			sprite.texture = texture

func _get_texture_path() -> String:
	var base_type = ingredient.base_type
	var state = ingredient.state
	
	# Chemin de base pour les textures d'ingrédients
	var base_path = "res://sprites/ingredients/"
	
	# Mapping des types d'ingrédients vers les noms de fichiers
	var type_mapping = {
		"tomato": "tomato",
		"mushroom": "mushroom",
		"dough": "dough",
		"onion": "onion",
		"cheese": "cheese"
	}
	
	var file_name = type_mapping.get(base_type, "default")
	
	# Ajouter le suffixe selon l'état
	match state:
		Ingredient.State.RAW:
			return base_path + file_name + "_raw.png"
		Ingredient.State.CUT:
			return base_path + file_name + "_cut.png"
		Ingredient.State.COOKED:
			return base_path + file_name + "_cooked.png"
		Ingredient.State.BURNED:
			return base_path + file_name + "_burned.png"
		_:
			return base_path + file_name + "_raw.png"
