extends Node2D

signal delivered

# Station de rendu pour déposer les ingrédients coupés
var stored_items: Array[Node2D] = []
@export var max_items: int = 5

func _ready() -> void:
	_update_visual()

func try_use_station(player: Node) -> void:
	if player == null or not player.has_method("get"):
		return
	
	var player_item = player.get("held_item") as Node2D
	
	# Si le joueur a un item et que c'est une assiette avec nourriture, on la prend
	if player_item != null:
		if player_item.has_method("is_plate") and player_item.is_plate() and stored_items.size() < max_items:
			# Prendre l'assiette complète
			player_item.reparent(self)
			player_item.position = Vector2.ZERO + Vector2(randf_range(-10, 10), randf_range(-10, 10))
			player.set("held_item", null)
			stored_items.append(player_item)
			_update_visual()
			emit_signal("delivered")
			return
		# N'accepte pas d'items seuls: il faut une assiette
	
	# Livraison est unidirectionnelle: on ne redonne rien au joueur

func _update_visual() -> void:
	var spr: Sprite2D = $Sprite
	if stored_items.size() == 0:
		spr.modulate = Color(0.6, 0.6, 0.6, 1)  # Gris quand vide
	elif stored_items.size() < max_items:
		spr.modulate = Color(0.3, 1.0, 0.3, 1)  # Vert quand il y a des items
	else:
		spr.modulate = Color(1.0, 1.0, 0.3, 1)  # Jaune quand plein

func get_stored_count() -> int:
	return stored_items.size()

func clear_station() -> void:
	for item in stored_items:
		item.queue_free()
	stored_items.clear()
	_update_visual()
