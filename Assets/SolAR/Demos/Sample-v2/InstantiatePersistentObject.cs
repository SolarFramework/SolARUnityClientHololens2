using System.Collections;
using Microsoft.MixedReality.Toolkit.UI;
using Unity.Netcode;
using UnityEngine;

using Bcom.SharedPlayground;

[RequireComponent(typeof(ButtonConfigHelper))]
public class InstantiatePersistentObject : MonoBehaviour
{
    public PrefabType prefabType;
    public Transform iconAndTextHandle;
    public GameObject itemPreview3D;

    void Awake()
    {
        GetComponent<ButtonConfigHelper>().OnClick.AddListener(InstantiateObject);
    }

    public void InstantiateObject()
    {
        if (NetworkManager.Singleton.LocalClient.PlayerObject.TryGetComponent(out PlaygroundPlayer playgroundPlayer))
        {
            playgroundPlayer.CreateObject(prefabType, FindObjectOfType<ScenePersistency>().gameObject);
        }
    }

    public void UpdateItemPreview(GameObject itemPrefab)
    {
        GameObject newItemPreview = Instantiate(itemPreview3D, iconAndTextHandle.transform);
        newItemPreview.GetComponent<MeshFilter>().mesh = itemPrefab.GetComponentInChildren<MeshFilter>().sharedMesh;
        newItemPreview.GetComponent<MeshRenderer>().material = itemPrefab.GetComponentInChildren<MeshRenderer>().sharedMaterial;
        iconAndTextHandle.GetComponentInChildren<TMPro.TextMeshPro>().text = prefabType.ToString();
    }
}
