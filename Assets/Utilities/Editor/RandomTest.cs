using System;
using UnityEngine;
using System.Collections.Generic;
using NUnit.Framework;
using PLib.Rand;
using PLib.Math;
using PLib.General;
using PLib.TestHelper;
using UnityRandom = UnityEngine.Random;

namespace RandomTest
{
    [TestFixture]
    public class RandomTest
    {
        #region Seeds

        [Test]
        public void GetSeedByHashCode_ReturnsCorrectSeed()
        {
            //  arrange
            int hashId = 50;

            //  act
            UnityRandom.State seed1 = PRand.GetSeedByHashCode(hashId);
            UnityRandom.State seed2 = PRand.GetSeedByHashCode(hashId);
            UnityRandom.State seed3 = PRand.GetSeedByHashCode(hashId + 1);

            //  assert
            Assert.AreEqual(seed1, seed2);
            Assert.AreNotEqual(seed1, seed3);
        }

        [Test]
        public void GetSeedByTransform_Works()
        {
            //  arrange
            GameObject g1 = new GameObject("test1");
            Transform t1 = g1.transform;
            GameObject g2 = new GameObject("test2");
            Transform t2 = g2.transform;

            //  act
            UnityRandom.State seed1 = PRand.GetSeedByTransform(t1);
            UnityRandom.State seed2 = PRand.GetSeedByTransform(t1);
            UnityRandom.State seed3 = PRand.GetSeedByTransform(t2);

            //  assert
            Assert.AreEqual(seed1, seed2);
            Assert.AreNotEqual(seed1, seed3);
        }

        [Test]
        public void SetSeed_Works()
        {
            //  arrange
            int seed = 1234;

            //  act
            UnityRandom.State old = UnityEngine.Random.state;
            PRand.SetSeed(seed);
            UnityRandom.State changed = UnityEngine.Random.state;

            //  assert
            Assert.IsFalse(!old.Equals(changed), "Random.State matches when it should not");
        }

        #endregion

        #region Chances

        [Test]
        public void PercentChance_IsReliable()
        {
            //  arrange
            int yes = 0;
            int no = 0;
            int runsize = 100000;
            float percent = 0.25f;

            //  act
            for (int i = 0; i < runsize; i++)
            {
                if (PRand.PercentChance(percent))
                    yes++;
                else
                    no++;
            }
            float expectedYes = percent;
            float actualYes = yes / (float)runsize;
            float expectedNo = 1 - percent;
            float actualNo = no / (float)runsize;

            //  assert
            Assert.IsTrue(PMath.Approx(expectedYes,actualYes,2), "% true is off {0} <> {1}", expectedYes, actualYes);
            Assert.IsTrue(PMath.Approx(expectedNo,actualNo,2), "% false is off {0} <> {1}", expectedNo, actualNo);
        }

        /// <summary>
        /// Ref: https://en.wikipedia.org/wiki/Poisson_distribution
        /// </summary>
        [Test]
        public void PoissonProbability_IsReliable()
        {
            //  prereq
            MathTest.MathTest math = new MathTest.MathTest();
            math.Approx_Works();
            math.Factorial_Works();

            //  arrange
            //  average number of goals scored during a world cup match
            float goalRate = 2.5f;  //  2.5 goals per game
            float[] chanceOfNGoals = new float[8];
            float[] goals = new float[] { 0.082f, 0.205f, 0.257f, 0.213f, 0.133f, 0.067f, 0.028f, 0.010f };

            for (int i = 0; i < 8; i++)
            {
                //  act
                chanceOfNGoals[i] = PRand.PoissonProbability(goalRate, i);
                //  assert
                Assert.IsTrue(PMath.Approx(goals[i], chanceOfNGoals[i],3), TestHelper.ShowVariables(goals[i], chanceOfNGoals[i]));
            }

            //  arrange
            //  100 year flood data (from wikipedia Poisson Distribution page)
            float floodRate = 1;    //  1 per 100 years

            //  act
            float[] chanceOfN = new float[7];
            float[] floods = new float[] { 0.368f, 0.368f, 0.184f, 0.061f, 0.015f, 0.003f, 0.0005f };

            for (int i = 0; i < 7; i++)
            {
                //  act
                chanceOfN[i] = PRand.PoissonProbability(floodRate, i);
                //  assert
                Assert.IsTrue(PMath.Approx(floods[i], chanceOfN[i],3), TestHelper.ShowVariables(floods[i], chanceOfN[i]));
            }
        }

        [Test]
        public void ChancePerSecond_IsReliable()
        {
            //  prereq
            PoissonProbability_IsReliable();
            MathTest.MathTest math = new MathTest.MathTest();
            math.Approx_Works();

            //  arrange
            int precision = 2;
            int runsize = 30000;    //  number of seconds
            float chance = 0.05f;   //  5%, per second
            float expectedSuccessCount = runsize * chance;
            float actualSuccessCount = 0;
            float deltaTime = Application.isPlaying ? Time.deltaTime : 1 / 60f;

            //  act
            float count = runsize / deltaTime;
            for (int i = 0; i < count; i++)
            {
                actualSuccessCount += PRand.ChancePerSec(chance) ? 1 : 0;
            }

            //  assert
            Assert.IsTrue(PMath.Approx(expectedSuccessCount/runsize, actualSuccessCount/runsize, precision), TestHelper.ShowVariables(expectedSuccessCount, actualSuccessCount));
        }

        [Test]
        public void IntChance_IsReliable()
        {
            //  arrange
            int yes = 0;
            int no = 0;
            int runsize = 1000;
            int win = 3;
            int lose = 7;
            float expectedWinRatio = (float)win / (win + lose);
            float expectedLoseRatio = (float)lose / (win + lose);

            //  act
            for (int i = 0; i < runsize; i++)
            {
                if (PRand.Chance(win, lose))
                    yes++;
                else
                    no++;
            }
            float winRatio = (float)yes / runsize;
            float loseRatio = (float)no / runsize;

            //  assert
            Assert.IsTrue(PMath.Approx(expectedWinRatio, winRatio, 1), "win: " + TestHelper.ShowVariables(expectedWinRatio, winRatio));
            Assert.IsTrue(PMath.Approx(expectedLoseRatio, loseRatio, 1), "lose: " + TestHelper.ShowVariables(expectedLoseRatio, loseRatio));
        }

        [Test]
        public void FloatChance_IsReliable()
        {
            //  arrange
            int yes = 0;
            int no = 0;
            int runsize = 1000;
            float win = 0.1f;
            float lose = 0.7f;
            float expectedWinRatio = win / (win + lose);
            float expectedLoseRatio = lose / (win + lose);

            //  act
            for (int i = 0; i < runsize; i++)
            {
                if (PRand.Chance(win, lose))
                    yes++;
                else
                    no++;
            }
            float winRatio = (float)yes / runsize;
            float loseRatio = (float)no / runsize;

            //  assert
            Assert.IsTrue(PMath.Approx(expectedWinRatio, winRatio, 1), "win: " + TestHelper.ShowVariables(expectedWinRatio, winRatio));
            Assert.IsTrue(PMath.Approx(expectedLoseRatio, loseRatio, 1), "lose: " + TestHelper.ShowVariables(expectedLoseRatio, loseRatio));
        }

        #endregion

        #region Random Values

