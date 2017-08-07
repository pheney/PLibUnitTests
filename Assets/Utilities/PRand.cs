using UnityEngine;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using PLib.Math;
using PLib.Logging;
using System.Linq;

namespace PLib.Rand
{
    /// <summary>
    /// 2018-1-6
    /// Randomization and probability
    ///	Converted to partial class.
    /// </summary>
    public static class PRand
    {
        /// <summary>
        /// Euler's Number 2.718281...
        /// System.Math.E return Euler's number
        /// UnityEngine.Mathf.Epsilon returns "the smallest non-zero float value"
        /// </summary>
        private static double E = System.Math.E;

        //////////////////////////////////////////////
        //	assigning a random seed, by game object	//
        //////////////////////////////////////////////

        #region Seeds

        private static Dictionary<int, Random.State> seedByHashCode = new Dictionary<int, Random.State>();

        /// <summary>
        /// 2017-8-2
        /// Removes all existing entries in the seed library. This should be called at
        /// the beginning of every game, otherwise the library will continue to grow
        /// indefinitely. It is unlikely this will cause a problem, but it is possible,
        /// e.g., if this is used in conjunction with non-recycled game objects such as
        /// bullets, then there will be a lot of seeds created.
        /// Returns the number of entries deleted.
        /// </summary>
        public static int FlushRandomSeeds()
        {
            int result = seedByHashCode.Keys.Count;
            seedByHashCode.Clear();
            return result;
        }

        /// <summary>
        /// 2016-3-17
        /// Retrieve a unique random seed that is specific to
        /// the hashcode parameter (usually based on the hashcode of a GameObject).
        /// Once a seed is created for a given hashcode, the seed never changes.
        /// </summary>
        public static Random.State GetSeedByHashCode(int hashCode)
        {
            if (!seedByHashCode.ContainsKey(hashCode))
            {
                System.DateTime now = System.DateTime.Now;
                int elapsedSecondsOfYear = now.Second;
                elapsedSecondsOfYear += now.Minute * 60;
                elapsedSecondsOfYear += now.Hour * 60 * 60;
                elapsedSecondsOfYear += now.DayOfYear * 24 * 60 * 60;
                int seed = elapsedSecondsOfYear + seedByHashCode.Count + hashCode;
                Random.InitState(seed);
                seedByHashCode.Add(hashCode, Random.state);
            }
            return seedByHashCode[hashCode];
        }

        /// <summary>
        /// The initial call creates, associates and then returns a randomiation seed number for the transform.
        /// Subsequent calls return the same seed value. This seed number can be passed to Random so that 
        /// randomization is repeatable.
        /// </summary>
        /// <returns>The random seed associated with the transform.</returns>
        public static Random.State GetSeedByTransform(this Transform transform)
        {
            return GetSeedByHashCode(transform.GetHashCode());
        }

        /// <summary>
        /// 2016-1-8
        /// Sets the randomization seed.
        /// </summary>
        /// <param name="seed">Seed.</param>
        public static void SetSeed(int seed)
        {
            Random.InitState(seed);
        }

        #endregion

        //////////////////////////////////////////////////
        //	variations on 'n' chances in 'something'	//
        //////////////////////////////////////////////////

        #region Chances

        /// <summary>
        /// Boolean randomizer.
        /// </summary>
        /// <returns><c>true</c>, if chance is less than the "percent of success", <c>false</c> otherwise.</returns>
        /// <param name="percentSuccess">The percent chance of success.</param>
        public static bool PercentChance(float percentSuccess)
        {
            return Random.value < Mathf.Clamp(percentSuccess, 0, 100);
        }

        /// <summary>
        /// 2016-10-12
        /// Returns the chance of something happening, given the rate at which it happens.
        /// For example, if something happens ON AVERAGE once every 100 years (lambda = 1),
        /// this returns the chance of it happening ONE TIME in 100 years.
        /// 
        /// If the optional parameter k is provided, this returns the chance that the
        /// event happens k times in the interval. For example, if something happens ON
        /// AVERAGE twice every 35 years (lambda = 2), and you want the chance of it happening
        /// three times (k = 3) in 35 years.
        /// 
        /// Ref: https://en.wikipedia.org/wiki/Poisson_distribution
        /// The Poisson distribution is an appropriate model if the following assumptions are
        /// true.
        ///     * K is the number of times an event occurs in an interval and K can take 
        ///       values 0, 1, 2, …
        ///     * The occurrence of one event does not affect the probability that a 
        ///       second event will occur. That is, events occur independently.
        ///     * The rate at which events occur is constant.The rate cannot be higher 
        ///       in some intervals and lower in other intervals.
        ///     * Two events cannot occur at exactly the same instant.
        ///     * The probability of an event in an interval is proportional to the 
        ///       length of the interval.
        /// If these conditions are true, then K is a Poisson random variable, and the
        /// distribution of K is a Poisson distribution.
        /// </summary>
        /// <param name="lambda">The average number of events in an interval</param>
        /// <param name="k">The number of events to observe (default is 1)</param>
        /// <returns>The chance of observing K events during the interval</returns>
        public static float PoissonProbability(float lambda, int k = 1)
        {
            return (float)(System.Math.Pow(lambda, k) * System.Math.Pow(E, -lambda) / PMath.Factorial(k));
        }

        /// <summary>
        /// 2016-3-17
        /// Breaks down a '25% chance per 8 seconds' probability
        /// to an 'n chance per update' probability.
        public static bool ChancePerSec(float chance, float perSeconds = 1)
        {
            if (chance < 0 || chance > 1) throw new System.ArgumentOutOfRangeException("chance", chance, "Must be between 0 and 1 (inclusive)");
            if (perSeconds < 0) throw new System.ArgumentOutOfRangeException("perSeconds", perSeconds, "Must be positive");

            float deltaTime = Application.isPlaying ? Time.deltaTime : 1 / 60f;
            float frames = perSeconds / deltaTime;
            float successRate = chance / frames;
            float individualChance = PoissonProbability(successRate, 1);
            return Random.value < individualChance;
        }        

