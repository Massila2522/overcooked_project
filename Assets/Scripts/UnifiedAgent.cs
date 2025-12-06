using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class UnifiedAgent : Agent
{
    // Références communes à tous les agents
    private RecipeManager recipeManager;
    private ReserveStation[] reserves;
    private CuttingStation cuttingStation; // Une seule station de découpage
    private CookingStation[] cookingStations;
    private PlateStation[] plateStations;
    private ServeStation[] serveStations;
    private GameObject[] vaisselleStations; // Pile d'assiettes
    private GameObject marmitePrefab;
    private GameObject panPrefab;

    private Recipe currentRecipe;
    private PlateStation currentPlateStation; // L'assiette en cours de préparation
    private CookingStation currentCookingStation; // La station de cuisson en cours d'utilisation

    [Header("Sprites (optionnel - si non assigné, utilise IngredientSpriteManager)")]
    public Sprite marmiteSpriteOverride;
    public Sprite panSpriteOverride;
    public Sprite plateSpriteOverride;
    public Sprite soupSpriteOverride;
    public Sprite burgerSpriteOverride;

    protected override void Start()
    {
        // Si agentLabel n'est pas défini dans l'inspecteur, utiliser une valeur par défaut
        if (string.IsNullOrEmpty(agentLabel))
        {
            agentLabel = "Unified Agent";
        }

        base.Start();

        // Initialiser toutes les références
        recipeManager = FindFirstObjectByType<RecipeManager>();
        reserves = FindObjectsByType<ReserveStation>(FindObjectsSortMode.None);
        CuttingStation[] allCuttingStations = FindObjectsByType<CuttingStation>(FindObjectsSortMode.None);
        cuttingStation = allCuttingStations.Length > 0 ? allCuttingStations[0] : null; // Prendre la première station
        cookingStations = FindObjectsByType<CookingStation>(FindObjectsSortMode.None);
        plateStations = FindObjectsByType<PlateStation>(FindObjectsSortMode.None);
        serveStations = FindObjectsByType<ServeStation>(FindObjectsSortMode.None);
        
        // Trouver les stations vaisselle (pile d'assiettes)
        GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        List<GameObject> vaisselleList = new List<GameObject>();
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

    // ============================================
    // WORK LOOP PRINCIPAL - Logique simplifiée
    // ============================================
    private IEnumerator WorkLoop()
    {
        while (true)
        {
            // Vérifier si le jeu est terminé
            if (GameManager.Instance != null && GameManager.Instance.IsGameFinished())
            {
                Debug.Log($"[{gameObject.name}] Jeu terminé - Agent arrêté.");
                yield break;
            }
            
            // Traiter une recette complète de A à Z
            yield return StartCoroutine(ProcessRecipe());
            yield return new WaitForSeconds(0.1f);
        }
    }

    // ============================================
    // LOGIQUE DE DÉCOUPAGE - Découpe et retourne l'ingrédient
    // ============================================
    private IEnumerator CutIngredientAtStation(Ingredient ingredient, System.Action<Ingredient> onCut)
    {
        if (cuttingStation == null || ingredient == null)
        {
            onCut?.Invoke(null);
            yield break;
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

        // Aller à la station de découpage
        MoveTo(cuttingStation.transform);
        yield return new WaitUntil(() => !isMoving);

        // Attendre que la station soit libre
        while (cuttingStation.HasIngredient() || cuttingStation.IsCutting())
        {
            yield return new WaitForSeconds(0.1f);
        }

        // Placer l'ingrédient
        PickUpIngredient(ingredient);
        bool placed = cuttingStation.PlaceIngredient(ingredient);
        if (!placed)
        {
            DropIngredient();
            onCut?.Invoke(null);
            yield break;
        }
        DropIngredient();

        // Commencer le découpage
        if (!cuttingStation.TryStartCutting(this))
        {
            cuttingStation.TakeIngredient();
            onCut?.Invoke(null);
            yield break;
        }

        // Attendre que le découpage soit terminé
        yield return new WaitUntil(() => !cuttingStation.IsCutting());

        // Prendre l'ingrédient découpé
        Ingredient cutIngredient = cuttingStation.TakeIngredient();
        if (cutIngredient == null)
        {
            onCut?.Invoke(null);
            yield break;
        }

        PickUpIngredient(cutIngredient);
        onCut?.Invoke(cutIngredient);
    }

    // ============================================
    // MÉTHODES UTILITAIRES
    // ============================================
    
    // Chercher un ingrédient dans la réserve
    private IEnumerator FetchIngredientFromReserve(IngredientType type, int recipeId, System.Action<Ingredient> onFetched)
    {
        ReserveStation reserve = FindReserve(type);
        if (reserve == null)
        {
            Debug.LogError($"Réserve pour {type} introuvable.");
            onFetched?.Invoke(null);
            yield break;
        }

        // Aller à la réserve
        MoveTo(reserve.transform);
        yield return new WaitUntil(() => !isMoving);

        // Prendre l'ingrédient
        Ingredient ingredient = reserve.TakeIngredient();
        if (ingredient == null)
        {
            Debug.LogError($"Impossible de prendre {type} de la réserve.");
            onFetched?.Invoke(null);
            yield break;
        }

        // Assigner le recipeId
        ingredient.RecipeId = recipeId;

        if (currentIngredient != null)
        {
            DropIngredient();
        }
        PickUpIngredient(ingredient);
        onFetched?.Invoke(ingredient);
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

    // Cuire la viande dans la poêle
    private IEnumerator CookMeatInPan(Ingredient cutMeat, Recipe recipe, System.Action<Ingredient> onCooked)
    {
        if (panPrefab == null || cutMeat == null)
        {
            Debug.LogError("Poêle prefab introuvable ou viande invalide.");
            onCooked?.Invoke(null);
            yield break;
        }

        // Trouver une station de cuisson libre
        currentCookingStation = FindFreeCookingStation();
        while (currentCookingStation == null)
        {
            yield return new WaitForSeconds(0.1f);
            currentCookingStation = FindFreeCookingStation();
        }

        // Placer la poêle
        GameObject panInstance = Instantiate(panPrefab);
        while (!currentCookingStation.PlaceUtensil(panInstance, false, this))
        {
            Destroy(panInstance);
            currentCookingStation = null;
            yield return new WaitForSeconds(0.1f);
            while (currentCookingStation == null)
            {
                yield return new WaitForSeconds(0.1f);
                currentCookingStation = FindFreeCookingStation();
            }
            panInstance = Instantiate(panPrefab);
        }

        // Mettre la viande dans la poêle
        if (currentIngredient != null)
        {
            DropIngredient();
        }
        PickUpIngredient(cutMeat);
        MoveTo(currentCookingStation.transform);
        yield return new WaitUntil(() => !isMoving);
        currentCookingStation.AddIngredient(cutMeat);
        DropIngredient();

        // Cuire
        MoveTo(currentCookingStation.transform);
        yield return new WaitUntil(() => !isMoving);
        Recipe meatRecipe = new Recipe(RecipeType.Burger, recipe.Order);
        currentCookingStation.StartCooking(meatRecipe);
        yield return new WaitUntil(() => currentCookingStation.IsReady());

        // Récupérer la viande cuite
        Ingredient cookedMeat = null;
        foreach (var ing in currentCookingStation.GetCookedIngredients())
        {
            if (ing.Type == IngredientType.Meat && ing.State == IngredientState.Cooked)
            {
                cookedMeat = ing;
                break;
            }
        }

        currentCookingStation.ClearStation();
        currentCookingStation = null;
        onCooked?.Invoke(cookedMeat);
    }

    // ============================================
    // LOGIQUE D'ASSEMBLAGE DE RECETTES - Nouvelle logique simplifiée
    // ============================================
    private IEnumerator ProcessRecipe()
    {
        // Récupérer la prochaine recette disponible
        currentRecipe = recipeManager.GetNextRecipe();
        if (currentRecipe == null)
        {
            yield return new WaitForSeconds(0.5f);
            yield break;
        }

        // Réserver la recette
        if (!recipeManager.TryReserveRecipe(currentRecipe, this))
        {
            currentRecipe = null;
            yield return new WaitForSeconds(0.5f);
            yield break;
        }

        // ÉTAPE 1 : Poser une assiette sur une table
        currentPlateStation = null;
        yield return StartCoroutine(PlacePlateForRecipe(currentRecipe, plate => currentPlateStation = plate));
        
        if (currentPlateStation == null)
        {
            currentRecipe.IsReserved = false;
            currentRecipe.ReservedBy = null;
            currentRecipe = null;
            yield return new WaitForSeconds(0.5f);
            yield break;
        }

        // ÉTAPE 2 : Assembler la recette selon le type
        if (currentRecipe.IsSoup())
        {
            yield return StartCoroutine(AssembleSoup(currentRecipe));
        }
        else
        {
            yield return StartCoroutine(AssembleBurger(currentRecipe));
        }

        // ÉTAPE 3 : Attendre que l'assiette soit prête
        yield return new WaitUntil(() => currentPlateStation.IsReady());

        // ÉTAPE 4 : Servir l'assiette
        GameObject finishedPlate = currentPlateStation.TakePlate();
        if (finishedPlate != null)
        {
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

        // Marquer la recette comme complétée
        recipeManager.CompleteRecipe(currentRecipe);
        currentRecipe = null;
        currentPlateStation = null;
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

    private IEnumerator AssembleSoup(Recipe recipe)
    {
        if (marmitePrefab == null)
        {
            Debug.LogError("Marmite prefab introuvable pour UnifiedAgent.");
            yield break;
        }

        // Trouver une station de cuisson libre
        currentCookingStation = FindFreeCookingStation();
        while (currentCookingStation == null)
        {
            yield return new WaitForSeconds(0.1f);
            currentCookingStation = FindFreeCookingStation();
        }

        // Placer la marmite
        GameObject marmiteInstance = Instantiate(marmitePrefab);
        while (!currentCookingStation.PlaceUtensil(marmiteInstance, true, this))
        {
            Destroy(marmiteInstance);
            currentCookingStation = null;
            yield return new WaitForSeconds(0.1f);
            while (currentCookingStation == null)
            {
                yield return new WaitForSeconds(0.1f);
                currentCookingStation = FindFreeCookingStation();
            }
            marmiteInstance = Instantiate(marmitePrefab);
        }

        // Pour chaque ingrédient (3 fois) : chercher → découper → mettre dans la marmite
        for (int i = 0; i < 3; i++)
        {
            IngredientType neededType = recipe.RequiredIngredients[i];
            
            // 1. Chercher l'ingrédient dans la réserve
            Ingredient rawIngredient = null;
            yield return StartCoroutine(FetchIngredientFromReserve(neededType, recipe.Order, fetched => rawIngredient = fetched));
            if (rawIngredient == null)
            {
                Debug.LogError($"Impossible de récupérer {neededType} de la réserve.");
                yield break;
            }

            // 2. Découper l'ingrédient
            Ingredient cutIngredient = null;
            yield return StartCoroutine(CutIngredientAtStation(rawIngredient, cut => cutIngredient = cut));
            if (cutIngredient == null)
            {
                Debug.LogError($"Impossible de découper {neededType}.");
                yield break;
            }

            // 3. Mettre directement dans la marmite
            if (currentIngredient != null)
            {
                DropIngredient();
            }
            PickUpIngredient(cutIngredient);
            MoveTo(currentCookingStation.transform);
            yield return new WaitUntil(() => !isMoving);
            currentCookingStation.AddIngredient(cutIngredient);
            DropIngredient();
        }

        // Cuire
        MoveTo(currentCookingStation.transform);
        yield return new WaitUntil(() => !isMoving);
        currentCookingStation.StartCooking(recipe);
        yield return new WaitUntil(() => currentCookingStation.IsReady());

        // Récupérer les ingrédients cuits et les mettre dans l'assiette
        var cookedIngredients = currentCookingStation.GetCookedIngredients();
        foreach (var ing in cookedIngredients)
        {
            if (currentIngredient != null)
            {
                DropIngredient();
            }
            if (currentPlate != null)
            {
                DropPlate();
            }

            PickUpIngredient(ing);
            MoveTo(currentPlateStation.transform);
            yield return new WaitUntil(() => !isMoving);
            currentPlateStation.AddIngredient(ing);
            DropIngredient();
        }

        currentCookingStation.ClearStation();
        currentCookingStation = null;
    }

    private IEnumerator AssembleBurger(Recipe recipe)
    {
        // 1. Chercher le pain et le mettre sur l'assiette
        Ingredient bun = null;
        yield return StartCoroutine(FetchIngredientFromReserve(IngredientType.BurgerBun, recipe.Order, fetched => bun = fetched));
        if (bun != null)
        {
            if (currentIngredient != null)
            {
                DropIngredient();
            }
            PickUpIngredient(bun);
            MoveTo(currentPlateStation.transform);
            yield return new WaitUntil(() => !isMoving);
            currentPlateStation.AddIngredient(bun);
            DropIngredient();
        }

        // 2. Chercher le steak → découper → mettre sur la poêle → cuire → mettre dans l'assiette
        Ingredient rawMeat = null;
        yield return StartCoroutine(FetchIngredientFromReserve(IngredientType.Meat, recipe.Order, fetched => rawMeat = fetched));
        if (rawMeat != null)
        {
            // Découper
            Ingredient cutMeat = null;
            yield return StartCoroutine(CutIngredientAtStation(rawMeat, cut => cutMeat = cut));
            if (cutMeat != null)
            {
                // Mettre sur la poêle et cuire
                Ingredient cookedMeat = null;
                yield return StartCoroutine(CookMeatInPan(cutMeat, recipe, cooked => cookedMeat = cooked));
                if (cookedMeat != null)
                {
                    // Mettre dans l'assiette
                    if (currentIngredient != null)
                    {
                        DropIngredient();
                    }
                    PickUpIngredient(cookedMeat);
                    MoveTo(currentPlateStation.transform);
                    yield return new WaitUntil(() => !isMoving);
                    currentPlateStation.AddIngredient(cookedMeat);
                    DropIngredient();
                }
            }
        }

        // 3. Chercher la salade → découper → mettre dans l'assiette
        Ingredient rawLettuce = null;
        yield return StartCoroutine(FetchIngredientFromReserve(IngredientType.Lettuce, recipe.Order, fetched => rawLettuce = fetched));
        if (rawLettuce != null)
        {
            Ingredient cutLettuce = null;
            yield return StartCoroutine(CutIngredientAtStation(rawLettuce, cut => cutLettuce = cut));
            if (cutLettuce != null)
            {
                if (currentIngredient != null)
                {
                    DropIngredient();
                }
                PickUpIngredient(cutLettuce);
                MoveTo(currentPlateStation.transform);
                yield return new WaitUntil(() => !isMoving);
                currentPlateStation.AddIngredient(cutLettuce);
                DropIngredient();
            }
        }

        // 4. Chercher la tomate → découper → mettre dans l'assiette
        Ingredient rawTomato = null;
        yield return StartCoroutine(FetchIngredientFromReserve(IngredientType.Tomato, recipe.Order, fetched => rawTomato = fetched));
        if (rawTomato != null)
        {
            Ingredient cutTomato = null;
            yield return StartCoroutine(CutIngredientAtStation(rawTomato, cut => cutTomato = cut));
            if (cutTomato != null)
            {
                if (currentIngredient != null)
                {
                    DropIngredient();
                }
                PickUpIngredient(cutTomato);
                MoveTo(currentPlateStation.transform);
                yield return new WaitUntil(() => !isMoving);
                currentPlateStation.AddIngredient(cutTomato);
                DropIngredient();
            }
        }
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


