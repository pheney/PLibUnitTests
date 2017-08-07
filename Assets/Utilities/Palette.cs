using UnityEngine;
using System.Collections.Generic;
using PLib.Rand;
using PLib.Math;

namespace PLib.Palette
{
    /// <summary>
    /// 2016-5-18
    /// Color Palette. Predefind colors and color operations.
    /// </summary>
    public static class Palette
    {
        //	From the Unity API on colors:

        //	Each color component is a floating point value with a range from 0 to 1.
        //	Components (r,g,b) define a color in RGB color space. Alpha component (a) 
        //	defines transparency - alpha of one is completely opaque, alpha of zero 
        //	is completely transparent.

        #region Colors
        #region Basics
        public static Color Orange = new Color(1, 203f / 255, 0);
        public static Color RoyalBlue = new Color(0, 190f / 255, 1);
        public static Color Aqua = new Color(0, 1, 180f / 255);
        public static Color Magenta = new Color(1, 0, 1);
        public static Color Brown = new Color(.5f, .25f, 0);
        public static Color LightBrown = new Color(.55f, .325f, .1f);
        #endregion

        #region Metals
        public static Color Gold = new Color(1, 188f / 255, 0);
        public static Color Silver = new Color(225f / 255, 209f / 255, 207f / 255);
        public static Color Copper = new Color(201f / 255, 174f / 255, 93f / 255);
        public static Color Bronze = new Color(205f / 255, 127f / 255, 50f / 255);
        public static Color SilverLight = new Color(225f / 255, 226f / 255, 227f / 255);
        public static Color SilverDark = new Color(201f / 255, 192f / 255, 187f / 255);
        public static Color BlueSilver = new Color(131f / 255, 150f / 255, 156f / 255);
        public static Color Titanium = new Color(182f / 255, 175f / 255, 169f / 255);
        public static Color TitaniumWhite = new Color(252f / 255, 1, 240f / 255);
        public static Color TitaniumYellow = new Color(238f / 255, 230f / 255, 0);
        public static Color Platinum = new Color(229f / 255, 228f / 255, 226f / 255);
        #endregion

        #region Darks
        public static Color Olive = new Color(29f / 255, 69f / 255, 0);
        public static Color Violet = new Color(79f / 255, 0, 133f / 255);
        public static Color Maroon = new Color(152f / 255, 0, 46f / 255);
        public static Color BurntRed = new Color(152f / 255, 0, 0);
        public static Color Purple = new Color(200f / 255, 0, 200f / 255);
        public static Color Khaki = new Color(189f / 255, 183f / 255, 107f / 255);
        public static Color SeaGreen = new Color(60f / 255, 179f / 255, 113f / 255);
        public static Color SlateGreen = new Color(46f / 255, 139f / 255, 87f / 255);
        public static Color SlateBlue = new Color(113f / 255, 128f / 255, 128f / 255);
        public static Color SlateGrey = new Color(113f / 255, 113f / 255, 128f / 255);
        #endregion

        #region Pastels
        public static Color ElectricBlue = new Color(200f / 255, 1, 1);
        public static Color Pink = new Color(1, 200f / 255, 1);
        public static Color PastelYellow = new Color(1, 1, 200f / 255);
        public static Color PastelRed = new Color(1, 200f / 255, 200f / 255);
        public static Color PastelGreen = new Color(200f / 255, 1, 200f / 255);
        public static Color PastelBlue = new Color(200f / 255, 200f / 255, 1);
        #endregion

        #region Greys
        public static Color Grey10 = new Color(.1f, .1f, .1f);
        public static Color Grey20 = new Color(.2f, .2f, .2f);
        public static Color Grey40 = new Color(.4f, .4f, .4f);
        public static Color Grey50 = new Color(.5f, .5f, .5f);
        public static Color Grey60 = new Color(.6f, .6f, .6f);
        public static Color Grey80 = new Color(.8f, .8f, .8f);
        public static Color Grey90 = new Color(.9f, .9f, .9f);
        #endregion
        #endregion

        //////////////////////
        //	Static Methods	//
        //////////////////////

        #region Palette methods

