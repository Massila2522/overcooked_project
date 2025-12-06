using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Affiche les tâches/étapes en cours pour chaque agent de manière visuelle.
/// Montre les ingrédients nécessaires et l'état d'avancement.
/// </summary>
public class TaskDisplayUI : MonoBehaviour
{
    public static TaskDisplayUI Instance { get; private set; }

    [Header("Style")]
    [SerializeField] private Color taskPendingColor = new Color(0.5f, 0.5f, 0.5f, 0.8f);
    [SerializeField] private Color taskActiveColor = new Color(0.2f, 0.6f, 0.9f, 0.9f);
    [SerializeField] private Color taskCompleteColor = new Color(0.3f, 0.8f, 0.4f, 0.9f);

    private Canvas mainCanvas;
    private Dictionary<int, AgentTaskPanel> agentPanels = new Dictionary<int, AgentTaskPanel>();

    private class AgentTaskPanel
    {
        public GameObject panel;
        public TextMeshProUGUI titleText;
        public TextMeshProUGUI recipeText;
        public List<GameObject> stepIndicators = new List<GameObject>();
        public int currentStep = 0;
        public int totalSteps = 0;
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Désactivé - on utilise juste le panneau simple de UIManager
        gameObject.SetActive(false);
        return;
        
        /*
        mainCanvas = FindFirstObjectByType<Canvas>();
        if (mainCanvas == null)
        {
            Debug.LogWarning("[TaskDisplayUI] Aucun Canvas trouvé!");
        }
        */
    }

    /// <summary>
    /// Enregistre un agent et crée son panneau de tâches.
    /// </summary>
    public void RegisterAgent(int agentId, string agentName, Color agentColor)
    {
        if (mainCanvas == null) return;
        if (agentPanels.ContainsKey(agentId)) return;

        AgentTaskPanel panel = new AgentTaskPanel();
        
        // Créer le panneau principal
        panel.panel = CreateAgentPanel(agentId, agentName, agentColor);
        
        // Trouver les références
        panel.titleText = panel.panel.transform.Find("Header/Title")?.GetComponent<TextMeshProUGUI>();
        panel.recipeText = panel.panel.transform.Find("Content/RecipeName")?.GetComponent<TextMeshProUGUI>();
        
        agentPanels[agentId] = panel;
    }

    private GameObject CreateAgentPanel(int agentId, string agentName, Color agentColor)
    {
        GameObject panel = new GameObject($"TaskPanel_Agent{agentId}");
        panel.transform.SetParent(mainCanvas.transform, false);
        
        RectTransform rt = panel.AddComponent<RectTransform>();
        // Positionner en bas à droite, empilé verticalement
        rt.anchorMin = new Vector2(1, 0);
        rt.anchorMax = new Vector2(1, 0);
        rt.pivot = new Vector2(1, 0);
        rt.anchoredPosition = new Vector2(-20, 20 + agentId * 180);
        rt.sizeDelta = new Vector2(320, 160);

        Image bg = panel.AddComponent<Image>();
        bg.color = new Color(0.12f, 0.12f, 0.18f, 0.92f);

        VerticalLayoutGroup vlg = panel.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(10, 10, 8, 8);
        vlg.spacing = 5;
        vlg.childAlignment = TextAnchor.UpperLeft;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        // Header avec couleur de l'agent
        GameObject header = new GameObject("Header");
        header.transform.SetParent(panel.transform, false);
        RectTransform headerRT = header.AddComponent<RectTransform>();
        headerRT.sizeDelta = new Vector2(300, 35);
        
        HorizontalLayoutGroup hlg = header.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 10;
        hlg.childAlignment = TextAnchor.MiddleLeft;
        hlg.childControlWidth = false;
        hlg.childControlHeight = true;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = true;

        // Indicateur de couleur
        GameObject colorIndicator = new GameObject("ColorIndicator");
        colorIndicator.transform.SetParent(header.transform, false);
        Image colorImg = colorIndicator.AddComponent<Image>();
        colorImg.color = agentColor;
        RectTransform colorRT = colorIndicator.GetComponent<RectTransform>();
        colorRT.sizeDelta = new Vector2(8, 30);

        // Titre
        GameObject titleGO = new GameObject("Title");
        titleGO.transform.SetParent(header.transform, false);
        TextMeshProUGUI titleText = titleGO.AddComponent<TextMeshProUGUI>();
        titleText.text = agentName;
        titleText.fontSize = 22;
        titleText.fontStyle = FontStyles.Bold;
        titleText.color = Color.white;
        RectTransform titleRT = titleGO.AddComponent<RectTransform>();
        titleRT.sizeDelta = new Vector2(250, 30);

        // Content zone
        GameObject content = new GameObject("Content");
        content.transform.SetParent(panel.transform, false);
        RectTransform contentRT = content.AddComponent<RectTransform>();
        contentRT.sizeDelta = new Vector2(300, 110);

        VerticalLayoutGroup contentVLG = content.AddComponent<VerticalLayoutGroup>();
        contentVLG.spacing = 5;
        contentVLG.childAlignment = TextAnchor.UpperLeft;
        contentVLG.childControlWidth = true;
        contentVLG.childControlHeight = false;
        contentVLG.childForceExpandWidth = true;
        contentVLG.childForceExpandHeight = false;

        // Nom de la recette
        GameObject recipeGO = new GameObject("RecipeName");
        recipeGO.transform.SetParent(content.transform, false);
        TextMeshProUGUI recipeText = recipeGO.AddComponent<TextMeshProUGUI>();
        recipeText.text = "En attente...";
        recipeText.fontSize = 18;
        recipeText.color = new Color(0.9f, 0.7f, 0.4f, 1f);
        RectTransform recipeRT = recipeGO.AddComponent<RectTransform>();
        recipeRT.sizeDelta = new Vector2(290, 25);

        // Zone des étapes
        GameObject stepsGO = new GameObject("Steps");
        stepsGO.transform.SetParent(content.transform, false);
        RectTransform stepsRT = stepsGO.AddComponent<RectTransform>();
        stepsRT.sizeDelta = new Vector2(290, 70);

        HorizontalLayoutGroup stepsHLG = stepsGO.AddComponent<HorizontalLayoutGroup>();
        stepsHLG.spacing = 8;
        stepsHLG.childAlignment = TextAnchor.MiddleLeft;
        stepsHLG.childControlWidth = false;
        stepsHLG.childControlHeight = false;
        stepsHLG.childForceExpandWidth = false;
        stepsHLG.childForceExpandHeight = false;
        stepsHLG.padding = new RectOffset(5, 5, 5, 5);

        return panel;
    }

