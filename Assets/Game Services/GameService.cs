using UnityEngine;
using UnityEngine.SceneManagement;

public abstract class GameService : ScriptableObject
{
    public void InitializeService()
    {
        CallbacksInstance.Started += Start;
        CallbacksInstance.Updated += Update;
        CallbacksInstance.Enabled += CallbacksObjectEnabled;
        CallbacksInstance.Disabled += CallbacksObjectDisabled;
        CallbacksInstance.LateUpdated += LateUpdate;
        CallbacksInstance.FixedUpdated += FixedUpdate;
        CallbacksInstance.SceneChanged += OnSceneChange;
        CallbacksInstance.Destroyed += InstanceDestroyed;
        CallbacksInstance.ApplicationQuited += OnApplicationQuit;
        CallbacksInstance.ApplicationFocused += OnApplicationFocused;
        CallbacksInstance.ApplicationPaused += OnApplicationPaused;
        CallbacksInstance.DrawGizmos += OnDrawGizmos;
        
        Initialize();
    }

    private static GameServiceCallbacks CallbacksInstance => GameServiceManager.Instance;
    
    protected static MonoBehaviour Instance => GameServiceManager.Instance;
    
    protected virtual void OnSceneChange(Scene from, Scene to) { }
    protected virtual void Initialize() { }
    protected virtual void CallbacksObjectEnabled() { }
    protected virtual void CallbacksObjectDisabled() { }
    protected virtual void Start() { }
    protected virtual void Update() { }
    protected virtual void LateUpdate() { }
    protected virtual void FixedUpdate() { }
    protected virtual void InstanceDestroyed() { }
    protected virtual void OnApplicationQuit() { }
    protected virtual void OnApplicationFocused(bool hasFocus) { }
    protected virtual void OnApplicationPaused(bool pauseStatus) { }
    protected virtual void OnDrawGizmos() { }
}
