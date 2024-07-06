using System.Collections.Generic;
using RTSEngine.ResourceExtension;
using RTSEngine.Game;
using System;
using System.Reflection;
using UnityEngine;

public class HappinessCalculator : MonoBehaviour
{
    [SerializeField, Tooltip("Reference to the resource manager to update the happiness resource.")]
    private ResourceManager resourceManager;

    private int maxHappiness = 100;
    private int minHappiness = 0;
    private int currentHappiness;
    private int previousHappiness;

    private IGameManager gameMgr;
    private IResourceManager resourceMgr;

    void Start()
    {
        this.gameMgr = FindObjectOfType<GameManager>();
        this.resourceMgr = gameMgr.GetService<IResourceManager>();
        SetResource(0, "happiness", 5, 100);
        previousHappiness = 5; // Initial value to match the SetResource call in Start
    }

    void Update()
    {
        // Here, you should implement how you update numberOfPeople and numberOfHomes,
        // maybe through other scripts or events in your game. For now, let's assume
        // they are updated elsewhere and we're just using the values here.

        UpdateHappiness();
    }

    static void PrintAllAttributes(object obj)
    {
        Type type = obj.GetType();
        PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        foreach (PropertyInfo property in properties)
        {
            object value = property.GetValue(obj, null);
            Debug.Log($"{property.Name}: {value}");
        }
    }

    private void GetResource(int factionID, string resourceKey, out int amount, out int capacity, out ResourceTypeInfo rtInfo)
    {
        amount = 0;
        capacity = 0;
        rtInfo = null;

        ResourceTypeInfo temp;
        bool worked = resourceMgr.TryGetResourceTypeWithKey(resourceKey, out temp);
        if (worked)
        {
            amount = resourceMgr.FactionResources[factionID].ResourceHandlers[temp].Amount;
            capacity = resourceMgr.FactionResources[factionID].ResourceHandlers[temp].Capacity;
            rtInfo = temp;
        }
    }

    private void SetResource(int factionID, string resourceKey, int amount1, int capacity1)
    {
        ResourceTypeInfo resourceType;
        int amount;
        int capacity;
        GetResource(0, "happiness", out amount, out capacity, out resourceType);
        ResourceTypeValue typeValue = new ResourceTypeValue { amount = amount1, capacity = capacity1 };
        IFactionResourceHandler resourceHandler = resourceMgr.FactionResources[factionID].ResourceHandlers[resourceType];
        resourceHandler.SetAmount(typeValue, out _);
    }

    private void UpdateHappiness()
    {
        int amount;
        int capacity;
        GetResource(0, "population", out amount, out capacity, out _);
        //Debug.Log(string.Format("Capacity {0}", capacity));
        //Debug.Log(string.Format("Amount {0}", amount));

        if (amount <= capacity)
        {
            currentHappiness = maxHappiness;
        }
        else
        {
            float temp = amount - capacity;
            temp = temp / amount;
            float percentage = 1 - temp;
            currentHappiness = Mathf.Clamp((int)(maxHappiness * percentage), minHappiness, maxHappiness);
        }

        if (currentHappiness != previousHappiness)
        {
            SetResource(0, "happiness", currentHappiness, 100);
            previousHappiness = currentHappiness;
        }
    }
}
