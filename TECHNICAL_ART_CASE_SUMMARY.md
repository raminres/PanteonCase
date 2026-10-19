# Stylized Jungle Pool — Technical-Art Case Summary

## Overview

This project is a Unity technical-art study that turns a supplied stylized jungle-pool illustration into a calm, interactive 2D scene. The scene is authored for Unity `6.3.23f1` with `URP 17.3.0` and is built on a Screen Space Overlay uGUI Canvas rather than a 3D environment.

The goal was to preserve the original composition while giving selected areas believable motion and material response. The implementation prioritizes clear artistic control, reusable shader logic, and a mobile-conscious rendering approach. The active scene is `Assets/Scenes/PanteonCaseScene.unity`.

## Technical Approach

The artwork remains layer-based and the animation is added through narrowly scoped, unlit HLSL UI shaders. Rather than relying on full-screen post effects, camera grabs, or simulation, the scene uses packed masks, UV motion, vertex deformation, and supplied reflection artwork.

This keeps the effects appropriate for a Canvas composition:

- The original illustration remains the visual foundation.
- Each animated element is isolated to the layer where it belongs.
- Shader parameters are exposed as artist-facing material properties and, where useful, as uGUI controls.
- The effects are intentionally restrained so the scene reads as a living illustration rather than a heavily distorted surface.

## Implemented Effects

| Area | Implementation | Key artistic controls |
| --- | --- | --- |
| Waterfall | `StylizedWaterfall.shader` animates a packed streak mask: `R` for flow streaks, `G` for foam, `B` for pulsing variation, and `A` for the fixed silhouette. | Width, flow speed, pulse strength, opacity, foam amount, foam position, and tiling. |
| Pool water | `StylizedPoolWater.shader` combines a two-colour vertical grade, supplied reflection art, caustics, waves, edge foam, and foam distortion. | Opacity, reflection opacity/depth, caustics size/intensity/contrast, caustics X/Y perspective scale, wave settings, and foam settings. |
| Waterfall impact ripples | The ripple system is integrated into the pool-water shader. A continuous five-ring train expands from the waterfall contact point; the same height field drives both the visible rings and local water-detail refraction. | Position, X/Y oval size, speed, spawn/end scale, spacing, end fade, visual fade, colour, foam intensity, rotation, and height/refraction strength. |
| Statue gold | `StatueGoldShine.shader` creates a procedural diagonal shine sweep from UV projection. The sprite alpha naturally masks the effect to the painted statue. | Colour/intensity, angle, speed, width, softness, and enable state. |
| Leaves and vines | `StylizedLeafWind.shader` uses vertex displacement on a segmented uGUI mesh. The base stays anchored while the body performs a broad, dephased sway. | Wind direction, body-bend strength, speed, broad-wave shape, and anchor falloff. |
| Clouds and rain | A cloud Animator is driven by a speed parameter. Rain controls particle emission and progressively darkens the selected cloud layer. | Cloud speed and rain amount. |

## Water and Ripple Design

The pool is the most layered effect in the scene. Its textures are intentionally packed to keep the material manageable:

- `TX_CausticsAndWaves_Mask`: `R/G` are two caustic states that blend over time; `B/A` are two wave states.
- `TX_WaterBorderAndNoises_Mask`: `R` defines the water perimeter, `G` places edge foam, and `B/A` provide foam-distortion noise.
- `TX_Scene_Reflection`: a supplied, UV-aligned reflection image that is vertically flipped and faded into the water.
- `TX_Ripples_Mask`: `B` is the expanding ring shape, `G` is the stationary outer-edge fade, and `R` contains additional foam detail.

The ripple system was initially prototyped as a separate transparent UI Image, then consolidated into the pool-water shader. This gives the ripple one source of truth: the visible foam/detail, faux height lighting, and small refraction offset all share the same continuous ring timeline. The final implementation remains local to the water material; it does not distort unrelated Canvas elements behind the water.

Caustics support separate X/Y perspective scaling so the pattern can be flattened to match the painted pool. A centred contrast control raises highlights and suppresses troughs without changing the mask's midpoint.

## Control Architecture

`Assets/Scripts/StylizedSceneController.cs` is the root-level configuration surface for the scene. It caches shader property IDs, reads authored material defaults on enable, binds optional uGUI sliders, and removes listeners on disable.

The controller updates assigned shared materials directly. It does not create runtime material instances, use per-frame control polling, or allocate per frame. An Editor Play Mode Save button persists the currently tuned material values for the statue, waterfall, leaves, and water.

## Rendering and Performance Decisions

The rendering choices are designed for mobile tile-based GPUs:

- All custom effects are unlit UI passes.
- No screen grab, opaque texture, blur, or scene-colour refraction is used.
- Water refraction offsets only the water shader's own reflection, caustics, waves, and foam lookups.
- The supplied reflection image replaces an expensive live reflection.
- Packed masks reduce the number of separate texture assets needed by each effect.
- Decorative motion is created with simple UV math or vertex motion rather than particles or simulation wherever possible.
- Shader code is documented with mask conventions, non-obvious math, and the intent behind the mobile-safe choices.

## Validation Status

The scene has been statically reviewed and the effects are structured for the required `1080 x 1920` (9:16) and `1536 x 2048` (3:4) layouts. A Frame Debugger observation of roughly 14–15 draw/batch events is encouraging, but it is not a device-performance result.

Before final delivery, the remaining validation work is:

1. Review the effects visually at both target aspect ratios.
2. Profile an iOS Development Build with the Unity Profiler attached.
3. Measure the large transparent water layer, auxiliary foam/rain cameras, texture memory, and UI raycast overhead.
4. Confirm the final package imports cleanly into a new Unity `6.3.23f1` project.

## Outcome

The project demonstrates how a static 2D composition can gain focused, art-directable motion without abandoning its original visual language. The completed system combines reusable Canvas shaders, packed texture workflows, a centralized control surface, and deliberately constrained rendering techniques suited to a stylized mobile presentation.
