# Panteon Technical Artist Case Implementation Plan

## Goal

Turn the supplied stylized jungle-pool composition into a calm, reusable, mobile-friendly uGUI scene in Unity 6.3.23f1 / URP 17.3.0. Preserve the composition at 1080 x 1920 (9:16) and 1536 x 2048 (3:4).

## Current implementation status

| Area | State | Notes |
| --- | --- | --- |
| Scene composition | In progress | `PanteonCaseScene.unity` is the authored scene. No reusable `StylizedJungleScene.prefab` exists yet. |
| Statue gold shine | Implemented / needs final art review | Masked, configurable UI HLSL material controlled by `StylizedSceneController`. |
| Clouds | Implemented / needs final art review | Animator Float parameter `Speed` is driven by the Cloud Speed UI slider. |
| Rain | Implemented / needs device verification | Rain slider maps 0–30 to particle emission and darkens `Cloud1` HSV value to 70%. A camera/RenderTexture path needs iOS verification. |
| Pool water | Implemented / needs art and device review | Two packed masks drive caustics, waves, perimeter, foam, and foam distortion; a supplied reflection texture is flipped and depth-faded. |
| Waterfall | Implemented / needs art and device review | Separate stylized flow/foam shader using a painted RGBA mask and linked width control. |
| Leaves and vines | Implemented / needs art review | Selected leaves use a shared anchored vertex-sway material. |
| Mobile profiling | Static audit complete; device work pending | Low reported Frame Debugger count is not sufficient; actual iOS GPU/CPU/memory profiling is required. |
| Layout validation | Not started | Test both required aspect ratios after the composition/prefab is stable. |

## Completed technical work

1. Imported layered PSD-based art and built a Canvas composition.
2. Added `Panteon/UI/Statue Gold Shine`, `MT_CatStatueShine.mat`, and the UI controls for shine angle, speed, width, softness, and colour/intensity.
3. Added editor-only Play Mode saving for the shine material through the Canvas button.
4. Connected the cloud Animator's `Speed` Float parameter to the Cloud Speed UI slider.
5. Connected the Rain Amount UI slider to `RainParticle` emission and `Cloud1` darkening.
6. Removed unused Visual Scripting, Timeline, and Multiplayer Center packages. This did not affect the uGUI visibility issue; the missing UI was caused by building the wrong scene.
7. Added independently controllable waterfall flow/foam and linked the waterfall, CatStatue, and foam-particle widths through one normalized control.
8. Added anchored vertex wind sway for selected leaves and its three UI controls.
9. Rebuilt pool water around `TX_CausticsAndWaves_Mask.png`, `TX_WaterBorderAndNoises_Mask.png`, and `TX_Scene_Reflection.png`; added water-panel controls and Editor Play Mode material saving.
10. Completed a read-only static performance audit. The primary candidates for device profiling are the large 9-sample water pass, PSD-derived 2K mipmapped texture pages, auxiliary particle-capture cameras, and enabled decorative UI raycasts.

## Water: current pass and next milestone

`StylizedPoolWater.shader` now reads two authored masks: RG caustic states and BA wave states from `TX_CausticsAndWaves_Mask`, plus R perimeter, G foam placement, and BA distortion noise from `TX_WaterBorderAndNoises_Mask`. It combines these with a vertically flipped supplied reflection texture. It remains unlit UI shading; it does not use a camera colour grab or screen-space reflection.

The next water milestone is visual/device review, not adding controls: check the final artistic read in Play Mode, keep a static fallback/enable path, then measure the cost of its large transparent nine-sample pass on device.

## Remaining sequence

1. Review the current effects visually and add their static fallback / enable switches.
2. Profile on the intended iOS device: baseline, water disabled, capture cameras disabled, and decorative raycasts disabled.
3. Apply accepted import/Canvas/camera optimizations and validate 9:16 and 3:4 after each one.
4. Create the final reusable prefab at `Assets/Prefabs/StylizedJungleScene.prefab`; place `StylizedSceneController` on its root and explicitly assign materials/layers.
5. Record Frame Debugger, Stats, device-profile results, and texture/RenderTexture memory.
6. Capture a short video and test a clean Unity 6.3.23f1 import before packaging.

## Constraints

- Keep `Assets/Sprites/TechnicalArtStageAsset.png` immutable.
- Use commented, unlit UI HLSL; avoid camera-colour grabs, full-screen blur, and avoidable material variants.
- Do not create runtime material instances, per-frame allocations, or repeated `GetComponent` calls.
- Keep the scene calm: very small distortion, low-frequency movement, and minimal overdraw.
- Do not overwrite user-created art, scene work, or prefab work.

For exact current paths, material values, known risks, and worktree cautions, read [PROJECT_HANDOFF.md](PROJECT_HANDOFF.md).
