using UnityEngine;
using UnityEditor;

/// <summary>
/// Outil d'édition pour créer rapidement des agents coopératifs dans la scène.
/// Accessible via le menu "Tools > Agents Coopératifs"
/// </summary>
public class CooperativeAgentSetup : EditorWindow
{
    private int numberOfAgents = 2;
    private float moveSpeed = 1.2f;
    private Color agent1Color = new Color(1f, 0.9f, 0.7f); // Jaune clair
    private Color agent2Color = new Color(0.7f, 0.9f, 1f); // Bleu clair
    private Sprite agentSprite;
    private Vector3 spawnPosition1 = new Vector3(-2, 0, 0);
    private Vector3 spawnPosition2 = new Vector3(2, 0, 0);

    [MenuItem("Tools/Agents Coopératifs/Créer 2 Agents")]
    public static void ShowWindow()
    {
        GetWindow<CooperativeAgentSetup>("Agents Coopératifs");
    }

    [MenuItem("Tools/Agents Coopératifs/Création Rapide (2 agents)")]
    public static void QuickCreate()
    {
        CreateAgents(2, 4f, 
            new Color(1f, 0.9f, 0.7f), 
            new Color(0.7f, 0.9f, 1f),
            null,
            new Vector3(-2, 0, 0),
            new Vector3(2, 0, 0));
    }

    private void OnGUI()
    {
        GUILayout.Label("Configuration des Agents Coopératifs", EditorStyles.boldLabel);
        GUILayout.Space(10);

        numberOfAgents = EditorGUILayout.IntSlider("Nombre d'agents", numberOfAgents, 1, 4);
        moveSpeed = EditorGUILayout.Slider("Vitesse de déplacement", moveSpeed, 1f, 10f);
        
        GUILayout.Space(10);
        GUILayout.Label("Couleurs des agents", EditorStyles.boldLabel);
        agent1Color = EditorGUILayout.ColorField("Agent 1", agent1Color);
        if (numberOfAgents >= 2)
            agent2Color = EditorGUILayout.ColorField("Agent 2", agent2Color);

        GUILayout.Space(10);
        GUILayout.Label("Positions de spawn", EditorStyles.boldLabel);
        spawnPosition1 = EditorGUILayout.Vector3Field("Position Agent 1", spawnPosition1);
        if (numberOfAgents >= 2)
            spawnPosition2 = EditorGUILayout.Vector3Field("Position Agent 2", spawnPosition2);

        GUILayout.Space(10);
        agentSprite = (Sprite)EditorGUILayout.ObjectField("Sprite des agents (optionnel)", agentSprite, typeof(Sprite), false);

        GUILayout.Space(20);

        if (GUILayout.Button("Créer les Agents", GUILayout.Height(40)))
        {
            CreateAgents(numberOfAgents, moveSpeed, agent1Color, agent2Color, agentSprite, spawnPosition1, spawnPosition2);
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Supprimer tous les UnifiedAgent"))
        {
            RemoveUnifiedAgents();
        }

        if (GUILayout.Button("Supprimer tous les CooperativeAgent"))
        {
            RemoveCooperativeAgents();
        }
    }

    private static void CreateAgents(int count, float speed, Color color1, Color color2, Sprite sprite, Vector3 pos1, Vector3 pos2)
    {
        // Créer un parent pour organiser
        GameObject parent = new GameObject("Agents");
        Undo.RegisterCreatedObjectUndo(parent, "Create Agents Parent");

        Color[] colors = { color1, color2, Color.green, Color.magenta };
        Vector3[] positions = { pos1, pos2, new Vector3(0, 2, 0), new Vector3(0, -2, 0) };
        string[] names = { "Chef 1", "Chef 2", "Chef 3", "Chef 4" };

        for (int i = 0; i < count; i++)
        {
            GameObject agent = new GameObject(names[i]);
            agent.transform.SetParent(parent.transform);
            agent.transform.position = positions[i];

            // Ajouter SpriteRenderer
            SpriteRenderer sr = agent.AddComponent<SpriteRenderer>();
            sr.color = colors[i];
            sr.sortingOrder = 1;

            if (sprite != null)
            {
                sr.sprite = sprite;
            }
            else
            {
                // Créer un sprite par défaut (carré)
                Texture2D tex = new Texture2D(32, 32);
                Color[] pixels = new Color[32 * 32];
                for (int p = 0; p < pixels.Length; p++)
                {
                    pixels[p] = Color.white;
                }
                tex.SetPixels(pixels);
                tex.Apply();
                sr.sprite = Sprite.Create(tex, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 32);
            }

            // Ajouter le CooperativeAgent
            CooperativeAgent coopAgent = agent.AddComponent<CooperativeAgent>();
            
            // Configurer via SerializedObject pour accéder aux champs privés
            SerializedObject so = new SerializedObject(coopAgent);
            so.FindProperty("agentLabel").stringValue = names[i];
            so.FindProperty("agentColor").colorValue = colors[i];
            so.FindProperty("moveSpeed").floatValue = speed;
            so.ApplyModifiedProperties();

            Undo.RegisterCreatedObjectUndo(agent, "Create Cooperative Agent");
        }

        Debug.Log($"✅ {count} agents coopératifs créés avec succès !");
        EditorUtility.DisplayDialog("Agents Créés", 
            $"{count} agents coopératifs ont été créés.\n\nN'oublie pas de vérifier que tu as assez de stations (découpage, cuisson, assiettes) pour le travail d'équipe !", 
            "OK");
    }

    private void RemoveUnifiedAgents()
    {
        UnifiedAgent[] agents = FindObjectsByType<UnifiedAgent>(FindObjectsSortMode.None);
        int count = agents.Length;
        
        foreach (UnifiedAgent agent in agents)
        {
            Undo.DestroyObjectImmediate(agent.gameObject);
        }

        Debug.Log($"🗑️ {count} UnifiedAgent(s) supprimé(s)");
    }

    private void RemoveCooperativeAgents()
    {
        CooperativeAgent[] agents = FindObjectsByType<CooperativeAgent>(FindObjectsSortMode.None);
        int count = agents.Length;
        
        foreach (CooperativeAgent agent in agents)
        {
            Undo.DestroyObjectImmediate(agent.gameObject);
        }

        Debug.Log($"🗑️ {count} CooperativeAgent(s) supprimé(s)");
    }
}

