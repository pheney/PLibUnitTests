using UnityEngine;
using NUnit.Framework;
using PLib.Math;
using PLib.Rand;
using PLib.General;
using PLib.TestHelper;
using System.Collections.Generic;

namespace UtilTest
{
    [TestFixture]
    public class UtilTest
    {
        //  Incomplete
        #region Altitude

        [Test]
        public void GetAltitudeAGL_UseLayerNumberCorrectly()
        {
            throw new System.NotImplementedException();
        }

        [Test]
        public void GetAltitudeAGL_UseLayerNameCorrectly()
        {
            throw new System.NotImplementedException();
        }

        #endregion

        //  Incomplete
        #region Aiming

        [Test]
        public void LookAwayFromTransform_ReturnsCorrectly()
        {
            TestHelper.CleanUpGameObjects();

            //  arrange
            GameObject main = new GameObject("main");
            GameObject target = new GameObject("target");
            target.transform.position = Vector3.forward * 10;

            //  act
            main.transform.LookAwayFrom(target.transform);

            //  assert
            Assert.AreEqual(Vector3.back, main.transform.forward, "Main pointing wrong way" + TestHelper.ShowVariables(Vector3.back, main.transform.forward));

            TestHelper.CleanUpGameObjects();
        }

        [Test]
        public void LookAwayFromGameObject_ReturnsCorrectly()
        {
            TestHelper.CleanUpGameObjects();

            //  arrange
            GameObject main = new GameObject("main");
            GameObject target = new GameObject("target");
            target.transform.position = Vector3.forward * 10;

            //  act
            main.LookAwayFrom(target);

            //  assert
            Assert.AreEqual(Vector3.back, main.transform.forward, "Main pointing wrong way" + TestHelper.ShowVariables(Vector3.back, main.transform.forward));

            TestHelper.CleanUpGameObjects();
        }

        [Test]
        public void IsAimedWithinTolerance_UsingTransformsReturnsCorrectly()
        {
            TestHelper.CleanUpGameObjects();

            //  prereq
            AimWithError_SingleAngleReturnsCorrectly();

            //  arrange
            GameObject source = new GameObject("source");
            GameObject target = new GameObject("target");
            target.transform.position = Vector3.forward;
            float maxError = 10;    //  degrees
            int count = 1000;

            //  data record
            Vector3[] directionSuccess = new Vector3[count];
            float[] aimErrorSuccess = new float[count];
            Vector3[] directionFail = new Vector3[count];
            float[] aimErrorFail = new float[count];

            //  act
            bool[] successes = new bool[count];
            for (int i = 0; i < count; i++)
            {
                source.transform.rotation = PUtil.AimAtWithError(source.transform, target.transform, Random.value * maxError);
                Vector3 aimDirection = source.transform.forward;
                Vector3 exactDirection = (target.transform.position - source.transform.position).normalized;
                directionSuccess[i] = source.transform.forward;
                aimErrorSuccess[i] = Vector3.Angle(aimDirection, exactDirection);
                successes[i] = source.transform.IsAimedWithinTolerance(target.transform, maxError);
            }

            bool[] failures = new bool[count];
            for (int i = 0; i < count; i++)
            {
                source.transform.rotation = PUtil.AimAtWithError(source.transform, target.transform, 2 * maxError);
                Vector3 aimDirection = source.transform.forward;
                Vector3 exactDirection = (target.transform.position - source.transform.position).normalized;
                directionFail[i] = source.transform.forward;
                aimErrorFail[i] = Vector3.Angle(aimDirection, exactDirection);
                failures[i] = source.transform.IsAimedWithinTolerance(target.transform, maxError);
            }
            //  assert
            for (int i = 0; i < count; i++)
            {
                Assert.IsTrue(successes[i], i + ": Source not aimed within tolerance (direction: {0}, actual error: {1} deg)", directionSuccess[i], aimErrorSuccess[i]);
                Assert.IsFalse(failures[i], i + ": Source not aimed outside tolerance (direction: {0}, actual error: {1} deg)", directionFail[i], aimErrorFail[i]);
            }

            TestHelper.CleanUpGameObjects();
        }

        [Test]
        public void IsAimedWithinTolerance_UsingVectorsReturnsCorrectly()
        {
            TestHelper.CleanUpGameObjects();

            //  prereq
            AimWithError_SingleAngleReturnsCorrectly();

            //  arrange
            GameObject source = new GameObject("source");
            GameObject target = new GameObject("target");
            target.transform.position = Vector3.forward;
            float maxError = 10;    //  degrees
            int count = 1000;

            //  data record
            Vector3[] directionSuccess = new Vector3[count];
            float[] aimErrorSuccess = new float[count];
            Vector3[] directionFail = new Vector3[count];
            float[] aimErrorFail = new float[count];

            //  act
            bool[] successes = new bool[count];
            for (int i = 0; i < count; i++)
            {
                source.transform.rotation = PUtil.AimAtWithError(source.transform, target.transform, Random.value * maxError);
                Vector3 aimDirection = source.transform.forward;
                Vector3 exactDirection = (target.transform.position - source.transform.position).normalized;
                directionSuccess[i] = source.transform.forward;
                aimErrorSuccess[i] = Vector3.Angle(aimDirection, exactDirection);
                successes[i] = PUtil.IsAimedWithinTolerance(source.transform.position, target.transform.position, aimDirection, maxError);
            }

            bool[] failures = new bool[count];
            for (int i = 0; i < count; i++)
            {
                source.transform.rotation = PUtil.AimAtWithError(source.transform, target.transform, 2 * maxError);
                Vector3 aimDirection = source.transform.forward;
                Vector3 exactDirection = (target.transform.position - source.transform.position).normalized;
                directionFail[i] = source.transform.forward;
                aimErrorFail[i] = Vector3.Angle(aimDirection, exactDirection);
                failures[i] = PUtil.IsAimedWithinTolerance(source.transform.position, target.transform.position, aimDirection, maxError);
            }
            //  assert
            for (int i = 0; i < count; i++)
            {
                Assert.IsTrue(successes[i], i + ": Source not aimed within tolerance (direction: {0}, actual error: {1} deg)", directionSuccess[i], aimErrorSuccess[i]);
                Assert.IsFalse(failures[i], i + ": Source not aimed outside tolerance (direction: {0}, actual error: {1} deg)", directionFail[i], aimErrorFail[i]);
            }

            TestHelper.CleanUpGameObjects();
        }

        [Test]
        public void LeadTarget_ReturnsCorrectly()
        {
            TestHelper.CleanUpGameObjects();
            //  prereq
            GetInterceptPoint_WorksWhenActorsStationary();
            GetInterceptPoint_WorksWhenShotAtAngle();
            GetInterceptPoint_WorksWhenShotFasterThanActors();
            GetInterceptPoint_WorksWhenShotVeryFast();

            //  arrange
            Transform firer = new GameObject("firer").transform;
            Transform target = new GameObject("target").transform;
            target.forward = Vector3.left;
            target.position = Vector3.forward;
            float targetSpeed = 10;
            float shotSpeed = 11;

            //  act
            Vector3 leadPoint = PUtil.LeadTarget(firer, target, targetSpeed, shotSpeed);

            //  assert
            Assert.AreNotEqual(Vector3.one * Mathf.Infinity, leadPoint);
            TestHelper.CleanUpGameObjects();
        }

