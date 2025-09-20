extends Node2D

const PlayerScene: PackedScene = preload("res://scenes/Player.tscn")

var player: Node2D
var recipe_manager: RecipeManager

@onready var message_label: Label = $MessageLabel
@onready var status_label: Label = $Label

func _ready() -> void:
	recipe_manager = RecipeManager.new()
	player = PlayerScene.instantiate() as Node2D
	add_child(player)
	player.position = Vector2(100, 200)
	
	_setup_player_stations()
	_connect_signals()
	_update_recipe_ui()

func _setup_player_stations() -> void:
	player.stations = {
		"plate": $PlateStation,
		"cutting": $CuttingStation,
		"cooking": $CookingStation,
		"rendu": $RenduStation
	}

func _connect_signals() -> void:
	if $UI/Menu/BtnPizza.has_signal("pressed"):
		$UI/Menu/BtnPizza.connect("pressed", _on_btn_pizza_pressed)
	if $UI/Menu/BtnFunghi.has_signal("pressed"):
		$UI/Menu/BtnFunghi.connect("pressed", _on_btn_funghi_pressed)
	if $RenduStation.has_signal("delivered"):
		$RenduStation.connect("delivered", _on_delivered)

func show_message(msg: String) -> void:
	status_label.text = msg

func _input(event: InputEvent) -> void:
	if event.is_action_pressed("recipe_1"):
		_set_recipe(recipe_manager.get_recipe_by_name("Pizza"))
	elif event.is_action_pressed("recipe_2"):
		_set_recipe(recipe_manager.get_recipe_by_name("Funghi"))

func _set_recipe(recipe: Recipe) -> void:
	if not recipe:
		return
	_reset_game_state()
	player.current_recipe = recipe
	player.step_index = 0
	player.start_ai()
	show_message("Recette sélectionnée: %s" % recipe.get_display_name())
	$UI/Menu.visible = false

func _reset_game_state() -> void:
	_reset_sources()
	_reset_player()

func _reset_sources() -> void:
	for station in get_tree().get_nodes_in_group("source"):
		station.is_empty = false
		station._update_visual()

func _reset_player() -> void:
	player.held_item = null
	player.has_delivered = false
	player.target_set = false
	player.global_position = Vector2(100, 200)

func _update_recipe_ui() -> void:
	var recipes = recipe_manager.get_all_recipes()
	if recipes.size() >= 2:
		$UI/Menu/BtnPizza.text = recipes[0].get_display_name()
		$UI/Menu/BtnFunghi.text = recipes[1].get_display_name()
		$UI/RecipesRight/BtnPizzaOverlay.text = recipes[0].get_display_name()
		$UI/RecipesRight/BtnFunghiOverlay.text = recipes[1].get_display_name()
	status_label.text = "Choisis une recette: 1=Pizza, 2=Funghi"

func _on_btn_pizza_pressed() -> void:
	_set_recipe(recipe_manager.get_recipe_by_name("Pizza"))

func _on_btn_funghi_pressed() -> void:
	_set_recipe(recipe_manager.get_recipe_by_name("Funghi"))

func _on_delivered() -> void:
	if $PlateStation and not $PlateStation.has_node("Plate"):
		$PlateStation._spawn_plate()
	_reset_sources()
