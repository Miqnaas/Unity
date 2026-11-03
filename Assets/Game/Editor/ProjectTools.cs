using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Sunnyland.Editor
{
    public static class ProjectTools
    {
        const string StartScene = "Assets/Game/Scenes/Start.unity";
        [MenuItem("Sunnyland/Open Start Scene")]
        public static void OpenStart()
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                EditorSceneManager.OpenScene(StartScene);
        }
        [MenuItem("Sunnyland/Validate Project")]
        public static void Validate()
        {
            var art = Resources.Load<ArtCatalog>("SunnylandCatalog");
            if (!art || !art.tileset || !art.music || art.idle.Length != 4 || art.run.Length != 6)
                throw new System.InvalidOperationException("Incomplete Sunnyland art catalog.");
            for (int i = 1; i <= 3; i++)
            {
                var data = GameRoot.ReadLevel(i);
                if (data.gems.Length < data.requiredGems || data.ground.Length == 0)
                    throw new System.InvalidOperationException("Invalid level " + i);
            }
            Debug.Log("Sunnyland assets and three level definitions verified. Run Test Runner and Play Mode for gameplay validation.");
        }
        [MenuItem("Sunnyland/Build Windows x64")]
        public static void BuildWindows() { Build(BuildTarget.StandaloneWindows64, "Builds/Windows/SunnylandAdventure.exe"); }
        [MenuItem("Sunnyland/Build Linux x64")]
        public static void BuildLinux() { Build(BuildTarget.StandaloneLinux64, "Builds/Linux/SunnylandAdventure"); }
        static void Build(BuildTarget target, string location)
        {
            Validate();
            Directory.CreateDirectory(Path.GetDirectoryName(location));
            var options = new BuildPlayerOptions { scenes = new[] { StartScene }, locationPathName = location, target = target, options = BuildOptions.None };
            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
                throw new System.InvalidOperationException("Build failed. Install the platform module and inspect the Console: " + report.summary.result);
            Debug.Log("Build succeeded: " + location);
        }
        [MenuItem("Sunnyland/Reset Saved Progress (local only)")]
        public static void ResetProgress()
        {
            if (!EditorUtility.DisplayDialog("Reset Sunnyland progress?", "This clears only Sunnyland unlocks, scores and times on this computer.", "Reset", "Cancel")) return;
            PlayerPrefs.DeleteKey("sunny.unlocked");
            for (int i = 1; i <= 3; i++)
                foreach (string suffix in new[] { ".stars", ".score", ".time" })
                    PlayerPrefs.DeleteKey("sunny.level." + i + suffix);
            PlayerPrefs.Save();
        }
    }
}
