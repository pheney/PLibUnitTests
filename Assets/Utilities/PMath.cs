using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using PLib.Logging;
using PLib.General;
using Mathd = System.Math;

/// <summary>
/// Math and Time
///	Converted to partial class.
/// </summary>
namespace PLib.Math
{
    public static class PMath
    {
        #region Constants

        public static int FRAMERATE = (int)(1 / Time.fixedDeltaTime);
        public static float FRAMELENGTH = 1000 / FRAMERATE;
        private static string buildNumber = "1";
        public static string BUILD_NUMBER
        {
            get { return "Build: " + buildNumber; }
        }

        public static float TAU = Mathf.PI * 2;
        public static double ROOT2_DOUBLE = 1.4142135623730950488016887242097;
        public static float ROOT2 = (float)ROOT2_DOUBLE;
        public static double UNIVERSAL_GRAVITATIONAL_CONSTANT = 6.673 * Mathf.Pow(10, -11);

        public static int SEC_PER_MINUTE = 60;
        public static int SEC_PER_HOUR = SEC_PER_MINUTE * 60;
        public static int SEC_PER_DAY = SEC_PER_HOUR * 24;
        public static int SEC_PER_WEEK = SEC_PER_DAY * 7;
        public static int SEC_PER_YEAR = SEC_PER_WEEK * 52;

        public static double GRAVITATIONAL_CONSTANT = Mathd.Pow(6.673, -11);

        #endregion

        //////////////////////
        //	Formating Time	//
        //////////////////////

        #region Formating Time

        /// <summary>
        /// 2016-12-09
        /// Formats the time from seconds to dd:hh:mm:ss.ss
        ///	Usage: FormatTime(184905.35) --> 2:03:21:45.35
        /// Optional parameter to show decimal fractions of seconds.
        /// </summary>
        /// <returns>The time.</returns>
        /// <param name="seconds">Seconds.</param>
        public static string FormatTime(float seconds, int decimalPlaces = 2)
        {
            int multiply = (int)Mathf.Pow(10, decimalPlaces);
            int dec = (int)((seconds * multiply) % multiply); //	dec = 184905.35 * 100 (18490535), %100 (35)
            int remainder = (int)Trunc(seconds, 0);     //	rem = trunc (184905.35,2) = 184905

            int sec = remainder % 60;                   //	sec = 184905 % 60 = 45
            remainder -= sec;                           //	rem = 192 - 45 = 184860

            remainder /= 60;                                //	rem = 184860 / 60 = 3081
            int min = remainder % 60;                   //	min = 3081 % 60 = 21
            remainder -= min;                           //	rem = 3081 - 21 = 3060

            remainder /= 60;                                //	rem = 3060 / 60 = 51
            int hr = remainder % 24;                    //	hr  = 51 % 24 = 3
            remainder -= hr;                                //	rem = 51 - 3 = 48

            int day = remainder / 24;                   //	day = 48 / 24 = 2

            string fTime = dec.ToString();
            while (fTime.Length < 2) fTime = "0" + fTime;
            fTime = sec + "." + fTime;

            if (min > 0 || hr > 0 || day > 0)
            {
                while (fTime.Length < 5) fTime = "0" + fTime;

                fTime = min + ":" + fTime;
                if (hr > 0 || day > 0)
                {
                    while (fTime.Length < 8) fTime = "0" + fTime;

                    fTime = hr + ":" + fTime;
                    if (day > 0)
                    {
                        while (fTime.Length < 11) fTime = "0" + fTime;

                        fTime = day + ":" + fTime;
                    }
                }
            }
            return fTime;
        }

        /// <summary>
        /// Formats the number to be comma separated.
        ///	Usage: value.ToCurrency() --> 1,234,567
        /// usage: value.ToCurrenty("$") --> $1,234,567
        /// </summary>
        /// <returns>The formatted number.</returns>
        /// <param name="symbol">Optional currency symbol,e.g., "$"</param>
        public static string ToCurrency(this int money, string symbol = "")
        {
            return string.Format(symbol + "{0:n0}", money);
        }

        /// <summary>
        /// Returns elapsed execution time.
        /// 
        /// The optional "format" boolean parameter indicates to use a
        /// nice format, e.g., 12:34.56, or just to return raw elapsed seconds, e.g., 754.56. This defaults
        /// to true.
        /// Usage: GetExecutionTime(false) returns "754.56"
        /// 
        /// The optional "separator" parameter allows a separator to be appended for use in console logging.
        /// Usage: GetFormattedTime(">>") returns "12:34.56 >> "
        /// 
        /// Usage: GetFormattedTime (false, "##>") returns "754.56 ##> "
        /// </summary>
        /// <returns>The formated elapsed execution time.</returns>
        /// <param name="format">Format.</param>
        /// <param name="separator">Separator.</param>
        public static string GetExecutionTime(bool format = true, string separator = "")
        {
            if (separator.Length > 0)
            {
                separator = " " + separator + " ";
            }

            if (format)
            {
                return FormatTime(Time.time) + separator;
            }
            else
            {
                return Time.time + separator;
            }
        }

        #endregion

        //////////////
        //	Math	//
        //////////////

        #region Math

        /// <summary>
        /// 2016-10-20
        /// Convert scientific notation to real number.
        /// Usage: Scientific(11.2, 3) => 11200
        /// </summary>
        public static double Scientific(double coefficient, int exponent)
        {
            return coefficient * Mathd.Pow(10, exponent);
        }

        /// <summary>
        /// Returns an eased value of y, based on the input x (0-1)
        /// value and the easing factor a. Best results for a = 1 - 3.
        /// </summary>
        public static float Ease(float x, float a)
        {
            PLog.LogNoUnitTests();
            float xa = Mathf.Pow(x, a);
            return xa / (xa+Mathf.Pow(1-x,a));
        }

        /// <summary>
        /// 2016-5-26
        /// Normalizes values to 0-1
        /// </summary>
        public static float[] Normalize(this float[] source)
        {
            float min = source.Min();
            float max = source.Max();
            for (int i = 0; i < source.Length; i++)
                source[i] = PMath.Map(source[i], min, max, 0, 1);
            return source;
        }

        /// <summary>
        ///	2016-5-10
        /// Indicates if the specified source is positive.
        /// </summary>
        public static bool IsPositive(this float source)
        {
            return source > 0;
        }

        /// <summary>
        /// 2016-5-11
        /// Indicates if the specified source is positive.
        /// </summary>
        public static bool IsPositive(this int source)
        {
            return source > 0;
        }

        /// <summary>
        ///	2016-5-10
        /// Indicates if the specified source is negative.
        /// </summary>
        public static bool IsNegative(this float source)
        {
            return source < 0;
        }

        public static bool IsEven(this int source)
        {
            return source % 2 == 0;
        }

