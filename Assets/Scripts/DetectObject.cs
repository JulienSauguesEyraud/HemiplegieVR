using UnityEngine;

public class DetectObject : MonoBehaviour
{
    [SerializeField] private string targetTag = "Object";
    [SerializeField] private Collider objectCollider;

    [Header("Movement Detection")]
    [Tooltip("Seuil de distance minimum pour considérer l'objet en mouvement (m)")]
    [SerializeField] private float movementThreshold = 0.0001f;

    private GameObject currentDetectedObject;
    private Vector3 lastPosition;
    private bool isMoving = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(targetTag))
        {
            currentDetectedObject = other.gameObject;
            lastPosition = currentDetectedObject.transform.position;
            
            Debug.Log("Detected object: " + currentDetectedObject.name);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(targetTag) && other.gameObject == currentDetectedObject)
        {
            Debug.Log($"Object '{currentDetectedObject.name}' has exited the detection zone");
            StopTracking();
        }
    }

    private void Update()
    {
        if (currentDetectedObject != null)
        {
            CheckMovement();
        }
    }

    private void CheckMovement()
    {
        if (currentDetectedObject == null)
            return;

        bool wasMoving = isMoving;
        
        Vector3 currentPosition = currentDetectedObject.transform.position;
        float distance = Vector3.Distance(currentPosition, lastPosition);
        
        isMoving = distance > movementThreshold;

        // Logger seulement lors du changement d'état
        if (isMoving != wasMoving)
        {
            if (isMoving)
            {
                Debug.Log($"Object '{currentDetectedObject.name}' is now MOVING (distance: {distance:F6} m)");
            }
            else
            {
                Debug.Log($"Object '{currentDetectedObject.name}' has STOPPED moving");
            }
        }

        lastPosition = currentPosition;
    }

    // Propriétés publiques pour accéder à l'état
    public bool IsObjectDetected => currentDetectedObject != null;
    public bool IsObjectMoving => isMoving;
    public GameObject DetectedObject => currentDetectedObject;
    
    // Méthode pour arrêter manuellement le suivi d'un objet
    public void StopTracking()
    {
        if (currentDetectedObject != null)
        {
            Debug.Log($"Stopped tracking object: {currentDetectedObject.name}");
        }
        currentDetectedObject = null;
        isMoving = false;
    }
}
