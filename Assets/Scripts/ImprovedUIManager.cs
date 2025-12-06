using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// UI Manager amélioré avec un style moderne inspiré d'Overcooked.
/// Affiche le timer, le compteur de recettes et les tâches en cours.
/// </summary>
public class ImprovedUIManager : MonoBehaviour
{
    public static ImprovedUIManager Instance { get; private set; }

    [Header("Références Automatiques")]
    private Canvas mainCanvas;
    private GameManager gameManager;
    private RecipeManager recipeManager;

    [Header("Paramètres de Style")]
    [SerializeField] private Color panelColor = new Color(0.15f, 0.15f, 0.2f, 0.9f);
    [SerializeField] private Color accentColor = new Color(1f, 0.6f, 0.2f, 1f); // Orange vif
    [SerializeField] private Color successColor = new Color(0.3f, 0.9f, 0.4f, 1f);
    [SerializeField] private Color warningColor = new Color(1f, 0.9f, 0.3f, 1f);
    [SerializeField] private Color dangerColor = new Color(1f, 0.3f, 0.3f, 1f);

    // Éléments UI générés
    private GameObject timerPanel;
    private TextMeshProUGUI timerText;
    private TextMeshProUGUI recipesText;
    private GameObject recipeQueuePanel;
    private List<GameObject> recipeCards = new List<GameObject>();
    private GameObject agentStatusPanel;
    private Dictionary<CooperativeAgent, GameObject> agentCards = new Dictionary<CooperativeAgent, GameObject>();
    private GameObject gameOverPanel;
    private TextMeshProUGUI gameOverText;
    private TextMeshProUGUI finalTimeText;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        // Désactivé - on utilise UIManager à la place
        gameObject.SetActive(false);
        return;
        