        /// <summary>
        /// 2016-5-11
        /// Indicates if the specified source is negative.
        /// </summary>
        public static bool IsNegative(this int source)
        {
            return source < 0;
        }

        /// <summary>
        /// 2016-3-28
        /// Returns the sum of "index" x "contents"
        /// e.g., { 3, 4, 5, 6 } return 0*3 + 1*4 + 2*4 + 3*6
        /// Used for comparing randomly shuffled arrays of numbers.
        /// </summary>
        public static float WeightedSum(this float[] source)
        {
            float total = 0;
            for (int i = 0; i < source.Length; i++)
            {
                total += i * source[i];
            }
            return total;
        }

        /// <summary>
        /// 2016-3-28
        /// Returns the sum of "index" x "contents"
        /// e.g., { 3, 4, 5, 6 } return 0*3 + 1*4 + 2*4 + 3*6
        /// Used for comparing randomly shuffled arrays of numbers.
        /// </summary>
        public static float WeightedSum(this List<float> source)
        {
            float total = 0;
            for (int i = 0; i < source.Count; i++)
            {
                total += i * source[i];
            }
            return total;
        }

        /// <summary>
        ///	2016-3-27
        /// Inidicates if the left and right are approximately equal. Precision is the number of decimal places to check.
        /// </summary>
        public static bool Approx(float left, float right, float precision = 6)
        {
            return Mathf.Abs(left - right) <= Mathf.Pow(10, -precision);
        }

        /// <summary>
        /// 2016-3-29
        /// </summary>
        public static int Factorial(int n)
        {
            if (n < 0) throw new System.ArgumentOutOfRangeException("Factorial operation is mathematically undefined for negative numbers (" + n + ")");
            if (n == 0) return 1;
            int total = 1;
            for (int i = n; i > 1; i--)
            {
                total *= i;
            }

            return total;
        }

        /// <summary>
        /// 2016-3-30
        /// Maps a value from one range to another.
        /// </summary>
        public static float Map(float value, float oldMin, float oldMax, float newMin, float newMax)
        {
            if (oldMin == oldMax)
            {
                throw new System.ArgumentException("oldMin (" + oldMin + ") and oldMax (" + oldMax + ") cannot be the same. ");
            }

            float fromMin = Mathf.Min(oldMin, oldMax);
            float fromMax = Mathf.Max(oldMin, oldMax);
            float toMin = Mathf.Min(newMin, newMax);
            float toMax = Mathf.Max(newMin, newMax);

            if (value <= fromMin) return toMin;
            if (value >= fromMax) return toMax;

            float fromRange = fromMax - fromMin;
            float toRange = toMax - toMin;

            return toMin + toRange * (value - fromMin) / fromRange;
        }

        /// <summary>
        /// 2016-3-26
        /// </summary>
        public static int IndexOfMin(this float[] source)
        {
            int index = 0;
            float value = source[0];
            for (int i = 0; i < source.Length; i++)
            {
                float current = source[i];
                if (current > value) continue;
                value = current;
                index = i;
            }
            return index;
        }

        /// <summary>
        /// 2016-3-26
        /// </summary>
        public static int IndexOfMax(this float[] source)
        {
            int index = 0;
            float value = source[0];
            for (int i = 0; i < source.Length; i++)
            {
                float current = source[i];
                if (current < value) continue;
                value = current;
                index = i;
            }
            return index;
        }

        /// <summary>
        /// 2016-3-17
        /// TODO -- make this use a generic type, where T is float, int, double, long, short, uint
        /// This can get expensive for large arrays because it converts the array to a list, does
        /// the find-min operations, then converts the resulting list back to an array.
        /// </summary>
        public static float RemoveMin(this List<float> source)
        {
            if (source == null || source.Count == 0) return default(float);

            int index = 0;
            for (int i = 0; i < source.Count; i++)
            {
                if (source[i] < source[index]) index = i;
            }
            float result = source[index];
            source.RemoveAt(index);

            return result;
        }

        /// <summary>
        /// 2016-3-17
        /// TODO -- make this use a generic type, where T is float, int, double, long, short, uint
        /// This can get expensive for large arrays because it converts the array to a list, does
        /// the find-min operations, then converts the resulting list back to an array.
        /// </summary>
        public static float RemoveMax(this List<float> source)
        {
            if (source == null || source.Count == 0) return default(float);

            int index = 0;
            for (int i = 0; i < source.Count; i++)
            {
                if (source[i] > source[index]) index = i;
            }
            float result = source[index];
            source.RemoveAt(index);

            return result;
        }

        /// <summary>
        /// 2016-16-17
        /// Returns true if the value is between the provided min/max parameters.
        /// Optional parameters can be used to EXCLUDE the boundary points.
        /// The first optional parameter controls the left bound, the second 
        /// parameter controls the right bound, e.g.,
        ///     5.IsBetween(5, 7) --> true
        ///     5.IsBetween(5, 7, true, true) --> true
        ///     5.IsBetween(5, 7, false, false) --> falseq
        ///     9.IsBetween(3, 9) --> true
        ///     9.IsBetween(3, 9, true, false) --> false
        ///     9.IsBetween(3, 9, true, true) --> true
        /// </summary>
        public static bool IsBetween(this float source, float min, float max, bool includeLeft = true, bool includeRight = true)
        {
            float actualMin = Mathf.Min(min, max);
            float actualMax = Mathf.Max(min, max);
            if (includeLeft && Mathf.Approximately(actualMin, source)) return true;
            if (includeRight && Mathf.Approximately(actualMax, source)) return true;
            return source > actualMin && source < actualMax;
        }

        /// <summary>
        /// 2016-16-17
        /// Returns true if the value is between the provided min/max parameters.
        /// Optional parameters can be used to EXCLUDE the boundary points.
        /// The first optional parameter controls the left bound, the second 
        /// parameter controls the right bound, e.g.,
        ///     5.IsBetween(5, 7) --> true
        ///     5.IsBetween(5, 7, true, true) --> true
        ///     5.IsBetween(5, 7, false, false) --> falseq
        ///     9.IsBetween(3, 9) --> true
        ///     9.IsBetween(3, 9, true, false) --> false
        ///     9.IsBetween(3, 9, true, true) --> true
        /// </summary>
 	    public static bool IsBetweenInt(this int source, float min, float max, bool includeLeft = true, bool includeRight = true)
        {
            return ((float)source).IsBetween(min, max, includeLeft, includeRight);
        }

        /// <summary>
        /// 2016-3-17
        /// </summary>
        public static bool BitIsOn(this int source, int bitIndex)
        {
            return (source & (1 << bitIndex)) != 0;
        }

        /// <summary>
        /// 2016-3-27
        /// </summary>
        public static int SetBitFor(int source, int index, bool value)
        {
            int bitMask = 1 << index;
            if (value)
            {
                //	turn on bit at index
                source |= bitMask;
            }
            else
            {
                //	turn off bit at index
                source &= ~bitMask;
            }
            return source;
        }

