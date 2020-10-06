using UnityEngine;
using EasyButtons;

public class ObjectManager : MonoBehaviour
{
    [Button("Clear")]
    public void ClearChildren()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            DestroyImmediate(transform.GetChild(i));
        }
    }
}
