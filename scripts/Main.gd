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
@onready var status_label: Label = $Label
var player_ref: Node = null

func _ready() -> void:
	# Spawn du joueur
	player = PlayerScene.instantiate() as Node2D
	add_child(player)
	player.position = ($Spawn_Player as Node2D).position
	player_ref = player
	
	# Utiliser les stations de la scène
	cutting_station = $ProcessingSection/CuttingStation
	cooking_station = $ProcessingSection/CookingStation
	plate_station = $ProcessingSection/PlateStation
	rendu_station = $ProcessingSection/RenduStation
	# Écouter la livraison
	if rendu_station.has_signal("delivered"):
		rendu_station.connect("delivered", Callable(self, "_on_delivered"))
	
	# Lier les cibles de navigation au joueur
	player.set("target_source", $IngredientsSection/Source_tomato)  # Utiliser une source par défaut
	player.set("target_cutting", $ProcessingSection/CuttingStation)
	player.set("target_cooking", $ProcessingSection/CookingStation)
	player.set("target_rendu", $ProcessingSection/RenduStation)
	player.set("target_plate", $ProcessingSection/PlateStation)
	
	# Initialiser l'interface
	status_label.text = "Choisis une recette: 1=Pizza, 2=Funghi"

func show_message(msg: String) -> void:
	status_label.text = msg
	print(msg)

func _input(event: InputEvent) -> void:
	if event.is_action_pressed("recipe_1"):
		_on_btn_pizza_pressed()
	elif event.is_action_pressed("recipe_2"):
		_on_btn_funghi_pressed()

func _on_btn_pizza_pressed() -> void:
	_set_recipe(["dough","tomato","cheese"], "Pizza")
	_hide_menu()

func _on_btn_funghi_pressed() -> void:
	_set_recipe(["dough","tomato","mushroom"], "Funghi")
	_hide_menu()

func _hide_menu() -> void:
	var menu := $UI/Menu
	if menu:
		menu.visible = false

func _set_recipe(seq: Array[String], name: String) -> void:
	_reset_sources()
	var assembly = get_tree().get_first_node_in_group("assembly")
	if assembly:
		assembly.set("required_sequence", seq)
		assembly.set("dish_name", name)
		assembly.set("pizza_ready", false)
		assembly.set("collected", [])
	var player = player_ref
	if player:
		player.set("current_recipe", seq)
		player.set("step_index", 0)
		if player.has_method("start_ai"):
			player.call("start_ai")
	show_message("Recette sélectionnée: %s" % name)

func _reset_sources() -> void:
	# Réinitialiser toutes les sources d'ingrédients
	var sources = [$IngredientsSection/Source_dough, $IngredientsSection/Source_tomato, $IngredientsSection/Source_cheese, $IngredientsSection/Source_onion, $IngredientsSection/Source_mushroom]
	for s in sources:
		if s and s.has_method("set"):
			s.set("is_empty", false)
			if s.has_method("_update_visual"):
				s.call("_update_visual")

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
		_show_instruction_message("🎉 Mission accomplie ! " + str(rendu_station.get_stored_count()) + " plat(s) livré(s) !")
		last_held_item_state = current_held_item_state
		return

	# PRIORITÉ IMMÉDIATE: si une cuisson/découpe est active et que le joueur n'a rien en main,
	# afficher tout de suite le message correspondant (sans attendre le cooldown)
	if held_item == null:
		if cooking_station.get("is_cooking") == true:
			_show_instruction_message("⏳ Un ingrédient est en train de cuire... Patiente !")
			last_held_item_state = current_held_item_state
			last_message_time = 0.0
			return
		if cutting_station.get("is_cutting") == true:
			_show_instruction_message("⏳ Un ingrédient est en train d'être découpé... Patiente !")
			last_held_item_state = current_held_item_state
			last_message_time = 0.0
			return
	
	# Si l'état a changé ou si le cooldown est écoulé
	if state_changed or last_message_time >= message_cooldown:
		# Si le joueur n'a rien en main
		if held_item == null:
			# Prioriser la cuisson en cours
			if cooking_station.get("is_cooking") == true:
				_show_instruction_message("⏳ Un ingrédient est en train de cuire... Patiente !")
				last_held_item_state = current_held_item_state
				last_message_time = 0.0
				return
			# Ensuite la découpe en cours
			if cutting_station.get("is_cutting") == true:
				_show_instruction_message("⏳ Un ingrédient est en train d'être découpé... Patiente !")
				last_held_item_state = current_held_item_state
				last_message_time = 0.0
				return
			# Sinon, aucun process actif
			_show_instruction_message("🍅 Ramasse les ingrédients pour ta recette !")
		
		# Si le joueur a un item
		elif held_item != null:
			if held_item.has_method("get_display_name"):
				var display_name = held_item.get_display_name()
				if "coupé et cuit" in display_name:
					_show_instruction_message("🎉 Va assembler ton plat !")
				elif "coupé" in display_name:
					_show_instruction_message("🔥 Va cuire l'ingrédient !")
				else:
					_show_instruction_message("🔪 Va découper l'ingrédient !")
		
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
	# 2) Réinitialiser toutes les sources d'ingrédients
	_reset_sources()
