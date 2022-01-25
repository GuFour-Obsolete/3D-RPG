using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// M_Studio���ͼ̳�Mono��������
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
    /// �ж��Ƿ��Ѿ���ʼ��
    /// </summary>
    public static bool IsInitialized
    {
        get { return instance != null; }
    }

    /// <summary>
    /// ���ٷ���
    /// </summary>
    protected virtual void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
}