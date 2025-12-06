using UnityEngine;

public class ServeStation : Station
{
    private int recipesServed = 0;

    public bool ServePlate(GameObject plate)
    {
        if (plate == null) return false;

        // Détruire l'assiette (servie)
        Destroy(plate);
        
        recipesServed++;
        
        // Notifier le GameManager
        GameManager.Instance?.OnRecipeServed();
        
        return true;
    }

    public int GetRecipesServed()
    {
        return recipesServed;
    }
}

