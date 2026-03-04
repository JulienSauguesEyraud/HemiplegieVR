using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ManagerUI : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    [Header("Boutons")]
    [Header("Pour les bras")]
    [SerializeField] private Button useLeftArm;
    [SerializeField] private Button useRightArm;

    [Header("Pour les activités")]
    [SerializeField] private Button freeMoveButton;
    [SerializeField] private Button objectMoveButton;

    [Header("Pour les objets interactifs")]
    [SerializeField] private Button resetObjectButton;

    [Header("Divers")]
    [Header("Managers")]
    [SerializeField] private MirrorTherapyManager mirrorManager;

    [Header("Spawn Points")]
    [SerializeField] private Transform freeSpawn;
    [SerializeField] private Transform ObjectSpawn;

    [Header("Entities")]
    [SerializeField] private GameObject ObjectToMove;

    [Header("Objets interactifs")]
    [SerializeField] private GameObject[] InteractableObjects;
    private List<Vector3> initialsPositions = new();
    private List<Quaternion> initialsRotations = new();

    [Header("Rendu des mains")]
    [SerializeField] SkinnedMeshRenderer leftHandRenderer;
    [SerializeField] SkinnedMeshRenderer rightHandRenderer;

    [Header("Panel de couleur")]
    [SerializeField] private GameObject panelColor;

    private void Start()
    {
        useLeftArm.interactable = true;
        useRightArm.interactable = false;

        objectMoveButton.interactable = true;
        freeMoveButton.interactable = false;
        ObjectToMove.transform.position = freeSpawn.position;
        ObjectToMove.transform.rotation = freeSpawn.rotation;

        resetObjectButton.gameObject.SetActive(false);

        foreach (var obj in InteractableObjects)
        {
            initialsPositions.Add(obj.transform.position);
            initialsRotations.Add(obj.transform.rotation);
        }
    }

    public void OnUseLeftArmClicked()
    {
        mirrorManager.sourceHand = SourceHand.Left;
        useLeftArm.interactable = false;
        useRightArm.interactable = true;
    }

    public void OnUseRightArmClicked()
    {
        mirrorManager.sourceHand = SourceHand.Right;
        useLeftArm.interactable = true;
        useRightArm.interactable = false;
    }

    public void OnFreeMoveClicked()
    {
        ObjectToMove.transform.position = freeSpawn.position;
        ObjectToMove.transform.rotation = freeSpawn.rotation;
        objectMoveButton.interactable = true;
        freeMoveButton.interactable = false;
        resetObjectButton.gameObject.SetActive(false);
    }

    public void OnObjectMoveClicked()
    {
        ObjectToMove.transform.position = ObjectSpawn.position;
        ObjectToMove.transform.rotation = ObjectSpawn.rotation;
        objectMoveButton.interactable = false;
        freeMoveButton.interactable = true;
        resetObjectButton.gameObject.SetActive(true);
    }

    public void setActiveForearms(bool active)
    {
        mirrorManager.SetShowForearms(active);
    }

    public void OnResetObjectClicked()
    {
        for (int i = 0; i < InteractableObjects.Length; i++)
        {
            InteractableObjects[i].GetComponent<Rigidbody>().useGravity = false;
            InteractableObjects[i].transform.position = initialsPositions[i];
            InteractableObjects[i].transform.rotation = initialsRotations[i];
            InteractableObjects[i].GetComponent<Rigidbody>().useGravity = true;
        }
    }

    public void OnShowColor()
    {
        panelColor.SetActive(true);
    }

    public void OnColorClicked(Material color)
    {
        leftHandRenderer.material = color;
        rightHandRenderer.material = color;
        mirrorManager.SetForearmMaterial(color);
        panelColor.SetActive(false);
    }
}
