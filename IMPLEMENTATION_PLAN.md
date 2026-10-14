# Panteon Technical Artist Case Implementation Plan

## Project outcome

Turn the supplied stylized jungle-pool artwork into a calm, animated 2D Canvas scene for mobile URP. The final scene must be reusable as one prefab, work in both 9:16 phone and 3:4 tablet layouts, and make its technical-art choices easy to inspect and explain.

## Requirements captured from the brief

- Use the supplied artwork as the visual source; preserve its composition and calm, stylized look.
- Build the scene entirely in Unity uGUI Canvas space with 2D image rendering.
- Separate the source into independently controllable layers. Use masks and textures where appropriate rather than destructively altering the original.
- Implement and document custom HLSL shaders. Keep shader code commented at the non-obvious decisions.
- Deliver four mandatory effects:
  1. pool surface and waterfall - water flow, reflection/refraction treatment, and side foam;
  2. calm sky and cloud movement;
  3. subtle wind movement in selected leaves and vines; and
  4. a configurable moving shine across the statue's metallic gold area, masked to that area and timed to fit the scene.
- Prioritize mobile/TBDR suitability, URP compatibility, visual quality, and stable performance.
- Use one reusable prefab and a controller that exposes and coordinates the effects.
- Validate at 1080 x 1920 (9:16) and 1536 x 2048 (3:4). Preserve the core composition in both.
- Prepare evidence: Frame Debugger / UI draw-call notes, short video, and technical documentation. The brief asks for a Unity 6.3.23f1 `.unitypackage` submission.

## Current baseline

- Unity 6.3.23f1 project using URP 17.3.0.
- `SampleScene` currently contains a Screen Space Overlay Canvas with the complete supplied image as one UI Image.
- The source artwork is `Assets/Sprites/TechnicalArtStageAsset.png` (1984 x 2152). Treat it as immutable source art.

## Work sequence

### 1. Layer and mask preparation - do this first

**Owner: manual art work**

Create editable source layers and export transparent PNGs at the original canvas dimensions whenever possible. Keep a PSD/Krita/Affinity source file outside Unity or in a clearly named `SourceArt` folder that is excluded from the package if it becomes too large.

Recommended rendered order, back to front:

1. sky gradient and clouds;
2. distant trees;
3. left/right middle vegetation;
4. statue base and non-metal statue body;
5. statue gold mask;
6. waterfall body;
7. waterfall foam and pool-side foam;
8. pool water base, reflection overlay, and ripple/caustic overlay;
9. foreground plants, stones, and bridge;
10. selected vine/leaf motion pieces.

Export utility masks alongside the visible layers:

- `Mask_Gold` - white only over gold metal that may receive the shine;
- `Mask_Water` - pool and waterfall water areas;
- `Mask_Foam` - foam/ripple boundaries;
- `Mask_Wind` - selected flexible foliage only; and
- optional grayscale depth/reflection mask for gentle water tinting.

Acceptance check: reassemble the PNG layers in the art editor and compare against the untouched source at 100% opacity. It should match before any animation is applied.

### 2. Canvas prefab foundation

**Owner: manual Unity setup**

1. Set the Canvas Scaler reference resolution to 1080 x 1920 and use `Scale With Screen Size` with a documented width/height match value.
2. Create `Assets/Prefabs/StylizedJungleScene.prefab` with one root RectTransform stretched to the Canvas.
3. Add child UI Images in the prepared render order. Keep every layer's name, sprite, material, and intended effect clear in the Inspector.
4. Use `RectMask2D` only where it saves work and does not introduce unwanted batches. Do not use a full-screen mask by default.
5. Make the demo scene contain only the Canvas/EventSystem and one instance of the prefab.

Acceptance check: the prefab visually matches the source at both target aspect ratios before effects are enabled.

### 3. Effect implementation

Implement in this order, validating after every effect.

