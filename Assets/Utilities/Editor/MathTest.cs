using UnityEngine;
using System.Collections.Generic;
using NUnit.Framework;
using PLib.Math;
using PLib.TestHelper;
using Mathd = System.Math;

namespace MathTest
{
    [TestFixture]
    public class MathTest
    {
        //  Complete
        #region Formatting

        [Test]
        public void FormatTime_ReturnsFormattedTime()
        {
            //  arrange
            float seconds = 5.0607f + 4 * 60 + 3 * 60 * 60 + 2 * 24 * 60 * 60;
            string expected = "2:03:04:05.06";

            //  act
            string actual = PMath.FormatTime(seconds);

            //  assert
            Assert.IsTrue(expected.Equals(actual), TestHelper.ShowVariables(expected, actual));
        }

        [Test]
        public void ToCurrency_ReturnsFormattedCurrency()
        {
            //  arrange
            int money = 1234567;

            //  act
            string currency = money.ToCurrency("$");

            //  assert
            Assert.IsTrue(currency.Equals("$1,234,567"));
        }

        [Test]
        public void GetExecutionTime_ReturnsCurrentExecutionTime()
        {
            //  arrange
            string separator = ">";
            float time = UnityEngine.Time.time;
            string expected = PMath.FormatTime(time) + " " + separator + " ";

            //  act
            string actual = PMath.GetExecutionTime(true, separator);

            //  assert
            Assert.IsTrue(expected.Equals(actual), TestHelper.ShowVariables(expected, actual));
        }

        #endregion

        //  Complete
        #region Math

        [Test]
        public void Scientific_Works()
        {
            //  arrange

            //  Earth mass (kg) 5.97237f x 10^24
            double coefficient = 5.97237;
            int exponent = 24;
            double actual = coefficient * Mathd.Pow(10, exponent);

            //  act
            double result = PMath.Scientific(coefficient, exponent);

            //  assert
            Assert.AreEqual(actual, result);
        }

        [Test]
        public void Normalize_Works()
        {
            //  arrange
            int runsize = 1000;
            int min = 23;
            int max = 36;
            float[] data = new float[runsize];

            //  act
            for (int i = 0; i < runsize; i++)
            {
                data[i] = Random.Range(min, max);
            }
            data = data.Normalize();

            //  assert
            Assert.IsTrue(0 <= Mathf.Min(data), "min is below 0");
            Assert.IsTrue(1 >= Mathf.Max(data), "max is above 1");
        }

        [Test]
        public void IsPositive_Works()
        {
            //  arrange
            float fpos = 1f;
            float fneg = -1f;
            int ipos = 2;
            int ineg = -2;

            //  act

            //  assert
            Assert.IsTrue(fpos.IsPositive());
            Assert.IsFalse(fneg.IsPositive());
            Assert.IsTrue(ipos.IsPositive());
            Assert.IsFalse(ineg.IsPositive());
        }

        [Test]
        public void IsNegative_Works()
        {
            //  arrange
            float fpos = 1f;
            float fneg = -1f;
            int ipos = 2;
            int ineg = -2;

            //  act

            //  assert
            Assert.IsFalse(fpos.IsNegative());
            Assert.IsTrue(fneg.IsNegative());
            Assert.IsFalse(ipos.IsNegative());
            Assert.IsTrue(ineg.IsNegative());
        }

        [Test]
        public void WeightedSumArray_Works()
        {
            //  prerequisite
            Approx_Works();

            //  arrange
            float[] array = new float[] { 0, 1.1f, 2.2f, 3.3f, 4.4f, 5.5f };
            float expected;

            //  act
            expected = 1.1f + 2 * 2.2f + 3 * 3.3f + 4 * 4.4f + 5 * 5.5f;
            float actual = array.WeightedSum();

            //  assert
            Assert.IsTrue(PMath.Approx(expected, actual), TestHelper.ShowVariables(expected, actual));
        }

        [Test]
        public void WeightedSumList_Works()
        {
            //  prerequisite
            Approx_Works();

            //  arrange
            List<float> source = new List<float> { 0, 1.1f, 2.2f, 3.3f, 4.4f, 5.5f };
            float expected;

            //  act
            expected = 1.1f + 2 * 2.2f + 3 * 3.3f + 4 * 4.4f + 5 * 5.5f;
            float actual = source.WeightedSum();

            //  assert
            Assert.IsTrue(PMath.Approx(expected, actual), TestHelper.ShowVariables(expected, actual));
        }

        [Test]
        public void Approx_Works()
        {
            //	arrange
            float left = 3.12341f;
            float right = 3.12345f;

            //	act
            bool approx = PMath.Approx(left, right, 4);

            //	assert
            Assert.IsTrue(approx);
        }

        [Test]
        public void Factorial_Works()
        {
            //  arrange
            int n = 5;
            int zero = 0;

            //  act
            int f = PMath.Factorial(n);
            int z = PMath.Factorial(zero);

            //  assert
            Assert.AreEqual(5 * 4 * 3 * 2, f);
            Assert.AreEqual(1, z);
        }

        [Test]
        public void Map_ReturnsExpandedValue()
        {
            //  arrange
            float r = 0.9f; // scale 0,1 -> scale 0,10
            float g = 5; // scale 1,10 -> scale 1,100
            float b = 2; // scale -10,0 -> scale 1,10

            //  act
            r = PMath.Map(r, 0, 1, 0, 10);
            g = PMath.Map(g, 0, 10, 0, 100);
            b = PMath.Map(b, 0, 3, 0, 9);

            //  assert
            Assert.AreEqual(9, r, "r: " + TestHelper.ShowVariables(9, r));
            Assert.AreEqual(50, g, "g: " + TestHelper.ShowVariables(50, g));
            Assert.AreEqual(6, b, "b: " + TestHelper.ShowVariables(6, b));
        }

