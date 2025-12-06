using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Script de configuration automatique de la scène.
/// Configure 2 agents coopératifs avec l'interface utilisateur améliorée.
/// Attachez ce script à un GameObject vide dans votre scène.
/// </summary>
public class SceneSetup : MonoBehaviour
{
    [Header("Configuration des Agents")]
    [SerializeField] private int numberOfAgents = 2;
    [SerializeField] private Color agent1Color = new Color(0.3f, 0.7f, 1f, 1f);  // Bleu clair
    [SerializeField] private Color agent2Color = new Color(1f, 0.5f, 0.3f, 1f);  // Orange
    [SerializeField] private Vector2 agent1StartPos = new Vector2(-3f, 0f);
    [SerializeField] private Vector2 agent2StartPos = new Vector2(3f, 0f);

    [Header("Configuration du Jeu")]
    [SerializeField] private int targetRecipes = 5;
    [SerializeField] private float recipeSpawnMinDelay = 2f;
    [SerializeField] private float recipeSpawnMaxDelay = 5f;

    [Header("Références Optionnelles")]
    [SerializeField] private Sprite agentSprite;

    private void Awake()
    {
        SetupScene();
    }

    private void SetupScene()
    {
        Debug.Log("🔧 Configuration automatique de la scène...");

        // 1. Configurer ou créer l'UI Manager
        SetupUIManager();

        // 2. Configurer ou créer le Game Manager
        SetupGameManager();

        // 3. Configurer les agents
        SetupAgents();

        // 4. Créer le panneau de démarrage
        CreateStartupPanel();

        Debug.Log("✅ Configuration terminée!");
    }

    private void SetupUIManager()
    {
        // Vérifier si un ImprovedUIManager existe déjà
        ImprovedUIManager existingUI = FindFirstObjectByType<ImprovedUIManager>();
        if (existingUI == null)
        {
            GameObject uiManagerGO = new GameObject("ImprovedUIManager");
            uiManagerGO.AddComponent<ImprovedUIManager>();
            Debug.Log("   ✓ ImprovedUIManager créé");
        }

        // Vérifier si un TaskDisplayUI existe
        TaskDisplayUI existingTask = FindFirstObjectByType<TaskDisplayUI>();
        if (existingTask == null)
        {
            GameObject taskDisplayGO = new GameObject("TaskDisplayUI");
            taskDisplayGO.AddComponent<TaskDisplayUI>();
            Debug.Log("   ✓ TaskDisplayUI créé");
        }
    }

    private void SetupGameManager()
    {
        // Chercher un GameManager existant
        GameManager existingGM = FindFirstObjectByType<GameManager>();
        if (existingGM != null)
        {
            Debug.Log("   ✓ GameManager trouvé (6 recettes en 2 min)");
        }
        else
        {
            // Créer un nouveau GameManager
            GameObject gmGO = new GameObject("GameManager");
            gmGO.AddComponent<GameManager>();
            Debug.Log("   ✓ GameManager créé (6 recettes en 2 min)");
        }

        // Configurer le RecipeManager si présent
        RecipeManager rm = FindFirstObjectByType<RecipeManager>();
        if (rm != null)
        {
            rm.minDelay = recipeSpawnMinDelay;
            rm.maxDelay = recipeSpawnMaxDelay;
            Debug.Log($"   ✓ RecipeManager configuré (délai: {recipeSpawnMinDelay}-{recipeSpawnMaxDelay}s)");
        }
    }

    private void SetupAgents()
    {
        // Chercher les agents existants
        CooperativeAgent[] existingAgents = FindObjectsByType<CooperativeAgent>(FindObjectsSortMode.None);

        if (existingAgents.Length >= numberOfAgents)
        {
            // Configurer les agents existants
            for (int i = 0; i < numberOfAgents && i < existingAgents.Length; i++)
            {
                ConfigureAgent(existingAgents[i], i);
            }
            Debug.Log($"   ✓ {numberOfAgents} agents configurés");
        }
        else
        {
            Debug.Log($"   ⚠ Seulement {existingAgents.Length} agent(s) trouvé(s) sur {numberOfAgents} requis");
            Debug.Log("   💡 Ajoutez des GameObjects avec le composant CooperativeAgent dans la scène");
            
            // Configurer ceux qui existent
            for (int i = 0; i < existingAgents.Length; i++)
            {
                ConfigureAgent(existingAgents[i], i);
            }
        }
    }

    private void ConfigureAgent(CooperativeAgent agent, int index)
    {
        // Appliquer la couleur
        SpriteRenderer sr = agent.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = index == 0 ? agent1Color : agent2Color;
        }

        // Positionner
        if (index == 0)
        {
            agent.transform.position = new Vector3(agent1StartPos.x, agent1StartPos.y, 0);
        }
        else if (index == 1)
        {
            agent.transform.position = new Vector3(agent2StartPos.x, agent2StartPos.y, 0);
        }
    }

    private void CreateStartupPanel()
    {
        // Chercher ou créer un Canvas
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasGO = new GameObject("MainCanvas");
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasGO.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
            canvasGO.AddComponent<GraphicRaycaster>();
        }

        // Créer un panneau d'instructions (optionnel, visible au démarrage)
        CreateInstructionsPanel(canvas.transform);
    }

    private void CreateInstructionsPanel(Transform canvasTransform)
    {
        // Panneau d'instructions en haut à gauche
        GameObject panel = new GameObject("InstructionsPanel");
        panel.transform.SetParent(canvasTransform, false);

        RectTransform rt = panel.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(0, 1);
        rt.pivot = new Vector2(0, 1);
        rt.anchoredPosition = new Vector2(20, -20);
        rt.sizeDelta = new Vector2(350, 180);

        Image bg = panel.AddComponent<Image>();
        bg.color = new Color(0.1f, 0.1f, 0.15f, 0.85f);

        VerticalLayoutGroup vlg = panel.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(15, 15, 10, 10);
        vlg.spacing = 8;
        vlg.childAlignment = TextAnchor.UpperLeft;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        // Titre
        CreateText(panel.transform, "📋 Objectif", 24, FontStyles.Bold, Color.white);
        
        // Instructions
        CreateText(panel.transform, $"• Servez {targetRecipes} recettes", 18, FontStyles.Normal, new Color(0.9f, 0.9f, 0.9f));
        CreateText(panel.transform, "• Les agents travaillent ensemble", 18, FontStyles.Normal, new Color(0.9f, 0.9f, 0.9f));
        CreateText(panel.transform, "• Soupes: 3 ingrédients + cuisson", 16, FontStyles.Normal, new Color(0.7f, 0.7f, 0.7f));
        CreateText(panel.transform, "• Burger: pain + viande + salade + tomate", 16, FontStyles.Normal, new Color(0.7f, 0.7f, 0.7f));

        // Auto-destruction après quelques secondes
        Destroy(panel, 10f);
    }

    private void CreateText(Transform parent, string content, int fontSize, FontStyles style, Color color)
    {
        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(parent, false);
        
        TextMeshProUGUI text = textGO.AddComponent<TextMeshProUGUI>();
        text.text = content;
        text.fontSize = fontSize;
        text.fontStyle = style;
        text.color = color;
        
        RectTransform rt = textGO.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(320, fontSize + 8);
    }
}