        [Test]
        public void RandomExponential_Works()
        {
            //  Prereq
            MathTest.MathTest math = new MathTest.MathTest();
            math.Residuals_ReturnsCorrect();
            IsNormalDistribution_ReturnsCorrectly();
            
            //  arrange
            const int exponent = 2;
            const int runsize = 10000;
            float[] rolls = new float[runsize];

            //  act
            for (int i = 0; i < runsize; i++)
            {
                rolls[i] = PRand.RandomExponential(exponent);
            }

            //  calculate test data
            bool isNormal = PRand.IsNormalDistribution(rolls);

            //  assert
            Assert.IsTrue(isNormal);
        }

        [Test]
        public void RandomFromRange_Works()
        {
            //  arrange
            int samples = 1000;
            Vector2 range = new Vector2(2, 3);
            float[] values = new float[samples];

            //  act
            for (int i = 0; i < samples; i++)
            {
                values[i] = PRand.RandomFromRange(range);
            }

            //  assert
            for (int i = 0; i < samples; i++)
            {
                Assert.IsTrue(values[i] >= range.x);
                Assert.IsTrue(values[i] <= range.y);
            }
        }

        /// <summary>
        /// 2016-5-13
        /// Empirical rule: 68-95-97.7
        ///     68% of data is within -1 to +1 SD of mean
        ///     27% of data is within -1 to -2 and +1 to +2 SD of mean
        ///     2.7% of data is within -2 to -3 and +2 to +SD of mean
        /// </summary>
        [Test]
        public void IsNormalDistribution_ReturnsCorrectly()
        {
            ////	prereq

            MathTest.MathTest math = new MathTest.MathTest();
            //  Average works
            math.Average_ReturnsAverageOfArray();
            //  StdDev works
            math.StdDev_ReturnsCorrect();
            //  Approx works
            math.Approx_Works();
            //  IsBetween works
            math.IsBetween_ReturnsCorrectAnswer();

            ////  arrange

            const int runsize = 1000;
            const int smoothness = 30;
            float[] normalArray = new float[runsize];

            ////  generate data

            //  generate normally distributed data
            for (int i = 0; i < runsize; i++)
            {
                //  Populate the normal array with normally distributed number 0-1
                for (int j = 0; j < smoothness; j++)
                {
                    normalArray[i] += UnityEngine.Random.value;
                }
                normalArray[i] /= smoothness;                
            }
            
            ////  act

            //  the generated "normal" array should be normally distributed
            bool normalArrayIsNormal = PRand.IsNormalDistribution(normalArray);
            
            ////  assert

            //string normalInfo = string.Format("Normal array: mean ({0}), median "
            //    + "({1}), mode ({2}), SD ({3})", normalArray.Average(),
            //    normalArray.Median(), normalArray.Mode(), PMath.StdDev(normalArray));
            //Debug.Log(normalInfo);
            Assert.IsTrue(normalArrayIsNormal, "Normal array is not recognized as normally distributed");
        }

        [Test]
        public void RandomSign_ReturnsOnlySigns()
        {
            //  arrange
            List<int> results = new List<int>();
            int runsize = 1000;

            //  act
            for (int i = 0; i < runsize; i++)
            {
                results.AddUnique(PRand.RandomSign());
            }

            //  assert
            Assert.IsTrue(results.Count.IsBetweenInt(1, 2, true, true));
            Assert.AreEqual(-1, Mathf.Min(results.ToArray()));
            Assert.AreEqual(1, Mathf.Max(results.ToArray()));
        }

        [Test]
        public void Random101_ReturnsOnlySignsOrZeros()
        {
            //  arrange
            List<int> results = new List<int>();
            int runsize = 1000;

            //  act
            for (int i = 0; i < runsize; i++)
            {
                results.AddUnique(PRand.Random101());
            }
            results.Sort();

            //  assert
            Assert.IsTrue(results.Count.IsBetweenInt(1, 3, true, true));
            Assert.AreEqual(-1, Mathf.Min(results.ToArray()));
            Assert.AreEqual(1, Mathf.Max(results.ToArray()));
            Assert.IsTrue(results.Contains(0));
        }

        [Test]
        public void RandomPosToNeg_ReturnsCorrectly()
        {
            //  arrange
            List<float> test = new List<float>();
            int runsize = 1000;

            //  act
            for (int i = 0; i < runsize; i++)
            {
                test.AddUnique(PRand.RandomPosToNeg());
            }
            test.Sort();
            float[] results = test.ToArray();

            //  assert
            Assert.IsTrue(-1 <= Mathf.Min(results));
            Assert.IsTrue(1 >= Mathf.Max(results));
        }

        [Test]
        public void RandomTop_ReturnsCorrectly()
        {
            //  arrange
            List<float> test = new List<float>();
            int runsize = 1000;

            //  act
            for (int i = 0; i < runsize; i++)
            {
                test.AddUnique(PRand.RandomTop(0.1f));
            }
            float[] results = test.ToArray();

            //  assert
            Assert.IsTrue(0.9f <= Mathf.Min(results));
            Assert.IsTrue(1 >= Mathf.Max(results));
        }

        [Test]
        public void RandomBool_ReturnsCorrectly()
        {
            //  Prereq
            MathTest.MathTest math = new MathTest.MathTest();
            math.Approx_Works();

            //  arrange
            float t = 0, f = 0;
            int runsize = 1000;

            //  act
            for (int i = 0; i < runsize; i++)
            {
                if (PRand.RandomBool())
                {
                    t++;
                }
                else
                {
                    f++;
                }
            }
            t /= runsize;
            f /= runsize;

            //  assert
            Assert.IsTrue(PMath.Approx(0.5f, t, 1));
            Assert.IsTrue(PMath.Approx(0.5f, f, 1));
        }

        [Test]
        public void RandomLinear_ReturnsCorrectly()
        {
            //  arrange
            const int precision = 2;
            const int bucketCount = 10;
            int normalizeFactor = 0; //  sum of 0+1+2+...+(bucketCount -1)
            for (int i = 0; i < bucketCount; normalizeFactor += i, i++) ;

            int[] buckets = new int[bucketCount];
            int runsize = 10000;

            //  act
            for (int i = 0; i < runsize; i++)
            {
                int bucket = Mathf.FloorToInt(PRand.RandomLinear() * bucketCount);
                buckets[bucket]++;
            }

            //  assert
            for (int i = 0; i < bucketCount; i++)
            {
                float expected = i / (bucketCount * normalizeFactor);
                float actual = buckets[i] / runsize;
                Assert.IsTrue(PMath.Approx(expected, actual, precision), "Count is off for bucket {0} (expected {1}, actual {2})", i, expected, actual);
            }
        }

        [Test]
        public void RandomQuaternion_ReturnsCorrectlyMasked()
        {
            //  arrange
            int precision = 1;
            int samples = 30000;
            Vector3[] dir = new Vector3[samples];
            Vector3 sum = Vector3.zero;
            Vector3 mask = Vector3.forward + Vector3.right;

            //  act
            for (int i = 0; i < samples; i++)
            {
                //dir[i] = PRand.RandomQuaternion(mask) * Vector3.forward;
                sum += dir[i];
            }
            float magnitude = sum.magnitude;
            float height = sum.y;

            //  assert
            Assert.IsTrue(PMath.Approx(magnitude, 0, precision), "Sum of {0} random directions does not total zero {1}", samples, magnitude);
            Assert.IsTrue(PMath.Approx(height, 0, precision), "Sum of {0} y-components does not total zero {1}", samples, height);
        }

