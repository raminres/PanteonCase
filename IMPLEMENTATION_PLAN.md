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
| Pool water | Exploratory / not accepted | Packed-mask shader exists but has not reached the desired visual quality. |
| Waterfall | Not implemented | Keep separate from the pool material if their motion needs differ. |
| Leaves and vines | Not implemented | Requires selected, anchored, separately exported layers. |
| Mobile profiling | Not started | Frame Debugger, Stats, and device profiling are still required. |
| Layout validation | Not started | Test both required aspect ratios after the composition/prefab is stable. |

## Completed technical work

1. Imported layered PSD-based art and built a Canvas composition.
2. Added `Panteon/UI/Statue Gold Shine`, `MT_CatStatueShine.mat`, and the UI controls for shine angle, speed, width, softness, and colour/intensity.
3. Added editor-only Play Mode saving for the shine material through the Canvas button.
4. Connected the cloud Animator's `Speed` Float parameter to the Cloud Speed UI slider.
5. Connected the Rain Amount UI slider to `RainParticle` emission and `Cloud1` darkening.
6. Removed unused Visual Scripting, Timeline, and Multiplayer Center packages. This did not affect the uGUI visibility issue; the missing UI was caused by building the wrong scene.
7. Added an initial water shader using `TX_CausticsWavesWaterFoam_Mask.png`.

## Water: current pass and the next milestone

`StylizedPoolWater.shader` currently reads a packed RGBA mask:

- R: drifting caustic pattern;
- G: panned wave height/noise, used for self-distortion and a pseudo normal;
- B: fixed water coverage/boundary; and
- A: painted foam placement.

The shader currently provides a two-colour vertical grade, local sprite distortion, pseudo-normal directional light, drifting caustics, and noise-distorted foam. It deliberately does **not** use a screen grab, camera colour texture, or physical planar reflection.

The user has judged this result unsatisfactory. Do not continue by merely adding more knobs. Start the next session with a visual diagnosis and choose a single stronger art direction:

1. inspect the Water layer, packed mask channels, and current material in Play Mode;
2. decide whether the desired water is painted/stylized ripples, visible reflected art, or transparent refracted background;
3. if reflected/refracted surrounding art is required, prepare a separate source image/layer behind the water instead of self-sampling the Water Image;
4. verify that the G channel is a smooth, low-frequency height texture and that A is only the intended foam edge; and
5. keep waterfall flow as a separate scoped pass.

## Remaining sequence

1. Finish the pool-water visual design and add a static fallback / enable switch.
2. Implement waterfall flow and side foam with a separately tuned material if needed.
3. Create the final reusable prefab at `Assets/Prefabs/StylizedJungleScene.prefab`; place `StylizedSceneController` on its root and explicitly assign materials/layers.
4. Add restrained anchored wind animation to selected vines/leaves.
5. Expose only approved water controls on the controller/UI after the water look is accepted.
6. Validate both target aspect ratios, then inspect Stats and Frame Debugger.
7. Profile on the intended iOS device before claiming mobile performance.
8. Record results, capture a short video, and test a clean Unity 6.3.23f1 import before packaging.

## Constraints

- Keep `Assets/Sprites/TechnicalArtStageAsset.png` immutable.
- Use commented, unlit UI HLSL; avoid camera-colour grabs, full-screen blur, and avoidable material variants.
- Do not create runtime material instances, per-frame allocations, or repeated `GetComponent` calls.
- Keep the scene calm: very small distortion, low-frequency movement, and minimal overdraw.
- Do not overwrite user-created art, scene work, or prefab work.

For exact current paths, material values, known risks, and worktree cautions, read [PROJECT_HANDOFF.md](PROJECT_HANDOFF.md).
