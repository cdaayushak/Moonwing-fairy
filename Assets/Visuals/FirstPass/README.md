# Moonfairy — complete first presentation pass

Open `Assets/Scenes/MoonwingForest.unity` and enter Play mode. The Caretaker's narration
uses four short segments over 26 seconds, with a SKIP button that immediately reveals the title.
PLAY releases the existing
gameplay and fades in the Moonlight meter. No generation command is required.

## What changed

- The approved clearing is retained. Southern glades, borders, varied glowing plants,
  mist, and fireflies now cover the existing 50 × 50 ground footprint. Open central and
  connecting routes are preserved. The existing distant backdrop remains scenery.
- Mushrooms use three emission strengths; flowers have a smaller boost. Bloom is 0.6.
  Two modest, shadowless glade lights help nearby plants appear to illuminate their surroundings.
- The existing sky shader preserves the moon and adds independently phased, slowly twinkling stars.
- Player, Caretaker, and ForestSpirit have visual prefab children. Their old capsule renderers
  are shadow-only proxies; root colliders, gameplay components, and references stay in place.
- Assassin_1 retains its root, collider, AI and health, with a hooded visual child and restrained
  shadow particles. The separate shadow-reaction component briefly accents a confirmed hit.
- Arrow retains its Rigidbody, collider, scale, speed and damage. A compensating child transform
  gives the new arrow sensible proportions without changing the scaled physics root.
- The released arrow ignites over 0.16 seconds, enveloping its physical silhouette in Moonlight energy.
  Its physical shaft, arrowhead and feathers remain visible inside the enchantment.
  Its detached world-space wake continues fading after destruction. A confirmed enemy collision
  creates a 22-particle Moonlight impact burst. None of these effects applies damage.
- The existing Moonlight slider has a gradient fill, draining halo, silver frame and crescent.
  Its presentation component reads the slider and gently dims/pulses at low magic; FairyMagic
  remains the only owner of its gameplay value.
- Original victory/capture panels retain their bindings, with new typography, framing and stars.
- A small open pavilion/residence and lantern garden foundation sits near (-15, 0, -14).
  It has no collision or home gameplay. Its lantern fill is the third new shadowless point light.

## In-game NPC dialogue

`MoonwingDialogueBubble` shows the existing NPCConversation and CaretakerInteraction lines
above the speaking NPC, with moonlit text, a translucent lavender-dark panel and a small pointer.
It creates one reusable, non-interactive canvas at runtime; no scene setup is needed. Lines fade
in over 0.25 seconds and out over 0.55 seconds, with 3.2–4.5 seconds total reading time.
Concurrent lines are presented sequentially (up to four waiting lines; stale lines expire after
12 seconds). The display follows wandering speakers, fades with distance, and hides behind the
camera or while gameplay is paused. Original random choices, lines, distance/cooldown checks,
wandering and Debug.Log output are unchanged. The two existing dialogue scripts only forward
their selected text to this presentation component.

## Aiming correction

`Assets/Scripts/PlayerShoot.cs` now projects the mouse ray onto a horizontal plane at the player's
arrow/torso height. It supports continuous 360-degree aiming independent of movement. The player
smoothly faces that direction, while arrows use the exact direction immediately. Space or left
click fires. UI clicks, paused gameplay and pointers outside the Game view do not fire shots.
The aim updates after CameraFollow to avoid a one-frame cursor offset while moving. A small
dead zone at the player avoids unstable direction changes. Speed, spawn distance, lifetime,
Rigidbody and damage are unchanged.

`PlayerMovement` keeps the existing normal speed and normalized WASD movement. Left Shift applies
a 1.65x multiplier only while held; releasing it immediately restores normal speed. Movement no
longer overrides the mouse-controlled facing. No jumping or stamina system was added.

## Second refinement pass

- Moonfairy's visual prefab now has a narrower pearl/ivory layered dress, rose-gold/silver trim,
  long chocolate-brown hair, smaller facial proportions, a visible curved bow and fine string.
- Reduced wings use a separate Gossamer shader: translucent centers fade to transparent edges,
  with thin internal veins, view-dependent color variation and slow shimmer. No bright outline.
  Flutter and pixie aura are restrained; the Forest Spirit's existing veil material is unchanged.
- Arrow geometry explicitly retains its shaft, pointed head and three feathers after ignition.
  The energy sheath, detached trail, impact particles and assassin hit response remain in place.
