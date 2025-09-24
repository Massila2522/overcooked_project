extends Node2D

@export var item_type: String = "tomato"
var is_empty: bool = false

@onready var type_label: Label = $Label

func _ready() -> void:
	_update_visual()
	type_label.text = item_type.to_upper()
	type_label.position = Vector2(-type_label.text.length() * 3, -30)

func try_use_station(player: Node) -> void:
	if is_empty or player.held_item != null:
		return
	if not player.is_ingredient_required(item_type):
		return
	_give_item_to_player(player)

func _give_item_to_player(player: Node) -> void:
	var item_scene: PackedScene = load("res://scenes/Item.tscn")
	var new_item: Node2D = item_scene.instantiate() as Node2D
	new_item.item_type = item_type
	add_child(new_item)
	new_item.reparent(player)
	new_item.position = player.position + Vector2(0, -16)
	player.held_item = new_item
	is_empty = true
	_update_visual()
	_get_main().show_message("Ingrédient %s pris" % item_type)
	player.on_ingredient_accepted()

func _update_visual() -> void:
	$Sprite.modulate = Color(0.6,0.6,0.6,1) if is_empty else Color(0.3,0.6,1.0,1)

func _get_main() -> Node:
	return get_tree().get_first_node_in_group("root") 