        /// <summary>
        /// Can be used every update to return the chance of an event every N minutes.
        /// Does <u>not</u> guarantee a single event per N minutes -- it merely adjusts the chance
        /// value provided by the current framerate (updated every frame) and checks for success/failure.
        /// 
        /// Equivalent to: Random.value < chance / framerate / (60 * perMin)
        /// </summary>
        /// <returns><c>true</c>, if the event happens, <c>false</c> otherwise.</returns>
        /// <param name="chance">Float value from 0-1</param>
        public static bool ChancePerMin(float chance, float perMin = 1)
        {
            return ChancePerSec(chance, perMin * 60);
        }        

        /// <summary>
        /// Returns Random.Range (0, win + lose) < win.
        /// </summary>
        /// <param name="win">Number of possible "win" results.</param>
        /// <param name="lose">Number of possible "lose" results.</param>
        public static bool Chance(int win, int lose)
        {
            if (win < 0) throw new System.ArgumentOutOfRangeException("win", win, "Must be positive");
            if (lose < 0) throw new System.ArgumentOutOfRangeException("lose", lose, "Must be positive");
            return Random.Range(0, win + lose) < win;
        }

        /// <summary>
        /// Returns Random.Range (0, win + lose) < win.
        /// </summary>
        /// <param name="win">Number of possible "win" results.</param>
        /// <param name="lose">Number of possible "lose" results.</param>
        public static bool Chance(float win, float lose)
        {
            if (win < 0) throw new System.ArgumentOutOfRangeException("win", win, "Must be positive");
            if (lose < 0) throw new System.ArgumentOutOfRangeException("lose", lose, "Must be positive");
            return Random.value * (win + lose) < win;
        }

        #endregion

        //////////////////////////////////
        //	random value generation		//
        //////////////////////////////////

        #region Random Values

        /// <summary>
        /// 2016-5-26
        /// Returns a random value from 0-1, raised to the power of [exponent]
        /// </summary>
        public static float RandomExponential(float exponent = 2)
        {
            return Mathf.Pow(Random.value, exponent);
        }

        /// <summary>
        /// 2016-5-17
        /// Marsaglia polar method of generating normally distributed
        /// numbers about the provided mean, with the provided standard
        /// deviation.
        /// 
        /// Ref:
        /// http://www.alanzucconi.com/2015/09/16/how-to-sample-from-a-gaussian-distribution/
        /// 
        /// Originally written in early 2015 as part of the 
        /// DragonDefense3D project. There were errors, which have
        /// been corrected in this code.
        /// 
        /// The original code is used to create the city in DD3, and
        /// that works very well. So I don't intend to update 
        /// DragonDefense3D until the results of the two Gaussian
        /// methods have been compared.
        /// 
        /// FWIW, The original implementation is faster (because there
        /// is no while-loop).
        /// </summary>
        public static float Gaussian(float mean, float stdDev)
        {
            float u1, u2, result;
            do
            {
                u1 = Random.Range(-1, 1f);
                u2 = Random.Range(-1, 1f);
                result = u1 * u1 + u2 * u2;
            } while (result >= 1f || result == 0);

            result = Mathf.Sqrt(-2f*Mathf.Log10(result)/result);
            return mean + stdDev * result;
        }

        /// <summary>
        /// 2016-5-12
        /// Returns a random value between x and y.
        /// </summary>
        public static float RandomFromRange(Vector2 range)
        {
            return Random.Range(range.x, range.y);
        }

        /// <summary>
        /// Returns 1 if true, 0 if false.
        /// </summary>
        public static int ToInt(this bool source)
        {
            return source ? 1 : 0;
        }

        /// <summary>
        ///	2016-16-16
        /// Determines if the source array is (roughly) a
        /// normally distributed set of numbers.
        /// </summary>
        public static bool IsNormalDistribution(params float[] array)
        {
            const int precision = 1;
            float mean = array.Average();
            float median = array.Median();
            float sd = PMath.StdDev(array);

            int sdCount = array.Where(x => x.IsBetween(mean - sd, mean + sd, true, true)).Count();

            return PMath.Approx(mean, median, precision)
                && PMath.Approx(0.68f, sdCount / (float)array.Count(), precision);
        }

        /// <summary>
        /// Returns a random sign as +1 or -1
        /// </summary>
        /// <returns>+1 or -1</returns>
        public static int RandomSign()
        {
            return (Random.Range(0, 2) * 2) - 1;
        }

        /// <summary>
        /// Returns -1, 0 or 1
        /// </summary>
        public static int Random101()
        {
            return Random.Range(0, 3) - 1;
        }

        /// <summary>
        /// Returns a random float from -1 to 1. The optional magnitude parameter
        /// allows this to be adjusted to -n to +n.
        /// </summary>
        /// <param name="magnitude">Scale factor for the random number</param>
        public static float RandomPosToNeg(float magnitude = 1)
        {
            return Random.Range(-1f, 1) * magnitude;
        }

        /// <summary>
        /// Returns a random value within the top part of 1.
        /// So top = 0.2, returns results from 0.8 to 1
        /// Same as 1 - Random.value * top
        /// </summary>
        /// <returns>The random value.</returns>
        /// <param name="top">Float value from 0-1</param>
        public static float RandomTop(float top)
        {
            if (!top.IsBetween(0, 1)) throw new System.ArgumentException();
            return 1 - Random.value * top;
        }

        /// <summary>
        /// Returns a random boolean value
        /// </summary>
        /// <returns><c>true</c> or <c>false</c>.</returns>
        public static bool RandomBool()
        {
            return Random.value < 0.5;
        }

        /// <summary>
        /// Returns a linear random value. This means the chance of returning
        /// a value, is that value itself, e.g., the chance of returning 0.1 is
        /// 0.1, the chance of 0.9 is 0.9.
        /// </summary>
        /// <returns><c>Linear random value</c>.</returns>
        public static float RandomLinear()
        {
            float result = -1;
            while (result == -1)
            {
                float chance = Random.value;
                if (Random.value < chance)
                {
                    result = chance;
                }
            }
            return result;
        }

        /// <summary>
        /// 2016-1-8
        /// Returns an inverse linear random value. Linear random value means that
        /// 0.1 has only a 10% chance to appear, while 0.9 has a 90% chance.
        ///	So this has a 90% chance to return 0.1 and a 10% chance to return 0.9.
        ///	Efectively the same as: 1 - RandomLinar()
        /// </summary>
        /// <returns><c>Linear random value</c>.</returns>
        public static float RandomInverseLinar()
        {
            return 1 - RandomLinear();
        }

