using UnityEngine;
using System;
using System.Collections;

public class Agent5 : Agent
{
    private RecipeManager recipeManager;
    private CookingStation[] cookingStations;
    private CutIngredientsStation[] cutIngredientsStations;
    private PlateStation[] plateStations;
    private ServeStation[] serveStations;
    private GameObject[] vaisselleStations;
    private GameObject marmitePrefab;
    private GameObject panPrefab;

    private Recipe currentRecipe;

    protected override void Start()
    {
        if (string.IsNullOrEmpty(agentLabel))
        {
            agentLabel = "Agent 5";
        }

        base.Start();

        recipeManager = FindFirstObjectByType<RecipeManager>();
        cookingStations = FindObjectsByType<CookingStation>(FindObjectsSortMode.None);
        cutIngredientsStations = FindObjectsByType<CutIngredientsStation>(FindObjectsSortMode.None);
        plateStations = FindObjectsByType<PlateStation>(FindObjectsSortMode.None);
        serveStations = FindObjectsByType<ServeStation>(FindObjectsSortMode.None);
        
        GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        System.Collections.Generic.List<GameObject> vaisselleList = new System.Collections.Generic.List<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj.name.Contains("Vaisselle"))
            {
                vaisselleList.Add(obj);
            }
        }
        vaisselleStations = vaisselleList.ToArray();

        LoadPrefabs();
        
        StartCoroutine(WorkLoop());
    }

    [Header("Sprites (optionnel - si non assigné, utilise IngredientSpriteManager)")]
    public Sprite marmiteSpriteOverride;
    public Sprite panSpriteOverride;
    public Sprite plateSpriteOverride;
    public Sprite soupSpriteOverride;
    public Sprite burgerSpriteOverride;

    private void LoadPrefabs()
    {
        // Charger marmite et poêle depuis IngredientSpriteManager ou override
        Sprite marmiteSprite = marmiteSpriteOverride;
        Sprite panSprite = panSpriteOverride;
        
        if (marmiteSprite == null && IngredientSpriteManager.Instance != null)
        {
            marmiteSprite = IngredientSpriteManager.Instance.GetUtensilSprite("marmite");
        }
        
        if (panSprite == null && IngredientSpriteManager.Instance != null)
        {
            panSprite = IngredientSpriteManager.Instance.GetUtensilSprite("pan");
        }
        
        if (marmiteSprite != null)
        {
            GameObject marmiteObj = new GameObject("Marmite");
            SpriteRenderer sr = marmiteObj.AddComponent<SpriteRenderer>();
            sr.sprite = marmiteSprite;
            sr.sortingOrder = 2;
            marmiteObj.SetActive(false);
            marmitePrefab = marmiteObj;
        }
        else
        {
            Debug.LogWarning("Sprite de marmite non trouvé. Assignez-le dans l'inspecteur ou dans IngredientSpriteManager.");
        }
        
        if (panSprite != null)
        {
            GameObject panObj = new GameObject("Poêle");
            SpriteRenderer sr = panObj.AddComponent<SpriteRenderer>();
            sr.sprite = panSprite;
            sr.sortingOrder = 2;
            panObj.SetActive(false);
            panPrefab = panObj;
        }
        else
        {
            Debug.LogWarning("Sprite de poêle non trouvé. Assignez-le dans l'inspecteur ou dans IngredientSpriteManager.");
        }
    }

    private IEnumerator WorkLoop()
    {
        while (true)
        {
            yield return StartCoroutine(ProcessRecipe());
            yield return new WaitForSeconds(0.1f);
        }
    }

    private IEnumerator ProcessRecipe()
    {
        // Récupérer la prochaine recette disponible (non réservée)
        currentRecipe = recipeManager.GetNextRecipe();
        if (currentRecipe == null)
        {
            yield return new WaitForSeconds(0.5f);
            yield break;
        }

        // Essayer de réserver la recette
        if (!recipeManager.TryReserveRecipe(currentRecipe, this))
        {
            // La recette a été réservée par un autre agent entre-temps
            currentRecipe = null;
            yield return new WaitForSeconds(0.5f);
            yield break;
        }

        // Vérifier si les ingrédients sont disponibles avant de commencer
        if (!AreIngredientsReady(currentRecipe))
        {
            // Libérer la réservation si les ingrédients ne sont pas prêts
            currentRecipe.IsReserved = false;
            currentRecipe.ReservedBy = null;
            currentRecipe = null;
            yield return new WaitForSeconds(0.5f);
            yield break;
        }

        if (currentRecipe.IsSoup())
        {
            yield return StartCoroutine(CookSoup(currentRecipe));
        }
        else
        {
            yield return StartCoroutine(CookBurger(currentRecipe));
        }

        // Marquer la recette comme complétée et la retirer de la file
        recipeManager.CompleteRecipe(currentRecipe);
        currentRecipe = null;
    }

    private bool AreIngredientsReady(Recipe recipe)
    {
        // Vérifier si tous les ingrédients nécessaires sont disponibles dans les stations pour cette recette
        foreach (IngredientType neededType in recipe.RequiredIngredients)
        {
            bool found = false;
            foreach (CutIngredientsStation station in cutIngredientsStations)
            {
                if (station.HasIngredientOfTypeForRecipe(neededType, recipe.Order))
                {
                    found = true;
                    break;
                }
            }
            if (!found && neededType != IngredientType.BurgerBun) // Le pain vient de la réserve
            {
                return false;
            }
        }
        return true;
    }

    private IEnumerator CookSoup(Recipe recipe)
    {
        if (marmitePrefab == null)
        {
            Debug.LogError("Marmite prefab introuvable pour Agent5.");
            yield break;
        }

        CookingStation cookingStation = FindFreeCookingStation();
        while (cookingStation == null)
        {
            yield return null;
            cookingStation = FindFreeCookingStation();
        }

        GameObject marmiteInstance = Instantiate(marmitePrefab);
        while (!cookingStation.PlaceUtensil(marmiteInstance, true, this))
        {
            Destroy(marmiteInstance);
            cookingStation = null;
            yield return null;
            while (cookingStation == null)
            {
                yield return null;
                cookingStation = FindFreeCookingStation();
            }
            marmiteInstance = Instantiate(marmitePrefab);
        }

        for (int i = 0; i < 3; i++)
        {
            IngredientType neededType = recipe.RequiredIngredients[i];
            Ingredient ingredient = GetCutIngredient(neededType, recipe.Order);

            while (ingredient == null)
            {
                yield return null;
                ingredient = GetCutIngredient(neededType, recipe.Order);
            }

            cookingStation.AddIngredient(ingredient);
        }

        MoveTo(cookingStation.transform);
        yield return new WaitUntil(() => !isMoving);

        cookingStation.StartCooking(recipe);
        yield return new WaitUntil(() => cookingStation.IsReady());

        GameObject plate = GetPlate();
        while (plate == null)
        {
            yield return null;
            plate = GetPlate();
        }

        PickUpPlate(plate);

        PlateStation plateStation = FindFreePlateStation();
        while (plateStation == null)
        {
            yield return null;
            plateStation = FindFreePlateStation();
        }

        MoveTo(plateStation.transform);
        yield return new WaitUntil(() => !isMoving);

        while (!plateStation.PlacePlate(plate, this))
        {
            yield return null;
            plateStation = null;
            while (plateStation == null)
            {
                yield return null;
                plateStation = FindFreePlateStation();
            }
        }

        plateStation.SetRecipe(recipe);
        DropPlate();

        var cookedIngredients = cookingStation.GetCookedIngredients();
        foreach (var ing in cookedIngredients)
        {
            plateStation.AddIngredient(ing);
        }

        cookingStation.ClearStation();

        yield return new WaitUntil(() => plateStation.IsReady());

        GameObject finishedPlate = plateStation.TakePlate();
        PickUpPlate(finishedPlate);

        Sprite soupSprite = soupSpriteOverride;
        if (soupSprite == null && IngredientSpriteManager.Instance != null)
        {
            soupSprite = IngredientSpriteManager.Instance.GetPlateSprite(recipe.Type);
        }
        
        if (soupSprite != null && finishedPlate != null)
        {
            SpriteRenderer sr = finishedPlate.GetComponent<SpriteRenderer>();
            if (sr != null) sr.sprite = soupSprite;
        }

        ServeStation serveStation = FindFreeServeStation();
        MoveTo(serveStation.transform);
        yield return new WaitUntil(() => !isMoving);

        serveStation?.ServePlate(finishedPlate);
        DropPlate();
    }

    private IEnumerator CookBurger(Recipe recipe)
    {
        GameObject plate = GetPlate();
        while (plate == null)
        {
            yield return null;
            plate = GetPlate();
        }

        PickUpPlate(plate);

        PlateStation plateStation = FindFreePlateStation();
        while (plateStation == null)
        {
            yield return null;
            plateStation = FindFreePlateStation();
        }

        MoveTo(plateStation.transform);
        yield return new WaitUntil(() => !isMoving);

        while (!plateStation.PlacePlate(plate, this))
        {
            yield return null;
            plateStation = null;
            while (plateStation == null)
            {
                yield return null;
                plateStation = FindFreePlateStation();
            }
        }

        plateStation.SetRecipe(recipe);
        DropPlate();

        foreach (IngredientType neededType in recipe.RequiredIngredients)
        {
            if (neededType == IngredientType.Meat)
            {
                Ingredient cookedMeat = null;
                yield return StartCoroutine(CookMeat(recipe, ing => cookedMeat = ing));
                if (cookedMeat != null)
                {
                    plateStation.AddIngredient(cookedMeat);
                }
            }
            else
            {
                Ingredient ingredient = GetCutIngredient(neededType, recipe.Order);
                while (ingredient == null)
                {
                    yield return null;
                    ingredient = GetCutIngredient(neededType, recipe.Order);
                }
                plateStation.AddIngredient(ingredient);
            }
        }

        yield return new WaitUntil(() => plateStation.IsReady());

        GameObject finishedPlate = plateStation.TakePlate();
        PickUpPlate(finishedPlate);

        Sprite burgerSprite = burgerSpriteOverride;
        if (burgerSprite == null && IngredientSpriteManager.Instance != null)
        {
            burgerSprite = IngredientSpriteManager.Instance.GetPlateSprite(RecipeType.Burger);
        }
        
        if (burgerSprite != null && finishedPlate != null)
        {
            SpriteRenderer sr = finishedPlate.GetComponent<SpriteRenderer>();
            if (sr != null) sr.sprite = burgerSprite;
        }

        ServeStation serveStation = FindFreeServeStation();
        MoveTo(serveStation.transform);
        yield return new WaitUntil(() => !isMoving);

        serveStation?.ServePlate(finishedPlate);
        DropPlate();
    }

    private IEnumerator CookMeat(Recipe recipe, Action<Ingredient> onCooked)
    {
        if (panPrefab == null)
        {
            Debug.LogError("Poêle prefab introuvable pour Agent5.");
            yield break;
        }

        Ingredient meat = GetCutIngredient(IngredientType.Meat, recipe.Order);
        while (meat == null || meat.State != IngredientState.Chopped)
        {
            yield return null;
            meat = GetCutIngredient(IngredientType.Meat, recipe.Order);
        }

        CookingStation station = FindFreeCookingStation();
        while (station == null)
        {
            yield return null;
            station = FindFreeCookingStation();
        }

        GameObject panInstance = Instantiate(panPrefab);
        while (!station.PlaceUtensil(panInstance, false, this))
        {
            Destroy(panInstance);
            station = null;
            yield return null;
            while (station == null)
            {
                yield return null;
                station = FindFreeCookingStation();
            }
            panInstance = Instantiate(panPrefab);
        }

        station.AddIngredient(meat);

        MoveTo(station.transform);
        yield return new WaitUntil(() => !isMoving);

        Recipe meatRecipe = new Recipe(RecipeType.Burger, recipe.Order);
        station.StartCooking(meatRecipe);
        yield return new WaitUntil(() => station.IsReady());

        Ingredient cookedMeat = null;
        foreach (var ing in station.GetCookedIngredients())
        {
            if (ing.Type == IngredientType.Meat && ing.State == IngredientState.Cooked)
            {
                cookedMeat = ing;
                break;
            }
        }

        station.ClearStation();
        onCooked?.Invoke(cookedMeat);
    }

    private Ingredient GetCutIngredient(IngredientType type, int recipeId)
    {
        foreach (CutIngredientsStation station in cutIngredientsStations)
        {
            if (station.HasIngredientOfTypeForRecipe(type, recipeId))
            {
                Ingredient ingredient = station.TakeIngredientOfTypeForRecipe(type, recipeId);
                if (ingredient == null)
                {
                    continue;
                }

                bool isReady = ingredient.State == IngredientState.Cut || ingredient.State == IngredientState.Chopped;

                if (type == IngredientType.BurgerBun)
                {
                    isReady = true;
                }

                if (isReady)
                {
                    return ingredient;
                }
            }
        }
        return null;
    }

    private GameObject GetPlate()
    {
        // Créer une assiette depuis le sprite (override ou SpriteManager)
        Sprite plateSprite = plateSpriteOverride;
        
        if (plateSprite == null && IngredientSpriteManager.Instance != null)
        {
            plateSprite = IngredientSpriteManager.Instance.GetUtensilSprite("assiette");
        }
        
        if (plateSprite != null)
        {
            GameObject plate = new GameObject("Assiette");
            SpriteRenderer sr = plate.AddComponent<SpriteRenderer>();
            sr.sprite = plateSprite;
            sr.sortingOrder = 2; // Même sortingOrder que les ingrédients
            plate.SetActive(true); // S'assurer que l'assiette est visible
            return plate;
        }
        else
        {
            Debug.LogWarning("Sprite d'assiette non trouvé. Assignez-le dans l'inspecteur ou dans IngredientSpriteManager.");
        }
        return null;
    }

    private CookingStation FindFreeCookingStation()
    {
        foreach (CookingStation station in cookingStations)
        {
            if (!station.HasUtensil() && station.IsAvailable())
            {
                return station;
            }
        }
        return null;
    }

    private PlateStation FindFreePlateStation()
    {
        foreach (PlateStation station in plateStations)
        {
            if (!station.HasPlate() && station.IsAvailable())
            {
                return station;
            }
        }
        return null;
    }

    private ServeStation FindFreeServeStation()
    {
        if (serveStations.Length > 0)
        {
            return serveStations[0];
        }
        return null;
    }
}