        [Test]
        public void Map_ReturnsCompressedValue()
        {
            //	prereq
            Approx_Works();

            //  arrange
            float r = 1;
            float g = 5;
            float b = 7;

            //  act
            r = PMath.Map(r, 0, 1, 0, 0.5f);	// scale 0,1 -> scale 0,0.5
            g = PMath.Map(g, 1, 10, 0.1f, 1);	// scale 1,10 -> scale 0.1,1
            b = PMath.Map(b, 1, 10, 0.5f, 5);	// scale -10,0 -> scale 1,5

            //  assert
            Assert.IsTrue(PMath.Approx(0.5f, r, 1), "r: " + TestHelper.ShowVariables(0.5f, r));
            Assert.IsTrue(PMath.Approx(0.5f, g, 1), "g: " + TestHelper.ShowVariables(0.5f, g));
            Assert.IsTrue(PMath.Approx(3.5f, b, 1), "b: " + TestHelper.ShowVariables(3.5f, b));
        }

        [Test]
        public void Map_ReturnsShiftedValue()
        {
            //  arrange
            float r = 1; // scale 0,1 -> scale 3,4
            float g = 5; // scale 1,10 -> scale 11,20
            float b = -7; // scale -10,0 -> scale -5,5

            //  act
            r = PMath.Map(r, 0, 1, 3, 4);
            g = PMath.Map(g, 1, 10, 11, 20);
            b = PMath.Map(b, -10, 0, -5, 5);

            //  assert
            Assert.AreEqual(4, r);
            Assert.AreEqual(15, g);
            Assert.AreEqual(-2, b);
        }

        [Test]
        public void Map_IgnoresIncorrectMinMaxOrder()
        {
            //  arrange
            float r = 0.65f; // scale 0,1 -> scale 4,3
            float g = 7.5f; // scale 10,5 -> scale 0,1
            float b = 7; // scale -10,0 -> scale 10,0

            //  act
            r = PMath.Map(r, 0, 1, 4, 3);
            g = PMath.Map(g, 10, 5, 0, 1);
            b = PMath.Map(b, 10, 0, 1, 0);

            //  assert
            Assert.AreEqual(3.65f, r);
            Assert.AreEqual(0.5f, g);
            Assert.AreEqual(0.7f, b);
        }

        [Test]
        public void Map_ConvertsNegativeRanges()
        {
            //  arrange
            float r = -1; // scale 0,1 -> scale 4,3
            float g = -7.5f; // scale 10,5 -> scale 0,1
            float b = -7; // scale -10,0 -> scale 10,0

            //  act
            r = PMath.Map(r, -10, 0, 0, 10);
            g = PMath.Map(g, -10, -5, 0, 10);
            b = PMath.Map(b, -7, 3, 1, 10);

            //	assert
            Assert.AreEqual(9, r);
            Assert.AreEqual(5, g);
            Assert.AreEqual(1, b);

        }

        [Test]
        public void Map_ClampsOutOfBoundValues()
        {
            //  arrange
            float r = 1; // scale 0,1 -> scale 4,3
            float g = 5; // scale 10,5 -> scale 0,1
            float b = -7; // scale -10,0 -> scale 10,0

            //  act
            r = PMath.Map(r, 0, 0.5f, 3, 5);
            g = PMath.Map(g, 0, 1, 3, 9);
            b = PMath.Map(b, 0, 15, 2, 99);

            //	assert
            Assert.AreEqual(5, r);
            Assert.AreEqual(9, g);
            Assert.AreEqual(2, b);
        }

        [Test]
        public void IndexOfMin_ReturnsCorrect()
        {
            //	arrange
            float[] source = new float[] { 2, 3, -4, 7 };
            int index;

            //	act
            index = source.IndexOfMin();

            //	assert
            Assert.AreEqual(2, index);
        }

        [Test]
        public void IndexOfMax_ReturnsCorrect()
        {
            //	arrange
            float[] source = new float[] { 2, 3, -4, 7 };
            int index;

            //	act
            index = source.IndexOfMax();

            //	assert
            Assert.AreEqual(3, index);
        }

        [Test]
        public void RemoveMin_ReturnsMinValueAndRemovesItFromList()
        {
            //  arrange
            float min = 0.1f;
            List<float> source = new List<float> { min * 2, min, min * 3 };

            //  act
            float removed = source.RemoveMin();

            //  assert
            Assert.AreEqual(min, removed);
            Assert.AreEqual(2, source.Count);
        }

        [Test]
        public void RemoveMax_ReturnsMaxValueAndRemovesItFromList()
        {
            //  arrange
            float max = 0.1f;
            List<float> source = new List<float> { max * 0.5f, max, max * 0.25f };

            //  act
            float removed = source.RemoveMax();

            //  assert
            Assert.AreEqual(max, removed);
            Assert.AreEqual(2, source.Count);
        }