        /// <summary>
        /// Returns a random quaternion rotation. Equivalent to Random.onUnitSphere.
        /// 
        /// Optionally, the rotation can be projected onto a plane, by providing a rotation mask.
        /// So, if rotationMask is the XY plane, the quaternion will be a randomized rotation
        /// on the XY plane.
        /// </summary>
        /// <returns>The quaternion.</returns>
        /// <param name="rotionMask">Vector3 defining a plane.</param>
        public static Quaternion RandomQuaternion(Vector3 rotationMask)
        {
            Vector3 randomVector = Random.onUnitSphere;
            randomVector.Scale(rotationMask);
            return Quaternion.LookRotation(randomVector.normalized);
        }

        /// <summary>
        /// Returns a random quaternion rotation. Equivalent to Random.onUnitSphere.
        /// 
        /// Optionally, the rotation can be projected onto a plane, by providing a rotation mask.
        /// So, if rotationMask is the XY plane, the quaternion will be a randomized rotation
        /// on the XY plane.
        /// </summary>
        /// <returns>The quaternion.</returns>
        /// <param name="rotionMask">Vector3 defining a plane.</param>
        public static Quaternion RandomQuaternion()
        {
            Vector3 randomVector = Random.onUnitSphere;
            return Quaternion.LookRotation(randomVector);
        }

        /// <summary>
        /// Returns a random angle, in degrees.
        /// </summary>
        /// <returns>The degree angle (from 0-360).</returns>
        public static float RandomDegreeAngle()
        {
            return Random.Range(0f, 360);
        }

        /// <summary>
        /// Returns a random angle, in radians.
        /// </summary>
        /// <returns>The radian angle (from 0 to 2Pi).</returns>
        public static float RandomRadianAngle()
        {
            return Random.Range(0f, PMath.TAU);
        }

        /// <summary>
        /// 2016-5-12
        /// Returns a random right angle: 0, 90, 180, 270
        /// </summary>
        public static int RandomRightAngle()
        {
            return Random.Range(0, 4) * 90;
        }

        /// <summary>
        /// Returns a random vertex from the mesh.
        /// </summary>
        /// <returns>The vertex on mesh.</returns>
        /// <param name="mesh">Mesh.</param>
        public static Vector3 RandomVertexOnMesh(Mesh mesh)
        {
            PLog.LogNoUnitTests();
            return mesh.vertices[Random.Range(0, mesh.vertices.Length)];
        }

        /// <summary>
        ///	2016-1-14
        /// Returns a random value from 0-1 that is normally distributed around a median of 0.5
        /// Equivalent to averaging 30 Random.value calls. This number can be changed by providing
        /// the optional parameter "smoothness".
        /// 
        /// The optional smoothness parameter adjusts the number of itterations, so generated results
        /// converge on a normal distribution faster. This parameter changes the number of rolls from
        /// 30 to the value provided.
        /// </summary>
        /// <returns>The normal distribution.</returns>
        /// <param name="smoothness">integer number of itterations to perform.</param>
        public static float RandomNormalDistribution(int smoothness = 30)
        {
            float sum = 0;
            for (int i = 0; i < smoothness; i++)
                sum += Random.value;
            sum /= smoothness;
            return sum;
        }

        /// <summary>
        /// Generates an unbounded random number from 0-1 (sort of).
        /// If number > repeatThreshold, then roll again and add the result.
        /// Stop if the generated number is less than the threshold, or the repetitions
        /// have hit the limit of maxRepeat (default 3).
        /// 
        /// Ex, RandomUnbounded(0.9) -- produces normal results from 0 to 0.9, and if the initial randomization
        /// is from 0.9 to 1, an additional random value (from 0 to 1) is added to the result. If this additional
        /// value is also from 0.9 to 1, then an additional random value is added. This repeats until the limit
        /// of maxRepeat is hit. In this example, maxRepeat uses the default value of 3. This example returns a 
        /// result from 0 to 4, with a 90% chance of being from 0 to 0.9.
        /// </summary>
        /// <returns>Unbounded random value</returns>
        /// <param name="repeatThreshold">Float value 0-1.</param>
        /// <param name="maxRepeat">Max times to repeat.</param>
        public static float RandomUnbounded(float repeatThreshold, int maxRepeat = 3)
        {
            if (!repeatThreshold.IsBetween(0, 1)) throw new System.ArgumentOutOfRangeException("Repeat Threshold (" + repeatThreshold + ") must be between 0 and 1.");
            float value = 0, total = 0;
            int rolls = 0;
            do
            {
                value = Random.value;
                total += value;
                rolls++;
            } while (value >= repeatThreshold
                && (rolls < maxRepeat || maxRepeat == -1));

            return total;
        }

        #endregion

        //////////////////////////
        //	Dice rolls			//
        //////////////////////////

        #region Dice

        public enum DieSize { D4 = 4, D6 = 6, D8 = 8, D10 = 10, D12 = 12, D20 = 20, D100 = 100 };
        public enum RollMethod
        {
            NORMAL, UNBOUNDED,
            MATCH_NUMBER, ROLL_ABOVE, ROLL_BELOW, ROLL_ABOVE_INCLUSIVE, ROLL_STRICTLY_BELOW
        };

        /// <summary>
        /// Rolls an n-sided die and returns the result
        /// </summary>
        /// <returns>The die roll.</returns>
        /// <param name="sides">Number of sides on the die.</param>
        private static int RollDieStandard(int sides)
        {
            return 1 + Random.Range(0, sides);
        }

        /// <summary>
        /// Rolls an "unbounded" n-sided die and returns the result.
        /// "Unbounded" means the following:
        /// 	If the die result is n (the highest roll possible), then roll again and add the result.
        /// 	Continue as long as the result is the highest roll possible, e.g., continue rolling
        /// 	as long as you keep getting 20's on a 20-sided die.
        /// </summary>
        /// <returns>The die roll.</returns>
        /// <param name="sides">Number of sides on the die.</param>
        private static int RollDieUnbounded(int sides, int threshold = -1)
        {
            if (threshold == -1) threshold = sides;
            int result = 0, roll;
            do
            {
                roll = RollDieStandard(sides);
                result += roll;
            } while (roll >= threshold);
            return result;
        }

