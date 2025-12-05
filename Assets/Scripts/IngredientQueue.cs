using System.Collections.Generic;
using UnityEngine;

#nullable enable
public class IngredientQueueItem
{
    public IngredientType Type { get; set; }
    public int RecipeId { get; set; }

    public IngredientQueueItem(IngredientType type, int recipeId)
    {
        Type = type;
        RecipeId = recipeId;
    }
}

public class IngredientQueue
{
    private Queue<IngredientQueueItem> ingredientQueue = new Queue<IngredientQueueItem>();
    private object lockObject = new object();

    public void AddIngredient(IngredientType type, int recipeId)
    {
        lock (lockObject)
        {
            ingredientQueue.Enqueue(new IngredientQueueItem(type, recipeId));
        }
    }

    public IngredientQueueItem? TakeIngredient()
    {
        lock (lockObject)
        {
            if (ingredientQueue.Count == 0)
            {
                return null;
            }
            return ingredientQueue.Dequeue();
        }
    }

    public bool HasIngredient()
    {
        return ingredientQueue.Count > 0;
    }

    public int Count()
    {
        return ingredientQueue.Count;
    }

    public void Clear()
    {
        lock (lockObject)
        {
            ingredientQueue.Clear();
        }
    }
}
#nullable disable

