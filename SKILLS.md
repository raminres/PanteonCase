# Panteon Case Working Playbook

This is a project workflow reference, not a list of installed tools. Use it with [PROJECT_HANDOFF.md](PROJECT_HANDOFF.md) so that assisted work remains reversible and consistent with the user’s manual art work.

## Composition and source art

- Preserve `Assets/Sprites/TechnicalArtStageAsset.png` as immutable source art.
- Use transparent, same-canvas-size exports for independently animated visual layers.
- Reassemble all exported layers at 100% opacity before adding motion.
- Keep the Canvas in Screen Space Overlay unless a feature genuinely requires another render mode. Cameras cannot hide Overlay UI.

## UI shader rules

- Keep custom effects unlit, commented HLSL and retain uGUI tint, clipping, stencil, and alpha-clip support.
- Prefer artist-authored masks and slow UV motion to camera textures, screen grabs, blur, or true scene refraction.
- Count texture samples and transparent area before calling an effect mobile-friendly. No device profiling claim without evidence.
- Keep a static visual fallback and an effect enable path for each completed effect.

## Current water-mask convention

- `TX_CausticsAndWaves_Mask`: `R/G` are interpolated caustic states; `B/A` are interpolated wave states.
- `TX_WaterBorderAndNoises_Mask`: `R` is water perimeter, `G` is edge-foam placement, `B/A` are foam-distortion noise.
- `TX_Scene_Reflection`: deliberate UV-aligned reflection artwork. The shader flips it vertically and fades it with depth; do not replace it with a camera grab.

The current water shader is a large transparent, multi-sample UI pass. Profile it before making it more complex, and provide a static/low-cost fallback before final delivery.

## Controller and UI practices

- Keep `StylizedSceneController` as the one root configuration point.
- Bind UI controls in `OnEnable` and unbind in `OnDisable`; avoid duplicated listeners.
- Cache shader property IDs and animator hashes. Do not allocate or look up components every frame.
- The Save button persists the statue, waterfall, leaf, and pool-water managed materials in Editor Play Mode. It must not be represented as a general runtime save system.

## Validation order

1. Check the static art match.
2. Review the effect visually in Play Mode on the authored scene.
3. Test 1080 x 1920 and 1536 x 2048.
4. Check Stats, Frame Debugger, and alpha-overdraw risk.
5. Test the device-specific path, especially iOS RenderTexture output for rain.
6. Record observed results in the README/handoff before calling the work complete.
