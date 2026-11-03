# Validation report

Prepared on 2026-11-03.

| Check | Result |
|---|---|
| Independent project/layout validation | 9 tests passed, 0 failed |
| C# syntax parsing with tree-sitter-c-sharp | 14 files parsed without syntax errors |
| Original supplied asset package preservation | 1,012 asset/metadata files matched byte for byte across 528 package paths |
| New catalog and scene GUID references | All resolve to included metadata |
| Original SWQS-BMY repository | Left unchanged |
| Unity C# compilation / asset import | Not run: Unity Editor is not installed |
| Unity EditMode tests | 7 cases supplied; not run |
| Unity PlayMode tests | 4 cases supplied; not run |
| Interactive gameplay / audio / target build | Not run; requires Unity and target build module |

The result is a Unity source project, not a verified executable. There is no EXE in this archive. The structural jump check is a simple ballistic estimate and does not establish that all encounters or collisions work correctly in Unity. Complete the checks in TESTING.md before submission.

The project uses no external graphical assets. Original package identity and verification counts are in ASSET_INTEGRITY.json. The collection sound is synthesized in code; background music is from the supplied package. No asset license was inferred or added.

Follow-up correction: enabled the built-in Animation module required by the supplied demo Animators. Target editor updated to 2022.3.62f3 after the Hub security alert for f1. The user screenshot shows Demo_characters running rather than the Start scene; gameplay is still not validated.
