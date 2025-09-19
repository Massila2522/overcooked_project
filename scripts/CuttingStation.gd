extends Node2D

@export var cut_duration: float = 1.5

var is_cutting: bool = false
var current_item: Node2D = null

@onready var progress: ProgressBar = $CanvasLayer/ProgressBar
@onready var timer: Timer = $Timer

func try_use_station(player: Node) -> void:
	if is_cutting:
		return
	if current_item == null and player != null and player.has_method("get"):
		if player.get("held_item") != null:
			current_item = player.get("held_item") as Node2D
			(player as Node).set("held_item", null)
			current_item.reparent(self)
			current_item.position = Vector2.ZERO
			_start_cut()
	elif current_item != null and player.get("held_item") == null:
		if not is_cutting:
			current_item.reparent(player)
			current_item.position = (player as Node2D).position + Vector2(0, -16)
			player.set("held_item", current_item)
			current_item = null

func _start_cut() -> void:
	is_cutting = true
	progress.visible = true
	progress.value = 0.0
	timer.start(cut_duration)

func _process(delta: float) -> void:
	if is_cutting and timer.time_left > 0.0:
		var t: float = 1.0 - (timer.time_left / cut_duration)
		progress.value = t

func _on_Timer_timeout() -> void:
	is_cutting = false
	progress.visible = false
	if current_item and current_item.has_method("mark_cut"):
		current_item.call("mark_cut") 