        private static Dictionary<string, Color> ColorDict = new Dictionary<string, Color>();
        private static Dictionary<float, Color> GreyDict = new Dictionary<float, Color>();

        /// <summary>
        ///	2016-3-30
        /// Empties the dictionary.
        /// </summary>
        public static void NewDictionary()
        {
            ColorDict.Clear();
            GreyDict.Clear();
        }

        /// <summary>
        /// Define a named color. Used as DefineColor ("Bright Red", Color(255,200,200))
        /// Use GetColor("Bright Red") to retrieve the color.
        /// </summary>
        /// <returns>The color.</returns>
        /// <param name="name">Name.</param>
        /// <param name="color">Color.</param>
        public static Color DefineColor(string name, Color color)
        {
            ColorDict.Add(name, color);
            return color;
        }

        /// <summary>
        /// Define a named color. Used as DefineColor ("Bright Red", 255,200,200)
        /// Use GetColor("Bright Red") to retrieve the color.
        /// </summary>
        /// <returns>The color.</returns>
        /// <param name="name">Name.</param>
        /// <param name="r">The red component.</param>
        /// <param name="g">The green component.</param>
        /// <param name="b">The blue component.</param>
        /// <param name="a">The alpha component.</param>
        public static Color DefineColor(string name, float r, float g, float b, float a = 1)
        {
            return DefineColor(name, new Color(r, g, b, a));
        }

        /// <summary>
        /// Determines if the specified color name has already been defined.
        /// </summary>
        /// <returns><c>true</c> if the specified name already exists; otherwise, <c>false</c>.</returns>
        /// <param name="name">Name.</param>
        public static bool IsDefined(string name)
        {
            return ColorDict.ContainsKey(name);
        }

        /// <summary>
        /// Retrieves a previously defined named color.
        /// Usage: GetColor ("BrightRed")
        /// If the color is not found, this will return grey 50%.
        /// </summary>
        /// <returns>The color.</returns>
        /// <param name="name">Name.</param>
        public static Color GetColor(string name)
        {
            Color color;
            if (ColorDict.TryGetValue(name, out color))
            {
                return color;
            }
            else
            {
                //	color not found in table
                return Grey50;
            }
        }

        /// <summary>
        /// 2016-1-15
        /// Grey at the specified value. 0 -> white, 0.5 -> 50% grey, 1 -> black
        /// </summary>
        /// <param name="value">Float value from 0-1.</param>
        public static Color Grey(float value)
        {
            Color grey;
            if (!GreyDict.TryGetValue(value, out grey))
            {
                //	color not found in table
                //	add color to table
                grey = new Color(value, value, value);
                GreyDict.Add(value, grey);
            }
            return grey;
        }

        /// <summary>
        /// Returns a random color. The new color will have RGB values between the respective RGB values
        /// of the parameter colors, and the alpha values will be averaged.
        /// 
        /// For example, RandomColor (Color (0.5,0.25,0), Color (0.78,1,0.78))
        /// The resulting color will be:
        /// 	R is between 0.5 and 0.78
        /// 	G is between 0.25 and 1
        /// 	B is between 0 and 0.78
        /// </summary>
        /// <returns>The color.</returns>
        /// <param name="startColor">Start color.</param>
        /// <param name="endColor">End color.</param>
        public static Color RandomColor(Color startColor, Color endColor)
        {
            return new Color(Random.Range(startColor.r, endColor.r),
                              Random.Range(startColor.g, endColor.g),
                              Random.Range(startColor.b, endColor.b),
                              (startColor.a + endColor.a) * .5f);
        }

        /// <summary>
        /// Returns a completely random color. The color has alpha 0.
        /// </summary>
        /// <returns>The color.</returns>
        public static Color RandomColor()
        {
            return RandomColor(Color.white, Color.black);
        }

        /// <summary>
        /// Returns a color with adjusted saturation. Equal chance to saturate or desaturate.
        /// </summary>
        /// <returns>A color.</returns>
        /// <param name="color">The source Color.</param>
        /// <param name="saturationVariance">Saturation variance.</param>
        public static Color RandomSaturation(Color color, float saturationVariance)
        {
            if (PRand.RandomBool())
            {
                return Palette.Desaturate(color, 0.5f * saturationVariance * Random.value);
            }
            else
            {
                return Palette.Saturate(color, 0.5f * saturationVariance * Random.value);
            }
        }

