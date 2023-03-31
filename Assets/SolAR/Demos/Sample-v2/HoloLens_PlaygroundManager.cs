using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

using Com.Bcom.Solar;

public class HoloLens_PlaygroundManager : MonoBehaviour
{
    public GameObject solARCloudHololens2;
    public ushort port;

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

    private void Connect(string frontendIp)
    {
        var unityTransport = GetComponent<UnityTransport>();
        // Use same ip as SolAR frontend (without http:// and port)
        string ipWithoutHTTP = frontendIp.Substring(7);
        unityTransport.ConnectionData.Address = ipWithoutHTTP.Split(':')[0];
        unityTransport.ConnectionData.Port = port;
        Debug.Log($"Connecting to 3D Assets Sync server at {unityTransport.ConnectionData.Address}:{unityTransport.ConnectionData.Port}");
        NetworkManager.Singleton.StartClient();
    }

    private void Disconnect()
    {
        if (NetworkManager.Singleton.IsClient && NetworkManager.Singleton.LocalClient.PlayerObject.TryGetComponent(out Bcom.SharedPlayground.PlaygroundPlayer playgroundPlayer))
        {
            playgroundPlayer.Disconnect();
        }
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
