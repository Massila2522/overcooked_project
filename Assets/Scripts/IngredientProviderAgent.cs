using UnityEngine;
using System.Collections;

public class IngredientProviderAgent : Agent
{
    private ReserveStation[] reserves;
    private CuttingStation[] cuttingStations;
    private RecipeManager recipeManager;

    protected override void Start()
    {
        // Si agentLabel n'est pas défini dans l'inspecteur, utiliser une valeur par défaut
        if (string.IsNullOrEmpty(agentLabel))
        {
            agentLabel = "Ingredient Provider";
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

        bool requiresProcessing = ingredient.NeedsCutting() || ingredient.NeedsCooking();
        if (!requiresProcessing)
        {
            yield return StartCoroutine(DeliverIngredientDirectly(ingredient, item.RecipeId));
        }
        else
        {
            yield return StartCoroutine(PlaceIngredientOnCuttingStation(ingredient));
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

    private IEnumerator PlaceIngredientOnCuttingStation(Ingredient ingredient)
    {
        if (ingredient == null)
        {
            yield break;
        }

        bool placed = false;
        while (!placed)
        {
            CuttingStation targetStation = FindFreeCuttingStation();
            while (targetStation == null)
            {
                currentState = AgentState.Waiting;
                yield return new WaitForSeconds(0.1f);
                targetStation = FindFreeCuttingStation();
            }

            MoveTo(targetStation.transform);
            yield return new WaitUntil(() => !isMoving);

            placed = targetStation.PlaceIngredient(ingredient);
            if (!placed)
            {
                yield return new WaitForSeconds(0.1f);
            }
        }

        DropIngredient();
    }

    private IEnumerator DeliverIngredientDirectly(Ingredient ingredient, int recipeId)
    {
        if (ingredient == null)
        {
            yield break;
        }

        bool delivered = false;
        while (!delivered)
        {
            PlateStation plateStation = PlateStation.FindPlateStationForRecipe(recipeId);
            if (plateStation == null)
            {
                currentState = AgentState.Waiting;
                yield return new WaitForSeconds(0.1f);
                continue;
            }

            MoveTo(plateStation.transform);
            yield return new WaitUntil(() => !isMoving);

            delivered = plateStation.AddIngredient(ingredient);
            if (!delivered)
            {
                yield return new WaitForSeconds(0.1f);
            }
        }

        DropIngredient();
    }
}

