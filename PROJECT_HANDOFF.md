# Project Handoff — 2026-10-15

## Purpose

This is the working continuation note for the next session. It records the implemented state observed in the project, the latest performance review, and the safe next actions. It is not final delivery documentation.

## Environment and safety

- Unity `6000.3.23f1`, URP `17.3.0`; authored scene: `Assets/Scenes/PanteonCaseScene.unity`.
- The scene uses a Screen Space Overlay uGUI Canvas. Its art is not rendered by the scene cameras.
- The project is intentionally dirty. Preserve the user's PSD, scene, material, and unsaved Editor work. Do not restore the deleted `SampleScene` or save/edit scene or prefab YAML outside Unity.
- `Assets/Sprites/TechnicalArtStageAsset.png` remains immutable source art. The layered PSD is the editable source for the sprite layers used by the composition.

## Implemented effects and controls

`Assets/Scripts/StylizedSceneController.cs` is the one root-level controller. It binds controls in `OnEnable`, unbinds in `OnDisable`, caches shader IDs, and does not create runtime material instances or use per-frame callbacks.

| Effect | Current implementation / managed material |
| --- | --- |
| Statue gold | `StatueGoldShine.shader` / `MT_CatStatueShine.mat`. Controls: angle, speed, width, softness, colour/intensity, and additive-shine enable. |
| Clouds and rain | Cloud Animator `Speed` is controlled by the UI. Rain Amount maps to `RainParticle` emission (`0–30`) and darkens `Cloud1` value to 70% at maximum rain. |
| Waterfall | `StylizedWaterfall.shader` / `MT_Waterfall.mat`, using `TX_StylizedStreaks_Mask` (`R` waves, `G` foam, `B` pulse, `A` silhouette). Controls include width, wave speed, pulse strength, opacity, and foam amount. The linked normalized width also changes CatStatue width (333 px at 1) and the foam particle box width (8 at 1); waterfall maximum width is 512 px. |
| Leaf wind | `StylizedLeafWind.shader` / `MT_SwayingLeaf.mat`. Vertex-based anchored sway uses a broad wave, avoiding visibly travelling small noise. Controls: body bend strength, wind speed, and base-anchor falloff. The Canvas must retain `TexCoord1` for this shader. |
| Pool water | `StylizedPoolWater.shader` / `MT_Water.mat`. It provides vertical two-colour grading, caustics, waves, edge foam, animated foam distortion, and a vertically flipped supplied reflection texture. |

The Save button calls `SaveControlledMaterialSettings()` in Editor Play Mode. It now persists all four controller-managed materials: statue, waterfall, leaf wind, and pool water. It is not a player save system.

## Active pool-water assets and mask convention

- `Assets/Textures/TX_CausticsAndWaves_Mask.png`: `R/G` are two caustic states that blend in time; `B/A` are two wave states that blend in time.
- `Assets/Textures/TX_WaterBorderAndNoises_Mask.png`: `R` water perimeter, `G` edge foam, `B/A` distortion-noise field.
- `Assets/Textures/TX_Scene_Reflection.png`: UV-aligned reflection art, flipped vertically in the shader and faded deeper into the water.
- Legacy `TX_CausticsWavesWaterFoam_Mask.png` remains referenced in serialized material data but is not used by the current shader. Do not remove it until the user verifies the current material and build dependencies.

Pool-water controls currently exposed on the controller/UI: opacity, reflection opacity, reflection fade depth, caustics size/intensity, waves size/intensity/move speed, foam intensity, and foam distortion speed/size.

## Latest static performance audit

The user saw roughly `14–15` in the Frame Debugger. That is a good low draw/batch indication, but it does not identify fill-rate, texture-bandwidth, CPU, or RAM limits. No actual-device performance claim has been made.

### Highest-priority checks

1. The pool-water Image covers a large transparent area and its shader currently performs about nine texture samples per shaded pixel. Profile it first; reducing an intensity to zero does not remove its texture sampling. A dedicated lite water shader/material is the right fallback if it proves GPU-bound.
2. The active composition uses 14 sprites from `TechnicalArtStageAsset.psd`, imported up to 2048 on iOS with mipmaps. PSD is not a runtime-format penalty: Unity runs the imported texture/sprite data. The risks are its final texture-page count, resolution, compression, and mip allocation. Test no mipmaps and a 1024 iOS override only after visual QA at both target aspect ratios.
3. 160 `Graphic` components currently have Raycast Target enabled. Turn it off on all decorative Images, RawImages, TMP labels, and leaves; retain it only on interactive controls.
4. The Canvas is configured to send all additional vertex streams. The leaf shader only needs `TexCoord1`; after checking other custom shaders, reduce Canvas Additional Shader Channels to `TexCoord1` only.

### Camera, RenderTexture, and URP findings

- Main Camera currently culls every layer even though the Canvas is Overlay. Foam (layer 30) and Rain (layer 31) have dedicated capture cameras. Exclude those two layers from the Main Camera, and test disabling the Main Camera if it draws no required background.
- `FoamCamera` and `RainCamera` correctly target `RTX_FoamCamera` (256×128) and `RTX_RainCamera` (128×128). Their texture-memory cost is small, but their extra rendering passes matter. For these unlit captures, test HDR, MSAA, shadows, and occlusion culling off; use no depth/stencil format if depth is unnecessary. Tag both auxiliary cameras `Untagged` so only the actual game camera is `MainCamera`.
- The Mobile URP asset already has render scale `0.8`, depth texture off, opaque texture off, SRP Batcher on, and native render pass on. HDR and unused lighting/shadow features remain enabled; if no lit 3D is planned, test disabling them. Post-processing is off on all cameras, so the Bloom/Vignette profile is not currently a runtime cost.
- Data masks correctly use sRGB off. Add explicit iOS ASTC import overrides after visual QA; start with ASTC 6×6 for masks/reflection. Disable mips for fixed-scale Canvas textures where quality permits.

### Known good baseline

- Particle systems are capped modestly and have no collision, trails, noise, or lights.
- There are no custom `Update`, `LateUpdate`, or `FixedUpdate` methods in `Assets/Scripts`.
- The canvas uses `m_CullTransparentMesh`, and the custom effects remain unlit UI shaders.

## Next-session validation order

1. Make an iOS Development Build with the Profiler attached.
2. Record four short captures: baseline; pool-water Image disabled; foam/rain capture cameras disabled; control panel hidden with decorative raycasts disabled.
3. Compare GPU ms, Main Thread, Render Thread, Texture Memory, and RenderTexture Memory. In Frame Debugger, verify that Main Camera does not also render Foam/Rain.
4. Only then decide whether the first cleanup is a lite water shader, texture import reduction, or UI raycast/auxiliary-camera cleanup.
5. Validate visuals at 1080×1920 and 1536×2048 after every accepted import or Canvas-channel change.

## Remaining delivery work

- Artist review and final acceptance/static fallback/enable switches for the effects.
- Create `Assets/Prefabs/StylizedJungleScene.prefab` when the scene is stable.
- Device profiling and Frame Debugger/Stats records.
- Aspect-ratio validation, a short technical README/performance record, clean-project import, and package smoke test.
