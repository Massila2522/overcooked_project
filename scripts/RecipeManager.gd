class_name RecipeManager
extends RefCounted

var recipes: Array[Recipe] = []

func _init() -> void:
	recipes = [
		Recipe.new("Pizza", [
			Ingredient.new("dough", Ingredient.State.RAW),
			Ingredient.new("tomato", Ingredient.State.CUT),
			Ingredient.new("cheese", Ingredient.State.RAW)
		]),
		Recipe.new("Funghi", [
			Ingredient.new("dough", Ingredient.State.RAW),
			Ingredient.new("tomato", Ingredient.State.CUT),
			Ingredient.new("mushroom", Ingredient.State.CUT)
		])
	]

func get_recipe_by_name(name: String) -> Recipe:
	for recipe in recipes:
		if recipe.name == name:
			return recipe
	return null

func get_all_recipes() -> Array[Recipe]:
	return recipes