        [Test]
        public void IsBetween_ReturnsCorrectAnswer()
        {
            //  arrange
            float value = 99;
            float min = 98, max = 100;

            //  act
            bool passA = value.IsBetween(min, max);
            bool passB = value.IsBetween(max, min);
            bool failA = value.IsBetween(value + min, value + max);
            bool failB = value.IsBetween(value - min, value - max);

            //  boundary checking
            value = min;
            bool leftDefault = value.IsBetween(min, max);
            bool leftPass = value.IsBetween(min, max, true);
            bool leftFail = value.IsBetween(min, max, false);
            value = max;
            bool rightDefault = value.IsBetween(min, max);
            bool rightPass = value.IsBetween(min, max, true, true);
            bool rightFail = value.IsBetween(min, max, true, false);

            //  assert
            Assert.IsTrue(passA);
            Assert.IsTrue(passB);
            Assert.IsTrue(leftDefault);
            Assert.IsTrue(leftPass);
            Assert.IsTrue(rightDefault);
            Assert.IsTrue(rightPass);
            Assert.IsFalse(failA);
            Assert.IsFalse(failB);
            Assert.IsFalse(leftFail);
            Assert.IsFalse(rightFail);
        }

        [Test]
        public void BitIsOn_ReturnIndicatesBitState()
        {
            //  arrange
            int number = 1;

            //  act
            bool first = number.BitIsOn(0);
            bool last = number.BitIsOn(8);

            //  assert
            Assert.IsTrue(first);
            Assert.IsFalse(last);
        }

        [Test]
        public void SetBit_ReturnsAdjustedNumber()
        {
            //  arrange
            int number = 1;

            //  act
            int adjusted = number;
            adjusted = PMath.SetBitFor(adjusted, 1, true);

            //  assert
            Assert.AreEqual(3, adjusted);
        }

        [Test]
        public void SignsMatch_ReturnsCorrectly()
        {
            //  arrange
            float x = -1.3f;
            int y = -7;
            int z = 321;

            //  act
            bool match = PMath.SignMatches(x, y);
            bool notMatch = PMath.SignMatches(x, z);

            //  assert
            Assert.IsTrue(match);
            Assert.IsFalse(notMatch);
        }

        [Test]
        public void Sum_ReturnsSumOfArray()
        {
            //  prerequisite
            Approx_Works();

            //  arrange
            int[] iArr = new int[] { 0, 1, 2, 3 };
            float[] fArr = new float[] { 0.1f, 1.1f, 2.1f, 3.1f };
            List<float> fList = new List<float>() { 0.1f, 1.1f, 2.1f, 3.1f };
            List<int> iList = new List<int>() { 0, 1, 2, 3 };

            //  act
            int iaTotal = iArr.Sum();
            float faTotal = fArr.Sum();
            float flTotal = fList.Sum();
            int ilTotal = iList.Sum();

            //  assert
            Assert.IsTrue(PMath.Approx(6, iaTotal));
            Assert.IsTrue(PMath.Approx(6.4f, faTotal));
            Assert.IsTrue(PMath.Approx(6.4f, flTotal));
            Assert.IsTrue(PMath.Approx(6, ilTotal));
        }

        [Test]
        public void Average_ReturnsAverageOfArray()
        {
            //  prerequisite
            Approx_Works();

            //  arrange
            int[] iArr = new int[] { 0, 1, 2, 3 };
            float[] fArr = new float[] { 0.1f, 1.1f, 2.1f, 3.1f };
            List<float> lArr = new List<float>() { 0.1f, 1.1f, 2.1f, 3.1f };

            //  act
            float iTotal = iArr.Average();
            float fTotal = fArr.Average();
            float lTotal = lArr.Average();

            //  assert
            Assert.IsTrue(PMath.Approx(6f / 4, iTotal));
            Assert.IsTrue(PMath.Approx(6.4f / 4, fTotal));
            Assert.IsTrue(PMath.Approx(6.4f / 4, lTotal));
        }

        [Test]
        public void Median_ReturnsCorrectly()
        {

            //	arrange
            float[] odd = new float[] { 1, 3, 5, 6, 7, 8, 9, 10, 10 };
            float expectedOddMedian = 7f;

            float[] even = new float[] { 9, 8, 7, 4, 3, 2, 2, 1 };
            float expectedEvenMedian = 3.5f;

            float[] one = new float[] { 99 };
            float expectedOne = 99;

            float[] none = new float[] { };
            float expectedNone = Mathf.NegativeInfinity;

            //	act
            float noneMedian = none.Median();
            float oneMedian = one.Median();
            float oddMedian = odd.Median();
            float evenMedian = even.Median();

            //	assert
            Assert.AreEqual(expectedNone, noneMedian, "wrong median for empty array " + noneMedian);
            Assert.AreEqual(expectedOne, oneMedian, "wrong median for single number " + oneMedian);
            Assert.AreEqual(expectedOddMedian, oddMedian, "wrong median for odd size " + oddMedian);
            Assert.AreEqual(expectedEvenMedian, evenMedian, "wrong median for even size " + evenMedian);
        }

        [Test]
        public void Mode_FloatReturnsCorrectly()
        {
            //	arrange
            float[] odd = new float[] { 10, 3, 5, 6, 7, 8, 9, 10, 10 };
            float expectedOddMode = 10f;

            float[] even = new float[] { 9, 8, 7, 4, 3, 2, 2, 1 };
            float expectedEvenMode = 2f;

            float[] one = new float[] { 99 };
            float expectedOne = 99;

            float[] none = new float[] { };

            //	act
            float oneMode = one.Mode();
            float oddMode = odd.Mode();
            float evenMode = even.Mode();

            //	assert
            Assert.Throws(typeof(System.ArgumentException), delegate { none.Mode(); });
            Assert.AreEqual(expectedOne, oneMode, "wrong mode for single number " + oneMode);
            Assert.AreEqual(expectedOddMode, oddMode, "wrong mode for odd size " + oddMode);
            Assert.AreEqual(expectedEvenMode, evenMode, "wrong mode for even size " + evenMode);
        }

