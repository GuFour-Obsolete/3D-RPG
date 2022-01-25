using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// M_Studio泛型继承Mono单例父级
/// </summary>
/// <typeparam name="T"></typeparam>
public class Singleton<T> : MonoBehaviour where T : Singleton<T>
{
    private static T instance;

    public static T Instance
    {
        get { return instance; }
    }

    protected virtual void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = (T)this;
        }
    }

    /// <summary>
    /// 判断是否已经初始化
    /// </summary>
    public static bool IsInitialized
    {
        get { return instance != null; }
    }

    /// <summary>
    /// 销毁方法
    /// </summary>
    protected virtual void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
}