        [Test]
        public void RandomQuaternion_ReturnsCorrectly()
        {
            //  arrange
            int precision = 1;
            int samples = 30000;
            Vector3[] dir = new Vector3[samples];
            Vector3 sum = Vector3.zero;

            //  act
            for(int i = 0; i < samples; i++)
            {
                dir[i] = PRand.RandomQuaternion() * Vector3.forward;
                sum += dir[i];
            }
            float magnitude = sum.magnitude;

            //  assert
            Assert.IsTrue(PMath.Approx(magnitude, 0, precision), "Sum of {0} random directions does not total zero {1}", samples, magnitude);
        }

        [Test]
        public void RandomDegreeAngle_ReturnsCorrectly()
        {
            //  arrange
            List<float> list = new List<float>();
            int runsize = 1000;

            //  act
            for (int i = 0; i < runsize; i++)
                list.AddUnique(PRand.RandomDegreeAngle());
            list.Sort();
            float min = list[0];
            float max = list[list.Count - 1];

            //  assert
            Assert.IsTrue(min > 0);
            Assert.IsTrue(max < 360);
        }

        [Test]
        public void RandomDegreeRadians_ReturnsCorrectly()
        {
            //  arrange
            List<float> list = new List<float>();
            int runsize = 1000;

            //  act
            for (int i = 0; i < runsize; i++)
                list.AddUnique(PRand.RandomRadianAngle());
            list.Sort();
            float min = list[0];
            float max = list[list.Count - 1];

            //  assert
            Assert.IsTrue(min > 0, TestHelper.ShowGComparison(min, 0));
            Assert.IsTrue(max < 2 * Mathf.PI, TestHelper.ShowLComparison(max, 2 * Mathf.PI));
        }

        [Test]
        public void RandomRightAngles_ReturnsCorrectly()
        {
            //  arrange
            int samples = 1000;
            float[] values = new float[samples];

            //  act
            for (int i = 0; i < samples; i++)
            {
                values[i] = PRand.RandomRightAngle() % 90;
            }

            //  assert
            for (int i = 0; i < samples; i++)
            {
                Assert.AreEqual(0, values[i]);
            }
        }

        [Test]
        public void RandomVertexOnMesh_ReturnsCorrectly()
        {
            throw new System.NotImplementedException();
        }

        [Test]
        public void RandomNormalDistribution_ReturnsCorrectly()
        {
            //  prereq
            IsNormalDistribution_ReturnsCorrectly();

            //  arrange
            const int runsize = 1000;
            float[] array = new float[runsize];

            //  act
            for (int i = 0; i < runsize; i++)
            {
                array[i] = PRand.RandomNormalDistribution();
            }
            bool normal = PRand.IsNormalDistribution(array);

            //  assert
            Assert.IsTrue(normal);
        }

        [Test]
        public void RandomUnbounded_ReturnsCorrectly()
        {
            //  arrange
            const float threshold = 0.8f;
            const int runsize = 1000;
            float total = 0;
            //  average 'unbounded' roll is 'max single roll' plus an average roll.
            float expected = (1 - threshold) * 0.5f + threshold * (1 - threshold) * 0.5f;

            //  act
            for (int i = 0; i < runsize; i++)
            {
                total += PRand.RandomUnbounded(threshold, 9);
            }
            total /= runsize;

            //  assert
            Assert.IsTrue(total > 0.5f, TestHelper.ShowVariables(total, 0.5f));
            Assert.IsTrue(expected <= total, TestHelper.ShowVariables(expected, total));
        }

        #endregion

        #region Dice

        [Test]
        public void RollDieStandard_ReturnsFlatDistribution()
        {
            //  Prereq
            MathTest.MathTest math = new MathTest.MathTest();
            math.Residuals_ReturnsCorrect();
            IsNormalDistribution_ReturnsCorrectly();

            //  NOTE -- Two flat distributions added together will
            //  become normally distributed

            //  arrange
            const int dieSize = 6;
            const int runsize = 1000;
            float[] rolls = new float[runsize];

            //  act
            for (int i = 0; i < runsize; i++)
            {
                rolls[i] = PRand.RollDie(dieSize, PRand.RollMethod.NORMAL);
                rolls[i] += PRand.RollDie(dieSize, PRand.RollMethod.NORMAL);
            }

            //  calculate test data
            bool isNormal = PRand.IsNormalDistribution(rolls);
            int min = (int)Mathf.Min(rolls);
            int max = (int)Mathf.Max(rolls);

            //  assert
            Assert.AreEqual(2, min, TestHelper.ShowVariables(2, min));
            Assert.AreEqual(2*dieSize, max, TestHelper.ShowVariables(2*dieSize, max));
            Assert.IsTrue(isNormal);
        }

        [Test]
        public void RollDieUnbounded_ReturnsResult()
        {
            //  Prereq
            MathTest.MathTest math = new MathTest.MathTest();
            math.Approx_Works();

            //  arrange
            const int dieSides = 6;
            const int runsize = 1000;
            int[] buckets = new int[dieSides];
            int unbounded = 0;
            //  average 'unbounded' roll is 'max single roll' plus an average roll.
            float expected = dieSides + 0.5f * (1 + dieSides);

            //  act
            for (int i = 0; i < runsize; i++)
            {
                int roll = PRand.RollDie(dieSides, PRand.RollMethod.UNBOUNDED);
                if (roll - 1 < dieSides)
                {
                    //  count rolls that exceed the normal roll,
                    //  or rolls that are 'unbounded'
                    buckets[roll]++;
                }
                else
                {
                    //  count the normal rolls
                    buckets[dieSides - 1]++;
                    unbounded += roll;
                }
            }
            unbounded /= buckets[dieSides - 1];

            //  assert
            //  each bucket should contain 1/diesides of the total rolls
            for (int i = 0; i < dieSides; i++)
            {
                float expectedContents = 1f / dieSides;
                Assert.IsTrue(PMath.Approx(expectedContents, buckets[i]), i + ": " + TestHelper.ShowVariables(expectedContents, buckets[i]));
            }

            Assert.IsTrue(PMath.Approx(expected, unbounded), TestHelper.ShowVariables(expected, unbounded));
            throw new NotImplementedException();
        }

        [Test]
        public void SuccessDieRoll_MatchReturnsCorrect()
        {
            //  Prereq
            MathTest.MathTest math = new MathTest.MathTest();
            math.Approx_Works();

            //  arrange
            int runsize = 1000;
            int diesize = 3;
            int target = 2;
            PRand.RollMethod method = PRand.RollMethod.MATCH_NUMBER;

            //  act
            float t = 0, f = 0;
            for (int i = 0; i < runsize; i++)
            {
                bool result = PRand.SuccessDieRoll(diesize, target, method);
                if (result) t++;
                else f++;
            }
            t /= runsize;
            f /= runsize;

            //  assert
            Assert.IsTrue(PMath.Approx(t * 2, f, 1), TestHelper.ShowVariables(t * 2, f));
        }

        [Test]
        public void SuccessDieRoll_RollAboveReturnsCorrect()
        {
            //  Prereq
            MathTest.MathTest math = new MathTest.MathTest();
            math.Approx_Works();

            //  arrange
            int runsize = 1000;
            int diesize = 3;
            int target = 2;
            PRand.RollMethod method = PRand.RollMethod.ROLL_ABOVE;

            //  act
            float t = 0, f = 0;
            for (int i = 0; i < runsize; i++)
            {
                bool result = PRand.SuccessDieRoll(diesize, target, method);
                if (result) t++;
                else f++;
            }
            t /= runsize;
            f /= runsize;

            //  assert
            Assert.IsTrue(PMath.Approx(t * 2, f, 1), TestHelper.ShowVariables(t * 2, f));
        }