        [Test]
        public void Mode_IntReturnsCorrectly()
        {
            //	arrange
            int[] odd = new int[] { 10, 3, 5, 6, 7, 8, 9, 10, 10 };
            int expectedOddMode = 10;

            int[] even = new int[] { 9, 8, 7, 4, 3, 2, 2, 1 };
            int expectedEvenMode = 2;

            int[] one = new int[] { 99 };
            int expectedOne = 99;

            int[] none = new int[] { };

            int oneMode = one.Mode();
            int oddMode = odd.Mode();
            int evenMode = even.Mode();

            //	assert
            Assert.Throws(typeof(System.ArgumentException), delegate { none.Mode(); });
            Assert.AreEqual(expectedOne, oneMode, "wrong mode for single number " + oneMode);
            Assert.AreEqual(expectedOddMode, oddMode, "wrong mode for odd size " + oddMode);
            Assert.AreEqual(expectedEvenMode, evenMode, "wrong mode for even size " + evenMode);
        }

        [Test]
        public void Trunc_ReturnsTruncatedNumber()
        {
            //  arrange
            float number = 123.456f;

            //  act
            float trunc = PMath.Trunc(number);
            float trunc1 = PMath.Trunc(number, 1);

            //  assert
            Assert.AreEqual(123f, trunc);
            Assert.AreEqual(123.4f, trunc1);
        }

        [Test]
        public void Remainder_ReturnsRemainderNumber()
        {
            //	prereq
            Approx_Works();

            //  arrange
            float number = 123.456f;

            //  act
            float remainder = PMath.Remainder(number);

            //  assert
            Assert.IsTrue(PMath.Approx(0.456f, remainder, 5), TestHelper.ShowVariables(0.456f, remainder));
        }

        [Test]
        public void IsQuadraticSolvable_Answer()
        {
            //  arrange
            float a = 5;
            float b = 3;
            float c = 7;

            //  act
            bool notSolvable = PMath.IsQuadraticSolvable(a, b, c);
            bool yesSolvable = PMath.IsQuadraticSolvable(a, b * 100, c);

            //  assert
            Assert.IsFalse(notSolvable);
            Assert.IsTrue(yesSolvable);
        }

        [Test]
        public void SolveQuadratic_SolutionXvalues()
        {
            //  arrange
            //  find x for x^2 + x -12
            //  answer: (x+4)(x-3) = x is -4, x is 3
            float a = 1;
            float b = 1;
            float c = -12;

            //  act
            Vector2 solution = PMath.SolveQuadratic(a, b, c);

            //  assert
            Assert.AreEqual(-4, solution.x);
            Assert.AreEqual(3, solution.y);
        }

        [Test]
        public void ClampMin_ReturnsBoundedNumber()
        {
            //  arrange
            int number = 7;

            //  act
            int bounded = PMath.ClampMin(number, 10);

            //  assert
            Assert.AreEqual(10, bounded);
        }

        [Test]
        public void ClampMax_ReturnsBoundedNumber()
        {
            //  arrange
            int number = 7;

            //  act
            int bounded = PMath.ClampMax(number, 5);

            //  assert
            Assert.AreEqual(5, bounded);
        }

        [Test]
        public void Round_ReturnsRoundedNumber()
        {
            //  arrange
            float a = 77.123f;
            float b = 99.987f;
            float c = 88.34567f;

            //  act
            a = PMath.Round(a);
            b = PMath.Round(b);
            c = PMath.Round(c, 3);

            //  assert
            Assert.AreEqual(77, a);
            Assert.AreEqual(100, b);
            Assert.AreEqual(88.346f, c);
        }

        #endregion

        //  Complete
        #region Statistics

        [Test]
        public void Var_ReturnsCorrect()
        {
            //	prereq
            Approx_Works();

            //  arrange
            //  format is { range, stdDev }
            float[,] expected = new float[,] {
                {1,0},{2,0.5f},{7,2.0f},{26,7.5f},
                {97,28.0f},{362,104.5f}
            };
            int dataSets = expected.GetLength(0);
            float[][] samples = new float[dataSets][];

            //  build the data sets
            for (int i = 0; i < dataSets; i++)
            {
                int setSize = (int)expected[i, 0];
                float[] set = new float[setSize];
                for (int j = 0; j < setSize; j++)
                {
                    set[j] = j + 1;
                }
                samples[i] = set;
            }

            //  act
            float[] actualSd = new float[dataSets];
            for (int i = 0; i < dataSets; i++)
            {
                actualSd[i] = PMath.Var(samples[i]);
            }

            //  assert
            for (int i = 0; i < dataSets; i++)
            {
                float expect = expected[i, 1];
                float actual = Mathf.Sqrt(actualSd[i]);
                Assert.AreEqual(expected[i, 0], samples[i].Length);
                Assert.IsTrue(PMath.Approx(expect, actual, 1), i + ": " + TestHelper.ShowVariables(expect, actual));
            }
        }

