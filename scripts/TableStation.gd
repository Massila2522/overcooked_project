extends Node2D

var stored_item: Node2D = null

func _ready() -> void:
	_update_visual()

func try_use_station(player: Node) -> void:
	if not player:
		return
	var player_item = player.held_item
	if stored_item == null and player_item != null:
		_store_item(player, player_item)
	elif stored_item != null and player_item == null:
		_give_item_to_player(player)

func _store_item(player: Node, item: Node2D) -> void:
	stored_item = item
	stored_item.reparent(self)
	stored_item.position = Vector2.ZERO
	player.held_item = null
	_update_visual()

func _give_item_to_player(player: Node) -> void:
	stored_item.reparent(player)
	stored_item.position = player.position + Vector2(0, -16)
	player.held_item = stored_item
	stored_item = null
	_update_visual()

func _update_visual() -> void:
	var spr: Sprite2D = $Sprite
	if stored_item == null:
		spr.modulate = Color(0.6,0.6,0.6,1)
	else:
		spr.modulate = Color(0.3,0.6,1.0,1) 