        /// <summary>
        /// Rolls an n-sided die and returns the result. This roll can either be normal or unbounded.
        /// By default, the roll is a normal roll. The optional RollMethod parameter can be used to indicate
        /// an unbounded roll.
        /// 
        /// A <u>normal<u> roll returns the result of the die roll.
        /// 
        /// An <u>unbounded</u> means the following:
        /// 	If the die result is n (the highest roll possible), then roll again and add the result.
        /// 	Continue as long as the result is the highest roll possible, e.g., continue rolling
        /// 	as long as you keep getting 20's on a 20-sided die.
        /// </summary>
        /// <returns>The die roll.</returns>
        /// <param name="sides">Number of sides on the die.</param>
        public static int RollDie(int sides, RollMethod method = RollMethod.NORMAL)
        {
            if (sides < 2) throw new System.ArgumentException("Dice have at least 2 sides", sides.ToString());
            if (method != RollMethod.NORMAL && method != RollMethod.UNBOUNDED)
                throw new System.ArgumentException("Roll method must be NORMAL or UNBOUNDED", method.ToString());

            switch (method)
            {
                case RollMethod.NORMAL:
                    return RollDieStandard(sides);
                case RollMethod.UNBOUNDED:
                    return RollDieUnbounded(sides);
                default:
                    return -1;
            }
        }

        /// <summary>
        /// Rolls an n-sided die and compares the result to the target number.
        /// The return value depends on the RollMethod selected. If RollMethod is omitted, then
        /// ROLL_BELOW will be used by default.
        /// 
        /// MATCH_NUMBER returns true when the die roll = the target.
        /// ROLL_ABOVE returns true when the die roll > the target.
        /// ROLL_BELOW returns true when the die roll <= the target.
        /// ROLL_ABOVE_INCLUSIVE returns true when the die roll >= the target.
        /// ROLL_STRICTLY_BELOW returns true when the die roll < the target.
        /// 
        /// </summary>
        /// <returns>Boolean indicating success or failure.</returns>
        /// <param name="sides">Number of sides on the die.</param>
        public static bool SuccessDieRoll(int sides, int target, RollMethod method = RollMethod.ROLL_BELOW)
        {

            switch (method)
            {
                case RollMethod.MATCH_NUMBER:
                    return RollDie(sides, RollMethod.NORMAL) == target;
                case RollMethod.ROLL_ABOVE:
                    return RollDie(sides, RollMethod.NORMAL) > target;
                case RollMethod.ROLL_ABOVE_INCLUSIVE:
                    return RollDie(sides, RollMethod.NORMAL) >= target;
                case RollMethod.ROLL_STRICTLY_BELOW:
                    return RollDie(sides, RollMethod.NORMAL) < target;
                case RollMethod.ROLL_BELOW:
                default:
                    return RollDie(sides, RollMethod.NORMAL) <= target;
            }
        }

        /// <summary>
        /// Rolls an n-sided die multiple times and totals the results.
        /// </summary>
        /// <returns>The sum of the die rolls.</returns>
        /// <param name="sides">Sides.</param>
        /// <param name="diceCount">Number of dice to roll.</param>
        public static int RollDice(int sides, int diceCount)
        {
            int result = 0;
            for (var i = 0; i < diceCount; i++)
            {
                result += RollDie(sides);
            }
            return result;
        }

        /// <summary>
        /// Rolls d4. The optional parameter can be used to indicate rolling multiple dice.
        /// </summary>
        /// <returns>The die result.</returns>
        public static int RollD4(int diceCount = 1)
        {
            return RollDice((int)DieSize.D4, diceCount);
        }

        /// <summary>
        /// Rolls d6. The optional parameter can be used to indicate rolling multiple dice.
        /// </summary>
        /// <returns>The die result.</returns>
        public static int RollD6(int diceCount = 1)
        {
            return RollDice((int)DieSize.D6, diceCount);
        }

        /// <summary>
        /// Rolls d8. The optional parameter can be used to indicate rolling multiple dice.
        /// </summary>
        /// <returns>The die result.</returns>
        public static int RollD8(int diceCount = 1)
        {
            return RollDice((int)DieSize.D8, diceCount);
        }

        /// <summary>
        /// Rolls d10. The optional parameter can be used to indicate rolling multiple dice.
        /// </summary>
        /// <returns>The die result.</returns>
        public static int RollD10(int diceCount = 1)
        {
            return RollDice((int)DieSize.D10, diceCount);
        }

        /// <summary>
        /// Rolls d12. The optional parameter can be used to indicate rolling multiple dice.
        /// </summary>
        /// <returns>The die result.</returns>
        public static int RollD12(int diceCount = 1)
        {
            return RollDice((int)DieSize.D12, diceCount);
        }

        /// <summary>
        /// Rolls d20. The optional parameter can be used to indicate rolling multiple dice.
        /// </summary>
        /// <returns>The die result.</returns>
        public static int RollD20(int diceCount = 1)
        {
            return RollDice((int)DieSize.D20, diceCount);
        }

        /// <summary>
        /// Rolls d100. The optional parameter can be used to indicate rolling multiple dice.
        /// </summary>
        /// <returns>The die result.</returns>
        public static int RollD100(int diceCount = 1)
        {
            return RollDice((int)DieSize.D100, diceCount);
        }

        #endregion

        //////////////////////////
        //	Random Selection	//
        //////////////////////////

        #region Random Selection

        /// <summary>
        ///	2016-1-13
        ///	Uses the Fisher-Yates shuffle algorithm:
        ///		Iterate the array, swap the current element with a random element
        ///		between the current element and the end.
        ///	The optional parameter allows a random seed to be specified, resulting
        ///	in the same 'random' shuffle being generated from the same seed.
        /// </summary>
        /// <param name="array">Array.</param>
        public static T[] Randomize<T>(this T[] source)
        {
            for (int i = 0; i < source.Length - 1; i++)
            {
                var firstValue = source[i];
                int swapIndex = Random.Range(i, source.Length);
                source[i] = source[swapIndex];
                source[swapIndex] = firstValue;
            }

            return source;
        }