        /// <summary>
        /// 2016-1-15
        /// Blends the colors. Ratio is the proportion of left to right color.
        ///     0.0 indicates 0% left, 100% right
        ///     1.0 indicates 100% left, 0% right
        ///     0.25 indicates 25% left, 75% right
        /// The parameter blendAlpha is optional and indicates if alpha values are blended at the same ratio.
        /// If omitted, or passed 'false', the resulting color will have the same alpha value as
        /// the left color
        /// </summary>
        /// <returns>A color.</returns>
        /// <param name="lhs">First color.</param>
        /// <param name="rhs">Second color.</param>
        /// <param name="ratio">Ratio to mix the colors.</param>
        public static Color BlendColors(Color lhs, Color rhs, float ratio, bool blendAlpha = false)
        {
            float invRatio = 1 - ratio;
            float r = (ratio) * lhs.r + invRatio * rhs.r;
            float g = (ratio) * lhs.g + invRatio * rhs.g;
            float b = (ratio) * lhs.b + invRatio * rhs.b;
            float a = blendAlpha ? (ratio) * lhs.a + invRatio * rhs.a : lhs.a;
            return new Color(r, g, b, a);
        }

        /// <summary>
        /// 2016-1-15
        /// Blends the colors. DropsLeft and DropsRight indicate the relative mixtures of the colors.
        /// Ex, BlendColors (Red, Blue, 3, 5)
        /// 	This means for every 3 'units' of Red, add 5 'units' of Blue.
        /// 	Or add 3/8 Red and 5/8 Blue.
        /// Equivalent to BlendColors (Red, Blue, 5/8)
        /// </summary>
        /// <returns>The colors.</returns>
        /// <param name="lhs">First color</param>
        /// <param name="rhs">Second color</param>
        /// <param name="dropsLeft">Drops left color.</param>
        /// <param name="dropsRight">Drops right color.</param>
        public static Color BlendColors(Color lhs, Color rhs, int dropsLeft, int dropsRight, bool blendAlpha = false)
        {
            return BlendColors(lhs, rhs, ((float)dropsLeft) / (dropsLeft + dropsRight), blendAlpha);
        }

        /// <summary>
        /// 2016-1-15
        /// Averages the colors together. Alpha values are also blended.
        /// Equivalent to BlendColors (lhs, rhs, 0.5f)
        /// </summary>
        /// <returns>A color.</returns>
        /// <param name="lhs">First color.</param>
        /// <param name="rhs">Second color.</param>
        public static Color AverageColors(Color lhs, Color rhs, bool blendAlpha = false)
        {
            return BlendColors(lhs, rhs, 0.5f, blendAlpha);
        }

        /// <summary>
        /// 2016-3-17
        /// Lighten the specified color by the percent indicated. Default percentage is 100%.
        /// Adds the difference between the highest value and 1, to all values (adjusted by percentage).
        /// Ex, Color (0, 0.4, 0.75).Lighten(0.9)
        /// 	Highest value is 0.75, so adjustment is 0.25 * 0.9 = .225
        /// 	0 + 0.225 = 0.225
        /// 	0.4 + 0.225 = 0.625
        /// 	0.75 + 0.225 = 0.975
        /// 	So final color is (0.225, 0.625, 0.975)
        /// Lighten() shifts (+) values toward 1. Saturate() scales (*) values toward 1.
        /// </summary>
        /// <param name="percent">Percent (0-1)</param>
        public static Color Lighten(Color color, float percent = 1)
        {
            if (color.IsWhite()) return color;

            List<float> values = new List<float> { color.r, color.g, color.b };
            float max;
            do
            {
                max = values.RemoveMax();
            } while (max == 1);

            float adjustment = (1 - max) * percent;
            color.r = color.r < 1 ? color.r + adjustment : color.r;
            color.g = color.g < 1 ? color.g + adjustment : color.g;
            color.b = color.b < 1 ? color.b + adjustment : color.b;
            return color;
        }