        /// <summary>
        /// 2016-3-17
        /// </summary>
        public static bool SignMatches(this float source, float other)
        {
            return Mathf.Sign(source) == Mathf.Sign(other);
        }

        /// <summary>
        /// Sum the values of the array.
        /// </summary>
        public static int Sum(this int[] source)
        {
            int total = 0;
            foreach (int i in source)
            {
                total += i;
            }
            return total;
        }

        /// <summary>
        /// Sum the values of the array.
        /// </summary>
        public static float Sum(this float[] source)
        {
            float total = 0;
            foreach (float i in source)
            {
                total += i;
            }
            return total;
        }

        /// <summary>
        /// Sum the values of the list.
        /// </summary>
        public static float Sum(this List<float> source)
        {
            float total = 0;
            foreach (float i in source)
            {
                total += i;
            }
            return total;
        }

        /// <summary>
        /// Sum the values of the list
        /// </summary>
        public static int Sum(this List<int> source)
        {
            int total = 0;
            foreach (int i in source)
            {
                total += i;
            }
            return total;
        }

        /// <summary>
        /// Average the values of the array.
        /// </summary>
        public static float Average(this int[] source)
        {
            return (float)source.Sum() / source.Length;
        }

        /// <summary>
        /// Average the values of the array.
        /// </summary>
        public static float Average(this float[] source)
        {
            return source.Sum() / source.Length;
        }

        /// <summary>
        /// Average the values of the list.
        /// </summary>
        public static float Average(this List<float> source)
        {
            return source.Sum() / source.Count;
        }

        /// <summary>
        /// Median value the specified source array.
        /// The middle value of the array.
        ///	This converts the entire source to a list, itterates the list and sorts it.
        ///	This can be expensive.
        /// </summary>
        /// <param name="source">Source.</param>
        public static float Median(this float[] source)
        {
            if (source.Length == 0) return Mathf.NegativeInfinity;
            if (source.Length == 1) return source[0];
            float median = Mathf.NegativeInfinity;
            List<float> sorted = new List<float>();
            foreach (float f in source) sorted.Add(f);
            sorted.Sort();
            if (source.Length % 2 == 0)
            {
                //	even number of elements
                int index = source.Length / 2;
                median = 0.5f * (sorted[index] + sorted[index - 1]);
            }
            else
            {
                //	odd number of elements
                int index = (int)Mathf.Floor(source.Length / 2f);
                median = sorted[index];
            }
            return median;
        }

        /// <summary>
        /// Mode of the specified source array.
        /// </summary>
        public static float Mode(this float[] source)
        {
            if (source.Length == 0) throw new System.ArgumentException();

            var groups = source.GroupBy(v => v);
            int maxCount = groups.Max(g => g.Count());
            float mode = groups.First(g => g.Count() == maxCount).Key;
            return mode;
        }

        /// <summary>
        /// 2016-5-28
        /// Mode of the specified source array.
        /// </summary>
        public static int Mode(this int[] source)
        {
            if (source.Length == 0) throw new System.ArgumentException();

            var groups = source.GroupBy(v => v);
            int maxCount = groups.Max(g => g.Count());
            int mode = groups.First(g => g.Count() == maxCount).Key;
            return mode;
        }

        /// <summary>
        /// Truncate the specified number to the indicated decimal places. Default is 0 decimal places.
        /// Usage: 123.456.Trunc(1) returns 123.4
        /// Usage: 123.45.Trunc() returns 123
        /// </summary>
        /// <param name="decimalPlaces">Decimal places to keep.</param>
        public static float Trunc(float number, int decimalPlaces = 0)
        {
            float multiplizer = Mathf.Pow(10, decimalPlaces);
            return ((int)(number * multiplizer)) / multiplizer;
        }

        /// <summary>
        /// Returns only the decimal places of the specified number.
        /// Usage: 213.45.Remainder() returns 0.45
        /// </summary>
        public static float Remainder(float number)
        {
            return number - PMath.Trunc(number);
        }

        /// <summary>
        /// Determines if is quadratic solvable using the specified coefficients.
        /// </summary>
        /// <returns><c>true</c> if is quadratic solvable; otherwise, <c>false</c>.</returns>
        /// <param name="a">The x^2 coefficient.</param>
        /// <param name="b">The x coefficient.</param>
        /// <param name="c">The constant.</param>
        public static bool IsQuadraticSolvable(float a, float b, float c)
        {
            return b * b - 4 * a * c >= 0;
        }

        /// <summary>
        /// Solves the quadratic equation.
        /// x = (-b +/- sqrt(b^2 - 4ac)) / 2a
        /// 
        /// Use this to solve for x when solving parametric equations: ax^2 + bx + c = 0
        /// 
        /// If there is no solution, returns Vector2 (-infinity,-infinity)
        /// </summary>
        /// <returns>The quadratic solution as a Vector2.</returns>
        /// <param name="a">The x^2 coefficient.</param>
        /// <param name="b">The x coefficient.</param>
        /// <param name="c">The constant.</param>
        public static Vector2 SolveQuadratic(float a, float b, float c)
        {
            if (!IsQuadraticSolvable(a, b, c)) return Vector2.one * Mathf.NegativeInfinity;

            float disc = b * b - 4 * a * c;
            float solutionA = (-b + Mathf.Sqrt(disc)) / (2 * a);
            float solutionB = (-b - Mathf.Sqrt(disc)) / (2 * a);
            return Vector2.up * solutionA + Vector2.right * solutionB;
        }

        /// <summary>
        /// Ensures the number is no smaller than the min.
        /// </summary>
        /// <returns>The number.</returns>
        /// <param name="min">Minimum.</param>
        public static float ClampMin(float number, float min)
        {
            return Mathf.Clamp(number, min, number);
        }

        /// <summary>
        /// Ensures the number is no smaller than the min.
        /// </summary>
        /// <returns>The number.</returns>
        /// <param name="min">Minimum.</param>
        public static int ClampMin(int number, int min)
        {
            return Mathf.Clamp(number, min, number);
        }

        /// <summary>
        /// Ensures the number is no larger than the min.
        /// </summary>
        /// <returns>The number.</returns>
        /// <param name="max">Maximum.</param>
        public static float ClampMax(float number, float max)
        {
            return Mathf.Clamp(number, number, max);
        }

        /// <summary>
        /// Ensures the number is no larger than the min.
        /// </summary>
        /// <returns>The number.</returns>
        /// <param name="max">Maximum.</param>
        public static int ClampMax(int number, int max)
        {
            return Mathf.Clamp(number, number, max);
        }

        /// <summary>
        /// Round the number.
        /// </summary>
        public static float Round(float number, int decimalPlaces = 0)
        {
            float p = Mathf.Pow(10, decimalPlaces);
            return Mathf.Round(number * p) / p;
        }

        #endregion