        [Test]
        public void GetInterceptPoint_FailsWhenShotStationary()
        {
            //  prereq
            GetInterceptTime_FailsWhenShotStationary();
            GetInterceptTime_FailsWhenShotTooSlow();
            GetInterceptTime_WorksWhenShotFasterThanActors();
            GetInterceptTime_WorksWhenShotPerpendicularToActors();
            GetInterceptTime_WorksWithActorsStationary();

            //  arrange
            Vector3 relativePosition = Vector3.forward;
            Vector3 relativeVelocity = Vector3.forward;
            float shotSpeed = 0;

            //  act
            Vector3 interceptPoint = PUtil.GetInterceptPoint(relativePosition, relativeVelocity, shotSpeed);

            //  assert
            Assert.AreEqual(Vector3.one * Mathf.Infinity, interceptPoint);
        }

        [Test]
        public void GetInterceptPoint_FailsWhenShotTooSlow()
        {
            //  prereq
            GetInterceptTime_FailsWhenShotStationary();
            GetInterceptTime_FailsWhenShotTooSlow();
            GetInterceptTime_WorksWhenShotFasterThanActors();
            GetInterceptTime_WorksWhenShotPerpendicularToActors();
            GetInterceptTime_WorksWithActorsStationary();

            //  arrange
            Vector3 relativePosition = Vector3.forward;
            Vector3 relativeVelocity = Vector3.forward;
            float shotSpeed = 0.5f;

            //  act
            Vector3 interceptPoint = PUtil.GetInterceptPoint(relativePosition, relativeVelocity, shotSpeed);

            //  assert
            Assert.AreEqual(Vector3.one * Mathf.Infinity, interceptPoint);
        }

        [Test]
        public void GetInterceptPoint_WorksWhenActorsStationary()
        {
            //  prereq
            GetInterceptTime_FailsWhenShotStationary();
            GetInterceptTime_FailsWhenShotTooSlow();
            GetInterceptTime_WorksWhenShotFasterThanActors();
            GetInterceptTime_WorksWhenShotPerpendicularToActors();
            GetInterceptTime_WorksWithActorsStationary();

            //  arrange
            Vector3 relativePosition = Vector3.forward;
            Vector3 relativeVelocity = Vector3.zero;
            float shotSpeed = 0.1f;

            //  act
            Vector3 interceptPoint = PUtil.GetInterceptPoint(relativePosition, relativeVelocity, shotSpeed);

            //  assert
            Assert.AreNotEqual(Vector3.one * Mathf.Infinity, interceptPoint);
        }

        [Test]
        public void GetInterceptPoint_WorksWhenShotFasterThanActors()
        {
            //  prereq
            GetInterceptTime_FailsWhenShotStationary();
            GetInterceptTime_FailsWhenShotTooSlow();
            GetInterceptTime_WorksWhenShotFasterThanActors();
            GetInterceptTime_WorksWhenShotPerpendicularToActors();
            GetInterceptTime_WorksWithActorsStationary();

            //  arrange
            Vector3 relativePosition = Vector3.forward;
            Vector3 relativeVelocity = Vector3.forward;
            float shotSpeed = 1.1f;

            //  act
            Vector3 interceptPoint = PUtil.GetInterceptPoint(relativePosition, relativeVelocity, shotSpeed);

            //  assert
            Assert.AreNotEqual(Vector3.one * Mathf.Infinity, interceptPoint);
        }

        [Test]
        public void GetInterceptPoint_WorksWhenShotVeryFast()
        {
            //  prereq
            MathTest.MathTest math = new MathTest.MathTest();
            math.Approx_Works();
            GetInterceptTime_FailsWhenShotStationary();
            GetInterceptTime_FailsWhenShotTooSlow();
            GetInterceptTime_WorksWhenShotFasterThanActors();
            GetInterceptTime_WorksWhenShotPerpendicularToActors();
            GetInterceptTime_WorksWithActorsStationary();

            //  arrange
            Vector3 relativePosition = Vector3.forward;
            Vector3 relativeVelocity = Vector3.back * 10;
            float shotSpeed = 10000;

            //  act
            Vector3 interceptPoint = PUtil.GetInterceptPoint(relativePosition, relativeVelocity, shotSpeed);

            //  assert
            Assert.AreNotEqual(Vector3.one * Mathf.Infinity, interceptPoint);
            Assert.IsTrue(PMath.Approx(interceptPoint.magnitude, 0, 3), TestHelper.ShowNEComparison(interceptPoint.magnitude, 0));
        }

        [Test]
        public void GetInterceptPoint_WorksWhenShotAtAngle()
        {
            //  prereq
            GetInterceptTime_FailsWhenShotStationary();
            GetInterceptTime_FailsWhenShotTooSlow();
            GetInterceptTime_WorksWhenShotFasterThanActors();
            GetInterceptTime_WorksWhenShotPerpendicularToActors();
            GetInterceptTime_WorksWithActorsStationary();

            //  arrange
            Vector3 relativePosition = Vector3.forward;
            Vector3 relativeVelocity = Vector3.forward + Vector3.right;
            float shotSpeed = 2;

            //  act
            Vector3 interceptPoint = PUtil.GetInterceptPoint(relativePosition, relativeVelocity, shotSpeed);

            //  assert
            Assert.AreNotEqual(Vector3.one * Mathf.Infinity, interceptPoint);
        }

        [Test]
        public void GetInterceptTime_FailsWhenShotStationary()
        {
            //  prereq
            MathTest.MathTest math = new MathTest.MathTest();
            math.IsQuadraticSolvable_Answer();
            math.SolveQuadratic_SolutionXvalues();
            math.MinComponentMagnitude_ExtensionReturnsCorrect();
            math.MaxComponentMagnitude_ExtensionReturnsCorrect();

            //  arrange
            Vector3 relativePosition = Vector3.forward; //  1 unit apart
            Vector3 relativeVelocity = Vector3.zero;    //  both stationary
            float shotSpeed = 0;                        //  bad shot speed

            //  act
            float interceptTime = PUtil.GetInterceptTime(relativePosition, relativeVelocity, shotSpeed);

            //  assert
            Assert.IsTrue(interceptTime == Mathf.Infinity, "Intercept time invalid. " + TestHelper.ShowVariables(Mathf.Infinity, interceptTime));
        }

        [Test]
        public void GetInterceptTime_FailsWhenShotTooSlow()
        {
            //  prereq
            MathTest.MathTest math = new MathTest.MathTest();
            math.IsQuadraticSolvable_Answer();
            math.SolveQuadratic_SolutionXvalues();
            math.MinComponentMagnitude_ExtensionReturnsCorrect();
            math.MaxComponentMagnitude_ExtensionReturnsCorrect();

            //  arrange
            Vector3 relativePosition = Vector3.forward; //  1 unit apart
            Vector3 relativeVelocity = Vector3.forward; //  both moving
            float shotSpeed = 0.1f;                     //  indequate shot speed

            //  act
            float interceptTime = PUtil.GetInterceptTime(relativePosition, relativeVelocity, shotSpeed);

            //  assert
            Assert.IsTrue(interceptTime == Mathf.Infinity, "Intercept time invalid. " + TestHelper.ShowNEComparison(Mathf.Infinity, interceptTime));
        }

