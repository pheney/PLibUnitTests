using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;
using PLib.Palette;
using PLib.Math;
using PLib.General;
using PLib.TestHelper;

namespace PaletteTest
{
    [TestFixture]
    public class PaletteTest
    {
        #region Palette methods

        [Test]
        public void DefineColor_AddNamedColorToDictionary()
        {
            //  prereq
            IsDefined_IndicatesIfColorNameIsDefined();

            //  arrange
            Palette.NewDictionary();
            Color color = new Color(0.1f, 0.2f, 0.99f);
            string colorName = "DefineColor_AddNamedColorToDictionary";
            bool preDefinedColor = Palette.IsDefined(colorName);

            //  act
            Palette.DefineColor(colorName, color);
            bool postDefinedColor = Palette.IsDefined(colorName);

            //  assert
            Assert.IsFalse(preDefinedColor);
            Assert.IsTrue(postDefinedColor);
        }

        [Test]
        public void DefineColor_AddColorByNumbersToDictionary()
        {
            //  prereq
            IsDefined_IndicatesIfColorNameIsDefined();

            //  arrange
            Palette.NewDictionary();
            string colorName = "DefineColor_AddColorByNumbersToDictionary";
            float r = 0.10f, g = 0.10f, b = 0.20f;
            bool preDefinedColor = Palette.IsDefined(colorName);

            //  act
            Palette.DefineColor(colorName, r, g, b);
            bool postDefinedColor = Palette.IsDefined(colorName);

            //  assert
            Assert.IsFalse(preDefinedColor);
            Assert.IsTrue(postDefinedColor);
        }

        [Test]
        public void DefineColor_AddColorByNumbersWithAlphaToDictionary()
        {
            //  prereq
            IsDefined_IndicatesIfColorNameIsDefined();

            //  arrange
            Palette.NewDictionary();
            string colorName = "DefineColor_AddColorByNumbersWithAlphaToDictionary";
            float r = 0.10f, g = 0.10f, b = 0.20f, a = 0.5f;
            bool preDefinedColor = Palette.IsDefined(colorName);

            //  act
            Palette.DefineColor(colorName, r, g, b, a);
            bool postDefinedColor = Palette.IsDefined(colorName);

            //  assert
            Assert.IsFalse(preDefinedColor);
            Assert.IsTrue(postDefinedColor);
        }

        [Test]
        public void IsDefined_IndicatesIfColorNameIsDefined()
        {
            //  arrange
            Palette.NewDictionary();
            string colorName = "IsDefined_IndicatesIfColorIsDefined";
            float r = 0.10f, g = 0.10f, b = 0.20f, a = 0.5f;

            //  act
            bool notDefined = Palette.IsDefined(colorName);
            Palette.DefineColor(colorName, r, g, b, a);
            bool isDefined = Palette.IsDefined(colorName);

            //  assert
            Assert.IsFalse(notDefined);
            Assert.IsTrue(isDefined);
        }

        [Test]
        public void GetColor_ReturnsColorOrGrey50()
        {
            //  prereq
            DefineColor_AddNamedColorToDictionary();

            //  arrange
            Palette.NewDictionary();
            string colorName = "GetColor_ReturnsColorOrGrey50";
            Color testColor = new Color(0.10f, 0.10f, 0.20f);
            Palette.DefineColor(colorName, testColor);

            //  act
            Color foundColor = Palette.GetColor(colorName);
            Color notFoundColor = Palette.GetColor(colorName + "abc");

            //  assert
            Assert.AreEqual(testColor, foundColor);
            Assert.AreEqual(Palette.Grey50, notFoundColor);
        }

        [Test]
        public void Grey_ReturnsGreyAndStoresInDictionary()
        {
            //  arrange
            Palette.NewDictionary();
            float greyValue = 0.73145796f;
            Color expectedGrey = new Color(greyValue, greyValue, greyValue);

            //  act
            Color foundGrey = Palette.Grey(greyValue);

            //  assert
            Assert.AreEqual(expectedGrey, foundGrey);
        }

        [Test]
        public void RandomColor_ReturnsRandomColorBetween()
        {
            //  arrange
            Color startColor = new Color(0.1f, 0.9f, 0.5f, 0.1f);
            Color endColor = new Color(0.3f, 0.7f, 0.5f, 0.5f);

            //  act
            Color randomColor = Palette.RandomColor(startColor, endColor);

            //  assert
            Assert.IsTrue(randomColor.r.IsBetween(startColor.r, endColor.r), TestHelper.ShowVariable("r", randomColor.r));
            Assert.IsTrue(randomColor.g.IsBetween(startColor.g, endColor.g), TestHelper.ShowVariable("g", randomColor.g));
            Assert.IsTrue(randomColor.b.IsBetween(startColor.b, endColor.b), TestHelper.ShowVariable("b", randomColor.b));
            Assert.AreEqual(0.5f * (startColor.a + endColor.a), randomColor.a);
        }