        /// <summary>
        /// 2016-3-17
        /// Darken the specified color by the percent indicated. Default percentage is 100%.
        /// Subtracts the difference between the lowest value and 0, to all values (adjusted by percentage).
        /// Ex, Color (0.3, 0.4, 0.75).Darken(0.5)
        /// 	Lowest value is 0.3, so adjustment is 0.3 * 0.5 = 0.15
        /// 	0.3 - 0.15 = 0.15
        /// 	0.4 + 0.15 = 0.25
        /// 	0.75 + 0.15 = 0.6
        /// 	So final color is (0.15, 0.25, 0.6)
        /// Darken() shifts (-) values toward 0. Desaturate() scales (*) values toward 0.
        /// </summary>
        /// <param name="percent">Percent (0-1)</param>
        public static Color Darken(Color color, float percent = 1)
        {
            if (color.IsBlack()) return color;

            List<float> values = new List<float> { color.r, color.g, color.b };
            float min;
            do
            {
                min = values.RemoveMin();
            } while (min == 0);

            float adjustment = min * percent;
            color.r = color.r > 0 ? color.r - adjustment : color.r;
            color.g = color.g > 0 ? color.g - adjustment : color.g;
            color.b = color.b > 0 ? color.b - adjustment : color.b;
            return color;
        }

        /// <summary>
        /// Adds the alpha value to the source color's current alpha value.
        /// </summary>
        /// <returns>The modified color.</returns>
        /// <param name="alpha">Alpha to add.</param>
        public static Color AddAlpha(Color color, float alpha)
        {
            return Palette.SetAlpha(color, color.a + alpha);
        }

        /// <summary>
        /// Sets the alpha value of the source color.
        /// </summary>
        /// <returns>The modified color.</returns>
        /// <param name="alpha">New alpha value to use.</param>
        public static Color SetAlpha(Color color, float alpha)
        {
            color.a = Mathf.Clamp01(alpha);
            return color;
        }

        /// <summary>
        /// Randomizes the saturation.
        /// </summary>
        /// <returns>The saturation.</returns>
        /// <param name="color">Color.</param>
        /// <param name="variance">Float indicates how much to vary the saturation (0 none, 1 = full).</param>
        public static Color RandomizeSaturation(Color color, float variance)
        {
            return Palette.Saturate(color, variance * Random.value);
        }

        /// <summary>
        /// Randomizes the alpha value.
        /// </summary>
        /// <returns>The alpha.</returns>
        /// <param name="color">Color.</param>
        /// <param name="variance">Deviation.</param>
        public static Color RandomizeAlpha(Color color, float deviation)
        {
            return Palette.SetAlpha(color, 0.5f + deviation * Random.Range(-1, 1));
        }

        /// <summary>
        /// Converts this color into a Pastel. Normalizes the brightest component to a value of 200/255,
        ///	adjusts the other values accordingly. No effect on alpha.
        /// </summary>
        public static Color Pastelify(Color color)
        {
            float min = color.Min();
            float pastel = 200f / 255;

            if (min > pastel)
            {
                float adj = color.Max() - pastel;
                color.r -= adj;
                color.g -= adj;
                color.b -= adj;
            }
            else
            {
                color = Palette.Normalize(color, min, pastel);
            }
            return color;
        }

        /// <summary>
        /// Normalizes the highest component to a value of max, the 
        /// lowest component to a value of min, adjusts the other 
        /// values accordingly. No effect on alpha.
        ///	Color (0.1, 0.8, 0.9) mapped as (0, 1) -> Color (0, 0.96, 1)
        ///	Color (0.1, 0.8, 0.9) mapped as (0.4, 0.6) -> (0.42, 0.56, 0.58)
        /// </summary>
        public static Color Normalize(Color color, float min = 0, float max = 1.0f)
        {
            float maxRGB = color.Max();
            float minRGB = color.Min();

            color.r = PMath.Map(color.r, minRGB, maxRGB, min, max);
            color.g = PMath.Map(color.g, minRGB, maxRGB, min, max);
            color.b = PMath.Map(color.b, minRGB, maxRGB, min, max);
            return color;
        }

