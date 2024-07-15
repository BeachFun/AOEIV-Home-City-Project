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

    private int maxHomeslessness = 100;
    private int minHomelessness = 0;
    private int currentHomelessness;
    private int previousHomelessness;

    private int maxHappiness = 100;
    private int minHappiness = 0;
    private int currentHappiness = 100;
    private float happinessDecreaseInterval = 10f; // Time in seconds to decrease happiness
    private float timeSinceLastDecrease;

    private IGameManager gameMgr;
    private IResourceManager resourceMgr;

    void Start()
    {
        this.gameMgr = FindObjectOfType<GameManager>();
        this.resourceMgr = gameMgr.GetService<IResourceManager>();
        SetResource(0, "homelessness", 5, 100);
        SetResource(0, "happiness", 100, 100);
        previousHomelessness = 5; // Initial value to match the SetResource call in Start
        timeSinceLastDecrease = 0f; // Initialize timer
    }

    void Update()
    {
        // Here, you should implement how you update numberOfPeople and numberOfHomes,
        // maybe through other scripts or events in your game. For now, let's assume
        // they are updated elsewhere and we're just using the values here.
        if (currentHappiness == 0)
        {
            gameMgr.OnFactionDefeatedLocal(0);
        }
        else
        {
            UpdateHomelessness();
            UpdateHappiness();
        }
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
        GetResource(0, resourceKey, out amount, out capacity, out resourceType);
        ResourceTypeValue typeValue = new ResourceTypeValue { amount = amount1, capacity = capacity1 };
        IFactionResourceHandler resourceHandler = resourceMgr.FactionResources[factionID].ResourceHandlers[resourceType];
        resourceHandler.SetAmount(typeValue, out _);
    }

    private void UpdateHomelessness()
    {
        int amount;
        int capacity;
        GetResource(0, "population", out amount, out capacity, out _);
        //Debug.Log(string.Format("Capacity {0}", capacity));
        //Debug.Log(string.Format("Amount {0}", amount));

        if (amount <= capacity)
        {
            currentHomelessness = maxHomeslessness;
        }
        else
        {
            float temp = amount - capacity;
            temp = temp / amount;
            float percentage = 1 - temp;
            currentHomelessness = Mathf.Clamp((int)(maxHomeslessness * percentage), minHomelessness, maxHomeslessness);
        }

        if (currentHomelessness != previousHomelessness)
        {
            SetResource(0, "homelessness", currentHomelessness, 100);
            previousHomelessness = currentHomelessness;
        }
    }
    private void UpdateHappiness()
    {
        if (currentHomelessness < maxHomeslessness)
        {
            timeSinceLastDecrease += Time.deltaTime;

            if (timeSinceLastDecrease >= happinessDecreaseInterval)
            {
                currentHappiness = Mathf.Clamp(currentHappiness - 5, minHappiness, maxHappiness);
                SetResource(0, "happiness", currentHappiness, 100);
                timeSinceLastDecrease = 0f;
            }
        }
    }
}
