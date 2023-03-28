using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(ObjectManipulator), typeof(BoundsControl))]
public class HoloLens_InteractionHelper : MonoBehaviour
{
    public GameObject deleteButtonPrefab;

    private ButtonConfigHelper deleteButtonConfig;

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

        deleteButtonConfig = GetComponentInChildren<ButtonConfigHelper>();
        if (deleteButtonConfig == null)
        {
            var deleteButton = Instantiate(deleteButtonPrefab, transform);
            deleteButtonConfig = deleteButton.GetComponent<ButtonConfigHelper>();
            deleteButtonConfig.OnClick.AddListener(DeleteObject);
            deleteButton.SetActive(false);
        }
    }

    private void Update()
    {
        if (deleteButtonConfig.gameObject.activeSelf)
        {
            // Make sure button is always correctly positioned and scaled according to its object
            Vector3 extents = GetComponent<Collider>().bounds.extents;
            deleteButtonConfig.transform.localScale = new Vector3(1f / transform.lossyScale.x, 1f / transform.lossyScale.y, 1f / transform.lossyScale.z);
            deleteButtonConfig.transform.localPosition = -Vector3.forward * (extents.z / transform.lossyScale.z + 0.05f);
        }
    }

    private void GrabObject()
    {
        deleteButtonConfig.gameObject.SetActive(true);
        if (NetworkManager.Singleton.LocalClient.PlayerObject.TryGetComponent(out Bcom.SharedPlayground.PlaygroundPlayer playgroundPlayer))
        {
            playgroundPlayer.GrabObject(gameObject);
        }
    }

    private void DropObject()
    {
        deleteButtonConfig.gameObject.SetActive(false);
        if (NetworkManager.Singleton.LocalClient.PlayerObject.TryGetComponent(out Bcom.SharedPlayground.PlaygroundPlayer playgroundPlayer))
        {
            playgroundPlayer.DropObject();
        }
    }

    private void DeleteObject()
    {
        if (NetworkManager.Singleton.LocalClient.PlayerObject.TryGetComponent(out Bcom.SharedPlayground.PlaygroundPlayer playgroundPlayer))
        {
            playgroundPlayer.DestroyObject();
        }
    }
}