        /// <summary>
        ///	2016-1-13
        ///	Uses the Fisher-Yates shuffle algorithm:
        ///		Iterate the array, swap the current element with a random element
        ///		between the current element and the end.
        ///	The optional parameter allows a random seed to be specified, resulting
        ///	in the same 'random' shuffle being generated from the same seed.
        /// </summary>
        /// <param name="array">Array.</param>
        public static T[] Randomize<T>(this T[] source, int seed)
        {
            System.Random rng = new System.Random(seed);

            for (int i = 0; i < source.Length - 1; i++)
            {
                var firstValue = source[i];
                int swapIndex = rng.Next(i, source.Length);
                source[i] = source[swapIndex];
                source[swapIndex] = firstValue;
            }

            return source;
        }

        /// <summary>
        ///	2016-1-13
        ///	Uses the Fisher-Yates shuffle algorithm:
        ///		Iterate the list, swap the current element with a random element
        ///		between the current element and the end.
        /// </summary>
        public static List<T> Randomize<T>(this List<T> source)
        {

            for (int i = 0; i < source.Count - 1; i++)
            {
                T item = source[i];
                source.RemoveAt(i);
                source.Insert(Random.Range(i, source.Count), item);
            }

            return source;
        }

        /// <summary>
        ///	2016-1-13
        ///	Uses the Fisher-Yates shuffle algorithm:
        ///		Iterate the list, swap the current element with a random element
        ///		between the current element and the end.
        ///	The optional parameter allows a random seed to be specified, resulting
        ///	in the same 'random' shuffle being generated from the same seed.
        /// </summary>
        /// <param name="array">Array.</param>
        public static List<T> Randomize<T>(this List<T> source, int seed)
        {

            System.Random rng = new System.Random(seed);

            for (int i = 0; i < source.Count - 1; i++)
            {
                T item = source[i];
                source.RemoveAt(i);
                source.Insert(rng.Next(i, source.Count), item);
            }

            return source;
        }

        /// <summary>
        /// Gets a random element from the list.
        /// </summary>
        /// <returns>An element from the list.</returns>
        /// <param name="list">List.</param>
        public static T GetRandom<T>(this List<T> list)
        {
            return list[Random.Range(0, list.Count)];
        }

        /// <summary>
        /// Gets a random element form the array.
        ///
        /// </summary>
        /// <returns>An element from the list.</returns>
        /// <param name="array">Array.</param>
        public static T GetRandom<T>(this T[] array)
        {
            return array[Random.Range(0, array.Length)];
        }

        /// <summary>
        /// Gets a random element selected from the first N elements of the list.
        /// </summary>
        /// <returns>An element.</returns>
        /// <param name="list">List.</param>
        /// <param name="topSize">Top size.</param>
        public static T GetRandomFromTop<T>(this List<T> list, int topSize)
        {
            if (list.Count == 0) return default(T);
            if (topSize > list.Count) topSize = list.Count;
            return list[Random.Range(0, Mathf.Clamp(list.Count, 1, topSize))];
        }

        /// <summary>
        /// Gets a random element selected from the last N elements of the list.
        /// </summary>
        /// <returns>An element.</returns>
        /// <param name="list">List.</param>
        /// <param name="bottomSize">Bottom size.</param>
        public static T GetRandomFromBottom<T>(this List<T> list, int bottomSize)
        {
            if (list.Count == 0) return default(T);
            if (bottomSize > list.Count) bottomSize = list.Count;
            return list[Random.Range(list.Count - bottomSize, list.Count)];
        }

        /// <summary>
        /// Randomly selects an index using the value in each position as the relative weight
        /// of the position to appear.
        /// 
        /// For example, [ 0.5, 2.25, 4, 0 ]
        /// 	There is a 0.5 / 6.75 chance to return the first index (return value 0)
        /// 	There is a 2.25 / 6.75 chance to return the second index (return value 1)
        /// 	There is a 4 / 6.75 chance to return the third index (return value 2)
        ///		there is a 0 / 6.75 chance to return the fourth index (return value 3)
        /// </summary>
        /// <returns>The random index.</returns>
        /// <param name="weights">Weights. An array of non-negative weights.</param>
        public static int WeightedRandomIndex(this float[] weights)
        {

            float sum = weights.Sum();

            if (sum < 0) throw new System.ArgumentException("Sum of weights array must not be negative.", weights.ToString());
            if (sum == 0) return Random.Range(0, weights.Length);

            float selection = Random.Range(0, sum);

            for (int i = 0; i < weights.Length; i++)
            {
                selection -= weights[i];
                if (selection <= 0) return i;
            }
            return -1;
        }

        /// <summary>
        /// Returns a point inside the polygon defined by the pointlist.
        /// Assumes the point list describes a normal concave polygon.
        /// </summary>
        /// <returns>The point inside polygon.</returns>
        /// <param name="pointList">Point list.</param>
        public static Vector2 RandomPointInsidePolygon2d(Vector2[] pointList)
        {

            //  determine polygon centerpoint
            Vector2 center = Vector2.zero;
            foreach (Vector2 p in pointList)
                center += p;
            center /= pointList.Length;

            //  select to random point on the edge of the polygon
            Vector2 edgePoint = RandomPointOnPolygon2dEdge(pointList);

            //  select a point between the center point and the edgePoint
            Vector2 point = center + (edgePoint - center) * Random.value;

            return point;
        }

        /// <summary>
        /// Returns a random point between a random pair of adjacent points.
        /// Points are considered adjacent if they are neighboring indicies.
        /// The first and last point are also considered adjacent.
        /// </summary>
        /// <returns>The point on polygon edge.</returns>
        /// <param name="pointList">Point list.</param>
        public static Vector2 RandomPointOnPolygon2dEdge(this Vector2[] pointList)
        {
            PLog.LogNoUnitTests();
            if (pointList.Length == 0) return Vector2.zero;
            if (pointList.Length == 1) return pointList[0];

            int indexA = Random.Range(0, pointList.Length);
            int indexB = (indexA + 1) % pointList.Length;
            Vector2 p1 = pointList[indexA];
            Vector2 p2 = pointList[indexB];

            return p1 + Random.value * (p2 - p1);
        }

