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
        GameObject.Instantiate(prefab,
            new Vector3(
                gameObject.transform.position.x + 0.3f,
                gameObject.transform.position.y,
                gameObject.transform.position.z),
            gameObject.transform.rotation,
            sceneRoot.transform);
    }
}