        [Test]
        public void SuccessDieRoll_RollAboveInclusiveReturnsCorrect()
        {
            //  Prereq
            MathTest.MathTest math = new MathTest.MathTest();
            math.Approx_Works();

            //  arrange
            int runsize = 1000;
            int diesize = 3;
            int target = 3;
            PRand.RollMethod method = PRand.RollMethod.ROLL_ABOVE_INCLUSIVE;

            //  act
            float t = 0, f = 0;
            for (int i = 0; i < runsize; i++)
            {
                bool result = PRand.SuccessDieRoll(diesize, target, method);
                if (result) t++;
                else f++;
            }
            t /= runsize;
            f /= runsize;

            //  assert
            Assert.IsTrue(PMath.Approx(t * 2, f, 1), TestHelper.ShowVariables(t * 2, f));
        }

        [Test]
        public void SuccessDieRoll_RollBelowReturnsCorrect()
        {
            //  Prereq
            MathTest.MathTest math = new MathTest.MathTest();
            math.Approx_Works();

            //  arrange
            int runsize = 1000;
            int diesize = 3;
            int target = 1;
            PRand.RollMethod method = PRand.RollMethod.ROLL_BELOW;

            //  act
            float t = 0, f = 0;
            for (int i = 0; i < runsize; i++)
            {
                bool result = PRand.SuccessDieRoll(diesize, target, method);
                if (result) t++;
                else f++;
            }
            t /= runsize;
            f /= runsize;

            //  assert
            Assert.IsTrue(PMath.Approx(t * 2, f, 1), TestHelper.ShowVariables(t * 2, f));
        }

        [Test]
        public void SuccessDieRoll_RollBelowStrictReturnsCorrect()
        {
            //  Prereq
            MathTest.MathTest math = new MathTest.MathTest();
            math.Approx_Works();

            //  arrange
            int runsize = 1000;
            int diesize = 3;
            int target = 2;
            PRand.RollMethod method = PRand.RollMethod.ROLL_STRICTLY_BELOW;

            //  act
            float t = 0, f = 0;
            for (int i = 0; i < runsize; i++)
            {
                bool result = PRand.SuccessDieRoll(diesize, target, method);
                if (result) t++;
                else f++;
            }
            t /= runsize;
            f /= runsize;

            //  assert
            Assert.IsTrue(PMath.Approx(t * 2, f, 1), TestHelper.ShowVariables(t * 2, f));
        }

        [Test]
        public void RollDice_ReturnsCorrect()
        {
            //  Prereq
            MathTest.MathTest math = new MathTest.MathTest();
            math.Approx_Works();
            IsNormalDistribution_ReturnsCorrectly();

            //  arrange
            const int dieSides = 6;
            const int diceCount = 2;
            float expected = (1 + dieSides);
            const int runsize = 10000;
            float[] array = new float[runsize];


            //  act
            for (int i = 0; i < runsize; i++)
            {
                array[i] = PRand.RollDice(dieSides, diceCount);
            }
            bool normal = PRand.IsNormalDistribution(array);
            float mean = array.Average();

            //  assert
            Assert.IsTrue(PMath.Approx(expected, mean,1), "expected value "+ expected+" doesn't match mean "+mean);
            Assert.IsTrue(normal, "not normally distributed");
        }

        [Test]
        public void RollD4_ReturnsCorrect()
        {
            //  Prereq
            MathTest.MathTest math = new MathTest.MathTest();
            math.Approx_Works();
            IsNormalDistribution_ReturnsCorrectly();

            //  arrange
            const int dieSides = 4;
            const int diceCount = 2;
            float expected = (1 + dieSides);
            const int runsize = 10000;
            float[] array = new float[runsize];


            //  act
            for (int i = 0; i < runsize; i++)
            {
                array[i] = PRand.RollD4(diceCount);
            }
            bool normal = PRand.IsNormalDistribution(array);
            float mean = array.Average();

            //  assert
            Assert.IsTrue(PMath.Approx(expected, mean, 1), "expected value " + expected + " doesn't match mean " + mean);
            Assert.IsTrue(normal, "not normally distributed");
        }

        [Test]
        public void RollD6_ReturnsCorrect()
        {
            //  Prereq
            MathTest.MathTest math = new MathTest.MathTest();
            math.Approx_Works();
            IsNormalDistribution_ReturnsCorrectly();

            //  arrange
            const int dieSides = 6;
            const int diceCount = 2;
            float expected = (1 + dieSides);
            const int runsize = 10000;
            float[] array = new float[runsize];


            //  act
            for (int i = 0; i < runsize; i++)
            {
                array[i] = PRand.RollD6(diceCount);
            }
            bool normal = PRand.IsNormalDistribution(array);
            float mean = array.Average();

            //  assert
            Assert.IsTrue(PMath.Approx(expected, mean, 1), "expected value " + expected + " doesn't match mean " + mean);
            Assert.IsTrue(normal, "not normally distributed");
        }

        [Test]
        public void RollD8_ReturnsCorrect()
        {
            //  Prereq
            MathTest.MathTest math = new MathTest.MathTest();
            math.Approx_Works();
            IsNormalDistribution_ReturnsCorrectly();

            //  arrange
            const int dieSides = 8;
            const int diceCount = 2;
            float expected = (1 + dieSides);
            const int runsize = 10000;
            float[] array = new float[runsize];


            //  act
            for (int i = 0; i < runsize; i++)
            {
                array[i] = PRand.RollD8(diceCount);
            }
            bool normal = PRand.IsNormalDistribution(array);
            float mean = array.Average();

            //  assert
            Assert.IsTrue(PMath.Approx(expected, mean, 1), "expected value " + expected + " doesn't match mean " + mean);
            Assert.IsTrue(normal, "not normally distributed");
        }

        [Test]
        public void RollD10_ReturnsCorrect()
        {
            //  Prereq
            MathTest.MathTest math = new MathTest.MathTest();
            math.Approx_Works();
            IsNormalDistribution_ReturnsCorrectly();

            //  arrange
            const int dieSides = 10;
            const int diceCount = 2;
            float expected = (1 + dieSides);
            const int runsize = 10000;
            float[] array = new float[runsize];


            //  act
            for (int i = 0; i < runsize; i++)
            {
                array[i] = PRand.RollD10(diceCount);
            }
            bool normal = PRand.IsNormalDistribution(array);
            float mean = array.Average();

            //  assert
            Assert.IsTrue(PMath.Approx(expected, mean, 1), "expected value " + expected + " doesn't match mean " + mean);
            Assert.IsTrue(normal, "not normally distributed");
        }

        [Test]
        public void RollD12_ReturnsCorrect()
        {
            //  Prereq
            MathTest.MathTest math = new MathTest.MathTest();
            math.Approx_Works();
            IsNormalDistribution_ReturnsCorrectly();

            //  arrange
            const int dieSides = 12;
            const int diceCount = 2;
            float expected = (1 + dieSides);
            const int runsize = 10000;
            float[] array = new float[runsize];


            //  act
            for (int i = 0; i < runsize; i++)
            {
                array[i] = PRand.RollD12(diceCount);
            }
            bool normal = PRand.IsNormalDistribution(array);
            float mean = array.Average();

            //  assert
            Assert.IsTrue(PMath.Approx(expected, mean, 1), "expected value " + expected + " doesn't match mean " + mean);
            Assert.IsTrue(normal, "not normally distributed");
        }