        //////////////////
        //	Statistics	//
        //////////////////

        #region Statistics

        /// <summary>
        /// 2016-5-28
        /// </summary>
        public static float Var(params float[] data)
        {
            float mean = data.Average();
            return data.Sum(x => Mathf.Pow(x - mean, 2)) / data.Length;
        }

        /// <summary>
        /// 2016-3-18
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static float StdDev(params float[] data)
        {
            return Mathf.Sqrt(Var(data));
        }

        /// <summary>
        /// 2016-3-18
        /// Returns a new array of equal length, containing the
        /// residuals of the data.
        ///     residual = value - mean
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static float[] Residuals(params float[] data)
        {
            float[] res = new float[data.Length];
            float mean = data.Average();
            for (int i = 0; i < data.Length; i++)
            {
                res[i] = data[i] - mean;
            }
            return res;
        }

        #endregion

        //////////////////
        //	Conversion	//
        //////////////////

        #region Conversion

        /// <summary>
        /// Convert the number
        /// </summary>
        public static int ToInt(this float number)
        {
            return (int)number;
        }

        /// <summary>
        /// 2016-6-27
        /// Convert a 3d Vector to a 2d Vector.
        /// 	V(1, 2, 3) -> V(1, 3)
        /// </summary>
        public static Vector2 ToVector2(this Vector3 source)
        {
            return Vector2.right * source.x + Vector2.up * source.z;
        }

        /// <summary>
        /// 2016-6-27
        /// Convert a 2d Vector to a 3d Vector.
        /// 	V(1, 2) -> V(1, 0, 2)
        /// </summary>
        public static Vector3 ToVector3(this Vector2 source)
        {
            return Vector3.right * source.x + Vector3.forward * source.y;
        }

        /// <summary>
        /// Converts the array from ContactPoint objects to Vector3 objects.
        /// </summary>
        /// <returns>A vector3 array.</returns>
        /// <param name="contactPoints">Contact points.</param>
        public static Vector3[] ToVector3(this ContactPoint[] contactPoints)
        {
            Vector3[] pointList = new Vector3[contactPoints.Length];

            for (int i = pointList.Length; i > 0; i--)
            {
                pointList[i - 1] = contactPoints[i - 1].point;
            }

            return pointList;
        }

        #endregion

        //////////////////
        //	Physics		//
        //////////////////

        #region Physics

        /// <summary>
        /// 2016-10-20
        /// Returns the magnitude of gravity between two objects. At Earth's surface, 
        /// this value is about 9.8 m/s/s
        /// </summary>
        /// <returns>The magnitude of gravitational acceleration (m/s/s).</returns>
        /// <param name="massA">Mass a (kg)</param>
        /// <param name="massB">Mass b (kg)</param>
        /// <param name="distance">Distance (meters)</param>
        public static double GravitationalAcceleration(double massA, double massB, double distance)
        {
            return GRAVITATIONAL_CONSTANT * (massA * massB) / (distance * distance);
        }

        /// <summary>
        /// 2016-10-20
        /// Returns the magnitude of gravity between two objects. At Earth's surface, 
        /// this value is about 9.8 m/s/s
        /// </summary>
        /// <returns>The magnitude of gravitational acceleration (m/s/s).</returns>
        /// <param name="massA">Mass a (kg)</param>
        /// <param name="massB">Mass b (kg)</param>
        /// <param name="distance">Distance (meters)</param>
        public static float GravitationalAcceleration(float massA, float massB, float distance)
        {
            return (float)GravitationalAcceleration(massA, massB, (double)distance);
        }

        /// <summary>
        /// Returns the magnitude of gravity between two rigidbody objects.
        /// </summary>
        public static float GravitationalAcceleration(Rigidbody bodyA, Rigidbody bodyB)
        {
            return GravitationalAcceleration(bodyA.mass, bodyB.mass, (bodyA.position - bodyB.position).magnitude);
        }

        /// <summary>
        /// 2016-10-20
        /// Returns the magnitude of gravity between this rigidbody object, and another.
        /// </summary>
        public static float GravitationalAccelerationToward(this Rigidbody bodyA, Rigidbody bodyB)
        {
            return GravitationalAcceleration(bodyA, bodyB);
        }

        /// <summary>
        /// 2016-10-20
        ///     v = sqrt(2 * gravity * distance)
        /// Gravity is the magnitude of the gravitational attraction 
        /// between the two objects (m/s/s).
        /// Distance is the distance between the objects, in meters.
        /// Velocity returned is m/s
        /// Ref: https://en.wikipedia.org/wiki/Escape_velocity
        /// </summary>
        public static double EscapeVelocity(double gravity, double distance)
        {
            return Mathd.Sqrt(2 * gravity * distance);
        }

        /// <summary>
        /// 2016-10-20
        /// Returns the escape velocity of object B from the source object A.
        /// </summary>
        public static double EscapeVelocityFrom(this Rigidbody bodyA, Rigidbody bodyB)
        {
            double g = bodyA.GravitationalAccelerationToward(bodyB);
            double d = Vector3.Distance(bodyA.position, bodyB.position);
            return EscapeVelocity(g, d);
        }

        /// <summary>
        /// Returns the mass, derived from the density and radius of a sphere.
        /// </summary>
        /// <returns>The escape velocity.</returns>
        public static float GetMassOfSphere(float radius, float density)
        {
            return Mathf.PI * Mathf.Pow(radius, 3) * density;
        }

        /// <summary>
        /// Gets the force in Newtons. A Newton (SI unit) is defined as Kg * meter/sec^2.
        /// This is Mass & Acceleration. Since this is *impact* force, we calculate
        /// accleration based on the percentage of velocity lost to the collision.
        /// </summary>
        /// <returns>The force in Newtons.</returns>
        /// <param name="massInKilograms">Mass in kilograms.</param>
        /// <param name="speedInMetersPerSecond">Speed in meters per second.</param>
        /// <param name="percentSpeedLost">Percentage of speed lost in the collision.</param>
        public static float ImpactForceInNewtons(float massInKg, float speedInMetersPerSecond, float percentSpeedLost, float deltaTime)
        {
            float acceleration = speedInMetersPerSecond * percentSpeedLost / deltaTime;
            return massInKg * acceleration;
        }

        /// <summary>
        /// Gets the force in Newtons. A Newton (SI unit) is defined as Kg * meter/sec^2.
        /// This is Mass & Acceleration. Since this is *impact* force, we calculate
        /// accleration based on the velocity lost to the collision.
        /// </summary>
        /// <returns>The force in Newtons.</returns>
        public static float ImpactForceInNewtons(this Rigidbody source, float speedLastFrame)
        {
            float speed = Mathf.Abs(speedLastFrame - source.velocity.magnitude);
            return ImpactForceInNewtons(source.mass, speed, 1, Time.fixedDeltaTime);
        }

