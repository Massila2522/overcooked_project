using UnityEngine;
using UnityEditor;

/// <summary>
/// Outil d'édition pour créer et configurer facilement une scène de cuisine.
/// Accessible depuis le menu Window > Kitchen Game > Scene Creator
/// </summary>
public class KitchenSceneCreator : EditorWindow
{
    private int numberOfAgents = 2;
    private int targetRecipes = 5;
    private Color agent1Color = new Color(0.3f, 0.7f, 1f, 1f);
    private Color agent2Color = new Color(1f, 0.5f, 0.3f, 1f);
    private Sprite agentSprite;

    [MenuItem("Window/Kitchen Game/Scene Creator")]
    public static void ShowWindow()
    {
        GetWindow<KitchenSceneCreator>("Kitchen Scene Creator");
    }

    private void OnGUI()
    {
        GUILayout.Label("🍳 Configuration de la Scène de Cuisine", EditorStyles.boldLabel);
        EditorGUILayout.Space(10);

        // Configuration des agents
        EditorGUILayout.LabelField("Agents", EditorStyles.boldLabel);
        numberOfAgents = EditorGUILayout.IntSlider("Nombre d'agents", numberOfAgents, 1, 4);
        agent1Color = EditorGUILayout.ColorField("Couleur Agent 1", agent1Color);
        if (numberOfAgents >= 2)
            agent2Color = EditorGUILayout.ColorField("Couleur Agent 2", agent2Color);
        agentSprite = (Sprite)EditorGUILayout.ObjectField("Sprite Agent", agentSprite, typeof(Sprite), false);

        EditorGUILayout.Space(10);

        // Configuration du jeu
        EditorGUILayout.LabelField("Paramètres du Jeu", EditorStyles.boldLabel);
        targetRecipes = EditorGUILayout.IntSlider("Recettes à servir", targetRecipes, 1, 20);

        EditorGUILayout.Space(20);

        // Boutons d'action
        if (GUILayout.Button("📦 Créer les Managers", GUILayout.Height(30)))
        {
            CreateManagers();
        }

        if (GUILayout.Button("👥 Créer les Agents", GUILayout.Height(30)))
        {
            CreateAgents();
        }

        if (GUILayout.Button("🎨 Appliquer les Couleurs", GUILayout.Height(30)))
        {
            ApplyAgentColors();
        }

        EditorGUILayout.Space(10);

        if (GUILayout.Button("✨ Configuration Complète", GUILayout.Height(40)))
        {
            CreateManagers();
            CreateAgents();
            ApplyAgentColors();
            EditorUtility.DisplayDialog("Succès", 
                "La scène a été configurée avec succès!\n\n" +
                $"- {numberOfAgents} agents créés\n" +
                $"- Objectif: {targetRecipes} recettes\n" +
                "- Interface utilisateur configurée", 
                "OK");
        }

        EditorGUILayout.Space(20);

        // Aide
        EditorGUILayout.HelpBox(
            "Instructions:\n" +
            "1. Assurez-vous d'avoir les stations dans la scène (Réserves, Découpage, Cuisson, etc.)\n" +
            "2. Cliquez sur 'Configuration Complète'\n" +
            "3. Lancez le jeu!", 
            MessageType.Info);
    }

    private void CreateManagers()
    {
        // GameManager
        if (FindFirstObjectByType<GameManager>() == null)
        {
            GameObject gmGO = new GameObject("GameManager");
            gmGO.AddComponent<GameManager>();
            Debug.Log("✓ GameManager créé (6 recettes en 2 min)");
        }
        else
        {
            Debug.Log("✓ GameManager existant (6 recettes en 2 min)");
        }

        // RecipeManager
        if (FindFirstObjectByType<RecipeManager>() == null)
        {
            GameObject rmGO = new GameObject("RecipeManager");
            rmGO.AddComponent<RecipeManager>();
            Debug.Log("✓ RecipeManager créé");
        }

        // ImprovedUIManager
        if (FindFirstObjectByType<ImprovedUIManager>() == null)
        {
            GameObject uiGO = new GameObject("ImprovedUIManager");
            uiGO.AddComponent<ImprovedUIManager>();
            Debug.Log("✓ ImprovedUIManager créé");
        }

        // TaskDisplayUI
        if (FindFirstObjectByType<TaskDisplayUI>() == null)
        {
            GameObject taskGO = new GameObject("TaskDisplayUI");
            taskGO.AddComponent<TaskDisplayUI>();
            Debug.Log("✓ TaskDisplayUI créé");
        }
    }

    private void CreateAgents()
    {
        // Vérifier combien d'agents existent
        CooperativeAgent[] existingAgents = FindObjectsByType<CooperativeAgent>(FindObjectsSortMode.None);
        int existingCount = existingAgents.Length;

        // Créer les agents manquants
        for (int i = existingCount; i < numberOfAgents; i++)
        {
            CreateAgent(i);
        }

        Debug.Log($"✓ {numberOfAgents} agents configurés");
    }

    private void CreateAgent(int index)
    {
        GameObject agentGO = new GameObject($"Agent {index + 1}");
        
        // Ajouter les composants
        SpriteRenderer sr = agentGO.AddComponent<SpriteRenderer>();
        if (agentSprite != null)
        {
            sr.sprite = agentSprite;
        }
        else
        {
            // Essayer de charger le sprite par défaut
            Sprite defaultSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/player/agent.png");
            if (defaultSprite != null)
            {
                sr.sprite = defaultSprite;
            }
        }
        sr.sortingOrder = 5;

        // Ajouter le script CooperativeAgent
        CooperativeAgent agent = agentGO.AddComponent<CooperativeAgent>();
        
        // Positionner
        float xPos = -3f + index * 6f;
        agentGO.transform.position = new Vector3(xPos, 0, 0);
    }

    private void ApplyAgentColors()
    {
        CooperativeAgent[] agents = FindObjectsByType<CooperativeAgent>(FindObjectsSortMode.None);
        
        // Trier par position X pour avoir un ordre cohérent
        System.Array.Sort(agents, (a, b) => a.transform.position.x.CompareTo(b.transform.position.x));

        for (int i = 0; i < agents.Length; i++)
        {
            SpriteRenderer sr = agents[i].GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                if (i == 0) sr.color = agent1Color;
                else if (i == 1) sr.color = agent2Color;
                else sr.color = Color.HSVToRGB((float)i / agents.Length, 0.7f, 1f);
            }
        }

        Debug.Log($"✓ Couleurs appliquées à {agents.Length} agents");
    }

    private T FindFirstObjectByType<T>() where T : Object
    {
        return Object.FindFirstObjectByType<T>();
    }

    private T[] FindObjectsByType<T>(FindObjectsSortMode sortMode) where T : Object
    {
        return Object.FindObjectsByType<T>(sortMode);
    }
}