        [Test]
        public void RollD20_ReturnsCorrect()
        {
            //  Prereq
            MathTest.MathTest math = new MathTest.MathTest();
            math.Approx_Works();
            IsNormalDistribution_ReturnsCorrectly();

            //  arrange
            const int dieSides = 20;
            const int diceCount = 2;
            float expected = (1 + dieSides);
            const int runsize = dieSides * 10000;
            float[] array = new float[runsize];


            //  act
            for (int i = 0; i < runsize; i++)
            {
                array[i] = PRand.RollD20(diceCount);
            }
            bool normal = PRand.IsNormalDistribution(array);
            float mean = array.Average();

            //  assert
            Assert.IsTrue(PMath.Approx(expected, mean, 1), "expected value " + expected + " doesn't match mean " + mean);
            Assert.IsTrue(normal, "not normally distributed");
        }

        [Test]
        public void RollD100_ReturnsCorrect()
        {
            //  Prereq
            MathTest.MathTest math = new MathTest.MathTest();
            math.Approx_Works();
            IsNormalDistribution_ReturnsCorrectly();

            //  arrange
            const int dieSides = 100;
            const int diceCount = 2;
            float expected = (1 + dieSides);
            const int runsize = dieSides * 10000;
            float[] array = new float[runsize];


            //  act
            for (int i = 0; i < runsize; i++)
            {
                array[i] = PRand.RollD100(diceCount);
            }
            bool normal = PRand.IsNormalDistribution(array);
            float mean = array.Average();

            //  assert
            Assert.IsTrue(PMath.Approx(expected, mean, 1), "expected value " + expected + " doesn't match mean " + mean);
            Assert.IsTrue(normal, "not normally distributed");
        }

        #endregion

        #region Random Selection

        [Test]
        public void ToInt_BoolExtensionWorks()
        {
            //  arrange

            //  act
            int t = true.ToInt();
            int f = false.ToInt();

            //  assert
            Assert.AreEqual(1, t);
            Assert.AreEqual(0, f);
        }

        [Test]
        public void RandomizeArray_ExtensionShufflesRandomly()
        {
            //  Prereq
            MathTest.MathTest math = new MathTest.MathTest();
            math.Approx_Works();
            math.WeightedSumArray_Works();

            //  arrange
            float[] array = new float[] { 0, 1.1f, 2.2f, 3.3f, 4.4f, 5.5f };
            int runsize = 1000;

            //  act
            float originalSum = array.WeightedSum() * runsize;
            float averageSum = 0;

            for (int i = 0; i < runsize; i++)
            {
                array.Randomize();
                averageSum += array.WeightedSum();
            }

            //  assert
            Assert.IsFalse(PMath.Approx(originalSum, averageSum, 1), TestHelper.ShowVariables(originalSum, averageSum));
        }

        [Test]
        public void RandomizeArray_ExtensionShufflesConsistantly()
        {
            //  Prereq
            MathTest.MathTest math = new MathTest.MathTest();
            math.Approx_Works();
            math.WeightedSumArray_Works();

            //  arrange
            int precision = 2;
            int seed = 1234;
            float[] source = new float[] { 0, 1.1f, 2.2f, 3.3f, 4.4f, 5.5f };
            int runsize = (int)Mathf.Pow(10, precision);

            //  act
            float[] expected = source.Clone() as float[];
            expected.Randomize(seed);
            float expectedSum = expected.WeightedSum() * runsize;

            float actualSum = 0;
            for (int i = 0; i < runsize; i++)
            {
                float[] actual = source.Clone() as float[];
                actual.Randomize(seed);
                actualSum += actual.WeightedSum();
            }

            //  assert
            Assert.IsTrue(PMath.Approx(expectedSum, actualSum, precision), TestHelper.ShowVariables(expectedSum, actualSum));
        }

        [Test]
        public void RandomizeList_ExtensionShufflesRandomly()
        {
            //  Prereq
            MathTest.MathTest math = new MathTest.MathTest();
            math.Approx_Works();
            math.WeightedSumList_Works();

            //  arrange
            List<float> source = new List<float> { 0, 1.1f, 2.2f, 3.3f, 4.4f, 5.5f };
            int runsize = 1000;

            //  act
            float originalSum = source.ToArray().WeightedSum() * runsize;
            float averageSum = 0;

            for (int i = 0; i < runsize; i++)
            {
                source.Randomize();
                averageSum += source.ToArray().WeightedSum();
            }

            //  assert
            Assert.IsFalse(PMath.Approx(originalSum, averageSum, 1), TestHelper.ShowVariables(originalSum, averageSum));
        }

        [Test]
        public void RandomizeList_ExtensionShufflesConsistantly()
        {
            //  Prereq
            MathTest.MathTest math = new MathTest.MathTest();
            math.Approx_Works();
            math.WeightedSumList_Works();
            UtilTest.UtilTest util = new UtilTest.UtilTest();
            util.ArrayToList_ReturnsCorrectly();

            //  arrange
            int precision = 1;
            int seed = 1234;
            float[] source = new float[] { 0, 1.1f, 2.2f, 3.3f, 4.4f, 5.5f };
            int runsize = (int)Mathf.Pow(10, precision);

            //  act
            List<float> expected = source.ToList();
            expected.Randomize(seed);
            float expectedSum = expected.WeightedSum() * runsize;

            float actualSum = 0;
            for (int i = 0; i < runsize; i++)
            {
                List<float> actual = source.ToList();
                actual.Randomize(seed);
                actualSum += actual.WeightedSum();
            }

            //  assert
            Assert.IsTrue(PMath.Approx(expectedSum, actualSum, precision), TestHelper.ShowVariables(expectedSum, actualSum));
        }

        [Test]
        public void GetRandomArray_ExtensionReturnsRandomly()
        {
            //  Prereq
            MathTest.MathTest math = new MathTest.MathTest();
            math.Var_ReturnsCorrect();

            //  arrange
            int runsize = 1000;
            int[] source = new int[runsize];
            for (int i = 0; i < runsize; i++) source[i] = i;

            //  act
            List<float> selections = new List<float>();
            for (int i = 0; i < runsize; i++) selections.Add(source.GetRandom());
            float variance = PMath.Var(selections.ToArray());

            //  assert
            Assert.AreNotEqual(0, variance, TestHelper.ShowVariable("Variance", variance));
        }

        [Test]
        public void GetRandomList_ExtensionReturnsRandomly()
        {
            //  Prereq
            MathTest.MathTest math = new MathTest.MathTest();
            math.Var_ReturnsCorrect();

            //  arrange
            int runsize = 1000;
            List<int> source = new List<int>();
            for (int i = 0; i < runsize; i++) source.Add(i);

            //  act
            List<float> selections = new List<float>();
            for (int i = 0; i < runsize; i++) selections.Add(source.GetRandom());
            float variance = PMath.Var(selections.ToArray());

            //  assert
            Assert.AreNotEqual(0, variance, TestHelper.ShowVariable("Variance", variance));
        }

        [Test]
        public void GetRandomFromTop_ExtensionReturnsCorrectly()
        {
            //  Prereq
            MathTest.MathTest math = new MathTest.MathTest();
            math.Approx_Works();

            //  arrange
            const int runsize = 1000;
            List<int> data = new List<int> { 0, 1, 2, 3, 4, 5, 6 };
            int[] samples = new int[runsize];

            //  act
            for (int i = 0; i < runsize; i++)
            {
                samples[i] = data.GetRandomFromTop(2);
            }
            float expectedMean = 0.5f * (data[0] + data[1]);
            float min = data[0] * runsize;
            float max = data[1] * runsize;
            bool expectedMeanMatches = PMath.Approx(expectedMean, samples.Average(), 1);
            bool between = samples.Sum().IsBetweenInt(min, max, false, false);

            //  assert
            Assert.IsTrue(expectedMeanMatches, TestHelper.ShowVariables(expectedMean, samples.Average()));
            Assert.IsTrue(between, min + "<" + samples.Sum() + "<" + max);
        }

