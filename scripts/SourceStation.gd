extends Node2D

@export var item_type: String = "tomato"
var is_empty: bool = false

func _ready() -> void:
	_update_visual()

func try_use_station(player: Node) -> void:
	if player == null or not player.has_method("get"):
		return
	if not is_empty and player.get("held_item") == null:
		var item_scene: PackedScene = load("res://scenes/Item.tscn")
		var new_item: Node2D = item_scene.instantiate() as Node2D
		add_child(new_item)
		new_item.set("item_type", item_type)
		new_item.reparent(player)
		new_item.position = (player as Node2D).position + Vector2(0, -16)
		player.set("held_item", new_item)
		is_empty = true
		_update_visual()
		return

func _update_visual() -> void:
	var spr: Sprite2D = $Sprite
	if is_empty:
		spr.modulate = Color(0.6,0.6,0.6,1)
	else:
		spr.modulate = Color(0.3,0.6,1.0,1) 