        #endregion

        ////////////////////
        //	Shapes		////
        ////////////////////

        #region Shapes

        /// <summary>
        /// 2016-5-13
        /// Defines a Helix
        /// </summary>
        public struct Helix
        {
            public float radius, height, frequency;
            public Helix(float radius, float height, float frequency)
            {
                this.radius = radius;
                this.height = height;
                this.frequency = frequency;
            }
            public Vector3 PointOnSurface
            {
                get
                {
                    return PRand.OnHelix(radius, height, frequency);
                }
            }
            public Vector3 PointInside
            {
                get
                {
                    return PRand.InsideHelix(radius, height, frequency);
                }
            }
            public bool Contains(Vector3 point, float errorMargin = 0.05f)
            {
                return PRand.HelixContainsPoint(point, radius, height, frequency, errorMargin);
            }
        }

        /// <summary>
        /// 2016-5-13
        /// Defines a Cone or conic section
        /// </summary>
        public struct Cone
        {
            public float topRadius, bottomRadius, height;
            public Cone(float topRadius, float bottomRadius, float height)
            {
                this.topRadius = topRadius;
                this.bottomRadius = bottomRadius;
                this.height = height;
            }
            public Vector3 PointOnSurface
            {
                get
                {
                    return PRand.OnCone(topRadius, bottomRadius, height);
                }
            }
            public Vector3 PointInside
            {
                get
                {
                    return PRand.InsideCone(topRadius, bottomRadius, height);
                }
            }
            public bool Contains(Vector3 point)
            {
                return PRand.ConeContainsPoint(point, topRadius, bottomRadius, height);
            }
        }

        /// <summary>
        /// 2016-5-13
        /// Defines a Cylinder
        /// </summary>
        public struct Cylinder
        {
            public float radius, height;
            public Cylinder(float radius, float height)
            {
                this.radius = radius;
                this.height = height;
            }
            public Vector3 PointOnSurface(bool includeEndcaps = false)
            {
                return PRand.OnCylinder(radius, height, includeEndcaps);
            }
            public Vector3 PointInside
            {
                get
                {
                    return PRand.InsideCylinder(radius, height);
                }
            }
            public bool Contains(Vector3 point)
            {
                return PRand.CylinderContainsPoint(point, radius, height);
            }
        }

        /// <summary>
        /// 2016-5-13
        /// Returns random point on the outter edge of a "unit" helix. 
        /// A Unit Helix is defined as a helix with radius 1, 
        /// height 1 and frequency of 1. Optionally, radius, height 
        /// and frequency can be specified.
        /// 
        /// Returns random point on the outter edge of a helix.
        /// Formula for helix: x = u⋅cos(v), y = u⋅sin(v), z = v/2
        /// Ref: http://english.rejbrand.se/algosim/visualisation.asp?id=helicoid
        /// </summary>
        public static Vector3 OnHelix(float radius = 1, float height = 1, float frequency = 1)
        {
            Vector3 result = Vector3.zero;

            //  generate a random point on the edge of a circle
            float angle = Random.Range(0, 360);
            float x = Mathf.Cos(angle);
            float y = Mathf.Sin(angle);
            Vector2 circumferencePoint = new Vector2(x, y) * radius;

            //  project the point upward onto the helix
            result.x = circumferencePoint.x * Mathf.Cos(circumferencePoint.y);
            result.z = circumferencePoint.y * Mathf.Sin(circumferencePoint.x);
            result.y = height * circumferencePoint.y / frequency;

            return result;
        }

        /// <summary>
        /// 2016-5-13
        /// Returns random point anywhere on the surface of a "unit" helix. 
        /// A Unit Helix is defined as a helix with radius 1, 
        /// height 1 and frequency of 1.
        /// 
        /// Optional parameters allow the radius, height and frequency
        /// to be specified.
        /// 
        /// Returns random point anywhere on the surface of a helix.
        /// Formula for helix: x = u*cos(v), y = u*sin(v), z = v/2
        /// Ref: http://english.rejbrand.se/algosim/visualisation.asp?id=helicoid
        /// </summary>
        public static Vector3 InsideHelix(float radius = 1, float height = 1, float frequency = 1)
        {
            Vector3 result = Vector3.zero;

            //  generate a random point on a circle
            Vector2 pointInCircle = Random.insideUnitCircle * radius;

            //  project the point upward onto the helix
            result.x = pointInCircle.x * Mathf.Cos(pointInCircle.y);
            result.z = pointInCircle.y * Mathf.Sin(pointInCircle.x);
            result.y = height * pointInCircle.y / frequency;

            //  The following should not be here, but it is preserved
            //  as an example of how to multiply Quaternions and vectors
            //  in order to rotate a vector.

            //  rotate to the correct direction
            //Quaternion direction = Quaternion.LookRotation(endPoint - startPoint);
            //result = direction * result;

            return result;
        }

        /// <summary>
        /// 2016-5-13
        /// Indicates if the provided point is contained within a unit helix.
        /// The helix parameters (radius, height, frequency) can be specified.
        /// Because a helix is a two-dimensional object, a margin of error must
        /// be specified for the point. This is the effective "thickness" of the
        /// helix that the point may fall within and be considered "on" the
        /// helix. By default this value is 0.05 (meters)
        /// </summary>
        /// <returns></returns>
        public static bool HelixContainsPoint(Vector3 point, float radius = 1, float height = 1, float frequency = 1, float error = 0.05f)
        {
            PLog.LogNoUnitTests();
            Vector3 expected = Vector3.zero;

            //  project the point on to a circle
            Vector2 pointInCircle = point.ToVector2();

            //  project the point upward onto the helix
            expected.x = pointInCircle.x * Mathf.Cos(pointInCircle.y);
            expected.z = pointInCircle.y * Mathf.Sin(pointInCircle.x);
            expected.y = height * pointInCircle.y / frequency;

            return Vector3.SqrMagnitude(point - expected) < error * error;
        }

