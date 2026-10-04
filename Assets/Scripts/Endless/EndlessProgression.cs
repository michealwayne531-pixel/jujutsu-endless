using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace Reconstructed.Endless
{
    [Serializable] public struct ChapterDefinition
    {
        public long chapterNumber;
        public string chapterName;
        public long totalLevels;
        public float difficultyScale;
    }

    [Serializable] public struct LevelDefinition
    {
        public long chapterNumber;
        public long levelNumber;
        public string logicalId;
        public float difficulty;
        public ulong seed;
        public float enemyHealthMultiplier;
        public float enemySpeedMultiplier;
        public float rewardMultiplier;
    }

    /// <summary>Deterministic, on-demand endless chapter generator. Reconstructed/new code.</summary>
    public static class EndlessProgression
    {
        public const long FirstDynamicChapter = 6;
        public const long MaxPracticalChapter = long.MaxValue;
        private const double MaxFiniteMultiplier = 1.0e12;

        public static ChapterDefinition GenerateChapter(long chapter)
        {
            if (chapter < 1) throw new ArgumentOutOfRangeException(nameof(chapter));
            long levels;
            if (chapter == 1) levels = 15; else if (chapter == 2) levels = 25; else if (chapter == 3) levels = 30; else if (chapter == 4) levels = 35; else if (chapter == 5) levels = 40; else levels = checked(40 + checked((chapter - 5) * 5));
            return new ChapterDefinition { chapterNumber = chapter, chapterName = $"Chapter {chapter}", totalLevels = levels, difficultyScale = (float)DifficultyScale(chapter) };
        }

        public static LevelDefinition GenerateLevel(long chapter, long level)
        {
            var c = GenerateChapter(chapter);
            if (level < 1 || level > c.totalLevels) throw new ArgumentOutOfRangeException(nameof(level));
            float position = c.totalLevels <= 1 ? 0f : (float)(level - 1) / (float)(c.totalLevels - 1);
            double chapterScale = DifficultyScale(chapter);
            return new LevelDefinition {
                chapterNumber = chapter, levelNumber = level,
                logicalId = MakeLogicalId(chapter, level), seed = DeterministicSeed(chapter, level),
                difficulty = SafeFloat(chapterScale * (1.0 + 0.25 * position)),
                enemyHealthMultiplier = SafeFloat(HealthMultiplier(chapter)),
                enemySpeedMultiplier = SafeFloat(SpeedMultiplier(chapter)),
                rewardMultiplier = SafeFloat(RewardMultiplier(chapter))
            };
        }

        // A collision-free logical pair identity; chapter and level remain 64-bit fields.
        public static string MakeLogicalId(long chapter, long level) => chapter.ToString(CultureInfo.InvariantCulture) + ":" + level.ToString(CultureInfo.InvariantCulture);
        public static ulong DeterministicSeed(long chapter, long level)
        {
            unchecked { ulong x = (ulong)chapter * 0x9E3779B97F4A7C15UL ^ (ulong)level * 0xBF58476D1CE4E5B9UL; x ^= x >> 30; x *= 0xBF58476D1CE4E5B9UL; x ^= x >> 27; return x ^ (x >> 31); }
        }
        public static double DifficultyScale(long c) => 1.0 + Math.Min(MaxFiniteMultiplier, c * (c <= 5 ? .05 : c <= 20 ? .07 : c <= 50 ? .10 : c <= 100 ? .15 : .20));
        public static double HealthMultiplier(long c) => SafeDouble(1.0 + c * (c <= 5 ? .05 : c <= 20 ? .07 : c <= 50 ? .10 : c <= 100 ? .15 : .20));
        public static double SpeedMultiplier(long c) => SafeDouble(1.0 + c * (c <= 5 ? .02 : c <= 20 ? .03 : c <= 50 ? .05 : c <= 100 ? .07 : .10));
        public static double RewardMultiplier(long c) => SafeDouble(1.0 + c * (c <= 5 ? .10 : c <= 20 ? .15 : c <= 50 ? .20 : c <= 100 ? .25 : .30));
        private static double SafeDouble(double value) => !(double.IsNaN(value) || double.IsInfinity(value)) ? Math.Min(MaxFiniteMultiplier, value) : MaxFiniteMultiplier;
        private static float SafeFloat(double value) => (float)Math.Min(float.MaxValue, SafeDouble(value));
    }

    [Serializable] public sealed class ProgressionSaveData
    {
        public long currentChapter = 1, currentLevel = 1, highestUnlockedChapter = 1, highestUnlockedLevel = 1;
        public List<string> completedLevelIds = new List<string>();
        public long currency;
        public long experience;
        public float playerHealth = 100f;
        public int version = 2;
    }

    public sealed class EndlessSaveSystem : MonoBehaviour
    {
        public ProgressionSaveData Data { get; private set; } = new ProgressionSaveData();
        private const string SaveKey = "reconstructed_endless_progress_v1";
        public void Load()
        {
            var json = PlayerPrefs.GetString(SaveKey, "");
            if (!string.IsNullOrEmpty(json)) Data = JsonUtility.FromJson<ProgressionSaveData>(json) ?? new ProgressionSaveData();
            else MigrateLegacyProgress();
            if (Data.version < 2) Data.version = 2;
        }
        private void MigrateLegacyProgress()
        {
            // Recovered strings expose these legacy PlayerPrefs concepts. Only read them when the new save is absent.
            long legacyLevel = PlayerPrefs.GetInt("lastLevelReached", 0);
            if (legacyLevel > 0) Data.currentLevel = Math.Max(1, legacyLevel);
            long legacyId = PlayerPrefs.GetInt("CurrentLevelId", 0);
            if (legacyId > 0) Data.currentLevel = Math.Max(Data.currentLevel, legacyId);
            Data.currency = Math.Max(0, PlayerPrefs.GetInt("Coins", 0));
        }
        public void Save() { PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(Data)); PlayerPrefs.Save(); }
        public void CompleteCurrentLevel()
        {
            string key = EndlessProgression.MakeLogicalId(Data.currentChapter, Data.currentLevel);
            if (!Data.completedLevelIds.Contains(key)) Data.completedLevelIds.Add(key);
            var level = EndlessProgression.GenerateLevel(Data.currentChapter, Data.currentLevel);
            long reward = SafeReward(100.0 * level.rewardMultiplier);
            Data.currency = SafeAdd(Data.currency, reward);
            Data.experience = SafeAdd(Data.experience, reward * 10L);
            Advance(); Save();
        }
        private static long SafeReward(double value) { if (double.IsNaN(value) || double.IsInfinity(value)) return long.MaxValue; return value >= long.MaxValue ? long.MaxValue : Math.Max(0L, (long)Math.Round(value)); }
        private static long SafeAdd(long a, long b) { if (b > 0 && a > long.MaxValue - b) return long.MaxValue; if (b < 0 && a < long.MinValue - b) return long.MinValue; return a + b; }
        private void Advance() { var c=EndlessProgression.GenerateChapter(Data.currentChapter); if (++Data.currentLevel > c.totalLevels) { Data.currentChapter++; Data.currentLevel=1; } Data.highestUnlockedChapter=Math.Max(Data.highestUnlockedChapter,Data.currentChapter); Data.highestUnlockedLevel=Math.Max(Data.highestUnlockedLevel,Data.currentLevel); }
    }
}
