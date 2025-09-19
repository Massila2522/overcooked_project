extends Node2D

var stored_item: Node2D = null

func _ready() -> void:
	_update_visual()

func try_use_station(player: Node) -> void:
	if player == null or not player.has_method("get"):
		return
	var player_item = player.get("held_item") as Node2D
	if stored_item == null and player_item != null:
		stored_item = player_item
		stored_item.reparent(self)
		stored_item.position = Vector2.ZERO
		player.set("held_item", null)
		_update_visual()
	elif stored_item != null and player_item == null:
		stored_item.reparent(player)
		stored_item.position = (player as Node2D).position + Vector2(0, -16)
		player.set("held_item", stored_item)
		stored_item = null
		_update_visual()

func _update_visual() -> void:
	var spr: Sprite2D = $Sprite
	if stored_item == null:
		spr.modulate = Color(0.6,0.6,0.6,1)
	else:
		spr.modulate = Color(0.3,0.6,1.0,1) 