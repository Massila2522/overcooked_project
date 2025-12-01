using UnityEngine;
using System;
using System.Collections;

public class DressingAgent : Agent
{
    private RecipeManager recipeManager;
    private CookingStation[] cookingStations;
    private CutIngredientsStation[] cutIngredientsStations;
    private PlateStation[] plateStations;
    private ServeStation[] serveStations;
    private GameObject[] vaisselleStations; // Pile d'assiettes
    private GameObject marmitePrefab;
    private GameObject panPrefab;

    private Recipe currentRecipe;

    protected override void Start()
    {
        // Si agentLabel n'est pas défini dans l'inspecteur, utiliser une valeur par défaut
        if (string.IsNullOrEmpty(agentLabel))
        {
            agentLabel = "Dressing Agent";
        }

        base.Start();

        recipeManager = FindFirstObjectByType<RecipeManager>();
        cookingStations = FindObjectsByType<CookingStation>(FindObjectsSortMode.None);
        cutIngredientsStations = FindObjectsByType<CutIngredientsStation>(FindObjectsSortMode.None);
        plateStations = FindObjectsByType<PlateStation>(FindObjectsSortMode.None);
        serveStations = FindObjectsByType<ServeStation>(FindObjectsSortMode.None);
        
        // Trouver les stations vaisselle (pile d'assiettes)
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

        // Charger les prefabs
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

        // ÉTAPE 1 : Poser une assiette sur une table à assiettes libre dès maintenant
        PlateStation myPlateStation = null;
        yield return StartCoroutine(PlacePlateForRecipe(currentRecipe, plate => myPlateStation = plate));
        
        if (myPlateStation == null)
        {
            // Libérer la réservation si on ne peut pas poser d'assiette
            currentRecipe.IsReserved = false;
            currentRecipe.ReservedBy = null;
            currentRecipe = null;
            yield return new WaitForSeconds(0.5f);
            yield break;
        }

        // ÉTAPE 2 : Traiter la recette en ajoutant les ingrédients un par un
        if (currentRecipe.IsSoup())
        {
            yield return StartCoroutine(AssembleSoup(currentRecipe, myPlateStation));
        }
        else
        {
            yield return StartCoroutine(AssembleBurger(currentRecipe, myPlateStation));
        }

        // ÉTAPE 3 : Attendre que l'assiette soit prête
        yield return new WaitUntil(() => myPlateStation.IsReady());

        // ÉTAPE 4 : Servir l'assiette
        GameObject finishedPlate = myPlateStation.TakePlate();
        if (finishedPlate != null)
        {
            // S'assurer qu'on ne porte rien d'autre
            if (currentIngredient != null)
            {
                DropIngredient();
            }
            
            PickUpPlate(finishedPlate);

            // Appliquer le sprite final
            Sprite finalSprite = null;
            if (currentRecipe.IsSoup())
            {
                finalSprite = soupSpriteOverride;
                if (finalSprite == null && IngredientSpriteManager.Instance != null)
                {
                    finalSprite = IngredientSpriteManager.Instance.GetPlateSprite(currentRecipe.Type);
                }
            }
            else
            {
                finalSprite = burgerSpriteOverride;
                if (finalSprite == null && IngredientSpriteManager.Instance != null)
                {
                    finalSprite = IngredientSpriteManager.Instance.GetPlateSprite(RecipeType.Burger);
                }
            }

            if (finalSprite != null && finishedPlate != null)
            {
                SpriteRenderer sr = finishedPlate.GetComponent<SpriteRenderer>();
                if (sr != null) sr.sprite = finalSprite;
            }

            ServeStation serveStation = FindFreeServeStation();
            MoveTo(serveStation.transform);
            yield return new WaitUntil(() => !isMoving);

            serveStation?.ServePlate(finishedPlate);
            DropPlate();
        }

        // Marquer la recette comme complétée et la retirer de la file
        recipeManager.CompleteRecipe(currentRecipe);
        currentRecipe = null;
    }

