using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.Tilemaps;

namespace Sunnyland.Tests
{
    public sealed class GameplayTests
    {
        GameObject root;
        GameRoot game;
        readonly Dictionary<string, int?> savedInts = new Dictionary<string, int?>();
        readonly Dictionary<string, float?> savedFloats = new Dictionary<string, float?>();
        [UnitySetUp] public IEnumerator SetUp()
        {
            savedInts.Clear(); savedFloats.Clear();
            savedInts["sunny.unlocked"] = PlayerPrefs.HasKey("sunny.unlocked") ? (int?)PlayerPrefs.GetInt("sunny.unlocked") : null;
            for (int i = 1; i <= 3; i++)
            {
                string prefix = "sunny.level." + i;
                foreach (string suffix in new[] { ".stars", ".score" })
                    savedInts[prefix + suffix] = PlayerPrefs.HasKey(prefix + suffix) ? (int?)PlayerPrefs.GetInt(prefix + suffix) : null;
                savedFloats[prefix + ".time"] = PlayerPrefs.HasKey(prefix + ".time") ? (float?)PlayerPrefs.GetFloat(prefix + ".time") : null;
            }
            root = new GameObject("Gameplay test");
            game = root.AddComponent<GameRoot>();
            yield return null;
            game.StartLevel(1);
            yield return new WaitForFixedUpdate();
        }
        [UnityTearDown] public IEnumerator TearDown()
        {
            Object.Destroy(root); Time.timeScale = 1; yield return null; yield return null;
            foreach (var pair in savedInts)
                if (pair.Value.HasValue) PlayerPrefs.SetInt(pair.Key, pair.Value.Value); else PlayerPrefs.DeleteKey(pair.Key);
            foreach (var pair in savedFloats)
                if (pair.Value.HasValue) PlayerPrefs.SetFloat(pair.Key, pair.Value.Value); else PlayerPrefs.DeleteKey(pair.Key);
            PlayerPrefs.Save();
        }
        [UnityTest] public IEnumerator PlayerLandsOnCollidableTilemapAndCanJump()
        {
            for (int i = 0; i < 40; i++) yield return new WaitForFixedUpdate();
            Assert.IsTrue(game.Player.Grounded);
            Assert.That(game.Player.transform.position.y, Is.InRange(1.6f, 2));
            Assert.IsNotNull(Object.FindObjectOfType<TilemapCollider2D>());
            game.Player.QueueJump();
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
            Assert.Greater(game.Player.GetComponent<Rigidbody2D>().velocity.y, 8);
        }
        [UnityTest] public IEnumerator GemTriggerAwardsScoreOnce()
        {
            var point = game.Current.gems[0];
            game.Player.transform.position = point;
            Physics2D.SyncTransforms();
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
            Assert.AreEqual(1, game.Gems);
            Assert.AreEqual(250, game.Score);
        }
        [UnityTest] public IEnumerator ExitRequiresGemsAndCompletionPersistsUnlock()
        {
            int previous = PlayerPrefs.GetInt("sunny.unlocked", 1);
            game.TryFinish();
            Assert.IsTrue(game.IsPlaying);
            for (int i = 0; i < game.Current.requiredGems; i++) game.Collect(true);
            game.TryFinish();
            Assert.AreEqual(GameScreen.Complete, game.ScreenState);
            Assert.GreaterOrEqual(game.Unlocked, 2);
            Assert.GreaterOrEqual(game.Stars, 1);
            PlayerPrefs.SetInt("sunny.unlocked", previous);
            PlayerPrefs.Save();
            yield return null;
        }
        [UnityTest] public IEnumerator PauseFreezesTimerAndFallUsesLife()
        {
            game.Pause();
            float elapsed = game.Elapsed;
            yield return new WaitForSecondsRealtime(.05f);
            Assert.AreEqual(elapsed, game.Elapsed);
            Assert.AreEqual(0, Time.timeScale);
            game.Resume();
            game.Fall();
            Assert.AreEqual(2, game.Lives);
            Assert.AreEqual(3, game.Player.Hearts);
            game.Fall(); game.Fall();
            Assert.AreEqual(GameScreen.GameOver, game.ScreenState);
            yield return null;
        }
    }
}
