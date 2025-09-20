extends Node2D

@export var cut_duration: float = 1.5
var is_cutting: bool = false
var current_item: Node2D = null

@onready var progress: ProgressBar = $CanvasLayer/ProgressBar
@onready var timer: Timer = $Timer

func try_use_station(player: Node) -> void:
	if is_cutting:
		return
	if current_item == null and player.held_item != null:
		_take_item_from_player(player)
	elif current_item != null and player.held_item == null:
		_give_item_to_player(player)

func _take_item_from_player(player: Node) -> void:
	var held_item = player.held_item
	if "coupé" in held_item.get_display_name():
		return
	current_item = held_item
	player.held_item = null
	current_item.reparent(self)
	current_item.position = Vector2.ZERO
	_start_cut()

func _give_item_to_player(player: Node) -> void:
	current_item.reparent(player)
	current_item.position = player.position + Vector2(0, -16)
	player.held_item = current_item
	current_item = null

func _start_cut() -> void:
	is_cutting = true
	progress.visible = true
	progress.value = 0.0
	timer.start(cut_duration)

func _process(delta: float) -> void:
	if is_cutting and timer.time_left > 0.0:
		progress.value = 1.0 - (timer.time_left / cut_duration)

func _on_Timer_timeout() -> void:
	is_cutting = false
	progress.visible = false
	if current_item:
		current_item.mark_cut() 
