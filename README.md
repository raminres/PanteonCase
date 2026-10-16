# Panteon Technical-Art Case

An in-progress Unity `6.3.23f1` / URP `17.3.0` stylized jungle-pool scene built with a Screen Space Overlay uGUI Canvas. It targets calm, mobile-friendly animated effects at 1080×1920 (9:16) and 1536×2048 (3:4).

## Current checkpoint — 2026-10-15

- `Assets/Scenes/PanteonCaseScene.unity` is the active authored scene. Preserve the user's dirty scene and PSD work.
- Implemented and UI-controlled: statue shine, cloud movement, rain/cloud darkening, waterfall, anchored leaf sway, and stylized pool water.
- `StylizedSceneController` is the root controller. Its Editor Play Mode Save button persists the statue, waterfall, leaf, and water material settings.
- Pool water uses separate caustics/waves, border/noise, and supplied reflection textures; waterfall and leaf wind use their own UI HLSL shaders/materials.
- The project has had a static performance audit, but no device-profiled performance claim or complete aspect-ratio validation exists yet.

## Important runtime assets

- `Assets/Scripts/StylizedSceneController.cs` — root control surface and UI bindings.
- `Assets/Shaders/UI/StylizedPoolWater.shader` / `Assets/Materials/MT_Water.mat` — water grade, masks, edge foam, distortion, and supplied reflection.
- `Assets/Shaders/UI/StylizedWaterfall.shader` / `Assets/Materials/MT_Waterfall.mat` — waterfall flow and foam.
- `Assets/Shaders/UI/StylizedLeafWind.shader` / `Assets/Materials/MT_SwayingLeaf.mat` — anchored vertex sway.
- `Assets/Textures/TX_CausticsAndWaves_Mask.png`, `TX_WaterBorderAndNoises_Mask.png`, and `TX_Scene_Reflection.png` — active water inputs.

## Current performance focus

The reported Frame Debugger value of roughly 14–15 indicates low draw/batch count, but does not establish CPU, GPU, or memory cost. Profile the large multi-sample transparent water layer, the PSD-derived texture pages/mips, two auxiliary RenderTexture cameras, and 160 enabled UI raycast targets on an actual iOS device before making an optimization claim.

Read [PROJECT_HANDOFF.md](PROJECT_HANDOFF.md) for the full current implementation state, performance findings, and exact next validation sequence.

## Documentation

- [PROJECT_HANDOFF.md](PROJECT_HANDOFF.md) — continuation state and next safe actions.
- [IMPLEMENTATION_PLAN.md](IMPLEMENTATION_PLAN.md) — remaining milestones.
- [SKILLS.md](SKILLS.md) — project technical working practices.
- [GEMINI.md](GEMINI.md) — collaboration operating instructions.
