using UnityEngine;
using System.Collections.Generic;

public class MirrorHand : MonoBehaviour
{
    [Header("Roots")]
    [SerializeField] private Transform rightHandRoot;
    [SerializeField] private Transform leftHandRoot;

    [Header("Mirror Therapy Settings")]
    [SerializeField] private bool enableMirrorTherapy = true;
    [Tooltip("Désactive automatiquement les composants de hand tracking sur la main gauche")]
    [SerializeField] private bool disableLeftHandTracking = true;

    private Dictionary<string, Transform> leftJointsByName = new();
    private SkinnedMeshRenderer[] leftHandRenderers;
    private Renderer[] leftHandAllRenderers;

    void Start()
    {
        if (disableLeftHandTracking)
        {
            DisableLeftHandTracking();
        }
        
        CacheLeftHandJoints();
        CacheLeftHandRenderers();
        ForceShowLeftHand();
    }

    void LateUpdate()
    {
        if (enableMirrorTherapy)
        {
            // Forcer l'affichage de la main gauche à chaque frame
            ForceShowLeftHand();
            MirrorRightToLeft();
        }
    }

    private void ForceShowLeftHand()
    {
        if (leftHandRoot == null)
            return;

        // S'assurer que le GameObject est actif
        if (!leftHandRoot.gameObject.activeInHierarchy)
        {
            leftHandRoot.gameObject.SetActive(true);
        }

        // Activer tous les renderers
        if (leftHandRenderers != null)
        {
            foreach (var renderer in leftHandRenderers)
            {
                if (renderer != null && !renderer.enabled)
                {
                    renderer.enabled = true;
                }
            }
        }

        if (leftHandAllRenderers != null)
        {
            foreach (var renderer in leftHandAllRenderers)
            {
                if (renderer != null && !renderer.enabled)
                {
                    renderer.enabled = true;
                }
            }
        }
    }

    private void CacheLeftHandRenderers()
    {
        if (leftHandRoot == null)
            return;

        leftHandRenderers = leftHandRoot.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        leftHandAllRenderers = leftHandRoot.GetComponentsInChildren<Renderer>(true);
        
        Debug.Log($"[MirrorHand] {leftHandRenderers.Length} SkinnedMeshRenderers trouvés sur la main gauche");
    }

    private void DisableLeftHandTracking()
    {
        if (leftHandRoot == null)
            return;

        // Recherche et désactivation de TOUS les composants de hand tracking sur la main gauche
        var allComponents = leftHandRoot.GetComponentsInChildren<MonoBehaviour>(true);
        
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
        
        Debug.Log("[MirrorHand] Désactivation du hand tracking gauche terminée - la main gauche sera contrôlée en miroir de la droite");
    }

    private void CacheLeftHandJoints()
    {
        if (leftHandRoot == null)
        {
            Debug.LogError("[MirrorHand] leftHandRoot n'est pas assigné !");
            return;
        }
        
        leftJointsByName.Clear();

        foreach (Transform t in leftHandRoot.GetComponentsInChildren<Transform>(true))
        {
            if (!leftJointsByName.ContainsKey(t.name))
                leftJointsByName.Add(t.name, t);
        }
        
        Debug.Log($"[MirrorHand] {leftJointsByName.Count} joints mis en cache pour la main gauche");
    }

    private void MirrorRightToLeft()
    {
        if (rightHandRoot == null || leftHandRoot == null)
            return;
            
        // Miroir de la position root
        Vector3 rightPos = rightHandRoot.localPosition;
        leftHandRoot.localPosition = new Vector3(-rightPos.x, rightPos.y, rightPos.z);
        
        // Miroir de la rotation root
        Quaternion rightRot = rightHandRoot.localRotation;
        leftHandRoot.localRotation = new Quaternion(rightRot.x, -rightRot.y, -rightRot.z, rightRot.w);

        foreach (Transform rightJoint in rightHandRoot.GetComponentsInChildren<Transform>())
        {
            if (rightJoint == rightHandRoot)
                continue;
                
            // Remplacer "Right" par "Left" dans le nom pour trouver le joint correspondant
            string leftJointName = rightJoint.name.Replace("Right", "Left").Replace("right", "left").Replace("_R", "_L");
            
            // Essayer d'abord avec le nom modifié, puis avec le nom original
            if (!leftJointsByName.TryGetValue(leftJointName, out Transform leftJoint))
            {
                if (!leftJointsByName.TryGetValue(rightJoint.name, out leftJoint))
                    continue;
            }

            // --- Position miroir (axe X local) ---
            Vector3 lp = rightJoint.localPosition;
            leftJoint.localPosition = new Vector3(-lp.x, lp.y, lp.z);

            // --- Rotation miroir ---
            Quaternion r = rightJoint.localRotation;
            leftJoint.localRotation = new Quaternion(r.x, -r.y, -r.z, r.w);
        }
    }
}
