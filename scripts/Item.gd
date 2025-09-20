class_name Item
extends RigidBody2D

@export var item_type: String = "tomato"
var ingredient: Ingredient

func _ready() -> void:
	ingredient = Ingredient.new(item_type, Ingredient.State.RAW)

func set_ingredient(new_ingredient: Ingredient) -> void:
	ingredient = new_ingredient
	item_type = ingredient.base_type

func get_ingredient() -> Ingredient:
	return ingredient

func mark_cut() -> bool:
	if not ingredient:
		return false
	return ingredient.cut()

func mark_cooked() -> bool:
	if not ingredient:
		return false
	return ingredient.cook()

func get_display_name() -> String:
	if not ingredient:
		return item_type
	return ingredient.display_name

func get_item_type() -> String:
	if not ingredient:
		return item_type
	return ingredient.base_type

func is_same_as(other_ingredient: Ingredient) -> bool:
	if not ingredient or not other_ingredient:
		return false
	return ingredient.is_same_as(other_ingredient)