- Assassins have layered charcoal robes, lamellar-style armor, a smaller cowl, face covering and
  a dark blade silhouette. AI, three-hit health and spawn behavior are unchanged.
- The intro uses a portrait rendered from the existing Caretaker model, four fading lore segments,
  a framed translucent panel and SKIP. Both Skip and natural completion hold gameplay until PLAY.
- The meter, title and ending screens share smaller moon motifs, restrained ornament, consistent
  spacing and typography. Existing ending text and Slider bindings remain intact.
- The existing pavilion retains its footprint and curved roof, with pale timber/stone, climbing
  flowering vines, a tea table/cushions, small water basin, glowing garden beds and stepping stones.
  These are visual children without colliders. Sparse distant trees fill gaps in the forest edge.
- Mushroom brightness varies from quiet clusters to a few stronger highlights. Botanical shaders
  now receive the existing local lantern/glade lights; no additional lights or packages were added.

Authoring files: `Editor/MoonwingRefinement.cs` and `Editor/MoonwingRefinementUIWorld.cs`.
Do not rerun the builder over the saved scene; edit its assets and scene children normally.
The only pre-existing gameplay code intentionally changed by this refinement is PlayerMovement
and PlayerShoot. Opening and ArrowMagic are presentation components from the first pass.

## Scene organization and tuning

- `Moonlit Clearing - Visual Study`: established forest assets and atmosphere.
- `Moonwing - Full Visual Pass`: additional forest, bioluminescent glade lights, and residence.
- Character roots: their new visual-prefab children, with shared materials in this folder.
- `Canvas/Moonfairy Opening`: narration duration, PLAY button, and gameplay gate bindings.
- Existing `MoonlightBar`: frame, fill, crescent and `MoonwingMeterGlow`.
- `Prefabs/Detached Moonlight Wake`: trail duration and maximum dust count (90).
- `Prefabs/Moonlight Impact Burst`: one short 22-particle burst, maximum 28 particles.
- `Materials/Moon Goddess Light`: shared emissive character/arrow magic.
- `Materials/Quiet Lavender Mushroom Caps` and `Moonwell Mushroom Caps`: emission variation.
- `MoonlitClearing/Settings/Moonwing Clearing Atmosphere.asset`: bloom and color grading.

The opening saves/restores each referenced behaviour's enabled state, including components that
were deliberately disabled. It pauses time until PLAY and uses unscaled time for narration/UI.
Ending panels are hidden during the opening. Unscaled particle lifetimes and wake cleanup allow
effects to finish even after the existing victory/defeat time freeze.

## Validation and practical limits

Unity compilation and asset/reference checks passed in an isolated copy. Play mode smoke checks
covered pre-PLAY gating, all four narration segments, natural title transition, SKIP, PLAY,
normalized movement and sprint/release through simulated input, nine projected mouse aim angles
(including 17, 133 and 293 degrees), actual projectile direction/speed, both NPCs wandering,
NPC conversations/bubbles, camera follow, assassin spawn/chase, magic drain, meter synchronization, three real
projectile collisions (health 3 → 2 → 1 → defeat), impact creation, residual wake cleanup, victory
and capture, including the shadow aura's reaction to a physical hit. NPC colliders were temporarily
disabled only inside the controlled collision test after their behaviour checks; the saved scene
retains them unchanged. Physical keyboard/mouse interaction and aiming feel still need a hands-on
Game view check; automation exercised the same movement and aim calculations with supplied inputs.

Updated previews and check output are in the ignored `Logs/SecondPassReview` folder. Camera portraits and
editor previews are illustrative; use the normal Game view to judge gameplay readability.

Meshes and materials are reused, and particles have per-system caps. This is a broad foundation,
not final character art or a rigged animation set. The scene has approximately 1.1 million mesh
triangles in the first pass (approximately 1.3 million after the refinement) across all instances
before culling. No Web build/frame-rate claim is made: profile on
the target browser/device and consider tree LODs or reduced density before release. Existing
movement and wandering remain unbounded; scenery adds no obstacle avoidance or movement limits.

`totalAssassins` remains **1**. Build Settings still contains the pre-existing stale SampleScene
entry; select MoonwingForest when preparing a build. No packages, paid assets, or external art
were added. Authoring code is editor-only and refuses to rebuild over an existing full pass.