        /// <summary>
        /// 2016-5-12
        /// Returns a random point on the surface of a Unit Cone,
        /// excluding the base. A Unit Cone is defined as a cone 
        /// with bottom radius 1, top radius 0 and height 1.
        /// Optional parameters allow the radii and height to be
        /// specified.
        /// </summary>
        public static Vector3 OnCone(float bottomRadius = 1, float topRadius = 0, float height = 1)
        {
            PLog.LogNoUnitTests();
            Vector3 result = Vector3.zero;
            float minRadius = Mathf.Min(bottomRadius, topRadius);

            bool onSurface = false;
            do
            {
                //  get point inside cone
                result = InsideCone(bottomRadius, topRadius, height);

                //  Remove any point in the central cylinder.
                //  This prevents clustering at the top and bottom.
                if (CylinderContainsPoint(result, minRadius, height)) continue;

                //  project remaining points to edge
                float radiusAtHeight = (bottomRadius - topRadius) * result.y + topRadius;
                Vector2 xy = new Vector2(result.x, result.z);
                xy.Normalize();
                xy *= radiusAtHeight;

                result.x = xy.x;
                result.z = xy.y;
                onSurface = true;

            } while (!onSurface);

            return result;
        }

        /// <summary>
        /// 2016-5-12
        /// Returns a random point inside a unit cone. Unit cone is defined
        /// as a cone with bottom radius 1, top radius 0 and height 1.
        /// Optional parameters allow the radii and height to be specified.
        /// </summary>
        public static Vector3 InsideCone(float bottomRadius, float topRadius = 1, float height = 1)
        {
            PLog.LogNoUnitTests();
            Vector3 result = Vector3.zero;

            //  define a bounding sphere
            float largestRadius = Mathf.Max(bottomRadius, topRadius);
            float sphereRadius = Mathf.Sqrt(Mathf.Pow(0.5f * height, 2) + Mathf.Pow(largestRadius, 2));

            bool inside = false;
            do
            {
                //  generate points within the bounding sphere
                result = Random.insideUnitSphere * sphereRadius + 0.5f * height * Vector3.up;

                //  check if the point is contained by the cone
                inside = ConeContainsPoint(result, bottomRadius, topRadius, height);

            } while (!inside);

            return result;
        }

        /// <summary>
        /// 2016-5-13
        /// Returns true if the provided point is contained within a unit cone.
        /// The cone dimensions can be changed with the optinoal parameters.
        /// </summary>
        public static bool ConeContainsPoint(Vector3 point, float bottomRadius = 1, float topRadius = 1, float height = 1)
        {
            PLog.LogNoUnitTests();
            //  check if the point is contained by the cone
            float radiusAtHeight = (bottomRadius - topRadius) * point.y + topRadius;
            float planarDistanceOfPoint = Vector2.Distance(Vector2.zero, new Vector2(point.x, point.z));
            return planarDistanceOfPoint <= radiusAtHeight;
        }

        /// <summary>
        /// 2016-5-12
        /// Returns a random point (Vector3) on the surface of a unit cylinder,
        /// excluding the endcaps. A unit cylinder is defined as circle with 
        /// radius 1, and height of 1.
        /// 
        /// By default, the point returned is only on the side of the
        /// cylinder. The optional boolean parameter can indicate to permit
        /// points returned on the endcaps as well.
        /// Notes:
        ///     Surface area of endcaps is 2 x (pi x r^2)
        ///     Surface area of side is (2 x pi) x h
        ///     Ratio of surface area of endcaps to sides is: r/h
        /// </summary>
        public static Vector3 OnCylinder(float radius = 1, float height = 1, bool includeEndcaps = false)
        {
            Vector3 result = Vector3.zero;
            if (includeEndcaps)
            {
                //  Generate point anywhere on cylinder,
                //  including endcaps.
                if (RandomBool())
                {
                    //  return point on side
                    //  Note that RandomVector2() returns a unit length vector2
                    result = OnCircle(radius).ToVector3() + Vector3.up * Random.value * height;
                }
                else
                {
                    //  return point on an endcap
                    result = InsideCircle(radius).ToVector3() * radius + Vector3.up * RandomBool().ToInt();
                }
            }
            else
            {
                //  Generate point anywhere on side of
                //  cylinder, excluding endcaps.
                result = OnCircle(radius).ToVector3() + Vector3.up * Random.value * height;
            }
            return result;
        }

        /// <summary>
        /// 2016-5-13
        /// Returns a random point inside a unit cylinder.
        /// Unit cylinder is radius 1, height 1. Optional
        /// parameters allow the radius and height to be
        /// specified.
        /// </summary>
        public static Vector3 InsideCylinder(float radius = 1, float height = 1)
        {
            return (Random.insideUnitCircle * radius).ToVector3() + Vector3.up * Random.value * height;
        }

        /// <summary>
        /// 2016-5-12
        /// Returns if the point is contained by the cylinder decribed by the parameters.
        /// Cylinder is assumed to be at the (0,0,0) location, and point is relative to cylinder.
        /// Cylinder base is at (0,0,0) and extends upward in y direction.
        /// </summary>
        public static bool CylinderContainsPoint(Vector3 point, float radius = 1, float height = 1)
        {
            if (!point.y.IsBetween(0, height)) return false;
            Vector2 xy = new Vector2(point.x, point.z);
            return (Vector2.SqrMagnitude(xy) < radius * radius);
        }

        /// <summary>
        /// 2016-5-13
        /// Returns a point on the surface of a unit sphere.
        /// Unit sphere is a sphere with radius 1. The 
        /// optional parameter allows a radius to be 
        /// specified.
        /// </summary>
        public static Vector3 OnSphere(float radius = 1)
        {
            return Random.onUnitSphere * radius;
        }

        /// <summary>
        /// 2016-5-13
        /// Returns a point contained within a unit sphere.
        /// Unit sphere is defined as a sphere with a
        /// radius of 1. The optional parameter allows the
        /// radius to be specified.
        /// </summary>
        public static Vector3 InsideSphere(float radius = 1)
        {
            return OnSphere(radius * Random.value);
        }

        /// <summary>
        /// 2016-5-13
        /// Returns true if the point is contained with in a unit sphere.
        /// Optionally, sphere radius can be specied.
        /// </summary>
        public static bool SphereContainsPoint(Vector3 point, float radius = 1)
        {
            return point.sqrMagnitude <= radius * radius;
        }

