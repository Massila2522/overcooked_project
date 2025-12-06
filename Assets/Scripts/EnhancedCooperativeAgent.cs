using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Version améliorée du CooperativeAgent avec intégration UI
/// et meilleure gestion des tâches visuelles.
/// </summary>
public class EnhancedCooperativeAgent : Agent
{
    private static List<EnhancedCooperativeAgent> allAgents = new List<EnhancedCooperativeAgent>();
    private static int agentCounter = 0;
    
    [Header("Agent Configuration")]
    [SerializeField] private Color agentColor = Color.white;
    [SerializeField] private int agentId;
    
    // Références communes à tous les agents
    private RecipeManager recipeManager;
    private ReserveStation[] reserves;
    private CuttingStation[] cuttingStations;
    private CookingStation[] cookingStations;
    private PlateStation[] plateStations;
    private ServeStation[] serveStations;
    private GameObject[] vaisselleStations;
    private GameObject marmitePrefab;
    private GameObject panPrefab;

    private Recipe currentRecipe;
    private PlateStation currentPlateStation;
    private CookingStation currentCookingStation;
    private CuttingStation currentCuttingStation;

    [Header("Sprites (optionnel)")]
    public Sprite marmiteSpriteOverride;
    public Sprite panSpriteOverride;
    public Sprite plateSpriteOverride;
    public Sprite soupSpriteOverride;
    public Sprite burgerSpriteOverride;

    // Statistiques
    private int recipesCompleted = 0;
    
    // Gestion des étapes pour l'UI
    private string[] currentSteps;
    private int currentStepIndex = 0;

    protected override void Start()
    {
        agentId = agentCounter++;
        
        if (string.IsNullOrEmpty(agentLabel))
        {
            agentLabel = $"Agent {agentId + 1}";
        }

        base.Start();

        // Appliquer la couleur de l'agent
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = agentColor;
        }

        // S'enregistrer dans la liste des agents
        if (!allAgents.Contains(this))
        {
            allAgents.Add(this);
        }

        // Initialiser les références
        recipeManager = FindFirstObjectByType<RecipeManager>();
        reserves = FindObjectsByType<ReserveStation>(FindObjectsSortMode.None);
        cuttingStations = FindObjectsByType<CuttingStation>(FindObjectsSortMode.None);
        cookingStations = FindObjectsByType<CookingStation>(FindObjectsSortMode.None);
        plateStations = FindObjectsByType<PlateStation>(FindObjectsSortMode.None);
        serveStations = FindObjectsByType<ServeStation>(FindObjectsSortMode.None);
        
        // Trouver les stations vaisselle
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

        LoadPrefabs();
        
        // Enregistrer auprès de l'UI
        RegisterWithUI();
        
