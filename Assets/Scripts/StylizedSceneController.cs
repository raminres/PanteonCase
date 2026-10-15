using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

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
    private static readonly int WaterfallWaveSpeedId = Shader.PropertyToID("_WaveSpeed");
    private static readonly int WaterfallNoiseStrengthId = Shader.PropertyToID("_NoiseStrength");
    private static readonly int WaterfallOpacityId = Shader.PropertyToID("_Opacity");
    private static readonly int WaterfallFoamAmountId = Shader.PropertyToID("_FoamAmount");
    private static readonly int LeafWindStrengthId = Shader.PropertyToID("_WindStrength");
    private static readonly int LeafWindSpeedId = Shader.PropertyToID("_WindSpeed");
    private static readonly int LeafAnchorFalloffId = Shader.PropertyToID("_AnchorFalloff");
    private static readonly int CloudSpeedId = Animator.StringToHash("Speed");
    private const float MaxRainEmissionRate = 30f;
    private const float RainCloudMinimumValue = 0.7f;
    private const float MinWaterfallImageWidth = 64f;
    private const float MaxWaterfallImageWidth = 512f;

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

    [Header("Cloud Movement UI Bindings")]
    [Tooltip("Animator on the Clouds parent. Its controller must expose a Float parameter named 'Speed'.")]
    [SerializeField] private Animator cloudAnimator;

    [Tooltip("On-screen slider that controls the Clouds Animator's Speed parameter. The current UI range is -1 to 1, where 0 pauses the motion.")]
    [SerializeField] private Slider cloudSpeedSlider;

    [Header("Rain UI Bindings")]
    [Tooltip("Particle System whose rate-over-time emission is controlled by the Rain Amount slider.")]
    [SerializeField] private ParticleSystem rainParticleSystem;

    [Tooltip("On-screen slider with a range of 0 to 30, matching the rain emission rate per second.")]
    [SerializeField] private Slider rainAmountSlider;

    [Tooltip("The single cloud layer darkened as rainfall increases. Its hue, saturation, and alpha are preserved.")]
    [SerializeField] private Image rainCloudImage;

    [Header("Waterfall UI Bindings")]
    [Tooltip("RectTransform of the Waterfall UI Image. Only its width is changed by the associated slider.")]
    [SerializeField] private RectTransform waterfallImageRectTransform;

    [Tooltip("Shared material using Panteon/UI/Stylized Waterfall. The controller updates this material directly and never creates runtime instances.")]
    [SerializeField] private Material waterfallMaterial;

    [Tooltip("Optional shared width slider. A 0 to 1 range is recommended; legacy pixel ranges are normalized automatically.")]
    [SerializeField] private Slider waterfallImageWidthSlider;

    [Header("Waterfall Linked Width Targets")]
    [Tooltip("The sliced CatStatue Image that scales horizontally with the normalized waterfall-width control.")]
    [SerializeField] private RectTransform waterfallCatStatueRectTransform;

    [Tooltip("CatStatue width in pixels when the waterfall-width control is at 1.")]
    [SerializeField, Min(0f)] private float waterfallCatStatueMaximumWidth = 333f;

    [Tooltip("Box-shaped foam Particle System that scales horizontally with the normalized waterfall-width control.")]
    [SerializeField] private ParticleSystem waterfallFoamParticleSystem;

    [Tooltip("Foam Particle System Shape-module X scale when the waterfall-width control is at 1.")]
    [SerializeField, Min(0f)] private float waterfallFoamParticleMaximumBoxWidth = 8f;

    [Header("Waterfall Material UI Bindings")]

    [Tooltip("Optional slider with a range of -2 to 2, matching the waterfall shader's Wave Speed property.")]
    [SerializeField] private Slider waterfallWaveSpeedSlider;

    [Tooltip("Optional slider with a range of 0 to 1, matching the waterfall shader's Pulse Noise Strength property.")]
    [SerializeField] private Slider waterfallPulseNoiseStrengthSlider;

    [Tooltip("Optional slider with a range of 0 to 1, matching the waterfall shader's Opacity property.")]
    [SerializeField] private Slider waterfallOpacitySlider;

    [Tooltip("Optional slider with a range of 0 to 1, matching the waterfall shader's Foam Amount property.")]
    [SerializeField] private Slider waterfallFoamAmountSlider;

    [Header("Leaf Wind UI Bindings")]
    [Tooltip("Shared MT_SwayingLeaf material. The controller updates it directly and never creates runtime material instances.")]
    [SerializeField] private Material swayingLeafMaterial;

    [Tooltip("Slider controlling the leaf shader's Body Bend Strength property. Its configured UI range is respected.")]
    [SerializeField] private Slider leafWindBendStrengthSlider;

    [Tooltip("Slider controlling the leaf shader's Wind Speed property. Its configured UI range is respected.")]
    [SerializeField] private Slider leafWindSpeedSlider;

    [Tooltip("Slider controlling the leaf shader's Base Anchor Falloff property. Its configured UI range is respected.")]
    [SerializeField] private Slider leafWindAnchorFalloffSlider;

    [Header("Authoring Save")]
    [Tooltip("Saves the current shine-material values to disk when clicked in Play mode. This action is editor-only and is omitted from player builds.")]
    [SerializeField] private Button saveSettingsButton;

    // These values are intentionally not serialized. The material is their authored fallback,
    // while the on-screen controls are the player-facing source of adjustment.
    private bool statueGoldShineEnabled = true;
    private float statueGoldShineAngle;
    private float statueGoldShineSpeed;
    private float statueGoldShineWidth;
    private float statueGoldShineSoftness;
    private float cloudAnimationSpeed;
    private Color rainCloudBaseColor;
    private bool rainCloudBaseColorCaptured;
    private float waterfallImageWidth;
    private float waterfallWidthNormalized;
    private float waterfallWaveSpeed;
    private float waterfallPulseNoiseStrength;
    private float waterfallOpacity;
    private float waterfallFoamAmount;
    private float leafWindBendStrength;
    private float leafWindSpeed;
    private float leafWindAnchorFalloff;

    private void OnEnable()
    {
        ReadStatueGoldShineValuesFromMaterial();
        ApplyStatueGoldShine();
        SynchronizeStatueGoldShineControls();
        BindStatueGoldShineControls();
        InitializeCloudMovement();
        BindCloudMovementControls();
        InitializeRain();
        BindRainControls();
        InitializeWaterfall();
        BindWaterfallControls();
        InitializeLeafWind();
        BindLeafWindControls();
    }

    private void OnDisable()
    {
        UnbindStatueGoldShineControls();
        UnbindCloudMovementControls();
        UnbindRainControls();
        UnbindWaterfallControls();
        UnbindLeafWindControls();
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

    /// <summary>
    /// Updates the authored Animator float parameter from the on-screen cloud-speed slider.
    /// Negative values intentionally support reversed cloud motion; zero pauses it.
    /// </summary>
    public void SetCloudAnimationSpeed(float speed)
    {
        cloudAnimationSpeed = Mathf.Clamp(speed, -1f, 1f);

        if (cloudAnimator != null)
        {
            cloudAnimator.SetFloat(CloudSpeedId, cloudAnimationSpeed);
        }
    }

    /// <summary>
    /// Maps the UI's 0-30 amount directly to Particle System rate-over-time emission.
    /// The same normalized amount lowers only Cloud1's HSV Value from 100% to 70%.
    /// </summary>
    public void SetRainAmount(float rainAmount)
    {
        if (!Application.isPlaying)
        {
            return;
        }

        float clampedAmount = Mathf.Clamp(rainAmount, 0f, MaxRainEmissionRate);

        if (rainParticleSystem != null)
        {
            ParticleSystem.EmissionModule emission = rainParticleSystem.emission;
            emission.rateOverTime = clampedAmount;
        }

        if (rainCloudImage == null)
        {
            return;
        }

        if (!rainCloudBaseColorCaptured)
        {
            rainCloudBaseColor = rainCloudImage.color;
            rainCloudBaseColorCaptured = true;
        }

        Color.RGBToHSV(rainCloudBaseColor, out float hue, out float saturation, out float baseValue);
        float rainFraction = clampedAmount / MaxRainEmissionRate;
        float value = Mathf.Lerp(baseValue, baseValue * RainCloudMinimumValue, rainFraction);
        Color rainTint = Color.HSVToRGB(hue, saturation, value);
        rainTint.a = rainCloudBaseColor.a;
        rainCloudImage.color = rainTint;
    }

    /// <summary>
    /// Maps the configured waterfall-width slider to 0..1, then updates every linked width.
    /// The slider may use either a 0..1 authoring range or its older pixel-based range.
    /// </summary>
    public void SetWaterfallImageWidth(float sliderValue)
    {
        float normalizedWidth = waterfallImageWidthSlider != null
            ? Mathf.InverseLerp(waterfallImageWidthSlider.minValue, waterfallImageWidthSlider.maxValue, sliderValue)
            : Mathf.InverseLerp(MinWaterfallImageWidth, MaxWaterfallImageWidth, sliderValue);

        SetWaterfallWidthNormalized(normalizedWidth);
    }

    /// <summary>
    /// Direct 0..1 callback for a normalized waterfall-width slider. At 1, the waterfall,
    /// CatStatue, and foam box use their configured maximum widths.
    /// </summary>
    public void SetWaterfallWidthNormalized(float normalizedWidth)
    {
        waterfallWidthNormalized = Mathf.Clamp01(normalizedWidth);
        waterfallImageWidth = Mathf.Lerp(MinWaterfallImageWidth, MaxWaterfallImageWidth, waterfallWidthNormalized);

        if (waterfallImageRectTransform != null)
        {
            waterfallImageRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, waterfallImageWidth);
        }

        if (waterfallCatStatueRectTransform != null)
        {
            float statueWidth = waterfallCatStatueMaximumWidth * waterfallWidthNormalized;
            waterfallCatStatueRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, statueWidth);
        }

        if (waterfallFoamParticleSystem != null)
        {
            ParticleSystem.ShapeModule shape = waterfallFoamParticleSystem.shape;
            Vector3 shapeScale = shape.scale;
            shapeScale.x = waterfallFoamParticleMaximumBoxWidth * waterfallWidthNormalized;
            shape.scale = shapeScale;
        }
    }

    /// <summary>Sets the signed flow speed for the waterfall mask's red wave channel.</summary>
    public void SetWaterfallWaveSpeed(float speed)
    {
        waterfallWaveSpeed = Mathf.Clamp(speed, -2f, 2f);
        ApplyWaterfallMaterialProperties();
    }

    /// <summary>Controls how strongly the blue mask channel pulses the wave and foam layers.</summary>
    public void SetWaterfallPulseNoiseStrength(float strength)
    {
        waterfallPulseNoiseStrength = Mathf.Clamp01(strength);
        ApplyWaterfallMaterialProperties();
    }

    /// <summary>Sets the shared alpha multiplier of the waterfall shader.</summary>
    public void SetWaterfallOpacity(float opacity)
    {
        waterfallOpacity = Mathf.Clamp01(opacity);
        ApplyWaterfallMaterialProperties();
    }

    /// <summary>Controls the green mask channel's visible foam contribution.</summary>
    public void SetWaterfallFoamAmount(float amount)
    {
        waterfallFoamAmount = Mathf.Clamp01(amount);
        ApplyWaterfallMaterialProperties();
    }

    /// <summary>Sets the shared leaf material's vertex-bend amount.</summary>
    public void SetLeafWindBendStrength(float strength)
    {
        leafWindBendStrength = strength;
        ApplyLeafWindMaterialProperties();
    }

    /// <summary>Sets the shared leaf material's wind animation speed.</summary>
    public void SetLeafWindSpeed(float speed)
    {
        leafWindSpeed = speed;
        ApplyLeafWindMaterialProperties();
    }

    /// <summary>Sets how strongly each leaf's pivot stays anchored.</summary>
    public void SetLeafWindAnchorFalloff(float falloff)
    {
        leafWindAnchorFalloff = falloff;
        ApplyLeafWindMaterialProperties();
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

        if (saveSettingsButton != null)
        {
            saveSettingsButton.onClick.AddListener(SaveStatueGoldShineMaterial);
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

        if (saveSettingsButton != null)
        {
            saveSettingsButton.onClick.RemoveListener(SaveStatueGoldShineMaterial);
        }
    }

    private void InitializeCloudMovement()
    {
        // The slider is the initial authoring source, so its saved UI value is respected
        // when entering Play mode instead of being overwritten by the Animator default.
        if (cloudSpeedSlider != null)
        {
            cloudAnimationSpeed = Mathf.Clamp(cloudSpeedSlider.value, -1f, 1f);
        }

        SetCloudAnimationSpeed(cloudAnimationSpeed);
    }

    private void BindCloudMovementControls()
    {
        UnbindCloudMovementControls();

        if (cloudSpeedSlider != null)
        {
            cloudSpeedSlider.onValueChanged.AddListener(SetCloudAnimationSpeed);
        }
    }

    private void UnbindCloudMovementControls()
    {
        if (cloudSpeedSlider != null)
        {
            cloudSpeedSlider.onValueChanged.RemoveListener(SetCloudAnimationSpeed);
        }
    }

    private void InitializeRain()
    {
        // Rain effects are previewed only in Play mode so authoring values on the UI Image
        // and Particle System are not modified while editing the scene.
        if (!Application.isPlaying)
        {
            return;
        }

        if (rainCloudImage != null)
        {
            rainCloudBaseColor = rainCloudImage.color;
            rainCloudBaseColorCaptured = true;
        }

        if (rainAmountSlider != null)
        {
            SetRainAmount(rainAmountSlider.value);
        }
    }

    private void BindRainControls()
    {
        UnbindRainControls();

        if (rainAmountSlider != null)
        {
            rainAmountSlider.onValueChanged.AddListener(SetRainAmount);
        }
    }

    private void UnbindRainControls()
    {
        if (rainAmountSlider != null)
        {
            rainAmountSlider.onValueChanged.RemoveListener(SetRainAmount);
        }
    }

    private void InitializeWaterfall()
    {
        if (waterfallImageRectTransform != null)
        {
            waterfallImageWidth = Mathf.Clamp(waterfallImageRectTransform.rect.width,
                                              MinWaterfallImageWidth,
                                              MaxWaterfallImageWidth);
        }

        waterfallWidthNormalized = waterfallImageWidthSlider != null
            ? Mathf.InverseLerp(waterfallImageWidthSlider.minValue,
                                waterfallImageWidthSlider.maxValue,
                                waterfallImageWidthSlider.value)
            : Mathf.InverseLerp(MinWaterfallImageWidth, MaxWaterfallImageWidth, waterfallImageWidth);

        ReadWaterfallValuesFromMaterial();
        SetWaterfallWidthNormalized(waterfallWidthNormalized);
        ApplyWaterfallMaterialProperties();
        SynchronizeWaterfallControls();
    }

    private void BindWaterfallControls()
    {
        UnbindWaterfallControls();

        if (waterfallImageWidthSlider != null)
        {
            waterfallImageWidthSlider.onValueChanged.AddListener(SetWaterfallImageWidth);
        }

        if (waterfallWaveSpeedSlider != null)
        {
            waterfallWaveSpeedSlider.onValueChanged.AddListener(SetWaterfallWaveSpeed);
        }

        if (waterfallPulseNoiseStrengthSlider != null)
        {
            waterfallPulseNoiseStrengthSlider.onValueChanged.AddListener(SetWaterfallPulseNoiseStrength);
        }

        if (waterfallOpacitySlider != null)
        {
            waterfallOpacitySlider.onValueChanged.AddListener(SetWaterfallOpacity);
        }

        if (waterfallFoamAmountSlider != null)
        {
            waterfallFoamAmountSlider.onValueChanged.AddListener(SetWaterfallFoamAmount);
        }
    }

    private void UnbindWaterfallControls()
    {
        if (waterfallImageWidthSlider != null)
        {
            waterfallImageWidthSlider.onValueChanged.RemoveListener(SetWaterfallImageWidth);
        }

        if (waterfallWaveSpeedSlider != null)
        {
            waterfallWaveSpeedSlider.onValueChanged.RemoveListener(SetWaterfallWaveSpeed);
        }

        if (waterfallPulseNoiseStrengthSlider != null)
        {
            waterfallPulseNoiseStrengthSlider.onValueChanged.RemoveListener(SetWaterfallPulseNoiseStrength);
        }

        if (waterfallOpacitySlider != null)
        {
            waterfallOpacitySlider.onValueChanged.RemoveListener(SetWaterfallOpacity);
        }

        if (waterfallFoamAmountSlider != null)
        {
            waterfallFoamAmountSlider.onValueChanged.RemoveListener(SetWaterfallFoamAmount);
        }
    }

    private void InitializeLeafWind()
    {
        ReadLeafWindValuesFromMaterial();
        ApplyLeafWindMaterialProperties();
        SynchronizeLeafWindControls();
    }

    private void BindLeafWindControls()
    {
        UnbindLeafWindControls();

        if (leafWindBendStrengthSlider != null)
        {
            leafWindBendStrengthSlider.onValueChanged.AddListener(SetLeafWindBendStrength);
        }

        if (leafWindSpeedSlider != null)
        {
            leafWindSpeedSlider.onValueChanged.AddListener(SetLeafWindSpeed);
        }

        if (leafWindAnchorFalloffSlider != null)
        {
            leafWindAnchorFalloffSlider.onValueChanged.AddListener(SetLeafWindAnchorFalloff);
        }
    }

    private void UnbindLeafWindControls()
    {
        if (leafWindBendStrengthSlider != null)
        {
            leafWindBendStrengthSlider.onValueChanged.RemoveListener(SetLeafWindBendStrength);
        }

        if (leafWindSpeedSlider != null)
        {
            leafWindSpeedSlider.onValueChanged.RemoveListener(SetLeafWindSpeed);
        }

        if (leafWindAnchorFalloffSlider != null)
        {
            leafWindAnchorFalloffSlider.onValueChanged.RemoveListener(SetLeafWindAnchorFalloff);
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

    private void SynchronizeWaterfallControls()
    {
        if (waterfallImageWidthSlider != null)
        {
            float sliderValue = Mathf.Lerp(waterfallImageWidthSlider.minValue,
                                           waterfallImageWidthSlider.maxValue,
                                           waterfallWidthNormalized);
            waterfallImageWidthSlider.SetValueWithoutNotify(sliderValue);
        }

        if (waterfallWaveSpeedSlider != null)
        {
            waterfallWaveSpeedSlider.SetValueWithoutNotify(waterfallWaveSpeed);
        }

        if (waterfallPulseNoiseStrengthSlider != null)
        {
            waterfallPulseNoiseStrengthSlider.SetValueWithoutNotify(waterfallPulseNoiseStrength);
        }

        if (waterfallOpacitySlider != null)
        {
            waterfallOpacitySlider.SetValueWithoutNotify(waterfallOpacity);
        }

        if (waterfallFoamAmountSlider != null)
        {
            waterfallFoamAmountSlider.SetValueWithoutNotify(waterfallFoamAmount);
        }
    }

    private void SynchronizeLeafWindControls()
    {
        if (leafWindBendStrengthSlider != null)
        {
            leafWindBendStrengthSlider.SetValueWithoutNotify(leafWindBendStrength);
        }

        if (leafWindSpeedSlider != null)
        {
            leafWindSpeedSlider.SetValueWithoutNotify(leafWindSpeed);
        }

        if (leafWindAnchorFalloffSlider != null)
        {
            leafWindAnchorFalloffSlider.SetValueWithoutNotify(leafWindAnchorFalloff);
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

    private void ReadWaterfallValuesFromMaterial()
    {
        if (waterfallMaterial == null)
        {
            return;
        }

        if (waterfallMaterial.HasProperty(WaterfallWaveSpeedId))
        {
            waterfallWaveSpeed = Mathf.Clamp(waterfallMaterial.GetFloat(WaterfallWaveSpeedId), -2f, 2f);
        }

        if (waterfallMaterial.HasProperty(WaterfallNoiseStrengthId))
        {
            waterfallPulseNoiseStrength = Mathf.Clamp01(waterfallMaterial.GetFloat(WaterfallNoiseStrengthId));
        }

        if (waterfallMaterial.HasProperty(WaterfallOpacityId))
        {
            waterfallOpacity = Mathf.Clamp01(waterfallMaterial.GetFloat(WaterfallOpacityId));
        }

        if (waterfallMaterial.HasProperty(WaterfallFoamAmountId))
        {
            waterfallFoamAmount = Mathf.Clamp01(waterfallMaterial.GetFloat(WaterfallFoamAmountId));
        }
    }

    private void ApplyWaterfallMaterialProperties()
    {
        if (waterfallMaterial == null)
        {
            return;
        }

        // HasProperty guards keep the controller safe while a material is being authored or replaced.
        if (waterfallMaterial.HasProperty(WaterfallWaveSpeedId))
        {
            waterfallMaterial.SetFloat(WaterfallWaveSpeedId, waterfallWaveSpeed);
        }

        if (waterfallMaterial.HasProperty(WaterfallNoiseStrengthId))
        {
            waterfallMaterial.SetFloat(WaterfallNoiseStrengthId, waterfallPulseNoiseStrength);
        }

        if (waterfallMaterial.HasProperty(WaterfallOpacityId))
        {
            waterfallMaterial.SetFloat(WaterfallOpacityId, waterfallOpacity);
        }

        if (waterfallMaterial.HasProperty(WaterfallFoamAmountId))
        {
            waterfallMaterial.SetFloat(WaterfallFoamAmountId, waterfallFoamAmount);
        }
    }

    private void ReadLeafWindValuesFromMaterial()
    {
        if (swayingLeafMaterial == null)
        {
            return;
        }

        if (swayingLeafMaterial.HasProperty(LeafWindStrengthId))
        {
            leafWindBendStrength = swayingLeafMaterial.GetFloat(LeafWindStrengthId);
        }

        if (swayingLeafMaterial.HasProperty(LeafWindSpeedId))
        {
            leafWindSpeed = swayingLeafMaterial.GetFloat(LeafWindSpeedId);
        }

        if (swayingLeafMaterial.HasProperty(LeafAnchorFalloffId))
        {
            leafWindAnchorFalloff = swayingLeafMaterial.GetFloat(LeafAnchorFalloffId);
        }
    }

    private void ApplyLeafWindMaterialProperties()
    {
        if (swayingLeafMaterial == null)
        {
            return;
        }

        if (swayingLeafMaterial.HasProperty(LeafWindStrengthId))
        {
            swayingLeafMaterial.SetFloat(LeafWindStrengthId, leafWindBendStrength);
        }

        if (swayingLeafMaterial.HasProperty(LeafWindSpeedId))
        {
            swayingLeafMaterial.SetFloat(LeafWindSpeedId, leafWindSpeed);
        }

        if (swayingLeafMaterial.HasProperty(LeafAnchorFalloffId))
        {
            swayingLeafMaterial.SetFloat(LeafAnchorFalloffId, leafWindAnchorFalloff);
        }
    }

    /// <summary>
    /// Persists the current material state while previewing in the Unity Editor.
    /// Runtime slider adjustments are otherwise discarded when Play mode ends.
    /// </summary>
    public void SaveStatueGoldShineMaterial()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            Debug.LogWarning("The shine material can only be saved from Play mode.", this);
            return;
        }

        if (statueGoldShineMaterial == null)
        {
            Debug.LogWarning("Cannot save the shine material because no material is assigned.", this);
            return;
        }

        EditorUtility.SetDirty(statueGoldShineMaterial);
        AssetDatabase.SaveAssets();
        Debug.Log($"Saved shine settings to '{AssetDatabase.GetAssetPath(statueGoldShineMaterial)}'.", this);
#else
        Debug.LogWarning("Saving authoring material settings is available only in the Unity Editor.", this);
#endif
    }
}