        [Test]
        public void GetInterceptTime_WorksWithActorsStationary()
        {
            //  prereq
            MathTest.MathTest math = new MathTest.MathTest();
            math.IsQuadraticSolvable_Answer();
            math.SolveQuadratic_SolutionXvalues();
            math.MinComponentMagnitude_ExtensionReturnsCorrect();
            math.MaxComponentMagnitude_ExtensionReturnsCorrect();

            //  arrange
            Vector3 relativePosition = Vector3.forward; //  1 unit apart
            Vector3 relativeVelocity = Vector3.zero;    //  both stationary
            float shotSpeed = 1;                        //  positive shot speed

            //  act
            float interceptTime = PUtil.GetInterceptTime(relativePosition, relativeVelocity, shotSpeed);

            //  assert
            Assert.IsTrue(interceptTime > 0, "Intercept time invalid. " + TestHelper.ShowGComparison(interceptTime, 0));
            Assert.IsTrue(interceptTime < Mathf.Infinity, "Intercept time invalid. " + TestHelper.ShowGComparison(interceptTime, Mathf.Infinity));
        }

        [Test]
        public void GetInterceptTime_WorksWhenShotFasterThanActors()
        {
            //  prereq
            MathTest.MathTest math = new MathTest.MathTest();
            math.IsQuadraticSolvable_Answer();
            math.SolveQuadratic_SolutionXvalues();
            math.MinComponentMagnitude_ExtensionReturnsCorrect();
            math.MaxComponentMagnitude_ExtensionReturnsCorrect();

            //  arrange
            Vector3 relativePosition = Vector3.forward; //  1 unit apart
            Vector3 relativeVelocity = Vector3.forward; //  both moving
            float shotSpeed = 1.1f;                     //  good shot speed

            //  act
            float interceptTime = PUtil.GetInterceptTime(relativePosition, relativeVelocity, shotSpeed);

            //  assert
            Assert.IsTrue(interceptTime > 0, "Intercept time invalid. " + TestHelper.ShowGComparison(interceptTime, 0));
            Assert.IsTrue(interceptTime < Mathf.Infinity, "Intercept time invalid. " + TestHelper.ShowGComparison(interceptTime, Mathf.Infinity));
        }

        [Test]
        public void GetInterceptTime_WorksWhenShotPerpendicularToActors()
        {
            //  prereq
            MathTest.MathTest math = new MathTest.MathTest();
            math.IsQuadraticSolvable_Answer();
            math.SolveQuadratic_SolutionXvalues();
            math.MinComponentMagnitude_ExtensionReturnsCorrect();
            math.MaxComponentMagnitude_ExtensionReturnsCorrect();

            //  arrange
            Vector3 relativePosition = Vector3.forward; //  1 unit apart
            Vector3 relativeVelocity = Vector3.left;    //  both moving
            float shotSpeed = 1.5f;                     //  good shot speed

            //  act
            float interceptTime = PUtil.GetInterceptTime(relativePosition, relativeVelocity, shotSpeed);

            //  assert
            Assert.IsTrue(interceptTime > 0, "Intercept time invalid. " + TestHelper.ShowGComparison(interceptTime, 0));
            Assert.IsTrue(interceptTime < Mathf.Infinity, "Intercept time invalid. " + TestHelper.ShowGComparison(interceptTime, Mathf.Infinity));
        }

        [Test]
        public void AimWithError_SingleAngleReturnsCorrectly()
        {
            TestHelper.CleanUpGameObjects();

            //  arrange
            int precision = 4;
            Transform firer = new GameObject("firer").transform;
            firer.position = Vector3.zero;
            Transform target = new GameObject("target").transform;
            target.position = Vector3.forward * 10;
            float errorAngle = 10;  //  degrees

            //  act
            firer.rotation = PUtil.AimAtWithError(firer, target, errorAngle);
            Vector3 toTarget = target.position - firer.position;
            float actualAngle = Vector3.Angle(firer.forward, toTarget);

            //  assert
            Assert.IsTrue(PMath.Approx(errorAngle, actualAngle, precision), TestHelper.ShowVariables(errorAngle, actualAngle));

            TestHelper.CleanUpGameObjects();
        }

        [Test]
        public void AimWithError_CorrectHorizontalError()
        {
            TestHelper.CleanUpGameObjects();

            //  arrange
            int precision = 4;
            Transform firer = new GameObject("firer").transform;
            Transform target = new GameObject("target").transform;
            firer.position = Vector3.zero;
            target.position = Vector3.forward * 10;
            float errorAngle = 10;  //  degrees

            //  act
            firer.rotation = PUtil.AimAtWithError(firer, target, errorAngle, 0);

            Vector3 toTarget = target.position - firer.position;

            //  project correct aim-vector into horizontal plane
            Vector3 toTargetH = toTarget;
            toTargetH.y = 0;
            toTargetH.Normalize();

            //  project actual aim-vector into horizontal plane
            Vector3 errorH = firer.forward;
            errorH.y = 0;
            errorH.Normalize();

            float actualAngleH = Vector3.Angle(errorH, toTargetH);

            //  assert horizontal error is correct
            Assert.IsTrue(PMath.Approx(errorAngle, actualAngleH, precision), "Horizontal assert failed: " + TestHelper.ShowVariables(errorAngle, actualAngleH));

            TestHelper.CleanUpGameObjects();
        }

        [Test]
        public void AimWithError_CorrectVerticalError()
        {
            TestHelper.CleanUpGameObjects();

            //  arrange
            int precision = 4;
            Transform firer = new GameObject("firer").transform;
            Transform target = new GameObject("target").transform;
            firer.position = Vector3.zero;
            target.position = Vector3.forward * 10;
            float errorAngle = 10;  //  degrees

            //  act
            firer.rotation = PUtil.AimAtWithError(firer, target, 0, errorAngle);

            Vector3 toTarget = target.position - firer.position;

            //  project correct aim-vector into vertical plane
            Vector3 toTargetV = toTarget;
            toTargetV.x = 0;
            toTargetV.Normalize();

            //  project actual aim-vector into vertical plane
            Vector3 errorV = firer.forward;
            errorV.x = 0;
            errorV.Normalize();

            float actualAngleV = Vector3.Angle(errorV, toTargetV);

            //  assert horizontal error is correct
            Assert.IsTrue(PMath.Approx(errorAngle, actualAngleV, precision), "Vertical assert failed: " + TestHelper.ShowVariables(errorAngle, actualAngleV));

            TestHelper.CleanUpGameObjects();
        }

        [Test]
        public void AimWithError_CorrectHorizontalAndVerticalErrors()
        {
            TestHelper.CleanUpGameObjects();

            //  prereq
            MathTest.MathTest math = new MathTest.MathTest();
            math.ProjectOntoPlane_ReturnsCorrectly();

            //  arrange
            int precision = 4;
            Transform firer = new GameObject("firer").transform;
            Transform target = new GameObject("target").transform;
            firer.position = Vector3.zero;
            target.position = Vector3.forward * 10;
            float errorAngle = 45;  //  degrees

            //  act
            firer.rotation = PUtil.AimAtWithError(firer, target, errorAngle, errorAngle);

            Vector3 correctDirection = (target.position - firer.position).normalized;
            Vector3 actualDirection = target.forward;

            //  angle between the horizontal component of the actual direction 
            //  and the horizontal component of the correct direction
            Vector3 actualComponentH = PMath.ProjectOntoPlane(Vector3.up, actualDirection);
            Vector3 correctComponentH = PMath.ProjectOntoPlane(Vector3.up, correctDirection);
            float errorAngleH = Vector3.Angle(actualComponentH, correctComponentH);

            //  angle between the vertical component of the actual direction 
            //  and the vertical component of the correct direction
            Vector3 actualComponentV = PMath.ProjectOntoPlane(firer.right, actualDirection);
            Vector3 correctComponentV = PMath.ProjectOntoPlane(firer.right, correctDirection);
            float errorAngleV = Vector3.Angle(actualComponentV, correctComponentV);

            Debug.Log(string.Format("Horizontal error: {0} expected, {1} actual", errorAngle, errorAngleH));
            Debug.Log(string.Format("Vertical error: {0} expected, {1} actual", errorAngle, errorAngleV));

            //  assert horizontal error is correct
            Assert.IsTrue(PMath.Approx(errorAngle, errorAngleH, precision), "Horizontal error component is wrong: " + TestHelper.ShowVariables(errorAngle, errorAngleH));

            //  assert horizontal error is correct
            Assert.IsTrue(PMath.Approx(errorAngle, errorAngleV, precision), "Vertical error component is wrong: " + TestHelper.ShowVariables(errorAngle, errorAngleV));

            TestHelper.CleanUpGameObjects();
        }