        /// <summary>
        /// Desaturate the specified color. Optional desaturation indicates how much to desaturate by.
        /// 1 means full desaturation (color turns black), 0 means no desaturation (no change to color).
        /// Default desaturation is 0.9f (so remove 90% of the color value).
        /// Desaturate() scales (*) values toward 0. Darken() shifts (-) values toward 0. 
        /// </summary>
        /// <param name="desaturation">Desaturation.</param>
        public static Color Desaturate(Color color, float desaturation = 0.9f)
        {
            color.r = color.r * (1 - desaturation);
            color.g = color.g * (1 - desaturation);
            color.b = color.b * (1 - desaturation);
            return color;
        }

        /// <summary>
        /// Saturate the specified color. Optional saturation indicates how much to saturate by.
        /// 1 means full saturation (color turns white).
        /// 0 means no saturation increase (color remains unchanged).
        /// Default saturation is 0.9 (so add 90% of the missing color value).
        /// Saturate() scales (*) values toward 1. Lighten() shifts (+) values toward 1. 
        /// </summary>
        /// <param name="saturation">Saturation.</param>
        public static Color Saturate(Color color, float saturation = 0.9f)
        {
            color.r = ((1 - color.r) * saturation + color.r);
            color.g = ((1 - color.g) * saturation + color.g);
            color.b = ((1 - color.b) * saturation + color.b);
            return color;
        }

        #endregion

        //////////////////
        //	Extensions	//
        //////////////////

        #region Extensions

        /// <summary>
        /// 2016-3-17
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static bool IsBlack(this Color color)
        {
            return color.r == 0 && color.g == 0 && color.b == 0;
        }

        /// <summary>
        /// 2016-3-17
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static bool IsWhite(this Color color)
        {
            return color.r == 1 && color.g == 1 && color.b == 1;
        }

        /// <summary>
        /// 2016-3-17
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static bool IsValidColor(this Color source)
        {
            float r = source.r;
            float g = source.g;
            float b = source.b;
            return r.IsBetween(0, 1) && g.IsBetween(0, 1) && b.IsBetween(0, 1);
        }

        /// <summary>
        /// 2016-3-17
        /// Returns the smallest RGB component of the color
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static float Min(this Color source)
        {
            return Mathf.Min(source.r, source.g, source.b);
        }

        /// <summary>
        /// 2016-3-17
        /// Returns the largest RGB component of the color
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static float Max(this Color source)
        {
            return Mathf.Max(source.r, source.g, source.b);
        }

        #endregion

        //////////////////
        //	Materials	//
        //////////////////

        #region Materials

        /// <summary>
        /// Convert a material to a metal. Preserves as much color information as possible, while still making it look
        /// like a metal. Parameter controls how "metallic" the object becomes.
        ///     Paramter range is 0 (brushed steel) -> 0.9 (shiny metal) -> 1.0 (chrome)
        ///     The default is 0.8125.
        ///     
        /// Prodecure:
        /// 
        ///     1. Set the shader type to "Standard" and not "Standard (Specular Setup)"
        ///     2. Set the shader properties
        ///     Albedo defines the overall color. Range (sRGB): 50-243 (plastic), 186-255 (metal)
        ///     Metallic (R): 1.0 (chrome) 0.8-0.9 (metal), 0.5-0.8 (plastic), 0.2-0.5 (worn plastic)
        ///     Smootheness for "metallic" albedo (A): 0.9-1.0 (chrome), 0.7-0.9 (shiny metal), 0.5-0.7 (brushed metal), 0.4-0.5 (old metal)
        ///     Smoothnesss for "non-metallic" albedo (A): 0.5-0.7 (plastic), 0.8-0.9 (wet surface)
        ///     
        /// Ref: http://docs.unity3d.com/ScriptReference/Shader.Find.html
        /// Ref: http://docs.unity3d.com/Manual/StandardShaderMaterialCharts.html
        /// </summary>
        public static Material Metallicize(this Material source, float metal = 0.8125f)
        {
            metal = Mathf.Clamp01(metal);
            source.shader = Shader.Find("Standard");
            source.SetFloat("_Metallic", Mathf.Lerp(0.7f, 1, metal));
            source.SetFloat("_Glossiness", Mathf.Lerp(0.4f, 1, metal));
            return source;
        }

