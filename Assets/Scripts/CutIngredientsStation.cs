using System.Collections.Generic;
using UnityEngine;

public class CutIngredientsStation : Station
{
    private Queue<Ingredient> ingredientQueue = new Queue<Ingredient>();

    public bool AddIngredient(Ingredient ingredient)
    {
        if (ingredient == null) return false;

        lock (lockObject)
        {
            ingredientQueue.Enqueue(ingredient);
            
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
            if (ingredientQueue.Count == 0)
            {
                IsOccupied = false;
                return null;
            }

            Ingredient ingredient = ingredientQueue.Dequeue();
            
            if (ingredient.GameObject != null)
            {
                ingredient.GameObject.SetActive(false);
            }
            
            if (ingredientQueue.Count == 0)
            {
                IsOccupied = false;
            }

            return ingredient;
        }
    }

    public Ingredient PeekIngredient()
    {
        lock (lockObject)
        {
            if (ingredientQueue.Count == 0)
            {
                return null;
            }
            return ingredientQueue.Peek();
        }
    }

    public Ingredient TakeIngredientOfType(IngredientType type)
    {
        lock (lockObject)
        {
            if (ingredientQueue.Count == 0)
            {
                IsOccupied = false;
                return null;
            }

            // Chercher le premier ingrédient du type demandé
            Queue<Ingredient> tempQueue = new Queue<Ingredient>();
            Ingredient found = null;

            while (ingredientQueue.Count > 0)
            {
                Ingredient ing = ingredientQueue.Dequeue();
                if (found == null && ing.Type == type)
                {
                    found = ing;
                    if (ing.GameObject != null)
                    {
                        ing.GameObject.SetActive(false);
                    }
                }
                else
                {
                    tempQueue.Enqueue(ing);
                }
            }

            // Remettre les autres ingrédients dans la queue
            while (tempQueue.Count > 0)
            {
                ingredientQueue.Enqueue(tempQueue.Dequeue());
            }

            if (ingredientQueue.Count == 0)
            {
                IsOccupied = false;
            }

            return found;
        }
    }

    public bool HasIngredient()
    {
        return ingredientQueue.Count > 0;
    }

    public bool HasIngredientOfType(IngredientType type)
    {
        lock (lockObject)
        {
            foreach (Ingredient ing in ingredientQueue)
            {
                if (ing.Type == type)
                {
                    return true;
                }
            }
            return false;
        }
    }

    public bool HasIngredientOfTypeForRecipe(IngredientType type, int recipeId)
    {
        lock (lockObject)
        {
            foreach (Ingredient ing in ingredientQueue)
            {
                if (ing.Type == type && ing.RecipeId == recipeId)
                {
                    return true;
                }
            }
            return false;
        }
    }

    public Ingredient TakeIngredientOfTypeForRecipe(IngredientType type, int recipeId)
    {
        lock (lockObject)
        {
            if (ingredientQueue.Count == 0)
            {
                IsOccupied = false;
                return null;
            }

            // Chercher le premier ingrédient du type et recipeId demandés
            Queue<Ingredient> tempQueue = new Queue<Ingredient>();
            Ingredient found = null;

            while (ingredientQueue.Count > 0)
            {
                Ingredient ing = ingredientQueue.Dequeue();
                if (found == null && ing.Type == type && ing.RecipeId == recipeId)
                {
                    found = ing;
                    if (ing.GameObject != null)
                    {
                        ing.GameObject.SetActive(false);
                    }
                }
                else
                {
                    tempQueue.Enqueue(ing);
                }
            }

            // Remettre les autres ingrédients dans la queue
            while (tempQueue.Count > 0)
            {
                ingredientQueue.Enqueue(tempQueue.Dequeue());
            }

            if (ingredientQueue.Count == 0)
            {
                IsOccupied = false;
            }

            return found;
        }
    }

    public int QueueCount()
    {
        return ingredientQueue.Count;
    }
}

