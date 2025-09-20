extends Node2D

signal delivered

var stored_items: Array[Node2D] = []
@export var max_items: int = 5

func _ready() -> void:
	_update_visual()

func try_use_station(player: Node) -> void:
	var player_item = player.held_item
	if not player_item or not player_item.is_plate() or stored_items.size() >= max_items:
		return
	_accept_delivery(player, player_item)

func _accept_delivery(player: Node, item: Node2D) -> void:
	item.reparent(self)
	item.position = Vector2.ZERO + Vector2(randf_range(-10, 10), randf_range(-10, 10))
	player.held_item = null
	stored_items.append(item)
	_update_visual()
	emit_signal("delivered")

func _update_visual() -> void:
	var colors = [Color(0.6, 0.6, 0.6, 1), Color(0.3, 1.0, 0.3, 1), Color(1.0, 1.0, 0.3, 1)]
	$Sprite.modulate = colors[0] if stored_items.size() == 0 else colors[1] if stored_items.size() < max_items else colors[2]

func get_stored_count() -> int:
	return stored_items.size()
