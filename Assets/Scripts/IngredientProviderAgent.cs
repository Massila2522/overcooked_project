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

        // Vérifier si l'ingrédient a besoin d'être découpé
        bool needsCutting = ingredient.NeedsCutting();

        if (!needsCutting)
        {
            // L'ingrédient n'a pas besoin d'être découpé, le mettre directement dans l'assiette
            // Trouver l'assiette correspondante à cette recette
            PlateStation plateStation = PlateStation.FindPlateStationForRecipe(item.RecipeId);
            
            if (plateStation != null)
            {
                // Aller à la station d'assiette
                MoveTo(plateStation.transform);
                yield return new WaitUntil(() => !isMoving);

                // Ajouter l'ingrédient directement dans l'assiette
                if (plateStation.AddIngredient(ingredient))
                {
                    DropIngredient();
                }
            }
            else
            {
                // Pas d'assiette trouvée, attendre un peu
                DropIngredient();
                yield return new WaitForSeconds(0.5f);
            }
        }
        else
        {
            // L'ingrédient doit être découpé, le poser sur une station de découpage
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