        [Test]
        public void RandomColor_ReturnsRandomColor()
        {
            //  arrange
            List<Color> list = new List<Color>();
            int runsize = 1000;
            float alpha = 0.05f;

            //  act
            for (int i = 0; i < runsize; i++)
            {
                list.AddUnique(Palette.RandomColor());
            }

            //  assert
            Assert.IsTrue(list.Count > (1 - alpha) * runsize);

        }

        [Test]
        public void RandomSaturation_ReturnsRandomColor()
        {
            //  arrange
            Color color = Palette.Orange;
            List<Color> list = new List<Color>();
            int runsize = 1000;
            float alpha = 0.05f;

            //  act
            for (int i = 0; i < runsize; i++)
            {
                list.AddUnique(Palette.RandomizeSaturation(color, 0.5f));
            }

            //  assert
            Assert.IsTrue(list.Count > (1 - alpha) * runsize);

        }

        [Test]
        public void BlendColor_ReturnsRatioBlendedColor()
        {
            //  prereq
            IsValidColor_ExtensionReturnsAnswer();

            //  arrange
            Color left = new Color(0.1f, 0.7f, 0.4f, 0.5f);
            Color right = new Color(0.6f, 0.2f, 0.4f, 0.9f);
            float ratio = 0.1f, invRatio = 1 - ratio;
            Color expectedNoAlpha = new Color(
                left.r * ratio + right.r * invRatio,
                left.g * ratio + right.g * invRatio,
                left.b * ratio + right.b * invRatio,
                left.a);
            Color expectedYesAlpha = expectedNoAlpha;
            expectedYesAlpha.a = ratio * left.a + invRatio * right.a;

            //  act
            Color blendNoAlpha = Palette.BlendColors(left, right, ratio);
            Color blendYesAlpha = Palette.BlendColors(left, right, ratio, true);

            //  assert
            Assert.IsTrue(blendNoAlpha.IsValidColor(), TestHelper.ShowVariable("blend w/o alpha is valid: ", false));
            Assert.AreEqual(expectedNoAlpha, blendNoAlpha, "no alpha: " + TestHelper.ShowVariables(expectedNoAlpha, blendNoAlpha));

            Assert.IsTrue(blendYesAlpha.IsValidColor(), TestHelper.ShowVariable("blend w/ alpha is valid: ", false));
            Assert.AreEqual(expectedYesAlpha, blendYesAlpha, "yes alpha: " + TestHelper.ShowVariables(expectedYesAlpha, blendYesAlpha));
        }

        [Test]
        public void BlendColor_ReturnsDropBlendedColor()
        {
            //  arrange
            Color left = Color.red;
            Color right = Palette.PastelYellow;
            int leftDrops = 5, rightDrops = 2;
            int totalDrops = leftDrops + rightDrops;

            //  act
            Color blend = Palette.BlendColors(left, right, 5, 2);
            float r = left.r * (float)leftDrops / totalDrops + right.r * (float)rightDrops / totalDrops;
            float g = left.g * (float)leftDrops / totalDrops + right.g * (float)rightDrops / totalDrops;
            float b = left.b * (float)leftDrops / totalDrops + right.b * (float)rightDrops / totalDrops;

            //  assert
            Assert.IsTrue(PMath.Approx(r, blend.r), "r: " + TestHelper.ShowVariables(r, blend.r));
            Assert.IsTrue(PMath.Approx(g, blend.g), "g: " + TestHelper.ShowVariables(g, blend.g));
            Assert.IsTrue(PMath.Approx(b, blend.b), "b: " + TestHelper.ShowVariables(b, blend.b));
        }

        [Test]
        public void AverageColors_ReturnsAveragedColors()
        {
            //  prereq
            BlendColor_ReturnsRatioBlendedColor();

            //  arrange
            Color left = Color.green;
            Color right = Color.cyan;

            //  act
            Color average = Palette.AverageColors(left, right);
            Color expected = Palette.BlendColors(left, right, 0.5f);

            //  assert
            Assert.AreEqual(expected, average, TestHelper.LabelAs("left, right"));
        }

