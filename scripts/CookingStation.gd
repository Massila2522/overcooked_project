extends Node2D

@export var cook_duration: float = 2.0
var is_cooking: bool = false
var current_item: Node2D = null

@onready var progress: ProgressBar = $CanvasLayer/ProgressBar
@onready var timer: Timer = $Timer

func try_use_station(player: Node) -> void:
	if is_cooking:
		return
	if current_item == null and player.held_item != null:
		_take_item_from_player(player)
	elif current_item != null and player.held_item == null:
		_give_item_to_player(player)

func _take_item_from_player(player: Node) -> void:
	var held_item = player.held_item
	var display_name = held_item.get_display_name()
	if "coupé" in display_name and "cuit" not in display_name:
		current_item = held_item
		player.held_item = null
		current_item.reparent(self)
		current_item.position = Vector2.ZERO
		_start_cooking()

func _give_item_to_player(player: Node) -> void:
	current_item.reparent(player)
	current_item.position = player.position + Vector2(0, -16)
	player.held_item = current_item
	current_item = null

func _start_cooking() -> void:
	is_cooking = true
	progress.visible = true
	progress.value = 0.0
	timer.start(cook_duration)

func _process(delta: float) -> void:
	if is_cooking and timer.time_left > 0.0:
		progress.value = 1.0 - (timer.time_left / cook_duration)

func _on_Timer_timeout() -> void:
	is_cooking = false
	progress.visible = false
	if current_item:
		current_item.mark_cooked()