1. **Waterfall and pool material**
   - Use one UI-compatible HLSL shader or a tightly scoped pair of materials.
   - Scroll a low-frequency distortion/noise texture for water flow; do not distort the pool silhouette or nearby art.
   - Use a subtle moving highlight/reflection overlay and masked foam/ripple motion.
   - Prefer shader motion over per-frame UI transform updates for broad water areas.
2. **Sky and clouds**
   - Keep the motion very slow: cloud UV drift/parallax plus a small color/gradient modulation is enough.
   - Avoid high-contrast or large-amplitude movement behind the statue.
3. **Foliage wind**
   - Animate only deliberately separated vines/leaves.
   - Use small, staggered sine-based offsets or a mesh/vertex solution with per-layer phase values. Pivot placement must make the motion feel anchored.
4. **Gold statue shine**
   - Use the gold mask, not a rectangular reveal.
   - Expose color, intensity, width, angle, speed, delay, and blur/softness.
   - The effect should sweep infrequently, with a soft edge and a deterministic phase so it feels intentional.
5. **Bonus only after the mandatory set is solid**
   - Strong candidates: very subtle caustics in the pool, a low-cost glowing-eye pulse, or occasional floating leaves.
   - Choose only one bonus unless profiling leaves clear headroom.

Acceptance check: each effect can be disabled independently, and disabling it returns its layer to the original look without artifacts.

### 4. Controller and configuration

Create one `StylizedSceneController` on the prefab root. It owns material-property updates, shared time values, effect enable switches, and high-level presets. It must not use `Update` to allocate memory or repeatedly call `GetComponent`.

Recommended exposed groups:

- global pause, master speed, and preview preset;
- water flow, ripple, foam, reflection, and refraction strengths;
- cloud speed and tint range;
- foliage amplitude, speed, and hover multiplier if hover is retained;
- shine color, width, angle, speed, intensity, interval, and mask material;
- optional bonus toggle.

Use material instances only where necessary. Prefer `MaterialPropertyBlock` only if it works reliably with the chosen UI renderer/material setup; otherwise keep a documented runtime material instance per effect group and destroy it cleanly.

Acceptance check: a reviewer can understand the scene from the prefab root and tune the four mandatory effects without opening shader code.

### 5. Mobile performance and layout validation

1. Check the Game view at 1080 x 1920 and 1536 x 2048, then a narrower/taller and a wider test resolution to catch anchoring issues.
2. Use the Unity Frame Debugger and Stats view after each major material addition. Record what causes each extra draw call.
3. Keep texture count and alpha-overdraw low. Avoid full-screen transparent layers when a smaller masked layer will do.
4. Prefer unlit UI shaders, low sample counts, no screen-color grab, and no expensive blur. Treat true refraction as stylized UV distortion, not a camera texture effect.
5. Test with the intended mobile-quality URP asset and check for shader/material compatibility.

Acceptance check: no obvious composition shift, clipping, stretched source art, or avoidable overdraw in either required target aspect ratio.

### 6. Submission evidence and packaging

1. Add comments to custom shader code and a README that explains layer order, masks, each effect, exposed controller parameters, and mobile-performance choices.
2. Capture a short video of the scene in action (the brief requests roughly 10 seconds at 24 FPS).
3. Capture Frame Debugger evidence with a one-line explanation of each relevant draw call or batch decision.
4. Export the demo scene and prefab in a `.unitypackage`, then import it into a clean Unity 6.3.23f1 project as a final smoke test.
5. Keep delivery documentation separate from working notes. Do not include employer-private instructions in the deliverable.

## Definition of done

- Four mandatory effects are visible, calm, configurable, and independently togglable.
- The scene is a single reusable prefab with one understandable controller.
- Custom HLSL is commented and URP/UI-compatible.
- The two required aspect ratios preserve the composition.
- The project has been checked for draw calls, batching/overdraw risks, and mobile suitability.
- Documentation, proof video, and clean `.unitypackage` import have all been verified.

## Immediate next action

Prepare the transparent layer exports and the five masks. Once those exist, we can map each file to its prefab child and decide the smallest shader/material set that achieves the mandatory effects.