        /// <summary>
        /// Sample data generated using Excell and the
        /// STDDEV.P() function, generating the SD for
        /// each set of numbers, from 1-1 through 1-1000,
        /// and then selecting the ranges that have easy SD to enter.
        /// </summary>
        [Test]
        public void StdDev_ReturnsCorrect()
        {
            //  arrange
            //  format is { range, stdDev }
            float[,] expected = new float[,] {
                {1,0},{2,0.5f},{7,2.0f},{26,7.5f},
                {97,28.0f},{362,104.5f}
            };
            int dataSets = expected.GetLength(0);
            float[][] samples = new float[dataSets][];

            //  build the data sets
            for (int i = 0; i < dataSets; i++)
            {
                int setSize = (int)expected[i, 0];
                float[] set = new float[setSize];
                for (int j = 1; j < setSize; j++)
                {
                    set[j - 1] = j;
                }
                samples[i] = set;
            }

            //  act
            float[] actualSd = new float[dataSets];
            for (int i = 0; i < dataSets; i++)
            {
                actualSd[i] = PMath.StdDev(samples[i]);
            }

            //  assert
            for (int i = 0; i < dataSets; i++)
            {
                Assert.AreEqual(expected[i, 1], actualSd[i]);
            }
        }

        [Test]
        public void Residuals_ReturnsCorrect()
        {
            //	prereq
            Average_ReturnsAverageOfArray();

            //  arrange
            float[] data = new float[] { 2, 4, 6 };
            float mean = data.Average();
            float[] actual = new float[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                actual[i] = data[i] - mean;
            }

            //  act
            float[] resi = PMath.Residuals(data);

            //  assert
            for (int i = 0; i < data.Length; i++)
            {
                Assert.AreEqual(actual[i], resi[i], i + ":" + TestHelper.ShowVariables(actual[i], resi[i]));
            }
        }

        #endregion

        //  Complete
        #region Conversion

        [Test]
        public void ToVector2_ReturnsConvertedVector()
        {
            //  arrange
            Vector3 v3 = new Vector3(4, 5, 6);
            Vector2 expected = new Vector2(4, 6);

            //  act
            Vector2 v2 = v3.ToVector2();

            //  assert
            Assert.AreEqual(expected, v2);
        }

        [Test]
        public void ToVector3_ReturnsConvertedVector()
        {
            //  arrange
            Vector2 v2 = new Vector2(5, 6);
            Vector3 expected = new Vector3(5, 0, 6);

            //  act
            Vector3 v3 = v2.ToVector3();

            //  assert
            Assert.AreEqual(expected, v3);
        }

        [Test]
        public void ToVector3Array_ReturnsCorrectly()
        {
            //  arrange
            int runsize = 1000;
            ContactPoint[] contacts = new ContactPoint[runsize];
            for (int i = 0; i < runsize; i++) contacts[i] = new ContactPoint();

            //  act
            Vector3[] converted = contacts.ToVector3();

            //  assert
            Assert.AreEqual(contacts.Length, converted.Length);
        }

        #endregion

        //  Incomplete (almost done)
        #region Physics

        [Test]
        public void GravitationalAcceleration_FloatDataReturnsCorrectValue()
        {
            //  prereq
            Approx_Works();
            Scientific_Works();

            //  arrange
            float massA = 10;   //  kg
            float massB = 20;   //  kg
            float distance = 5 * 1000; //  km => meters
            int precision = 5;

            //  act
            float gravity = PMath.GravitationalAcceleration(massA, massB, distance);

            //  assert
            Assert.IsTrue(PMath.Approx(0f, gravity, precision), "Gravitational Acceleration: " + TestHelper.ShowNEComparison(0f, gravity));
        }

        [Test]
        public void GravitationalAcceleration_DoubleDataReturnsCorrectValue()
        {
            //  prereq
            Approx_Works();
            Scientific_Works();

            //  arrange

            //  Earth radius (meters) 6.371 x 10^6
            double distance = PMath.Scientific(6.371, 6);

            //  kg
            double massA = 68;

            //  Earth mass (kg) 5.97237f x 10^24
            double massB = PMath.Scientific(5.97237, 24);

            //  m/s/s
            double expected = 9.80665d;

            //  act
            double gravity = PMath.GravitationalAcceleration(massA, massB, distance);

            //  assert
            Assert.AreEqual(expected, gravity, "Gravitational Force: {0} != {1}", expected, gravity);
        }

        [Test]
        public void EscapeVelocity_ReturnsCorrectValue()
        {
            //  prereq
            Approx_Works();
            Scientific_Works();
            GravitationalAcceleration_FloatDataReturnsCorrectValue();

            //  arrange
            //  mean Earth radius (km => meters)
            float distance = (float) PMath.Scientific(6.371, 6);
            //  typical person mass (kg)
            float personMass = 70;
            //  Earth mass (kg)
            //  Ref: https://en.wikipedia.org/wiki/Earth_mass
            double earthMass = PMath.Scientific(5.97237, 24);

            //  force is m/s/s
            float force = PMath.GravitationalAcceleration(personMass, (float)earthMass, distance);

            //  escape velocity on earth is 11.2 km/s (km/s => m/s)
            float expected = 11.2f * 1000;

            //  act (m/s)
            double velocity = PMath.EscapeVelocity(force, distance);

            //  assert
            Assert.IsTrue(PMath.Approx(expected, (float)velocity, 1), "Escape Velocity: expected "
                + expected + " m/s != actual " + velocity + " m/s");
        }

        [Test]
        public void GetMassOfSphere_ReturnsCorrectValue()
        {
            //  arrange
            float radius = 10;
            float density = 1.5f;

            //  act
            float mass = PMath.GetMassOfSphere(radius, density);

            //  assert
            Assert.AreEqual(Mathf.PI * Mathf.Pow(radius, 3) * density, mass);
        }

