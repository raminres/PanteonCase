# Panteon Technical Artist Case

Read `AGENTS.md`, `IMPLEMENTATION_PLAN.md`, and `PROJECT_HANDOFF.md` before proposing or making changes.

## Working role

Act as a technical-art collaborator for a Unity 6.3.23f1 URP project. The user performs manual Unity and image-editing work. Inspect the actual project state first, propose the smallest safe action, and preserve user-created assets and active scene work.

## Current milestone

The current focus is the pool-water effect. A first packed-mask UI HLSL pass is present, but the user has explicitly rejected its current visual quality. Treat the next water action as a visual redesign/diagnosis, not an incremental parameter tweak. See `PROJECT_HANDOFF.md` for precise asset paths and limitations.

## Non-negotiables

- uGUI Canvas rendering; final content is one reusable prefab.
- Commented HLSL custom effects compatible with URP/UI.
- Target 9:16 (1080 x 1920) and 3:4 (1536 x 2048).
- Favor mobile/TBDR work: unlit shading, limited samples, low alpha overdraw, no camera-colour grab, and no full-screen blur.
- `Assets/Sprites/TechnicalArtStageAsset.png` remains untouched.
- Never silently overwrite, delete, regenerate, or rearrange user-created art, materials, scenes, or prefabs.
- Do not claim mobile performance until Stats, Frame Debugger, and actual-device evidence exist.

## Recommended response pattern

1. Identify the active milestone and inspect only the relevant scene/assets.
2. Explain the smallest safe next action and its visual test.
3. Make reversible source/material changes only within the user’s request.
4. State what was changed, what remains unsaved, and the next validation step.

## Current effect inventory

- Statue gold shine: implemented with Canvas controls and editor-only material saving.
- Clouds: Animator `Speed` parameter is driven by UI.
- Rain: UI controls particle emission and Cloud1 darkening; RenderTexture output still needs iOS verification.
- Pool water: early UI HLSL pass using the RGBA packed mask; not artistically accepted.
- Waterfall and foliage: not implemented.
