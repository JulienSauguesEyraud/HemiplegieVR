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

    [Header("Divers")]
    [Header("Managers")]
    [SerializeField] private MirrorTherapyManager mirrorManager;

    [Header("Spawn Points")]
    [SerializeField] private Transform freeSpawn;
    [SerializeField] private Transform ObjectSpawn;

    [Header("Entities")]
    [SerializeField] private GameObject camPlayer;
    [SerializeField] private DuplicateObject duplicateObjectManager;

    [Header("Objets interactifs")]
    [SerializeField] private GameObject[] InteractableObjects;
    [SerializeField] private Transform MovePosition;
    [SerializeField] private Transform DuplicatePosition;
    [SerializeField] private GameObject objectSectionUI;
    [SerializeField] private GameObject panelObjects;
    private Dictionary<GameObject, (Vector3 position, Quaternion rotation)> initialTransforms = new();
    private GameObject ObjectToMove;

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
        camPlayer.transform.position = freeSpawn.position;
        camPlayer.transform.rotation = freeSpawn.rotation;

        panelObjects.SetActive(false);
        panelColor.SetActive(false);
        objectSectionUI.SetActive(false);

        foreach (var obj in InteractableObjects)
        {
            initialTransforms[obj] = (obj.transform.position, obj.transform.rotation);
        }

        ObjectToMove = InteractableObjects[0];
        ObjectToMove.transform.position = MovePosition.position;
        ObjectToMove.transform.rotation = MovePosition.rotation;
        duplicateObjectManager.PreInstantiateDuplicate(ObjectToMove);
    }

    public void OnUseLeftArmClicked()
    {
        mirrorManager.sourceHand = SourceHand.Left;
        useLeftArm.interactable = false;
        useRightArm.interactable = true;
        panelObjects.SetActive(false);
        panelColor.SetActive(false);

        switchPositions();
    }

    public void OnUseRightArmClicked()
    {
        mirrorManager.sourceHand = SourceHand.Right;
        useLeftArm.interactable = true;
        useRightArm.interactable = false;
        panelObjects.SetActive(false);
        panelColor.SetActive(false);

        switchPositions();
    }
    public void setActiveForearms(bool active)
    {
        mirrorManager.SetShowForearms(active);
        panelObjects.SetActive(false);
        panelColor.SetActive(false);
    }

    public void OnFreeMoveClicked()
    {
        camPlayer.transform.position = freeSpawn.position;
        camPlayer.transform.rotation = freeSpawn.rotation;
        objectMoveButton.interactable = true;
        freeMoveButton.interactable = false;
        objectSectionUI.SetActive(false);
        panelColor.SetActive(false);
        panelObjects.SetActive(false);

        mirrorManager.RequestMirrorPlaneUpdate();
    }

    public void OnObjectMoveClicked()
    {
        camPlayer.transform.position = ObjectSpawn.position;
        camPlayer.transform.rotation = ObjectSpawn.rotation;
        objectMoveButton.interactable = false;
        freeMoveButton.interactable = true;
        objectSectionUI.SetActive(true);
        panelColor.SetActive(false);

        mirrorManager.RequestMirrorPlaneUpdate();
    }

    public void OnShowObjects()
    {
        panelObjects.SetActive(true);
        panelColor.SetActive(false);
    }

    public void OnSetObjectToMove(GameObject objectToMove)
    {
        ObjectToMove.transform.position = initialTransforms[ObjectToMove].position;
        ObjectToMove.transform.rotation = initialTransforms[ObjectToMove].rotation;

        ObjectToMove = objectToMove;
        ObjectToMove.transform.position = MovePosition.position;
        ObjectToMove.transform.rotation = initialTransforms[ObjectToMove].rotation;
        duplicateObjectManager.PreInstantiateDuplicate(ObjectToMove);

        panelObjects.SetActive(false);
    }

    public void OnResetObjectClicked()
    {
        ObjectToMove.transform.position = MovePosition.position;
        ObjectToMove.transform.rotation = initialTransforms[ObjectToMove].rotation;
        panelObjects.SetActive(false);
        panelColor.SetActive(false);
        duplicateObjectManager.PreInstantiateDuplicate(ObjectToMove);
    }

    public void OnShowColor()
    {
        panelColor.SetActive(true);
        panelObjects.SetActive(false);
    }

    public void OnColorClicked(Material color)
    {
        leftHandRenderer.material = color;
        rightHandRenderer.material = color;
        mirrorManager.SetForearmMaterial(color);
        panelColor.SetActive(false);
    }

    public void OnResetMirror()
    {
        mirrorManager.RequestMirrorPlaneUpdate();
    }


    private void switchPositions()
    {
        Transform temp = new GameObject().transform;
        temp.position = MovePosition.position;
        MovePosition.position = DuplicatePosition.position;
        DuplicatePosition.position = temp.position;
        Destroy(temp.gameObject);

        OnResetObjectClicked();
    }
}
