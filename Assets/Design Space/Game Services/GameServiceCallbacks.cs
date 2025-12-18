using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameServiceCallbacks : MonoBehaviour
{
    public event Action<Scene, Scene> SceneChanged;
    public event Action Started;
    public event Action Enabled;
    public event Action Disabled;
    public event Action Updated;
    public event Action LateUpdated;
    public event Action FixedUpdated;
    public event Action Destroyed;
    public event Action ApplicationQuited;
    public event Action<bool> ApplicationFocused;
    public event Action<bool> ApplicationPaused;
    public event Action DrawGizmos;
    
    private void Awake() => SceneManager.activeSceneChanged += (from, to) => SceneChanged?.Invoke(from, to);
    private void Start() => Started?.Invoke();
    private void OnEnable() => Enabled?.Invoke();
    private void OnDisable() => Disabled?.Invoke();
    private void Update() => Updated?.Invoke();
    private void LateUpdate() => LateUpdated?.Invoke();
    private void FixedUpdate() => FixedUpdated?.Invoke();
    private void OnDestroy() => Destroyed?.Invoke();
    private void OnApplicationQuit() => ApplicationQuited?.Invoke();
    private void OnApplicationFocus(bool hasFocus) => ApplicationFocused?.Invoke(hasFocus);
    private void OnApplicationPause(bool pauseStatus) => ApplicationPaused?.Invoke(pauseStatus);
    private void OnDrawGizmos() => DrawGizmos?.Invoke();
}