        [Test]
        public void ImpactForceInNewtons_ReturnsCorrectValue()
        {
            //  prereq
            Approx_Works();

            //  arrange
            float mass = 10;         //  kg
            float speed = 50;        //  m/s
            float lossRatio = 0.5f;  //  50% speed energy loss
            float deltaTime = 1f / 60;

            //  act
            float force = PMath.ImpactForceInNewtons(mass, speed, lossRatio, deltaTime);
            float expected = mass * speed * lossRatio / deltaTime;

            //  assert
            Assert.IsTrue(PMath.Approx(expected, force), TestHelper.ShowVariables(expected, force));
        }

        [Test]
        public void AngleFromGround_ReturnsCorrectAngle()
        {
            //  arrange
            Vector3 direction = new Vector3(1, 1, 0);

            //  act
            float angle = PMath.AngleFromGround(direction);

            //  assert
            Assert.AreEqual(45 * Mathf.Deg2Rad, angle);
        }

        /// <summary>
        /// Ref: Range = V0^2 * sin (2 * theta) / g => V0^2 = Range * g / (sin (2 * theta))
        /// Ref: http://physics.stackexchange.com/questions/27992/solving-for-initial-velocity-required-to-launch-a-projectile-to-a-given-destinat
        /// initial speed = [ 1/cos(angle) ] * sqrt( (0.5 * g * distance^2) / (distance * tan(angle) + height) )
        /// Note that "Distance" is the straight line distance from initial to final position (not just the horizontal distance)
        /// Sample values calculated from this site: http://keisan.casio.com/exec/system/1225100367
        /// </summary>
        [Test]
        public void VelocityOfReach_ReturnsCorrectValue()
        {
            //  prereq
            Approx_Works();
            AngleOfReach_ReturnsCorrectAngles();

            //  arrange        
            float expectedSpeed = 30;
            float targetHeight = -20;
            float distance = 90;

            //	angles (confirmed correct)
            float lowAngleDeg = 17.63152f;
            float highAngleDeg = 59.83967f;
            float lowAngleRad = lowAngleDeg * Mathf.Deg2Rad;
            float highAngleRad = highAngleDeg * Mathf.Deg2Rad;

            //  act
            float lowAngleSpeed = PMath.VelocityOfReach(lowAngleRad, distance, targetHeight);
            float highAngleSpeed = PMath.VelocityOfReach(highAngleRad, distance, targetHeight);

            //  assert
            string message = "low angle / speed: {0} / {1}, high angle / speed: {2} / {3}";
            string output = string.Format(message, lowAngleDeg, lowAngleSpeed, highAngleDeg, highAngleSpeed);
            Debug.Log(output);
            Assert.IsTrue(PMath.Approx(expectedSpeed, lowAngleSpeed, 1), "low angle: " + TestHelper.ShowVariables(expectedSpeed, lowAngleSpeed));
            Assert.IsTrue(PMath.Approx(expectedSpeed, highAngleSpeed, 1), TestHelper.ShowVariable("high angle: ", highAngleSpeed));
        }

        [Test]
        public void VelocityOfReach_UsingVectorsReturnsCorrectValue()
        {
            //	prereq
            Approx_Works();
            AngleOfReach_ReturnsCorrectAngles();

            //	arrange
            float distance = 20;
            float targetHeight = -30;
            Vector3 source = Vector3.zero;
            Vector3 target = Vector3.down * targetHeight + Vector3.forward * distance;
            float launchAngle = 17.63152f;	//	deg
            float expected = 30;

            //	act
            float speed = PMath.VelocityOfReach(launchAngle * Mathf.Deg2Rad, source, target);

            //	assert
            Assert.IsTrue(PMath.Approx(expected, speed, 5), TestHelper.ShowVariables(expected, speed));
            throw new System.NotImplementedException();
        }

        [Test]
        public void AngleOfReachDiscriminant_ReturnsCorrect()
        {

            //	Prereq
            Approx_Works();

            //	arrange
            float speed = 30;
            float targetHeight = -20;
            float distance = 89.709227069098f;
            float expected = 388676.3f;

            //	act
            float disc = PMath.AngleOfReachDiscriminant(speed, distance, targetHeight);

            //	assert
            Assert.IsTrue(PMath.Approx(expected, disc, 1), TestHelper.ShowVariables(expected, disc));
        }

        [Test]
        public void AngleOfReachExists_ReturnsCorrectValue()
        {
            //  arrange
            float speed = 30;
            float targetHeight = -20;
            float distance = 89.709227069098f;

            //  act
            bool exists = PMath.AngleOfReachExists(speed, distance, targetHeight);

            //  assert
            Assert.IsTrue(exists);
        }

        /// <summary>
        /// Refactor PMath.LaunchAngleForBallisticFlight() to AngleOfReach()
        /// To hit a target at range x and altitude y when fired from (0,0) and with initial speed v the required
        /// angle(s) of launch \theta are:
        ///     \theta = \arctan{\left(\frac{v^2\pm\sqrt{v^4-g(gx^2+2yv^2)}}{gx}\right)} 
        /// The two roots of the equation correspond to the two possible launch angles, so long as they aren't 
        /// imaginary, in which case the initial speed is not great enough to reach the point (x,y) selected. 
        /// This formula allows one to find the angle of launch needed without the restriction of y = 0.
        /// Ref: https://en.wikipedia.org/wiki/Trajectory_of_a_projectile
        /// </summary>
        [Test]
        public void AngleOfReach_ReturnsCorrectAngles()
        {
            //  prereq
            Approx_Works();

            //  arrange
            float speed = 30;
            float angleLow = 17.63152f;		//	deg
            float angleHigh = 59.83967f;	//	deg
            float targetHeight = -20;
            float distance = 90;

            //  act
            float[] angles = PMath.AngleOfReach(speed, distance, targetHeight);
            angles[0] *= Mathf.Rad2Deg;
            angles[1] *= Mathf.Rad2Deg;

            //  assert
            Assert.IsTrue(PMath.Approx(angleLow, angles[0], 5), "[0]: " + TestHelper.ShowVariables(angleLow, angles[0]));
            Assert.IsTrue(PMath.Approx(angleHigh, angles[1], 5), "[1]: " + TestHelper.ShowVariables(angleHigh, angles[1]));
        }