        /*
        gameManager = FindFirstObjectByType<GameManager>();
        recipeManager = FindFirstObjectByType<RecipeManager>();
        
        CreateUI();
        StartCoroutine(UpdateUILoop());
        */
    }

    private void CreateUI()
    {
        // Créer le Canvas principal s'il n'existe pas
        mainCanvas = FindFirstObjectByType<Canvas>();
        if (mainCanvas == null)
        {
            GameObject canvasGO = new GameObject("MainCanvas");
            mainCanvas = canvasGO.AddComponent<Canvas>();
            mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            mainCanvas.sortingOrder = 100;
            canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasGO.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
            canvasGO.AddComponent<GraphicRaycaster>();
        }

        CreateTimerPanel();
        CreateRecipeQueuePanel();
        CreateAgentStatusPanel();
        CreateGameOverPanel();
    }

    private void CreateTimerPanel()
    {
        // Panneau principal en haut à droite
        timerPanel = CreatePanel("TimerPanel", mainCanvas.transform);
        RectTransform rt = timerPanel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(1, 1);
        rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(1, 1);
        rt.anchoredPosition = new Vector2(-20, -20);
        rt.sizeDelta = new Vector2(280, 120);

        // Layout vertical
        VerticalLayoutGroup vlg = timerPanel.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(15, 15, 10, 10);
        vlg.spacing = 5;
        vlg.childAlignment = TextAnchor.MiddleCenter;
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        // Icône et Timer
        GameObject timerRow = new GameObject("TimerRow");
        timerRow.transform.SetParent(timerPanel.transform, false);
        HorizontalLayoutGroup hlg = timerRow.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 10;
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.childControlWidth = false;
        hlg.childControlHeight = false;
        RectTransform timerRowRT = timerRow.AddComponent<RectTransform>();
        timerRowRT.sizeDelta = new Vector2(250, 45);

        // Icône horloge (emoji texte)
        GameObject clockIcon = new GameObject("ClockIcon");
        clockIcon.transform.SetParent(timerRow.transform, false);
        TextMeshProUGUI clockText = clockIcon.AddComponent<TextMeshProUGUI>();
        clockText.text = "⏱";
        clockText.fontSize = 32;
        clockText.alignment = TextAlignmentOptions.Center;
        RectTransform clockRT = clockIcon.GetComponent<RectTransform>();
        clockRT.sizeDelta = new Vector2(40, 45);

        // Texte Timer
        GameObject timerTextGO = new GameObject("TimerText");
        timerTextGO.transform.SetParent(timerRow.transform, false);
        timerText = timerTextGO.AddComponent<TextMeshProUGUI>();
        timerText.text = "Temps: 0:00";
        timerText.fontSize = 36;
        timerText.fontStyle = FontStyles.Bold;
        timerText.color = Color.white;
        timerText.alignment = TextAlignmentOptions.Left;
        RectTransform timerTextRT = timerTextGO.GetComponent<RectTransform>();
        timerTextRT.sizeDelta = new Vector2(200, 45);

        // Compteur de recettes
        GameObject recipesRow = new GameObject("RecipesRow");
        recipesRow.transform.SetParent(timerPanel.transform, false);
        RectTransform recipesRowRT = recipesRow.AddComponent<RectTransform>();
        recipesRowRT.sizeDelta = new Vector2(250, 40);

        recipesText = recipesRow.AddComponent<TextMeshProUGUI>();
        recipesText.text = "🍽 Recettes: 0/5";
        recipesText.fontSize = 28;
        recipesText.fontStyle = FontStyles.Bold;
        recipesText.color = accentColor;
        recipesText.alignment = TextAlignmentOptions.Center;
    }

    private void CreateRecipeQueuePanel()
    {
        // Panneau des recettes en attente (en haut au centre)
        recipeQueuePanel = CreatePanel("RecipeQueuePanel", mainCanvas.transform);
        RectTransform rt = recipeQueuePanel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 1);
        rt.anchorMax = new Vector2(0.5f, 1);
        rt.pivot = new Vector2(0.5f, 1);
        rt.anchoredPosition = new Vector2(0, -20);
        rt.sizeDelta = new Vector2(600, 100);

        // Titre
        GameObject title = new GameObject("Title");
        title.transform.SetParent(recipeQueuePanel.transform, false);
        TextMeshProUGUI titleText = title.AddComponent<TextMeshProUGUI>();
        titleText.text = "📋 Commandes en attente";
        titleText.fontSize = 20;
        titleText.fontStyle = FontStyles.Bold;
        titleText.color = Color.white;
        titleText.alignment = TextAlignmentOptions.Center;
        RectTransform titleRT = title.GetComponent<RectTransform>();
        titleRT.anchorMin = new Vector2(0, 1);
        titleRT.anchorMax = new Vector2(1, 1);
        titleRT.pivot = new Vector2(0.5f, 1);
        titleRT.anchoredPosition = new Vector2(0, -5);
        titleRT.sizeDelta = new Vector2(0, 25);

        // Container pour les cartes de recettes
        GameObject cardsContainer = new GameObject("CardsContainer");
        cardsContainer.transform.SetParent(recipeQueuePanel.transform, false);
        RectTransform cardsRT = cardsContainer.AddComponent<RectTransform>();
        cardsRT.anchorMin = new Vector2(0, 0);
        cardsRT.anchorMax = new Vector2(1, 1);
        cardsRT.offsetMin = new Vector2(10, 10);
        cardsRT.offsetMax = new Vector2(-10, -30);

        HorizontalLayoutGroup hlg = cardsContainer.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 10;
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.childControlWidth = false;
        hlg.childControlHeight = true;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = true;
    }

    private void CreateAgentStatusPanel()
    {
        // Panneau des agents (en bas à gauche)
        agentStatusPanel = CreatePanel("AgentStatusPanel", mainCanvas.transform);
        RectTransform rt = agentStatusPanel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 0);
        rt.anchorMax = new Vector2(0, 0);
        rt.pivot = new Vector2(0, 0);
        rt.anchoredPosition = new Vector2(20, 20);
        rt.sizeDelta = new Vector2(300, 150);

        VerticalLayoutGroup vlg = agentStatusPanel.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(10, 10, 10, 10);
        vlg.spacing = 8;
        vlg.childAlignment = TextAnchor.UpperLeft;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        // Titre
        GameObject title = new GameObject("Title");
        title.transform.SetParent(agentStatusPanel.transform, false);
        TextMeshProUGUI titleText = title.AddComponent<TextMeshProUGUI>();
        titleText.text = "👥 Agents";
        titleText.fontSize = 22;
        titleText.fontStyle = FontStyles.Bold;
        titleText.color = Color.white;
        titleText.alignment = TextAlignmentOptions.Left;
        RectTransform titleRT = title.AddComponent<RectTransform>();
        titleRT.sizeDelta = new Vector2(280, 30);
    }

    private void CreateGameOverPanel()
    {
        gameOverPanel = CreatePanel("GameOverPanel", mainCanvas.transform);
        RectTransform rt = gameOverPanel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(500, 300);

        // Changer la couleur de fond pour le game over
        Image img = gameOverPanel.GetComponent<Image>();
        img.color = new Color(0.1f, 0.1f, 0.15f, 0.95f);

        VerticalLayoutGroup vlg = gameOverPanel.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(30, 30, 30, 30);
        vlg.spacing = 20;
        vlg.childAlignment = TextAnchor.MiddleCenter;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        // Titre "TERMINÉ"
        GameObject titleGO = new GameObject("Title");
        titleGO.transform.SetParent(gameOverPanel.transform, false);
        gameOverText = titleGO.AddComponent<TextMeshProUGUI>();
        gameOverText.text = "🏆 MISSION ACCOMPLIE! 🏆";
        gameOverText.fontSize = 42;
        gameOverText.fontStyle = FontStyles.Bold;
        gameOverText.color = successColor;
        gameOverText.alignment = TextAlignmentOptions.Center;
        RectTransform titleRT = titleGO.AddComponent<RectTransform>();
        titleRT.sizeDelta = new Vector2(440, 60);

        // Temps final
        GameObject timeGO = new GameObject("FinalTime");
        timeGO.transform.SetParent(gameOverPanel.transform, false);
        finalTimeText = timeGO.AddComponent<TextMeshProUGUI>();
        finalTimeText.text = "Temps: 0:00.000";
        finalTimeText.fontSize = 36;
        finalTimeText.color = Color.white;
        finalTimeText.alignment = TextAlignmentOptions.Center;
        RectTransform timeRT = timeGO.AddComponent<RectTransform>();
        timeRT.sizeDelta = new Vector2(440, 50);

        // Message de félicitations
        GameObject msgGO = new GameObject("Message");
        msgGO.transform.SetParent(gameOverPanel.transform, false);
        TextMeshProUGUI msgText = msgGO.AddComponent<TextMeshProUGUI>();
        msgText.text = "Excellente coordination!\nLes agents ont travaillé en équipe parfaitement.";
        msgText.fontSize = 22;
        msgText.color = new Color(0.8f, 0.8f, 0.8f, 1f);
        msgText.alignment = TextAlignmentOptions.Center;
        RectTransform msgRT = msgGO.AddComponent<RectTransform>();
        msgRT.sizeDelta = new Vector2(440, 80);

        gameOverPanel.SetActive(false);
    }

    private GameObject CreatePanel(string name, Transform parent)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);
        
        RectTransform rt = panel.AddComponent<RectTransform>();
        
        Image img = panel.AddComponent<Image>();
        img.color = panelColor;
        
        // Ajouter un contour subtil
        Outline outline = panel.AddComponent<Outline>();
        outline.effectColor = new Color(1f, 1f, 1f, 0.2f);
        outline.effectDistance = new Vector2(2, -2);

        return panel;
    }

    private IEnumerator UpdateUILoop()
    {
        while (true)
        {
            UpdateTimerDisplay();
            UpdateRecipeQueue();
            UpdateAgentStatus();
            CheckGameOver();
            
            yield return new WaitForSeconds(0.1f);
        }
    }

    private void UpdateTimerDisplay()
    {
        if (gameManager == null) return;

        string time = gameManager.GetFormattedTime();
        int served = gameManager.GetTotalRecipesServed();
        int max = gameManager.GetMaxRecipes();

        timerText.text = $"Temps: {time}";
        recipesText.text = $"🍽 Recettes: {served}/{max}";

        // Changer la couleur selon la progression
        float progress = (float)served / max;
        if (progress >= 1f)
        {
            recipesText.color = successColor;
        }
        else if (progress >= 0.6f)
        {
            recipesText.color = warningColor;
        }
        else
        {
            recipesText.color = accentColor;
        }
    }

    private void UpdateRecipeQueue()
    {
        if (recipeManager == null) return;

        // Trouver le container des cartes
        Transform cardsContainer = recipeQueuePanel.transform.Find("CardsContainer");
        if (cardsContainer == null) return;

        // Nettoyer les anciennes cartes
        foreach (GameObject card in recipeCards)
        {
            if (card != null) Destroy(card);
        }
        recipeCards.Clear();

        // Créer les nouvelles cartes
        List<Recipe> recipes = recipeManager.GetAllRecipes();
        int displayCount = Mathf.Min(recipes.Count, 5);

        for (int i = 0; i < displayCount; i++)
        {
            Recipe recipe = recipes[i];
            GameObject card = CreateRecipeCard(recipe, cardsContainer);
            recipeCards.Add(card);
        }

        // Afficher le nombre restant si > 5
        if (recipes.Count > 5)
        {
            GameObject moreCard = new GameObject("MoreCard");
            moreCard.transform.SetParent(cardsContainer, false);
            RectTransform rt = moreCard.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(60, 60);

            Image img = moreCard.AddComponent<Image>();
            img.color = new Color(0.3f, 0.3f, 0.4f, 0.8f);

            TextMeshProUGUI text = moreCard.AddComponent<TextMeshProUGUI>();
            text.text = $"+{recipes.Count - 5}";
            text.fontSize = 24;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Center;

            recipeCards.Add(moreCard);
        }
    }

    private GameObject CreateRecipeCard(Recipe recipe, Transform parent)
    {
        GameObject card = new GameObject($"RecipeCard_{recipe.Order}");
        card.transform.SetParent(parent, false);
        
        RectTransform rt = card.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(100, 60);

        // Fond de la carte
        Image bgImg = card.AddComponent<Image>();
        if (recipe.IsReserved)
        {
            bgImg.color = new Color(0.2f, 0.4f, 0.3f, 0.9f); // Vert si réservé
        }
        else
        {
            bgImg.color = new Color(0.3f, 0.25f, 0.2f, 0.9f); // Brun sinon
        }

        // Contenu vertical
        VerticalLayoutGroup vlg = card.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(5, 5, 3, 3);
        vlg.spacing = 2;
        vlg.childAlignment = TextAnchor.MiddleCenter;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        // Icône du plat
        GameObject iconGO = new GameObject("Icon");
        iconGO.transform.SetParent(card.transform, false);
        TextMeshProUGUI iconText = iconGO.AddComponent<TextMeshProUGUI>();
        iconText.text = GetRecipeEmoji(recipe.Type);
        iconText.fontSize = 28;
        iconText.alignment = TextAlignmentOptions.Center;
        RectTransform iconRT = iconGO.AddComponent<RectTransform>();
        iconRT.sizeDelta = new Vector2(90, 35);

        // Nom court
        GameObject nameGO = new GameObject("Name");
        nameGO.transform.SetParent(card.transform, false);
        TextMeshProUGUI nameText = nameGO.AddComponent<TextMeshProUGUI>();
        nameText.text = GetShortRecipeName(recipe.Type);
        nameText.fontSize = 12;
        nameText.color = Color.white;
        nameText.alignment = TextAlignmentOptions.Center;
        RectTransform nameRT = nameGO.AddComponent<RectTransform>();
        nameRT.sizeDelta = new Vector2(90, 18);

        return card;
    }

    private string GetRecipeEmoji(RecipeType type)
    {
        switch (type)
        {
            case RecipeType.OnionSoup:
                return "🧅🍲";
            case RecipeType.TomatoSoup:
                return "🍅🍲";
            case RecipeType.MushroomSoup:
                return "🍄🍲";
            case RecipeType.Burger:
                return "🍔";
            default:
                return "❓";
        }
    }

    private string GetShortRecipeName(RecipeType type)
    {
        switch (type)
        {
            case RecipeType.OnionSoup:
                return "Oignon";
            case RecipeType.TomatoSoup:
                return "Tomate";
            case RecipeType.MushroomSoup:
                return "Champi";
            case RecipeType.Burger:
                return "Burger";
            default:
                return "???";
        }
    }

    private void UpdateAgentStatus()
    {
        CooperativeAgent[] agents = FindObjectsByType<CooperativeAgent>(FindObjectsSortMode.None);
        
        foreach (CooperativeAgent agent in agents)
        {
            if (!agentCards.ContainsKey(agent))
            {
                // Créer une nouvelle carte pour cet agent
                GameObject card = CreateAgentCard(agent);
                agentCards[agent] = card;
            }
            else
            {
                // Mettre à jour la carte existante
                UpdateAgentCard(agent, agentCards[agent]);
            }
        }

        // Supprimer les cartes des agents détruits
        List<CooperativeAgent> toRemove = new List<CooperativeAgent>();
        foreach (var kvp in agentCards)
        {
            if (kvp.Key == null)
            {
                if (kvp.Value != null) Destroy(kvp.Value);
                toRemove.Add(kvp.Key);
            }
        }
        foreach (var agent in toRemove)
        {
            agentCards.Remove(agent);
        }
    }

    private GameObject CreateAgentCard(CooperativeAgent agent)
    {
        GameObject card = new GameObject($"AgentCard_{agent.GetAgentId()}");
        card.transform.SetParent(agentStatusPanel.transform, false);
        
        RectTransform rt = card.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(280, 40);

        HorizontalLayoutGroup hlg = card.AddComponent<HorizontalLayoutGroup>();
        hlg.padding = new RectOffset(5, 5, 5, 5);
        hlg.spacing = 10;
        hlg.childAlignment = TextAnchor.MiddleLeft;
        hlg.childControlWidth = false;
        hlg.childControlHeight = true;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = true;

        // Couleur de l'agent
        GameObject colorDot = new GameObject("ColorDot");
        colorDot.transform.SetParent(card.transform, false);
        Image dotImg = colorDot.AddComponent<Image>();
        SpriteRenderer sr = agent.GetComponent<SpriteRenderer>();
        dotImg.color = sr != null ? sr.color : Color.white;
        RectTransform dotRT = colorDot.GetComponent<RectTransform>();
        dotRT.sizeDelta = new Vector2(20, 20);

        // Nom et stats
        GameObject infoGO = new GameObject("Info");
        infoGO.transform.SetParent(card.transform, false);
        TextMeshProUGUI infoText = infoGO.AddComponent<TextMeshProUGUI>();
        infoText.text = $"Agent {agent.GetAgentId() + 1}: 0 recettes";
        infoText.fontSize = 18;
        infoText.color = Color.white;
        RectTransform infoRT = infoGO.AddComponent<RectTransform>();
        infoRT.sizeDelta = new Vector2(200, 30);

        return card;
    }

    private void UpdateAgentCard(CooperativeAgent agent, GameObject card)
    {
        if (card == null || agent == null) return;

        Transform infoTransform = card.transform.Find("Info");
        if (infoTransform != null)
        {
            TextMeshProUGUI infoText = infoTransform.GetComponent<TextMeshProUGUI>();
            if (infoText != null)
            {
                int completed = agent.GetRecipesCompleted();
                string status = agent.isMoving ? "🏃" : "✋";
                infoText.text = $"{status} Agent {agent.GetAgentId() + 1}: {completed} recettes";
            }
        }
    }

    private void CheckGameOver()
    {
        if (gameManager != null && gameManager.IsGameFinished())
        {
            if (!gameOverPanel.activeSelf)
            {
                ShowGameOver();
            }
        }
    }

    private void ShowGameOver()
    {
        gameOverPanel.SetActive(true);
        
        string finalTime = gameManager.GetFormattedTimeWithMs();
        finalTimeText.text = $"⏱ Temps final: {finalTime}";

        // Calculer les statistiques
        CooperativeAgent[] agents = FindObjectsByType<CooperativeAgent>(FindObjectsSortMode.None);
        int totalRecipes = 0;
        foreach (var agent in agents)
        {
            totalRecipes += agent.GetRecipesCompleted();
        }

        gameOverText.text = $"🏆 MISSION ACCOMPLIE! 🏆\n{agents.Length} agents - {totalRecipes} recettes";

        // Animation de victoire
        StartCoroutine(VictoryAnimation());
    }

    private IEnumerator VictoryAnimation()
    {
        RectTransform rt = gameOverPanel.GetComponent<RectTransform>();
        float startScale = 0.5f;
        float endScale = 1f;
        float duration = 0.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float scale = Mathf.Lerp(startScale, endScale, Mathf.SmoothStep(0, 1, t));
            rt.localScale = new Vector3(scale, scale, 1);
            yield return null;
        }

        rt.localScale = Vector3.one;
    }
}

