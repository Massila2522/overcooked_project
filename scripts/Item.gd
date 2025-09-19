extends RigidBody2D

@export var item_type: String = "tomate"
var is_cut: bool = false
var is_cooked: bool = false

func mark_cut() -> void:
	is_cut = true

func mark_cooked() -> void:
	is_cooked = true

func get_display_name() -> String:
	var state = ""
	if is_cut and is_cooked:
		state = " (coupé et cuit)"
	elif is_cut:
		state = " (coupé)"
	elif is_cooked:
		state = " (cuit)"
	else:
		state = " (cru)"
	
	return item_type + state
