using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(ObjectManipulator), typeof(BoundsControl))]
public class HoloLens_InteractionHelper : MonoBehaviour
{
    void Awake()
    {
        var objectManipulator = GetComponent<ObjectManipulator>();
        objectManipulator.OnManipulationStarted.AddListener((ManipulationEventData eventData) => GrabObject());
        objectManipulator.OnManipulationEnded.AddListener((ManipulationEventData eventData) => DropObject());
        var boundsControl = GetComponent<BoundsControl>();
        boundsControl.RotateStarted.AddListener(GrabObject);
        boundsControl.RotateStopped.AddListener(DropObject);
        boundsControl.ScaleStarted.AddListener(GrabObject);
        boundsControl.ScaleStopped.AddListener(DropObject);
    }

    private void GrabObject()
    {
        if (NetworkManager.Singleton.LocalClient.PlayerObject.TryGetComponent(out Bcom.SharedPlayground.PlaygroundPlayer playgroundPlayer))
        {
            playgroundPlayer.GrabObject(gameObject);
        }
    }

    private void DropObject()
    {
        if (NetworkManager.Singleton.LocalClient.PlayerObject.TryGetComponent(out Bcom.SharedPlayground.PlaygroundPlayer playgroundPlayer))
        {
            playgroundPlayer.DropObject();
        }
    }
}
