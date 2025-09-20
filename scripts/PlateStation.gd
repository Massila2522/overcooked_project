extends Node2D

var plate: Node2D = null

func _ready() -> void:
	_update_visual()
	# Spawn initial d'une assiette si absente
	if plate == null:
		_spawn_plate()

func _spawn_plate() -> void:
	var plate_scene: PackedScene = load("res://scenes/Plate.tscn")
	plate = plate_scene.instantiate() as Node2D
	add_child(plate)
	plate.position = Vector2.ZERO
	_update_visual()

func try_use_station(player: Node) -> void:
	if player == null or not player.has_method("get"):
		return
	var player_item := player.get("held_item") as Node2D
	# Si le joueur tient un aliment, tenter de le poser dans l'assiette
	if player_item != null and plate != null and plate.has_method("try_place_food"):
		if plate.try_place_food(player_item):
			# Retirer l'aliment des mains du joueur
			player.set("held_item", null)
			# Donner immédiatement l'assiette complétée au joueur
			plate.reparent(player)
			plate.position = (player as Node2D).position + Vector2(0, -16)
			player.set("held_item", plate)
			plate = null
			_update_visual()
			return
		else:
			# Si l'aliment n'est pas accepté (pas cuit), ne rien faire
			return
	# Sinon, si le joueur ne tient rien, donner l'assiette seulement si elle contient de la nourriture
	if player_item == null and plate != null:
		if plate.has_method("is_empty") and plate.is_empty():
			return
		# Prendre l'assiette complète
		plate.reparent(player)
		plate.position = (player as Node2D).position + Vector2(0, -16)
		player.set("held_item", plate)
		plate = null
		_update_visual()
		return

func _update_visual() -> void:
	var spr: Sprite2D = $Sprite
	if plate == null:
		spr.modulate = Color(0.6,0.6,0.6,1)
	else:
		spr.modulate = Color(1.0,0.9,0.6,1)

