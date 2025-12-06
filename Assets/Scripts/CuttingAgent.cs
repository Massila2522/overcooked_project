using UnityEngine;
using System.Collections;

public class CuttingAgent : Agent
{
    private CuttingStation[] cuttingStations;
    private CutIngredientsStation[] cutIngredientsStations;

    protected override void Start()
    {
        // Si agentLabel n'est pas défini dans l'inspecteur, utiliser une valeur par défaut
        if (string.IsNullOrEmpty(agentLabel))
        {
            agentLabel = "Cutting Agent";
        }

        base.Start();

        cuttingStations = FindObjectsByType<CuttingStation>(FindObjectsSortMode.None);
        cutIngredientsStations = FindObjectsByType<CutIngredientsStation>(FindObjectsSortMode.None);
        
        StartCoroutine(WorkLoop());
    }

    private IEnumerator WorkLoop()
    {
        while (true)
        {
            yield return StartCoroutine(CutIngredient());
            yield return new WaitForSeconds(0.1f);
        }
    }

    private IEnumerator CutIngredient()
    {
        // Trouver une station de découpage avec un ingrédient
        CuttingStation station = FindStationWithIngredient();
        if (station == null)
        {
            yield return new WaitForSeconds(0.5f);
            yield break;
        }

        // Aller à la station
        MoveTo(station.transform);
        yield return new WaitUntil(() => !isMoving);

        // Essayer de réserver et commencer le découpage
        if (!station.TryStartCutting(this))
        {
            yield return new WaitForSeconds(0.5f);
            yield break;
        }

        // Attendre que le découpage soit terminé
        yield return new WaitUntil(() => !station.IsCutting());

        // Prendre l'ingrédient découpé
        Ingredient cutIngredient = station.TakeIngredient();
        if (cutIngredient == null)
        {
            yield return new WaitForSeconds(0.1f);
            yield break;
        }

        PickUpIngredient(cutIngredient);

        // Trouver une place pour ingrédients découpés libre
        CutIngredientsStation freeStation = FindFreeCutIngredientsStation();
        if (freeStation == null)
        {
            // Attendre qu'une place se libère
            yield return new WaitUntil(() => FindFreeCutIngredientsStation() != null);
            freeStation = FindFreeCutIngredientsStation();
        }

        // Aller à la place
        MoveTo(freeStation.transform);
        yield return new WaitUntil(() => !isMoving);

        // Déposer l'ingrédient, attendre devant si la place se referme entre temps,
        // mais basculer immédiatement si une autre station libre apparaît.
        bool placed = false;
        while (!placed)
        {
            placed = freeStation.AddIngredient(cutIngredient);
            if (!placed)
            {
                CutIngredientsStation alternative = FindAlternativeCutStation(freeStation);
                if (alternative != null)
                {
                    freeStation = alternative;
                    MoveTo(freeStation.transform);
                    yield return new WaitUntil(() => !isMoving);
                    continue;
                }

                currentState = AgentState.Waiting;
                yield return new WaitForSeconds(0.1f);
            }
        }

        DropIngredient();
        currentState = AgentState.Idle;

        yield return new WaitForSeconds(0.1f);
    }

    private CuttingStation FindStationWithIngredient()
    {
        foreach (CuttingStation station in cuttingStations)
        {
            if (station.HasIngredient() && !station.IsCutting())
            {
                return station;
            }
        }
        return null;
    }

    private CutIngredientsStation FindFreeCutIngredientsStation()
    {
        foreach (CutIngredientsStation station in cutIngredientsStations)
        {
            if (station.IsAvailable())
            {
                return station;
            }
        }
        return null;
    }

    private CutIngredientsStation FindAlternativeCutStation(CutIngredientsStation current)
    {
        foreach (CutIngredientsStation station in cutIngredientsStations)
        {
            if (station == null || station == current)
            {
                continue;
            }

            if (station.IsAvailable())
            {
                return station;
            }
        }

        return null;
    }
}

