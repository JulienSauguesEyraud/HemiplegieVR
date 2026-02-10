using UnityEngine;
using System.Collections.Generic;

public enum SourceHand
{
    Right,
    Left
}

public enum EyeReference
{
    CenterEye,
    LeftEye,
    RightEye
}

public class MirrorHand : MonoBehaviour
{
    [Header("Roots")]
    [SerializeField] private Transform rightHandRoot;
    [SerializeField] private Transform leftHandRoot;
    [SerializeField] private GameObject rightInteraction;
    [SerializeField] private GameObject leftInteraction;

    [Header("Forearm References")]
    [Tooltip("Transform du poignet droit (wrist) - cherche automatiquement 'b_r_wrist' si vide")]
    [SerializeField] private Transform rightWrist;
    [Tooltip("Transform du poignet gauche (wrist) - cherche automatiquement 'b_l_wrist' si vide")]
    [SerializeField] private Transform leftWrist;
    [Tooltip("Active l'affichage des avant-bras")]
    [SerializeField] private bool showForearms = true;

    [Header("Eye References")]
    [Tooltip("L'oeil central (CenterEyeAnchor)")]
    [SerializeField] private Transform centerEye;
    [Tooltip("L'oeil gauche (LeftEyeAnchor)")]
    [SerializeField] private Transform leftEye;
    [Tooltip("L'oeil droit (RightEyeAnchor)")]
    [SerializeField] private Transform rightEye;
    [Tooltip("Quel oeil utiliser comme référence pour le plan de miroir")]
    [SerializeField] private EyeReference eyeReference = EyeReference.CenterEye;

    [Header("Mirror Therapy Settings")]
    [SerializeField] private bool enableMirrorTherapy = false;
    [SerializeField] private SourceHand sourceHand = SourceHand.Right;
    [Tooltip("Désactive automatiquement les composants de hand tracking sur la main miroir")]
    [SerializeField] private bool disableMirrorHandTracking = true;
    
    [Header("Collision Settings")]
    [Tooltip("Distance minimale entre les mains (empêche les mains de se traverser)")]
    [SerializeField] private float minimumHandDistance = 0.05f;

    [Header("Forearm Settings")]
    [Tooltip("Rayon de l'avant-bras")]
    [SerializeField] private float forearmRadius = 0.025f;
    [Tooltip("Longueur minimale de l'avant-bras")]
    [SerializeField] private float forearmMinLength = 0.05f;
    [Tooltip("Longueur maximale de l'avant-bras")]
    [SerializeField] private float forearmMaxLength = 0.30f;
    [Tooltip("Distance de recul depuis le poignet (pour éviter que l'avant-bras rentre dans le poignet)")]
    [SerializeField] private float forearmWristOffset = 0.01f;
    [Tooltip("Opacité des avant-bras (0 = invisible, 1 = opaque)")]
    [SerializeField][Range(0f, 1f)] private float forearmOpacity = 0.7f;
    [Tooltip("Utiliser la transparence pour les avant-bras")]
    [SerializeField] private bool forearmUseTransparency = true;
    [Tooltip("Offset horizontal du coude depuis la tête (+ = vers l'extérieur)")]
    [SerializeField] private float elbowHorizontalOffset = 0.25f;
    [Tooltip("Offset vertical du coude depuis la tête (+ = haut)")]
    [SerializeField] private float elbowVerticalOffset = -0.15f;
    
    [Header("Wrist Rotation Effects")]
    [Tooltip("Intensité du décalage latéral quand on tourne le poignet (0 = pas de décalage)")]
    [SerializeField] private float wristRotationInfluence = 0.08f;
    [Tooltip("Intensité du décalage vertical quand on tourne le poignet")]
    [SerializeField] private float wristRotationVerticalInfluence = 0.04f;
    [Tooltip("Lissage du mouvement du coude (plus élevé = plus fluide)")]
    [SerializeField][Range(0f, 0.95f)] private float elbowSmoothing = 0.7f;

    private Dictionary<string, Transform> mirrorJointsByName = new();
    private SkinnedMeshRenderer[] mirrorHandRenderers;
    private Renderer[] mirrorHandAllRenderers;
    private SourceHand lastSourceHand;

    // Plan de miroir
    private Vector3 mirrorPlaneNormal;
    private Vector3 mirrorPlanePoint;

