using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Text;
using System.Linq;

public class UIManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI blackboardText; // TextMeshPro sur le BlackBoard
    public TextMeshProUGUI timeCounterText; // Compteur de temps en bas à gauche
    public TextMeshProUGUI recipeCounterText; // Compteur de recettes en bas à gauche

    private RecipeManager recipeManager;
    private GameManager gameManager;
    private Agent[] agents;
    private CuttingStation[] cuttingStations;
    private CookingStation[] cookingStations;
    private CutIngredientsStation[] cutIngredientsStations;
    private PlateStation[] plateStations;
    
    private float gameStartTime;

    private void Start()
    {
        recipeManager = FindFirstObjectByType<RecipeManager>();
        gameManager = FindFirstObjectByType<GameManager>();
        
        // Trouver tous les agents et stations
        agents = FindObjectsByType<Agent>(FindObjectsSortMode.None);
        cuttingStations = FindObjectsByType<CuttingStation>(FindObjectsSortMode.None);
        cookingStations = FindObjectsByType<CookingStation>(FindObjectsSortMode.None);
        cutIngredientsStations = FindObjectsByType<CutIngredientsStation>(FindObjectsSortMode.None);
        plateStations = FindObjectsByType<PlateStation>(FindObjectsSortMode.None);
        
        // Initialiser le compteur de temps
        gameStartTime = Time.time;
    }

    private void Update()
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        // Mettre à jour le compteur de temps (indépendant des managers)
        if (timeCounterText != null)
        {
            float elapsedTime = Time.time - gameStartTime;
            int minutes = Mathf.FloorToInt(elapsedTime / 60f);
            int seconds = Mathf.FloorToInt(elapsedTime % 60f);
            timeCounterText.text = $"Temps: {minutes}:{seconds:D2}";
        }
        
        // Mettre à jour le compteur de recettes
        if (recipeCounterText != null)
        {
            if (gameManager != null)
            {
                int recipesServed = gameManager.GetTotalRecipesServed();
                recipeCounterText.text = $"Recettes: {recipesServed}";
            }
            else
            {
                recipeCounterText.text = "Recettes: 0";
            }
        }

        // Mettre à jour le blackboard (nécessite les managers)
        if (recipeManager == null || gameManager == null) return;

        if (blackboardText != null)
        {
            blackboardText.text = GetBlackboardText();
        }
    }

    private string GetBlackboardText()
    {
        StringBuilder sb = new StringBuilder();
        
        // === RECETTES EN ATTENTE ===
        sb.AppendLine("<size=18><b>[RECETTES EN ATTENTE]</b></size>");
        List<Recipe> recipes = recipeManager.GetAllRecipes();
        List<Recipe> pendingRecipes = recipes.Where(r => !r.IsReserved).ToList();

        if (pendingRecipes.Count == 0)
        {
            sb.AppendLine("<color=#888>Aucune recette en attente</color>");
        }
        else
        {
            for (int i = 0; i < Mathf.Min(pendingRecipes.Count, 5); i++) // Afficher max 5 recettes
            {
                Recipe recipe = pendingRecipes[i];
                sb.AppendLine($"  {i + 1}. {recipe.GetDisplayName()}");
            }
            if (pendingRecipes.Count > 5)
            {
                sb.AppendLine($"  ... et {pendingRecipes.Count - 5} autres");
            }
        }
        sb.AppendLine();
        
        // === INGRÉDIENTS EN ATTENTE ===
        sb.AppendLine("<size=18><b>[INGREDIENTS A TRAITER]</b></size>");
        int ingredientCount = recipeManager.GetIngredientQueueCount();
        sb.AppendLine($"Total : {ingredientCount} ingredients");
        sb.AppendLine();
        
        // === ÉTAT DES STATIONS ===
        sb.AppendLine("<size=18><b>[STATIONS DE DECOUPAGE]</b></size>");
        int cuttingBusy = cuttingStations.Count(s => s.IsOccupied || s.HasIngredient());
        sb.AppendLine($"{cuttingBusy}/{cuttingStations.Length} occupees");
        sb.AppendLine();
        
        sb.AppendLine("<size=18><b>[INGREDIENTS DECOUPES]</b></size>");
        int cutIngredientsCount = cutIngredientsStations.Sum(s => s.QueueCount());
        sb.AppendLine($"{cutIngredientsCount} ingredients prets");
        sb.AppendLine();
        
        sb.AppendLine("<size=18><b>[PLAQUES DE CUISSON]</b></size>");
        int cookingBusy = cookingStations.Count(s => s.HasUtensil() || s.IsOccupied);
        sb.AppendLine($"{cookingBusy}/{cookingStations.Length} occupees");
        sb.AppendLine();
        
        sb.AppendLine("<size=18><b>[PLACES D'ASSIETTES]</b></size>");
        int plateBusy = plateStations.Count(s => s.HasPlate());
        sb.AppendLine($"{plateBusy}/{plateStations.Length} occupees");
        sb.AppendLine();
        
        // === STATISTIQUES ===
        sb.AppendLine("<size=18><b>[STATISTIQUES]</b></size>");
        sb.AppendLine($"Recettes servies : <b>{gameManager.GetTotalRecipesServed()}</b>");
        sb.AppendLine();
        
        // === ÉTAT DES AGENTS ===
        sb.AppendLine("<size=18><b>[AGENTS]</b></size>");
        if (agents != null && agents.Length > 0)
        {
            for (int i = 0; i < agents.Length; i++)
            {
                string agentName = $"Agent {i + 1}";
                string status = GetAgentStatus(agents[i]);
                sb.AppendLine($"{agentName}: {status}");
            }
        }
        
        return sb.ToString();
    }

    private string GetAgentStatus(Agent agent)
    {
        if (agent == null) return "<color=#888>Inactif</color>";
        
        // Vérifier l'état de l'agent (simplifié)
        if (agent.currentIngredient != null)
        {
            return $"<color=#ffaa00>Porte {GetIngredientName(agent.currentIngredient.Type)}</color>";
        }
        if (agent.currentPlate != null)
        {
            return "<color=#00aaff>Porte assiette</color>";
        }
        return "<color=#88ff88>Libre</color>";
    }

    private string GetIngredientName(IngredientType type)
    {
        switch (type)
        {
            case IngredientType.Onion: return "oignon";
            case IngredientType.Tomato: return "tomate";
            case IngredientType.Mushroom: return "champignon";
            case IngredientType.Lettuce: return "salade";
            case IngredientType.Meat: return "viande";
            case IngredientType.BurgerBun: return "pain";
            default: return type.ToString();
        }
    }
}

