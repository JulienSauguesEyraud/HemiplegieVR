using UnityEngine;

public class DuplicateObject : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DetectObject rightHandDetector;
    [SerializeField] private DetectObject leftHandDetector;
    [SerializeField] private MirrorHand mirrorHand;
    
    [Header("Duplication Settings")]
    [Tooltip("Parent pour l'objet dupliqué (optionnel)")]
    [SerializeField] private Transform duplicateParent;
    
    [Tooltip("Décalage local à appliquer à l'objet dupliqué")]
    [SerializeField] private Vector3 localOffset = Vector3.zero;
    
    [Tooltip("Appliquer la rotation miroir à l'objet dupliqué")]
    [SerializeField] private bool applyMirrorRotation = true;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;
    
    private GameObject duplicatedObject;
    private GameObject originalObject;
    private bool isDuplicating = false;
    private DetectObject activeDetector; // La main qui tient actuellement l'objet
    private bool hasStartedMoving = false; // Pour tracker si l'objet a commencé à bouger au moins une fois
    
    private void Update()
    {
        // Déterminer quelle main détecte l'objet (priorité à celle qui l'a attrapé en premier)
        DetectObject currentDetector = GetActiveDetector();
        
        // Si on n'a pas encore commencé la duplication
        if (!isDuplicating)
        {
            // Vérifier si les conditions sont remplies pour démarrer
            if (ShouldStartDuplication(currentDetector))
            {
                activeDetector = currentDetector;
                hasStartedMoving = true;
                StartDuplication();
            }
        }
        else
        {
            // Si on est en train de dupliquer, mettre à jour la position
            if (duplicatedObject != null && originalObject != null)
            {
                UpdateDuplicateTransform();
            }
            
            // Arrêter la duplication UNIQUEMENT si l'objet n'est plus détecté par AUCUNE main
            if (!IsObjectDetectedByAnyHand())
            {
                StopDuplication();
            }
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
            if (showDebugLogs) Debug.Log("[DuplicateObject] Miroir non activé");
            return false;
        }
        
        if (detector == null)
        {
            if (showDebugLogs) Debug.Log("[DuplicateObject] Aucun détecteur actif");
            return false;
        }
        
        if (!detector.IsObjectDetected)
        {
            if (showDebugLogs) Debug.Log("[DuplicateObject] Objet non détecté");
            return false;
        }
        
        if (!detector.IsObjectMoving)
        {
            if (showDebugLogs) Debug.Log("[DuplicateObject] Objet non en mouvement");
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
            Debug.LogWarning("[DuplicateObject] activeDetector est null dans StartDuplication");
            return;
        }
        
        originalObject = activeDetector.DetectedObject;
        
        if (originalObject == null)
        {
            Debug.LogWarning("[DuplicateObject] Impossible de dupliquer : objet null");
            return;
        }
        
        // Créer la copie
        duplicatedObject = Instantiate(originalObject);
        duplicatedObject.name = originalObject.name + "_Mirror";
        
        // Définir le parent si spécifié
        if (duplicateParent != null)
        {
            duplicatedObject.transform.SetParent(duplicateParent);
        }
        
        // Désactiver les composants physiques sur la copie pour éviter les interférences
        DisablePhysicsComponents(duplicatedObject);
        
        // Mettre à jour la transformation
        UpdateDuplicateTransform();
        
        isDuplicating = true;
        
        string handName = (activeDetector == rightHandDetector) ? "droite" : "gauche";
        Debug.Log($"[DuplicateObject] ? Duplication démarrée pour {originalObject.name} (main {handName})");
    }
    
    /// <summary>
    /// Arrête la duplication et détruit l'objet dupliqué
    /// </summary>
    private void StopDuplication()
    {
        if (duplicatedObject != null)
        {
            string handName = activeDetector != null ? 
                ((activeDetector == rightHandDetector) ? "droite" : "gauche") : 
                "inconnue";
            Debug.Log($"[DuplicateObject] ? Duplication arrêtée pour {originalObject?.name ?? "objet inconnu"} (main {handName})");
            Destroy(duplicatedObject);
        }
        
        duplicatedObject = null;
        originalObject = null;
        activeDetector = null;
        isDuplicating = false;
        hasStartedMoving = false;
    }
    
    /// <summary>
    /// Met à jour la position et rotation de l'objet dupliqué pour suivre le miroir
    /// </summary>
    private void UpdateDuplicateTransform()
    {
        if (duplicatedObject == null || originalObject == null)
            return;
        
        // Calculer la position miroir de l'objet
        Vector3 mirroredPosition = GetMirrorPosition(originalObject.transform.position);
        duplicatedObject.transform.position = mirroredPosition + localOffset;
        
        // Appliquer la rotation miroir si activé
        if (applyMirrorRotation)
        {
            Quaternion mirroredRotation = GetMirrorRotation(originalObject.transform.rotation);
            duplicatedObject.transform.rotation = mirroredRotation;
        }
        else
        {
            duplicatedObject.transform.rotation = originalObject.transform.rotation;
        }
        
        // Copier l'échelle
        duplicatedObject.transform.localScale = originalObject.transform.localScale;
    }
    
    /// <summary>
    /// Calcule la position miroir d'un point
    /// </summary>
    private Vector3 GetMirrorPosition(Vector3 position)
    {
        if (mirrorHand == null)
            return position;
        
        // Utiliser la même logique de miroir que MirrorHand
        Transform eye = GetSelectedEye();
        if (eye == null)
            return position;
        
        // Plan de miroir
        Vector3 mirrorPlanePoint = eye.position;
        Vector3 eyeRight = eye.right;
        Vector3 mirrorPlaneNormal = new Vector3(eyeRight.x, 0, eyeRight.z).normalized;
        
        // Si la normale est trop petite, utiliser forward
        if (mirrorPlaneNormal.sqrMagnitude < 0.001f)
        {
            Vector3 eyeForward = eye.forward;
            mirrorPlaneNormal = new Vector3(-eyeForward.z, 0, eyeForward.x).normalized;
        }
        
        // Réflexion de la position
        float distance = Vector3.Dot(position - mirrorPlanePoint, mirrorPlaneNormal);
        return position - 2 * distance * mirrorPlaneNormal;
    }
    
    /// <summary>
    /// Calcule la rotation miroir d'une rotation
    /// </summary>
    private Quaternion GetMirrorRotation(Quaternion rotation)
    {
        Transform eye = GetSelectedEye();
        if (eye == null)
            return rotation;
        
        // Plan de miroir
        Vector3 eyeRight = eye.right;
        Vector3 mirrorPlaneNormal = new Vector3(eyeRight.x, 0, eyeRight.z).normalized;
        
        if (mirrorPlaneNormal.sqrMagnitude < 0.001f)
        {
            Vector3 eyeForward = eye.forward;
            mirrorPlaneNormal = new Vector3(-eyeForward.z, 0, eyeForward.x).normalized;
        }
        
        // Réflexion des axes
        Vector3 forward = rotation * Vector3.forward;
        Vector3 up = rotation * Vector3.up;
        
        Vector3 reflectedForward = ReflectDirection(forward, mirrorPlaneNormal);
        Vector3 reflectedUp = ReflectDirection(up, mirrorPlaneNormal);
        
        if (reflectedForward != Vector3.zero && reflectedUp != Vector3.zero)
        {
            return Quaternion.LookRotation(reflectedForward, reflectedUp);
        }
        return rotation;
    }
    
    /// <summary>
    /// Réfléchit une direction par rapport au plan de miroir
    /// </summary>
    private Vector3 ReflectDirection(Vector3 direction, Vector3 normal)
    {
        return direction - 2 * Vector3.Dot(direction, normal) * normal;
    }
    
    /// <summary>
    /// Retourne le transform de l'œil sélectionné dans MirrorHand
    /// </summary>
    private Transform GetSelectedEye()
    {
        if (mirrorHand == null)
            return null;
        
        // Utiliser la réflexion pour accéder aux références d'yeux privées
        var mirrorHandType = mirrorHand.GetType();
        
        // Récupérer l'EyeReference actuel
        var eyeRefField = mirrorHandType.GetProperty("CurrentEyeReference");
        if (eyeRefField == null)
            return null;
        
        EyeReference eyeRef = (EyeReference)eyeRefField.GetValue(mirrorHand);
        
        // Récupérer le Transform correspondant
        Transform centerEye = GetPrivateField<Transform>(mirrorHand, "centerEye");
        Transform leftEye = GetPrivateField<Transform>(mirrorHand, "leftEye");
        Transform rightEye = GetPrivateField<Transform>(mirrorHand, "rightEye");
        
        switch (eyeRef)
        {
            case EyeReference.LeftEye:
                return leftEye;
            case EyeReference.RightEye:
                return rightEye;
            case EyeReference.CenterEye:
            default:
                return centerEye;
        }
    }
    
    /// <summary>
    /// Récupère un champ privé via réflexion
    /// </summary>
    private T GetPrivateField<T>(object obj, string fieldName) where T : class
    {
        var field = obj.GetType().GetField(fieldName, 
            System.Reflection.BindingFlags.NonPublic | 
            System.Reflection.BindingFlags.Instance);
        
        if (field != null)
        {
            return field.GetValue(obj) as T;
        }
        return null;
    }
    
    /// <summary>
    /// Désactive les composants physiques sur l'objet dupliqué
    /// </summary>
    private void DisablePhysicsComponents(GameObject obj)
    {
        // Désactiver les Rigidbody
        Rigidbody[] rigidbodies = obj.GetComponentsInChildren<Rigidbody>();
        foreach (var rb in rigidbodies)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
        
        // Désactiver les Colliders pour éviter les interactions physiques
        Collider[] colliders = obj.GetComponentsInChildren<Collider>();
        foreach (var col in colliders)
        {
            col.enabled = false;
        }
        
        if (showDebugLogs)
        {
            Debug.Log($"[DuplicateObject] {rigidbodies.Length} Rigidbodies et {colliders.Length} Colliders désactivés sur {obj.name}");
        }
    }
    
    /// <summary>
    /// Propriété publique pour savoir si on est en train de dupliquer
    /// </summary>
    public bool IsDuplicating => isDuplicating;
    
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
