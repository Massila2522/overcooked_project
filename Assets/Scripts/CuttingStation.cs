using UnityEngine;

public class CuttingStation : Station
{
    [Header("Cutting Settings")]
    public float cuttingRadius = 1.5f; // Distance pour déclencher le découpage
    public float cuttingTime = 2f; // Temps de découpage en secondes

    private Ingredient currentIngredient;
    private bool isCutting = false;
    private float cuttingTimer = 0f;

    private void Update()
    {
        if (isCutting && currentIngredient != null)
        {
            cuttingTimer += Time.deltaTime;
            
            if (cuttingTimer >= cuttingTime)
            {
                CompleteCutting();
            }
        }
    }

    public bool HasIngredient()
    {
        return currentIngredient != null;
    }

    public bool PlaceIngredient(Ingredient ingredient)
    {
        if (currentIngredient != null) return false;

        currentIngredient = ingredient;
        if (ingredient.GameObject != null)
        {
            ingredient.GameObject.transform.position = transform.position;
            ingredient.GameObject.SetActive(true);
        }
        return true;
    }

    public bool TryStartCutting(Agent agent)
    {
        if (currentIngredient == null || isCutting) return false;
        if (!TryReserve(agent)) return false;

        // Vérifier la distance
        float distance = Vector2.Distance(agent.transform.position, transform.position);
        if (distance > cuttingRadius) return false;

        isCutting = true;
        cuttingTimer = 0f;
        return true;
    }

    private void CompleteCutting()
    {
        if (currentIngredient == null) return;

        // Changer l'état de l'ingrédient
        if (currentIngredient.Type == IngredientType.Meat)
        {
            currentIngredient.ChangeState(IngredientState.Chopped);
        }
        else
        {
            currentIngredient.ChangeState(IngredientState.Cut);
        }

        isCutting = false;
    }

    public Ingredient TakeIngredient()
    {
        if (currentIngredient == null || isCutting) return null;

        Ingredient ingredient = currentIngredient;
        currentIngredient = null;
        
        // Désactiver le GameObject de l'ingrédient pour nettoyer la station visuellement
        if (ingredient.GameObject != null)
        {
            ingredient.GameObject.SetActive(false);
        }
        
        Release(CurrentAgent);
        return ingredient;
    }

    public bool IsCutting()
    {
        return isCutting;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, cuttingRadius);
    }
}