        [Test]
        public void AimWithErrorBetween_ReturnsCorrectly()
        {
            TestHelper.CleanUpGameObjects();

            //  prereq
            MathTest.MathTest math = new MathTest.MathTest();
            math.Approx_Works();

            //  arrange
            Transform firer = new GameObject("firer").transform;
            firer.position = Vector3.zero;
            Transform target = new GameObject("target").transform;
            target.position = Vector3.forward * 10;
            float errorAngleMin = 10;  //  degrees
            float errorAngleMax = 20;  //  degrees

            //  act
            firer.rotation = PUtil.AimAtWithErrorBetween(firer, target, errorAngleMin, errorAngleMax);
            Vector3 toTarget = target.position - firer.position;
            float actualAngle = Vector3.Angle(firer.forward, toTarget);

            //  assert
            Assert.IsTrue(errorAngleMin <= actualAngle, TestHelper.ShowVariables(errorAngleMin, actualAngle));
            Assert.IsTrue(errorAngleMax >= actualAngle, TestHelper.ShowVariables(errorAngleMax, actualAngle));
            Assert.IsTrue(!PMath.Approx(actualAngle, 0), TestHelper.ShowVariables(0, actualAngle));

            TestHelper.CleanUpGameObjects();
        }

        #endregion

        //  Incomplete
        #region Environment

        [Test]
        public void SetFog_ReturnsCorrectly()
        {
            throw new System.NotImplementedException();
        }

        #endregion

        //  Complete
        #region Screen space

        [Test]
        public void ScreenSize_ReturnsCorrectly()
        {
            throw new System.NotImplementedException();
        }

        [Test]
        public void IsOnScreen_ReturnsCorrectly()
        {
            //  cleanup
            TestHelper.ResetAllCameras();

            //  arrange
            Camera c = Camera.main;
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);

            //  act
            bool gameObjectIsSeen = cube.IsOnScreen(c);
            bool transformIsSeen = cube.transform.IsOnScreen(c);
            bool vectorIsSeen = cube.transform.position.IsOnScreen(c);

            //  assert
            Assert.IsTrue(gameObjectIsSeen);
            Assert.IsTrue(transformIsSeen);
            Assert.IsTrue(vectorIsSeen);
        }

        [Test]
        public void PositionToScreenSpace_ReturnsCorrectly()
        {
            //  cleanup
            TestHelper.ResetAllCameras();

            //  arrange
            Camera c = Camera.main;
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);

            //  act
            Vector3 gameObjectScreenSpace = cube.PositionToScreenSpace(c);
            Vector3 transformScreenSpace = cube.transform.PositionToScreenSpace(c);
            Vector3 vectorScreenSpace = cube.transform.position.PositionToScreenSpace(c);