        /// <summary>
        /// Gets the force in Newtons. A Newton (SI unit) is defined as Kg * meter/sec^2.
        /// This is Mass & Acceleration. When called without a speedLastFrame parameter,
        /// this calculates accleration based on the entire velocity of the object.
        /// </summary>
        /// <returns>The force in Newtons.</returns>
        public static float ImpactForceInNewtons(this Rigidbody source)
        {
            float speed = source.velocity.magnitude;
            return ImpactForceInNewtons(source.mass, speed, 1, Time.fixedDeltaTime);
        }

        /// <summary>
        /// Gets the angle from ground.
        /// </summary>
        /// <returns>The vertical angle (in radians) from ground.</returns>
        /// <param name="vector">The direction vector.</param>
        public static float AngleFromGround(Vector3 direction)
        {
            float dx = direction.ToVector2().magnitude;
            float dy = direction.y;
            return Mathf.Atan2(dy, dx);
        }

        /// <summary>
        /// OLD -- suspect formula is incorrect.
        /// Gets the initial velocity for ballistic flight.
        /// </summary>
        /// <returns>The initial velocity for ballistic flight.</returns>
        /// <param name="angle">Firing angle (in radians) from horizontal</param>
        /// <param name="firer">Firer position</param>
        /// <param name="target">Target position</param>
        public static float XInitialVelocityForBallisticFlight(float angle, Vector3 firer, Vector3 target)
        {

            //	pre library update
            //float	deltaX = (target - firer).ScaleAndReturn(PMath.XZ_PLANE).magnitude;

            //	post library update
            Vector3 direction = target - firer;
            direction.Scale(PMath.XZ_PLANE);
            float deltaX = direction.magnitude;
            float deltaY = target.y - firer.y;
            float v0Squared = (Physics.gravity.magnitude * deltaX * deltaX) / (2 * Mathf.Cos(angle) * (deltaX * Mathf.Tan(angle) - deltaY));
            return Mathf.Sqrt(v0Squared);
        }

        /// <summary>
        /// 2016-3-17
        /// DEPRECIATED -- Use VelocityOfReach() as of 2016-3-17
        /// Suspect formula is incorrect.
        /// Gets the initial velocity for ballistic flight.
        /// </summary>
        /// <returns>The initial velocity for ballistic flight.</returns>
        /// <param name="theta">Firering angle (in radians) from horizontal</param>
        /// <param name="firer">Firer position</param>
        /// <param name="target">Target position</param>
        public static float YInitialVelocityForBallisticFlight(float theta, //	launch angle in radians
                                                               Vector3 firer,   //	firer position
                                                               Vector3 target)  //	target position
        {
            //	pre library update
            //float dx = (target - firer).ScaleAndReturn(PMath.XZ_PLANE).magnitude;
            //	post library update
            Vector3 dir = target - firer;
            dir.Scale(PMath.XZ_PLANE);
            float dx = dir.magnitude;

            float dy = target.y - firer.y;
            float gravity = Physics.gravity.magnitude;
            float tanTheta = Mathf.Tan(theta);
            float numerator = Mathf.Sqrt(gravity) * Mathf.Sqrt(dx) * Mathf.Sqrt((tanTheta * tanTheta) + 1);
            float denominator = Mathf.Sqrt(2 * tanTheta - (2 * gravity * dy) / dx);
            return numerator / denominator;
        }

        /// <summary>
        /// 2016-3-28
        /// Derived as follows.
        ///     theta = launch angle
        ///     v = initial velocity
        ///     vx = x component of initial velocity = v * cos (theta)
        ///     vy = y component of initial velocity = v * sin (theta)
        ///     g = constant acceleration in y direction (this value is gravity)
        ///     a = constant acceleration in x direction (this value is 0)
        ///     t = flight time
        ///     
        ///     dx = distance traveled = vx * t + 0.5 * a * t^2
        ///     solving for t gives
        ///     t = distance / vx
        ///     
        ///     dy = distance dropped = vy * t + 0.5 * g * t^2
        ///     solving for t gives
        ///     t^2 = 2(dy - vy * t) / g
        ///     
        ///     Combine the two equations of t derived from dx and dy gives
        ///     (d/vx)^2 = 2 * (dy - vy * d/vx) / g
        ///     
        ///     Substitute vx = v * cos(theta) and vy = v * sin(theta) gives
        ///     (d/(v * cos(theta)))^2 = 2 * (dy - v * sin(theta) * d / (v * cos(theta))) / g
        ///     
        ///     Multiply everyting out and solve for v gives
        ///     v = sqrt( 0.5 * d^2 * g / (h * cos^2(theta) - d * cos(theta) * sin(theta)) )
        /// </summary>
        public static float VelocityOfReach(float angleInRadians, float distance, float targetHeight)
        {
            float gravity = Physics.gravity.magnitude;
            float cos = Mathf.Cos(angleInRadians);
            float sin = Mathf.Sin(angleInRadians);
            float numerator = 0.5f * Mathf.Pow(distance, 2) * gravity;
            float denominator = targetHeight * Mathf.Pow(cos, 2) - distance * cos * sin;
            //if (PMath.Approx(denominator, 0f, 5))
            //{
            //    string message = "Arguments ({0} radians, {1} meters, {2} meters) resulted in division by zero";
            //    throw new System.ArgumentException(string.Format(message, angleInRadians, distance, targetHeight));
            //}
            return Mathf.Sqrt(numerator / denominator);
        }

        /// <summary>
        /// 2016-3-30
        ///	I suspect this is the wrong formula, or incorrect implementation of the formula.
        ///	Ref: https://en.wikipedia.org/wiki/Projectile_motion
        /// </summary>
        public static float XVelocityOfReach(float angleInRadians, float distance, float targetHeight)
        {
            float g = Physics.gravity.magnitude;
            float cos = Mathf.Cos(angleInRadians);
            float sinDouble = Mathf.Sin(2 * angleInRadians);
            float numerator = Mathf.Pow(distance, 2) * g;
            float denominator = distance * sinDouble - 2 * targetHeight * Mathf.Pow(cos, 2);
            return numerator / denominator;
        }

        /// <summary>
        /// 2016-3-28
        /// </summary>
        public static float VelocityOfReach(float angleInRadians, Vector3 firer, Vector3 target)
        {
            float distance = Vector2.Distance(firer.ToVector2(), target.ToVector2());
            float targetHeight = target.y - firer.y;
            return VelocityOfReach(angleInRadians, distance, targetHeight);
        }

