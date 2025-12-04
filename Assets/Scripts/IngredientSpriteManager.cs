using UnityEngine;
using System.Collections.Generic;

public class IngredientSpriteManager : MonoBehaviour
{
    public static IngredientSpriteManager Instance { get; private set; }

    [Header("Sprites d'ingrédients")]
    [Tooltip("Assignez les sprites pour chaque type d'ingrédient")]
    public IngredientSpriteData onionSprites;
    public IngredientSpriteData tomatoSprites;
    public IngredientSpriteData mushroomSprites;
    public IngredientSpriteData lettuceSprites;
    public IngredientSpriteData meatSprites;
    public IngredientSpriteData burgerBunSprites; // Pour le pain

    [Header("Sprites d'ustensiles")]
    public Sprite marmiteSprite;
    public Sprite panSprite;
    public Sprite plateSprite; // Assiette vide

    [Header("Sprites de plats finis")]
    public Sprite soupSprite; // Pour toutes les soupes
    public Sprite burgerSprite;

    private Dictionary<IngredientType, IngredientSpriteData> spriteDataMap;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeSpriteMap();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeSpriteMap()
    {
        spriteDataMap = new Dictionary<IngredientType, IngredientSpriteData>
        {
            { IngredientType.Onion, onionSprites },
            { IngredientType.Tomato, tomatoSprites },
            { IngredientType.Mushroom, mushroomSprites },
            { IngredientType.Lettuce, lettuceSprites },
            { IngredientType.Meat, meatSprites },
            { IngredientType.BurgerBun, burgerBunSprites }
        };
    }

    public Sprite GetIngredientSprite(IngredientType type, IngredientState state)
    {
        if (!spriteDataMap.ContainsKey(type))
        {
            Debug.LogWarning($"Pas de sprite data pour {type}");
            return null;
        }

        IngredientSpriteData data = spriteDataMap[type];
        if (data == null)
        {
            Debug.LogWarning($"Sprite data null pour {type}");
            return null;
        }

        switch (state)
        {
            case IngredientState.Raw:
                return data.rawSprite;
            case IngredientState.Cut:
                return data.cutSprite;
            case IngredientState.Chopped:
                return data.choppedSprite;
            case IngredientState.Cooked:
                return data.cookedSprite;
            default:
                return data.rawSprite;
        }
    }

    public Sprite GetUtensilSprite(string utensilName)
    {
        switch (utensilName.ToLower())
        {
            case "marmite":
                return marmiteSprite;
            case "pan":
            case "poêle":
            case "poele":
                return panSprite;
            case "plate":
            case "assiette":
                return plateSprite;
            default:
                Debug.LogWarning($"Utensil sprite non trouvé : {utensilName}");
                return null;
        }
    }

    public Sprite GetPlateSprite(RecipeType recipeType)
    {
        switch (recipeType)
        {
            case RecipeType.OnionSoup:
            case RecipeType.TomatoSoup:
            case RecipeType.MushroomSoup:
                return soupSprite;
            case RecipeType.Burger:
                return burgerSprite;
            default:
                return plateSprite;
        }
    }
}

