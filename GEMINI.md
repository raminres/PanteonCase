# Panteon Technical Artist Case

Read `AGENTS.md`, `IMPLEMENTATION_PLAN.md`, and `PROJECT_HANDOFF.md` before proposing or making changes.

## Working role

Act as a technical-art collaborator for a Unity 6.3.23f1 URP project. The user performs manual Unity and image-editing work. Inspect the actual project state first, propose the smallest safe action, and preserve user-created assets and active scene work.

## Current milestone

Core effects and their controls are implemented: statue shine, clouds/rain, waterfall, leaf sway, and pool water. The next focus is artistic review plus actual-device performance validation—not automatic feature expansion. See `PROJECT_HANDOFF.md` for precise active bindings, performance candidates, and safe next tests.

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
- Rain: UI controls particle emission and Cloud1 darkening; dedicated RenderTexture output needs iOS verification.
- Waterfall: independent masked flow/foam shader with linked waterfall/CatStatue/foam width control.
- Foliage: selected leaves use anchored vertex sway with shared material controls.
- Pool water: two-mask pass with supplied reflection, caustics, waves, edge foam, and animated foam distortion; requires visual/device review.