        /// <summary>
        /// 2016-3-17
        /// DEPRECIATED -- Use AngleOfReach() as of 2016-3-17
        /// Gets the initial angle for ballistic flight.
        /// </summary>
        /// <returns>The launch angle for ballistic flight.</returns>
        /// <param name="speed">Projectile's initial speed</param>
        /// <param name="firer">Firer position</param>
        /// <param name="target">Target position</param>
        public static float XLaunchAngleForBallisticFlight(float speed, //	launch speed
                                                           Vector3 firer,   //	firer position
                                                           Vector3 target)  //	target position
        {
            //	pre library update
            //float dx = (target - firer).ScaleAndReturn(PMath.XZ_PLANE).magnitude;
            //	post library update
            Vector3 dir = target - firer;
            dir.Scale(PMath.XZ_PLANE);
            float dx = dir.magnitude;

            float dy = target.y - firer.y;
            float gravity = Physics.gravity.magnitude;
            float s = Mathf.Pow(speed, 4) - gravity * (gravity * dx * dx + 2 * dy * speed * speed);
            return Mathf.Atan2(speed * speed + Mathf.Sqrt(s), gravity * dx);
        }

        /// <summary>
        /// 2017-3-28
        /// </summary>
        public static float AngleOfReachDiscriminant(float speed, float distance, float targetHeight)
        {
            float gravity = Physics.gravity.magnitude;
            float v4 = Mathf.Pow(speed, 4);
            float gx2 = gravity * Mathf.Pow(distance, 2);
            float yv2 = 2 * targetHeight * Mathf.Pow(speed, 2);
            return v4 - gravity * (gx2 + yv2);
        }

        /// <summary>
        /// The angle of reach calculation is:
        ///     \theta = \arctan{\left(\frac{v^2\pm\sqrt{v^4-g(gx^2+2yv^2)}}{gx}\right)} 
        /// An angle exists if the discriminant of the square root is non-negative.
        /// Ref: https://en.wikipedia.org/wiki/Trajectory_of_a_projectile
        /// </summary>
        public static bool AngleOfReachExists(float speed, float distance, float targetHeight)
        {
            return AngleOfReachDiscriminant(speed, distance, targetHeight) >= 0;
        }

        /// <summary>
        /// 2016-3-17
        /// Refactor PMath.LaunchAngleForBallisticFlight() to AngleOfReach()
        /// To hit a target at range x and altitude y when fired from (0,0) and with initial speed v the required
        /// angle(s) of launch \theta are:
        ///     \theta = \arctan{\left(\frac{v^2\pm\sqrt{v^4-g(gx^2+2yv^2)}}{gx}\right)} 
        /// The two roots of the equation correspond to the two possible launch angles, so long as they aren't 
        /// imaginary, in which case the initial speed is not great enough to reach the point (x,y) selected. 
        /// This formula allows one to find the angle of launch needed without the restriction of y = 0.
        /// Ref: https://en.wikipedia.org/wiki/Trajectory_of_a_projectile
        /// </summary>
        public static float[] AngleOfReach(float speed, float distance, float targetHeight)
        {
            float discriminant = AngleOfReachDiscriminant(speed, distance, targetHeight);
            float[] result = new float[] { 1, 1 };
            if (discriminant < 0) return result.AssignAll(Mathf.NegativeInfinity);

            float numerator = Mathf.Pow(speed, 2);
            float denominator = Physics.gravity.magnitude * distance;
            discriminant = Mathf.Sqrt(discriminant);
            float highAngle = Mathf.Atan2(numerator + discriminant, denominator);
            float lowAngle = Mathf.Atan2(numerator - discriminant, denominator);
            result[0] = lowAngle;
            result[1] = highAngle;
            return result;
        }

        /// <summary>
        /// 2016-3-17
        /// </summary>
        public static float[] AngleOfReach(float speed, Vector3 firer, Vector3 target)
        {
            float distance = Vector3.Distance(firer, target);
            float targetHeight = target.y - firer.y;
            return AngleOfReach(speed, distance, targetHeight);
        }

        #endregion

        //////////////////
        //	Vectors		//
        //////////////////

        #region Vectors

        #region planes
        public static Vector3 XY_PLANE = Vector3.right + Vector3.up;
        public static Vector3 XZ_PLANE = Vector3.forward + Vector3.right;
        public static Vector3 YZ_PLANE = Vector3.forward + Vector3.up;
        #endregion
        #region normals
        public static Vector3 XY_NORMAL = Vector3.forward;
        public static Vector3 XZ_NORMAL = Vector3.up;
        public static Vector3 YZ_NORMAL = Vector3.right;
        #endregion

        /// <summary>
        /// 2016-10-19
        /// </summary>
        public static Vector3 ProjectOntoPlane(Vector3 planeNormal, Vector3 vector)
        {
            float distance = -Vector3.Dot(planeNormal, vector);
            return vector + planeNormal * distance;
        }

        /// <summary>
        /// Ensures the sign of all component vectors is positive, e.g., (-1, 2, -3) becomes (1, 2, 3)
        /// </summary>
        /// <returns>The vector.</returns>
        public static Vector2 Abs(Vector2 source)
        {
            source.x = Mathf.Abs(source.x);
            source.y = Mathf.Abs(source.y);
            return source;
        }

        /// <summary>
        /// Ensures the sign of all component vectors is positive, e.g., (-1, 2, -3) becomes (1, 2, 3)
        /// </summary>
        /// <returns>The vector.</returns>
        public static Vector3 Abs(Vector3 source)
        {
            source.x = Mathf.Abs(source.x);
            source.y = Mathf.Abs(source.y);
            source.z = Mathf.Abs(source.z);
            return source;
        }

        /// <summary>
        /// 2016-3-29
        /// Returns the largest the component of the source vector with the largest magnitude.
        ///	When passed a vector such as (1, 3.5, 2), this returns Vector (0, 3.5, 0)
        ///	When passed a vector such as (0, -1.5, .75), this returns Vector (0, -1.5, 0)
        /// </summary>
        public static Vector3 LargestComponentVector(this Vector3 v)
        {
            if (Vector3.zero.Equals(v)) return Vector3.zero;

            float[] d = new float[3] { Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z) };
            float max = Mathf.Max(d);

            if (max == d[0])
            {
                return Vector3.right * v.x;
            }
            if (max == d[1])
            {
                return Vector3.up * v.y;
            }
            if (max == d[2])
            {
                return Vector3.forward * v.z;
            }

            return Vector3.zero;
        }

        /// <summary>
        /// 2016-3-29
        /// Returns the largest the component of the source vector with the largest magnitude.
        ///	When passed a vector such as (1, 3.5), this returns Vector (0, 3.5)
        ///	When passed a vector such as (0, -1.5), this returns Vector (0, -1.5)
        /// </summary>
        public static Vector2 LargestComponentVector(this Vector2 v)
        {
            if (Vector2.zero.Equals(v)) return Vector2.zero;

            float[] d = new float[2] { Mathf.Abs(v.x), Mathf.Abs(v.y) };
            float max = Mathf.Max(d);

            if (max == d[0])
            {
                return Vector2.right * v.x;
            }
            if (max == d[1])
            {
                return Vector2.up * v.y;
            }

            return Vector2.zero;
        }

