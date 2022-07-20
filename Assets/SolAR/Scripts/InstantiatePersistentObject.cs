using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstantiatePersistentObject : MonoBehaviour
{
    public GameObject sceneRoot;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void InstantiateObject(GameObject prefab)
    {
        GameObject.Instantiate(prefab, sceneRoot.transform);
    }
}
