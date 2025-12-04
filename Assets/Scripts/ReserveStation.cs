using UnityEngine;
using System.Collections.Generic;

public class ReserveStation : Station
{
    [Header("Reserve Settings")]
    public IngredientType ingredientType;
    public int maxStock = 100; // Stock maximum (infini dans les specs)
    
    [Header("Sprite (optionnel - si non assigné, utilise IngredientSpriteManager)")]
    [Tooltip("Sprite pour l'ingrédient brut. Si vide, utilise IngredientSpriteManager")]
    public Sprite rawIngredientSprite;
    
    private Queue<Ingredient> stock = new Queue<Ingredient>();

    private void Start()
    {
        // Initialiser le stock avec des ingrédients bruts
        for (int i = 0; i < 10; i++) // Stock initial
        {
            CreateIngredient();
        }
    }

    private void CreateIngredient()
    {
        GameObject ingredientObj = new GameObject($"{ingredientType}_Raw_{stock.Count}");
        SpriteRenderer sr = ingredientObj.AddComponent<SpriteRenderer>();
        
        // Configurer le sortingOrder pour que les ingrédients soient visibles
        sr.sortingOrder = 2;
        
        // Charger le sprite depuis l'inspecteur ou le SpriteManager
        Sprite sprite = rawIngredientSprite;
        
        if (sprite == null && IngredientSpriteManager.Instance != null)
        {
            sprite = IngredientSpriteManager.Instance.GetIngredientSprite(ingredientType, IngredientState.Raw);
        }
        
        if (sprite != null)
        {
            sr.sprite = sprite;
        }
        else
        {
            Debug.LogWarning($"Sprite non trouvé pour {ingredientType} (Raw). Assignez-le dans l'inspecteur ou dans IngredientSpriteManager.");
        }

        ingredientObj.transform.position = transform.position;
        ingredientObj.SetActive(false); // Désactivé dans le stock

        Ingredient ingredient = new Ingredient(ingredientType, IngredientState.Raw, ingredientObj);
        stock.Enqueue(ingredient);
    }

    public Ingredient TakeIngredient()
    {
        lock (lockObject)
        {
            if (stock.Count == 0)
            {
                // Recréer un ingrédient si le stock est vide (stock infini)
                CreateIngredient();
            }

            Ingredient ingredient = stock.Dequeue();
            
            // Maintenir le stock (recréer pour avoir toujours des ingrédients disponibles)
            if (stock.Count < 5)
            {
                CreateIngredient();
            }

            if (ingredient.GameObject != null)
            {
                ingredient.GameObject.SetActive(true);
            }

            return ingredient;
        }
    }

    public bool HasIngredient()
    {
        return stock.Count > 0;
    }
}

