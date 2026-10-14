# Project Handoff — 2026-10-14

## Purpose

This is the continuation note for the next working session. It records actual implemented state, current limitations, and safe next actions. It is a working document, not final delivery documentation.

## Environment and scene

- Unity `6000.3.23f1`, URP `17.3.0`.
- The current authored scene is `Assets/Scenes/PanteonCaseScene.unity`.
- The Canvas is Screen Space Overlay. A camera cannot hide or render its Overlay UI; use this when diagnosing camera/RenderTexture issues.
- The project is intentionally dirty. Preserve all pre-existing user changes, especially the PSD edits and the deletion of `SampleScene` / its meta file. Do not restore or save them automatically.

## Implemented scene controls

`Assets/Scripts/StylizedSceneController.cs` is attached to the scene root and currently owns:

| Feature | Current binding / behaviour |
| --- | --- |
| Statue shine | Uses `MT_CatStatueShine.mat`; UI sliders control angle (0–360), speed (-2–2), width (0.01–0.5), and softness (0.001–0.25). Colour stays serialized on the controller. |
| Save button | In Unity Editor Play Mode only, writes the current statue-shine material values to disk. It does not save general scene/UI/water state. |
| Clouds | `Clouds` Animator Float parameter `Speed` is driven by the Cloud Speed UI slider (-1–1). Negative values reverse motion. |
| Rain | Rain Amount slider (0–30) writes `RainParticle` rate-over-time. The same normalized amount darkens `Cloud1` HSV value from 100% to 70%, preserving hue/saturation/alpha. |

No water controls are wired into this controller yet; do not add them until the desired water look is agreed.

## Rain / iOS note

- `RainFall` RawImage should reference `Assets/Textures/RTX_RainCamera.renderTexture`.
- Its colour format `R8G8B8A8_UNORM` is compatible with iOS. A previous inspection found the `RainCamera` target texture unset even though the RawImage pointed to the render texture; verify this in the Inspector before the next device build.
- A depth/stencil buffer is not required for simple rain particles and can be removed later if device tests warrant it.

## Water: exact current state

### Assets

- Shader: `Assets/Shaders/UI/StylizedPoolWater.shader`
- Active material: `Assets/Materials/MT_Water.mat`
- Intended target: `PF_PanteonCase/Water` Image
- Packed mask: `Assets/Textures/TX_CausticsWavesWaterFoam_Mask.png`

The active material references the packed mask. Its user-authored material values should be preserved. The Water Image uses the PSD Water sprite and the material above.

### Packed texture channels

| Channel | Intended use in current shader |
| --- | --- |
| R | Panned/scaled caustics pattern |
| G | Panned wave-height/noise field, reconstructed as a pseudo normal |
| B | Static water coverage / final alpha boundary |
| A | Hand-authored foam placement |

### What the current shader does

- Keeps the B coverage boundary static.
- Samples the G channel three times to reconstruct a local pseudo normal.
- Uses that normal to shift the Water Image’s own UVs (local/self refraction) and produce directional surface light.
- Applies a two-colour vertical grade while retaining the sprite detail.
- Pans R caustics independently.
- Offsets the A foam lookup with the same G noise instead of using a sine pulse.
- Supports standard uGUI stencil, RectMask2D clipping, and alpha clipping.

### Why it is not yet accepted

The user reviewed this version and said it remains visually unsatisfactory. The shader can only distort and light the Water Image itself; it cannot actually reflect/refract nearby scene art because it has no source texture for that art. Adding more controls alone is unlikely to solve the artistic issue.

The next session should begin with a Play Mode visual review and answer one question: **what should the water primarily communicate—painted ripples, a reflected jungle/sky layer, or transparent pool water with the art below visibly refracted?**

Recommended next action after that choice:

1. Inspect the mask channels at full resolution. The G channel must be a smooth, low-frequency height field; A must only contain foam edges.
2. If actual reflection/refraction is desired, ask the artist for a separate, UV-aligned source layer (for example, a muted reflected sky/foliage image or background-under-water image). Sample that in a second pass/material instead of adding a camera grab.
3. Keep pool water and waterfall as separate materials/effects.
4. Once the visual direction works, expose only the chosen 3–5 water parameters on `StylizedSceneController` / the Canvas panel.
5. Then profile: the current exploratory pass has one Water-sprite sample and six packed-mask samples per shaded pixel, so it must be reviewed with overdraw and device evidence before final approval.

## Build history and packages

- Visual Scripting, Timeline, and Multiplayer Center packages were removed to reduce unused project content. No asset/code usage was found.
- A previous missing-control-panel build was diagnosed as a build-scene problem: iOS build settings then included `SampleScene` instead of `PanteonCaseScene`. Because `SampleScene` is now a user worktree deletion, re-check the build scene list manually before the next iOS build. Do not assume it is fixed.

## Unfinished delivery work

- Final prefab at `Assets/Prefabs/StylizedJungleScene.prefab`.
- Waterfall flow and foam treatment.
- Selected leaf/vine wind with anchored pivots.
- Water acceptance, static fallback, and enable switch.
- 9:16 / 3:4 validation.
- Frame Debugger, Stats, iOS device profiling, and RenderTexture validation.
- Final README performance notes, video, clean-project import, and `.unitypackage` smoke test.

## Safe continuation rules

- Do not edit `.unity` or `.prefab` YAML while the Editor is open; use the live Editor/CLI.
- Do not save the active scene or apply prefab overrides unless the user explicitly asks.
- Do not delete/restore current untracked/modified art, scenes, or material assets.
- Do not claim the project is optimized for mobile without recorded profiling evidence.
