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
    private static readonly int PoolWaterOpacityId = Shader.PropertyToID("_Opacity");
    private static readonly int PoolWaterReflectionOpacityId = Shader.PropertyToID("_ReflectionOpacity");
    private static readonly int PoolWaterReflectionFadeDepthId = Shader.PropertyToID("_ReflectionFadeDepth");
    private static readonly int PoolWaterCausticsSizeId = Shader.PropertyToID("_CausticsSize");
    private static readonly int PoolWaterCausticsIntensityId = Shader.PropertyToID("_CausticsIntensity");
    private static readonly int PoolWaterWavesSizeId = Shader.PropertyToID("_WavesSize");
    private static readonly int PoolWaterWavesIntensityId = Shader.PropertyToID("_WavesIntensity");
    private static readonly int PoolWaterWavesMoveSpeedId = Shader.PropertyToID("_WavesMoveSpeed");
    private static readonly int PoolWaterFoamIntensityId = Shader.PropertyToID("_FoamIntensity");
    private static readonly int PoolWaterFoamDistortionSpeedId = Shader.PropertyToID("_FoamDistortionSpeed");
    private static readonly int PoolWaterFoamDistortionSizeId = Shader.PropertyToID("_FoamDistortionSize");
    private static readonly int WaterfallRippleSpeedId = Shader.PropertyToID("_RippleSpeed");
    private static readonly int PoolRippleRingSpawnScaleId = Shader.PropertyToID("_RippleRingSpawnScale");
    private static readonly int PoolRippleRingEndScaleId = Shader.PropertyToID("_RippleRingEndScale");
    private static readonly int PoolRippleRingSpacingId = Shader.PropertyToID("_RippleRingSpacing");
    private static readonly int PoolRippleSpawnFadeDurationId = Shader.PropertyToID("_RippleSpawnFadeDuration");
    private static readonly int PoolRippleVisualFadeId = Shader.PropertyToID("_RippleVisualFade");
    private static readonly int PoolRippleHeightStrengthId = Shader.PropertyToID("_RippleHeightStrength");
    private static readonly int PoolRippleFoamIntensityId = Shader.PropertyToID("_RippleFoamIntensity");
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

    [Header("Pool Water UI Bindings")]
    [Tooltip("Shared MT_Water material using Panteon/UI/Stylized Pool Water Foundation. The controller updates it directly and never creates runtime material instances.")]
    [SerializeField] private Material poolWaterMaterial;

    [Tooltip("Slider controlling the pool water's overall opacity. Its configured UI range is respected.")]
    [SerializeField] private Slider poolWaterOpacitySlider;

    [Tooltip("Slider controlling the reflected scene opacity. Its configured UI range is respected.")]
    [SerializeField] private Slider poolWaterReflectionOpacitySlider;

    [Tooltip("Slider controlling how far the reflection fade extends into the water. Its configured UI range is respected.")]
    [SerializeField] private Slider poolWaterReflectionFadeDepthSlider;

    [Tooltip("Slider controlling caustic pattern size. Its configured UI range is respected.")]
    [SerializeField] private Slider poolWaterCausticsSizeSlider;

    [Tooltip("Slider controlling caustic intensity. Its configured UI range is respected.")]
    [SerializeField] private Slider poolWaterCausticsIntensitySlider;

    [Tooltip("Slider controlling wave pattern size. Its configured UI range is respected.")]
    [SerializeField] private Slider poolWaterWavesSizeSlider;

    [Tooltip("Slider controlling wave intensity. Its configured UI range is respected.")]
    [SerializeField] private Slider poolWaterWavesIntensitySlider;

    [Tooltip("Slider controlling wave movement speed. Its configured UI range is respected.")]
    [SerializeField] private Slider poolWaterWavesMoveSpeedSlider;

    [Tooltip("Slider controlling edge-foam intensity. Its configured UI range is respected.")]
    [SerializeField] private Slider poolWaterFoamIntensitySlider;

    [Tooltip("Slider controlling foam-distortion speed. Its configured UI range is respected.")]
    [SerializeField] private Slider poolWaterFoamDistortionSpeedSlider;

    [Tooltip("Slider controlling foam-distortion pattern size. Its configured UI range is respected.")]
    [SerializeField] private Slider poolWaterFoamDistortionSizeSlider;

    [Header("Waterfall Ripple UI Bindings")]
    [Tooltip("Optional slider controlling the visible opacity of the ripple rings. Use a range of 0 to 1.")]
    [SerializeField] private Slider waterfallRippleRingFadeSlider;

    [Tooltip("Optional slider controlling the R-channel foam detail layered on the ripple rings. Use a range of 0 to 3.")]
    [SerializeField] private Slider waterfallRippleFoamIntensitySlider;

    [Tooltip("Optional slider controlling the ripple height effect for the water's visible relief and refraction. Use a range of 0 to 4.")]
    [SerializeField] private Slider waterfallRippleHeightEffectSlider;

    [Header("Authoring Save")]
    [Tooltip("Saves all controller-managed material values to disk when clicked in Play mode. This action is editor-only and is omitted from player builds.")]
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
    private float poolWaterOpacity;
    private float poolWaterReflectionOpacity;
    private float poolWaterReflectionFadeDepth;
    private float poolWaterCausticsSize;
    private float poolWaterCausticsIntensity;
    private float poolWaterWavesSize;
    private float poolWaterWavesIntensity;
    private float poolWaterWavesMoveSpeed;
    private float poolWaterFoamIntensity;
    private float poolWaterFoamDistortionSpeed;
    private float poolWaterFoamDistortionSize;
    private float waterfallRippleSpeed;
    private float waterfallRippleRingSpawnScale;
    private float waterfallRippleRingEndScale;
    private float waterfallRippleRingSpacing;
    private float waterfallRippleSpawnFadeDuration;
    private float waterfallRippleVisualFade;
    private float waterfallRippleFoamIntensity;
    private float waterfallRippleHeightEffect;

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
        InitializePoolWater();
        BindPoolWaterControls();
        InitializeWaterfallRipples();
        BindWaterfallRippleControls();
    }

    private void OnDisable()
    {
        UnbindStatueGoldShineControls();
        UnbindCloudMovementControls();
        UnbindRainControls();
        UnbindWaterfallControls();
        UnbindLeafWindControls();
        UnbindPoolWaterControls();
        UnbindWaterfallRippleControls();
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

    public void SetPoolWaterOpacity(float opacity)
    {
        poolWaterOpacity = opacity;
        ApplyPoolWaterMaterialProperties();
    }

    public void SetPoolWaterReflectionOpacity(float opacity)
    {
        poolWaterReflectionOpacity = opacity;
        ApplyPoolWaterMaterialProperties();
    }

    public void SetPoolWaterReflectionFadeDepth(float fadeDepth)
    {
        poolWaterReflectionFadeDepth = fadeDepth;
        ApplyPoolWaterMaterialProperties();
    }

    public void SetPoolWaterCausticsSize(float size)
    {
        poolWaterCausticsSize = size;
        ApplyPoolWaterMaterialProperties();
    }

    public void SetPoolWaterCausticsIntensity(float intensity)
    {
        poolWaterCausticsIntensity = intensity;
        ApplyPoolWaterMaterialProperties();
    }

    public void SetPoolWaterWavesSize(float size)
    {
        poolWaterWavesSize = size;
        ApplyPoolWaterMaterialProperties();
    }

    public void SetPoolWaterWavesIntensity(float intensity)
    {
        poolWaterWavesIntensity = intensity;
        ApplyPoolWaterMaterialProperties();
    }

    public void SetPoolWaterWavesMoveSpeed(float speed)
    {
        poolWaterWavesMoveSpeed = speed;
        ApplyPoolWaterMaterialProperties();
    }

    public void SetPoolWaterFoamIntensity(float intensity)
    {
        poolWaterFoamIntensity = intensity;
        ApplyPoolWaterMaterialProperties();
    }

    public void SetPoolWaterFoamDistortionSpeed(float speed)
    {
        poolWaterFoamDistortionSpeed = speed;
        ApplyPoolWaterMaterialProperties();
    }

    public void SetPoolWaterFoamDistortionSize(float size)
    {
        poolWaterFoamDistortionSize = size;
        ApplyPoolWaterMaterialProperties();
    }

    /// <summary>Sets the visible opacity of the composited ripple rings.</summary>
    public void SetWaterfallRippleRingFade(float fade)
    {
        waterfallRippleVisualFade = Mathf.Clamp01(fade);
        ApplyWaterfallRippleMaterialProperties();
    }

    /// <summary>Sets the visible R-channel foam detail without changing the ring height field.</summary>
    public void SetWaterfallRippleFoamIntensity(float intensity)
    {
        waterfallRippleFoamIntensity = Mathf.Clamp(intensity, 0f, 3f);
        ApplyWaterfallRippleMaterialProperties();
    }

    /// <summary>Sets the shared height multiplier used by the visible relief and pool refraction.</summary>
    public void SetWaterfallRippleHeightEffect(float strength)
    {
        waterfallRippleHeightEffect = Mathf.Clamp(strength, 0f, 4f);
        ApplyWaterfallRippleMaterialProperties();
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
            saveSettingsButton.onClick.AddListener(SaveControlledMaterialSettings);
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
            saveSettingsButton.onClick.RemoveListener(SaveControlledMaterialSettings);
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

    private void InitializePoolWater()
    {
        ReadPoolWaterValuesFromMaterial();
        ApplyPoolWaterMaterialProperties();
        SynchronizePoolWaterControls();
    }

    private void BindPoolWaterControls()
    {
        UnbindPoolWaterControls();

        if (poolWaterOpacitySlider != null)
        {
            poolWaterOpacitySlider.onValueChanged.AddListener(SetPoolWaterOpacity);
        }

        if (poolWaterReflectionOpacitySlider != null)
        {
            poolWaterReflectionOpacitySlider.onValueChanged.AddListener(SetPoolWaterReflectionOpacity);
        }

        if (poolWaterReflectionFadeDepthSlider != null)
        {
            poolWaterReflectionFadeDepthSlider.onValueChanged.AddListener(SetPoolWaterReflectionFadeDepth);
        }

        if (poolWaterCausticsSizeSlider != null)
        {
            poolWaterCausticsSizeSlider.onValueChanged.AddListener(SetPoolWaterCausticsSize);
        }

        if (poolWaterCausticsIntensitySlider != null)
        {
            poolWaterCausticsIntensitySlider.onValueChanged.AddListener(SetPoolWaterCausticsIntensity);
        }

        if (poolWaterWavesSizeSlider != null)
        {
            poolWaterWavesSizeSlider.onValueChanged.AddListener(SetPoolWaterWavesSize);
        }

        if (poolWaterWavesIntensitySlider != null)
        {
            poolWaterWavesIntensitySlider.onValueChanged.AddListener(SetPoolWaterWavesIntensity);
        }

        if (poolWaterWavesMoveSpeedSlider != null)
        {
            poolWaterWavesMoveSpeedSlider.onValueChanged.AddListener(SetPoolWaterWavesMoveSpeed);
        }

        if (poolWaterFoamIntensitySlider != null)
        {
            poolWaterFoamIntensitySlider.onValueChanged.AddListener(SetPoolWaterFoamIntensity);
        }

        if (poolWaterFoamDistortionSpeedSlider != null)
        {
            poolWaterFoamDistortionSpeedSlider.onValueChanged.AddListener(SetPoolWaterFoamDistortionSpeed);
        }

        if (poolWaterFoamDistortionSizeSlider != null)
        {
            poolWaterFoamDistortionSizeSlider.onValueChanged.AddListener(SetPoolWaterFoamDistortionSize);
        }
    }

    private void UnbindPoolWaterControls()
    {
        if (poolWaterOpacitySlider != null)
        {
            poolWaterOpacitySlider.onValueChanged.RemoveListener(SetPoolWaterOpacity);
        }

        if (poolWaterReflectionOpacitySlider != null)
        {
            poolWaterReflectionOpacitySlider.onValueChanged.RemoveListener(SetPoolWaterReflectionOpacity);
        }

        if (poolWaterReflectionFadeDepthSlider != null)
        {
            poolWaterReflectionFadeDepthSlider.onValueChanged.RemoveListener(SetPoolWaterReflectionFadeDepth);
        }

        if (poolWaterCausticsSizeSlider != null)
        {
            poolWaterCausticsSizeSlider.onValueChanged.RemoveListener(SetPoolWaterCausticsSize);
        }

        if (poolWaterCausticsIntensitySlider != null)
        {
            poolWaterCausticsIntensitySlider.onValueChanged.RemoveListener(SetPoolWaterCausticsIntensity);
        }

        if (poolWaterWavesSizeSlider != null)
        {
            poolWaterWavesSizeSlider.onValueChanged.RemoveListener(SetPoolWaterWavesSize);
        }

        if (poolWaterWavesIntensitySlider != null)
        {
            poolWaterWavesIntensitySlider.onValueChanged.RemoveListener(SetPoolWaterWavesIntensity);
        }

        if (poolWaterWavesMoveSpeedSlider != null)
        {
            poolWaterWavesMoveSpeedSlider.onValueChanged.RemoveListener(SetPoolWaterWavesMoveSpeed);
        }

        if (poolWaterFoamIntensitySlider != null)
        {
            poolWaterFoamIntensitySlider.onValueChanged.RemoveListener(SetPoolWaterFoamIntensity);
        }

        if (poolWaterFoamDistortionSpeedSlider != null)
        {
            poolWaterFoamDistortionSpeedSlider.onValueChanged.RemoveListener(SetPoolWaterFoamDistortionSpeed);
        }

        if (poolWaterFoamDistortionSizeSlider != null)
        {
            poolWaterFoamDistortionSizeSlider.onValueChanged.RemoveListener(SetPoolWaterFoamDistortionSize);
        }
    }

    private void InitializeWaterfallRipples()
    {
        ReadWaterfallRippleValuesFromMaterial();
        ApplyWaterfallRippleMaterialProperties();
        SynchronizeWaterfallRippleControls();
    }

    private void BindWaterfallRippleControls()
    {
        UnbindWaterfallRippleControls();

        if (waterfallRippleRingFadeSlider != null)
        {
            waterfallRippleRingFadeSlider.onValueChanged.AddListener(SetWaterfallRippleRingFade);
        }

        if (waterfallRippleFoamIntensitySlider != null)
        {
            waterfallRippleFoamIntensitySlider.onValueChanged.AddListener(SetWaterfallRippleFoamIntensity);
        }

        if (waterfallRippleHeightEffectSlider != null)
        {
            waterfallRippleHeightEffectSlider.onValueChanged.AddListener(SetWaterfallRippleHeightEffect);
        }
    }

    private void UnbindWaterfallRippleControls()
    {
        if (waterfallRippleRingFadeSlider != null)
        {
            waterfallRippleRingFadeSlider.onValueChanged.RemoveListener(SetWaterfallRippleRingFade);
        }

        if (waterfallRippleFoamIntensitySlider != null)
        {
            waterfallRippleFoamIntensitySlider.onValueChanged.RemoveListener(SetWaterfallRippleFoamIntensity);
        }

        if (waterfallRippleHeightEffectSlider != null)
        {
            waterfallRippleHeightEffectSlider.onValueChanged.RemoveListener(SetWaterfallRippleHeightEffect);
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

    private void SynchronizePoolWaterControls()
    {
        if (poolWaterOpacitySlider != null)
        {
            poolWaterOpacitySlider.SetValueWithoutNotify(poolWaterOpacity);
        }

        if (poolWaterReflectionOpacitySlider != null)
        {
            poolWaterReflectionOpacitySlider.SetValueWithoutNotify(poolWaterReflectionOpacity);
        }

        if (poolWaterReflectionFadeDepthSlider != null)
        {
            poolWaterReflectionFadeDepthSlider.SetValueWithoutNotify(poolWaterReflectionFadeDepth);
        }

        if (poolWaterCausticsSizeSlider != null)
        {
            poolWaterCausticsSizeSlider.SetValueWithoutNotify(poolWaterCausticsSize);
        }

        if (poolWaterCausticsIntensitySlider != null)
        {
            poolWaterCausticsIntensitySlider.SetValueWithoutNotify(poolWaterCausticsIntensity);
        }

        if (poolWaterWavesSizeSlider != null)
        {
            poolWaterWavesSizeSlider.SetValueWithoutNotify(poolWaterWavesSize);
        }

        if (poolWaterWavesIntensitySlider != null)
        {
            poolWaterWavesIntensitySlider.SetValueWithoutNotify(poolWaterWavesIntensity);
        }

        if (poolWaterWavesMoveSpeedSlider != null)
        {
            poolWaterWavesMoveSpeedSlider.SetValueWithoutNotify(poolWaterWavesMoveSpeed);
        }

        if (poolWaterFoamIntensitySlider != null)
        {
            poolWaterFoamIntensitySlider.SetValueWithoutNotify(poolWaterFoamIntensity);
        }

        if (poolWaterFoamDistortionSpeedSlider != null)
        {
            poolWaterFoamDistortionSpeedSlider.SetValueWithoutNotify(poolWaterFoamDistortionSpeed);
        }

        if (poolWaterFoamDistortionSizeSlider != null)
        {
            poolWaterFoamDistortionSizeSlider.SetValueWithoutNotify(poolWaterFoamDistortionSize);
        }
    }

    private void SynchronizeWaterfallRippleControls()
    {
        if (waterfallRippleRingFadeSlider != null)
        {
            waterfallRippleRingFadeSlider.SetValueWithoutNotify(waterfallRippleVisualFade);
        }

        if (waterfallRippleFoamIntensitySlider != null)
        {
            waterfallRippleFoamIntensitySlider.SetValueWithoutNotify(waterfallRippleFoamIntensity);
        }

        if (waterfallRippleHeightEffectSlider != null)
        {
            waterfallRippleHeightEffectSlider.SetValueWithoutNotify(waterfallRippleHeightEffect);
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

    private void ReadPoolWaterValuesFromMaterial()
    {
        if (poolWaterMaterial == null)
        {
            return;
        }

        if (poolWaterMaterial.HasProperty(PoolWaterOpacityId)) poolWaterOpacity = poolWaterMaterial.GetFloat(PoolWaterOpacityId);
        if (poolWaterMaterial.HasProperty(PoolWaterReflectionOpacityId)) poolWaterReflectionOpacity = poolWaterMaterial.GetFloat(PoolWaterReflectionOpacityId);
        if (poolWaterMaterial.HasProperty(PoolWaterReflectionFadeDepthId)) poolWaterReflectionFadeDepth = poolWaterMaterial.GetFloat(PoolWaterReflectionFadeDepthId);
        if (poolWaterMaterial.HasProperty(PoolWaterCausticsSizeId)) poolWaterCausticsSize = poolWaterMaterial.GetFloat(PoolWaterCausticsSizeId);
        if (poolWaterMaterial.HasProperty(PoolWaterCausticsIntensityId)) poolWaterCausticsIntensity = poolWaterMaterial.GetFloat(PoolWaterCausticsIntensityId);
        if (poolWaterMaterial.HasProperty(PoolWaterWavesSizeId)) poolWaterWavesSize = poolWaterMaterial.GetFloat(PoolWaterWavesSizeId);
        if (poolWaterMaterial.HasProperty(PoolWaterWavesIntensityId)) poolWaterWavesIntensity = poolWaterMaterial.GetFloat(PoolWaterWavesIntensityId);
        if (poolWaterMaterial.HasProperty(PoolWaterWavesMoveSpeedId)) poolWaterWavesMoveSpeed = poolWaterMaterial.GetFloat(PoolWaterWavesMoveSpeedId);
        if (poolWaterMaterial.HasProperty(PoolWaterFoamIntensityId)) poolWaterFoamIntensity = poolWaterMaterial.GetFloat(PoolWaterFoamIntensityId);
        if (poolWaterMaterial.HasProperty(PoolWaterFoamDistortionSpeedId)) poolWaterFoamDistortionSpeed = poolWaterMaterial.GetFloat(PoolWaterFoamDistortionSpeedId);
        if (poolWaterMaterial.HasProperty(PoolWaterFoamDistortionSizeId)) poolWaterFoamDistortionSize = poolWaterMaterial.GetFloat(PoolWaterFoamDistortionSizeId);
    }

    private void ApplyPoolWaterMaterialProperties()
    {
        if (poolWaterMaterial == null)
        {
            return;
        }

        // Each guard keeps this controller safe if the water material is temporarily swapped.
        if (poolWaterMaterial.HasProperty(PoolWaterOpacityId)) poolWaterMaterial.SetFloat(PoolWaterOpacityId, poolWaterOpacity);
        if (poolWaterMaterial.HasProperty(PoolWaterReflectionOpacityId)) poolWaterMaterial.SetFloat(PoolWaterReflectionOpacityId, poolWaterReflectionOpacity);
        if (poolWaterMaterial.HasProperty(PoolWaterReflectionFadeDepthId)) poolWaterMaterial.SetFloat(PoolWaterReflectionFadeDepthId, poolWaterReflectionFadeDepth);
        if (poolWaterMaterial.HasProperty(PoolWaterCausticsSizeId)) poolWaterMaterial.SetFloat(PoolWaterCausticsSizeId, poolWaterCausticsSize);
        if (poolWaterMaterial.HasProperty(PoolWaterCausticsIntensityId)) poolWaterMaterial.SetFloat(PoolWaterCausticsIntensityId, poolWaterCausticsIntensity);
        if (poolWaterMaterial.HasProperty(PoolWaterWavesSizeId)) poolWaterMaterial.SetFloat(PoolWaterWavesSizeId, poolWaterWavesSize);
        if (poolWaterMaterial.HasProperty(PoolWaterWavesIntensityId)) poolWaterMaterial.SetFloat(PoolWaterWavesIntensityId, poolWaterWavesIntensity);
        if (poolWaterMaterial.HasProperty(PoolWaterWavesMoveSpeedId)) poolWaterMaterial.SetFloat(PoolWaterWavesMoveSpeedId, poolWaterWavesMoveSpeed);
        if (poolWaterMaterial.HasProperty(PoolWaterFoamIntensityId)) poolWaterMaterial.SetFloat(PoolWaterFoamIntensityId, poolWaterFoamIntensity);
        if (poolWaterMaterial.HasProperty(PoolWaterFoamDistortionSpeedId)) poolWaterMaterial.SetFloat(PoolWaterFoamDistortionSpeedId, poolWaterFoamDistortionSpeed);
        if (poolWaterMaterial.HasProperty(PoolWaterFoamDistortionSizeId)) poolWaterMaterial.SetFloat(PoolWaterFoamDistortionSizeId, poolWaterFoamDistortionSize);
    }

    private void ReadWaterfallRippleValuesFromMaterial()
    {
        if (poolWaterMaterial == null)
        {
            return;
        }

        // The pool material is now the single source of truth for both the visible impact and
        // its local refraction. Keeping this state in one material removes timeline drift.
        if (poolWaterMaterial.HasProperty(WaterfallRippleSpeedId)) waterfallRippleSpeed = poolWaterMaterial.GetFloat(WaterfallRippleSpeedId);
        if (poolWaterMaterial.HasProperty(PoolRippleRingSpawnScaleId)) waterfallRippleRingSpawnScale = poolWaterMaterial.GetFloat(PoolRippleRingSpawnScaleId);
        if (poolWaterMaterial.HasProperty(PoolRippleRingEndScaleId)) waterfallRippleRingEndScale = poolWaterMaterial.GetFloat(PoolRippleRingEndScaleId);
        if (poolWaterMaterial.HasProperty(PoolRippleRingSpacingId)) waterfallRippleRingSpacing = poolWaterMaterial.GetFloat(PoolRippleRingSpacingId);
        if (poolWaterMaterial.HasProperty(PoolRippleSpawnFadeDurationId)) waterfallRippleSpawnFadeDuration = poolWaterMaterial.GetFloat(PoolRippleSpawnFadeDurationId);
        if (poolWaterMaterial.HasProperty(PoolRippleVisualFadeId)) waterfallRippleVisualFade = poolWaterMaterial.GetFloat(PoolRippleVisualFadeId);
        if (poolWaterMaterial.HasProperty(PoolRippleFoamIntensityId)) waterfallRippleFoamIntensity = poolWaterMaterial.GetFloat(PoolRippleFoamIntensityId);
        if (poolWaterMaterial.HasProperty(PoolRippleHeightStrengthId)) waterfallRippleHeightEffect = poolWaterMaterial.GetFloat(PoolRippleHeightStrengthId);
    }

    private void ApplyWaterfallRippleMaterialProperties()
    {
        if (poolWaterMaterial == null)
        {
            return;
        }

        // HasProperty guards retain a safe static-water fallback while this material is authored.
        if (poolWaterMaterial.HasProperty(WaterfallRippleSpeedId)) poolWaterMaterial.SetFloat(WaterfallRippleSpeedId, waterfallRippleSpeed);
        if (poolWaterMaterial.HasProperty(PoolRippleRingSpawnScaleId)) poolWaterMaterial.SetFloat(PoolRippleRingSpawnScaleId, waterfallRippleRingSpawnScale);
        if (poolWaterMaterial.HasProperty(PoolRippleRingEndScaleId)) poolWaterMaterial.SetFloat(PoolRippleRingEndScaleId, waterfallRippleRingEndScale);
        if (poolWaterMaterial.HasProperty(PoolRippleRingSpacingId)) poolWaterMaterial.SetFloat(PoolRippleRingSpacingId, waterfallRippleRingSpacing);
        if (poolWaterMaterial.HasProperty(PoolRippleSpawnFadeDurationId)) poolWaterMaterial.SetFloat(PoolRippleSpawnFadeDurationId, waterfallRippleSpawnFadeDuration);
        if (poolWaterMaterial.HasProperty(PoolRippleVisualFadeId)) poolWaterMaterial.SetFloat(PoolRippleVisualFadeId, waterfallRippleVisualFade);
        if (poolWaterMaterial.HasProperty(PoolRippleFoamIntensityId)) poolWaterMaterial.SetFloat(PoolRippleFoamIntensityId, waterfallRippleFoamIntensity);
        if (poolWaterMaterial.HasProperty(PoolRippleHeightStrengthId)) poolWaterMaterial.SetFloat(PoolRippleHeightStrengthId, waterfallRippleHeightEffect);
    }

    /// <summary>
    /// Persists every controller-managed material state while previewing in the Unity Editor.
    /// Runtime slider adjustments are otherwise discarded when Play mode ends.
    /// </summary>
    public void SaveControlledMaterialSettings()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            Debug.LogWarning("Controlled material settings can only be saved from Play mode.", this);
            return;
        }

        int materialCount = 0;
        materialCount += MarkMaterialForSave(statueGoldShineMaterial) ? 1 : 0;
        materialCount += MarkMaterialForSave(waterfallMaterial) ? 1 : 0;
        materialCount += MarkMaterialForSave(swayingLeafMaterial) ? 1 : 0;
        materialCount += MarkMaterialForSave(poolWaterMaterial) ? 1 : 0;

        if (materialCount == 0)
        {
            Debug.LogWarning("Cannot save settings because no controller-managed materials are assigned.", this);
            return;
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"Saved {materialCount} controller-managed material setting asset(s).", this);
#else
        Debug.LogWarning("Saving authoring material settings is available only in the Unity Editor.", this);
#endif
    }

    /// <summary>Compatibility callback for an existing manually wired Save button event.</summary>
    public void SaveStatueGoldShineMaterial()
    {
        SaveControlledMaterialSettings();
    }

#if UNITY_EDITOR
    private static bool MarkMaterialForSave(Material material)
    {
        if (material == null)
        {
            return false;
        }

        EditorUtility.SetDirty(material);
        return true;
    }
#endif
}
