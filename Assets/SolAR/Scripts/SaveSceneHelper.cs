using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveSceneHelper : MonoBehaviour
{
    public GameObject solARScene;
    public Bcom.Solar.SolArCloudHololens2 solarCloudHololens2;

    public GameObject serializableCubePrefab;
    public GameObject serializableSpherePrefab;
    public GameObject serializableArrowPrefab;
    public GameObject serializableCoffeeCupPrefab;
    public GameObject serializableText3DPrefab;
    public GameObject serializableElectricBoxPrefab;

    public bool save;
    public bool load;

    private string PERSISTENT_SCENE_DATA_PATH;


    [System.Serializable]
    public class Persistent3DObjects
    {
        [SerializeField]
        public List<Serializable3DObject.SerializableObjectData> persistentObjects = new List<Serializable3DObject.SerializableObjectData>();
    }

    private void Awake() {
        PERSISTENT_SCENE_DATA_PATH = Path.Combine(Application.persistentDataPath, "sceneData.json");

        solarCloudHololens2.OnStart += OnStart;
    }

    private void Update()
    {
        if (save)
        {
            save = false;
            SaveSceneState(solARScene);
        }
        if (load)
        {
            load = false;
            LoadSceneState(solARScene);
        }
    }

    private void OnStart(bool sensorsStarted, bool rpcAvailable)
    {
        // LoadSceneState(gameObject);
        load = true;
    }

    public void SaveSceneState(GameObject sceneRoot)
    {
        if (!sceneRoot)
            Debug.LogError("No sceneRoot specified!");

        var objectsToSave = sceneRoot.GetComponentsInChildren<Serializable3DObject>();

        Persistent3DObjects persistent3DObjects = new Persistent3DObjects();
        foreach( var objectToSave in objectsToSave)
        {
            persistent3DObjects.persistentObjects.Add(objectToSave.Serialize());
        }

        string jsonData = JsonUtility.ToJson(persistent3DObjects, true);
        File.WriteAllText(PERSISTENT_SCENE_DATA_PATH, jsonData);
        Debug.Log("Saved " + objectsToSave.Length + " persistent scene objects");
    }

    public void LoadSceneState(GameObject sceneRoot)
    {
        if (!sceneRoot)
            Debug.LogError("No sceneRoot specified!");

        CleanupScene(sceneRoot);

        try
        {
            var serializedData = File.ReadAllText(PERSISTENT_SCENE_DATA_PATH);
            var objectsToLoad = JsonUtility.FromJson<Persistent3DObjects>(serializedData);

            foreach(var objectToLoad in objectsToLoad.persistentObjects)
            {
                Debug.Log("objectName: " + objectToLoad.objectName);

                var loadedObject = InstantiateSerializedObject(objectToLoad.objectType, sceneRoot.transform);
                loadedObject.name = objectToLoad.objectName;
                loadedObject.transform.localPosition = objectToLoad.position;
                loadedObject.transform.localRotation = objectToLoad.rotation;
                loadedObject.transform.localScale = objectToLoad.scale;
            }

            Debug.Log("Loaded " + objectsToLoad.persistentObjects.Count + " persistent scene objects");
        }
        catch (System.IO.FileNotFoundException e)
        {
            // Nothing to load
            return;
        }
    }

    public void CleanupScene(GameObject sceneRoot)
    {
        var objectsToClean = sceneRoot.GetComponentsInChildren<Serializable3DObject>();
        foreach(var objectToClean in objectsToClean)
            GameObject.Destroy(objectToClean.gameObject);
    }

    private GameObject InstantiateSerializedObject(Serializable3DObject.ObjectType objectType, Transform parentTransform)
    {
        switch(objectType)
        {
            case Serializable3DObject.ObjectType.CUBE: return GameObject.Instantiate(serializableCubePrefab, parentTransform);
            case Serializable3DObject.ObjectType.SPHERE: return GameObject.Instantiate(serializableSpherePrefab, parentTransform);
            case Serializable3DObject.ObjectType.ARROW: return GameObject.Instantiate(serializableArrowPrefab, parentTransform);
            case Serializable3DObject.ObjectType.COFFEE_CUP: return GameObject.Instantiate(serializableCoffeeCupPrefab, parentTransform);
            case Serializable3DObject.ObjectType.TEXT: return GameObject.Instantiate(serializableText3DPrefab, parentTransform);
            case Serializable3DObject.ObjectType.ELECTRIC_BOX: return GameObject.Instantiate(serializableElectricBoxPrefab, parentTransform);
            default: return GameObject.Instantiate(serializableCubePrefab, parentTransform);
        }
    }
}
