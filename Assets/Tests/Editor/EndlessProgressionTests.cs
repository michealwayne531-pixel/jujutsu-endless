using NUnit.Framework;
using Reconstructed.Endless;
public class EndlessProgressionTests
{
 [TestCase(1,15)][TestCase(2,25)][TestCase(3,30)][TestCase(4,35)][TestCase(5,40)][TestCase(6,45)][TestCase(100,515)][TestCase(9999,50010)] public void Counts(long c,long expected)=>Assert.AreEqual(expected,EndlessProgression.GenerateChapter(c).totalLevels);
 [TestCase(1,1)][TestCase(5,40)][TestCase(100,25)][TestCase(9999,35)] public void LevelIdsAndSeedsAreStable(long c,long l){var a=EndlessProgression.GenerateLevel(c,l);var b=EndlessProgression.GenerateLevel(c,l);Assert.AreEqual(a.logicalId,b.logicalId);Assert.AreEqual(a.seed,b.seed);}
 [Test] public void LargeNumbersRemainFinite(){foreach(long c in new[]{1L,5L,6L,20L,21L,50L,51L,100L,500L,1000L,5000L,9999L}){var d=EndlessProgression.GenerateLevel(c,1);Assert.IsTrue(!(float.IsNaN(d.enemyHealthMultiplier)||float.IsInfinity(d.enemyHealthMultiplier)));Assert.IsTrue(!(float.IsNaN(d.rewardMultiplier)||float.IsInfinity(d.rewardMultiplier)));}}
}
