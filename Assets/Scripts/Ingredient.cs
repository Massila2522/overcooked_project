using UnityEngine;

public class Ingredient
{
    public IngredientType Type { get; private set; }
    public IngredientState State { get; private set; }
    public GameObject GameObject { get; private set; }
    public SpriteRenderer SpriteRenderer { get; private set; }
    public int RecipeId { get; set; } = -1; // ID de la recette à laquelle cet ingrédient appartient (-1 = non assigné)

    public Ingredient(IngredientType type, IngredientState state, GameObject gameObject, int recipeId = -1)
    {
        Type = type;
        State = state;
        GameObject = gameObject;
        RecipeId = recipeId;
        
        if (gameObject != null)
        {
            // Ne récupérer le SpriteRenderer que s'il existe déjà (GameObject instancié)
            // Ne pas modifier les GameObjects existants dans la scène
            SpriteRenderer = gameObject.GetComponent<SpriteRenderer>();
            
            // S'assurer que le SpriteRenderer a le bon sortingOrder
            // Seulement si c'est un GameObject instancié dynamiquement
            if (SpriteRenderer != null)
            {
                SpriteRenderer.sortingOrder = 2;
                // Initialiser le sprite selon l'état (seulement pour les GameObjects instanciés)
                UpdateSprite();
            }
        }
    }

    public void ChangeState(IngredientState newState)
    {
        State = newState;
        UpdateSprite();
    }

    private void UpdateSprite()
    {
        if (SpriteRenderer == null || GameObject == null) return;

        // Utiliser IngredientSpriteManager pour obtenir le sprite
        Sprite sprite = null;
        if (IngredientSpriteManager.Instance != null)
        {
            sprite = IngredientSpriteManager.Instance.GetIngredientSprite(Type, State);
        }
        
        if (sprite != null)
        {
            SpriteRenderer.sprite = sprite;
            // S'assurer que le sortingOrder est correct
            SpriteRenderer.sortingOrder = 2;
        }
        else
        {
            Debug.LogWarning($"Sprite non trouvé pour {Type} ({State}). Vérifiez IngredientSpriteManager.");
        }
    }

    public bool NeedsCutting()
    {
        return State == IngredientState.Raw && 
               (Type == IngredientType.Onion || 
                Type == IngredientType.Tomato || 
                Type == IngredientType.Mushroom || 
                Type == IngredientType.Lettuce || 
                Type == IngredientType.Meat);
    }

    public bool NeedsCooking()
    {
        return Type == IngredientType.Meat && 
               (State == IngredientState.Chopped);
    }
}

