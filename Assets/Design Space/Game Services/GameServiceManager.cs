using System;
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = ProjectConstants.ServicesFolder + "Game Service Manager", fileName = GameServiceManagerResourceName)]
public class GameServiceManager : ScriptableObject
{
    [SerializeField] private List<GameService> services;

    private void OnValidate()
    {
        if (name != GameServiceManagerResourceName)
        {
            Debug.LogError($"Game Service Manager can't be loaded from resources! The asset must be named {GameServiceManagerResourceName}!", this);
        }
    }

    private const string GameServiceManagerResourceName = "Game Service Manager";
    
    private static GameServiceCallbacks instance;
    public static GameServiceCallbacks Instance => instance != null ? instance : SpawnInstance();

    public void AddService(GameService gameService) => services.Add(gameService);
    public bool ContainsService(GameService gameService) => services.Contains(gameService);
    
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static GameServiceCallbacks SpawnInstance()
    {
        instance = new GameObject("Game Service Callbacks Instance").AddComponent<GameServiceCallbacks>();
        DontDestroyOnLoad(instance);
        
        // load manager
        
        var manager = Resources.Load<GameServiceManager>(GameServiceManagerResourceName);

        if (manager == null)
        {
            Debug.LogWarning("No GameServiceManager was found in a resources folder!");
            return null;
        }

        // initialize services

        foreach (var service in manager.services)
            service.InitializeService();

        return instance;
    }
}
