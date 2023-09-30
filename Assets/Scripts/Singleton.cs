using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Only single instance of this object exists.
/// Can be obtained by '.Instance' call.
/// </summary>
public abstract class SingletonClass<T> : MonoBehaviour where T : MonoBehaviour
{
    public static T Instance { get; private set; }
    protected virtual void Awake()
    {
        if (Instance != null)
            Destroy(gameObject);
        else
            Instance = this as T;
    }

    protected virtual void OnApplicationQuit()
    {
        Instance = null;
        Destroy(gameObject);
    }
}

/// <summary>
/// Persists through scene loads.
/// </summary>
public abstract class PersistentSingletonClass<T> : SingletonClass<T> where T : MonoBehaviour
{

    protected override void Awake()
    {
        DontDestroyOnLoad(gameObject);
        base.Awake();
    }
}