        [Test]
        public void Lighten_ReturnsLighterColor()
        {
            //  prereq
            IsValidColor_ExtensionReturnsAnswer();

            //  arrange
            Color source = new Color(0.2f, 0.3f, 0.5f);

            float percent = 0.1f;
            float distance = 1 - source.Max();
            float fullAdjust = distance * 1.0f;
            float partialAdjust = distance * percent;

            Color expectedFull = new Color(
                source.r + fullAdjust,
                source.g + fullAdjust,
                source.b + fullAdjust,
                source.a
                );
            Color expectedPartial = new Color(
                source.r + partialAdjust,
                source.g + partialAdjust,
                source.b + partialAdjust,
                source.a
                );

            //  act
            Color full = Palette.Lighten(source);
            Color partial = Palette.Lighten(source, percent);

            //  assert
            Assert.IsTrue(full.IsValidColor());
            Assert.AreEqual(expectedFull, full);

            Assert.IsTrue(partial.IsValidColor());
            Assert.AreEqual(expectedPartial, partial);
        }

        [Test]
        public void LightenBound_ReturnsLighterColor()
        {
            //  prereq
            IsValidColor_ExtensionReturnsAnswer();

            //  arrange
            Color source = new Color(1, 0.6f, 0.7f);

            float percent = 0.1f;
            float distance = 1 - 0.7f;
            float fullAdjust = distance * 1.0f;
            float partialAdjust = distance * percent;

            Color expectedFull = new Color(
                source.r,
                source.g + fullAdjust,
                source.b + fullAdjust,
                source.a
                );
            Color expectedPartial = new Color(
                source.r,
                source.g + partialAdjust,
                source.b + partialAdjust,
                source.a
                );

            //  act
            Color full = Palette.Lighten(source);
            Color partial = Palette.Lighten(source, percent);

            //  assert
            Assert.IsTrue(full.IsValidColor(), TestHelper.ShowVariable("full color is valid: ", full));
            Assert.AreEqual(expectedFull, full, "full: " + TestHelper.ShowVariables(expectedFull, full));

            Assert.IsTrue(partial.IsValidColor(), TestHelper.ShowVariable("partial color is valid: ", partial));
            Assert.AreEqual(expectedPartial, partial, "partial: " + TestHelper.ShowVariables(expectedPartial, partial));
        }

        [Test]
        public void Darken_ReturnsDarkerColor()
        {
            //  prereq
            IsValidColor_ExtensionReturnsAnswer();
            IsBlack_ExtensionReturnsCorrectReponse();
            Min_ExtensionReturnsSmallestRGBComponent();
            MathTest.MathTest math = new MathTest.MathTest();
            math.RemoveMin_ReturnsMinValueAndRemovesItFromList();

            //  arrange
            Color source = new Color(0.2f, 0.3f, 0.5f);

            float percent = 0.1f;
            float distance = source.Min();
            float fullAdjust = distance * 1.0f;
            float partialAdjust = distance * percent;

            Color expectedFull = new Color(
                source.r - fullAdjust,
                source.g - fullAdjust,
                source.b - fullAdjust,
                source.a
                );
            Color expectedPartial = new Color(
                source.r - partialAdjust,
                source.g - partialAdjust,
                source.b - partialAdjust,
                source.a
                );

            //  act
            Color full = Palette.Darken(source);
            Color partial = Palette.Darken(source, percent);

            //  assert
            Assert.IsTrue(full.IsValidColor(), "'full' "+full.ToString()+" is not a valid color");
            Assert.AreEqual(expectedFull, full, "expected color and 'full' do not match");

            Assert.IsTrue(partial.IsValidColor(), "'partial' "+partial.ToString()+" is not a valid color");
            Assert.AreEqual(expectedPartial, partial, "expected color and 'partial' do not match");
        }

        [Test]
        public void DarkenBound_ReturnsDarkerColor()
        {
            //  prereq
            IsValidColor_ExtensionReturnsAnswer();

            //  arrange
            Color source = new Color(0, 0.5f, 0.6f);

            float percent = 0.1f;
            float distance = 0.5f;
            float fullAdjust = distance * 1.0f;
            float partialAdjust = distance * percent;

            Color expectedFull = new Color(
                source.r,
                source.g - fullAdjust,
                source.b - fullAdjust,
                source.a
                );
            Color expectedPartial = new Color(
                source.r,
                source.g - partialAdjust,
                source.b - partialAdjust,
                source.a
                );

            //  act
            Color full = Palette.Darken(source);
            Color partial = Palette.Darken(source, percent);

            //  assert
            Assert.IsTrue(full.IsValidColor());
            Assert.AreEqual(expectedFull, full);

            Assert.IsTrue(partial.IsValidColor());
            Assert.AreEqual(expectedPartial, partial);
        }