        StartCoroutine(DelayedStart());
    }

    private void RegisterWithUI()
    {
        if (TaskDisplayUI.Instance != null)
        {
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            Color color = sr != null ? sr.color : agentColor;
            TaskDisplayUI.Instance.RegisterAgent(agentId, agentLabel, color);
        }
    }

    private void OnDestroy()
    {
        allAgents.Remove(this);
    }

    private IEnumerator DelayedStart()
    {
        yield return new WaitForSeconds(agentId * 0.3f);
        StartCoroutine(WorkLoop());
    }

    private void LoadPrefabs()
    {
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
        
        if (panSprite != null)
        {
            GameObject panObj = new GameObject("Poêle");
            SpriteRenderer sr = panObj.AddComponent<SpriteRenderer>();
            sr.sprite = panSprite;
            sr.sortingOrder = 2;
            panObj.SetActive(false);
            panPrefab = panObj;
        }
    }

    // ============================================
    // MISE À JOUR DE L'UI
    // ============================================
    
    private void UpdateUITask(string recipeName, string[] steps, int currentStep)
    {
        currentSteps = steps;
        currentStepIndex = currentStep;
        
        if (TaskDisplayUI.Instance != null)
        {
            TaskDisplayUI.Instance.SetAgentTask(agentId, recipeName, steps, currentStep);
        }
    }

    private void AdvanceUIStep()
    {
        currentStepIndex++;
        if (TaskDisplayUI.Instance != null)
        {
            TaskDisplayUI.Instance.AdvanceStep(agentId);
        }
    }

    private void UpdateUIStatus(string status)
    {
        if (TaskDisplayUI.Instance != null)
        {
            TaskDisplayUI.Instance.SetAgentStatus(agentId, status);
        }
    }

    private void CompleteUITask()
    {
        if (TaskDisplayUI.Instance != null)
        {
            TaskDisplayUI.Instance.CompleteTask(agentId);
        }
    }

    // ============================================
    // WORK LOOP PRINCIPAL
    // ============================================
    private IEnumerator WorkLoop()
    {
        while (true)
        {
            if (GameManager.Instance != null && GameManager.Instance.IsGameFinished())
            {
                Debug.Log($"[{agentLabel}] Jeu terminé - {recipesCompleted} recettes complétées.");
                yield break;
            }
            
            UpdateUIStatus("idle");
            yield return StartCoroutine(ProcessRecipe());
            yield return new WaitForSeconds(0.1f);
        }
    }

    // ============================================
    // COORDINATION ENTRE AGENTS
    // ============================================
    
    private CuttingStation FindBestCuttingStation()
    {
        CuttingStation best = null;
        float bestDist = float.MaxValue;
        
        foreach (CuttingStation station in cuttingStations)
        {
            if (!station.HasIngredient() && !station.IsCutting() && station.IsAvailable())
            {
                float dist = Vector2.Distance(transform.position, station.transform.position);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    best = station;
                }
            }
        }
        return best;
    }

    private IEnumerator WaitForCuttingStation()
    {
        currentCuttingStation = FindBestCuttingStation();
        int attempts = 0;
        
        while (currentCuttingStation == null && attempts < 100)
        {
            UpdateUIStatus("waiting");
            yield return new WaitForSeconds(0.2f);
            currentCuttingStation = FindBestCuttingStation();
            attempts++;
        }
    }

    // ============================================
    // LOGIQUE DE DÉCOUPAGE
    // ============================================
    private IEnumerator CutIngredientAtStation(Ingredient ingredient, System.Action<Ingredient> onCut)
    {
        if (ingredient == null)
        {
            onCut?.Invoke(null);
            yield break;
        }

        if (currentIngredient != null) DropIngredient();
        if (currentPlate != null) DropPlate();

        yield return StartCoroutine(WaitForCuttingStation());
        
        if (currentCuttingStation == null)
        {
            onCut?.Invoke(null);
            yield break;
        }

        UpdateUIStatus("moving");
        MoveTo(currentCuttingStation.transform);
        yield return new WaitUntil(() => !isMoving);

        int waitAttempts = 0;
        while ((currentCuttingStation.HasIngredient() || currentCuttingStation.IsCutting()) && waitAttempts < 50)
        {
            UpdateUIStatus("waiting");
            yield return new WaitForSeconds(0.1f);
            waitAttempts++;
            
            if (waitAttempts > 10)
            {
                CuttingStation other = FindBestCuttingStation();
                if (other != null && other != currentCuttingStation)
                {
                    currentCuttingStation = other;
                    MoveTo(currentCuttingStation.transform);
                    yield return new WaitUntil(() => !isMoving);
                    waitAttempts = 0;
                }
            }
        }

        UpdateUIStatus("working");
        PickUpIngredient(ingredient);
        bool placed = currentCuttingStation.PlaceIngredient(ingredient);
        if (!placed)
        {
            DropIngredient();
            currentCuttingStation = null;
            onCut?.Invoke(null);
            yield break;
        }
        DropIngredient();

        if (!currentCuttingStation.TryStartCutting(this))
        {
            currentCuttingStation.TakeIngredient();
            currentCuttingStation = null;
            onCut?.Invoke(null);
            yield break;
        }

        yield return new WaitUntil(() => !currentCuttingStation.IsCutting());

        Ingredient cutIngredient = currentCuttingStation.TakeIngredient();
        currentCuttingStation = null;
        
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
    
    private IEnumerator FetchIngredientFromReserve(IngredientType type, int recipeId, System.Action<Ingredient> onFetched)
    {
        ReserveStation reserve = FindReserve(type);
        if (reserve == null)
        {
            onFetched?.Invoke(null);
            yield break;
        }

        UpdateUIStatus("moving");
        MoveTo(reserve.transform);
        yield return new WaitUntil(() => !isMoving);

        Ingredient ingredient = reserve.TakeIngredient();
        if (ingredient == null)
        {
            onFetched?.Invoke(null);
            yield break;
        }

        ingredient.RecipeId = recipeId;

        if (currentIngredient != null) DropIngredient();
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

    private IEnumerator CookMeatInPan(Ingredient cutMeat, Recipe recipe, System.Action<Ingredient> onCooked)
    {
        if (panPrefab == null || cutMeat == null)
        {
            onCooked?.Invoke(null);
            yield break;
        }

        currentCookingStation = FindFreeCookingStation();
        while (currentCookingStation == null)
        {
            UpdateUIStatus("waiting");
            yield return new WaitForSeconds(0.15f);
            currentCookingStation = FindFreeCookingStation();
        }

        UpdateUIStatus("working");
        GameObject panInstance = Instantiate(panPrefab);
        while (!currentCookingStation.PlaceUtensil(panInstance, false, this))
        {
            Destroy(panInstance);
            currentCookingStation = null;
            yield return new WaitForSeconds(0.15f);
            while (currentCookingStation == null)
            {
                yield return new WaitForSeconds(0.15f);
                currentCookingStation = FindFreeCookingStation();
            }
            panInstance = Instantiate(panPrefab);
        }

        if (currentIngredient != null) DropIngredient();
        PickUpIngredient(cutMeat);
        MoveTo(currentCookingStation.transform);
        yield return new WaitUntil(() => !isMoving);
        currentCookingStation.AddIngredient(cutMeat);
        DropIngredient();

        MoveTo(currentCookingStation.transform);
        yield return new WaitUntil(() => !isMoving);
        Recipe meatRecipe = new Recipe(RecipeType.Burger, recipe.Order);
        currentCookingStation.StartCooking(meatRecipe);
        yield return new WaitUntil(() => currentCookingStation.IsReady());

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
    // LOGIQUE PRINCIPALE DE RECETTES
    // ============================================
    private IEnumerator ProcessRecipe()
    {
        currentRecipe = recipeManager.GetNextRecipe();
        if (currentRecipe == null)
        {
            yield return new WaitForSeconds(0.5f);
            yield break;
        }

        if (!recipeManager.TryReserveRecipe(currentRecipe, this))
        {
            currentRecipe = null;
            yield return new WaitForSeconds(0.3f);
            yield break;
        }

        Debug.Log($"[{agentLabel}] Démarre: {currentRecipe.GetDisplayName()}");

        // Définir les étapes pour l'UI
        string[] steps = GetRecipeSteps(currentRecipe);
        UpdateUITask(currentRecipe.GetDisplayName(), steps, 0);

        // ÉTAPE 1 : Poser une assiette
        currentPlateStation = null;
        yield return StartCoroutine(PlacePlateForRecipe(currentRecipe, plate => currentPlateStation = plate));
        AdvanceUIStep();
        
        if (currentPlateStation == null)
        {
            currentRecipe.IsReserved = false;
            currentRecipe.ReservedBy = null;
            currentRecipe = null;
            yield return new WaitForSeconds(0.5f);
            yield break;
        }

        // ÉTAPE 2 : Assembler selon le type
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
        AdvanceUIStep();

        // ÉTAPE 4 : Servir
        GameObject finishedPlate = currentPlateStation.TakePlate();
        if (finishedPlate != null)
        {
            if (currentIngredient != null) DropIngredient();
            
            PickUpPlate(finishedPlate);

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
            UpdateUIStatus("moving");
            MoveTo(serveStation.transform);
            yield return new WaitUntil(() => !isMoving);

            serveStation?.ServePlate(finishedPlate);
            DropPlate();
        }

        recipeManager.CompleteRecipe(currentRecipe);
        recipesCompleted++;
        Debug.Log($"[{agentLabel}] ✅ Recette terminée! Total: {recipesCompleted}");
        
        CompleteUITask();
        
        currentRecipe = null;
        currentPlateStation = null;
    }

    private string[] GetRecipeSteps(Recipe recipe)
    {
        if (recipe.IsSoup())
        {
            return new string[]
            {
                "Assiette",
                "Marmite",
                "Ingrédient 1",
                "Ingrédient 2",
                "Ingrédient 3",
                "Cuisson",
                "Servir"
            };
        }
        else // Burger
        {
            return new string[]
            {
                "Assiette",
                "Pain",
                "Viande",
                "Cuisson",
                "Salade",
                "Tomate",
                "Servir"
            };
        }
    }

    private IEnumerator PlacePlateForRecipe(Recipe recipe, System.Action<PlateStation> onPlatePlaced)
    {
        PlateStation freeStation = FindFreePlateStation();
        while (freeStation == null)
        {
            UpdateUIStatus("waiting");
            yield return new WaitForSeconds(0.15f);
            freeStation = FindFreePlateStation();
        }

        if (currentIngredient != null) DropIngredient();
        if (currentPlate != null) DropPlate();

        GameObject plate = null;
        yield return StartCoroutine(FetchPlateFromVaisselle(fetchedPlate => plate = fetchedPlate));
        if (plate == null)
        {
            yield return new WaitForSeconds(0.1f);
            onPlatePlaced?.Invoke(null);
            yield break;
        }

        UpdateUIStatus("moving");
        MoveTo(freeStation.transform);
        yield return new WaitUntil(() => !isMoving);

        while (!freeStation.PlacePlate(plate, this, recipe.Order))
        {
            yield return new WaitForSeconds(0.15f);
            freeStation = null;
            while (freeStation == null)
            {
                yield return new WaitForSeconds(0.15f);
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
            yield break;
        }

        currentCookingStation = FindFreeCookingStation();
        while (currentCookingStation == null)
        {
            UpdateUIStatus("waiting");
            yield return new WaitForSeconds(0.15f);
            currentCookingStation = FindFreeCookingStation();
        }
        AdvanceUIStep(); // Marmite

        UpdateUIStatus("working");
        GameObject marmiteInstance = Instantiate(marmitePrefab);
        while (!currentCookingStation.PlaceUtensil(marmiteInstance, true, this))
        {
            Destroy(marmiteInstance);
            currentCookingStation = null;
            yield return new WaitForSeconds(0.15f);
            while (currentCookingStation == null)
            {
                yield return new WaitForSeconds(0.15f);
                currentCookingStation = FindFreeCookingStation();
            }
            marmiteInstance = Instantiate(marmitePrefab);
        }

        for (int i = 0; i < 3; i++)
        {
            IngredientType neededType = recipe.RequiredIngredients[i];
            
            Ingredient rawIngredient = null;
            yield return StartCoroutine(FetchIngredientFromReserve(neededType, recipe.Order, fetched => rawIngredient = fetched));
            if (rawIngredient == null) yield break;

            Ingredient cutIngredient = null;
            yield return StartCoroutine(CutIngredientAtStation(rawIngredient, cut => cutIngredient = cut));
            if (cutIngredient == null) yield break;

            if (currentIngredient != null) DropIngredient();
            PickUpIngredient(cutIngredient);
            MoveTo(currentCookingStation.transform);
            yield return new WaitUntil(() => !isMoving);
            currentCookingStation.AddIngredient(cutIngredient);
            DropIngredient();
            
            AdvanceUIStep(); // Ingrédient i
        }

        MoveTo(currentCookingStation.transform);
        yield return new WaitUntil(() => !isMoving);
        currentCookingStation.StartCooking(recipe);
        yield return new WaitUntil(() => currentCookingStation.IsReady());
        AdvanceUIStep(); // Cuisson

        var cookedIngredients = currentCookingStation.GetCookedIngredients();
        foreach (var ing in cookedIngredients)
        {
            if (currentIngredient != null) DropIngredient();
            if (currentPlate != null) DropPlate();

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
        // 1. Pain
        Ingredient bun = null;
        yield return StartCoroutine(FetchIngredientFromReserve(IngredientType.BurgerBun, recipe.Order, fetched => bun = fetched));
        if (bun != null)
        {
            if (currentIngredient != null) DropIngredient();
            PickUpIngredient(bun);
            MoveTo(currentPlateStation.transform);
            yield return new WaitUntil(() => !isMoving);
            currentPlateStation.AddIngredient(bun);
            DropIngredient();
        }
        AdvanceUIStep(); // Pain

        // 2. Viande → découper → cuire
        Ingredient rawMeat = null;
        yield return StartCoroutine(FetchIngredientFromReserve(IngredientType.Meat, recipe.Order, fetched => rawMeat = fetched));
        if (rawMeat != null)
        {
            Ingredient cutMeat = null;
            yield return StartCoroutine(CutIngredientAtStation(rawMeat, cut => cutMeat = cut));
            AdvanceUIStep(); // Viande découpée
            
            if (cutMeat != null)
            {
                Ingredient cookedMeat = null;
                yield return StartCoroutine(CookMeatInPan(cutMeat, recipe, cooked => cookedMeat = cooked));
                AdvanceUIStep(); // Cuisson
                
                if (cookedMeat != null)
                {
                    if (currentIngredient != null) DropIngredient();
                    PickUpIngredient(cookedMeat);
                    MoveTo(currentPlateStation.transform);
                    yield return new WaitUntil(() => !isMoving);
                    currentPlateStation.AddIngredient(cookedMeat);
                    DropIngredient();
                }
            }
        }

        // 3. Salade → découper
        Ingredient rawLettuce = null;
        yield return StartCoroutine(FetchIngredientFromReserve(IngredientType.Lettuce, recipe.Order, fetched => rawLettuce = fetched));
        if (rawLettuce != null)
        {
            Ingredient cutLettuce = null;
            yield return StartCoroutine(CutIngredientAtStation(rawLettuce, cut => cutLettuce = cut));
            if (cutLettuce != null)
            {
                if (currentIngredient != null) DropIngredient();
                PickUpIngredient(cutLettuce);
                MoveTo(currentPlateStation.transform);
                yield return new WaitUntil(() => !isMoving);
                currentPlateStation.AddIngredient(cutLettuce);
                DropIngredient();
            }
        }
        AdvanceUIStep(); // Salade

        // 4. Tomate → découper
        Ingredient rawTomato = null;
        yield return StartCoroutine(FetchIngredientFromReserve(IngredientType.Tomato, recipe.Order, fetched => rawTomato = fetched));
        if (rawTomato != null)
        {
            Ingredient cutTomato = null;
            yield return StartCoroutine(CutIngredientAtStation(rawTomato, cut => cutTomato = cut));
            if (cutTomato != null)
            {
                if (currentIngredient != null) DropIngredient();
                PickUpIngredient(cutTomato);
                MoveTo(currentPlateStation.transform);
                yield return new WaitUntil(() => !isMoving);
                currentPlateStation.AddIngredient(cutTomato);
                DropIngredient();
            }
        }
        AdvanceUIStep(); // Tomate
    }

    private IEnumerator FetchPlateFromVaisselle(System.Action<GameObject> onFetched)
    {
        GameObject station = FindAvailableVaisselleStation();
        while (station == null)
        {
            currentState = AgentState.Waiting;
            UpdateUIStatus("waiting");
            yield return new WaitForSeconds(0.1f);
            station = FindAvailableVaisselleStation();
        }

        UpdateUIStatus("moving");
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
        return null;
    }

    private CookingStation FindFreeCookingStation()
    {
        CookingStation best = null;
        float bestDist = float.MaxValue;
        
        foreach (CookingStation station in cookingStations)
        {
            if (!station.HasUtensil() && station.IsAvailable())
            {
                float dist = Vector2.Distance(transform.position, station.transform.position);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    best = station;
                }
            }
        }
        return best;
    }

    private PlateStation FindFreePlateStation()
    {
        PlateStation best = null;
        float bestDist = float.MaxValue;
        
        foreach (PlateStation station in plateStations)
        {
            if (!station.HasPlate() && station.IsAvailable())
            {
                float dist = Vector2.Distance(transform.position, station.transform.position);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    best = station;
                }
            }
        }
        return best;
    }

    private ServeStation FindFreeServeStation()
    {
        if (serveStations.Length > 0)
        {
            return serveStations[0];
        }
        return null;
    }

    // ============================================
    // MÉTHODES PUBLIQUES
    // ============================================
    
    public static int GetActiveAgentCount()
    {
        return allAgents.Count;
    }

    public int GetRecipesCompleted()
    {
        return recipesCompleted;
    }

    public int GetAgentId()
    {
        return agentId;
    }
}

