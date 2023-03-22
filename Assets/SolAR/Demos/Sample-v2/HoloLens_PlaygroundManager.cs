using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

using Com.Bcom.Solar;

public class HoloLens_PlaygroundManager : MonoBehaviour
{
    public GameObject solARCloudHololens2;

    private SolARCloud solARCloud;
    private SolARCloudHololens2Specific solARCloudHololens2Specific;
    // Start is called before the first frame update
    void Start()
    {
        solARCloud = solARCloudHololens2.GetComponent<SolARCloud>();
        solARCloudHololens2Specific = solARCloudHololens2.GetComponent<SolARCloudHololens2Specific>();
        var unityTransport = GetComponent<UnityTransport>();
        // Use same ip as SolAR frontend (without http://)
        unityTransport.ConnectionData.Address = solARCloud.frontendIp.Substring(7);
        unityTransport.ConnectionData.Port = 32767;
        NetworkManager.Singleton.StartClient();
        Debug.Log("Connect to 3D Assets Sync server...");
    }

    // Update is called once per frame
    void Update()
    {
        if (solARCloudHololens2Specific.solarScene == null)
        {
            var scene = FindObjectOfType<Bcom.SharedPlayground.ScenePersistency>();
            if (scene)
            {
                solARCloudHololens2Specific.SetSolARScene(scene.gameObject);
                Debug.Log("Set up SolAR scene");
            }
        }
    }
}
