using UnityEngine;
using System.Collections.Generic;

public class PlateStation : Station
{
    private GameObject currentPlate;
    private Recipe currentRecipe;
    private List<Ingredient> ingredientsOnPlate = new List<Ingredient>();

    public bool PlacePlate(GameObject plate, Agent agent)
    {
        if (currentPlate != null || plate == null || agent == null) return false;
        if (!TryReserve(agent)) return false;

        currentPlate = plate;
        
        if (plate != null)
        {
            plate.transform.position = transform.position;
            plate.SetActive(true);
        }

        return true;
    }

    public bool AddIngredient(Ingredient ingredient)
    {
        if (currentPlate == null) return false;
        if (currentRecipe == null) return false;

        ingredientsOnPlate.Add(ingredient);
        
        if (ingredient.GameObject != null)
        {
            // Positionner l'ingrédient sur l'assiette (légèrement au-dessus)
            ingredient.GameObject.transform.position = transform.position + Vector3.up * 0.2f;
            ingredient.GameObject.SetActive(true); // Garder l'ingrédient visible sur l'assiette
        }

        return true;
    }

    public void SetRecipe(Recipe recipe)
    {
        currentRecipe = recipe;
    }

    public bool IsReady()
    {
        if (currentPlate == null || currentRecipe == null) return false;

        int requiredCount = currentRecipe.IsSoup() ? 3 : 4;
        
        // Vérifier qu'on a le bon nombre d'ingrédients
        if (ingredientsOnPlate.Count != requiredCount) return false;

        // Vérifier que tous les ingrédients appartiennent à la même recette
        int recipeId = currentRecipe.Order;
        foreach (Ingredient ingredient in ingredientsOnPlate)
        {
            if (ingredient.RecipeId != recipeId)
            {
                Debug.LogWarning($"Ingrédient avec recipeId {ingredient.RecipeId} trouvé dans recette {recipeId}!");
                return false;
            }
        }

        return true;
    }

    public GameObject TakePlate()
    {
        if (currentPlate == null) return null;

        // Désactiver tous les GameObjects des ingrédients avant de les retirer
        foreach (Ingredient ingredient in ingredientsOnPlate)
        {
            if (ingredient != null && ingredient.GameObject != null)
            {
                ingredient.GameObject.SetActive(false);
            }
        }

        GameObject plate = currentPlate;
        currentPlate = null;
        ingredientsOnPlate.Clear();
        currentRecipe = null;
        Release(CurrentAgent);
        
        return plate;
    }

    public bool HasPlate()
    {
        return currentPlate != null;
    }

    public int IngredientCount()
    {
        return ingredientsOnPlate.Count;
    }
}

