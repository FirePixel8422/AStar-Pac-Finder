using System;
using System.Collections;
using Unity.Collections;
using UnityEngine;

public static class ExtensionMethods
{
    #region Invoke

    /// <summary>
    /// Call function after a delay
    /// </summary>
    /// <param name="mb"></param>
    /// <param name="f">Function to call.</param>
    /// <param name="delay">Wait time before calling function.</param>
    public static void Invoke(this MonoBehaviour mb, Action f, float delay)
    {
        mb.StartCoroutine(InvokeRoutine(f, delay));
    }

    public static void Invoke<T>(this MonoBehaviour mb, Action<T> f, T param, float delay)
    {
        mb.StartCoroutine(InvokeRoutine(f, param, delay));
    }

    private static IEnumerator InvokeRoutine(Action f, float delay)
    {
        yield return new WaitForSeconds(delay);
        f.Invoke();
    }

    private static IEnumerator InvokeRoutine<T>(Action<T> f, T param, float delay)
    {
        yield return new WaitForSeconds(delay);
        f.Invoke(param);
    }

    #endregion


    #region SetParent

    public static void SetParent(this Transform trans, Transform parent, bool keepLocalPos, bool keepLocalRot)
    {
        if (parent == null)
        {
#if UNITY_EDITOR
            DebugLogger.LogWarning("You are trying to set a transform to a parent that doesnt exist, this is not allowed");
#endif
            return;
        }

        trans.SetParent(parent);
        if (!keepLocalPos)
        {
            trans.localPosition = Vector3.zero;
        }
        if (!keepLocalRot)
        {
            trans.localRotation = Quaternion.identity;
        }
    }
    public static void SetParent(this Transform trans, Transform parent, bool keepLocalPos, bool keepLocalRot, bool keepLocalScale)
    {
        if (parent == null)
        {
#if UNITY_EDITOR
            DebugLogger.LogWarning("You are trying to set a transform to a parent that doesnt exist, this is not allowed");
#endif
            return;
        }

        trans.SetParent(parent);
        if (!keepLocalPos)
        {
            trans.localPosition = Vector3.zero;
        }
        if (!keepLocalRot)
        {
            trans.localRotation = Quaternion.identity;
        }
        if (!keepLocalScale)
        {
            trans.localScale = Vector3.one;
        }
    }

    #endregion


    #region TryGetComponent(s)

    public static bool TryGetComponents<T>(this Transform trans, out T[] components) where T : Component
    {
        components = trans.GetComponents<T>();

        return components.Length > 0;
    }

    public static bool TryGetComponentInChildren<T>(this Transform trans, out T component, bool includeInactive = false) where T : Component
    {
        component = trans.GetComponentInChildren<T>(includeInactive);
        return component != null;
    }
    public static bool TryGetComponentsInChildren<T>(this Transform trans, out T[] components, bool includeInactive) where T : Component
    {
        components = trans.GetComponentsInChildren<T>(includeInactive);

        return components.Length > 0;
    }

    public static bool TryGetComponentInParent<T>(this Transform trans, out T component) where T : Component
    {
        component = trans.GetComponentInParent<T>();
        return component != null;
    }
    public static bool TryGetComponentsInParent<T>(this Transform trans, out T[] component) where T : Component
    {
        component = trans.GetComponentsInParent<T>();
        return component != null;
    }

    public static bool TryFindObjectOfType<T>(this UnityEngine.Object obj, out T component, FindObjectsInactive includeInactive = FindObjectsInactive.Include) where T : Component
    {
        component = UnityEngine.Object.FindFirstObjectByType<T>(includeInactive);
        return component != null;
    }
    public static bool TryFindObjectsOfType<T>(this UnityEngine.Object obj, out T[] component, FindObjectsInactive includeInactive = FindObjectsInactive.Include, FindObjectsSortMode sortMode = FindObjectsSortMode.None) where T : Component
    {
        component = UnityEngine.Object.FindObjectsByType<T>(includeInactive, sortMode);
        return component != null;
    }

    #endregion


    #region HasComponent

    public static bool HasComponent<T>(this Transform trans) where T : Component
    {
        return trans.GetComponent<T>() != null;
    }

    public static bool HasComponentInChildren<T>(this Transform trans, bool includeInactive = false) where T : Component
    {
        return trans.GetComponentInChildren<T>(includeInactive) != null;
    }

    public static bool HasComponentInParent<T>(this Transform trans, bool includeInactive = false) where T : Component
    {
        return trans.GetComponentInParent<T>(includeInactive) != null;
    }

    #endregion



    /// <summary>
    /// Check if the NativeArray is created, and if so, dispose of it
    /// </summary>
    public static void DisposeIfCreated<T>(this NativeArray<T> array) where T : unmanaged
    {
        if (array.IsCreated)
            array.Dispose();
    }
    /// <summary>
    /// Check if the NativeArray is created, and if so, dispose of it
    /// </summary>
    public static void DisposeIfCreated<T>(this NativeList<T> array) where T : unmanaged
    {
        if (array.IsCreated)
            array.Dispose();
    }
    /// <summary>
    /// Check if the NativeArray is created, and if so, dispose of it
    /// </summary>
    public static void DisposeIfCreated<T>(this NativeReference<T> array) where T : unmanaged
    {
        if (array.IsCreated)
            array.Dispose();
    }
    /// <summary>
    /// Check if the NativeArray is created, and if so, dispose of it
    /// </summary>
    public static void DisposeIfCreated<T>(this NativeHashSet<T> array) where T : unmanaged, IEquatable<T>
    {
        if (array.IsCreated)
            array.Dispose();
    }
    /// <summary>
    /// Check if the NativeArray is created, and if so, dispose of it
    /// </summary>
    public static void DisposeIfCreated<Tkey, TValue>(this NativeHashMap<Tkey, TValue> array) where Tkey : unmanaged, IEquatable<Tkey> where TValue : unmanaged
    {
        if (array.IsCreated)
            array.Dispose();
    }


    /// <summary>
    /// Lets AudioSource play selected clip with selected pitch
    /// </summary>
    public static void PlayClipWithPitch(this AudioSource source, AudioClip clip, float pitch)
    {
        source.clip = clip;
        source.pitch = pitch;
        source.Play();
    }
    /// <summary>
    /// Lets AudioSource play with selected pitch
    /// </summary>
    public static void PlayWithPitch(this AudioSource source, float pitch)
    {
        source.pitch = pitch;
        source.Play();
    }

    /// <summary>
    /// Set the active state of a Behaviour component only if the state is different from the current state.
    /// </summary>
    public static void SetActiveStateSmart(this Behaviour comp, bool state)
    {
        if (comp.enabled != state)
        {
            comp.enabled = state;
        }
    }

    /// <summary>
    /// Set the active state of a GameObject only if the state is different from the current state.
    /// </summary>
    public static void SetActiveSmart(this GameObject obj, bool state)
    {
        if (obj.activeInHierarchy != state)
        {
            obj.SetActive(state);
        }
    }


    /// <summary>
    /// Get PlayerGameId through ClintManager using OwnerClientId.
    /// </summary>
    //public static int GetOwnerClientGameId(this NetworkObject networkObj)
    //{
    //    return ClientManager.GetClientGameId(networkObj.OwnerClientId);
    //}
}