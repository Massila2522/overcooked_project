extends Node2D

@export var required_sequence: Array[String] = ["dough", "tomato", "cheese"]
@export var dish_name: String = "Plat"

var collected: Array[String] = []
var pizza_ready: bool = false

func _ready() -> void:
	add_to_group("assembly")
	_update_visual()

func get_needed_type() -> String:
	if pizza_ready:
		return ""
	if collected.size() < required_sequence.size():
		return required_sequence[collected.size()]
	return ""

func try_use_station(player: Node) -> void:
	if not player:
		return
	if pizza_ready:
		_reset_assembly()
		return
	var held_item = player.held_item
	if not held_item or held_item.item_type != get_needed_type():
		return
	_accept_ingredient(player, held_item)

func _reset_assembly() -> void:
	pizza_ready = false
	collected.clear()
	_update_visual()
	_get_main().show_message("Plan remis à zéro")

func _accept_ingredient(player: Node, item: Node2D) -> void:
	item.queue_free()
	player.held_item = null
	collected.append(item.item_type)
	_get_main().show_message("Ingrédient %s accepté (%d/%d)" % [item.item_type, collected.size(), required_sequence.size()])
	
	if collected.size() == required_sequence.size():
		pizza_ready = true
		_get_main().show_message("%s prêt !" % dish_name)
	else:
		player.on_ingredient_accepted()
	
	_update_visual()

func _update_visual() -> void:
	var spr: Sprite2D = $Sprite
	if pizza_ready:
		spr.modulate = Color(0.2, 1.0, 0.3, 1)
	else:
		var r = 0.6 - 0.2 * float(collected.size())
		var b = 0.6 + 0.2 * float(collected.size())
		spr.modulate = Color(r, 0.6, b, 1)

func _get_main() -> Node:
	return get_tree().get_first_node_in_group("root") if get_tree() else get_parent()
