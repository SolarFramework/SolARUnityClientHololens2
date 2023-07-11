using UnityEngine;

using Com.Bcom.Solar;

public class HoloLens_PlaygroundManager : Bcom.SharedPlayground.PlaygroundManager
{
    public GameObject solARCloudHololens2;

    private SolARCloud solARCloud;
    private SolARCloudHololens2Specific solARCloudHololens2Specific;
    // Start is called before the first frame update
    void Start()
    {
        solARCloud = solARCloudHololens2.GetComponent<SolARCloud>();
        solARCloud.OnConnect += Connect;
        solARCloud.OnDisconnect += Disconnect;
        solARCloudHololens2Specific = solARCloudHololens2.GetComponent<SolARCloudHololens2Specific>();
    }

    // Update is called once per frame
    void Update()
    {
        if (solARCloudHololens2Specific.solarScene == null)
        {
            var scene = FindObjectOfType<Bcom.SharedPlayground.ScenePersistency>();
            if (scene)
            {
                // Here we apply a global rotation offset around the X axis to be aligned with the SolAR CS
                scene.transform.Rotate(Vector3.right, 180f);
                solARCloudHololens2Specific.SetSolARScene(scene.gameObject);
                Debug.Log("Set up SolAR scene");
            }
        }
    }
}
