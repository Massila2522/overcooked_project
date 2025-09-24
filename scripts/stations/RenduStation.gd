extends Node2D

signal delivered

@export var max_items: int = 5

var stored_items: Array[Node2D] = []

#func _ready() -> void:

func try_use_station(player: Node) -> void:
	if stored_items.size() >= max_items:
		return
	var player_item = player.held_item
	if not player_item or not player_item.is_plate():
		return
	# Livraison instantanée
	_deliver_instantly(player, player_item)

func _deliver_instantly(player: Node, item: Node2D) -> void:
	item.reparent(self)
	item.position = Vector2.ZERO + Vector2(randf_range(-10, 10), randf_range(-10, 10))
	player.held_item = null
	stored_items.append(item)
	emit_signal("delivered")


func get_stored_count() -> int:
	return stored_items.size()