        [Test]
        public void GetRandomFromBottom_ExtensionReturnsCorrectly()
        {
            //  Prereq
            MathTest.MathTest math = new MathTest.MathTest();
            math.Approx_Works();

            //  arrange
            const int runsize = 1000;
            List<int> data = new List<int> { 0, 1, 2, 3, 4, 5, 6 };
            int[] samples = new int[runsize];

            //  act
            for (int i = 0; i < runsize; i++)
            {
                samples[i] = data.GetRandomFromBottom(2);
            }
            int dataSize = data.Count;
            float expectedMean = 0.5f * (data[dataSize - 2] + data[dataSize - 1]);
            float min = data[dataSize - 2] * runsize;
            float max = data[dataSize - 1] * runsize;

            //  assert
            Assert.IsTrue(PMath.Approx(expectedMean, samples.Average(), 1), TestHelper.ShowVariables(expectedMean, samples.Average()));
            Assert.IsTrue(samples.Sum().IsBetweenInt(min, max), min + "<" + samples.Sum() + "<" + max);
        }

        [Test]
        public void WeightedRandomIndex_ValidWeightsReturnsCorrectly()
        {
            //  Prereq
            MathTest.MathTest math = new MathTest.MathTest();
            math.Approx_Works();

            //  arrange
            float[] p = { 1.0f, 1.0f, 3.0f };
            int runsize = Mathf.CeilToInt(1000 * p.Sum());
            int accuracy = 1;

            //	act
            float[] actual = new float[3];
            for (int i = 0; i < runsize; i++)
            {
                actual[PRand.WeightedRandomIndex(p)]++;
            }
            float[] expected = new float[3] {
				runsize * p[0] / p.Sum(),
				runsize * p[1] / p.Sum(),
				runsize * p[2] / p.Sum()
			};

            for (int i = 0; i < 3; i++)
            {
                actual[i] /= runsize;
                expected[i] /= runsize;
            }

            //  assert
            string data = "{ " + actual[0] + ", " + actual[1] + ", " + actual[2] + " }";
            Assert.IsTrue(PMath.Approx(expected[0], actual[0], accuracy), TestHelper.ShowVariables(expected[0], actual[0]) + " " + data);
            Assert.IsTrue(PMath.Approx(expected[1], actual[1], accuracy), TestHelper.ShowVariables(expected[1], actual[1]) + " " + data);
            Assert.IsTrue(PMath.Approx(expected[2], actual[2], accuracy), TestHelper.ShowVariables(expected[2], actual[2]) + " " + data);
        }

        [Test]
        public void WeightedRandomIndex_NoWeightsReturnsCorrectly()
        {
            //  Prereq
            MathTest.MathTest math = new MathTest.MathTest();
            math.Approx_Works();

            //  arrange
            float[] p = { 0, 0, 0, 0 };
            int runsize = Mathf.CeilToInt(Mathf.Max(1000, 1000 * p.Sum()));
            int accuracy = 1;

            //	act
            float[] actual = new float[p.Length];
            for (int i = 0; i < runsize; i++) actual[PRand.WeightedRandomIndex(p)]++;
            for (int i = 0; i < p.Length; i++) actual[i] /= runsize;

            float[] expected = new float[p.Length];
            expected.AssignAll(1f / p.Length);

            //  assert
            string data = "{ " + actual[0] + ", " + actual[1] + ", " + actual[2] + " }";
            Assert.IsTrue(PMath.Approx(expected[0], actual[0], accuracy), TestHelper.ShowVariables(expected[0], actual[0]) + " " + data);
            Assert.IsTrue(PMath.Approx(expected[1], actual[1], accuracy), TestHelper.ShowVariables(expected[1], actual[1]) + " " + data);
            Assert.IsTrue(PMath.Approx(expected[2], actual[2], accuracy), TestHelper.ShowVariables(expected[2], actual[2]) + " " + data);
        }

        [Test]
        public void WeightedRandomIndex_EqualWeightsReturnsCorrectly()
        {
            //  Prereq
            MathTest.MathTest math = new MathTest.MathTest();
            math.Approx_Works();

            //  arrange
            float[] p = { 2.2f, 2.2f, 2.2f };
            int runsize = Mathf.CeilToInt(Mathf.Max(1000, 1000 * p.Sum()));
            int accuracy = 1;

            //	act
            float[] actual = new float[p.Length];
            for (int i = 0; i < runsize; i++) actual[PRand.WeightedRandomIndex(p)]++;
            for (int i = 0; i < p.Length; i++) actual[i] /= runsize;

            float[] expected = new float[p.Length];
            expected.AssignAll(1f / p.Length);

            //  assert
            string data = "{ " + actual[0] + ", " + actual[1] + ", " + actual[2] + " }";
            Assert.IsTrue(PMath.Approx(expected[0], actual[0], accuracy), TestHelper.ShowVariables(expected[0], actual[0]) + " " + data);
            Assert.IsTrue(PMath.Approx(expected[1], actual[1], accuracy), TestHelper.ShowVariables(expected[1], actual[1]) + " " + data);
            Assert.IsTrue(PMath.Approx(expected[2], actual[2], accuracy), TestHelper.ShowVariables(expected[2], actual[2]) + " " + data);
        }

        [Test]
        public void WeightedRandomIndex_NegativeWeightsThrowException()
        {
            //  arrange
            float[] p = { 1.3f, 6.7f, -9.9f };

            //	act
            bool exceptionThrown = false;
            try
            {
                PRand.WeightedRandomIndex(p);
            }
            catch (System.ArgumentException)
            {
                exceptionThrown = true;
            }

            //  assert
            Assert.IsTrue(exceptionThrown);
        }

        [Test]
        public void RandomPointInsidePolygon2d_ReturnsCorrectly()
        {
            //  prereq
            MathTest.MathTest math = new MathTest.MathTest();
            math.AngleSum2d_ReturnsCorrectly();

            //  arrange
            Vector2[] polygon = new Vector2[] { Vector2.up, Vector2.right, Vector2.down, Vector2.left };
            Vector2 point;

            //  act
            point = PRand.RandomPointInsidePolygon2d(polygon);
            bool inside = Mathf.Approximately(Mathf.PI * 2, PMath.AngleSum2d(point, polygon));

            //  assert
            Assert.IsTrue(inside);
        }

        [Test]
        public void RandomPointOnPolygon2dEdge_FailsCorrectly()
        {
            //  arrange
            Vector2[] v = new Vector2[0];

            //  act
            Vector2 result = PRand.RandomPointOnPolygon2dEdge(v);

            //  assert
            Assert.AreEqual(Vector2.zero, result);
        }

        [Test]
        public void RandomPointOnPolygon2dEdge_HandlesTrivialCaseCorrectly()
        {
            //  arrange
            Vector2[] v = new Vector2[] { new Vector2(2, 5) };

            //  act
            Vector2 result = PRand.RandomPointOnPolygon2dEdge(v);

            //  assert
            Assert.AreEqual(v[0], result);
        }

