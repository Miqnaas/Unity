using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Sunnyland.Tests
{
    public sealed class ProjectIntegrityTests
    {
        [Test] public void CatalogContainsOriginalArtwork()
        {
            var art = Resources.Load<ArtCatalog>("SunnylandCatalog");
            Assert.IsNotNull(art);
            Assert.IsNotNull(art.tileset);
            Assert.IsNotNull(art.music);
            foreach (var sprite in art.idle) Assert.IsNotNull(sprite);
            foreach (var sprite in art.run) Assert.IsNotNull(sprite);
            Assert.AreEqual(4, art.idle.Length);
            Assert.AreEqual(6, art.run.Length);
            Assert.AreEqual(2, art.jump.Length);
            foreach (var sequence in new[] { art.hurt, art.opossum, art.frog, art.eagle, art.cherry, art.gem })
            { Assert.IsNotEmpty(sequence); foreach (var sprite in sequence) Assert.IsNotNull(sprite); }
        }
        [TestCase(1)] [TestCase(2)] [TestCase(3)]
        public void LevelHasValidBoundsAndRequiredGems(int index)
        {
            var data = GameRoot.ReadLevel(index);
            Assert.AreEqual(index + 2, data.requiredGems);
            Assert.AreEqual(data.requiredGems, data.gems.Length);
            Assert.Greater(data.width, data.exit.x);
            Assert.Greater(data.parTime, 0);
            foreach (var span in data.ground)
            { Assert.Greater(span.width, 0); Assert.Greater(span.depth, 0); Assert.LessOrEqual(span.x + span.width, data.width); }
            foreach (var enemy in data.enemies)
            { Assert.Less(enemy.left, enemy.right); Assert.That(enemy.x, Is.InRange(enemy.left, enemy.right)); }
        }
        [Test] public void BuildStartsAtMenu()
        {
            Assert.AreEqual("Assets/Game/Scenes/Start.unity", EditorBuildSettings.scenes[0].path);
            Assert.IsTrue(EditorBuildSettings.scenes[0].enabled);
        }
        [Test] public void StageDifficultyIncreases()
        {
            Assert.Less(GameRoot.ReadLevel(1).width, GameRoot.ReadLevel(2).width);
            Assert.Less(GameRoot.ReadLevel(2).width, GameRoot.ReadLevel(3).width);
        }
        [Test] public void TimerFormatsMinutesAndSeconds()
        { Assert.AreEqual("02:05", GameHUD.TimeText(125.9f)); }
    }
}