        /// <summary>
        /// 2016-5-13
        /// Returns a point on the circumference of a unit circle.
        /// Unit circle is circle with radius 1. Radius can be
        /// overridden with optional parameter.
        /// </summary>
        public static Vector2 OnCircle(float radius = 1)
        {
            return Random.insideUnitCircle.normalized * radius;
        }

        /// <summary>
        /// 2016-5-13
        /// Returns a point inside a unit circle. Unit circle 
        /// is circle with radius 1. Radius can be overridden 
        /// with optional parameter.
        /// </summary>
        public static Vector2 InsideCircle(float radius = 1)
        {
            return Random.insideUnitCircle * radius;
        }

        /// <summary>
        /// 2016-5-13
        /// Returns true if a point is contained within a unit circle.
        /// Unit circle is circle with radius 1. Radius can be
        /// overridden with optional parameter.
        /// </summary>
        public static bool CircleContainsPoint(Vector2 point, float radius = 1)
        {
            return Vector2.SqrMagnitude(point) < radius * radius;
        }

        #endregion

        ////////////////////
        //	Vectors		////
        ////////////////////

        #region Vectors

        /// <summary>
        /// 2016-5-13 DEPRECATED Use OnSphere(radius) instead.
        /// Returns a random unit vector. Optional parameter changes the length of the vector.
        ///	Equivalent to Random.onUnitSphere * magnitude
        /// </summary>
        /// <returns>The vector.</returns>
        /// <param name="magnitude">Magnitude.</param>
        public static Vector3 RandomVector3(float magnitude = 1)
        {
            return Random.onUnitSphere * magnitude;
        }

        /// <summary>
        /// 2016-5-13 DEPRECATED Use OnCircle(radius) instead.
        /// Returns a random unit vector2. Optional parameter changes the length of the vector.
        ///	Equivalent to Random.insideUnitCircle.normalized * magnitude
        /// </summary>
        /// <returns>The vector.</returns>
        /// <param name="magnitude">Magnitude.</param>
        public static Vector2 RandomVector2(float magnitude = 1)
        {
            return Random.insideUnitCircle.normalized * magnitude;
        }

        /// <summary>
        /// 2016-5-13 DEPRECATED Use InsideSphere(radius) instead
        /// Adds the a random vector to the source vector. This "dirt" can be scaled with the optional
        ///	parameter "magnitude".
        ///	Equivalent to vector += Random.onUnitSphere * magnitude
        /// </summary>
        /// <returns>The dirt.</returns>
        /// <param name="magnitude">Magnitude.</param>
        public static Vector3 Dirt3(float magnitude = 1)
        {
            return RandomVector3(Random.value * magnitude);
        }

        /// <summary>
        /// 2016-5-16 DEPRECATED Use InsideCircle(radius) instead
        /// Adds the a random vector to the source vector. This "dirt" can be scaled with the optional
        ///	parameter "magnitude".
        ///	Equivalent to vector += Random.insideUnitCircle * magnitude
        /// </summary>
        /// <returns>The dirt.</returns>
        /// <param name="magnitude">Magnitude.</param>
        public static Vector2 Dirt2(float magnitude = 1)
        {
            return RandomVector2(Random.value * magnitude);
        }

        /// <summary>
        /// 2016-5-13 DEPRECATED There is no point to this
        /// Scales the source vector and returns it. This is equivalent to the Vector3.Scale() method
        /// except that it returns the scaled vector instead of void.
        /// </summary>
        /// <returns>The and return.</returns>
        /// <param name="scalingVector">Scaling vector.</param>
        public static Vector3 Scale(Vector3 source, Vector3 scalingVector)
        {
            source.Scale(scalingVector);
            return source;
        }

        /// <summary>
        /// 2016-5-13 DEPRECATED There is no point to this
        /// Scales the source vector and returns it. This is equivalent to the Vector2.Scale() method
        /// except that it returns the scaled vector instead of void.
        /// </summary>
        /// <returns>The and return.</returns>
        /// <param name="scalingVector">Scaling vector.</param>
        public static Vector2 Scale(Vector2 source, Vector2 scalingVector)
        {
            source.Scale(scalingVector);
            return source;
        }

        /// <summary>
        /// 2016-1-8
        /// Returns a new vector equal to 1/x, 1/y, 1/z
        /// </summary>
        public static Vector3 InvertAxis(Vector3 vector)
        {
            return new Vector3(1 / vector.x, 1 / vector.y, 1 / vector.z);
        }

        /// <summary>
        /// 2016-1-8
        /// Returns a new vector equal to 1/x, 1/y
        /// </summary>
        public static Vector2 InvertAxis(Vector2 vector)
        {
            return new Vector2(1 / vector.x, 1 / vector.y);
        }

        /// <summary>
        /// 2016-5-13
        /// Returns a random unit vector on the plane defined by the 
        /// "planeNormal" parameter. The optional "magnitude" parameter 
        /// allows the returned vector to be scaled.
        /// </summary>
        public static Vector3 RandomPlanarVector(Vector3 planeNormal, float magnitude = 1)
        {
            Vector3 v = Vector3.ProjectOnPlane(Random.onUnitSphere, planeNormal);
            return v.normalized * magnitude;
        }

        /// <summary>
        /// Returns a unit vector on the XZ plane (the default "world" horizontal plane).
        /// Equivalent to Random.onUnitSphere.ScaleAndReturn(XZ_PLANE).normalized * magnitude
        /// </summary>
        /// <returns>The horizontal vector.</returns>
        /// <param name="magnitude">Magnitude.</param>
        public static Vector3 RandomHorizontalVector(float magnitude = 1)
        {
            return RandomPlanarVector(Vector3.up, magnitude);
        }

        /// <summary>
        /// Returns a random unit vector on one of the three coordinate planes.
        /// Optional parameter allows the return vector to be scaled.
        /// </summary>
        /// <returns>The ortho vector.</returns>
        public static Vector3 RandomOrthoVector(float magnitude = 1)
        {
            switch (Random.Range(0, 3))
            {
                case 0:
                    return Vector3.forward * magnitude;
                case 1:
                    return Vector3.right * magnitude;
                case 2:
                    return Vector3.up * magnitude;
                default:
                    return Vector3.zero;
            }
        }

        #endregion
    }
}