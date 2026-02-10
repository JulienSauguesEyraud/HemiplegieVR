using UnityEngine;

/// <summary>
/// Génère un avant-bras procédural entre le poignet et le coude
/// S'adapte dynamiquement à la position et l'orientation de la main
/// </summary>
public class ProceduralForearm : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Le transform du poignet (wrist joint de la main)")]
    [SerializeField] private Transform wrist;
    
    [Tooltip("Le transform du coude (peut être estimé ou réel)")]
    [SerializeField] private Transform elbow;
    
    [Tooltip("La caméra/tête pour estimer la position du coude si non fourni")]
    [SerializeField] private Transform headReference;

    [Header("Visual Settings")]
    [Tooltip("Rayon de l'avant-bras")]
    [SerializeField] private float radius = 0.020f; // Réduit de 0.035f à 0.020f (2cm au lieu de 3.5cm)
    
    [Tooltip("Longueur minimale de l'avant-bras")]
    [SerializeField] private float minLength = 0.05f;
    
    [Tooltip("Longueur maximale de l'avant-bras")]
    [SerializeField] private float maxLength = 0.30f;
    
    [Tooltip("Longueur estimée de l'avant-bras (utilisée si elbow n'est pas fourni)")]
    [SerializeField] private float estimatedLength = 0.25f;
    
    [Tooltip("Distance de recul depuis le poignet (pour éviter que l'avant-bras rentre dans le poignet)")]
    [SerializeField] private float wristOffset = 0.01f;
    
    [Tooltip("Offset horizontal depuis la tête pour estimer le coude (+ = droite)")]
    [SerializeField] private float elbowHorizontalOffset = 0.25f;
    
    [Tooltip("Offset vertical depuis la tête pour estimer le coude (+ = haut)")]
    [SerializeField] private float elbowVerticalOffset = -0.15f;
    
    [Tooltip("Offset avant/arrière depuis la tête pour estimer le coude (+ = avant)")]
    [SerializeField] private float elbowDepthOffset = 0.0f;

    [Header("Wrist Rotation Adjustment")]
    [Tooltip("Intensité du décalage latéral basé sur la rotation du poignet (0 = pas de décalage)")]
    [SerializeField] private float wristRotationInfluence = 0.08f;
    
    [Tooltip("Intensité du décalage vertical basé sur la rotation du poignet")]
    [SerializeField] private float wristRotationVerticalInfluence = 0.04f;
    
    [Tooltip("Lissage du mouvement du coude (plus élevé = plus fluide mais moins réactif)")]
    [SerializeField][Range(0f, 0.95f)] private float elbowSmoothing = 0.7f;

    [Header("Material Settings")]
    [Tooltip("Matériau à utiliser pour l'avant-bras (copié depuis les mains)")]
    [SerializeField] private Material handMaterial;
    
    [Tooltip("Opacité de l'avant-bras (0 = invisible, 1 = opaque)")]
    [SerializeField][Range(0f, 1f)] private float opacity = 0.7f;
    
    [Tooltip("Utiliser une transparence pour un effet thérapeutique")]
    [SerializeField] private bool useTransparency = true;

    private GameObject forearmMesh;
    private MeshRenderer meshRenderer;
    private Material forearmMaterial;
    private Vector3 estimatedElbowPosition;
    private Vector3 smoothedElbowPosition;
    private Quaternion previousWristRotation;
    private bool useEstimatedElbow = false;
    private bool isInitialized = false;

    void Start()
    {
        CreateForearmMesh();
        useEstimatedElbow = (elbow == null);
        
        if (useEstimatedElbow && headReference == null)
        {
            Debug.LogWarning("[ProceduralForearm] Ni coude ni référence tête fournie. L'avant-bras ne s'affichera pas correctement.");
        }

        if (wrist != null)
        {
            previousWristRotation = wrist.rotation;
        }
    }

    void LateUpdate()
    {
        if (wrist == null || !wrist.gameObject.activeInHierarchy)
        {
            if (forearmMesh != null)
                forearmMesh.SetActive(false);
            return;
        }

        if (forearmMesh != null && !forearmMesh.activeSelf)
            forearmMesh.SetActive(true);

        UpdateElbowPosition();
        UpdateForearmTransform();
        UpdateMaterial();
    }

    /// <summary>
    /// Crée le mesh cylindrique pour l'avant-bras
    /// </summary>
    private void CreateForearmMesh()
    {
        // Créer un objet enfant pour le mesh
        forearmMesh = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        forearmMesh.name = "ForearmMesh";
        forearmMesh.transform.SetParent(transform);
        
        // Enlever le collider (pas besoin pour du visuel)
        Collider col = forearmMesh.GetComponent<Collider>();
        if (col != null)
            Destroy(col);

        // Récupérer et configurer le renderer
        meshRenderer = forearmMesh.GetComponent<MeshRenderer>();
        
        // Utiliser le matériau de la main si disponible
        if (handMaterial != null)
        {
            // Créer une instance du matériau pour pouvoir modifier l'opacité
            forearmMaterial = new Material(handMaterial);
            
            // Si on veut de la transparence, configurer le matériau
            if (useTransparency && opacity < 1.0f)
            {
                SetupTransparentMaterial(forearmMaterial);
            }
        }
        else
        {
            // Fallback : créer un matériau simple
            forearmMaterial = new Material(Shader.Find("Standard"));
            forearmMaterial.color = new Color(0.95f, 0.85f, 0.75f, opacity);
            
            if (useTransparency)
            {
                SetupTransparentMaterial(forearmMaterial);
            }
        }
        
        meshRenderer.material = forearmMaterial;
    }

    /// <summary>
    /// Configure un matériau pour la transparence
    /// </summary>
    private void SetupTransparentMaterial(Material mat)
    {
        // Vérifier si le shader supporte le mode transparent
        if (mat.HasProperty("_Mode"))
        {
            mat.SetFloat("_Mode", 3); // Transparent mode
        }
        
        if (mat.HasProperty("_SrcBlend"))
        {
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
        }
        
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = 3000;
    }

    /// <summary>
    /// Calcule le décalage du coude basé sur la rotation du poignet
    /// </summary>
    private Vector3 GetWristRotationOffset()
    {
        if (wrist == null || headReference == null)
            return Vector3.zero;

        // Calculer l'axe de rotation (l'axe de l'avant-bras)
        Vector3 forearmAxis = (wrist.position - estimatedElbowPosition).normalized;
        
        // Obtenir l'axe "up" du poignet dans l'espace monde
        Vector3 wristUp = wrist.up;
        
        // Obtenir l'axe "right" du poignet dans l'espace monde
        Vector3 wristRight = wrist.right;
        
        // Calculer l'angle de rotation du poignet autour de l'axe de l'avant-bras
        // En projetant wristRight sur le plan perpendiculaire à l'avant-bras
        Vector3 projectedRight = Vector3.ProjectOnPlane(wristRight, forearmAxis);
        Vector3 projectedUp = Vector3.ProjectOnPlane(wristUp, forearmAxis);
        
        // Calculer le vecteur de référence (direction vers l'extérieur depuis la tête)
        Vector3 headToWrist = (wrist.position - headReference.position).normalized;
        Vector3 outwardDirection = Vector3.ProjectOnPlane(headToWrist, forearmAxis);
        
        if (outwardDirection.sqrMagnitude < 0.001f)
            return Vector3.zero;
        
        outwardDirection.Normalize();
        
        // Calculer l'angle de rotation du poignet
        float rotationAngle = Vector3.SignedAngle(outwardDirection, projectedUp, forearmAxis);
        
        // Normaliser l'angle entre -180 et 180
        if (rotationAngle > 180f) rotationAngle -= 360f;
        if (rotationAngle < -180f) rotationAngle += 360f;
        
        // Convertir l'angle en radians pour les calculs
        float rotationRadians = rotationAngle * Mathf.Deg2Rad;
        
        // Calculer le décalage latéral (perpendiculaire à l'avant-bras et à la verticale)
        Vector3 lateralAxis = Vector3.Cross(forearmAxis, Vector3.up).normalized;
        if (lateralAxis.sqrMagnitude < 0.001f)
        {
            // Fallback si l'avant-bras est vertical
            lateralAxis = Vector3.Cross(forearmAxis, headReference.forward).normalized;
        }
        
        // Le décalage latéral suit un pattern sinusoïdal basé sur la rotation
        float lateralOffset = Mathf.Sin(rotationRadians) * wristRotationInfluence;
        
        // Le décalage vertical suit aussi la rotation (légèrement différent)
        float verticalOffset = -Mathf.Abs(Mathf.Sin(rotationRadians * 0.5f)) * wristRotationVerticalInfluence;
        
        // Combiner les offsets
        Vector3 totalOffset = lateralAxis * lateralOffset + Vector3.up * verticalOffset;
        
        return totalOffset;
    }

    /// <summary>
    /// Estime ou récupère la position du coude
    /// </summary>
    private void UpdateElbowPosition()
    {
        Vector3 targetElbowPosition;
        
        if (useEstimatedElbow)
        {
            if (headReference != null)
            {
                // Estimer la position du coude à partir de la tête
                targetElbowPosition = headReference.position
                    + headReference.right * elbowHorizontalOffset
                    + headReference.up * elbowVerticalOffset
                    + headReference.forward * elbowDepthOffset;
            }
            else
            {
                // Fallback : position simple derrière le poignet
                targetElbowPosition = wrist.position - wrist.forward * estimatedLength;
            }
        }
        else
        {
            targetElbowPosition = elbow.position;
        }
        
        // Stocker la position de base avant d'appliquer le décalage
        estimatedElbowPosition = targetElbowPosition;
        
        // Appliquer le décalage basé sur la rotation du poignet
        Vector3 rotationOffset = GetWristRotationOffset();
        targetElbowPosition += rotationOffset;
        
        // Initialiser la position lissée lors de la première frame
        if (!isInitialized)
        {
            smoothedElbowPosition = targetElbowPosition;
            isInitialized = true;
        }
        
        // Lisser le mouvement du coude pour éviter les saccades
        smoothedElbowPosition = Vector3.Lerp(smoothedElbowPosition, targetElbowPosition, 1f - elbowSmoothing);
        
        // Mettre à jour la position finale du coude
        estimatedElbowPosition = smoothedElbowPosition;
        
        // Sauvegarder la rotation actuelle pour la prochaine frame
        if (wrist != null)
        {
            previousWristRotation = wrist.rotation;
        }
    }

    /// <summary>
    /// Met à jour la position, rotation et scale de l'avant-bras
    /// </summary>
    private void UpdateForearmTransform()
    {
        if (forearmMesh == null || wrist == null)
            return;

        Vector3 wristPos = wrist.position;
        Vector3 elbowPos = estimatedElbowPosition;

        // Direction de l'avant-bras
        Vector3 direction = elbowPos - wristPos;
        float totalLength = direction.magnitude;

        if (totalLength < 0.001f || direction == Vector3.zero)
            return;

        // Calculer le point de départ de l'avant-bras (reculé depuis le poignet)
        Vector3 forearmStart = wristPos + direction.normalized * wristOffset;
        
        // Recalculer la direction et la longueur depuis le nouveau point de départ
        direction = elbowPos - forearmStart;
        float length = direction.magnitude;

        // Limiter la longueur entre min et max
        length = Mathf.Clamp(length, minLength, maxLength);

        // Recalculer la position du coude avec la longueur limitée
        if (direction != Vector3.zero)
        {
            elbowPos = forearmStart + direction.normalized * length;
        }

        // Position au milieu de l'avant-bras (entre le point de départ reculé et le coude)
        forearmMesh.transform.position = forearmStart + (elbowPos - forearmStart) * 0.5f;

        // Rotation pour pointer vers le coude
        // Le cylindre Unity pointe vers le haut (Y), on doit donc le tourner
        if (direction != Vector3.zero)
        {
            forearmMesh.transform.rotation = Quaternion.FromToRotation(Vector3.up, direction);
        }

        // Scale : diamètre en X et Z, longueur en Y
        forearmMesh.transform.localScale = new Vector3(radius * 2f, length * 0.5f, radius * 2f);
    }

    /// <summary>
    /// Met à jour le matériau (opacité)
    /// </summary>
    private void UpdateMaterial()
    {
        if (forearmMaterial != null && useTransparency)
        {
            // Mettre à jour l'alpha si le matériau le supporte
            if (forearmMaterial.HasProperty("_Color"))
            {
                Color currentColor = forearmMaterial.color;
                currentColor.a = opacity;
                forearmMaterial.color = currentColor;
            }
        }
    }

    /// <summary>
    /// Configure le poignet de référence
    /// </summary>
    public void SetWrist(Transform wristTransform)
    {
        wrist = wristTransform;
    }

    /// <summary>
    /// Configure le coude de référence
    /// </summary>
    public void SetElbow(Transform elbowTransform)
    {
        elbow = elbowTransform;
        useEstimatedElbow = (elbow == null);
    }

    /// <summary>
    /// Configure la référence de tête pour l'estimation du coude
    /// </summary>
    public void SetHeadReference(Transform head)
    {
        headReference = head;
    }

    /// <summary>
    /// Définit le matériau à utiliser (copié depuis les mains)
    /// </summary>
    public void SetHandMaterial(Material material)
    {
        if (material == null)
            return;

        handMaterial = material;
        
        // Recréer le matériau avec le nouveau matériau de référence
        if (forearmMaterial != null)
        {
            Destroy(forearmMaterial);
        }
        
        forearmMaterial = new Material(material);
        
        if (useTransparency && opacity < 1.0f)
        {
            SetupTransparentMaterial(forearmMaterial);
            if (forearmMaterial.HasProperty("_Color"))
            {
                Color c = forearmMaterial.color;
                c.a = opacity;
                forearmMaterial.color = c;
            }
        }
        
        if (meshRenderer != null)
        {
            meshRenderer.material = forearmMaterial;
        }
    }

    /// <summary>
    /// Définit l'opacité de l'avant-bras
    /// </summary>
    public void SetOpacity(float newOpacity)
    {
        opacity = Mathf.Clamp01(newOpacity);
    }

    /// <summary>
    /// Active ou désactive l'affichage de l'avant-bras
    /// </summary>
    public void SetVisible(bool visible)
    {
        if (forearmMesh != null)
        {
            forearmMesh.SetActive(visible);
        }
    }

    void OnDestroy()
    {
        if (forearmMaterial != null)
            Destroy(forearmMaterial);
        if (forearmMesh != null)
            Destroy(forearmMesh);
    }
}
