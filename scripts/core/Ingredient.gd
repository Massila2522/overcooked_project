class_name Ingredient
extends RefCounted

enum State { RAW, CUT, COOKED, BURNED }

var base_type: String
var state: State = State.RAW
var display_name: String

func _init(type: String, initial_state: State = State.RAW):
	base_type = type
	state = initial_state
	_update_display_name()

func _update_display_name() -> void:
	display_name = base_type + ("" if state == State.RAW else " coupé" if state == State.CUT else " cuit" if state == State.COOKED else " brûlé")

func cut() -> bool:
	if state == State.RAW:
		state = State.CUT
		_update_display_name()
		return true
	return false

func cook() -> bool:
	if state == State.RAW or state == State.CUT:
		state = State.COOKED
		_update_display_name()
		return true
	return false

func get_id() -> String:
	return base_type + "_" + State.keys()[state].to_lower()

func is_same_as(other: Ingredient) -> bool:
	return base_type == other.base_type and state == other.state
