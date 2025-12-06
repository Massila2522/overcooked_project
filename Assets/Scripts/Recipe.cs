using System.Collections.Generic;
using UnityEngine;

public class Recipe
{
    public RecipeType Type { get; private set; }
    public List<IngredientType> RequiredIngredients { get; private set; }
    public int Order { get; private set; } // Ordre d'arrivée de la recette
    public bool IsReserved { get; set; } = false; // Si un agent a réservé cette recette
    public Agent ReservedBy { get; set; } = null; // L'agent qui a réservé cette recette

    public Recipe(RecipeType type, int order)
    {
        Type = type;
        Order = order;
        RequiredIngredients = GetRequiredIngredients(type);
    }

    private List<IngredientType> GetRequiredIngredients(RecipeType recipeType)
    {
        switch (recipeType)
        {
            case RecipeType.OnionSoup:
                return new List<IngredientType> { IngredientType.Onion, IngredientType.Onion, IngredientType.Onion };
            
            case RecipeType.TomatoSoup:
                return new List<IngredientType> { IngredientType.Tomato, IngredientType.Tomato, IngredientType.Tomato };
            
            case RecipeType.MushroomSoup:
                return new List<IngredientType> { IngredientType.Mushroom, IngredientType.Mushroom, IngredientType.Mushroom };
            
            case RecipeType.Burger:
                return new List<IngredientType> { IngredientType.BurgerBun, IngredientType.Lettuce, IngredientType.Tomato, IngredientType.Meat };
            
            default:
                return new List<IngredientType>();
        }
    }

    public string GetDisplayName()
    {
        switch (Type)
        {
            case RecipeType.OnionSoup:
                return "Soupe aux oignons";
            case RecipeType.TomatoSoup:
                return "Soupe aux tomates";
            case RecipeType.MushroomSoup:
                return "Soupe aux champignons";
            case RecipeType.Burger:
                return "Hamburger";
            default:
                return "Recette inconnue";
        }
    }

    public bool IsSoup()
    {
        return Type == RecipeType.OnionSoup || 
               Type == RecipeType.TomatoSoup || 
               Type == RecipeType.MushroomSoup;
    }
}

