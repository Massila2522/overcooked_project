using UnityEngine;

[CreateAssetMenu(fileName = "IngredientSpriteData", menuName = "Cuisine/Ingredient Sprite Data")]
public class IngredientSpriteData : ScriptableObject
{
    [Header("Sprites pour cet ingrédient")]
    public Sprite rawSprite;
    public Sprite cutSprite;
    public Sprite choppedSprite; // Pour la viande uniquement
    public Sprite cookedSprite;
}

