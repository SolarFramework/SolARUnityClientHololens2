using Com.Bcom.Solar;
using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.Utilities;

public class ConnectButtonHandler : MonoBehaviour
{
    public SolARCloud solar;
    public ButtonConfigHelper buttonConfigHelper;
    public List<GameObject> enabledWhenDisconnected = new List<GameObject>();
    public List<GameObject> enabledWhenConnected = new List<GameObject>();
    private bool update = false;
    private bool error;

    void Start()
    {
        update = true;
    }

    void Update()
    {
        if (update)
        {
            buttonConfigHelper.MainLabelText = $"{(solar.Isregistered() ? "Disconnect" : "Connect")}{(error ? "\nerror" : "")}";
            foreach (var button in enabledWhenDisconnected) button.SetActive(!solar.Isregistered());           
            foreach (var button in enabledWhenConnected) button.SetActive(solar.Isregistered());
            GetComponent<GridObjectCollection>()?.UpdateCollection();
            update = false;
        }
    }

    public async void ToggleConnection()
    {
        error = !(solar.Isregistered() ? await solar.Disconnect() : await solar.Connect());
        update = true;
    }
}
