extends Node2D

var held_food: Node2D = null

func is_plate() -> bool:
	return true

func has_food() -> bool:
	return held_food != null

func try_place_food(food: Node2D) -> bool:
	if held_food != null or food == null:
		return false
	# Accepte seulement un item cuit (et éventuellement coupé)
	if food.has_method("get_display_name"):
		var name: String = food.get_display_name()
		if "cuit" in name:
			held_food = food
			food.reparent(self)
			food.position = Vector2(0, -6)
			return true
	return false

func is_empty() -> bool:
	return held_food == null

func take_food() -> Node2D:
	var f := held_food
	held_food = null
	return f

func get_display_name() -> String:
	if held_food != null and held_food.has_method("get_display_name"):
		return "assiette (" + held_food.get_display_name() + ")"
	return "assiette (vide)"

