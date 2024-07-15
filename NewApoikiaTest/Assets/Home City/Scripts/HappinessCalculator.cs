using System.Collections.Generic;
using RTSEngine.ResourceExtension;
using RTSEngine.Game;
using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.IO;

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

    private const float AOEFetchInterval = 600f; // 10 minutes in seconds

    private string linkedUsername = "Chilly5";

    void Start()
    {
        this.gameMgr = FindObjectOfType<GameManager>();
        this.resourceMgr = gameMgr.GetService<IResourceManager>();
        SetResource(0, "homelessness", 5, 100);
        SetResource(0, "happiness", 100, 100);
        previousHomelessness = 5; // Initial value to match the SetResource call in Start
        timeSinceLastDecrease = 0f; // Initialize timer

        // Test out API call
        InvokeRepeating("StartAoEUserRequest", 0f, AOEFetchInterval);
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

    // Start the coroutine to hit the URL
    public void StartAoEUserRequest()
    {
        StartCoroutine(GetRequest(linkedUsername));
    }

    // Coroutine to handle the web request
    IEnumerator GetRequest(string username)
    {
        string url = $"https://fernandodm.com.br/aoe4-city-builder/players/{username}/init";
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            // Check for errors
            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error: " + webRequest.error);
            }
            else
            {
                // Get the response text
                string jsonResponse = webRequest.downloadHandler.text;
                Debug.Log("Response: " + jsonResponse);

                // Current number of wins
                int currentTotalWins;
                ParseJsonResponse(jsonResponse, out currentTotalWins);

                // Path to the user's file
                string path = Path.Combine(Application.persistentDataPath, $"{username}_data.json");

                // Check if a file exists for this user
                if (File.Exists(path))
                {
                    // Read the existing JSON from the file
                    string previousJsonResponse = File.ReadAllText(path);

                    // Parse the previous JSON response
                    int previousTotalWins;
                    ParseJsonResponse(previousJsonResponse, out previousTotalWins);

                    // Compare the previous total win count with the current one
                    if (previousTotalWins < currentTotalWins)
                    {
                        // Give the user X number of villagers (add your logic here)
                        Debug.Log("Villagers to Give");
                        int villagersToGive = currentTotalWins - previousTotalWins;
                        Debug.Log(villagersToGive);
                    }
                }

                // Save the JSON response to a local file
                SaveJsonToFile(username, jsonResponse);
            }
        }
    }

    // Method to save JSON to a local file
    void SaveJsonToFile(string username, string jsonResponse)
    {
        string path = Path.Combine(Application.persistentDataPath, $"{username}_data.json");

        try
        {
            File.WriteAllText(path, jsonResponse);
            Debug.Log($"JSON saved to: {path}");
        }
        catch (IOException e)
        {
            Debug.LogError("Error saving JSON to file: " + e.Message);
        }
    }

    // Method to parse the JSON response
    void ParseJsonResponse(string jsonResponse, out int totalWins)
    {
        PlayerData playerData = JsonUtility.FromJson<PlayerData>(jsonResponse);
        totalWins = playerData.stats.totalWins;
        Debug.Log("Total Wins: " + totalWins);
    }


    // JSON parsing classes
    [System.Serializable]
    public class PlayerData
    {
        public Profile profile;
        public Stats stats;
        public ModeStats modeStats;
        public string last_updated;
    }

    [System.Serializable]
    public class Profile
    {
        public int id;
        public string name;
        public string country;
        public Avatars avatars;
    }

    [System.Serializable]
    public class Avatars
    {
        // Add fields for avatars if needed
    }

    [System.Serializable]
    public class Stats
    {
        public int totalWins;
    }

    [System.Serializable]
    public class ModeStats
    {
        public GameMode rm_team;
        public GameMode rm_solo;
        public GameMode rm_1v1_elo;
        public GameMode rm_2v2_elo;
        public GameMode rm_3v3_elo;
        public GameMode rm_4v4_elo;
        public GameMode qm_1v1;
        public GameMode qm_2v2;
        public GameMode qm_3v3;
        public GameMode qm_4v4;
        public GameMode rm_1v1;
    }

    [System.Serializable]
    public class GameMode
    {
        public int gamesCount;
        public int winsCount;
    }
}