            //  assert
            Assert.IsTrue(false, gameObjectScreenSpace.ToString());
            Assert.IsTrue(false, transformScreenSpace.ToString());
            Assert.IsTrue(false, vectorScreenSpace.ToString());
        }

        [Test]
        public void CanSeeRenderer_UsingGameObject()
        {
            //  cleanup
            TestHelper.ResetAllCameras();

            //  arrange
            Camera c = Camera.main;
            c.transform.forward = Vector3.forward;
            c.transform.position = Vector3.zero;

            GameObject canSee = GameObject.CreatePrimitive(PrimitiveType.Cube);
            canSee.transform.position = Vector3.forward * 10;

            GameObject cannotSee = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cannotSee.transform.position = Vector3.forward * -10;

            //  act
            bool seeRenderer = c.CanSee(canSee.GetComponent<Renderer>());
            bool seeGameObject = c.CanSee(canSee);
            bool seeVector = c.CanSee(canSee.transform.position);

            bool notSeeRenderer = c.CanSee(cannotSee.GetComponent<Renderer>());
            bool notSeeGameObject = c.CanSee(cannotSee);
            bool notSeeVector = c.CanSee(cannotSee.transform.position);

            //  assert
            Assert.IsTrue(seeRenderer, "Renderer not within camera frustum");
            Assert.IsTrue(seeGameObject, "GameObject not within camera frustum");
            Assert.IsTrue(seeVector, "Point not within camera frustum");

            Assert.IsFalse(notSeeRenderer, "Renderer should not be within camera frustum");
            Assert.IsFalse(notSeeGameObject, "GameObject should not be within camera frustum");
            Assert.IsFalse(notSeeVector, "Point should not be within camera frustum");
        }

        #endregion

        //  Complete
        #region Sorting

        [Test]
        public void SortByDistanceFrom_UsingGameObject()
        {
            TestHelper.CleanUpGameObjects();

            //  arrange
            GameObject target = new GameObject("target");
            target.transform.position = Vector3.zero;

            int count = 1000;
            List<GameObject> list = new List<GameObject>();
            for (int i = 0; i < count; i++)
            {
                GameObject g = new GameObject(i.ToString());
                g.transform.position = Vector3.forward * Random.Range(1f, count);
                list.Add(g);
            }

            //  act
            list.SortByDistanceFrom(target);

            //  assert
            float last = list[0].transform.position.z;
            for (int i = 1; i < count; i++)
            {
                float curr = list[i].transform.position.z;
                Assert.IsTrue(curr >= last, "Distances are not in order: " + TestHelper.ShowVariables(curr, last));
                last = curr;
            }

            TestHelper.CleanUpGameObjects();
        }

        [Test]
        public void SortByDistanceFrom_UsingTransform()
        {
            TestHelper.CleanUpGameObjects();

            //  arrange
            Transform target = new GameObject("target").transform;
            target.transform.position = Vector3.zero;

            int count = 1000;
            List<Transform> list = new List<Transform>();
            for (int i = 0; i < count; i++)
            {
                Transform g = new GameObject(i.ToString()).transform;
                g.transform.position = Vector3.forward * Random.Range(1f, count);
                list.Add(g);
            }

            //  act
            list.SortByDistanceFrom(target);

            //  assert
            float last = list[0].position.z;
            for (int i = 1; i < count; i++)
            {
                float curr = list[i].position.z;
                Assert.IsTrue(curr >= last, "Distances are not in order: " + TestHelper.ShowVariables(curr, last));
                last = curr;
            }

            TestHelper.CleanUpGameObjects();
        }

        [Test]
        public void SortByDistanceFrom_UsingVector()
        {
            TestHelper.CleanUpGameObjects();

            //  arrange
            Vector3 target = Vector3.zero;

            int count = 1000;
            List<Vector3> list = new List<Vector3>();
            for (int i = 0; i < count; i++)
            {
                Vector3 g = Vector3.forward * Random.Range(1f, count);
                list.Add(g);
            }

            //  act
            list.SortByDistanceFrom(target);

            //  assert
            float last = list[0].z;
            for (int i = 1; i < count; i++)
            {
                float curr = list[i].z;
                Assert.IsTrue(curr >= last, "Distances are not in order: " + TestHelper.ShowVariables(curr, last));
                last = curr;
            }

            TestHelper.CleanUpGameObjects();
        }

        [Test]
        public void SortByRange_ReturnsCorrectly()
        {
            //  arrange
            List<RaycastHit> list = new List<RaycastHit>();
            int count = 1000;
            for (int i = 0; i < count; i++)
            {
                RaycastHit r = new RaycastHit();
                r.distance = Random.Range(1f, count);
                list.Add(r);
            }

            //  act
            list.SortByRange();

            //  assert
            float last = list[0].distance;
            for (int i = 1; i < count; i++)
            {
                float curr = list[i].distance;
                Assert.IsTrue(curr >= last, "Distances are not in order: " + TestHelper.ShowVariables(curr, last));
                last = curr;
            }
        }

        [Test]
        public void SortByAccuracy_ReturnsCorrectly()
        {
            TestHelper.CleanUpGameObjects();

            //  prereq
            RandomTest.RandomTest randomTest = new RandomTest.RandomTest();
            randomTest.RandomVector3_ReturnsCorrectly();

            //  arrange
            GameObject target = GameObject.CreatePrimitive(PrimitiveType.Cube);
            target.transform.position = Vector3.forward;

            List<RaycastHit> list = new List<RaycastHit>();
            int count = 1000;
            for (int i = 0; i < count; i++)
            {
                RaycastHit info;
                Vector3 aimOffset = Vector3.up * Random.value * 0.1f;
                Vector3 source = Vector3.zero;
                Vector3 direction = target.transform.position + aimOffset - source;
                Physics.Raycast(source, direction, out info);

                list.Add(info);
            }

            //  act
            list.SortByRange();

            //  assert
            float last = (list[0].point - list[0].transform.position).magnitude;
            for (int i = 1; i < count; i++)
            {
                float curr = (list[i].point - list[i].transform.position).magnitude;
                Assert.IsTrue(curr >= last, "Accuracies are not in order: " + TestHelper.ShowVariables(curr, last));
                last = curr;
            }

            TestHelper.CleanUpGameObjects();
        }

        [Test]
        public void CompareRaycastHitByAccuracy_ReturnsCorrectly()
        {
            TestHelper.CleanUpGameObjects();

            //  arrange
            GameObject target = GameObject.CreatePrimitive(PrimitiveType.Cube);
            target.transform.position = Vector3.forward * 2;
            Vector3 targetPosition = target.transform.position;
            Vector3 nearPosition = Vector3.forward;
            Vector3 farPosition = Vector3.zero;
            Vector3 aimOffset = Vector3.up * 0.1f;
            RaycastHit nearInfo, farInfo;
            Physics.Raycast(nearPosition, targetPosition - nearPosition, out nearInfo);
            Physics.Raycast(farPosition, aimOffset + targetPosition - farPosition, out farInfo);
            float nearAccuracy = Vector3.zero.magnitude;
            float farAccuracy = aimOffset.magnitude;

            //  act
            int result = PUtil.CompareRaycastHitByAccuracy(nearInfo, farInfo);
            int expected = nearAccuracy.CompareTo(farAccuracy);

            //  assert
            Assert.AreEqual(expected, result, "Accuracies are not in order: " + TestHelper.ShowVariables(expected, result));

            TestHelper.CleanUpGameObjects();
        }

        [Test]
        public void CompareDistanceToTarget_ReturnCorrectly()
        {
            TestHelper.CleanUpGameObjects();

            //  arrange
            GameObject close = new GameObject("close");
            close.transform.position = Vector3.forward;
            GameObject far = new GameObject("far");
            far.transform.position = Vector3.forward * 5;
            GameObject target = new GameObject("target");
            target.transform.position = Vector3.zero;

            float closeDistance = (target.transform.position - close.transform.position).magnitude;
            float farDistance = (target.transform.position - far.transform.position).magnitude;

            //  act
            int resultG = PUtil.CompareDistanceToTarget(close, far, target);
            int resultT = PUtil.CompareDistanceToTarget(close.transform, far.transform, target.transform);
            int resultV = PUtil.CompareDistanceToTarget(close.transform.position, far.transform.position, target.transform.position);
            int expected = closeDistance.CompareTo(farDistance);

            //  assert
            Assert.AreEqual(expected, resultG);
            Assert.AreEqual(expected, resultT);
            Assert.AreEqual(expected, resultV);

            TestHelper.CleanUpGameObjects();
        }

        [Test]
        public void CompareInstanceID_ReturnsCorrectly()
        {
            TestHelper.CleanUpGameObjects();

            //  arrange
            int size = 1000;
            List<GameObject> goList = new List<GameObject>();
            for (int i = 0; i < size; i++)
            {
                GameObject g = new GameObject();
                g.name = g.GetInstanceID().ToString();
                goList.Insert(Random.Range(0, goList.Count), g);
            }

            //  act
            goList.Sort((lhs, rhs) =>
            {
                return PUtil.CompareInstanceID(lhs.transform, rhs.transform);
            });

            //  perfectly valid alternative is to use a delegate
            //goList.Sort(delegate (GameObject lhs, GameObject rhs)
            //{
            //    return PUtil.CompareInstanceID(lhs.transform, rhs.transform);
            //});

            //  assert
            GameObject last = goList[0];
            for (int i = 1; i < goList.Count; i++)
            {
                Assert.IsTrue(goList[i].GetInstanceID() > last.GetInstanceID(), "Failed at indices {0}, {1}", i - 1, i);
                last = goList[i];
            }
            goList.Clear();
            TestHelper.CleanUpGameObjects();
        }

        #endregion

        //  Complete
        #region Array Type Conversion

        [Test]
        public void ArrayToList_ReturnsCorrectly()
        {
            //  arrange
            int count = 100;
            float[] data = new float[count];
            for (int i = 0; i < count; i++)
            {
                data[i] = UnityEngine.Random.value;
            }

            //  act
            List<float> dataList = new List<float>();
            dataList = data.ToList();

            //  assert
            Assert.AreEqual(count, dataList.Count);
            for (int i = 0; i < count; i++)
            {
                Assert.AreEqual(data[i], dataList[i], "index {0} are not equal {1} <> {2}", i, data[i], dataList[i]);
            }
        }

        [Test]
        public void ToTransformArray_ReturnsCorrectly()
        {
            //  arrange
            int count = 100;
            GameObject[] data = new GameObject[count];
            for (int i = 0; i < count; i++)
            {
                data[i] = GameObject.CreatePrimitive(PrimitiveType.Cube);
                data[i].transform.position = PRand.RandomVector3();
                data[i].transform.localScale = PRand.RandomVector3();
                data[i].transform.rotation = PRand.RandomQuaternion();
            }

            //  act
            Transform[] transformData = data.ToTransformArray();

            //  assert
            Assert.AreEqual(count, transformData.Length);
            for (int i = 0; i < count; i++)
            {
                Assert.AreEqual(data[i], transformData[i].gameObject, "index {0} are not equal {1} <> {2}", i, data[i], transformData[i].gameObject);
                Assert.AreEqual(data[i].transform, transformData[i], "index {0} do not have the same transform {1} <> {2}", i, data[i].transform.transform, transformData[i]);
            }
        }

        [Test]
        public void ToPositionArray_ReturnsCorrectly()
        {
            //  arrange
            int count = 100;
            Transform[] data = new Transform[count];
            for (int i = 0; i < count; i++)
            {
                data[i] = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
                data[i].transform.position = PRand.RandomVector3();
            }

            //  act
            Vector3[] transformData = data.ToPositionArray();

            //  assert
            Assert.AreEqual(count, transformData.Length);
            for (int i = 0; i < count; i++)
            {
                Assert.AreEqual(data[i].position, transformData[i], "index {0} do not have the same position {1} <> {2}", i, data[i].position, transformData[i]);
            }
        }

        [Test]
        public void ToRotationArray_ReturnsCorrectly()
        {
            //  arrange
            int count = 100;
            Transform[] data = new Transform[count];
            for (int i = 0; i < count; i++)
            {
                data[i] = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
                data[i].transform.rotation = PRand.RandomQuaternion();
            }

            //  act
            Quaternion[] quaternionData = data.ToRotationArray();

            //  assert
            Assert.AreEqual(count, quaternionData.Length);
            for (int i = 0; i < count; i++)
            {
                Assert.AreEqual(data[i].rotation, quaternionData[i], "index {0} do not have the same rotation {1} <> {2}", i, data[i].rotation, quaternionData[i]);
            }
        }

        [Test]
        public void ToEulerArray_ReturnsCorrectly()
        {
            //  arrange
            int count = 100;
            Quaternion[] data = new Quaternion[count];
            for (int i = 0; i < count; i++)
            {
                data[i] = PRand.RandomQuaternion();
            }

            //  act
            Vector3[] eulerData = data.ToEulerArray();

            //  assert
            Assert.AreEqual(count, eulerData.Length);
            for (int i = 0; i < count; i++)
            {
                Assert.AreEqual(data[i].eulerAngles, eulerData[i], "index {0} do not have the same euler values {1} <> {2}", i, data[i].eulerAngles, eulerData[i]);
            }
        }

        #endregion

        //  Complete
        #region Lists and Arrays

        [Test]
        public void ContentsToString_ArrayExtensionWorks()
        {
            //  arrange
            string[] words = new string[] { "the", "quick", "fox" };

            //  act
            string actualSpace = words.ContentsToString();
            string actualComma = words.ContentsToString(",");
            string expectedSpace = "the quick fox";
            string expectedComma = "the, quick, fox";

            //  assert
            Assert.AreEqual(expectedSpace, actualSpace);
            Assert.AreEqual(expectedComma, actualComma);
        }

        [Test]
        public void ContentsToString_ListExtensionWorks()
        {
            //  arrange
            List<string> words = new List<string>();
            words.Add("the");
            words.Add("quick");
            words.Add("fox");

            //  act
            string actualSpace = words.ContentsToString();
            string actualComma = words.ContentsToString(",");
            string expectedSpace = "the quick fox";
            string expectedComma = "the, quick, fox";

            //  assert
            Assert.AreEqual(expectedSpace, actualSpace);
            Assert.AreEqual(expectedComma, actualComma);
        }

        [Test]
        public void RemoveNulls_ReturnsCorrectly()
        {
            //  arrange
            int samples = 100;
            int nulls = 20;
            List<int?> ints = new List<int?>();
            for (int i = 0; i < samples; i++)
            {
                ints.Add(UnityEngine.Random.Range(0, 99));
                if (i < nulls) ints.Add(null);
            }

            //  act
            int total = ints.Count;
            ints.RemoveNulls();
            int nonNullTotal = ints.Count;

            //  assert
            Assert.AreEqual(samples + nulls, total);
            Assert.AreEqual(samples, nonNullTotal);
        }

        [Test]
        public void RemoveDuplicates_ReturnsCorrectly()
        {
            //  arrange
            int samples = 10;
            int duplicates = 3;
            List<int> ints = new List<int>();
            for (int i = 0; i < samples; i++)
            {
                ints.Add(i);
                if (i < duplicates) ints.Add(i);
            }

            //  act
            int total = ints.Count;
            ints.RemoveDuplicates();
            int newTotal = ints.Count;

            //  assert
            Assert.AreEqual(samples + duplicates, total,
                "expected size (with duplicates) does not match " +
                "actual size {0} <> {1}\n{2}", samples + duplicates, total, PUtil.ContentsToString(ints));
            Assert.AreEqual(samples, newTotal,
                "expected  size (after deduplication) does not match " +
                "actual size {0} <> {1}\n{2}", samples, newTotal, PUtil.ContentsToString(ints));
        }

        [Test]
        public void RemoveLast_ReturnsCorrectly()
        {
            //  arrange
            int samples = 100;
            List<int> ints = new List<int>();
            for (int i = 0; i < samples; i++)
            {
                ints.Add(i);
            }

            //  act
            int removed = ints.RemoveLast();

            //  assert
            Assert.AreEqual(samples - 1, removed);
            Assert.AreEqual(samples - 1, ints.Count);
        }

        [Test]
        public void RemoveFirst_ReturnsCorrectly()
        {
            //  arrange
            int samples = 100;
            List<int> ints = new List<int>();
            for (int i = 0; i < samples; i++)
            {
                ints.Add(i);
            }

            //  act
            int removed = ints.RemoveFirst();

            //  assert
            Assert.AreEqual(0, removed);
            Assert.AreEqual(samples - 1, ints.Count);
        }

        [Test]
        public void AddUnique_ReturnsCorrectly()
        {
            //  arrange
            int samples = 100;
            List<int?> ints = new List<int?>();
            for (int i = 0; i < samples; i++)
            {
                ints.Add(i);
            }

            //  act
            int total = ints.Count;
            ints.AddUnique(5);
            int newTotal = ints.Count;

            //  assert
            Assert.AreEqual(samples, total);
            Assert.AreEqual(samples, newTotal);
        }

        [Test]
        public void AddUniques_ReturnsCorrectly()
        {
            //  arrange
            int samples = 100;
            List<int?> ints = new List<int?>();
            List<int?> duplicates = new List<int?>();
            for (int i = 0; i < samples; i++)
            {
                ints.Add(i);
                duplicates.Add(i);
            }

            //  act
            int total = ints.Count;
            ints.AddUniques(duplicates);
            int newTotal = ints.Count;

            //  assert
            Assert.AreEqual(samples, total);
            Assert.AreEqual(samples, newTotal);
        }

        [Test]
        public void AssignAll_ReturnsCorrectly()
        {
            //  arrange
            int samples = 100;
            int[] ints = new int[samples];
            for (int i = 0; i < samples; i++)
            {
                ints[i] = 0;
            }

            //  act
            int total = ints.Sum();
            int expected = 0 * samples;
            ints.AssignAll(5);
            int newTotal = ints.Sum();
            int newExpected = samples * 5;

            //  assert
            Assert.AreEqual(expected, total);
            Assert.AreEqual(newExpected, newTotal);
        }

        [Test]
        public void Contains_ReturnsCorrectly()
        {
            //  arrange
            int samples = 100;
            int[] ints = new int[samples];
            for (int i = 0; i < samples; i++)
            {
                ints[i] = i;
            }
            int yes = UnityEngine.Random.Range(0, samples);
            int no = samples * samples;

            //  act
            bool contains = ints.Contains(yes);
            bool noContains = ints.Contains(no);

            //  assert
            Assert.IsTrue(contains);
            Assert.IsFalse(noContains);

        }

        [Test]
        public void ArrayContainsInstanceId_UsingGameObjectReturnsCorrectly()
        {
            //  arrange
            int samples = 100;
            GameObject[] gos = new GameObject[samples];
            for (int i = 0; i < samples; i++)
            {
                gos[i] = new GameObject(i.ToString());
            }
            int id = gos[9].GetInstanceID();

            //  act
            bool contains = gos.ContainsInstanceId(id);

            //  assert
            Assert.IsTrue(contains);
        }

        [Test]
        public void ArrayContainsInstanceId_UsingTransformReturnsCorrectly()
        {
            //  arrange
            int samples = 100;
            Transform[] gos = new Transform[samples];
            for (int i = 0; i < samples; i++)
            {
                gos[i] = new GameObject(i.ToString()).transform;
            }
            int id = gos[9].GetInstanceID();

            //  act
            bool contains = gos.ContainsInstanceId(id);

            //  assert
            Assert.IsTrue(contains);
        }

        [Test]
        public void ListContainsInstanceId_UsingGameObjectReturnsCorrectly()
        {
            //  arrange
            int samples = 100;
            List<GameObject> gos = new List<GameObject>();
            for (int i = 0; i < samples; i++)
            {
                gos.Add(new GameObject(i.ToString()));
            }
            int id = gos[9].GetInstanceID();

            //  act
            bool contains = gos.ContainsInstanceId(id);

            //  assert
            Assert.IsTrue(contains);
        }

        [Test]
        public void ListContainsInstanceId_UsingTransformReturnsCorrectly()
        {
            //  arrange
            int samples = 100;
            List<Transform> gos = new List<Transform>();
            for (int i = 0; i < samples; i++)
            {
                gos.Add(new GameObject(i.ToString()).transform);
            }
            int id = gos[9].GetInstanceID();

            //  act
            bool contains = gos.ContainsInstanceId(id);

            //  assert
            Assert.IsTrue(contains);
        }

        [Test]
        public void RemoveInstanceById_FromGameObjectListReturnsCorrectly()
        {
            //  arrange
            int samples = 100;
            List<GameObject> gos = new List<GameObject>();
            for (int i = 0; i < samples; i++)
            {
                gos.Add(new GameObject(i.ToString()));
            }
            int id = gos[9].GetInstanceID();

            //  act
            GameObject removed = gos.RemoveByInstanceId(id);

            //  assert
            Assert.AreEqual(id, removed.GetInstanceID());
            Assert.AreEqual(samples - 1, gos.Count);
            Assert.IsFalse(gos.ContainsInstanceId(id));
        }

        [Test]
        public void RemoveInstanceById_FromTransformListReturnsCorrectly()
        {
            //  arrange
            int samples = 100;
            List<Transform> gos = new List<Transform>();
            for (int i = 0; i < samples; i++)
            {
                gos.Add(new GameObject(i.ToString()).transform);
            }
            int id = gos[9].GetInstanceID();

            //  act
            Transform removed = gos.RemoveByInstanceId(id);

            //  assert
            Assert.AreEqual(id, removed.GetInstanceID());
            Assert.AreEqual(samples - 1, gos.Count);
            Assert.IsFalse(gos.ContainsInstanceId(id));
        }

        [Test]
        public void Push_IntoArrayReturnsCorrectly()
        {
            //  arrange
            int samples = 100;
            GameObject[] gos = new GameObject[samples];
            for (int i = 0; i < samples; i++)
            {
                gos[i] = new GameObject(i.ToString());
            }
            GameObject pushed = new GameObject((samples + 1).ToString());

            //  act
            gos.Push(pushed);

            //  assert
            Assert.AreEqual(samples, gos.Length, "Array size mismatch {0} <> {1}", samples, gos.Length);
            Assert.IsTrue(gos.Contains(pushed), "Array does not contained the pushed object");
            Assert.AreEqual(pushed, gos[0], "Pushed object is not the first object in array");
        }

        #endregion

        //  Complete
        #region Queues

        [Test]
        public void Churn_Works()
        {
            //  arrange
            int samples = 100;
            Queue<int> q = new Queue<int>();
            for (int i = 0; i < samples; i++)
            {
                q.Enqueue(i);
            }

            //  act
            int churned = q.Churn();

            //  assert
            Assert.AreEqual(0, churned);
            Assert.AreEqual(1, q.Peek());
            Assert.AreEqual(samples, q.Count);
        }

        #endregion

        //  Complete
        #region Boundaries

        [Test]
        public void GetCombinedBoundsOfChildrenExtension_Works()
        {
            //  arrange
            const int size = 4;
            GameObject root = new GameObject("main");
            GameObject a = GameObject.CreatePrimitive(PrimitiveType.Cube);
            GameObject b = GameObject.CreatePrimitive(PrimitiveType.Cube);
            GameObject c = GameObject.CreatePrimitive(PrimitiveType.Cube);

            GameObject expected = GameObject.CreatePrimitive(PrimitiveType.Cube);
            expected.transform.localScale = Vector3.one * (size + 1);
            expected.transform.Translate(Vector3.forward * size * 0.5f
                + Vector3.right * size * 0.5f
                + Vector3.up * size * 0.5f);

            a.transform.position = Vector3.forward * size;
            b.transform.position = Vector3.right * size;
            c.transform.position = Vector3.up * size;

            a.transform.parent = root.transform;
            b.transform.parent = root.transform;
            c.transform.parent = root.transform;

            //  act
            Bounds combinedBounds = root.GetCombinedBoundsOfChildren();
            Bounds expectedBounds = expected.GetComponent<Renderer>().bounds;

            //  assert
            //  To match bounds: the center point and extents must be equal.
            Assert.IsTrue(combinedBounds.Equals(expectedBounds), "Bound objects do not match.\nExpected: {0}\nActual: {1}", expectedBounds.ToString(), combinedBounds.ToString());
        }

        [Test]
        public void UnparentExtension_UnparentsCorrectly()
        {
            //  arrange
            GameObject a = GameObject.CreatePrimitive(PrimitiveType.Cube);
            GameObject b = GameObject.CreatePrimitive(PrimitiveType.Cube);
            b.transform.parent = a.transform;

            //  act
            b.Unparent();

            //  assert
            Assert.IsNull(b.transform.parent);
        }

        [Test]
        public void SetParentExtension_ParentsCorrectly()
        {
            //  arrange
            GameObject p = GameObject.CreatePrimitive(PrimitiveType.Cube);
            GameObject c = GameObject.CreatePrimitive(PrimitiveType.Cube);
            c.tag = TestHelper.TEST_TAG;

            //  act
            c.SetParent(p);
            Transform[] children = p.FindChildrenWithTag(TestHelper.TEST_TAG);

            //  assert
            Assert.AreEqual(1, children.Length);
            Assert.AreEqual(c.tag, children[0].tag);
        }

        [Test]
        public void AddChildExtension_ParentsCorrectly()
        {
            //  arrange
            GameObject p = GameObject.CreatePrimitive(PrimitiveType.Cube);
            GameObject c = GameObject.CreatePrimitive(PrimitiveType.Cube);
            c.tag = TestHelper.TEST_TAG;

            //  act
            p.AddChild(c);
            Transform[] children = p.FindChildrenWithTag(TestHelper.TEST_TAG);

            //  assert
            Assert.AreEqual(1, children.Length);
            Assert.AreEqual(c.tag, children[0].tag);
        }

        [Test]
        public void DetatchChildExtension_DetatchesCorrectly()
        {
            //  arrange
            GameObject parent = GameObject.CreatePrimitive(PrimitiveType.Cube);
            GameObject child = GameObject.CreatePrimitive(PrimitiveType.Cube);
            child.transform.parent = parent.transform;

            //  act
            GameObject result = parent.DetachChild(0);

            //  assert
            Assert.IsTrue(result != null, "Detatched child does not exist");
            Assert.IsTrue(result.Equals(child), "Detatched child does not match child object");
        }

        #endregion

        //  Incomplete
        #region Messaging

        #endregion

        //  Incomplete
        #region Navigation

        #endregion

        //  Incomplete
        #region Identification and Tags

        #endregion

        //  Incomplete
        #region Layers

        #endregion

        //  Incomplete
        #region Distances and distance-based comparison

        #endregion

        //  Incomplete
        #region Components

        [Test]
        public void DeepFind_ReturnsSuccessfulOnActive()
        {
            //  arrange
            GameObject root = GameObject.CreatePrimitive(PrimitiveType.Cube);
            root.name = "root";
            GameObject p = root;

            for (int i = 0; i < 5; i++)
            {
                GameObject g = GameObject.CreatePrimitive(PrimitiveType.Cube);
                g.name = i.ToString();
                g.transform.SetParent(p.transform);
                p = g;
            }

            //  act
            Transform dfsFound = root.transform.DeepFind("3", searchType: PUtil.DeepFindSearch.DFS, includeInactive: false);
            Transform dfsNotfound = root.transform.DeepFind("9", searchType: PUtil.DeepFindSearch.DFS, includeInactive: false);
            Transform bfsFound = root.transform.DeepFind("3", searchType: PUtil.DeepFindSearch.BFS, includeInactive: false);
            Transform bfsNotfound = root.transform.DeepFind("9", searchType: PUtil.DeepFindSearch.BFS, includeInactive: false);

            ////  assert
            Assert.IsTrue(dfsFound, "DFS Failed to find child");
            Assert.IsFalse(dfsNotfound, "DFS Did not expect to find anything, but did");
            Assert.IsTrue(bfsFound, "BFS Failed to find child");
            Assert.IsFalse(bfsNotfound, "BFS Did not expect to find anything, but did");
        }

        [Test]
        public void DeepFind_ReturnsSuccessfulOnInactive()
        {
            //  arrange
            GameObject root = GameObject.CreatePrimitive(PrimitiveType.Cube);
            root.name = "root";
            GameObject p = root;

            for (int i = 0; i < 5; i++)
            {
                GameObject g = GameObject.CreatePrimitive(PrimitiveType.Cube);
                g.name = i.ToString();
                g.transform.SetParent(p.transform);
                p = g;
            }

            //  make all game objects "inactive"
            for(int i=0;i<5;i++)
            {
                p.SetActive(false);
                p = p.transform.parent.gameObject;
            }

            //  set the root "inactive"
            root.SetActive(false);

            //  Set the starting point to the first child of 'root'.
            //  So we are starting on an inactive object,
            //  under and inactive object,
            //  and looking for a deeper inactive object.
            root = root.transform.GetChild(0).gameObject;

            //  act
            Transform dfsFound = root.transform.DeepFind("3", searchType: PUtil.DeepFindSearch.DFS, includeInactive: true);
            Transform dfsNotfound = root.transform.DeepFind("9", searchType: PUtil.DeepFindSearch.DFS, includeInactive: true);
            Transform bfsFound = root.transform.DeepFind("3", searchType: PUtil.DeepFindSearch.BFS, includeInactive: true);
            Transform bfsNotfound = root.transform.DeepFind("9", searchType: PUtil.DeepFindSearch.BFS, includeInactive: true);
            
            ////  assert
            Assert.IsTrue(dfsFound, "DFS Failed to find child");
            Assert.IsFalse(dfsNotfound, "DFS Did not expect to find anything, but did");
            Assert.IsTrue(bfsFound, "BFS Failed to find child");
            Assert.IsFalse(bfsNotfound, "BFS Did not expect to find anything, but did");
        }

        [Test]
        public void DeepFindChildrenWithTag_ReturnsSuccessfulForNested()
        {
            //  arrange
            GameObject root = GameObject.CreatePrimitive(PrimitiveType.Cube);
            root.name = "root";
            GameObject p = root;
            //  This tag must exist in Unity
            string TAG = "Player";
            int nestLevel = 5;

            //  create a nested list of objects
            //  p->g1->g2->g3->g4->g5
            //  all objects (except p) have TAG
            for (int i = 0; i < nestLevel; i++)
            {
                GameObject g = GameObject.CreatePrimitive(PrimitiveType.Cube);
                g.tag = TAG;
                g.name = i.ToString();
                g.transform.SetParent(p.transform);
                p = g;
            }
            
            //  act
            List<Transform> tagsFound = root.transform.DeepFindChildrenByTag(TAG);
            
            //  assert
            Assert.AreEqual(nestLevel, tagsFound.Count);
        }

        [Test]
        public void DeepFindChildrenWithTag_ReturnsSuccessfulForInactive()
        {
            //  arrange
            GameObject root = GameObject.CreatePrimitive(PrimitiveType.Cube);
            root.name = "root";
            GameObject p = root;
            //  This tag must exist in Unity
            string TAG = "Player";
            int nestLevel = 5;

            //  create a nested list of objects
            //  p->g1->g2->g3->g4->g5
            //  all objects (except p) have TAG
            for (int i = 0; i < nestLevel; i++)
            {
                GameObject g = GameObject.CreatePrimitive(PrimitiveType.Cube);
                g.tag = TAG;
                g.name = i.ToString();
                g.transform.SetParent(p.transform);
                p = g;
            }

            //  make all game objects "inactive"
            for (int i = 0; i < nestLevel; i++)
            {
                p.SetActive(false);
                p = p.transform.parent.gameObject;
            }

            //  set the root "inactive"
            root.SetActive(false);

            //  Set the starting point to the first child of 'root'.
            //  So we are starting on an inactive object,
            //  under and inactive object,
            //  and looking for a deeper inactive object.
            //  This means the required 'nestLevel' will be reduced by 1.
            root = root.transform.GetChild(0).gameObject;

            //  act
            List<Transform> tagsFound = root.transform.DeepFindChildrenByTag(TAG);

            //  assert
            Assert.AreEqual(nestLevel-1, tagsFound.Count);
        }

        [Test]
        public void DeepFindChildrenWithTag_ReturnsSuccessfulForMultiplesPerLevel()
        {
            //  arrange
            GameObject root = GameObject.CreatePrimitive(PrimitiveType.Cube);
            root.name = "root";
            GameObject p = root;
            //  This tag must exist in Unity
            string TAG = "Player";
            int nestLevel = 5;
            int horizontalQuantity = 8;

            //  create a nested list of objects
            //  p->g1->g2->g3->g4->g5
            //  all objects (except p) have TAG
            for (int i = 0; i < nestLevel; i++)
            {
                for (int j = 0; j < horizontalQuantity; j++)
                {
                    GameObject g = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    g.tag = TAG;
                    g.name = i.ToString();
                    g.transform.SetParent(p.transform);
                    if (j == horizontalQuantity - 1) p = g;
                }
            }
            
            //  act
            List<Transform> tagsFound = root.transform.DeepFindChildrenByTag(TAG);

            //  assert
            Assert.AreEqual(nestLevel*horizontalQuantity, tagsFound.Count);
        }

        #endregion

        //  Incomplete
        #region Enumerations

        #endregion

    }
}