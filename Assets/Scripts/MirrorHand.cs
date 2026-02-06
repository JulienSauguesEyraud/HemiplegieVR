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

    private Dictionary<string, Transform> mirrorJointsByName = new();
    private SkinnedMeshRenderer[] mirrorHandRenderers;
    private Renderer[] mirrorHandAllRenderers;
    private SourceHand lastSourceHand;

    // Plan de miroir
    private Vector3 mirrorPlaneNormal;
    private Vector3 mirrorPlanePoint;

    void Start()
    {
        lastSourceHand = sourceHand;
        InitializeMirror();
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

    void LateUpdate()
    {
        // Vérifier si la main source a changé
        if (lastSourceHand != sourceHand)
        {
            lastSourceHand = sourceHand;
            InitializeMirror();
        }

        if (enableMirrorTherapy)
        {
            UpdateMirrorPlane();
            ForceShowMirrorHand();
            MirrorHands();
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
    }

    public void SetSourceHand(SourceHand hand)
    {
        if (sourceHand != hand)
        {
            sourceHand = hand;
            InitializeMirror();
        }
    }

    public void SetEyeReference(EyeReference eye)
    {
        eyeReference = eye;
    }

    public bool IsEnabled => enableMirrorTherapy;
    public SourceHand CurrentSourceHand => sourceHand;
    public EyeReference CurrentEyeReference => eyeReference;

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
