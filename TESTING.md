# Verification and acceptance

## Checks performed in the creation environment

See `VALIDATION_REPORT.md` for the final recorded results. Python validation covers project references, level data, basic jump-distance bounds, patrol boundaries and source import settings. C# syntax parsing is **not** Unity compilation. The jump model does not simulate collision resolution, input timing or actual enemy encounters.

## Required Unity verification (not run here)

- Import with Unity 2022.3.62f1 and confirm no C# compiler errors.
- Run EditMode tests (7 cases) and PlayMode tests (4 cases).
- Start from `Start.unity`; verify mouse and keyboard menu navigation.
- Move both ways; confirm fox flipping, idle/run, rising/falling and hurt frames.
- Test jump buffering, coyote time, short hops, landing and side collisions.
- Jump onto an enemy; confirm one score reward and bounce. Touch an enemy from the side and confirm damage plus temporary invulnerability.
- Check spikes, pit deaths, three-heart depletion and three-life game over.
- Touch a checkpoint and then fall; confirm respawn there and preserved collected items.
- Reach the house without every gem; confirm the prompt. Collect every gem and return; confirm completion and next-stage unlock.
- Complete all three stages and verify final completion screen.
- Confirm pause freezes movement and timer; resume, restart and menu all work.
- Change volume, restart the application and confirm volume/progress persist.
- Verify the minimum 1280x720 layout, 1920x1080 and a 4:3 window; look for text clipping and camera boundaries.
- Build Windows x64 with the matching module; run the EXE independently of Unity and repeat one complete stage.

Do not describe this project as a tested executable until the import, gameplay checks and target build succeed.

## Scope

Desktop keyboard/mouse, single player, three stages. No touch controls, multiplayer, cloud saves or mobile build has been implemented. Restarting a stage resets its current score and collectibles; saved best results remain. A new session preserves unlocks/best results rather than the exact in-progress position.
