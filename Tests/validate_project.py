"""Dependency-free structural/layout checks. These do not replace Unity tests."""
from pathlib import Path
import json
import math
import re
import unittest

ROOT = Path(__file__).resolve().parents[1]
RESOURCES = ROOT / "Assets/Game/Resources"


class ProjectTests(unittest.TestCase):
    def levels(self):
        return [json.loads((RESOURCES / f"Level{i}.json").read_text()) for i in range(1, 4)]

    def test_original_demo_animators_have_builtin_module(self):
        manifest = json.loads((ROOT / "Packages/manifest.json").read_text())
        self.assertEqual("1.0.0", manifest["dependencies"]["com.unity.modules.animation"])

    def test_all_project_json_is_valid(self):
        for p in list(ROOT.rglob("*.json")) + list(ROOT.rglob("*.asmdef")):
            with self.subTest(path=p):
                json.loads(p.read_text())

    def test_new_unity_guids_are_unique_and_references_resolve(self):
        guids = {}
        for p in (ROOT / "Assets").rglob("*.meta"):
            match = re.search(r"^guid: (\w+)$", p.read_text(), re.M)
            self.assertIsNotNone(match, str(p))
            guid = match[1]
            self.assertNotIn(guid, guids, f"Duplicate GUID: {p}")
            guids[guid] = p
        for p in (ROOT / "Assets/Game").rglob("*"):
            if p.suffix not in (".asset", ".unity"):
                continue
            for guid in re.findall(r"guid: ([0-9a-f]{32})", p.read_text()):
                self.assertIn(guid, guids, f"Unresolved reference in {p}")

    def test_catalog_sprite_references_have_correct_import_settings(self):
        catalog = (RESOURCES / "SunnylandCatalog.asset").read_text()
        self.assertGreaterEqual(catalog.count("fileID: 21300000"), 35)
        for p in (ROOT / "Assets/SunnyLand Artwork/Sprites/player").rglob("*.png.meta"):
            text = p.read_text()
            self.assertIn("spritePixelsToUnits: 100", text)
            self.assertIn("spriteMode: 1", text)

    def test_required_gems_have_reachable_supporting_surfaces(self):
        # A 12-unit/s jump at gravity 9.81*3 rises 2.446 units.
        # A 2-unit step is reachable; main-floor gaps are only 3 units wide.
        jump_speed, gravity, speed = 12, 9.81 * 3, 6
        for level in self.levels():
            ground = level["ground"]
            start = next(i for i, s in enumerate(ground) if s["x"] <= level["spawn"]["x"] < s["x"] + s["width"] and s["y"] == 0)
            reached = {start}
            changed = True
            while changed:
                changed = False
                for i in list(reached):
                    a = ground[i]
                    for j, b in enumerate(ground):
                        rise = b["y"] - a["y"]
                        discriminant = jump_speed ** 2 - 2 * gravity * rise
                        if discriminant < 0:
                            continue
                        flight = (jump_speed + math.sqrt(discriminant)) / gravity
                        distance = max(0, b["x"] - (a["x"] + a["width"]), a["x"] - (b["x"] + b["width"]))
                        if distance < flight * speed - .5 and j not in reached:
                            reached.add(j)
                            changed = True
            for gem in level["gems"]:
                self.assertTrue(any(i in reached and s["x"] <= gem["x"] <= s["x"] + s["width"] and abs(gem["y"] - (s["y"] + 2)) < .01 for i, s in enumerate(ground)), (level["title"], gem))

    def test_spawns_checkpoints_and_exits_are_above_main_floor(self):
        for level in self.levels():
            for point in [level["spawn"], level["exit"], *level["checkpoints"]]:
                self.assertTrue(any(s["y"] == 0 and s["x"] <= point["x"] < s["x"] + s["width"] for s in level["ground"]))

    def test_enemy_patrols_do_not_walk_over_pits(self):
        for level in self.levels():
            for enemy in level["enemies"]:
                self.assertLess(enemy["left"], enemy["right"])
                if enemy["kind"] != "eagle":
                    self.assertTrue(any(s["y"] == 0 and s["x"] <= enemy["left"] < enemy["right"] <= s["x"] + s["width"] for s in level["ground"]))

    def test_every_required_collectible_is_in_bounds(self):
        for index, level in enumerate(self.levels(), 1):
            self.assertEqual(index + 2, level["requiredGems"])
            self.assertEqual(level["requiredGems"], len(level["gems"]))
            for point in level["gems"] + level["cherries"]:
                self.assertTrue(0 < point["x"] < level["width"])

    def test_build_scene_and_four_playable_scene_roots(self):
        self.assertIn("Assets/Game/Scenes/Start.unity", (ROOT / "ProjectSettings/EditorBuildSettings.asset").read_text())
        scenes = list((ROOT / "Assets/Game/Scenes").glob("*.unity"))
        self.assertEqual(4, len(scenes))
        for p in scenes:
            self.assertIn("m_Name: Sunnyland Game", p.read_text())


if __name__ == "__main__":
    unittest.main(verbosity=2)
