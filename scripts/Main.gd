extends Node2D

const PlayerScene: PackedScene = preload("res://scenes/Player.tscn")
const ItemScene: PackedScene = preload("res://scenes/Item.tscn")
const CuttingStationScene: PackedScene = preload("res://scenes/CuttingStation.tscn")
const CookingStationScene: PackedScene = preload("res://scenes/CookingStation.tscn")
const RenduStationScene: PackedScene = preload("res://scenes/RenduStation.tscn")

var player: Node2D
var cutting_station: Node2D
var cooking_station: Node2D
var plate_station: Node2D
var rendu_station: Node2D
var last_message_time: float = 0.0
var message_cooldown: float = 2.0
var last_held_item_state: bool = false

@onready var message_label: Label = $MessageLabel

func _ready() -> void:
	
	# Spawn du joueur
	player = PlayerScene.instantiate() as Node2D
	add_child(player)
	player.position = ($Spawn_Player as Node2D).position
	
	# La tomate sera générée par la station source quand le joueur interagira avec
	
	# Spawn de la station de découpe
	cutting_station = CuttingStationScene.instantiate() as Node2D
	add_child(cutting_station)
	cutting_station.position = ($Spawn_Cutting as Node2D).position
	
	# Spawn de la station de cuisson
	cooking_station = CookingStationScene.instantiate() as Node2D
	add_child(cooking_station)
	cooking_station.position = ($Spawn_Cooking as Node2D).position

	# Spawn de la station d'assiettes
	plate_station = load("res://scenes/PlateStation.tscn").instantiate() as Node2D
	add_child(plate_station)
	if has_node("Spawn_Plate"):
		plate_station.position = ($Spawn_Plate as Node2D).position
	else:
		plate_station.position = Vector2(625, 200)
	
	# Spawn de la station de rendu
	rendu_station = RenduStationScene.instantiate() as Node2D
	add_child(rendu_station)
	rendu_station.position = ($Spawn_Rendu as Node2D).position
	# Écouter la livraison
	if rendu_station.has_signal("delivered"):
		rendu_station.connect("delivered", Callable(self, "_on_delivered"))
	
	# Lier les cibles de navigation au joueur
	player.set("target_source", $SourceStation)
	player.set("target_cutting", cutting_station)
	player.set("target_cooking", cooking_station)
	player.set("target_rendu", rendu_station)
	player.set("target_plate", plate_station)
	
	# Premier message d'instruction
	_show_instruction_message("🍅 Ramasse la tomate !")

func _process(delta: float) -> void:
	last_message_time += delta
	_check_and_show_instruction()

func _check_and_show_instruction() -> void:
	var held_item = player.get("held_item") as Node2D
	var current_held_item_state = (held_item != null)
	
	# Vérifier si l'état a changé (ramassage/dépôt d'objet)
	var state_changed = (current_held_item_state != last_held_item_state)
	
	# Vérifier si la station de rendu a des items (mission accomplie)
	if rendu_station.has_method("get_stored_count") and rendu_station.get_stored_count() > 0:
		_show_instruction_message("🎉 Mission accomplie ! " + str(rendu_station.get_stored_count()) + " tomate(s) livrée(s) !")
		last_held_item_state = current_held_item_state
		return

	# PRIORITÉ IMMÉDIATE: si une cuisson/découpe est active et que le joueur n'a rien en main,
	# afficher tout de suite le message correspondant (sans attendre le cooldown)
	if held_item == null:
		if cooking_station.get("is_cooking") == true:
			_show_instruction_message("⏳ La tomate est en train de cuire... Patiente !")
			last_held_item_state = current_held_item_state
			last_message_time = 0.0
			return
		if cutting_station.get("is_cutting") == true:
			_show_instruction_message("⏳ La tomate est en train d'être découpée... Patiente !")
			last_held_item_state = current_held_item_state
			last_message_time = 0.0
			return
	
	# Si l'état a changé ou si le cooldown est écoulé
	if state_changed or last_message_time >= message_cooldown:
		# Si le joueur n'a rien en main
		if held_item == null:
			# Prioriser la cuisson en cours
			if cooking_station.get("is_cooking") == true:
				_show_instruction_message("⏳ La tomate est en train de cuire... Patiente !")
				last_held_item_state = current_held_item_state
				last_message_time = 0.0
				return
			# Ensuite la découpe en cours
			if cutting_station.get("is_cutting") == true:
				_show_instruction_message("⏳ La tomate est en train d'être découpée... Patiente !")
				last_held_item_state = current_held_item_state
				last_message_time = 0.0
				return
			# Sinon, aucun process actif
			_show_instruction_message("🍅 Ramasse la tomate !")
		
		# Si le joueur a un item
		elif held_item != null:
			if held_item.has_method("get_display_name"):
				var display_name = held_item.get_display_name()
				if "coupé et cuit" in display_name:
					_show_instruction_message("🎉 Donne la tomate cuite !")
				elif "coupé" in display_name:
					_show_instruction_message("🔥 Va cuire la tomate !")
				else:
					_show_instruction_message("🔪 Va découper la tomate !")
		
		last_held_item_state = current_held_item_state
		last_message_time = 0.0

func _show_instruction_message(message: String) -> void:
	message_label.text = message 

func _on_delivered() -> void:
	# Après livraison, remettre le joueur sur le flux initial
	# 1) Régénérer une assiette sur la station d'assiettes si vide
	if plate_station != null and not plate_station.has_node("Plate"):
		# respawn simple en réinitialisant la station
		if plate_station.has_method("_spawn_plate"):
			plate_station.call("_spawn_plate")
	# 2) Réinitialiser la source pour qu'elle indique des ingrédients dispos
	if has_node("SourceStation"):
		var src = $SourceStation
		src.set("is_empty", false)
		if src.has_method("_update_visual"):
			src.call("_update_visual")
