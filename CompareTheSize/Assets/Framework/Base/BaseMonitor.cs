using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class BaseMotiner : MonoBehaviour
{
    private AsyncOperationHandle m_handle;
    public static void monitor(GameObject obj)
    {
        if ((obj == null) || obj.Equals(null))
        {
            return;
        }

        var monitor = obj.GetComponent<BaseMotiner>();
        if (monitor == null)
        {
            monitor = obj.AddComponent<BaseMotiner>();
        }
    }

    public static void monitor(AsyncOperationHandle handle, GameObject obj)
    {
        if ((obj == null) || obj.Equals(null))
        {
            return;
        }

        var monitor = obj.GetComponent<BaseMotiner>();
        if (monitor == null)
        {
            monitor = obj.AddComponent<BaseMotiner>();
        }

        monitor.m_handle = handle;
    }

    void OnDestroy()
    {
        // Debug.LogWarning("Motiner.OnDestroy: " + this.name);
        if (m_handle.IsValid())
        {
            Addressables.Release(m_handle);
        }
    }
}
