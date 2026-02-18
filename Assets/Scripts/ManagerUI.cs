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

    [Header("Managers")]
    [SerializeField] private MirrorTherapyManager mirrorManager;

    [Header("Spawn Points")]
    [SerializeField] private Transform freeSpawn;
    [SerializeField] private Transform ObjectSpawn;

    [Header("Entities")]
    [SerializeField] private GameObject ObjectToMove;


    private void Start()
    {
        useLeftArm.interactable = true;
        useRightArm.interactable = false;

        objectMoveButton.interactable = true;
        freeMoveButton.interactable = false;
        ObjectToMove.transform.position = freeSpawn.position;
        ObjectToMove.transform.rotation = freeSpawn.rotation;
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
    }

    public void OnObjectMoveClicked()
    {
        ObjectToMove.transform.position = ObjectSpawn.position;
        ObjectToMove.transform.rotation = ObjectSpawn.rotation;
        objectMoveButton.interactable = false;
        freeMoveButton.interactable = true;
    }
}
