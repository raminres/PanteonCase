# Panteon Case Collaboration Instructions

## Purpose

This repository is a Unity technical-art case study. Preserve the supplied stylized jungle-pool composition while building a performant, reusable, Canvas-based animated scene for mobile URP.

## Collaboration model

- The user performs the manual art and Unity Editor work.
- Before proposing code or editor changes, inspect the current project state and state the smallest safe next action.
- Do not overwrite, delete, regenerate, or rearrange user-created assets without explicit approval.
- Treat `Assets/Sprites/TechnicalArtStageAsset.png` as immutable source art. New work must use exported layers, masks, materials, shaders, scripts, prefabs, and documentation.
- Keep unrelated worktree changes intact. The project may be actively edited in Unity while collaboration is underway.

## Technical constraints

- Unity 6.3.23f1, URP 17.3.0, uGUI Canvas scene.
- The completed effect system must be one reusable prefab controlled from its root.
- Support 1080 x 1920 (9:16) and 1536 x 2048 (3:4) without breaking the composition.
- Custom visual shaders use HLSL and must have comments for intent, assumptions, and non-obvious math.
- Optimize for mobile tile-based renderers: unlit UI work, few texture samples, low alpha overdraw, no avoidable material variants, no expensive screen grabs or blur.
- Effects must remain calm and stylized. Do not use strong distortion, noisy movement, or effects that reshape the composition.

## Required effects

1. Waterfall and pool water: flow, restrained stylized reflection/refraction, side foam/ripple treatment.
2. Sky/clouds: slow, calm movement.
3. Selected leaves/vines: subtle wind motion with convincing anchors.
4. Statue gold: a masked, configurable shine sweep aligned to the metallic area.

## Architecture target

- `Assets/Prefabs/StylizedJungleScene.prefab` - one scene unit.
- `Assets/Scripts/StylizedSceneController.cs` - one root-level configuration surface and effect-time coordinator.
- `Assets/Shaders/UI/` - custom UI HLSL shaders, narrowly scoped by effect.
- `Assets/Materials/` - materials assigned explicitly to prefab layers.
- `Assets/Sprites/Layers/` and `Assets/Sprites/Masks/` - transparent render layers and utility masks. Keep the original image outside these generated asset folders.
- `Docs/` or root Markdown files - technical decisions, profiling notes, and delivery checklist.

Create only the folders/assets that are useful at the current milestone; do not add placeholder scripts or shaders merely to satisfy this structure.

## Current project checkpoint (2026-10-15)

- The authored scene is `Assets/Scenes/PanteonCaseScene.unity`; retain current unsaved/user work and do not restore the deleted `SampleScene` without explicit instruction.
- Statue shine, cloud-speed UI binding, rain/cloud darkening, waterfall flow/foam, anchored leaf sway, and pool-water controls are implemented.
- `StylizedSceneController` controls all four shared effect materials and its Editor Play Mode Save button persists their current values. Pool water uses two packed masks plus a supplied reflection image; it is not a camera grab.
- A static performance audit found likely GPU/memory/input candidates, but no device-profiled performance result exists. The next task is art/device validation, not adding controls by default.
- Read `PROJECT_HANDOFF.md` before continuing work. It contains the active asset bindings, RenderTexture state, performance findings, and the remaining validation work.

## Quality gates

- A layer rebuild must visually match the supplied source before animation is added.
- Each effect has an enable switch and an obvious fallback to the static art.
- Use named serialized fields, tooltips, and sensible ranges for artist-facing parameters.
- No per-frame allocations, repeated `GetComponent` calls, or hidden runtime material proliferation.
- Validate at both required aspect ratios and record Frame Debugger/Stats observations before calling the work complete.
- Never claim mobile performance is acceptable without observing the actual profiler/Frame Debugger evidence.

## Delivery discipline

- Keep the case brief and private working notes out of the final package unless explicitly requested.
- Maintain a short technical README covering layers, masks, effects, controller parameters, performance decisions, and exact Unity version.
- Final package must be imported into a clean Unity 6.3.23f1 project before delivery.