    /// <summary>
    /// Met à jour la tâche en cours pour un agent.
    /// </summary>
    public void SetAgentTask(int agentId, string recipeName, string[] steps, int currentStep)
    {
        if (!agentPanels.ContainsKey(agentId)) return;

        AgentTaskPanel panel = agentPanels[agentId];
        
        // Mettre à jour le nom de la recette
        if (panel.recipeText != null)
        {
            panel.recipeText.text = $"🍳 {recipeName}";
        }

        // Recréer les indicateurs d'étapes
        UpdateStepIndicators(panel, steps, currentStep);
        
        panel.currentStep = currentStep;
        panel.totalSteps = steps.Length;
    }

    private void UpdateStepIndicators(AgentTaskPanel panel, string[] steps, int currentStep)
    {
        // Nettoyer les anciens indicateurs
        foreach (GameObject indicator in panel.stepIndicators)
        {
            if (indicator != null) Destroy(indicator);
        }
        panel.stepIndicators.Clear();

        // Trouver la zone des étapes
        Transform stepsContainer = panel.panel.transform.Find("Content/Steps");
        if (stepsContainer == null) return;

        // Créer les nouveaux indicateurs
        for (int i = 0; i < steps.Length && i < 6; i++) // Max 6 étapes affichées
        {
            GameObject step = CreateStepIndicator(steps[i], i, currentStep, stepsContainer);
            panel.stepIndicators.Add(step);
        }
    }

    private GameObject CreateStepIndicator(string stepText, int index, int currentStep, Transform parent)
    {
        GameObject step = new GameObject($"Step_{index}");
        step.transform.SetParent(parent, false);
        
        RectTransform rt = step.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(45, 55);

        Image bg = step.AddComponent<Image>();
        
        // Couleur selon l'état
        if (index < currentStep)
        {
            bg.color = taskCompleteColor;
        }
        else if (index == currentStep)
        {
            bg.color = taskActiveColor;
        }
        else
        {
            bg.color = taskPendingColor;
        }

        VerticalLayoutGroup vlg = step.AddComponent<VerticalLayoutGroup>();
        vlg.childAlignment = TextAnchor.MiddleCenter;
        vlg.spacing = 2;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.padding = new RectOffset(2, 2, 3, 3);

        // Icône/Emoji
        GameObject iconGO = new GameObject("Icon");
        iconGO.transform.SetParent(step.transform, false);
        TextMeshProUGUI iconText = iconGO.AddComponent<TextMeshProUGUI>();
        iconText.text = GetStepEmoji(stepText);
        iconText.fontSize = 22;
        iconText.alignment = TextAlignmentOptions.Center;
        RectTransform iconRT = iconGO.AddComponent<RectTransform>();
        iconRT.sizeDelta = new Vector2(40, 30);

        // Numéro de l'étape
        GameObject numGO = new GameObject("Num");
        numGO.transform.SetParent(step.transform, false);
        TextMeshProUGUI numText = numGO.AddComponent<TextMeshProUGUI>();
        numText.text = (index + 1).ToString();
        numText.fontSize = 12;
        numText.color = Color.white;
        numText.alignment = TextAlignmentOptions.Center;
        RectTransform numRT = numGO.AddComponent<RectTransform>();
        numRT.sizeDelta = new Vector2(40, 15);

        return step;
    }

