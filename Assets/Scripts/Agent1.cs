using UnityEngine;
using System.Collections;

public class Agent1 : Agent
{
    private ReserveStation[] reserves;
    private CuttingStation[] cuttingStations;
    private RecipeManager recipeManager;

    protected override void Start()
    {
        if (string.IsNullOrEmpty(agentLabel))
        {
            agentLabel = "Agent 1";
        }

        base.Start();

        reserves = FindObjectsByType<ReserveStation>(FindObjectsSortMode.None);
        cuttingStations = FindObjectsByType<CuttingStation>(FindObjectsSortMode.None);
        recipeManager = FindFirstObjectByType<RecipeManager>();

        StartCoroutine(WorkLoop());
    }

    private IEnumerator WorkLoop()
    {
        while (true)
        {
            yield return StartCoroutine(GetNextIngredientTask());
            yield return new WaitForSeconds(0.1f);
        }
    }

    private IEnumerator GetNextIngredientTask()
    {
        // Récupérer le prochain ingrédient nécessaire depuis RecipeManager
        IngredientQueueItem item = recipeManager.GetNextNeededIngredient();

        if (item == null)
        {
            yield return new WaitForSeconds(0.5f);
            yield break;
        }

        // Trouver la réserve correspondante
        ReserveStation reserve = FindReserve(item.Type);
        if (reserve == null)
        {
            yield return new WaitForSeconds(0.5f);
            yield break;
        }

        // Aller à la réserve
        MoveTo(reserve.transform);
        yield return new WaitUntil(() => !isMoving);

        // Prendre l'ingrédient
        Ingredient ingredient = reserve.TakeIngredient();
        if (ingredient == null)
        {
            yield return new WaitForSeconds(0.5f);
            yield break;
        }

        // Assigner le recipeId à l'ingrédient
        ingredient.RecipeId = item.RecipeId;

        PickUpIngredient(ingredient);

        // Trouver une place de découpage libre
        CuttingStation freeStation = FindFreeCuttingStation();
        if (freeStation == null)
        {
            // Attendre qu'une place se libère
            yield return new WaitUntil(() => FindFreeCuttingStation() != null);
            freeStation = FindFreeCuttingStation();
        }

        // Aller à la place de découpage
        MoveTo(freeStation.transform);
        yield return new WaitUntil(() => !isMoving);

        // Si c'est du pain (BurgerBun), il n'a pas besoin d'être découpé
        // Le placer directement dans CutIngredientsStation
        if (ingredient.Type == IngredientType.BurgerBun)
        {
            // Trouver une station d'ingrédients découpés
            CutIngredientsStation[] cutStations = FindObjectsByType<CutIngredientsStation>(FindObjectsSortMode.None);
            CutIngredientsStation freeCutStation = null;

            foreach (CutIngredientsStation station in cutStations)
            {
                if (station.IsAvailable() || station.QueueCount() < 2)
                {
                    freeCutStation = station;
                    break;
                }
            }

            if (freeCutStation != null)
            {
                // Simuler un ingrédient prêt (pas de découpe nécessaire)
                ingredient.ChangeState(IngredientState.Cut);

                MoveTo(freeCutStation.transform);
                yield return new WaitUntil(() => !isMoving);

                if (freeCutStation.AddIngredient(ingredient))
                {
                    DropIngredient();
                }
            }
        }
        else
        {
            // Poser l'ingrédient sur la station de découpage
            if (freeStation.PlaceIngredient(ingredient))
            {
                DropIngredient();
            }
        }

        yield return new WaitForSeconds(0.1f);
    }

    private ReserveStation FindReserve(IngredientType type)
    {
        foreach (ReserveStation reserve in reserves)
        {
            if (reserve.ingredientType == type)
            {
                return reserve;
            }
        }
        return null;
    }

    private CuttingStation FindFreeCuttingStation()
    {
        foreach (CuttingStation station in cuttingStations)
        {
            if (!station.HasIngredient() && station.IsAvailable())
            {
                return station;
            }
        }
        return null;
    }
}

