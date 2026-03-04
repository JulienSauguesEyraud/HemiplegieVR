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
    public SourceHand sourceHand = SourceHand.Right;

    [Tooltip("Choisir quel oeil utiliser comme référence pour le plan de miroir")]
    [SerializeField] private EyeReference eyeReference = EyeReference.CenterEye;

    [Header("Avant-bras")]
    [Tooltip("Afficher les avant-bras")]
    public bool showForearms = true;
    
    [Tooltip("Opacité des avant-bras (0 = invisible, 1 = opaque)")]
    [SerializeField][Range(0f, 1f)] private float forearmOpacity = 0.7f;
    
    [Tooltip("Utiliser la transparence pour les avant-bras")]
    [SerializeField] private bool forearmUseTransparency = true;

    private bool lastEnableState;
    private SourceHand lastSourceHand;
    private EyeReference lastEyeReference;
    private bool lastShowForearms;
    private float lastForearmOpacity;
    private bool lastForearmUseTransparency;

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
        lastEyeReference = eyeReference;
        lastShowForearms = showForearms;
        lastForearmOpacity = forearmOpacity;
        lastForearmUseTransparency = forearmUseTransparency;
    }

    void Update()
    {
        // Détecter les changements dans l'Inspector
        if (lastEnableState != enableMirrorTherapy || 
            lastSourceHand != sourceHand || 
            lastEyeReference != eyeReference ||
            lastShowForearms != showForearms ||
            !Mathf.Approximately(lastForearmOpacity, forearmOpacity) ||
            lastForearmUseTransparency != forearmUseTransparency)
        {
            ApplySettings();
            lastEnableState = enableMirrorTherapy;
            lastSourceHand = sourceHand;
            lastEyeReference = eyeReference;
            lastShowForearms = showForearms;
            lastForearmOpacity = forearmOpacity;
            lastForearmUseTransparency = forearmUseTransparency;
        }
    }

    private void ApplySettings()
    {
        if (mirrorHand == null)
            return;

        mirrorHand.SetSourceHand(sourceHand);
        mirrorHand.SetEyeReference(eyeReference);
        mirrorHand.SetEnableMirrorTherapy(enableMirrorTherapy);
        mirrorHand.SetShowForearms(showForearms);
        mirrorHand.SetForearmOpacity(forearmOpacity);
        mirrorHand.SetForearmUseTransparency(forearmUseTransparency);
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
    /// Définit quel oeil est utilisé comme référence pour le plan de miroir
    /// </summary>
    public void SetEyeReference(EyeReference eye)
    {
        eyeReference = eye;
        ApplySettings();
    }

    /// <summary>
    /// Active ou désactive l'affichage des avant-bras
    /// </summary>
    public void SetShowForearms(bool show)
    {
        showForearms = show;
        ApplySettings();
    }

    /// <summary>
    /// Définit l'opacité des avant-bras
    /// </summary>
    public void SetForearmOpacity(float opacity)
    {
        forearmOpacity = Mathf.Clamp01(opacity);
        ApplySettings();
    }

    /// <summary>
    /// Définit si la transparence est utilisée pour les avant-bras
    /// </summary>
    public void SetForearmUseTransparency(bool useTransparency)
    {
        forearmUseTransparency = useTransparency;
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

    /// <summary>
    /// Bascule l'affichage des avant-bras
    /// </summary>
    public void ToggleForearms()
    {
        SetShowForearms(!showForearms);
    }

    // Propriétés publiques pour accéder à l'état actuel
    public bool IsEnabled => enableMirrorTherapy;
    public SourceHand CurrentSourceHand => sourceHand;
    public EyeReference CurrentEyeReference => eyeReference;
    public bool AreForeArmsVisible => showForearms;
    public float ForearmOpacity => forearmOpacity;
    public bool ForearmUseTransparency => forearmUseTransparency;

    public void SetForearmMaterial(Material material)
    {
        if (mirrorHand != null)
        {
            mirrorHand.SetForearmMaterial(material);
        }
    }
}
