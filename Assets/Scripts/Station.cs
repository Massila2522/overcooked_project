using UnityEngine;
using System;

public abstract class Station : MonoBehaviour
{
    [Header("Station Settings")]
    public bool IsOccupied { get; protected set; }
    public Agent CurrentAgent { get; protected set; }
    
    protected object lockObject = new object();

    public virtual bool TryReserve(Agent agent)
    {
        lock (lockObject)
        {
            if (!IsOccupied)
            {
                IsOccupied = true;
                CurrentAgent = agent;
                return true;
            }
            return false;
        }
    }

    public virtual void Release(Agent agent)
    {
        lock (lockObject)
        {
            if (CurrentAgent == agent)
            {
                IsOccupied = false;
                CurrentAgent = null;
            }
        }
    }

    public virtual bool IsAvailable()
    {
        return !IsOccupied;
    }
}