        [Test]
        public void AngleOfReach_ReturnsCorrectResponseWhenNoAnglesExist()
        {
            //  arrange
            float speed = 5;
            float targetHeight = -20;
            float distance = 89.709227069098f;

            //  act
            float[] angles = PMath.AngleOfReach(speed, distance, targetHeight);

            //  assert
            Assert.AreEqual(Mathf.NegativeInfinity, angles[0]);
            Assert.AreEqual(Mathf.NegativeInfinity, angles[1]);
        }

        #endregion

        //  Incomplete (Vector3.Approx() remains)
        #region Vectors

        [Test]
        public void ProjectOntoPlane_ReturnsCorrectly()
        {
            //  arrange
            Vector3 sample = Vector3.forward + Vector3.up;

            //  act
            Vector3 resultF = PMath.ProjectOntoPlane(Vector3.up, sample);
            Vector3 resultU = PMath.ProjectOntoPlane(Vector3.forward, sample);
            Vector3 resultR = PMath.ProjectOntoPlane(Vector3.right, sample);

            //  assert
            Assert.AreEqual(Vector3.forward, resultF, "x projection failed" + TestHelper.ShowVariables(Vector3.forward, resultF));
            Assert.AreEqual(Vector3.up, resultU, "y projection failed" + TestHelper.ShowVariables(Vector3.up, resultU));
            Assert.AreEqual(sample, resultR, "z projection failed" + TestHelper.ShowVariables(sample, resultR));
        }

        [Test]
        public void Abs_V2ReturnsCorrectly()
        {
            //  arrange
            Vector2 v = new Vector2(-3, 1);
            Vector2 expected = new Vector2(
                Mathf.Abs(v.x), Mathf.Abs(v.y));

            //  act
            v = PMath.Abs(v);

            //  assert
            Assert.AreEqual(expected, v);
            Assert.AreEqual(expected.x, v.x);
            Assert.AreEqual(expected.y, v.y);
        }

        [Test]
        public void Abs_V3ReturnsCorrectly()
        {
            //  arrange
            Vector3 v = new Vector3(-3, 1, -8);
            Vector3 expected = new Vector3(
                Mathf.Abs(v.x),
                Mathf.Abs(v.y),
                Mathf.Abs(v.z));

            //  act
            v = PMath.Abs(v);

            //  assert
            Assert.AreEqual(expected.x, v.x);
            Assert.AreEqual(expected.y, v.y);
            Assert.AreEqual(expected.z, v.z);
        }

        [Test]
        public void LargestComponentVector_ReturnsComponent()
        {
            //  arrange
            Vector3 v3 = new Vector3(1, 3, 2);
            Vector2 v2 = new Vector2(5, 6);

            //  act
            Vector3 component3 = v3.LargestComponentVector();
            Vector2 component2 = v2.LargestComponentVector();

            //  assert
            Assert.AreEqual(Vector3.up * 3, component3);
            Assert.AreEqual(Vector2.up * 6, component2);
        }

        [Test]
        public void SmallestComponentVector_ReturnsComponent()
        {
            //  arrange
            Vector3 v3 = new Vector3(1, 3, 2);
            Vector2 v2 = new Vector2(5, 6);

            //  act
            Vector3 component3 = v3.SmallestComponentVector();
            Vector2 component2 = v2.SmallestComponentVector();

            //  assert
            Assert.AreEqual(Vector3.right * 1, component3);
            Assert.AreEqual(Vector2.right * 5, component2);
        }

        [Test]
        public void GetEndpointAtDistance_ReturnsEndpoint()
        {
            //  arrange
            Vector3 position = new Vector3(0, 5, 10);
            Vector3 direction = new Vector3(1, 0, 0);
            float distance = 1024;

            //  act
            Vector3 endpoint = PMath.GetEndpointAtDistance(position, direction, distance);

            //  assert
            Assert.AreEqual(new Vector3(1024, 5, 10), endpoint);
        }

        [Test]
        public void GetOffscreenEndpoint_ReturnsEndpoint()
        {
            //  prereq
            GetEndpointAtDistance_ReturnsEndpoint();
            UtilTest.UtilTest util = new UtilTest.UtilTest();
            util.IsOnScreen_ReturnsCorrectly();

            //  clean up
            TestHelper.ResetAllCameras();

            //  arrange
            GameObject gameObject = new GameObject();
            gameObject.AddComponent(typeof(Camera));
            Camera cam = gameObject.GetComponent<Camera>();

            cam.transform.position = Vector3.back * 10;
            cam.transform.forward = Vector3.forward;

            //  act
            Vector3 endpointImplied = PMath.GetOffscreenEndpoint(Vector3.zero, Vector3.right);
            Vector3 endpointExplicit = PMath.GetOffscreenEndpoint(Vector3.zero, Vector3.right, cam);

            //  assert
            bool onScreenImp = PLib.General.PUtil.IsOnScreen(endpointImplied, cam);
            bool onScreenExp = PLib.General.PUtil.IsOnScreen(endpointExplicit, cam);
            Assert.IsFalse(onScreenImp, "endpoint: " + endpointImplied.ToString());
            Assert.IsFalse(onScreenExp, "endpoint: " + endpointExplicit.ToString());
        }

