extends Node2D

const PlayerScene: PackedScene = preload("res://scenes/Player.tscn")

func _ready() -> void:
	var player: Node2D = PlayerScene.instantiate() as Node2D
	add_child(player)
	player.position = ($Spawn_Player as Node2D).position 
