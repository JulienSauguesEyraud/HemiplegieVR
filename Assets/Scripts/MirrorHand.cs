using UnityEngine;
using System.Collections.Generic;

public enum SourceHand
{
    Right,
    Left
}

public class MirrorHand : MonoBehaviour
{
    [Header("Roots")]
    [SerializeField] private Transform rightHandRoot;
    [SerializeField] private Transform leftHandRoot;
    [SerializeField] private GameObject rightInteraction;
    [SerializeField] private GameObject leftInteraction;

    [Header("Mirror Therapy Settings")]
    [SerializeField] private bool enableMirrorTherapy = false;
    [SerializeField] private SourceHand sourceHand = SourceHand.Right;
    [Tooltip("Désactive automatiquement les composants de hand tracking sur la main miroir")]
    [SerializeField] private bool disableMirrorHandTracking = true;

    private Dictionary<string, Transform> mirrorJointsByName = new();
    private SkinnedMeshRenderer[] mirrorHandRenderers;
    private Renderer[] mirrorHandAllRenderers;
    private SourceHand lastSourceHand;

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
            ForceShowMirrorHand();
            MirrorHands();
        }
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

    public bool IsEnabled => enableMirrorTherapy;
    public SourceHand CurrentSourceHand => sourceHand;

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
            
        // Miroir de la position root
        Vector3 sourcePos = sourceRoot.localPosition;
        mirrorRoot.localPosition = new Vector3(-sourcePos.x, sourcePos.y, sourcePos.z);
        
        // Miroir de la rotation root
        Quaternion sourceRot = sourceRoot.localRotation;
        mirrorRoot.localRotation = new Quaternion(sourceRot.x, -sourceRot.y, -sourceRot.z, sourceRot.w);

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

            // --- Position miroir (axe X local) ---
            Vector3 lp = sourceJoint.localPosition;
            mirrorJoint.localPosition = new Vector3(-lp.x, lp.y, lp.z);

            // --- Rotation miroir ---
            Quaternion r = sourceJoint.localRotation;
            mirrorJoint.localRotation = new Quaternion(r.x, -r.y, -r.z, r.w);
        }
    }
}
