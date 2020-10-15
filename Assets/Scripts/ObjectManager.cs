using UnityEngine;
using EasyButtons;

public class ObjectManager : MonoBehaviour
{
    [Button("Clear")]
    public void ClearChildren()
    {
        while (transform.childCount != 0)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }
    }
}