    // Avant-bras procéduraux
    private ProceduralForearm rightForearm;
    private ProceduralForearm leftForearm;
    private GameObject rightForearmObj;
    private GameObject leftForearmObj;

    void Start()
    {
        lastSourceHand = sourceHand;
        InitializeMirror();
        InitializeForearms();
    }

    private void InitializeMirror()
    {
        if (disableMirrorHandTracking)
        {
            DisableMirrorHandTracking();
        }

        CacheMirrorHandJoints();
        CacheMirrorHandRenderers();

        if (enableMirrorTherapy)
        {
            ForceShowMirrorHand();
        }
    }

    /// <summary>
    /// Initialise les avant-bras procéduraux
    /// </summary>
    private void InitializeForearms()
    {
        // Chercher automatiquement les poignets si non assignés
        if (rightWrist == null && rightHandRoot != null)
        {
            rightWrist = FindWrist(rightHandRoot, "right");
        }
        if (leftWrist == null && leftHandRoot != null)
        {
            leftWrist = FindWrist(leftHandRoot, "left");
        }

        // Récupérer le matériau des mains
        Material handMaterial = GetHandMaterial();

        // Créer l'avant-bras droit
        if (rightWrist != null)
        {
            rightForearmObj = new GameObject("RightForearm");
            rightForearmObj.transform.SetParent(transform);
            rightForearm = rightForearmObj.AddComponent<ProceduralForearm>();
            
            ConfigureForearm(rightForearm, rightWrist, elbowHorizontalOffset, handMaterial);
        }

        // Créer l'avant-bras gauche
        if (leftWrist != null)
        {
            leftForearmObj = new GameObject("LeftForearm");
            leftForearmObj.transform.SetParent(transform);
            leftForearm = leftForearmObj.AddComponent<ProceduralForearm>();
            
            ConfigureForearm(leftForearm, leftWrist, -elbowHorizontalOffset, handMaterial); // Négatif pour la main gauche
        }

        UpdateForearmVisibility();
    }

    /// <summary>
    /// Récupère le matériau des mains pour l'appliquer aux avant-bras
    /// </summary>
    private Material GetHandMaterial()
    {
        // Chercher dans les deux mains
        Transform[] hands = new Transform[] { rightHandRoot, leftHandRoot };
        
        foreach (Transform hand in hands)
        {
            if (hand == null)
                continue;

            // Chercher un SkinnedMeshRenderer avec un matériau
            SkinnedMeshRenderer[] renderers = hand.GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (var renderer in renderers)
            {
                if (renderer != null && renderer.sharedMaterial != null)
                {
                    Debug.Log($"[MirrorHand] Matériau trouvé sur {renderer.gameObject.name}: {renderer.sharedMaterial.name}");
                    return renderer.sharedMaterial;
                }
            }

            // Chercher un MeshRenderer en fallback
            MeshRenderer[] meshRenderers = hand.GetComponentsInChildren<MeshRenderer>();
            foreach (var renderer in meshRenderers)
            {
                if (renderer != null && renderer.sharedMaterial != null)
                {
                    Debug.Log($"[MirrorHand] Matériau trouvé sur {renderer.gameObject.name}: {renderer.sharedMaterial.name}");
                    return renderer.sharedMaterial;
                }
            }
        }

        Debug.LogWarning("[MirrorHand] Aucun matériau trouvé sur les mains. Utilisation du matériau par défaut.");
        return null;
    }

    /// <summary>
    /// Configure un avant-bras procédural
    /// </summary>
    private void ConfigureForearm(ProceduralForearm forearm, Transform wrist, float horizontalOffset, Material handMaterial)
    {
        forearm.SetWrist(wrist);
        forearm.SetHeadReference(GetSelectedEye());
        
        // Définir le matériau de la main
        if (handMaterial != null)
        {
            forearm.SetHandMaterial(handMaterial);
        }
        
        // Configuration via réflexion pour éviter les erreurs si les champs ne sont pas publics
        var forearmType = forearm.GetType();
        
        SetPrivateField(forearm, "radius", forearmRadius);
        SetPrivateField(forearm, "minLength", forearmMinLength);
        SetPrivateField(forearm, "maxLength", forearmMaxLength);
        SetPrivateField(forearm, "wristOffset", forearmWristOffset);
        SetPrivateField(forearm, "opacity", forearmOpacity);
        SetPrivateField(forearm, "useTransparency", forearmUseTransparency);
        SetPrivateField(forearm, "elbowHorizontalOffset", horizontalOffset);
        SetPrivateField(forearm, "elbowVerticalOffset", elbowVerticalOffset);
        SetPrivateField(forearm, "wristRotationInfluence", wristRotationInfluence);
        SetPrivateField(forearm, "wristRotationVerticalInfluence", wristRotationVerticalInfluence);
        SetPrivateField(forearm, "elbowSmoothing", elbowSmoothing);
        
        // Nouveaux paramètres de rotation du poignet
        SetPrivateField(forearm, "wristRotationOffset", 0f);
        SetPrivateField(forearm, "wristRotationVerticalOffset", 0f);
    }

