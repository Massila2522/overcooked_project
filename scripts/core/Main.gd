extends Node2D

const PlayerScene: PackedScene = preload("res://scenes/player/Player.tscn")

var players: Array[Node2D] = []
var recipe_manager: RecipeManager
var blackboard: Blackboard

@onready var status_label: Label = $Label

func _ready() -> void:
	recipe_manager = RecipeManager.new()
	
	# Créer la Blackboard
	blackboard = Blackboard.new()
	blackboard.name = "Blackboard"
	add_child(blackboard)
	
	# Créer les 3 agents
	_create_agents()
	
	_setup_players_stations()
	_connect_signals()
	# Lancer automatiquement les recettes
	start_recipes_automatically()

func _create_agents() -> void:
	# Créer le coureur
	var runner = PlayerScene.instantiate() as Node2D
	runner.agent_role = runner.AgentRole.RUNNER
	runner.position = Vector2(100, 200)
	add_child(runner)
	players.append(runner)
	
	# Créer le découpeur
	var cutter = PlayerScene.instantiate() as Node2D
	cutter.agent_role = cutter.AgentRole.CUTTER
	cutter.position = Vector2(150, 200)
	add_child(cutter)
	players.append(cutter)
	
	# Créer l'assembleur
	var assembler = PlayerScene.instantiate() as Node2D
	assembler.agent_role = assembler.AgentRole.ASSEMBLER
	assembler.position = Vector2(200, 200)
	add_child(assembler)
	players.append(assembler)

func _setup_players_stations() -> void:
	# Configurer les stations pour tous les agents
	for player in players:
		player.stations = {
			"plate": $PlateStation,
			"cutting": $CuttingStation,
			"cooking": $CookingStation,
			"rendu": $RenduStation,
			"assembly": $AssemblyStation
		}

func _connect_signals() -> void:
	#$UI/Menu/BtnPizza.connect("pressed", _on_btn_pizza_pressed)
	#$UI/Menu/BtnFunghi.connect("pressed", _on_btn_funghi_pressed)
	$RenduStation.connect("delivered", _on_delivered)

func show_message(msg: String) -> void:
	status_label.text = msg

#func _input(event: InputEvent) -> void:
#	if event.is_action_pressed("recipe_1"):
#		_set_recipe(recipe_manager.get_recipe_by_name("Pizza"))
#	elif event.is_action_pressed("recipe_2"):
#		_set_recipe(recipe_manager.get_recipe_by_name("Funghi"))
func _set_recipe(recipe: Recipe) -> void:
	if not recipe:
		return
	_reset_game_state()
	
	# Configurer la recette dans la blackboard
	if blackboard:
		blackboard.set_current_recipe(recipe)
	
	# Configurer la recette pour tous les agents
	for player in players:
		player.current_recipe = recipe
		player.step_index = 0
		player.start_ai()
	
	show_message("Recette sélectionnée: %s" % recipe.get_display_name())
	if has_node("UI/Menu"):
		$UI/Menu.visible = false

func _reset_game_state() -> void:
	_reset_sources()
	_reset_player()

func _reset_sources() -> void:
	for station in get_tree().get_nodes_in_group("source"):
		station.is_empty = false

func _reset_player() -> void:
	# Réinitialiser tous les agents
	for i in range(players.size()):
		var player = players[i]
		player.held_item = null
		player.has_delivered = false
		player.target_set = false
		player.global_position = Vector2(100 + i * 50, 200)




func start_recipes_automatically() -> void:
	var recipes = recipe_manager.get_all_recipes()
	for recipe in recipes:
		_set_recipe(recipe)
		
		# Attendre que la recette soit complétée (via la blackboard)
		if blackboard:
			await blackboard.recipe_completed

		var timer = get_tree().create_timer(5.0)
		await timer.timeout



func _on_delivered() -> void:
	if not $PlateStation.has_node("Plate"):
		$PlateStation._spawn_plate()
	_reset_sources()
	
	# Notifier la blackboard que la recette est complétée
	if blackboard:
		blackboard.emit_signal("recipe_completed")
