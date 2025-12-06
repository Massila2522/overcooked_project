using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Affiche un compte à rebours au démarrage du jeu, style Overcooked.
/// Se détruit automatiquement après le compte à rebours.
/// </summary>
public class CountdownUI : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private float countdownDuration = 3f;
    [SerializeField] private float textScaleAnimation = 1.5f;
    [SerializeField] private Color countdownColor = new Color(1f, 0.9f, 0.3f, 1f);
    [SerializeField] private Color goColor = new Color(0.3f, 1f, 0.4f, 1f);

    private Canvas canvas;
    private GameObject countdownPanel;
    private TextMeshProUGUI countdownText;
    private bool isCountingDown = false;

    private void Start()
    {
        CreateUI();
        StartCoroutine(RunCountdown());
    }

    private void CreateUI()
    {
        // Créer ou trouver le Canvas
        canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasGO = new GameObject("CountdownCanvas");
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 999; // Au-dessus de tout
            canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasGO.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
            canvasGO.AddComponent<GraphicRaycaster>();
        }

        // Créer le panneau de fond semi-transparent
        countdownPanel = new GameObject("CountdownPanel");
        countdownPanel.transform.SetParent(canvas.transform, false);
        
        RectTransform panelRT = countdownPanel.AddComponent<RectTransform>();
        panelRT.anchorMin = Vector2.zero;
        panelRT.anchorMax = Vector2.one;
        panelRT.offsetMin = Vector2.zero;
        panelRT.offsetMax = Vector2.zero;

        Image bgImage = countdownPanel.AddComponent<Image>();
        bgImage.color = new Color(0f, 0f, 0f, 0.7f);

        // Créer le texte du compte à rebours
        GameObject textGO = new GameObject("CountdownText");
        textGO.transform.SetParent(countdownPanel.transform, false);
        
        RectTransform textRT = textGO.AddComponent<RectTransform>();
        textRT.anchorMin = new Vector2(0.5f, 0.5f);
        textRT.anchorMax = new Vector2(0.5f, 0.5f);
        textRT.pivot = new Vector2(0.5f, 0.5f);
        textRT.anchoredPosition = Vector2.zero;
        textRT.sizeDelta = new Vector2(400, 200);

        countdownText = textGO.AddComponent<TextMeshProUGUI>();
        countdownText.text = "3";
        countdownText.fontSize = 150;
        countdownText.fontStyle = FontStyles.Bold;
        countdownText.color = countdownColor;
        countdownText.alignment = TextAlignmentOptions.Center;
        countdownText.enableAutoSizing = false;

        // Ajouter une ombre
        Shadow shadow = textGO.AddComponent<Shadow>();
        shadow.effectColor = new Color(0, 0, 0, 0.5f);
        shadow.effectDistance = new Vector2(4, -4);
    }

    private IEnumerator RunCountdown()
    {
        isCountingDown = true;

        // Pause le temps au début (optionnel)
        // Time.timeScale = 0f;

        float remaining = countdownDuration;
        int lastSecond = Mathf.CeilToInt(remaining);

        while (remaining > 0)
        {
            int currentSecond = Mathf.CeilToInt(remaining);
            
            if (currentSecond != lastSecond && currentSecond > 0)
            {
                lastSecond = currentSecond;
                countdownText.text = currentSecond.ToString();
                countdownText.color = countdownColor;
                
                // Animation de scale
                StartCoroutine(AnimateScale());
                
                Debug.Log($"⏳ {currentSecond}...");
            }

            remaining -= Time.unscaledDeltaTime;
            yield return null;
        }

        // Afficher "GO!"
        countdownText.text = "GO!";
        countdownText.color = goColor;
        countdownText.fontSize = 180;
        StartCoroutine(AnimateScale());
        
        Debug.Log("🚀 GO!");

        // Attendre un peu puis disparaître
        yield return new WaitForSecondsRealtime(0.8f);

        // Reprendre le temps
        // Time.timeScale = 1f;

        // Animation de fondu
        yield return StartCoroutine(FadeOut());

        isCountingDown = false;
        
        // Détruire ce composant et le panneau
        Destroy(countdownPanel);
        Destroy(this);
    }

    private IEnumerator AnimateScale()
    {
        if (countdownText == null) yield break;

        RectTransform rt = countdownText.rectTransform;
        float duration = 0.3f;
        float elapsed = 0f;

        // Scale up puis down
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            
            // Courbe: monte vite puis redescend
            float scale;
            if (t < 0.3f)
            {
                scale = Mathf.Lerp(1f, textScaleAnimation, t / 0.3f);
            }
            else
            {
                scale = Mathf.Lerp(textScaleAnimation, 1f, (t - 0.3f) / 0.7f);
            }
            
            rt.localScale = new Vector3(scale, scale, 1);
            yield return null;
        }

        rt.localScale = Vector3.one;
    }

    private IEnumerator FadeOut()
    {
        if (countdownPanel == null) yield break;

        Image bgImage = countdownPanel.GetComponent<Image>();
        float duration = 0.5f;
        float elapsed = 0f;
        Color startColor = bgImage.color;
        Color textStartColor = countdownText.color;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            
            // Fade le fond
            bgImage.color = new Color(startColor.r, startColor.g, startColor.b, startColor.a * (1 - t));
            
            // Fade le texte
            countdownText.color = new Color(textStartColor.r, textStartColor.g, textStartColor.b, textStartColor.a * (1 - t));
            
            // Zoom out du texte
            countdownText.rectTransform.localScale = Vector3.one * (1 + t * 0.5f);
            
            yield return null;
        }
    }

    public bool IsCountingDown()
    {
        return isCountingDown;
    }
}

