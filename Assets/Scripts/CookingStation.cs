using UnityEngine;
using System.Collections.Generic;

public class CookingStation : Station
{
    [Header("Cooking Settings")]
    public float cookingTime = 5f; // Temps de cuisson en secondes
    
    private GameObject currentUtensil; // Marmite ou Poêle
    private Recipe currentRecipe;
    private List<Ingredient> ingredientsInPot = new List<Ingredient>();
    private bool isCooking = false;
    private float cookingTimer = 0f;
    private bool isSoup; // true = soupe (marmite), false = hamburger (poêle)

    private void Update()
    {
        if (isCooking)
        {
            cookingTimer += Time.deltaTime;
            
            if (cookingTimer >= cookingTime)
            {
                CompleteCooking();
            }
        }
    }

    public bool PlaceUtensil(GameObject utensil, bool soup, Agent agent)
    {
        if (utensil == null || agent == null) return false;
        if (currentUtensil != null) return false;
        if (!TryReserve(agent)) return false;

        currentUtensil = utensil;
        isSoup = soup;
        
        utensil.transform.position = transform.position;
        utensil.SetActive(true);

        return true;
    }

    public bool AddIngredient(Ingredient ingredient)
    {
        if (currentUtensil == null) return false;
        if (ingredientsInPot.Count >= 3 && isSoup) return false; // Soupe = max 3 ingrédients
        if (ingredientsInPot.Count >= 4 && !isSoup) return false; // Hamburger = max 4 ingrédients

        ingredientsInPot.Add(ingredient);
        
        if (ingredient.GameObject != null && currentUtensil != null)
        {
            // Positionner l'ingrédient sur la marmite/poêle (légèrement au-dessus)
            ingredient.GameObject.transform.position = currentUtensil.transform.position + Vector3.up * 0.2f;
            ingredient.GameObject.SetActive(true); // Garder l'ingrédient visible
        }

        return true;
    }

    public bool StartCooking(Recipe recipe)
    {
        if (currentUtensil == null) return false;
        if (isCooking) return false;

        // Pour soupe : vérifier qu'on a 3 ingrédients
        if (recipe.IsSoup() && ingredientsInPot.Count != 3) return false;
        
        // Pour hamburger : on peut cuire juste la viande (1 ingrédient)
        if (!recipe.IsSoup() && ingredientsInPot.Count == 0) return false;

        currentRecipe = recipe;
        isCooking = true;
        cookingTimer = 0f;
        return true;
    }

    private void CompleteCooking()
    {
        isCooking = false;
        
        // Changer l'état des ingrédients cuits
        foreach (Ingredient ingredient in ingredientsInPot)
        {
            if (ingredient.Type == IngredientType.Meat && ingredient.State == IngredientState.Chopped)
            {
                ingredient.ChangeState(IngredientState.Cooked);
            }
        }
        
        // La cuisson est terminée, prêt à verser/assembler
    }

    public bool IsReady()
    {
        if (isCooking || ingredientsInPot.Count == 0) return false;
        
        // Pour soupe : 3 ingrédients
        if (isSoup && ingredientsInPot.Count == 3) return true;
        
        // Pour viande : 1 ingrédient (viande) qui doit être cuite
        if (!isSoup && ingredientsInPot.Count == 1)
        {
            Ingredient meat = ingredientsInPot[0];
            if (meat.Type == IngredientType.Meat && meat.State == IngredientState.Cooked)
            {
                return true;
            }
        }
        
        return false;
    }

    public List<Ingredient> GetCookedIngredients()
    {
        return new List<Ingredient>(ingredientsInPot);
    }

    public void ClearStation()
    {
        // Désactiver tous les GameObjects des ingrédients avant de les retirer
        foreach (Ingredient ingredient in ingredientsInPot)
        {
            if (ingredient != null && ingredient.GameObject != null)
            {
                ingredient.GameObject.SetActive(false);
            }
        }
        
        ingredientsInPot.Clear();
        
        if (currentUtensil != null)
        {
            currentUtensil.SetActive(false);
            currentUtensil = null;
        }

        currentRecipe = null;
        isCooking = false;
        cookingTimer = 0f;
        Release(CurrentAgent);
    }

    public bool HasUtensil()
    {
        return currentUtensil != null;
    }

    public int IngredientCount()
    {
        return ingredientsInPot.Count;
    }
}

