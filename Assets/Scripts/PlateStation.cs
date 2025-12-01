using UnityEngine;
using System.Collections.Generic;

public class PlateStation : Station
{
    private static readonly List<PlateStation> allStations = new List<PlateStation>();

    [Header("Réglages d'affichage")]
    [Tooltip("Point d'ancrage où poser l'assiette (facultatif, utilise la position du GameObject sinon).")]
    [SerializeField] private Transform plateAnchor;
    [SerializeField] private float ingredientStackOffset = 0.05f;

    private GameObject currentPlate;
    private Recipe currentRecipe;
    private int currentRecipeId = -1;
    private readonly List<Ingredient> ingredientsOnPlate = new List<Ingredient>();
    private bool isReady = false;

    private void OnEnable()
    {
        if (!allStations.Contains(this))
        {
            allStations.Add(this);
        }
    }

    private void OnDisable()
    {
        allStations.Remove(this);
    }

    public override bool IsAvailable()
    {
        // La station est disponible uniquement si aucune assiette n'est en place
        return currentPlate == null && !IsOccupied;
    }

    public bool HasPlate()
    {
        return currentPlate != null;
    }

    public bool PlacePlate(GameObject plate, Agent agent, int recipeId)
    {
        if (plate == null || agent == null)
        {
            return false;
        }

        lock (lockObject)
        {
            if (currentPlate != null || IsOccupied)
            {
                return false;
            }

            IsOccupied = true;
            CurrentAgent = agent;

            currentPlate = plate;
            currentRecipeId = recipeId;
            currentRecipe = null;
            isReady = false;
            ingredientsOnPlate.Clear();

            Transform anchor = plateAnchor != null ? plateAnchor : transform;
            currentPlate.transform.SetParent(null);
            currentPlate.transform.position = anchor.position;
            currentPlate.SetActive(true);

            return true;
        }
    }

    public void SetRecipe(Recipe recipe)
    {
        lock (lockObject)
        {
            currentRecipe = recipe;
            if (recipe != null)
            {
                currentRecipeId = recipe.Order;
            }
        }
    }

    public bool AddIngredient(Ingredient ingredient)
    {
        if (ingredient == null)
        {
            return false;
        }

        lock (lockObject)
        {
            if (currentPlate == null)
            {
                return false;
            }

            // Vérifier la correspondance de recette
            if (currentRecipeId >= 0 && ingredient.RecipeId >= 0 && ingredient.RecipeId != currentRecipeId)
            {
                return false;
            }

            if (ingredient.RecipeId < 0 && currentRecipeId >= 0)
            {
                ingredient.RecipeId = currentRecipeId;
            }

            if (!IsIngredientNeeded(ingredient))
            {
                return false;
            }

            ingredientsOnPlate.Add(ingredient);

            if (ingredient.GameObject != null)
            {
                Transform anchor = plateAnchor != null ? plateAnchor : transform;
                Vector3 basePos = anchor.position;
                float heightOffset = ingredientStackOffset * ingredientsOnPlate.Count;
                ingredient.GameObject.transform.SetParent(null);
                ingredient.GameObject.transform.position = basePos + Vector3.up * heightOffset;
                ingredient.GameObject.SetActive(true);
            }

            UpdateReadyState();
            return true;
        }
    }

    private bool IsIngredientNeeded(Ingredient ingredient)
    {
        if (currentRecipe == null)
        {
            return true;
        }

        int required = 0;
        int alreadyPlaced = 0;

        foreach (IngredientType type in currentRecipe.RequiredIngredients)
        {
            if (type == ingredient.Type)
            {
                required++;
            }
        }

        foreach (Ingredient ing in ingredientsOnPlate)
        {
            if (ing.Type == ingredient.Type)
            {
                alreadyPlaced++;
            }
        }

        if (required == 0)
        {
            return false;
        }

        return alreadyPlaced < required;
    }

    private void UpdateReadyState()
    {
        if (currentRecipe == null)
        {
            isReady = false;
            return;
        }

        isReady = ingredientsOnPlate.Count >= currentRecipe.RequiredIngredients.Count;
    }

    public bool IsReady()
    {
        return isReady;
    }

    public GameObject TakePlate()
    {
        lock (lockObject)
        {
            if (currentPlate == null || !isReady)
            {
                return null;
            }

            // Attacher les ingrédients à l'assiette pour qu'ils suivent l'agent
            for (int i = 0; i < ingredientsOnPlate.Count; i++)
            {
                Ingredient ingredient = ingredientsOnPlate[i];
                if (ingredient?.GameObject == null)
                {
                    continue;
                }

                ingredient.GameObject.transform.SetParent(currentPlate.transform);
                ingredient.GameObject.transform.localPosition = Vector3.up * ingredientStackOffset * (i + 1);
                ingredient.GameObject.SetActive(true);
            }

            GameObject plate = currentPlate;

            currentPlate = null;
            currentRecipe = null;
            currentRecipeId = -1;
            ingredientsOnPlate.Clear();
            isReady = false;
            IsOccupied = false;
            CurrentAgent = null;

            return plate;
        }
    }

    public static PlateStation FindPlateStationForRecipe(int recipeId)
    {
        if (recipeId < 0)
        {
            return null;
        }

        for (int i = 0; i < allStations.Count; i++)
        {
            PlateStation station = allStations[i];
            if (station != null && station.currentPlate != null && station.currentRecipeId == recipeId)
            {
                return station;
            }
        }

        return null;
    }
}