        [Test]
        public void AddAlpha_AdjustsColorAlpha()
        {
            //  arrange
            Color color = new Color(0.1f, 0.2f, 0.3f, 0.5f);

            //  act
            color = Palette.AddAlpha(color, 0.1f);
            Color bound = Palette.AddAlpha(color, 9);

            //  assert
            Assert.AreEqual(0.6f, color.a, TestHelper.LabelAs("color"));
            Assert.AreEqual(1, bound.a, TestHelper.LabelAs("bound"));
        }

        [Test]
        public void SetAlpha_AdjustsColorAlpha()
        {
            //  arrange
            Color color = new Color(0.1f, 0.2f, 0.3f, 0.5f);

            //  act
            color = Palette.SetAlpha(color, 0.1f);
            Color bound = Palette.SetAlpha(color, 9);

            //  assert
            Assert.AreEqual(0.1f, color.a);
            Assert.AreEqual(1, bound.a);
        }

        [Test]
        public void RandomizeSaturation_AdjustsColor()
        {
            //  prereq
            MathTest.MathTest math = new MathTest.MathTest();
            math.IsBetween_ReturnsCorrectAnswer();

            //  arrange
            Color color = new Color(0.1f, 0.2f, 0.3f);
            float variation = 0.1f;

            //  act
            float minR = color.r;
            float maxR = minR + (1 - color.r) * variation;
            color = Palette.RandomizeSaturation(color, variation);

            //  assert
            Assert.AreNotEqual(0.1f, color.r);
            Assert.IsTrue(color.r.IsBetween(minR, maxR));
        }

        [Test]
        public void RandomizeAlpha_AdjustsAlpha()
        {
            //  prereq
            MathTest.MathTest math = new MathTest.MathTest();
            math.IsBetween_ReturnsCorrectAnswer();

            //  arrange
            Color color = new Color(0.1f, 0.2f, 0.3f, 0.5f);
            float variation = 0.25f;

            //  act
            float min = 0.5f - variation;
            float max = 0.5f + variation;
            color = Palette.RandomizeAlpha(color, variation);

            //  assert
            Assert.AreNotEqual(0.5f, color.a, "Does not always pass. Try running this test again.");
            Assert.IsTrue(color.a.IsBetween(min, max, true, true));
        }

        [Test]
        public void PastelifyUp_ReturnsPastelColor()
        {
            //  prereq
            Min_ExtensionReturnsSmallestRGBComponent();
            Max_ExtensionReturnsLargestRGBComponent();

            //  arrange
            Color dark = new Color(0.1f, 0.2f, 0.3f);
            float pastel = 200f / 255;

            //  act
            dark = Palette.Pastelify(dark);

            //  assert
            Assert.AreEqual(0.1f, dark.Min());
            Assert.AreEqual(pastel, dark.Max());
        }

        [Test]
        public void PastelifyDown_ReturnsPastelColor()
        {
            //  prereq
            Min_ExtensionReturnsSmallestRGBComponent();
            Max_ExtensionReturnsLargestRGBComponent();

            //  arrange
            Color bright = new Color(1, 0.99f, 0.9f);
            float pastel = 200f / 255;

            //  act
            bright = Palette.Pastelify(bright);

            //  assert
            Assert.IsTrue(bright.Min() < 0.9f);
            Assert.AreEqual(pastel, bright.Max());
        }

        [Test]
        public void PastelifyBound_ReturnsPastelColor()
        {
            //  prereq
            Min_ExtensionReturnsSmallestRGBComponent();
            Max_ExtensionReturnsLargestRGBComponent();

            //  arrange
            Color bound = new Color(0, 1, 0.6f);
            float pastel = 200f / 255;

            //  act
            bound = Palette.Pastelify(bound);

            //  assert
            Assert.AreEqual(0, bound.Min());
            Assert.AreEqual(pastel, bound.Max());
        }

        [Test]
        public void Normalize_ReturnsExpandedValues()
        {
            //  prereq
            Min_ExtensionReturnsSmallestRGBComponent();
            Max_ExtensionReturnsLargestRGBComponent();

            //  arrange
            Color source = new Color(0.2f, 0.3f, 0.4f);
            float min = 0.1f;
            float max = 0.9f;

            //  act
            Color full = Palette.Normalize(source);
            Color partial = Palette.Normalize(source, min, max);

            //  assert
            Assert.AreEqual(0, full.Min());
            Assert.AreEqual(1f, full.Max());

            Assert.AreEqual(min, partial.Min());
            Assert.AreEqual(max, partial.Max());
        }

