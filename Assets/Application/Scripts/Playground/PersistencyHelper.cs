using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine;

public class PersistencyHelper : MonoBehaviour
{
    public ButtonConfigHelper clearSceneButton;
    public ButtonConfigHelper saveSceneButton;
    public ButtonConfigHelper loadSceneButton;

    void Awake()
    {
        clearSceneButton.OnClick.AddListener(Clear);
        saveSceneButton.OnClick.AddListener(Save);
        loadSceneButton.OnClick.AddListener(Load);
    }

    void Clear() => FindObjectOfType<Bcom.SharedPlayground.ScenePersistency>().CleanUpSceneServerRpc();
    void Save() => FindObjectOfType<Bcom.SharedPlayground.ScenePersistency>().SaveSceneStateServerRpc();
    void Load() => FindObjectOfType<Bcom.SharedPlayground.ScenePersistency>().LoadSceneStateServerRpc();
}
