using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NetworkedPrefab
{
    public GameObject prefab = null;

    public string path = "";

    public NetworkedPrefab(GameObject prefab, string path)
    {
        this.prefab = prefab;
        this.path = ReturnModifiedPrefabPath(path);
    }

    private string ReturnModifiedPrefabPath(string path)
    {
        int extensionLength = System.IO.Path.GetExtension(path).Length;
        int additionalLength = 7;
        int startIndex = path.ToLower().IndexOf("prefabs");

        if (startIndex == -1)
        {
            Debug.Log("path not found");
            return "";
        } else
        {
            return path.Substring(startIndex + additionalLength, path.Length - (startIndex + additionalLength + extensionLength));
        }
    }
}
