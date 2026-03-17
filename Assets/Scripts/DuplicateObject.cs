using UnityEngine;

public class DuplicateObject : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DetectObject rightHandDetector;
    [SerializeField] private DetectObject leftHandDetector;
    [SerializeField] private MirrorHand mirrorHand;
    
    [Header("Duplication Settings")]
    [Tooltip("Spawn pour l'objet dupliqué (optionnel)")]
    [SerializeField] private Transform duplicateSpawnPoint;

    [Tooltip("Décalage local à appliquer à l'objet dupliqué")]
    [SerializeField] private Vector3 localOffset = Vector3.zero;
    
    [Tooltip("Appliquer la rotation miroir à l'objet dupliqué")]
    [SerializeField] private bool applyMirrorRotation = true;
    
    private GameObject duplicatedObject;
    private GameObject originalObject;
    private DetectObject activeDetector; // La main qui tient actuellement l'objet
    private bool isTracking = false; // Vrai si l'objet a commencé à bouger
    
    private void Update()
    {        
        bool detectedThisFrame = false;

        // Déterminer quelle main détecte l'objet physiquement dans son collider
        if (rightHandDetector != null && rightHandDetector.IsObjectDetected && rightHandDetector.DetectedObject == originalObject)
        {
            activeDetector = rightHandDetector;
            detectedThisFrame = true;
        }
        else if (leftHandDetector != null && leftHandDetector.IsObjectDetected && leftHandDetector.DetectedObject == originalObject)
        {
            activeDetector = leftHandDetector;
            detectedThisFrame = true;
        }

        // Sécurité pour les mouvements amples (Hystérésis) : 
        // Le Rigidbody de l'objet peut subir un petit retard par rapport au Tracking de la main lors d'un mouvement rapide,
        // ce qui le fait sortir brièvement du Collider. On ne veut pas casser la duplication dans ce cas.
        if (!detectedThisFrame)
        {
            if (activeDetector != null && originalObject != null)
            {
                float distance = Vector3.Distance(activeDetector.transform.position, originalObject.transform.position);
                // Si l'objet s'est trop éloigné (ex: +20 cm), c'est qu'il a vraiment été relâché
                if (distance > 0.2f)
                {
                    activeDetector = null; 
                }
            }
            else
            {
                activeDetector = null;
            }
        }

        // Si une main tient l'objet
        if (activeDetector != null)
        {
            // On ne commence à le déplacer en miroir que s'il commence à bouger
            if (!isTracking && detectedThisFrame && activeDetector.IsObjectMoving)
            {
                isTracking = true;
            }

            // Si le suivi a commencé, on met à jour la position
            if (isTracking)
            {
                UpdateDuplicateTransform();
            }
        }
        else
        {
            // L'objet a été lâché physiquement, on réinitialise le suivi
            isTracking = false;
        }
    }
    
    /// <summary>
    /// Vérifie si l'objet est détecté par au moins une main
    /// </summary>
    private bool IsObjectDetectedByAnyHand()
    {
        bool rightDetected = rightHandDetector != null && rightHandDetector.IsObjectDetected;
        bool leftDetected = leftHandDetector != null && leftHandDetector.IsObjectDetected;
        return rightDetected || leftDetected;
    }
    
    /// <summary>
    /// Récupère le détecteur actif (priorité à celui qui a l'objet)
    /// </summary>
    private DetectObject GetActiveDetector()
    {
        // Si on a déjà un détecteur actif et qu'il détecte toujours l'objet, on le garde
        if (activeDetector != null && activeDetector.IsObjectDetected)
        {
            return activeDetector;
        }
        
        // Sinon, chercher quel détecteur a l'objet
        if (rightHandDetector != null && rightHandDetector.IsObjectDetected)
        {
            return rightHandDetector;
        }
        
        if (leftHandDetector != null && leftHandDetector.IsObjectDetected)
        {
            return leftHandDetector;
        }
        
        return null;
    }
    
    /// <summary>
    /// Vérifie si les conditions sont remplies pour commencer la duplication
    /// </summary>
    private bool ShouldStartDuplication(DetectObject detector)
    {
        if (mirrorHand == null || !mirrorHand.IsEnabled)
        {
            return false;
        }
        
        if (detector == null)
        {
            return false;
        }
        
        if (!detector.IsObjectDetected)
        {
            return false;
        }
        
        if (!detector.IsObjectMoving)
        {
            return false;
        }
        
        return true;
    }
    
    /// <summary>
    /// Démarre la duplication de l'objet
    /// </summary>
    private void StartDuplication()
    {
        if (activeDetector == null)
        {
            return;
        }
        
        originalObject = activeDetector.DetectedObject;
        
        if (originalObject == null)
        {
            return;
        }
        
        // Créer la copie seulement si elle n'existe pas déjà
        if (duplicatedObject == null)
        {
            duplicatedObject = Instantiate(originalObject, duplicateSpawnPoint.position, duplicateSpawnPoint.rotation);
            duplicatedObject.name = originalObject.name + "_Mirror";
            
            // Appliquer l'effet miroir sur l'échelle dès l'instanciation
            Vector3 initialScale = originalObject.transform.localScale;
            if (applyMirrorRotation)
            {
                initialScale.x = -initialScale.x;
            }
            duplicatedObject.transform.localScale = initialScale;
            
            // Désactiver les composants physiques sur la copie pour éviter les interférences
            DisablePhysicsComponents(duplicatedObject);
        }
        
        // Mettre à jour la transformation
        UpdateDuplicateTransform();
        
        isTracking = true;
    }
    
    /// <summary>
    /// Arrête la duplication. Si un spawn point est défini, l'objet y retourne au lieu d'être détruit.
    /// </summary>
    private void StopDuplication()
    {
        activeDetector = null;
        isTracking = false;
    }
    
    /// <summary>
    /// Instancie l'objet à l'avance sur le point de spawn
    /// </summary>
    public void PreInstantiateDuplicate(GameObject sourceObject)
    {
        if (sourceObject == null || duplicateSpawnPoint == null)
        {
            Debug.LogWarning("PreInstantiateDuplicate: sourceObject ou duplicateSpawnPoint est manquant.");
            return;
        }

        if (duplicatedObject != null)
        {
            Destroy(duplicatedObject);
        }

        originalObject = sourceObject;
        duplicatedObject = Instantiate(sourceObject, duplicateSpawnPoint.position, duplicateSpawnPoint.rotation);
        duplicatedObject.name = sourceObject.name + "_Mirror";

        // Appliquer l'effet miroir sur l'échelle dès l'instanciation
        Vector3 initialScale = sourceObject.transform.localScale;
        if (applyMirrorRotation)
        {
            initialScale.x = -initialScale.x;
        }
        duplicatedObject.transform.localScale = initialScale;

        DisablePhysicsComponents(duplicatedObject);
    }
    
    /// <summary>
    /// Met à jour la position et rotation de l'objet dupliqué pour suivre le miroir
    /// </summary>
    private void UpdateDuplicateTransform()
    {
        if (duplicatedObject == null || originalObject == null)
            return;
        
        DetectObject mirrorDetector = (activeDetector == rightHandDetector) ? leftHandDetector : rightHandDetector;
        
        if (mirrorDetector != null)
        {
            // Calculer la position relative de l'objet par rapport à la main active
            Vector3 localPos = activeDetector.transform.InverseTransformPoint(originalObject.transform.position);
            
            // Inverser l'axe X (généralement utilisé pour la symétrie gauche/droite des mains dans Unity)
            localPos.x = -localPos.x;
            
            duplicatedObject.transform.position = mirrorDetector.transform.TransformPoint(localPos) + localOffset;
            
            // Appliquer la rotation miroir si activé
            if (applyMirrorRotation)
            {
                // On conserve la rotation relative intacte
                Quaternion localRot = Quaternion.Inverse(activeDetector.transform.rotation) * originalObject.transform.rotation;
                duplicatedObject.transform.rotation = mirrorDetector.transform.rotation * localRot;
            }
            else
            {
                duplicatedObject.transform.rotation = originalObject.transform.rotation;
            }
        }
        
        // Copier l'échelle et inverser l'axe X pour faire l'effet miroir parfait via le scale
        Vector3 newScale = originalObject.transform.localScale;
        if (applyMirrorRotation)
        {
            newScale.x = -newScale.x;
        }
        duplicatedObject.transform.localScale = newScale;
    }
    
    /// <summary>
    /// Désactive les composants physiques sur l'objet dupliqué
    /// </summary>
    private void DisablePhysicsComponents(GameObject obj)
    {
        // Désactiver l'enfant (ou les enfants) qui contient la logique Meta Building Blocks
        for (int i = 0; i < obj.transform.childCount; i++)
        {
            obj.transform.GetChild(i).gameObject.SetActive(false);
        }
        
        // Appliquer le layer IgnoreObject pour éviter les collisions (si configuré dans la matrice)
        int ignoreLayer = LayerMask.NameToLayer("IgnoreObject");
        if (ignoreLayer != -1)
        {
            SetLayerRecursively(obj, ignoreLayer);
        }

        // Changer le tag pour éviter que le trigger de l'autre main (DetectObject) ne détecte par erreur la copie
        SetTagRecursively(obj, "Untagged");

        // Ignorer explicitement la collision entre l'objet original et son clone
        if (originalObject != null)
        {
            Collider[] originalColliders = originalObject.GetComponentsInChildren<Collider>(true);
            Collider[] duplicateColliders = obj.GetComponentsInChildren<Collider>(true);

            foreach (var colOrig in originalColliders)
            {
                foreach (var colDup in duplicateColliders)
                {
                    Physics.IgnoreCollision(colOrig, colDup, true);
                }
            }
        }
    }
    
    /// <summary>
    /// Applique le layer récursivement à tous les enfants
    /// </summary>
    private void SetLayerRecursively(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }

    /// <summary>
    /// Applique un tag récursivement à tous les enfants
    /// </summary>
    private void SetTagRecursively(GameObject obj, string newTag)
    {
        obj.tag = newTag;
        foreach (Transform child in obj.transform)
        {
            SetTagRecursively(child.gameObject, newTag);
        }
    }
    
    /// <summary>
    /// Propriété publique pour savoir si on est en train de dupliquer
    /// </summary>
    public bool IsDuplicating => isTracking;
    
    /// <summary>
    /// Propriété publique pour accéder à l'objet dupliqué
    /// </summary>
    public GameObject DuplicatedObject => duplicatedObject;
    
    /// <summary>
    /// Quelle main tient actuellement l'objet (null si aucune)
    /// </summary>
    public DetectObject ActiveDetector => activeDetector;
    
    /// <summary>
    /// Force l'arrêt de la duplication (utile pour les scripts externes)
    /// </summary>
    public void ForceStopDuplication()
    {
        StopDuplication();
    }
    
    private void OnDestroy()
    {
        // Nettoyer l'objet dupliqué si le script est détruit
        if (duplicatedObject != null)
        {
            Destroy(duplicatedObject);
        }
    }
}
