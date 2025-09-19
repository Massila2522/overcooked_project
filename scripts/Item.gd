extends RigidBody2D

@export var item_type: String = "tomato"
var is_cut: bool = false

func mark_cut() -> void:
	is_cut = true

func get_display_name() -> String:
	return item_type + (" (coup)" if is_cut else " (cru)") 