extends Node2D

@export var item_type: String = "tomato"
var is_empty: bool = false

@onready var type_label: Label = Label.new()

func _ready() -> void:
	_update_visual()
	type_label.text = item_type
	type_label.position = Vector2(-12, -24)
	add_child(type_label)

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
		_get_main().show_message("Ingrédient %s pris" % item_type)

func _update_visual() -> void:
	var spr: Sprite2D = $Sprite
	if is_empty:
		spr.modulate = Color(0.6,0.6,0.6,1)
	else:
		spr.modulate = Color(0.3,0.6,1.0,1)

func _get_main() -> Node:
	return get_tree().get_first_node_in_group("root") if get_tree() else get_parent() 