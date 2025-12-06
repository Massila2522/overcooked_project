using UnityEngine;

public static class SpriteLoader
{
    public static Sprite LoadSprite(string path)
    {
        // Essayer Resources.Load d'abord
        Sprite sprite = Resources.Load<Sprite>(path);
        
        if (sprite != null)
        {
            return sprite;
        }

        // En éditeur, essayer AssetDatabase
        #if UNITY_EDITOR
        sprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/{path}.png");
        if (sprite != null)
        {
            return sprite;
        }
        #endif

        return null;
    }
}