    /// <summary>
    /// Définit un champ privé via réflexion
    /// </summary>
    private void SetPrivateField(object obj, string fieldName, object value)
    {
        var field = obj.GetType().GetField(fieldName, 
            System.Reflection.BindingFlags.NonPublic | 
            System.Reflection.BindingFlags.Instance);
        if (field != null)
        {
            field.SetValue(obj, value);
        }
    }

    /// <summary>
    /// Cherche le transform du poignet dans la hiérarchie de la main
    /// </summary>
    private Transform FindWrist(Transform handRoot, string side)
    {
        // Patterns de noms courants pour le poignet
        string[] wristPatterns = new string[]
        {
            $"b_{side[0]}_wrist",  // b_r_wrist ou b_l_wrist
            $"{side}_wrist",        // right_wrist ou left_wrist
            $"Wrist_{side[0].ToString().ToUpper()}", // Wrist_R ou Wrist_L
            "wrist",
            "Wrist"
        };

        foreach (Transform child in handRoot.GetComponentsInChildren<Transform>())
        {
            string childName = child.name.ToLower();
            foreach (string pattern in wristPatterns)
            {
                if (childName.Contains(pattern.ToLower()))
                {
                    Debug.Log($"[MirrorHand] Poignet {side} trouvé : {child.name}");
                    return child;
                }
            }
        }

        Debug.LogWarning($"[MirrorHand] Poignet {side} non trouvé. Recherchez dans la hiérarchie.");
        return null;
    }

    /// <summary>
    /// Met à jour la visibilité des avant-bras selon l'état du miroir
    /// </summary>
    private void UpdateForearmVisibility()
    {
        if (!showForearms || !enableMirrorTherapy)
        {
            // Masquer les deux avant-bras si désactivé
            if (rightForearm != null) rightForearm.SetVisible(false);
            if (leftForearm != null) leftForearm.SetVisible(false);
            return;
        }

        // Afficher l'avant-bras source et miroir
        if (sourceHand == SourceHand.Right)
        {
            if (rightForearm != null) rightForearm.SetVisible(true);
            if (leftForearm != null) leftForearm.SetVisible(true);
        }
        else
        {
            if (leftForearm != null) leftForearm.SetVisible(true);
            if (rightForearm != null) rightForearm.SetVisible(true);
        }
    }

    void LateUpdate()
    {
        // Vérifier si la main source a changé
        if (lastSourceHand != sourceHand)
        {
            lastSourceHand = sourceHand;
            InitializeMirror();
            UpdateForearmVisibility();
        }

        if (enableMirrorTherapy)
        {
            UpdateMirrorPlane();
            ForceShowMirrorHand();
            MirrorHands();
            UpdateForearmVisibility();
        }
    }