        [Test]
        public void RandomPointOnPolygon2dEdge_ReturnsCorrectly()
        {
            //  Prereq
            MathTest.MathTest math = new MathTest.MathTest();
            math.Approx_Works();

            //  arrange
            Vector2[] points = new Vector2[] { 
                Vector2.up,
                Vector2.right,
                Vector2.down,
                Vector2.left
            };

            //  act
            Vector2 result = PRand.RandomPointOnPolygon2dEdge(points);

            //  assert
            bool slopeMatch = false;
            bool pointsBounded = false;
            for (int i = 0; i < points.Length; i++)
            {
                int startIndex = i;
                int endIndex = (i + 1) % points.Length;
                Vector2 start = points[startIndex];
                Vector2 end = points[endIndex];
                Vector2 v = end - start;
                float expectedSlope = v.x / v.y;
                float actualSlope = result.x / result.y;
                slopeMatch |= PMath.Approx(expectedSlope, actualSlope);

                bool bounded = result.x.IsBetween(start.x, end.x);
                bounded &= result.y.IsBetween(start.y, end.y);
                pointsBounded |= bounded;
            }
            Assert.IsTrue(slopeMatch, "slopes don't match");
            Assert.IsTrue(pointsBounded, "result isn't between any vertex pair");
        }

        #endregion

        #region Shapes

        [Test]
        public void OnHelix_ReturnsCorrectly()
        {
            //  prereq
            MathTest.MathTest math = new MathTest.MathTest();
            math.Approx_Works();

            //  arrange
            float radius = .2f;
            int samples = 300;
            int correct = 0;

            //  act
            for (int i = 0; i < samples; i++)
            {
                Vector3 point = PRand.OnHelix(radius);
                correct += PMath.Approx(Vector2.SqrMagnitude(point.ToVector2()), radius * radius,1) ? 1 : 0;
            }

            //  assert
            Debug.LogWarning("UnitTest for OnHelix is not robust. Test only checks that the returned point is on the helix radius.");
            Assert.AreEqual(samples, correct);
        }

        [Test]
        public void InsideHelix_ReturnsCorrectly()
        {
            //  prereq
            MathTest.MathTest math = new MathTest.MathTest();
            math.Approx_Works();

            //  arrange
            float radius = 0.2f;
            int samples = 300;
            int correct = 0;

            //  act
            for (int i = 0; i < samples; i++)
            {
                Vector3 point = PRand.InsideHelix(radius);
                correct += Vector2.SqrMagnitude(point.ToVector2()) <= radius * radius ? 1 : 0;
            }

            //  assert
            Debug.LogWarning("UnitTest for OnHelix is not robust. Test only checks that the returned point is within the helix radius.");
            Assert.AreEqual(samples, correct);
        }

        [Test]
        public void HelixConstainsPoint_ReturnsCorrectly()
        {
            throw new NotImplementedException();
        }

        [Test]
        public void OnCone_ReturnsCorrectly()
        {
            throw new NotImplementedException();
        }

        [Test]
        public void InsideCone_ReturnsCorrectly()
        {
            throw new NotImplementedException();
        }

        [Test]
        public void ConeConstainsPoint_ReturnsCorrectly()
        {
            throw new NotImplementedException();
        }

        [Test]
        public void OnCylinder_ReturnsSidesOnly()
        {
            //  prereq
            MathTest.MathTest math = new MathTest.MathTest();
            math.Approx_Works();

            //  arrange
            int samples = 100;
            int correct = 0;
            float radius = 0.15f;
            float height = 3;

            //  act
            for (int i = 0; i < samples; i++)
            {
                Vector3 point = PRand.InsideCylinder(radius, height);
                correct += point.y.IsBetween(0, height, true, true)
                    && PMath.Approx(Vector2.SqrMagnitude(point.ToVector2()), radius * radius,1) ? 1 : 0;
            }

            //  assert
            Assert.AreEqual(samples, correct, "Some samples had incorrect radius " + samples + " != " + correct);
        }

        [Test]
        public void OnCylinder_ReturnIncludesEndcaps()
        {
            //  prereq
            MathTest.MathTest math = new MathTest.MathTest();
            math.Approx_Works();
            math.ToVector2_ReturnsConvertedVector();

            //  arrange
            int samples = 100;
            int endcaps = 0;
            int sides = 0;
            int correct = 0;
            float radius = 0.15f;
            float height = 3;

            //  act
            for (int i = 0; i < samples; i++)
            {
                Vector3 point = PRand.OnCylinder(radius, height, true);
                if (PMath.Approx(point.y,0,6) || PMath.Approx(point.y, height,6))
                {
                    //  point on endcap
                    correct++;
                    endcaps++;
                    continue;
                }
                else
                {
                    //  point in the middle
                    bool validY = point.y.IsBetween(0, height, true, true);
                    bool validXZ = PMath.Approx(Vector2.SqrMagnitude(point.ToVector2()), radius * radius,1);
                    correct += validY && validXZ ? 1 : 0;
                    sides += validY && validXZ ? 1 : 0;
                    continue;
                }
            }

            //  assert
            Assert.IsTrue(endcaps > 0, "endcaps " + endcaps + " not greater than 0");
            Assert.AreEqual(samples, correct, "total correct " + correct + " don't equal samples " + samples);
            Assert.AreEqual(samples - endcaps,sides, "sides " + sides + " plus endcaps " + endcaps + " don't add up to " + samples);
        }

        [Test]
        public void InsideCylinder_ReturnsCorrectly()
        {
            //  prereq
            MathTest.MathTest math = new MathTest.MathTest();
            math.Approx_Works();

            //  arrange
            int samples = 100;
            int correct = 0;
            float radius = 0.15f;
            float height = 3;

            //  act
            for (int i = 0; i < samples; i++)
            {
                Vector3 point = PRand.InsideCylinder(radius, height);
                correct += point.y.IsBetween(0, height, true, true)
                    && Vector2.SqrMagnitude(point.ToVector2()) <= radius * radius ? 1 : 0;
            }

            //  assert
            Assert.AreEqual(samples, correct, "Some samples had incorrect radius " + samples + " != " + correct);
        }

        [Test]
        public void CylinderContains_ReturnsCorrectly()
        {
            //  prereq
            MathTest.MathTest math = new MathTest.MathTest();
            math.Approx_Works();

            //  arrange
            Vector3 inPoint = Vector3.one * 0.5f;
            Vector3 outPoint = Vector3.one;

            //  act
            bool inIsTrue = PRand.CylinderContainsPoint(inPoint);
            bool outIsFalse = PRand.CylinderContainsPoint(outPoint);

            //  assert
            Assert.IsTrue(inIsTrue);
            Assert.IsFalse(outIsFalse);
        }

        [Test]
        public void OnSphere_ReturnsCorrectly()
        {
            //  arrange
            int samples = 100;
            int correct = 0;
            float radius = 0.15f;

            //  act
            for (int i = 0; i < samples; i++)
            {
                Vector3 point = PRand.OnSphere(radius);
                correct += PMath.Approx(Vector3.SqrMagnitude(point), radius * radius) ? 1 : 0;
            }

            //  assert
            Assert.AreEqual(samples, correct);
        }

        [Test]
        public void InsideSphere_ReturnsCorrectly()
        {
            //  arrange
            int samples = 100;
            int correct = 0;
            float radius = 0.15f;

            //  act
            for (int i = 0; i < samples; i++)
            {
                Vector3 point = PRand.InsideSphere(radius);
                correct += Vector3.SqrMagnitude(point) <= radius * radius ? 1 : 0;
            }

            //  assert
            Assert.AreEqual(samples, correct);
        }

