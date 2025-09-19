extends Node2D

const PlayerScene: PackedScene = preload("res://scenes/Player.tscn")

@onready var status_label: Label = $Label
var player_ref: Node = null

func _ready() -> void:
	var player: Node2D = PlayerScene.instantiate() as Node2D
	add_child(player)
	player.position = ($Spawn_Player as Node2D).position
	player_ref = player
	status_label.text = "Choisis une recette: 1=Pizza, 2=Funghi"

func show_message(msg: String) -> void:
	status_label.text = msg
	print(msg)

func _input(event: InputEvent) -> void:
	if event.is_action_pressed("recipe_1"):
		_on_btn_pizza_pressed()
	elif event.is_action_pressed("recipe_2"):
		_on_btn_funghi_pressed()

func _on_btn_pizza_pressed() -> void:
	_set_recipe(["dough","tomato","cheese"], "Pizza")
	_hide_menu()

func _on_btn_funghi_pressed() -> void:
	_set_recipe(["dough","tomato","mushroom"], "Funghi")
	_hide_menu()

func _hide_menu() -> void:
	var menu := $UI/Menu
	if menu:
		menu.visible = false

func _set_recipe(seq: Array[String], name: String) -> void:
	_reset_sources()
	var assembly = get_tree().get_first_node_in_group("assembly")
	if assembly:
		assembly.set("required_sequence", seq)
		assembly.set("dish_name", name)
		assembly.set("pizza_ready", false)
		assembly.set("collected", [])
	var player = player_ref
	if player:
		player.set("current_recipe", seq)
		player.set("step_index", 0)
		if player.has_method("start_ai"):
			player.call("start_ai")
	show_message("Recette sélectionnée: %s" % name)

func _reset_sources() -> void:
	for s in get_tree().get_nodes_in_group("source"):
		s.set("is_empty", false)
		if s.has_method("_update_visual"):
			s.call("_update_visual")
