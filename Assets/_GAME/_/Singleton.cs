using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    private static readonly object _instanceLock = new object();
    private static bool _quitting = false;
    private static bool _destroying = false;

    public static T Instance
    {
        get
        {
            lock (_instanceLock)
            {
                if (_instance == null && !_quitting && !_destroying)
                {

                    _instance = GameObject.FindObjectOfType<T>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject(typeof(T).ToString());
                        _instance = go.AddComponent<T>();
                    }
                }

                return _instance;
            }
        }
    }

    protected virtual void Awake()
    {
        _destroying = false;
        Init();
        if (_instance == null) { 
            _instance = gameObject.GetComponent<T>();
        }
        else if (_instance.GetInstanceID() != GetInstanceID())
        {
            Destroy(gameObject);
            throw new System.Exception(string.Format("Instance of {0} already exists, removing {1}", GetType().FullName, ToString()));
        }
    }

    protected virtual void OnDestroy()
    {
        if (_instance == this)
        {
            _destroying = true;
            _instance = null;
        }
    }

    protected virtual void OnApplicationQuit()
    {
        _quitting = true;
    }

    protected virtual void Init() { }
}