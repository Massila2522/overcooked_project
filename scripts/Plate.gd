extends Node2D

var held_foods: Array[Node2D] = []
var deposited_ingredients: Array[String] = []
var is_cooked: bool = false

func is_plate() -> bool:
	return true

func try_place_food(food: Node2D) -> bool:
	if not food:
		return false
	held_foods.append(food)
	food.reparent(self)
	food.position = Vector2(0, -6) + Vector2(randf_range(-10, 10), randf_range(-10, 10))
	deposited_ingredients.append(food.get_display_name())
	return true

func is_empty() -> bool:
	return held_foods.size() == 0 and not is_cooked

func cook_plate() -> void:
	_clear_ingredients()
	_create_cooked_sprite()

func _clear_ingredients() -> void:
	for food in held_foods:
		food.queue_free()
	held_foods.clear()
	deposited_ingredients.clear()
	is_cooked = true

func _create_cooked_sprite() -> void:
	var cooked_sprite = Sprite2D.new()
	var texture = load("res://icon.svg")
	cooked_sprite.texture = texture
	cooked_sprite.scale = Vector2(0.5, 0.5)
	cooked_sprite.position = Vector2(0, -6)
	add_child(cooked_sprite)

func get_display_name() -> String:
	if is_cooked:
		return "🍕 Pizza cuite"
	if deposited_ingredients.size() > 0:
		return "assiette (" + ", ".join(deposited_ingredients) + ")"
	return "assiette (vide)"