    /// <summary>
    /// Retourne le transform de l'oeil sélectionné
    /// </summary>
    private Transform GetSelectedEye()
    {
        switch (eyeReference)
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
    /// Met à jour le plan de miroir basé sur la position et l'orientation de l'oeil sélectionné
    /// Le plan est vertical et perpendiculaire à la direction du regard (axe X de l'oeil)
    /// </summary>
    private void UpdateMirrorPlane()
    {
        Transform eye = GetSelectedEye();
        if (eye == null)
        {
            Debug.LogWarning("[MirrorHand] Aucun oeil de référence assigné !");
            return;
        }

        // Le plan de miroir passe par la position de l'oeil
        mirrorPlanePoint = eye.position;

        // La normale du plan est l'axe X de l'oeil (droite/gauche), projeté sur le plan horizontal
        // pour garder le plan vertical même si on regarde en haut ou en bas
        Vector3 eyeRight = eye.right;
        mirrorPlaneNormal = new Vector3(eyeRight.x, 0, eyeRight.z).normalized;

        // Si la normale est trop petite (regard parfaitement vertical), utiliser forward
        if (mirrorPlaneNormal.sqrMagnitude < 0.001f)
        {
            Vector3 eyeForward = eye.forward;
            mirrorPlaneNormal = new Vector3(-eyeForward.z, 0, eyeForward.x).normalized;
        }
    }

    /// <summary>
    /// Réfléchit une position par rapport au plan de miroir
    /// </summary>
    private Vector3 ReflectPosition(Vector3 position)
    {
        // Distance signée du point au plan
        float distance = Vector3.Dot(position - mirrorPlanePoint, mirrorPlaneNormal);
        // Position réfléchie
        return position - 2 * distance * mirrorPlaneNormal;
    }

    /// <summary>
    /// Réfléchit une rotation par rapport au plan de miroir
    /// </summary>
    private Quaternion ReflectRotation(Quaternion rotation)
    {
        // Obtenir les axes de la rotation
        Vector3 forward = rotation * Vector3.forward;
        Vector3 up = rotation * Vector3.up;

        // Réfléchir les axes par rapport au plan
        Vector3 reflectedForward = ReflectDirection(forward);
        Vector3 reflectedUp = ReflectDirection(up);

        // Reconstruire la rotation à partir des axes réfléchis
        if (reflectedForward != Vector3.zero && reflectedUp != Vector3.zero)
        {
            return Quaternion.LookRotation(reflectedForward, reflectedUp);
        }
        return rotation;
    }

    /// <summary>
    /// Réfléchit une direction par rapport au plan de miroir
    /// </summary>
    private Vector3 ReflectDirection(Vector3 direction)
    {
        return direction - 2 * Vector3.Dot(direction, mirrorPlaneNormal) * mirrorPlaneNormal;
    }

    /// <summary>
    /// Empêche la main miroir de traverser la main source
    /// </summary>
    private Vector3 PreventHandOverlap(Vector3 sourcePos, Vector3 mirrorPos)
    {
        Vector3 toMirror = mirrorPos - sourcePos;
        float distance = toMirror.magnitude;

        // Si les mains sont trop proches, repousser la main miroir
        if (distance < minimumHandDistance)
        {
            if (distance > 0.001f)
            {
                // Repousser la main miroir dans la direction opposée
                Vector3 pushDirection = toMirror.normalized;
                return sourcePos + pushDirection * minimumHandDistance;
            }
            else
            {
                // Si les mains sont exactement au même endroit, utiliser la normale du plan
                return sourcePos + mirrorPlaneNormal * minimumHandDistance;
            }
        }

        return mirrorPos;
    }

    /// <summary>
    /// Empêche la main miroir de traverser le plan de miroir (elle doit rester de son côté)
    /// </summary>
    private Vector3 ClampToMirrorSide(Vector3 sourcePos, Vector3 mirrorPos)
    {
        // Calculer de quel côté du plan la main source se trouve
        float sourceDistance = Vector3.Dot(sourcePos - mirrorPlanePoint, mirrorPlaneNormal);
        float mirrorDistance = Vector3.Dot(mirrorPos - mirrorPlanePoint, mirrorPlaneNormal);

        // La main miroir doit être du côté opposé à la main source
        // Si sourceDistance > 0, mirrorDistance doit être < 0 (et vice versa)
        bool sourceOnPositiveSide = sourceDistance > 0;
        bool mirrorOnPositiveSide = mirrorDistance > 0;

        // Si la main miroir est du mauvais côté du plan, la ramener sur le plan
        if (sourceOnPositiveSide == mirrorOnPositiveSide)
        {
            // Projeter la main miroir sur le plan, puis la décaler légèrement du bon côté
            float offsetSign = sourceOnPositiveSide ? -1f : 1f;
            Vector3 projectedPos = mirrorPos - mirrorDistance * mirrorPlaneNormal;
            return projectedPos + offsetSign * minimumHandDistance * 0.5f * mirrorPlaneNormal;
        }

        return mirrorPos;
    }

    private Transform GetSourceHandRoot()
    {
        return sourceHand == SourceHand.Right ? rightHandRoot : leftHandRoot;
    }

    private Transform GetMirrorHandRoot()
    {
        return sourceHand == SourceHand.Right ? leftHandRoot : rightHandRoot;
    }

    public void SetEnableMirrorTherapy(bool enable)
    {
        enableMirrorTherapy = enable;
        if (enable)
        {
            InitializeMirror();
        }
        UpdateForearmVisibility();
    }

    public void SetSourceHand(SourceHand hand)
    {
        if (sourceHand != hand)
        {
            sourceHand = hand;
            InitializeMirror();
            UpdateForearmVisibility();
        }
    }

    public void SetEyeReference(EyeReference eye)
    {
        eyeReference = eye;
        
        // Mettre à jour la référence de tête pour les avant-bras
        Transform selectedEye = GetSelectedEye();
        if (rightForearm != null) rightForearm.SetHeadReference(selectedEye);
        if (leftForearm != null) leftForearm.SetHeadReference(selectedEye);
    }

    /// <summary>
    /// Active ou désactive l'affichage des avant-bras
    /// </summary>
    public void SetShowForearms(bool show)
    {
        showForearms = show;
        UpdateForearmVisibility();
    }

    /// <summary>
    /// Définit l'opacité des avant-bras
    /// </summary>
    public void SetForearmOpacity(float opacity)
    {
        forearmOpacity = Mathf.Clamp01(opacity);
        if (rightForearm != null) rightForearm.SetOpacity(forearmOpacity);
        if (leftForearm != null) leftForearm.SetOpacity(forearmOpacity);
    }

    /// <summary>
    /// Définit si les avant-bras utilisent la transparence
    /// </summary>
    public void SetForearmUseTransparency(bool useTransparency)
    {
        forearmUseTransparency = useTransparency;
        // Réinitialiser les avant-bras pour appliquer le changement
        if (rightForearmObj != null) Destroy(rightForearmObj);
        if (leftForearmObj != null) Destroy(leftForearmObj);
        InitializeForearms();
    }

    public bool IsEnabled => enableMirrorTherapy;
    public SourceHand CurrentSourceHand => sourceHand;
    public EyeReference CurrentEyeReference => eyeReference;
    public bool AreForeArmsVisible => showForearms;

    private void ForceShowMirrorHand()
    {
        var mirrorRoot = GetMirrorHandRoot();
        if (mirrorRoot == null)
            return;

        // S'assurer que le GameObject est actif
        if (!mirrorRoot.gameObject.activeInHierarchy)
        {
            mirrorRoot.gameObject.SetActive(true);
        }

        // Activer tous les renderers
        if (mirrorHandRenderers != null)
        {
            foreach (var renderer in mirrorHandRenderers)
            {
                if (renderer != null && !renderer.enabled)
                {
                    renderer.enabled = true;
                }
            }
        }

        if (mirrorHandAllRenderers != null)
        {
            foreach (var renderer in mirrorHandAllRenderers)
            {
                if (renderer != null && !renderer.enabled)
                {
                    renderer.enabled = true;
                }
            }
        }
    }

    private void CacheMirrorHandRenderers()
    {
        var mirrorRoot = GetMirrorHandRoot();
        if (mirrorRoot == null)
            return;

        mirrorHandRenderers = mirrorRoot.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        mirrorHandAllRenderers = mirrorRoot.GetComponentsInChildren<Renderer>(true);

        string handName = sourceHand == SourceHand.Right ? "gauche" : "droite";
        Debug.Log($"[MirrorHand] {mirrorHandRenderers.Length} SkinnedMeshRenderers trouvés sur la main {handName} (miroir)");
    }

    private void DisableMirrorHandTracking()
    {
        var mirrorRoot = GetMirrorHandRoot();
        if (mirrorRoot == null)
            return;

        if (sourceHand == SourceHand.Right)
        {
            leftInteraction.SetActive(false);
            rightInteraction.SetActive(true);
        }
        else
        {
            leftInteraction.SetActive(true);
            rightInteraction.SetActive(false);
        }
        // Recherche et désactivation de TOUS les composants de hand tracking sur la main miroir
        var allComponents = mirrorRoot.GetComponentsInChildren<MonoBehaviour>(true);

        foreach (var component in allComponents)
        {
            if (component == null || component == this)
                continue;

            var componentType = component.GetType().Name;

            // Désactiver les composants OVR (Oculus/Meta)
            if (componentType.Contains("OVRSkeleton") ||
                componentType.Contains("OVRHand") ||
                componentType.Contains("OVRMesh") ||
                componentType.Contains("OVRMeshRenderer") ||
                componentType.Contains("OVRSkeletonRenderer") ||
                // Désactiver aussi les composants XR Hands
                componentType.Contains("XRHandSkeleton") ||
                componentType.Contains("XRHandTrackingEvents") ||
                componentType.Contains("XRHandMeshController") ||
                componentType.Contains("HandTracking"))
            {
                component.enabled = false;
                Debug.Log($"[MirrorHand] Désactivé: {componentType} sur {component.gameObject.name}");
            }
        }

        string handName = sourceHand == SourceHand.Right ? "gauche" : "droite";
        Debug.Log($"[MirrorHand] Désactivation du hand tracking {handName} terminée - cette main sera contrôlée en miroir");
    }

    private void CacheMirrorHandJoints()
    {
        var mirrorRoot = GetMirrorHandRoot();
        if (mirrorRoot == null)
        {
            Debug.LogError("[MirrorHand] mirrorHandRoot n'est pas assigné !");
            return;
        }

        mirrorJointsByName.Clear();

        foreach (Transform t in mirrorRoot.GetComponentsInChildren<Transform>(true))
        {
            if (!mirrorJointsByName.ContainsKey(t.name))
                mirrorJointsByName.Add(t.name, t);
        }

        string handName = sourceHand == SourceHand.Right ? "gauche" : "droite";
        Debug.Log($"[MirrorHand] {mirrorJointsByName.Count} joints mis en cache pour la main {handName} (miroir)");
    }

    private void MirrorHands()
    {
        var sourceRoot = GetSourceHandRoot();
        var mirrorRoot = GetMirrorHandRoot();

        if (sourceRoot == null || mirrorRoot == null)
            return;

        // Position miroir en espace monde
        Vector3 sourceWorldPos = sourceRoot.position;
        Vector3 mirroredWorldPos = ReflectPosition(sourceWorldPos);

        // Empêcher les mains de se traverser
        mirroredWorldPos = ClampToMirrorSide(sourceWorldPos, mirroredWorldPos);
        mirroredWorldPos = PreventHandOverlap(sourceWorldPos, mirroredWorldPos);

        mirrorRoot.position = mirroredWorldPos;

        // Rotation miroir en espace monde
        Quaternion sourceWorldRot = sourceRoot.rotation;
        Quaternion mirroredWorldRot = ReflectRotation(sourceWorldRot);
        mirrorRoot.rotation = mirroredWorldRot;

        // Détermine les patterns de remplacement selon la main source
        string sourcePattern1, mirrorPattern1, sourcePattern2, mirrorPattern2, sourcePattern3, mirrorPattern3;
        if (sourceHand == SourceHand.Right)
        {
            sourcePattern1 = "Right"; mirrorPattern1 = "Left";
            sourcePattern2 = "right"; mirrorPattern2 = "left";
            sourcePattern3 = "_R"; mirrorPattern3 = "_L";
        }
        else
        {
            sourcePattern1 = "Left"; mirrorPattern1 = "Right";
            sourcePattern2 = "left"; mirrorPattern2 = "right";
            sourcePattern3 = "_L"; mirrorPattern3 = "_R";
        }

        foreach (Transform sourceJoint in sourceRoot.GetComponentsInChildren<Transform>())
        {
            if (sourceJoint == sourceRoot)
                continue;

            // Remplacer le pattern source par le pattern miroir dans le nom
            string mirrorJointName = sourceJoint.name
                .Replace(sourcePattern1, mirrorPattern1)
                .Replace(sourcePattern2, mirrorPattern2)
                .Replace(sourcePattern3, mirrorPattern3);

            // Essayer d'abord avec le nom modifié, puis avec le nom original
            if (!mirrorJointsByName.TryGetValue(mirrorJointName, out Transform mirrorJoint))
            {
                if (!mirrorJointsByName.TryGetValue(sourceJoint.name, out mirrorJoint))
                    continue;
            }

            // Pour les joints enfants, on copie la rotation locale avec inversion sur certains axes
            Quaternion r = sourceJoint.localRotation;
            mirrorJoint.localRotation = new Quaternion(r.x, -r.y, -r.z, r.w);

            // Position locale avec inversion X pour les doigts
            Vector3 lp = sourceJoint.localPosition;
            mirrorJoint.localPosition = new Vector3(-lp.x, lp.y, lp.z);
        }
    }
}
