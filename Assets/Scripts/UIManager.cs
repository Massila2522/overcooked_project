using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private GameManager gameManager;
    private GameObject timerPanel;
    private Text timerText;
    private Text recipesText;
    private bool panelCreated = false;

    private void Start()
    {
        gameManager = FindFirstObjectByType<GameManager>();
    }

    private void Update()
    {
        if (gameManager == null) return;

        // Créer le panneau une seule fois
        if (!panelCreated)
        {
            CreatePanel();
            panelCreated = true;
        }

        // Mettre à jour les textes
        if (timerText != null && recipesText != null)
        {
            float elapsed = gameManager.GetElapsedTime();
            int served = gameManager.GetTotalRecipesServed();
            int max = gameManager.GetMaxRecipes();

            int minutes = Mathf.FloorToInt(elapsed / 60f);
            int seconds = Mathf.FloorToInt(elapsed % 60f);

            timerText.text = $"Temps: {minutes}:{seconds:D2}";
            recipesText.text = $"Recettes: {served}/{max}";

            if (gameManager.IsGameFinished())
            {
                timerText.color = Color.green;
                recipesText.color = Color.green;
            }
        }
    }

    private void CreatePanel()
    {
        // Trouver ou créer Canvas
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasGO = new GameObject("Canvas");
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();
        }

        // Désactiver tous les anciens textes sur le Canvas
        Text[] existingTexts = canvas.GetComponentsInChildren<Text>(true);
        foreach (Text t in existingTexts)
        {
            t.gameObject.SetActive(false);
        }
        
        // Désactiver aussi les TextMeshPro s'il y en a
        var tmpTexts = canvas.GetComponentsInChildren<TMPro.TextMeshProUGUI>(true);
        foreach (var t in tmpTexts)
        {
            t.gameObject.SetActive(false);
        }

        // Créer le panneau
        timerPanel = new GameObject("TimerPanel");
        timerPanel.transform.SetParent(canvas.transform, false);

        RectTransform panelRT = timerPanel.AddComponent<RectTransform>();
        panelRT.anchorMin = new Vector2(0, 0);
        panelRT.anchorMax = new Vector2(0, 0);
        panelRT.pivot = new Vector2(0, 0);
        panelRT.anchoredPosition = new Vector2(20, 20);
        panelRT.sizeDelta = new Vector2(220, 90);

        Image bg = timerPanel.AddComponent<Image>();
        bg.color = new Color(0.1f, 0.2f, 0.4f, 0.9f);

        // Texte Temps
        GameObject timeGO = new GameObject("TimeText");
        timeGO.transform.SetParent(timerPanel.transform, false);
        RectTransform timeRT = timeGO.AddComponent<RectTransform>();
        timeRT.anchorMin = new Vector2(0, 0.5f);
        timeRT.anchorMax = new Vector2(1, 1);
        timeRT.offsetMin = new Vector2(15, 5);
        timeRT.offsetMax = new Vector2(-15, -5);
        timerText = timeGO.AddComponent<Text>();
        timerText.text = "Temps: 0:00";
        timerText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        timerText.fontSize = 24;
        timerText.fontStyle = FontStyle.Bold;
        timerText.color = Color.white;

        // Texte Recettes
        GameObject recGO = new GameObject("RecipesText");
        recGO.transform.SetParent(timerPanel.transform, false);
        RectTransform recRT = recGO.AddComponent<RectTransform>();
        recRT.anchorMin = new Vector2(0, 0);
        recRT.anchorMax = new Vector2(1, 0.5f);
        recRT.offsetMin = new Vector2(15, 5);
        recRT.offsetMax = new Vector2(-15, -5);
        recipesText = recGO.AddComponent<Text>();
        recipesText.text = "Recettes: 0/6";
        recipesText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        recipesText.fontSize = 24;
        recipesText.fontStyle = FontStyle.Bold;
        recipesText.color = Color.white;
    }
}