    private IEnumerator PlacePlateForRecipe(Recipe recipe, System.Action<PlateStation> onPlatePlaced)
    {
        // Trouver une table à assiettes libre
        PlateStation freeStation = FindFreePlateStation();
        while (freeStation == null)
        {
            yield return new WaitForSeconds(0.1f);
            freeStation = FindFreePlateStation();
        }

        // S'assurer qu'on ne porte rien
        if (currentIngredient != null)
        {
            DropIngredient();
        }
        if (currentPlate != null)
        {
            DropPlate();
        }

        // Aller chercher une assiette à la station de vaisselle
        GameObject plate = null;
        yield return StartCoroutine(FetchPlateFromVaisselle(fetchedPlate => plate = fetchedPlate));
        if (plate == null)
        {
            yield return new WaitForSeconds(0.1f);
            onPlatePlaced?.Invoke(null);
            yield break;
        }

        // Aller à la station
        MoveTo(freeStation.transform);
        yield return new WaitUntil(() => !isMoving);

        // Poser l'assiette avec le RecipeId
        while (!freeStation.PlacePlate(plate, this, recipe.Order))
        {
            yield return new WaitForSeconds(0.1f);
            freeStation = null;
            while (freeStation == null)
            {
                yield return new WaitForSeconds(0.1f);
                freeStation = FindFreePlateStation();
            }
            MoveTo(freeStation.transform);
            yield return new WaitUntil(() => !isMoving);
        }

        freeStation.SetRecipe(recipe);
        DropPlate();
        onPlatePlaced?.Invoke(freeStation);
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

    private IEnumerator AssembleSoup(Recipe recipe, PlateStation plateStation)
    {
        if (marmitePrefab == null)
        {
            Debug.LogError("Marmite prefab introuvable pour DressingAgent.");
            yield break;
        }

        // Trouver une station de cuisson libre
        CookingStation cookingStation = FindFreeCookingStation();
        while (cookingStation == null)
        {
            yield return new WaitForSeconds(0.1f);
            cookingStation = FindFreeCookingStation();
        }

        // Placer la marmite
        GameObject marmiteInstance = Instantiate(marmitePrefab);
        while (!cookingStation.PlaceUtensil(marmiteInstance, true, this))
        {
            Destroy(marmiteInstance);
            cookingStation = null;
            yield return new WaitForSeconds(0.1f);
            while (cookingStation == null)
            {
                yield return new WaitForSeconds(0.1f);
                cookingStation = FindFreeCookingStation();
            }
            marmiteInstance = Instantiate(marmitePrefab);
        }

        // Ajouter tous les ingrédients à la marmite
        for (int i = 0; i < 3; i++)
        {
            IngredientType neededType = recipe.RequiredIngredients[i];
            Ingredient ingredient = null;
            yield return StartCoroutine(FetchCutIngredient(neededType, recipe.Order, fetched => ingredient = fetched));
            if (ingredient == null)
            {
                yield return new WaitForSeconds(0.1f);
                i--;
                continue;
            }

            // S'assurer qu'on ne porte rien
            if (currentIngredient != null)
            {
                DropIngredient();
            }
            if (currentPlate != null)
            {
                DropPlate();
            }

            PickUpIngredient(ingredient);
            MoveTo(cookingStation.transform);
            yield return new WaitUntil(() => !isMoving);
            cookingStation.AddIngredient(ingredient);
            DropIngredient();
        }

        // Cuire
        MoveTo(cookingStation.transform);
        yield return new WaitUntil(() => !isMoving);
        cookingStation.StartCooking(recipe);
        yield return new WaitUntil(() => cookingStation.IsReady());

        // Récupérer les ingrédients cuits et les ajouter un par un à l'assiette
        var cookedIngredients = cookingStation.GetCookedIngredients();
        foreach (var ing in cookedIngredients)
        {
            // S'assurer qu'on ne porte rien
            if (currentIngredient != null)
            {
                DropIngredient();
            }
            if (currentPlate != null)
            {
                DropPlate();
            }

            PickUpIngredient(ing);
            MoveTo(plateStation.transform);
            yield return new WaitUntil(() => !isMoving);
            plateStation.AddIngredient(ing);
            DropIngredient();
        }

        cookingStation.ClearStation();
    }

    private IEnumerator AssembleBurger(Recipe recipe, PlateStation plateStation)
    {
        // Ajouter les ingrédients un par un dans l'assiette
        foreach (IngredientType neededType in recipe.RequiredIngredients)
        {
            if (neededType == IngredientType.Meat)
            {
                // Cuire la viande d'abord
                Ingredient cookedMeat = null;
                yield return StartCoroutine(CookMeat(recipe, ing => cookedMeat = ing));
                
                if (cookedMeat != null)
                {
                    // S'assurer qu'on ne porte rien
                    if (currentIngredient != null)
                    {
                        DropIngredient();
                    }
                    if (currentPlate != null)
                    {
                        DropPlate();
                    }

                    PickUpIngredient(cookedMeat);
                    MoveTo(plateStation.transform);
                    yield return new WaitUntil(() => !isMoving);
                    plateStation.AddIngredient(cookedMeat);
                    DropIngredient();
                }
            }
            else
            {
                Ingredient ingredient = null;
                yield return StartCoroutine(FetchCutIngredient(neededType, recipe.Order, fetched => ingredient = fetched));
                if (ingredient == null)
                {
                    continue;
                }

                // S'assurer qu'on ne porte rien
                if (currentIngredient != null)
                {
                    DropIngredient();
                }
                if (currentPlate != null)
                {
                    DropPlate();
                }

                PickUpIngredient(ingredient);
                MoveTo(plateStation.transform);
                yield return new WaitUntil(() => !isMoving);
                plateStation.AddIngredient(ingredient);
                DropIngredient();
            }
        }
    }

    private IEnumerator CookMeat(Recipe recipe, Action<Ingredient> onCooked)
    {
        if (panPrefab == null)
        {
            Debug.LogError("Poêle prefab introuvable pour DressingAgent.");
            yield break;
        }

        Ingredient meat = null;
        yield return StartCoroutine(FetchCutIngredient(IngredientType.Meat, recipe.Order, fetched => meat = fetched));
        while (meat == null || meat.State != IngredientState.Chopped)
        {
            yield return new WaitForSeconds(0.1f);
            yield return StartCoroutine(FetchCutIngredient(IngredientType.Meat, recipe.Order, fetched => meat = fetched));
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

    private IEnumerator FetchCutIngredient(IngredientType type, int recipeId, Action<Ingredient> onFetched)
    {
        Ingredient fetched = null;
        while (fetched == null)
        {
            CutIngredientsStation targetStation = FindCutStationWithIngredient(type, recipeId);
            while (targetStation == null)
            {
                currentState = AgentState.Waiting;
                yield return new WaitForSeconds(0.1f);
                targetStation = FindCutStationWithIngredient(type, recipeId);
            }

            MoveTo(targetStation.transform);
            yield return new WaitUntil(() => !isMoving);

            Ingredient candidate = targetStation.TakeIngredientOfTypeForRecipe(type, recipeId);
            if (candidate == null)
            {
                yield return new WaitForSeconds(0.05f);
                continue;
            }

            bool ready = candidate.State == IngredientState.Cut || candidate.State == IngredientState.Chopped;
            if (type == IngredientType.BurgerBun)
            {
                ready = true;
            }

            if (!ready)
            {
                Debug.LogWarning($"Ingrédient {candidate.Type} récupéré dans un état inattendu ({candidate.State}).");
                continue;
            }

            fetched = candidate;
        }

        currentState = AgentState.Idle;
        onFetched?.Invoke(fetched);
    }

    private CutIngredientsStation FindCutStationWithIngredient(IngredientType type, int recipeId)
    {
        foreach (CutIngredientsStation station in cutIngredientsStations)
        {
            if (station.HasIngredientOfTypeForRecipe(type, recipeId))
            {
                return station;
            }
        }

        return null;
    }

    private IEnumerator FetchPlateFromVaisselle(System.Action<GameObject> onFetched)
    {
        GameObject station = FindAvailableVaisselleStation();
        while (station == null)
        {
            currentState = AgentState.Waiting;
            yield return new WaitForSeconds(0.1f);
            station = FindAvailableVaisselleStation();
        }

        MoveTo(station.transform);
        yield return new WaitUntil(() => !isMoving);

        GameObject plate = CreatePlateObject();
        if (plate != null)
        {
            PickUpPlate(plate);
        }

        onFetched?.Invoke(plate);
    }

    private GameObject FindAvailableVaisselleStation()
    {
        if (vaisselleStations == null || vaisselleStations.Length == 0)
        {
            return null;
        }

        // simple round-robin: retourner la station la plus proche disponible
        GameObject closest = null;
        float bestDist = float.MaxValue;
        foreach (GameObject station in vaisselleStations)
        {
            if (station == null) continue;
            float dist = Vector2.Distance(transform.position, station.transform.position);
            if (dist < bestDist)
            {
                bestDist = dist;
                closest = station;
            }
        }
        return closest;
    }

    private GameObject CreatePlateObject()
    {
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
            sr.sortingOrder = 2;
            plate.SetActive(true);
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
            return serveStations[0]; // Toujours disponible
        }
        return null;
    }
}

