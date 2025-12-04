using UnityEngine;
using UnityEditor;
using System.Linq;

public class AgentConverter : EditorWindow
{
    [MenuItem("Tools/Convert to Unified Agent")]
    public static void ShowWindow()
    {
        GetWindow<AgentConverter>("Agent Converter");
    }

    private void OnGUI()
    {
        GUILayout.Label("Conversion des Agents", EditorStyles.boldLabel);
        GUILayout.Space(10);

        // Compter les agents existants
        var ingredientProviders = Object.FindObjectsOfType<IngredientProviderAgent>();
        var cuttingAgents = Object.FindObjectsOfType<CuttingAgent>();
        var dressingAgents = Object.FindObjectsOfType<DressingAgent>();
        var unifiedAgents = Object.FindObjectsOfType<UnifiedAgent>();

        int totalOldAgents = ingredientProviders.Length + cuttingAgents.Length + dressingAgents.Length;

        GUILayout.Label($"Agents trouvés dans la scène :", EditorStyles.label);
        GUILayout.Label($"  - IngredientProviderAgent: {ingredientProviders.Length}");
        GUILayout.Label($"  - CuttingAgent: {cuttingAgents.Length}");
        GUILayout.Label($"  - DressingAgent: {dressingAgents.Length}");
        GUILayout.Label($"  - UnifiedAgent: {unifiedAgents.Length}");
        GUILayout.Space(10);

        if (totalOldAgents == 0 && unifiedAgents.Length > 0)
        {
            EditorGUILayout.HelpBox("✓ La scène contient déjà uniquement des UnifiedAgent !", MessageType.Info);
            return;
        }

        if (totalOldAgents == 0 && unifiedAgents.Length == 0)
        {
            EditorGUILayout.HelpBox("Aucun agent trouvé dans la scène. Créez un UnifiedAgent manuellement.", MessageType.Warning);
            if (GUILayout.Button("Créer un UnifiedAgent"))
            {
                CreateUnifiedAgent();
            }
            return;
        }

        EditorGUILayout.HelpBox(
            $"Cette opération va :\n" +
            $"1. Créer un UnifiedAgent\n" +
            $"2. Supprimer tous les anciens agents ({totalOldAgents})\n" +
            $"3. Conserver le premier agent comme référence pour la position",
            MessageType.Info
        );

        GUILayout.Space(10);

        if (GUILayout.Button("Convertir vers UnifiedAgent", GUILayout.Height(30)))
        {
            ConvertToUnifiedAgent();
        }

        GUILayout.Space(10);
        EditorGUILayout.HelpBox("⚠️ Assurez-vous d'avoir sauvegardé votre scène avant de convertir !", MessageType.Warning);
    }

    private void ConvertToUnifiedAgent()
    {
        // Trouver tous les anciens agents
        var ingredientProviders = Object.FindObjectsOfType<IngredientProviderAgent>();
        var cuttingAgents = Object.FindObjectsOfType<CuttingAgent>();
        var dressingAgents = Object.FindObjectsOfType<DressingAgent>();
        var existingUnified = Object.FindObjectsOfType<UnifiedAgent>();

        // Si un UnifiedAgent existe déjà, le supprimer aussi pour en créer un nouveau propre
        foreach (var agent in existingUnified)
        {
            Undo.DestroyObjectImmediate(agent.gameObject);
        }

        // Récupérer les paramètres du premier agent trouvé (pour préserver position, vitesse, etc.)
        Agent firstAgent = null;
        Vector3 position = Vector3.zero;
        float moveSpeed = 3f;
        string agentLabel = "Unified Agent";

        if (ingredientProviders.Length > 0)
        {
            firstAgent = ingredientProviders[0];
        }
        else if (cuttingAgents.Length > 0)
        {
            firstAgent = cuttingAgents[0];
        }
        else if (dressingAgents.Length > 0)
        {
            firstAgent = dressingAgents[0];
        }

        if (firstAgent != null)
        {
            position = firstAgent.transform.position;
            moveSpeed = firstAgent.moveSpeed;
            if (!string.IsNullOrEmpty(firstAgent.GetType().GetField("agentLabel", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(firstAgent) as string))
            {
                // Essayer de récupérer le label via reflection si possible
            }
        }

        // Créer le nouveau UnifiedAgent
        GameObject unifiedAgentGO = new GameObject("UnifiedAgent");
        unifiedAgentGO.transform.position = position;
        
        UnifiedAgent unifiedAgent = unifiedAgentGO.AddComponent<UnifiedAgent>();
        unifiedAgent.moveSpeed = moveSpeed;

        // Copier le SpriteRenderer si le premier agent en avait un
        if (firstAgent != null)
        {
            SpriteRenderer oldSR = firstAgent.GetComponent<SpriteRenderer>();
            if (oldSR != null)
            {
                SpriteRenderer newSR = unifiedAgentGO.AddComponent<SpriteRenderer>();
                newSR.sprite = oldSR.sprite;
                newSR.sortingOrder = oldSR.sortingOrder;
                newSR.color = oldSR.color;
            }
        }

        Undo.RegisterCreatedObjectUndo(unifiedAgentGO, "Create UnifiedAgent");

        // Supprimer tous les anciens agents
        int deletedCount = 0;
        foreach (var agent in ingredientProviders)
        {
            Undo.DestroyObjectImmediate(agent.gameObject);
            deletedCount++;
        }
        foreach (var agent in cuttingAgents)
        {
            Undo.DestroyObjectImmediate(agent.gameObject);
            deletedCount++;
        }
        foreach (var agent in dressingAgents)
        {
            Undo.DestroyObjectImmediate(agent.gameObject);
            deletedCount++;
        }

        // Sélectionner le nouvel agent
        Selection.activeGameObject = unifiedAgentGO;

        Debug.Log($"✓ Conversion terminée : {deletedCount} ancien(s) agent(s) supprimé(s), 1 UnifiedAgent créé.");
        EditorUtility.DisplayDialog("Conversion terminée", 
            $"Conversion réussie !\n\n" +
            $"- {deletedCount} ancien(s) agent(s) supprimé(s)\n" +
            $"- 1 UnifiedAgent créé à la position {position}\n\n" +
            $"N'oubliez pas de sauvegarder la scène !", 
            "OK");
    }

    private void CreateUnifiedAgent()
    {
        GameObject unifiedAgentGO = new GameObject("UnifiedAgent");
        unifiedAgentGO.transform.position = Vector3.zero;
        
        UnifiedAgent unifiedAgent = unifiedAgentGO.AddComponent<UnifiedAgent>();
        unifiedAgent.moveSpeed = 3f;

        Undo.RegisterCreatedObjectUndo(unifiedAgentGO, "Create UnifiedAgent");
        Selection.activeGameObject = unifiedAgentGO;

        Debug.Log("✓ UnifiedAgent créé.");
        EditorUtility.DisplayDialog("UnifiedAgent créé", 
            "Un UnifiedAgent a été créé dans la scène.\n" +
            "Positionnez-le et configurez-le dans l'inspecteur.", 
            "OK");
    }
}

