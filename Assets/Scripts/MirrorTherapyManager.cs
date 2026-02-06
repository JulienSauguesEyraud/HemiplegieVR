using UnityEngine;

/// <summary>
/// Manager pour contrôler la thérapie miroir.
/// Permet de choisir quelle main est la source et d'activer/désactiver la thérapie.
/// </summary>
public class MirrorTherapyManager : MonoBehaviour
{
    [Header("Références")]
    [SerializeField] private MirrorHand mirrorHand;

    [Header("Configuration")]
    [Tooltip("Active ou désactive la thérapie miroir")]
    [SerializeField] private bool enableMirrorTherapy = true;
    
    [Tooltip("Choisir quelle main est la source (l'autre sera le miroir)")]
    [SerializeField] private SourceHand sourceHand = SourceHand.Right;

    private bool lastEnableState;
    private SourceHand lastSourceHand;

    void Start()
    {
        if (mirrorHand == null)
        {
            mirrorHand = FindFirstObjectByType<MirrorHand>();
            if (mirrorHand == null)
            {
                Debug.LogError("[MirrorTherapyManager] Aucun MirrorHand trouvé dans la scène !");
                return;
            }
        }

        ApplySettings();
        lastEnableState = enableMirrorTherapy;
        lastSourceHand = sourceHand;
    }

    void Update()
    {
        // Détecter les changements dans l'Inspector
        if (lastEnableState != enableMirrorTherapy || lastSourceHand != sourceHand)
        {
            ApplySettings();
            lastEnableState = enableMirrorTherapy;
            lastSourceHand = sourceHand;
        }
    }

    private void ApplySettings()
    {
        if (mirrorHand == null)
            return;

        mirrorHand.SetSourceHand(sourceHand);
        mirrorHand.SetEnableMirrorTherapy(enableMirrorTherapy);
    }

    /// <summary>
    /// Active ou désactive la thérapie miroir
    /// </summary>
    public void SetMirrorTherapyEnabled(bool enabled)
    {
        enableMirrorTherapy = enabled;
        ApplySettings();
    }

    /// <summary>
    /// Définit quelle main est utilisée comme source
    /// </summary>
    public void SetSourceHand(SourceHand hand)
    {
        sourceHand = hand;
        ApplySettings();
    }

    /// <summary>
    /// Utilise la main droite comme source (la gauche sera le miroir)
    /// </summary>
    public void UseRightHandAsSource()
    {
        SetSourceHand(SourceHand.Right);
    }

    /// <summary>
    /// Utilise la main gauche comme source (la droite sera le miroir)
    /// </summary>
    public void UseLeftHandAsSource()
    {
        SetSourceHand(SourceHand.Left);
    }

    /// <summary>
    /// Bascule entre main gauche et main droite comme source
    /// </summary>
    public void ToggleSourceHand()
    {
        SetSourceHand(sourceHand == SourceHand.Right ? SourceHand.Left : SourceHand.Right);
    }

    /// <summary>
    /// Bascule l'activation de la thérapie miroir
    /// </summary>
    public void ToggleMirrorTherapy()
    {
        SetMirrorTherapyEnabled(!enableMirrorTherapy);
    }

    // Propriétés publiques pour accéder à l'état actuel
    public bool IsEnabled => enableMirrorTherapy;
    public SourceHand CurrentSourceHand => sourceHand;
}