    private string GetStepEmoji(string stepText)
    {
        string lower = stepText.ToLower();
        
        if (lower.Contains("assiette") || lower.Contains("plate"))
            return "🍽️";
        if (lower.Contains("découp") || lower.Contains("cut"))
            return "🔪";
        if (lower.Contains("cuire") || lower.Contains("cook"))
            return "🔥";
        if (lower.Contains("oignon"))
            return "🧅";
        if (lower.Contains("tomate"))
            return "🍅";
        if (lower.Contains("champignon") || lower.Contains("mushroom"))
            return "🍄";
        if (lower.Contains("viande") || lower.Contains("meat"))
            return "🥩";
        if (lower.Contains("salade") || lower.Contains("lettuce"))
            return "🥬";
        if (lower.Contains("pain") || lower.Contains("bun"))
            return "🍞";
        if (lower.Contains("servir") || lower.Contains("serve"))
            return "🛎️";
        if (lower.Contains("marmite") || lower.Contains("pot"))
            return "🍲";
        if (lower.Contains("poêle") || lower.Contains("pan"))
            return "🍳";
            
        return "📋";
    }

    /// <summary>
    /// Avance d'une étape pour un agent.
    /// </summary>
    public void AdvanceStep(int agentId)
    {
        if (!agentPanels.ContainsKey(agentId)) return;

        AgentTaskPanel panel = agentPanels[agentId];
        if (panel.currentStep < panel.totalSteps - 1)
        {
            panel.currentStep++;
            UpdateStepColors(panel);
        }
    }

    /// <summary>
    /// Marque la tâche comme terminée pour un agent.
    /// </summary>
    public void CompleteTask(int agentId)
    {
        if (!agentPanels.ContainsKey(agentId)) return;

        AgentTaskPanel panel = agentPanels[agentId];
        
        // Marquer toutes les étapes comme complètes
        foreach (GameObject step in panel.stepIndicators)
        {
            if (step != null)
            {
                Image bg = step.GetComponent<Image>();
                if (bg != null) bg.color = taskCompleteColor;
            }
        }

        // Réinitialiser après un court délai
        StartCoroutine(ResetTaskAfterDelay(agentId, 1f));
    }

    private System.Collections.IEnumerator ResetTaskAfterDelay(int agentId, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (agentPanels.ContainsKey(agentId))
        {
            AgentTaskPanel panel = agentPanels[agentId];
            if (panel.recipeText != null)
            {
                panel.recipeText.text = "En attente...";
            }
            
            foreach (GameObject step in panel.stepIndicators)
            {
                if (step != null) Destroy(step);
            }
            panel.stepIndicators.Clear();
        }
    }

    private void UpdateStepColors(AgentTaskPanel panel)
    {
        for (int i = 0; i < panel.stepIndicators.Count; i++)
        {
            GameObject step = panel.stepIndicators[i];
            if (step == null) continue;

            Image bg = step.GetComponent<Image>();
            if (bg == null) continue;

            if (i < panel.currentStep)
            {
                bg.color = taskCompleteColor;
            }
            else if (i == panel.currentStep)
            {
                bg.color = taskActiveColor;
            }
            else
            {
                bg.color = taskPendingColor;
            }
        }
    }

    /// <summary>
    /// Définit le statut de l'agent (idle, working, etc.)
    /// </summary>
    public void SetAgentStatus(int agentId, string status)
    {
        if (!agentPanels.ContainsKey(agentId)) return;

        AgentTaskPanel panel = agentPanels[agentId];
        if (panel.titleText != null)
        {
            // Ajouter un indicateur de statut
            string emoji = "";
            switch (status.ToLower())
            {
                case "moving":
                    emoji = "🏃";
                    break;
                case "working":
                    emoji = "⚡";
                    break;
                case "waiting":
                    emoji = "⏳";
                    break;
                default:
                    emoji = "✋";
                    break;
            }
            
            // Mettre à jour le titre avec le statut
            string baseName = $"Agent {agentId + 1}";
            panel.titleText.text = $"{emoji} {baseName}";
        }
    }
}

