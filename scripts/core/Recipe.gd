class_name Recipe
extends RefCounted

var name: String
var ingredients: Array[Ingredient] = []

func _init(recipe_name: String, recipe_ingredients: Array[Ingredient]):
	name = recipe_name
	ingredients = recipe_ingredients.duplicate()

func get_ingredient_count() -> int:
	return ingredients.size()

func get_ingredient_at(index: int) -> Ingredient:
	if index < 0 or index >= ingredients.size():
		return null
	return ingredients[index]

func get_display_name() -> String:
	return name