        [Test]
        public void CollisionSurfaceTangent_ReturnsComponent()
        {
            //  arrange
            //  45 degree angle (down and to right) impacts
            //  the XZ horizontal plane

            Vector3 impactDirection = new Vector3(1, -1, 0).normalized;
            Vector3 surfaceNormal = Vector3.up;
            Vector3 expected = Vector3.right;

            //  act
            Vector3 tangent = PMath.CollisionSurfaceTangent(impactDirection, surfaceNormal);

            //  assert
            Assert.AreEqual(expected, tangent, TestHelper.ShowVariables(expected, tangent));
        }

        [Test]
        public void SnapToAxis_ReturnsAdjustedVector()
        {
            //  arrange
            Vector3 v = new Vector3(3.2f, 4.51f, 7.1f);
            Vector3 mask = new Vector3(0, 1, 1);

            //  act
            v = PMath.SnapToAxis(v, mask);
            Vector3 expected = new Vector3(v.x, 5, 7);

            //  assert
            Assert.AreEqual(expected, v);
        }

        [Test]
        public void MinComponentMagnitude_ExtensionReturnsCorrect()
        {
            //  arrange
            Vector3 v3 = new Vector3(1, 2, 3);
            Vector2 v2 = new Vector2(4, -5);

            //  act
            float min3 = v3.MinComponentMagnitude();
            float min2 = v2.MinComponentMagnitude();

            //  assert
            Assert.AreEqual(1f, min3);
            Assert.AreEqual(-5f, min2);
        }

        [Test]
        public void MaxComponentMagnitude_ExtensionReturnsCorrect()
        {
            //  arrange
            Vector3 v3 = new Vector3(1, 2, 3);
            Vector2 v2 = new Vector2(4, -5);

            //  act
            float max3 = v3.MaxComponentMagnitude();
            float max2 = v2.MaxComponentMagnitude();

            //  assert
            Assert.AreEqual(3f, max3);
            Assert.AreEqual(4f, max2);
        }

        [Test]
        public void ApproxVector3_Works()
        {
            //	arrange
            Vector3 left = Vector3.one*3.12341f;
            Vector3 right = Vector3.one * 3.12345f;

            //	act
            bool approx = PMath.Approx(left, right, 4);

            //	assert
            Assert.IsTrue(approx);
        }

        #endregion

        //  Complete
        #region Angles

        [Test]
        public void AngleSum3d_ReturnsCorrectly()
        {
            //  arrange
            Vector3[] polygon = new Vector3[] { Vector3.left, Vector3.up, Vector3.right, Vector3.down };
            Vector3 pointInside = Vector3.zero;
            Vector3 planarOutside = Vector3.up + Vector3.right;
            Vector3 extraplanar = Vector3.forward;

            //  act
            bool inside = Mathf.Approximately(Mathf.PI * 2, PMath.AngleSum3d(pointInside, polygon));
            bool outside = Mathf.Approximately(Mathf.PI * 2, PMath.AngleSum3d(planarOutside, polygon));
            bool nonPlanar = Mathf.Approximately(Mathf.PI * 2, PMath.AngleSum3d(extraplanar, polygon));

            //  assert
            Assert.IsTrue(inside);
            Assert.IsFalse(outside);
            Assert.IsFalse(nonPlanar);
        }

        [Test]
        public void AngleSum2d_ReturnsCorrectly()
        {
            //  arrange
            Vector2[] polygon = new Vector2[] { Vector2.left, Vector2.up, Vector2.right, Vector2.down };
            Vector2 pointInside = Vector2.zero;
            Vector2 planarOutside = Vector2.up + Vector2.right;

            //  act
            bool inside = Mathf.Approximately(Mathf.PI * 2, PMath.AngleSum2d(pointInside, polygon));
            bool outside = Mathf.Approximately(Mathf.PI * 2, PMath.AngleSum2d(planarOutside, polygon));

            //  assert
            Assert.IsTrue(inside);
            Assert.IsFalse(outside);
        }

        [Test]
        public void SignedAngleFromCOS_ReturnsAngle()
        {
            //  prereq
            Approx_Works();

            //  arrange
            float cos = 0.5f;

            //  act
            float angle = PMath.SignedAngleFromCOS(cos);

            //  assert
            Assert.IsTrue(PMath.Approx(60f, angle, 5), TestHelper.ShowVariables(60f, angle));
        }

        [Test]
        public void NormalizeAngleByDeg_ReturnsAngle()
        {
            //  arrange
            float angle = 361;

            //  act
            angle = PMath.NormalizeDegreeAngle(angle);

            //  assert
            Assert.AreEqual(1, angle);
        }

        [Test]
        public void NormalizeAngleByRad_ReturnsAngle()
        {
            //  prereq
            Approx_Works();

            //  arrange
            float angle = Mathf.PI * 3;

            //  act
            angle = PMath.NormalizeRadianAngle(angle);

            //  assert
            Assert.IsTrue(PMath.Approx(Mathf.PI, angle));
        }

        [Test]
        public void Deg2DotProduct_ReturnsAngle()
        {
            //  prereq
            Approx_Works();

            //  arrange
            float angle = 90;

            //  act
            float dot = PMath.DotD(angle);

            //  assert
            Assert.IsTrue(Mathf.Abs(0 - dot) < 0.0000001f, " 0 ~ " + dot);
            Assert.IsTrue(PMath.Approx(0, dot), " 0 ~ " + dot);
        }

        #endregion

    }
}