        /// <summary>
        ///	2016-1-15
        /// Convert a material to a plastic. Preserves color and textures.
        ///     Parameter is 0 (worn plastic) -> 1.0 (brand new shiny plastic)
        ///     The default is 0.5.
        ///     
        /// Plastic:
        /// 	Shader type: Standard
        ///		Albedo (sRGB): 50-243 (plastic) -- ignored
        ///     Metallic (R): 0.5-0.8 (plastic), 0.2-0.5 (worn plastic)
        ///		Smoothness (A):  0.5-0.7 (plastic)
        ///     
        /// Ref: http://docs.unity3d.com/ScriptReference/Shader.Find.html
        /// Ref: http://docs.unity3d.com/Manual/StandardShaderMaterialCharts.html
        /// </summary>
        public static Material Plasticize(Material source, float shine = 0.5f)
        {

            shine = Mathf.Clamp01(shine);

            //	duplicat the original material
            Material metal = new Material(source);

            //	set this to  a 'metallic' material/shader
            metal.shader = Shader.Find("Standard");

            //	set the gloss and metallic properties
            metal.SetFloat("_Metallic", Mathf.Lerp(0.2f, 0.8f, shine));
            metal.SetFloat("_Glossiness", Mathf.Lerp(0.5f, 0.5f, shine));
            return metal;
        }

        /// <summary>
        ///	2016-1-15
        /// Convert a material to a glay, plaster or other non-reflective surface. Preserves color and textures.
        ///     Parameter is 0 (worn) -> 1.0 (brand new)
        ///     The default is 0.5.
        ///     
        /// Clay:
        /// 	Shader type: Standard
        ///		Albedo (sRGB): 0-100 -- ignored
        ///     Metallic (R): 0-0.1 (clay, cloth)
        ///		Smoothness (A):  0-0.2
        ///     
        /// Ref: http://docs.unity3d.com/ScriptReference/Shader.Find.html
        /// Ref: http://docs.unity3d.com/Manual/StandardShaderMaterialCharts.html
        /// </summary>
        public static Material Clayify(Material source, float wear = 0.5f)
        {

            wear = Mathf.Clamp01(wear);

            //	duplicat the original material
            Material metal = new Material(source);

            //	set this to  a 'metallic' material/shader
            metal.shader = Shader.Find("Standard");

            //	set the gloss and metallic properties
            metal.SetFloat("_Metallic", Mathf.Lerp(0, 0.1f, wear));
            metal.SetFloat("_Glossiness", Mathf.Lerp(0, 0.2f, wear));
            return metal;
        }
        #endregion

        //////////////////
        //	Structures	//
        //////////////////

        #region Sub-Palettes
        /// <summary>
        /// Holds a color pair. When created, the pair consists of 'normal', which equals 
        /// the color, and 'high', which is a copy of the original color with .Lighten() applied.
        /// </summary>
        [System.Serializable]
        public struct ColorSet2
        {
            public Color high;
            public Color normal;
            public ColorSet2(Color color)
            {
                normal = color;
                high = Palette.Lighten(color);
            }
        }

        /// <summary>
        /// Holds a 3-color set. When created, the triplet consists of 'normal', which is 
        /// the orginal color, plus 'high', which is color.Lighten() and 'low', which is color.Darken().
        /// </summary>
        [System.Serializable]
        public struct ColorSet3
        {
            public Color bright;
            public Color normal;
            public Color dark;
            public ColorSet3(Color color)
            {
                bright = Palette.Lighten(color);
                normal = color;
                dark = Palette.Darken(color);
            }
        }

        /// <summary>
        /// 2016-5-10
        /// Holds a 5-color set. When created, this consists of 'normal', which is 
        /// the orginal color, plus 'high', which is color.Lighten() and 'low', which is color.Darken(),
        /// plus a 'white' and 'black' version of the color.
        /// </summary>
        [System.Serializable]
        public struct ColorSet5
        {
            public Color white, black;
            public Color normal;
            public Color bright, dark;

            public ColorSet5(Color color)
            {
                normal = color;
                white = Palette.Normalize(color, 0.9f, 1);
                black = Palette.Normalize(color, 0, 0.1f);
                bright = Palette.Lighten(color);
                dark = Palette.Darken(color);
            }
        }
        #endregion
    }
}