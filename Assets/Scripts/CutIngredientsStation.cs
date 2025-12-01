using UnityEngine;

public class CutIngredientsStation : Station
{
    private Ingredient currentIngredient; // Un seul ingrédient à la fois

    public bool AddIngredient(Ingredient ingredient)
    {
        if (ingredient == null) return false;

        lock (lockObject)
        {
            // Ne peut accepter qu'un seul ingrédient à la fois
            if (currentIngredient != null) return false;
            
            currentIngredient = ingredient;
            
            if (ingredient.GameObject != null)
            {
                ingredient.GameObject.transform.position = transform.position;
                ingredient.GameObject.SetActive(true);
            }

            IsOccupied = true;
            return true;
        }
    }

    public Ingredient TakeIngredient()
    {
        lock (lockObject)
        {
            if (currentIngredient == null)
            {
                IsOccupied = false;
                return null;
            }

            Ingredient ingredient = currentIngredient;
            currentIngredient = null;
            
            if (ingredient.GameObject != null)
            {
                ingredient.GameObject.SetActive(false);
            }
            
            IsOccupied = false;
            return ingredient;
        }
    }

    public Ingredient PeekIngredient()
    {
        lock (lockObject)
        {
            return currentIngredient;
        }
    }

    public Ingredient TakeIngredientOfType(IngredientType type)
    {
        lock (lockObject)
        {
            if (currentIngredient == null)
            {
                IsOccupied = false;
                return null;
            }

            if (currentIngredient.Type == type)
            {
                Ingredient found = currentIngredient;
                currentIngredient = null;
                
                if (found.GameObject != null)
                {
                    found.GameObject.SetActive(false);
                }
                
                IsOccupied = false;
                return found;
            }

            return null;
        }
    }

    public bool HasIngredient()
    {
        return currentIngredient != null;
    }

    public bool HasIngredientOfType(IngredientType type)
    {
        lock (lockObject)
        {
            return currentIngredient != null && currentIngredient.Type == type;
        }
    }

    public bool HasIngredientOfTypeForRecipe(IngredientType type, int recipeId)
    {
        lock (lockObject)
        {
            return currentIngredient != null && 
                   currentIngredient.Type == type && 
                   currentIngredient.RecipeId == recipeId;
        }
    }

    public Ingredient TakeIngredientOfTypeForRecipe(IngredientType type, int recipeId)
    {
        lock (lockObject)
        {
            if (currentIngredient == null)
            {
                IsOccupied = false;
                return null;
            }

            if (currentIngredient.Type == type && currentIngredient.RecipeId == recipeId)
            {
                Ingredient found = currentIngredient;
                currentIngredient = null;
                
                if (found.GameObject != null)
                {
                    found.GameObject.SetActive(false);
                }
                
                IsOccupied = false;
                return found;
            }

            return null;
        }
    }

    public int QueueCount()
    {
        return currentIngredient != null ? 1 : 0;
    }
}

