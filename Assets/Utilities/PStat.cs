using UnityEngine;
using PLib.Math;

namespace PLib.Statistics
{
    /// <summary>
    /// 2016-1-26
    /// TODO - Reconcile with PMath. Fairly certain PMath contains some statistics
    /// functions as well.
    /// Statistics calculations
    /// </summary>
    public static class PStat
    {

        //////////////////////
        //	Static Methods	//
        //////////////////////

        #region Statistic methods

        /// <summary>
        /// Returns slope of the line that minimizes the squared error of all data points in the populations
        /// </summary>
        /// <param name="xPopulation"></param>
        /// <param name="yPopulation"></param>
        /// <returns></returns>
        public static float RegressionLineSlope(float[] xPopulation, float[] yPopulation)
        {
            float meanXY = 0;
            float meanXX = 0;
            float meanX = xPopulation.Average();
            float meanY = yPopulation.Average();
            for (int i = 0; i < xPopulation.Length; i++)
            {
                meanXY += xPopulation[i] * yPopulation[i];
                meanXX += xPopulation[i] * xPopulation[i];
            }

            meanXY /= xPopulation.Length;
            meanXX /= yPopulation.Length;

            return (meanXY - meanY * meanX) / (meanXX - meanX * meanX);
        }

        /// <summary>
        /// Returns the Sum of Squares Total.
        /// Sum of (sample - Average)
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static float SumOfSquareTotal(float[,] data)
        {
            float sst = 0;

            float totalMean = data.Average();
            foreach (float sample in data)
            {
                sst += Mathf.Pow(sample - totalMean, 2);
            }

            return sst;
        }

        /// <summary>
        /// Returns SS(Total) - SS(Treatment)
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static float SumOfSquareError(float[,] data)
        {
            return SumOfSquareTotal(data) - SumOfSquareTreatment(data);
        }

        /// <summary>
        /// Returns the sum of (treatment average - total average)^2
        /// Assumes samples are x-aligned and treatments are y-aligned
        /// So "data" looks like this:
        ///     x1  x2  x3  x4
        /// y1  
        /// y2
        /// y3
        /// 
        /// Here 'y' is the treatment row and 'x' is the sample in the treatment
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static float SumOfSquareTreatment(float[,] data)
        {
            float ssTrial = 0;
            float dataAverage = data.Average();
            float samples = data.GetLength(0);
            float treatments = data.GetLength(1);

            for (int j = 0; j < treatments; j++)
            {
                float treatmentAverage = 0;
                for (int i = 0; i < samples; i++)
                {
                    treatmentAverage += data[i, j];
                }
                treatmentAverage /= samples;
                //  trial average, minus data average, weighted by the number of samples in the trial
                ssTrial += samples * Mathf.Pow(treatmentAverage - dataAverage, 2);
            }
            return ssTrial;
        }

        /// <summary>
        /// returns SS(treatments) / df(treatments)
        /// df(treatments) is number of treatements, minus 1
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static float MeanSquareTreatment(float[,] data)
        {
            float ssTreatment = SumOfSquareTreatment(data);
            int degreesOfFreedom = data.GetLength(1) - 1;
            return ssTreatment / degreesOfFreedom;
        }

        /// <summary>
        /// This is a {ooled Estimate of the common variance witin each of the treatments (a)
        /// returns SS(Error) / df(error)
        /// df(error) is a * (n-1)
        /// where a is number of treatments, n is samples per treatment
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static float MeanSquareError(float[,] data)
        {
            float ssError = SumOfSquareError(data);
            int treatments = data.GetLength(1);
            int samples = data.GetLength(0);
            int degreesOfFreedom = treatments * (samples - 1);
            return ssError / degreesOfFreedom;
        }

        /// <summary>
        /// Returns the f(0) score for  the data
        /// This is SS(treatments) / df(treatments), divided by SS(error) / df(error)
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static float fZeroScore(float[,] data)
        {
            return MeanSquareTreatment(data) / MeanSquareError(data);
        }

        /// <summary>
        /// Expected Variance of the data is the MS(error)
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static float ExpectedVariance(float[,] data)
        {
            return MeanSquareError(data);
        }

        /// <summary>
        /// Expected Variance of the treatments is MS(treatments) - MS(error), divided by samples-per-treatments
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static float ExpectedTreatmentVariance(float[,] data)
        {
            return (MeanSquareTreatment(data) - MeanSquareError(data)) / data.GetLength(0);
        }

        /// <summary>
        /// Returns y - average(population)
        /// </summary>
        /// <param name="y"></param>
        /// <param name="population"></param>
        /// <returns></returns>
        public static float Deviation(float y, float[] population)
        {
            return y - population.Average();
        }

        /// <summary>
        /// returns z = (y - mean) / (standard deviation)
        /// </summary>
        /// <param name="y"></param>
        /// <param name="population"></param>
        /// <returns></returns>
        public static float zScore(float y, float[] population)
        {
            return Deviation(y, population) / StandardDeviation(population);
        }

        /// <summary>
        /// Returns sqrt(Variance)
        /// </summary>
        /// <param name="population"></param>
        /// <returns></returns>
        public static float StandardDeviation(float[] population)
        {
            return Mathf.Sqrt(Variance(population));
        }

        /// <summary>
        /// Returns Sum (sqr (x - avg(X)))
        /// </summary>
        /// <param name="population"></param>
        /// <returns></returns>
        public static float Variance(float[] population)
        {
            return Covariance(population, population);
        }

        /// <summary>
        /// Returns Sum ((x - avg(X)) * (y - avg(Y)))
        /// </summary>
        /// <param name="xPopulation"></param>
        /// <param name="yPopulation"></param>
        /// <returns></returns>
        public static float Covariance(float[] xPopulation, float[] yPopulation)
        {

            float averageX = xPopulation.Average();
            float averageY = yPopulation.Average();
            float covariance = 0;
            for (int i = 0; i < xPopulation.Length; i++)
            {
                covariance += (xPopulation[i] - averageX) * (yPopulation[i] - averageY);
            }

            return covariance;
        }

        #endregion

        //////////////////
        //	Extensions	//
        //////////////////

        #region Extensions

        /// <summary>
        /// Returns the average of all the values in the rectangular array.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static float Average(this float[,] source)
        {
            float totalMean = 0;
            foreach (float sample in source)
            {
                totalMean += sample;
            }
            totalMean /= source.Length;
            return totalMean;
        }

        #endregion

    }
}