        [Test]
        public void Normalize_ReturnsCompressedValues()
        {
            //  prereq
            Min_ExtensionReturnsSmallestRGBComponent();
            Max_ExtensionReturnsLargestRGBComponent();

            //  arrange
            Color source = new Color(0.1f, 0.8f, 0.9f);
            float min = 0.3f;
            float max = 0.6f;

            //  act
            Color partial = Palette.Normalize(source, min, max);

            //  assert

            Assert.AreEqual(min, partial.Min());
            Assert.AreEqual(max, partial.Max());
        }

        [Test]
        public void Desaturate_ReturnsDesaturatedColor()
        {
            //  arrange
            Color source = new Color(0.1f, 0.2f, 0.3f);

            //  act
            Color partial = Palette.Desaturate(source);
            Color full = Palette.Desaturate(source, 1.0f);

            //  assert
            Assert.IsTrue(partial.r < source.r);
            Assert.IsTrue(partial.g < source.g);
            Assert.IsTrue(partial.b < source.b);

            Assert.AreEqual(0, full.r);
            Assert.AreEqual(0, full.g);
            Assert.AreEqual(0, full.b);
        }

        [Test]
        public void Saturate_ReturnsSaturatedColor()
        {
            //  arrange
            Color source = new Color(0.1f, 0.2f, 0.3f);

            //  act
            Color partial = Palette.Saturate(source);
            Color full = Palette.Saturate(source, 1.0f);

            //  assert
            Assert.IsTrue(partial.r > source.r, TestHelper.LabelAs("partial.r (" + partial.r + ">" + source.r + ")"));
            Assert.IsTrue(partial.g > source.g, TestHelper.LabelAs("partial.g"));
            Assert.IsTrue(partial.b > source.b, TestHelper.LabelAs("partial.b"));

            Assert.AreEqual(1, full.r, TestHelper.LabelAs("partial.r"));
            Assert.AreEqual(1, full.g, TestHelper.LabelAs("partial.g"));
            Assert.AreEqual(1, full.b, TestHelper.LabelAs("partial.b"));
        }

        #endregion

        //  Complete
        #region extensions

        [Test]
        public void IsBlack_ExtensionReturnsCorrectReponse()
        {
            //  arrange

            //  act
            bool notBlack = Color.white.IsBlack();
            bool isBlack = Color.black.IsBlack();

            //  assert
            Assert.IsFalse(notBlack, "color IS black");
            Assert.IsTrue(isBlack, "color is NOT black");
        }

        [Test]
        public void IsWhite_ExtensionReturnsCorrectReponse()
        {
            //  arrange

            //  act
            bool isWhite = Color.white.IsWhite();
            bool notWhite = Color.black.IsWhite();

            //  assert
            Assert.IsFalse(notWhite, "color IS white");
            Assert.IsTrue(isWhite, "color is NOT white");
        }

        [Test]
        public void IsValidColor_ExtensionReturnsAnswer()
        {
            //  arrange
            Color invalidColor = new Color(-1, -0.5f, 6);
            Color validColor = new Color(0.1f, 0.2f, 0.3f, 0.9f);
            Color boundaryColor = new Color(0f, 1f, 0f, 1f);

            //  act
            bool invalid = invalidColor.IsValidColor();
            bool valid = validColor.IsValidColor();
            bool boundary = boundaryColor.IsValidColor();

            //  assert
            Assert.IsFalse(invalid, "invalid color incorrectly reported as 'valid'");
            Assert.IsTrue(valid, "valid color incorrectly reported as 'invalid'");
            Assert.IsTrue(boundary, "valid boundary color incorrectly reported as 'invalid'");
        }

        [Test]
        public void Min_ExtensionReturnsSmallestRGBComponent()
        {
            //  arrange
            float minRGB = 0.1f;
            Color color = new Color(minRGB, minRGB + 0.01f, minRGB + 0.02f);

            //  act
            float min = color.Min();

            //  assert
            Assert.AreEqual(minRGB, min);
        }

        [Test]
        public void Max_ExtensionReturnsLargestRGBComponent()
        {
            //  arrange
            float maxRGB = 0.1f;
            Color color = new Color(maxRGB, maxRGB + 0.01f, maxRGB + 0.02f);

            //  act
            float max = color.Min();

            //  assert
            Assert.AreEqual(maxRGB, max);
        }

        #endregion

        //  Incomplete
        #region Materials

        [Test]
        public void Metallicize_ReturnsConvertedMaterial()
        {
            throw new System.NotImplementedException();
        }

        [Test]
        public void Plasticize_ReturnsConvertedMaterial()
        {
            throw new System.NotImplementedException();
        }

        [Test]
        public void Clayify_ReturnsConvertedMaterial()
        {
            throw new System.NotImplementedException();
        }

        #endregion

    }
}
