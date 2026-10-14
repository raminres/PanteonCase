using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Root-level configuration and UI binding point for the stylized jungle scene.
/// This pass owns the statue-gold shine; future water, foliage, VFX, and Animator
/// sections can be added here without introducing separate controller objects.
/// </summary>
[DisallowMultipleComponent]
public sealed class StylizedSceneController : MonoBehaviour
{
    private static readonly int ShineColorId = Shader.PropertyToID("_ShineColor");
    private static readonly int ShineAngleId = Shader.PropertyToID("_ShineAngle");
    private static readonly int ShineSpeedId = Shader.PropertyToID("_ShineSpeed");
    private static readonly int ShineWidthId = Shader.PropertyToID("_ShineWidth");
    private static readonly int ShineSoftnessId = Shader.PropertyToID("_ShineSoftness");

    [Header("Statue Gold Shine")]
    [Tooltip("Shared material used by the CatStatueShine UI Image. The controller updates this existing material and never creates runtime material instances.")]
    [SerializeField] private Material statueGoldShineMaterial;

    [Tooltip("Highlight tint. Its alpha is the highlight intensity.")]
    [SerializeField] private Color statueGoldShineColor = new(1f, 0.82f, 0.35f, 0.65f);

    [Header("Statue Gold Shine UI Bindings")]
    [Tooltip("On-screen slider with a range of -2 to 2.")]
    [SerializeField] private Slider statueGoldShineSpeedSlider;

    [Tooltip("On-screen slider with a range of 0.01 to 0.5.")]
    [SerializeField] private Slider statueGoldShineWidthSlider;

    [Tooltip("On-screen slider with a range of 0 to 360.")]
    [SerializeField] private Slider statueGoldShineAngleSlider;

    [Tooltip("On-screen slider with a range of 0.001 to 0.25.")]
    [SerializeField] private Slider statueGoldShineSoftnessSlider;

    // These values are intentionally not serialized. The material is their authored fallback,
    // while the on-screen controls are the player-facing source of adjustment.
    private bool statueGoldShineEnabled = true;
    private float statueGoldShineAngle;
    private float statueGoldShineSpeed;
    private float statueGoldShineWidth;
    private float statueGoldShineSoftness;

    private void OnEnable()
    {
        ReadStatueGoldShineValuesFromMaterial();
        ApplyStatueGoldShine();
        SynchronizeStatueGoldShineControls();
        BindStatueGoldShineControls();
    }

    private void OnDisable()
    {
        UnbindStatueGoldShineControls();
    }

    private void OnValidate()
    {
        statueGoldShineColor.a = Mathf.Clamp01(statueGoldShineColor.a);
        ApplyStatueGoldShineColor();
    }

    /// <summary>Optional Toggle callback that hides only the additive highlight.</summary>
    public void SetStatueGoldShineEnabled(bool isEnabled)
    {
        statueGoldShineEnabled = isEnabled;
        ApplyStatueGoldShineColor();
    }

    public void SetStatueGoldShineAngle(float degrees)
    {
        statueGoldShineAngle = Mathf.Clamp(degrees, 0f, 360f);
        ApplyStatueGoldShine();
    }

    public void SetStatueGoldShineSpeed(float speed)
    {
        statueGoldShineSpeed = Mathf.Clamp(speed, -2f, 2f);
        ApplyStatueGoldShine();
    }

    public void SetStatueGoldShineWidth(float width)
    {
        statueGoldShineWidth = Mathf.Clamp(width, 0.01f, 0.5f);
        ApplyStatueGoldShine();
    }

    public void SetStatueGoldShineSoftness(float softness)
    {
        statueGoldShineSoftness = Mathf.Clamp(softness, 0.001f, 0.25f);
        ApplyStatueGoldShine();
    }

    /// <summary>Sets the highlight intensity without changing its RGB tint.</summary>
    public void SetStatueGoldShineIntensity(float intensity)
    {
        statueGoldShineColor.a = Mathf.Clamp01(intensity);
        ApplyStatueGoldShineColor();
    }

    /// <summary>For a colour-picker callback or future art-direction code.</summary>
    public void SetStatueGoldShineColor(Color color)
    {
        statueGoldShineColor = color;
        statueGoldShineColor.a = Mathf.Clamp01(statueGoldShineColor.a);
        ApplyStatueGoldShineColor();
    }