        /// <summary>
        /// 2016-3-29
        /// Returns the component vector of the source vector with the smallest magnitude.
        ///	When passed a vector such as (1, 3.5, 7), this returns Vector (1, 0, 0)
        ///	When passed a vector such as (-3.8, 1.5, 2), this returns Vector (0, 1.5, 0)
        /// </summary>
        public static Vector3 SmallestComponentVector(this Vector3 v)
        {
            if (Vector3.zero.Equals(v)) return Vector3.zero;

            float[] d = new float[3] { Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z) };
            float min = Mathf.Min(d);

            if (min == d[0])
            {
                return Vector3.right * v.x;
            }
            if (min == d[1])
            {
                return Vector3.up * v.y;
            }
            if (min == d[2])
            {
                return Vector3.forward * v.z;
            }

            return Vector3.zero;
        }

        /// <summary>
        /// 2016-3-29
        /// Returns the component vector of the source vector with the smallest magnitude.
        ///	When passed a vector such as (1, 3.5), this returns Vector (1, 0)
        ///	When passed a vector such as (-3.8, 1.5), this returns Vector (0, 1.5)
        /// </summary>
        public static Vector2 SmallestComponentVector(this Vector2 v)
        {
            if (Vector2.zero.Equals(v)) return Vector2.zero;

            float[] d = new float[2] { Mathf.Abs(v.x), Mathf.Abs(v.y) };
            float min = Mathf.Min(d);

            if (min == d[0])
            {
                return Vector2.right * v.x;
            }
            if (min == d[1])
            {
                return Vector2.up * v.y;
            }

            return Vector2.zero;
        }

        /// <summary>
        /// 2016-3-29
        /// Returns position + distance * direction, e.g., 
        ///     position = { 2, 4, 3 }
        ///     direction = { 0, 0, 1 }
        ///     distance = 45
        ///     Returns { 2, 4, 48 }
        /// </summary>
        public static Vector3 GetEndpointAtDistance(Vector3 position,
                                                     Vector3 direction,
                                                     float distance)
        {
            return direction.normalized * distance + position;
        }

        /// <summary>
        /// Gets the offscreen endpoint by extending the Direction vector from Position until
        /// it is off screen. Returns the position of the offscreen endpoint.
        /// 
        /// The point direction vector is progressively extended by an amount equal to stepSize.
        /// This parameter is optional and defaults to screen width.
        /// </summary>
        /// <returns>The offscreen endpoint.</returns>
        public static Vector3 GetOffscreenEndpoint(Vector3 position,
                                                    Vector3 direction,
                                                    Camera camera = null,
                                                    float stepSize = -1)
        {
            if (!camera) camera = Camera.main;
            if (stepSize <= 0) stepSize = Screen.width;

            Vector3 endPoint = position;
            float distance = 0;

            while (PUtil.IsOnScreen(endPoint))
            {
                distance += stepSize;
                endPoint = GetEndpointAtDistance(position, direction, distance);
            }
            return endPoint;
        }

        /// <summary>
        /// Finds a direction vector that is parallel to the surface of a collision object
        /// and is in a similar direction as the impact vector.
        /// For example, the direction an object will slide along a wall when it hits.
        /// </summary>
        public static Vector3 CollisionSurfaceTangent(Vector3 impactDirection, Vector3 surfaceNormal)
        {
            Vector3 verticalVector = Vector3.Cross(surfaceNormal, impactDirection);
            return Vector3.Cross(verticalVector, surfaceNormal).normalized;
        }

        /// <summary>
        /// 2016-3-29
        /// Snaps the components of the vector to whole numbers. The 'axis' parameter indicates which
        /// components to snap, e.g., (1,0,1) will snap the X and Z axis to the nearest whole numbers.
        /// </summary>
        /// <returns>The to modified vector</returns>
        /// <param name="snapAxisMask">A vector indicating which axis to snap to whole values. Non-zero components
        /// indicate the axis should be snapped, zero components indicate no snapping.</param>
        public static Vector3 SnapToAxis(Vector3 source, Vector3 snapAxisMask)
        {
            if (snapAxisMask.x != 0) source.x = Mathf.Round(source.x);
            if (snapAxisMask.y != 0) source.y = Mathf.Round(source.y);
            if (snapAxisMask.z != 0) source.z = Mathf.Round(source.z);
            return source;
        }

        /// <summary>
        /// 2016-3-21
        /// </summary>
        public static float MinComponentMagnitude(this Vector3 source)
        {
            return Mathf.Min(source.x, source.y, source.z);
        }

        /// <summary>
        /// 2016-3-21
        /// </summary>
        public static float MinComponentMagnitude(this Vector2 source)
        {
            return Mathf.Min(source.x, source.y);
        }

        /// <summary>
        /// 2016-3-21
        /// </summary>
        public static float MaxComponentMagnitude(this Vector3 source)
        {
            return Mathf.Max(source.x, source.y, source.z);
        }

        /// <summary>
        /// 2016-3-21
        /// </summary>
        public static float MaxComponentMagnitude(this Vector2 source)
        {
            return Mathf.Max(source.x, source.y);
        }

        /// <summary>
        /// 2016-11-08
        /// </summary>
        public static bool Approx( Vector3 left, Vector3 right, int precision=6)
        {
            return Approx(left.x, right.x, precision)
                && Approx(left.y, right.y, precision)
                && Approx(left.z, right.z, precision);
        }

        #endregion

        //////////////////
        //	Angles		//
        //////////////////

        #region Angles

        /// <summary>
        /// 2016-5-10
        /// Returns the sum of the angles between a point and an array
        /// of points (a polygon). If the sum is 2PI, then the point
        /// is 1) on the same plane as the polygon, and 2)
        /// contained within the polygon.
        /// 
        /// Ref: http://bbs.dartmouth.edu/~fangq/MATH/download/source/Determining%20if%20a%20point%20lies%20on%20the%20interior%20of%20a%20polygon.htm
        /// Converted C function for 3d polygons (bottom of the page)
        /// </summary>
        /// <returns></returns>
        public static float AngleSum3d(Vector3 point, Vector3[] polygon)
        {
            float sum = 0;
            Vector3 p1, p2;
            float m1, m2;

            for (int i = 0; i < polygon.Length; i++)
            {
                int left = i, right = (i + 1) % polygon.Length;
                p1 = polygon[left] - point;
                p2 = polygon[right] - point;
                m1 = p1.magnitude;
                m2 = p2.magnitude;
                if (m1 * m2 < Mathf.Epsilon)
                {
                    //  Point is on a polygon vertex, so
                    //  the point is contained by the polygon.
                    return Mathf.PI * 2;
                }
                else
                {
                    float cos = (p1.x * p2.x + p1.y * p2.y + p1.z * p2.z) / (m1 * m2);
                    sum += Mathf.Acos(cos);
                }
            }

            return sum;
        }

        /// <summary>
        /// 2016-5-10
        /// Returns the sum of the angles between a point and an array
        /// of points (a polygon). If the sum is 2PI, then the point
        /// is 1) on the same plane as the polygon, and 2)
        /// contained within the polygon.
        /// 
        /// Ref: http://bbs.dartmouth.edu/~fangq/MATH/download/source/Determining%20if%20a%20point%20lies%20on%20the%20interior%20of%20a%20polygon.htm
        /// Converted C function for 3d polygons (bottom of the page)
        /// </summary>
        /// <returns></returns>
        public static float AngleSum2d(Vector2 point, Vector2[] polygon)
        {
            float sum = 0;
            Vector2 p1, p2;
            float m1, m2;

            for (int i = 0; i < polygon.Length; i++)
            {
                int left = i, right = (i + 1) % polygon.Length;
                p1 = polygon[left] - point;
                p2 = polygon[right] - point;
                m1 = p1.magnitude;
                m2 = p2.magnitude;
                if (m1 * m2 < Mathf.Epsilon)
                {
                    //  Point is on a polygon vertex, so
                    //  the point is contained by the polygon.
                    return Mathf.PI * 2;
                }
                else
                {
                    float cos = (p1.x * p2.x + p1.y * p2.y) / (m1 * m2);
                    sum += Mathf.Acos(cos);
                }
            }

            return sum;
        }

        /// <summary>
        /// Returns the inverse cosine of the parameter, the angle (in degrees) 
        /// with Cosine as the parameter provided.
        /// </summary>
        /// <param name="cosTheta">Cosine of theta.</param>
        public static float SignedAngleFromCOS(float cosTheta)
        {
            return Mathf.Rad2Deg * Mathf.Acos(cosTheta) * Mathf.Sign(cosTheta);
        }

        /// <summary>
        /// Ensures the source value is between 0 and 360.
        /// </summary>
        /// <returns>The angle by deg.</returns>
        public static float NormalizeDegreeAngle(float angle)
        {
            while (angle < 0) angle += 360;
            while (angle > 360) angle -= 360;
            return angle;
        }

        /// <summary>
        /// Ensures the source value is between 0 and 2*PI.
        /// </summary>
        /// <returns>The angle by deg.</returns>
        public static float NormalizeRadianAngle(float angle)
        {
            while (angle < 0) angle += 2 * Mathf.PI;
            while (angle > 2 * Mathf.PI) angle -= 2 * Mathf.PI;
            return angle;
        }

        /// <summary>
        /// Returns the dot product of the angle.
        /// Equivalent to Cos(angle in degrees)
        /// </summary>
        /// <returns>The dot product.</returns>
        /// <param name="degrees">Degrees.</param>
        public static float DotD(float degrees)
        {
            return Mathf.Cos(degrees * Mathf.Deg2Rad);
        }

        /// <summary>
        /// Returns the dot product of the angle.
        /// Equivalent to Cos(angle in radians)
        /// </summary>
        /// <returns>The dot product.</returns>
        /// <param name="degrees">Degrees.</param>
        public static float DotR(float radians)
        {
            return Mathf.Cos(radians);
        }

        #endregion
    }

    /// <summary>
    /// Struct that holds a minimum and maximum value.
    /// </summary>
    [System.Serializable]
    public class Range
    {
        public float min, max;
    }

    /// <summary>
    /// Vector2 for integers.
    ///	Minimalist implementation
    /// </summary>
    [System.Serializable]
    public struct Vector2Int
    {
        public int x { get; set; }
        public int y { get; set; }

        public Vector2Int(int x, int y)
            : this()
        {
            this.x = x;
            this.y = y;
        }
    }

    /// <summary>
    /// Vector3 for integers.
    ///	Minimalist implementation
	/// </summary>
	[System.Serializable]
    public struct Vector3Int
    {
        public int x { get; set; }
        public int y { get; set; }
        public int z { get; set; }

        public Vector3Int(int x, int y, int z)
            : this()
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
    }

    /*
     * This vector class is here for reference.
    public class Vector2D
    {
        public double x
        {
            get;
            set;
        }
        public double y
        {
            get;
            set;
        }
        public Vector2D() { }
        public Vector2D(double x, double y)
        {
            this.x = x;
            this.y = y;
        }

        public virtual double Magnitude()
        {
            return System.Math.Sqrt(x * x + y * y);
        }

        public Vector2D Normalize()
        {
            double m = Magnitude();
            this.x /= m;
            this.y /= m;
            return this;
        }

        public Vector2D Rotate(double angle)
        {
            this.x *= System.Math.Cos(angle);
            this.y *= System.Math.Sin(angle);
            return this;
        }

        public enum Axis { x, y }
        public double this[Axis axis]
        {
            get
            {
                return this[(int)axis];
            }
            set
            {
                this[(int)axis] = value;
            }
        }
        public double this[int index]
        {
            get
            {
                switch (index) {
                    case 0: return this.x;
                    case 1: return this.y;
                    default: throw new System.IndexOutOfRangeException();
                }
            }
            set
            {
                switch (index)
                {
                    case 0: 
                        this.x = value;
                        break;
                    case 1:
                        this.y = value;
                        break;
                    default: 
                        throw new System.IndexOutOfRangeException();
                }

            }
        }

        /// <summary>
        /// *= automatically overloaded when * operator is overloaded.
        /// </summary>
        public static Vector2D operator *(Vector2D left, Vector2D right) {
            return new Vector2D(left.x * right.x, left.y * right.y);
        }
    }

    public class Vector3D : Vector2D
    {
        public double z { get; set; }
        public Vector3D() : base() { }
        public Vector3D(double x, double y, double z) : base(x, y) {
            this.z = z;
        }

        public override double Magnitude()
        {
            return System.Math.Sqrt(x * x + y * y + z * z);
        }

        public new Vector3D Normalize()
        {
            double m = this.Magnitude();
            this.x /= m;
            this.y /= m;
            this.z /= m;
            return this;
        }

        public new enum Axis { x, y, z }
        public double this[Axis axis]
        {
            get
            {
                return this[(int)axis];
            }
            set
            {
                this[(int)axis] = value;
            }
        }
        public new double this[int index]
        {
            get
            {
                switch (index) {
                    case 0: 
                    case 1: return base[index];
                    case 2: return this.z;
                    default: throw new System.IndexOutOfRangeException();
                }
            }
            set
            {
                switch (index)
                {
                    case 0: 
                    case 1:
                        base[index] = value;
                        break;
                    case 2:
                        this.z = value;
                        break;
                    default: 
                        throw new System.IndexOutOfRangeException();
                }
            }
        }

        /// <summary>
        /// *= automatically overloaded when * operator is overloaded.
        /// </summary>
        public static Vector3D operator *(Vector3D left, Vector3D right)
        {
            return new Vector3D(left.x * right.x, left.y * right.y, left.z * right.z);
        }
    }
    */
}
