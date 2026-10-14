# Panteon Technical-Art Case

An in-progress Unity 6.3.23f1 / URP 17.3.0 technical-art scene built with a Screen Space Overlay uGUI Canvas. The goal is a calm, mobile-oriented stylized jungle pool with independently controllable effects.

## Current checkpoint

- The active authored scene is `Assets/Scenes/PanteonCaseScene.unity`.
- Gold statue shine, cloud-speed UI control, and rain amount / cloud darkening are implemented.
- A first pool-water shader and material exist, but the user has explicitly marked the visual result as unsatisfactory. Treat it as an exploratory pass, not a finished effect.
- No performance claim, device profiling result, aspect-ratio validation record, reusable prefab, or clean-package smoke test exists yet.

## Key runtime assets

- `Assets/Scripts/StylizedSceneController.cs` — root UI/effect controller.
- `Assets/Shaders/UI/StatueGoldShine.shader` and `Assets/Materials/MT_CatStatueShine.mat` — statue shine.
- `Assets/Shaders/UI/StylizedPoolWater.shader` and `Assets/Materials/MT_Water.mat` — exploratory pool water.
- `Assets/Textures/TX_CausticsWavesWaterFoam_Mask.png` — packed pool mask: R caustics, G waves/height, B water coverage, A foam.

Read [PROJECT_HANDOFF.md](PROJECT_HANDOFF.md) before continuing water work. It records the active implementation, current limits, known mobile constraints, and the recommended next session.

## Documentation

- [PROJECT_HANDOFF.md](PROJECT_HANDOFF.md) — current technical handoff and next steps.
- [IMPLEMENTATION_PLAN.md](IMPLEMENTATION_PLAN.md) — remaining milestone plan.
- [SKILLS.md](SKILLS.md) — project-specific technical working practices.
- [GEMINI.md](GEMINI.md) — collaboration operating instructions.