    private void ReadStatueGoldShineValuesFromMaterial()
    {
        if (statueGoldShineMaterial == null)
        {
            return;
        }

        statueGoldShineAngle = Mathf.Clamp(statueGoldShineMaterial.GetFloat(ShineAngleId), 0f, 360f);
        statueGoldShineSpeed = Mathf.Clamp(statueGoldShineMaterial.GetFloat(ShineSpeedId), -2f, 2f);
        statueGoldShineWidth = Mathf.Clamp(statueGoldShineMaterial.GetFloat(ShineWidthId), 0.01f, 0.5f);
        statueGoldShineSoftness = Mathf.Clamp(statueGoldShineMaterial.GetFloat(ShineSoftnessId), 0.001f, 0.25f);
    }

    private void BindStatueGoldShineControls()
    {
        // Remove first so an enable/disable cycle cannot register duplicate listeners.
        UnbindStatueGoldShineControls();

        if (statueGoldShineSpeedSlider != null)
        {
            statueGoldShineSpeedSlider.onValueChanged.AddListener(SetStatueGoldShineSpeed);
        }

        if (statueGoldShineWidthSlider != null)
        {
            statueGoldShineWidthSlider.onValueChanged.AddListener(SetStatueGoldShineWidth);
        }

        if (statueGoldShineAngleSlider != null)
        {
            statueGoldShineAngleSlider.onValueChanged.AddListener(SetStatueGoldShineAngle);
        }

        if (statueGoldShineSoftnessSlider != null)
        {
            statueGoldShineSoftnessSlider.onValueChanged.AddListener(SetStatueGoldShineSoftness);
        }
    }

    private void UnbindStatueGoldShineControls()
    {
        if (statueGoldShineSpeedSlider != null)
        {
            statueGoldShineSpeedSlider.onValueChanged.RemoveListener(SetStatueGoldShineSpeed);
        }

        if (statueGoldShineWidthSlider != null)
        {
            statueGoldShineWidthSlider.onValueChanged.RemoveListener(SetStatueGoldShineWidth);
        }

        if (statueGoldShineAngleSlider != null)
        {
            statueGoldShineAngleSlider.onValueChanged.RemoveListener(SetStatueGoldShineAngle);
        }

        if (statueGoldShineSoftnessSlider != null)
        {
            statueGoldShineSoftnessSlider.onValueChanged.RemoveListener(SetStatueGoldShineSoftness);
        }
    }

    private void SynchronizeStatueGoldShineControls()
    {
        // This keeps the UI in sync without invoking callbacks during setup.
        if (statueGoldShineSpeedSlider != null)
        {
            statueGoldShineSpeedSlider.SetValueWithoutNotify(statueGoldShineSpeed);
        }

        if (statueGoldShineWidthSlider != null)
        {
            statueGoldShineWidthSlider.SetValueWithoutNotify(statueGoldShineWidth);
        }

        if (statueGoldShineAngleSlider != null)
        {
            statueGoldShineAngleSlider.SetValueWithoutNotify(statueGoldShineAngle);
        }

        if (statueGoldShineSoftnessSlider != null)
        {
            statueGoldShineSoftnessSlider.SetValueWithoutNotify(statueGoldShineSoftness);
        }
    }

    private void ApplyStatueGoldShine()
    {
        if (statueGoldShineMaterial == null)
        {
            return;
        }

        ApplyStatueGoldShineColor();
        statueGoldShineMaterial.SetFloat(ShineAngleId, statueGoldShineAngle);
        statueGoldShineMaterial.SetFloat(ShineSpeedId, statueGoldShineSpeed);
        statueGoldShineMaterial.SetFloat(ShineWidthId, statueGoldShineWidth);
        statueGoldShineMaterial.SetFloat(ShineSoftnessId, statueGoldShineSoftness);
    }

    private void ApplyStatueGoldShineColor()
    {
        if (statueGoldShineMaterial == null)
        {
            return;
        }

        // A zero alpha removes only the additive shine; it does not disable the UI Image.
        Color appliedColor = statueGoldShineColor;
        appliedColor.a = statueGoldShineEnabled ? appliedColor.a : 0f;
        statueGoldShineMaterial.SetColor(ShineColorId, appliedColor);
    }
}
