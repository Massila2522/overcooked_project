extends Node2D

@export var cook_duration: float = 2.0

var is_cooking: bool = false
var current_item: Node2D = null

@onready var progress: ProgressBar = $CanvasLayer/ProgressBar
@onready var timer: Timer = $Timer

func try_use_station(player: Node) -> void:
	if is_cooking:
		return
	if current_item == null and player != null and player.has_method("get"):
		if player.get("held_item") != null:
			var held_item = player.get("held_item") as Node2D
			if held_item.has_method("get_display_name"):
				var display_name = held_item.get_display_name()
				if "coupé" in display_name and "cuit" not in display_name:
					current_item = held_item
					(player as Node).set("held_item", null)
					current_item.reparent(self)
					current_item.position = Vector2.ZERO
					_start_cooking()
				elif "cuit" in display_name:
					# L'item est déjà cuit
					return
	elif current_item != null and player.get("held_item") == null:
		if not is_cooking:
			current_item.reparent(player)
			current_item.position = (player as Node2D).position + Vector2(0, -16)
			player.set("held_item", current_item)
			current_item = null

func _start_cooking() -> void:
	is_cooking = true
	progress.visible = true
	progress.value = 0.0
	timer.start(cook_duration)

func _process(delta: float) -> void:
	if is_cooking and timer.time_left > 0.0:
		var t: float = 1.0 - (timer.time_left / cook_duration)
		progress.value = t

func _on_Timer_timeout() -> void:
	is_cooking = false
	progress.visible = false
	if current_item and current_item.has_method("mark_cooked"):
		current_item.call("mark_cooked")