        [Test]
        public void SphereConstainsPoint_ReturnsCorrectly()
        {
            //  arrange
            Vector3 inside = Vector3.zero;
            Vector3 outside = Vector3.one;

            //  act
            bool isInside = PRand.SphereContainsPoint(inside);
            bool isOutside = PRand.SphereContainsPoint(outside);

            //  assert
            Assert.IsTrue(isInside);
            Assert.IsFalse(isOutside);
        }

        [Test]
        public void OnCircle_ReturnsCorrectly()
        {
            //  prereq
            MathTest.MathTest math = new MathTest.MathTest();
            math.Approx_Works();

            //  arrange
            int samples = 100;
            int correct = 0;
            float radius = 0.15f;

            //  act
            for (int i = 0; i < samples; i++)
            {
                Vector2 point = PRand.OnCircle(radius);
                correct += PMath.Approx(Vector2.SqrMagnitude(point), radius * radius,6) ? 1 : 0;
            }

            //  assert
            Assert.AreEqual(samples, correct, "Some samples had incorrect radius " + samples + " != " + correct);
        }

        [Test]
        public void InsideCircle_ReturnsCorrectly()
        {
            //  arrange
            int samples = 100;
            int insideCount = 0;
            float radius = 0.15f;

            //  act
            for (int i = 0; i < samples; i++)
            {
                Vector2 point = PRand.InsideCircle(radius);
                insideCount += Vector2.SqrMagnitude(point) <= radius * radius ? 1 : 0;
            }

            //  assert
            Assert.AreEqual(samples, insideCount);
        }

        [Test]
        public void CircleConstainsPoint_ReturnsCorrectly()
        {
            //  arrange
            Vector2 inside = Vector2.zero;
            Vector2 outside = Vector2.one;

            //  act
            bool isInside = PRand.CircleContainsPoint(inside);
            bool isOutside = PRand.CircleContainsPoint(outside);

            //  assert
            Assert.IsTrue(isInside);
            Assert.IsFalse(isOutside);
        }

        #endregion

        #region Vectors

        [Test]
        public void RandomVector3_ReturnsCorrectly()
        {
            //  prereq
            MathTest.MathTest math = new MathTest.MathTest();
            math.Approx_Works();

            //  arrange
            int magnitude = 3;

            //  act
            Vector3 v = PRand.RandomVector3(magnitude);
            float m = v.magnitude;

            //  assert
            Assert.IsTrue(PMath.Approx(magnitude, m,1));
        }

        [Test]
        public void RandomVector2_ReturnsCorrectly()
        {
            //  arrange
            int magnitude = 3;

            //  act
            Vector2 v = PRand.RandomVector2(magnitude);
            float m = v.magnitude;

            //  assert
            Assert.IsTrue(PMath.Approx(magnitude, m));
        }

        [Test]
        public void Dirt2_ReturnsCorrectly()
        {
            //  arrange
            float dirtMagnitude = 3;

            //  act
            Vector2 actual = PRand.Dirt2(3);
            float m = actual.magnitude;

            //  assert
            Assert.IsTrue(m.IsBetween(0, dirtMagnitude));
        }

        [Test]
        public void Dirt3_ReturnsCorrectly()
        {
            //  arrange
            float dirtMagnitude = 3;

            //  act
            Vector3 actual = PRand.Dirt3(3);
            float m = actual.magnitude;

            //  assert
            Assert.IsTrue(m.IsBetween(0, dirtMagnitude));
        }

        [Test]
        public void ScaleAndReturn_V3ExtensionReturnsCorrectly()
        {
            //  arrange
            Vector3 scale = new Vector3(1, 2, 3);
            Vector3 source = new Vector3(2, 3, 4);

            //  act
            Vector3 expected = Vector3.right * scale.x * source.x
                + Vector3.up * scale.y * source.y
                + Vector3.forward * scale.z * source.z;
            float expectedMag = expected.magnitude;

            Vector3 actual = PRand.Scale(source, scale);
            float actualMag = actual.magnitude;

            //  assert
            Assert.AreNotEqual(source, expected);
            Assert.AreEqual(expected, actual);
            Assert.AreEqual(expectedMag, actualMag);
        }

        [Test]
        public void ScaleAndReturn_V2ExtensionReturnsCorrectly()
        {
            //  arrange
            Vector2 source = new Vector2(1, 2);
            Vector2 scale = new Vector2(2, 3);

            //  act
            Vector2 expected = source;
            expected.Scale(scale);
            float expectedMag = expected.magnitude;

            Vector2 actual = PRand.Scale(source, scale);
            float actualMag = actual.magnitude;

            //  assert
            Assert.AreNotEqual(source, expected);
            Assert.AreEqual(expected, actual);
            Assert.AreEqual(expectedMag, actualMag);
        }

        [Test]
        public void InvertAxis3_ReturnsCorrectly()
        {
            //  arrange
            Vector3 v = new Vector3(3, 4, 5);

            //  act
            Vector3 expected = new Vector3(1f / 3, 1f / 4, 1f / 5);
            Vector3 actual = PRand.InvertAxis(v);

            //  assert
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void InvertAxis2_ReturnsCorrectly()
        {
            //  arrange
            Vector2 v = new Vector2(3, 4);

            //  act
            Vector2 expected = new Vector2(1f / 3, 1f / 4);
            Vector2 actual = PRand.InvertAxis(v);

            //  assert
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void RandomPlanarVector_ReturnsCorrectly()
        {
            //  arrange
            float mag = 3;

            //  act
            Vector3 xz = PRand.RandomPlanarVector(Vector3.up, mag);
            Vector3 xy = PRand.RandomPlanarVector(Vector3.forward, mag);
            Vector3 yz = PRand.RandomPlanarVector(Vector3.right, mag);
            
            //  assert
            Assert.IsTrue(PMath.Approx(mag, xz.magnitude), "Magnitudes do not match: {0} ~ {1}", mag, xz.magnitude);
            Assert.IsTrue(PMath.Approx(0, xz.y), "y-component is non-zero: {0}", xz.ToString());
            Assert.IsTrue(PMath.Approx(mag, xy.magnitude), "Magnitudes do not match: {0} ~ {1}", mag, xy.magnitude);
            Assert.IsTrue(PMath.Approx(0, xy.z), "z-component is non-zero: {0}", xy.ToString());
            Assert.IsTrue(PMath.Approx(mag, yz.magnitude), "Magnitudes do not match: {0} ~ {1}", mag, yz.magnitude);
            Assert.IsTrue(PMath.Approx(0, yz.x), "x-component is non-zero: {0}", yz.ToString());
        }

        [Test]
        public void RandomHorizontalVector_ReturnsCorrectly()
        {
            //  Prereq
            MathTest.MathTest math = new MathTest.MathTest();
            math.Approx_Works();

            //  arrange
            float mag = 3;

            //  act
            Vector3 v = PRand.RandomHorizontalVector(mag);

            //  assert
            Assert.IsTrue(PMath.Approx(mag, v.magnitude), "Magnitudes do not match: {0} ~ {1}", mag, v.magnitude);
            Assert.IsTrue(PMath.Approx(0, v.y),"y-component is non-zero: {0}", v.ToString());
        }

        [Test]
        public void RandomOrthoVector_ReturnsCorrectly()
        {
            //  arrange
            Vector3[] expected = new Vector3[] { Vector3.forward, Vector3.right, Vector3.up };
            Vector3 actual;

            //  act
            actual = PRand.RandomOrthoVector();

            //  assert
            Assert.IsTrue(expected.Contains(actual), "actual vector not contained in expected array");

        }

        #endregion
    }
}