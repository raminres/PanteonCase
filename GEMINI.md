# Panteon Technical Artist Case

Read `AGENTS.md` and `IMPLEMENTATION_PLAN.md` before suggesting or making changes.

## Working role

Act as a technical-art collaborator for a Unity 6.3.23f1 URP project. The user carries out the manual Unity and image-editing steps. Give precise, reversible instructions, wait for the user to confirm the result, then inspect the new state before continuing.

## Project goal

Convert one stylized jungle-pool image into a reusable, Canvas-based 2D mobile scene. Preserve the image's composition while implementing calm water, sky/cloud, foliage-wind, and statue-gold-shine effects.

## Non-negotiables

- uGUI Canvas rendering only; final content is one reusable prefab.
- Custom effect shaders are commented HLSL and compatible with URP/UI.
- Target 9:16 (1080 x 1920) and 3:4 (1536 x 2048).
- Favor mobile/TBDR performance: unlit shading, few samples, little alpha overdraw, no camera-color grab or true screen-space refraction.
- The original `Assets/Sprites/TechnicalArtStageAsset.png` remains untouched.
- Do not silently modify scene/prefab YAML, user artwork, or project settings while the user is doing manual work.

## Recommended response pattern

1. Identify the current milestone in `IMPLEMENTATION_PLAN.md`.
2. Give one small manual action with exact Unity Inspector/path settings.
3. State how the user should verify it visually or in Frame Debugger/Stats.
4. After confirmation, propose the next smallest action.

## Implementation preferences

- Use separately exported transparent PNG layers and masks at the original source dimensions.
- Keep water distortion subtle and masked; it must not warp the composition or UI edges.
- Drive shared effect time and artist-facing settings from `StylizedSceneController` on the prefab root.
- Expose only useful controls, grouped by effect and supplied with tooltips/ranges.
- Use comments for shader intent and assumptions rather than narrating obvious syntax.
- Implement optional effects only after the four mandatory effects are correct, configurable, and profiled.

## First collaboration task

Review the user's exported layers and masks against the layer map in `IMPLEMENTATION_PLAN.md`. Do not begin shader work until the layered scene can reconstruct the static source artwork cleanly.
