//#define UNIT_TEST

using UnityEngine;
using System.Collections.Generic;
using NUnit.Framework;
using PLib.Pooling;
using Random = UnityEngine.Random;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace PLib
{
#if UNIT_TEST
    public class Debug
    {
        public static void Log(string s) { Console.WriteLine(s); }
        public static void LogWarning(string s) { Console.WriteLine(s); }
        public static void LogError(string s) { Console.WriteLine(s); }
    }

    public class MonoBehaviour 
    {
        void Instantiate() { }
        void Destroy() { }
        void DestroyImmediate() { }
    }
#endif
}
namespace PoolTest
{
    [TestFixture]
    public class PoolTest
    {
        GameObject prefab;

        #region Default standard behavior

        [SetUp]
        public void Setup()
        {
            prefab = GameObject.CreatePrimitive(PrimitiveType.Cube);
            prefab.name += " (prefab)";
        }

        [Test]
        public void Get_EmptyPoolInstantiatesObject()
        {
            //  setup
            PPool.Clear(prefab);

            //  arrange
            GameObject instance;

            //  act
            instance = PPool.Get(prefab);
            bool isActive = instance.activeSelf;
            bool nameMatch = prefab.name.Equals(instance.name);

            //  assert
            Assert.IsNotNull(prefab, "Prefab is null");
            Assert.IsNotNull(instance, "Instance is null");
            Assert.AreNotSame(prefab, instance, "Prefab and instance are the same object");
            Assert.IsTrue(isActive, "Instanced object is not active");
            Assert.IsTrue(nameMatch, "Prefab and Instanced object have different names {0} <> {1}",
                prefab.name, instance.name);
        }

        [Test]
        public void Get_PoolProvidesExistingObject()
        {
            //  setup
            PPool.Clear(prefab);

            //  arrange
            GameObject first, second;
            int firstHash, secondHash;

            //  act
            first = PPool.Get(prefab);
            firstHash = first.GetHashCode();
            PPool.Put(first);

            second = PPool.Get(prefab);
            secondHash = second.GetHashCode();

            //  assert
            Assert.AreEqual(firstHash, secondHash, "First and second hash do not match");
        }

        [Test]
        public void Get_AssignsTransformData()
        {
            //  setup
            PPool.Clear(prefab);

            //  arrange
            GameObject instance;
            Vector3 position = new Vector3(Random.value, Random.value, Random.value);
            Quaternion rotation = Quaternion.Euler(Random.value, Random.value, Random.value);

            //  act
            instance = PPool.Get(prefab, position, rotation);
            Vector3 posResult = instance.transform.position;
            Quaternion qtResult = instance.transform.rotation;
            float angle = Quaternion.Angle(rotation, qtResult);

            //  assert
            Assert.AreEqual(position, posResult, "Positions are not the same");
            Assert.AreEqual(0, angle, "Rotations are not the same");
        }

        [Test]
        public void Put_ReturnsObjectToPool()
        {
            //  setup
            PPool.Clear(prefab);

            //  arrange
            GameObject instance = PPool.Get(prefab);
            int available = PPool.GetAvailable(prefab);

            ////  act
            bool putResult = PPool.Put(instance);
            int result = PPool.GetAvailable(prefab);

            //  assert
            Assert.IsTrue(putResult, "Pool indicated object was destroyed");
            Assert.AreEqual(0, available, "Should be no available objects");
            Assert.AreEqual(1, result, "Should be one available object");
        }

        [Test]
        public void Put_DestroysNonPoolObject()
        {
            //  setup
            PPool.Clear(prefab);

            //  arrange
            GameObject nonPoolObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);

            //  act
            bool result = PPool.Put(nonPoolObject);

            //  Force the use of the Unity overloaded operator == when checking
            //  for destroyed objects. (Destroy() does not *actually* destroy the object,
            //  it just tells Unity to treat the object AS destroyed for the moment.
            //  Ref: https://gamedev.stackexchange.com/questions/115716/how-to-make-an-assert-isnull-test-pass-when-the-value-is-reported-as-null
            bool isDestroyed = nonPoolObject == null;

            //  assert
            Assert.IsFalse(result, "Pool indicated object was reclaimed.");
            Assert.IsTrue(isDestroyed, "Object was not Destroyed");
        }

        [Test]
        public void GetAvailable_IndicatesUnusedQuantity()
        {
            //  setup
            PPool.Clear(prefab);

            //  arrange
            int count = 3;
            GameObject[] gos = new GameObject[count];

            //  act
            for (int i = 0; i < count; i++)
            {
                gos[i] = PPool.Get(prefab);
            }
            int resultForEmpty = PPool.GetAvailable(prefab);

            for (int i = 0; i < count; i++)
            {
                PPool.Put(gos[i]);
            }
            int resultForReclaimed = PPool.GetAvailable(prefab);

            //  assert
            Assert.AreEqual(0, resultForEmpty, "Pool not empty");
            Assert.AreEqual(count, resultForReclaimed, "Pool missing reclaimed objects");
        }

        [Test]
        public void GetInUse_IndicatesUsedQuantity()
        {
            //  setup
            PPool.Clear(prefab);

            //  arrange
            int count = 3;
            GameObject[] gos = new GameObject[count];

            //  act
            for (int i = 0; i < count; i++)
            {
                gos[i] = PPool.Get(prefab);
            }
            int resultForUsed = PPool.GetInUse(prefab);

            for (int i = 0; i < count; i++)
            {
                PPool.Put(gos[i]);
            }
            int resultForReclaimed = PPool.GetInUse(prefab);

            //  assert
            Assert.AreEqual(count, resultForUsed, "Pool has wrong number of active objects");
            Assert.AreEqual(0, resultForReclaimed, "Pool has wrong number of active objects");
        }

        [Test]
        public void SetLimit_AssignsCreationLimit()
        {
            //  setup
            PPool.Clear(prefab);

            //  arrange
            int limit = 3;
            PPool.SetLimit(prefab, poolSize: limit);

            //  act
            GameObject[] g = new GameObject[limit + 1];
            for (int i = 0; i < limit + 1; i++)
            {
                g[i] = PPool.Get(prefab);
            }

            //  assert
            Assert.IsNull(g[limit], "Pool created objects in excess of assigned limit");
        }

        [Test]
        public void SetLimit_AssignsPersistenceLimit()
        {
            //  setup
            PPool.Clear(prefab);

            //  arrange
            float duration = 0;
            PPool.SetLimit(prefab, staleDuration: duration);

            //  act
            GameObject instance = PPool.Get(prefab);
            bool putResult = PPool.Put(instance);
            Delay(duration + 1);
            PPool.Expire(prefab);
            Delay(duration + 1);
            int available = PPool.GetAvailable(prefab);

            //  assert
            Assert.IsTrue(putResult, "Pool indicates item was destroyed");
            Assert.AreEqual(0, available, "Pool kept objects past expiration");
        }

        [Test]
        public void SetLimit_ReducesObjectCount()
        {
            //  setup
            PPool.Clear(prefab);

            //  arrange
            float longStale = 9;
            float shortStale = 0;
            int count = 7;
            PPool.SetLimit(prefab, staleDuration: longStale);
            List<GameObject> list = new List<GameObject>();
            for (int i = 0; i < count; i++) list.Add(PPool.Get(prefab));
            for (int i = 0; i < count; i++) PPool.Put(list[i]);
            list.Clear();

            //  act
            PPool.SetLimit(prefab, staleDuration: shortStale);
            //Delay(count * 2 + 0.1f);
            int available = PPool.GetAvailable(prefab);

            //  assert
            Assert.AreEqual(0, available, "Pool did not expire unused objects");
        }

        [Test]
        public void Prewarm_CreatesObjects()
        {
            //  setup
            PPool.Clear(prefab);

            //  arrange
            int count = 8;
            float duration = 0; //  second

            //  act
            PPool.Prewarm(prefab, count, duration);
            int complete = PPool.GetAvailable(prefab);

            //  assert
            Assert.AreEqual(count, complete, "Contains wrong number of objects");
        }

        [Test]
        public void Cull_ReducesObjectCount()
        {
            //  setup
            PPool.Clear(prefab);

            //  arrange
            int count = 9;
            int reduced = count - 1;
            List<GameObject> list = new List<GameObject>();
            for (int i = 0; i < count; i++) list.Add(PPool.Get(prefab));
            for (int i = 0; i < count; i++) PPool.Put(list[0]);
            list.Clear();

            //  act
            PPool.SetLimit(prefab, poolSize: reduced);
            int available = PPool.GetAvailable(prefab);

            //  assert
            Assert.AreEqual(reduced, available, "Available objects does not match expected count");
        }

        [Test]
        public void Clear_DestroysAllObjects()
        {
            //  setup
            PPool.Clear(prefab);

            //  arrange
            List<GameObject> list = new List<GameObject>();
            int count = 9;

            //  act
            for (int i = 0; i < count; i++)
            {
                GameObject g = PPool.Get(prefab);
                if (g != null) list.Add(g);
            }
            for (int i = 0; i < count; i++) PPool.Put(list[0]);
            PPool.Clear(prefab);
            int total = PPool.GetAvailable(prefab) + PPool.GetInUse(prefab);

            //  assert
            Assert.AreEqual(0, total, "Pool contains objects");
        }

        [Test]
        public void Expire_ReducesObjectCount()
        {
            //  setup
            PPool.Clear(prefab);

            //  arrange
            float shortStale = 0;
            int count = 7;
            List<GameObject> list = new List<GameObject>();
            PPool.SetLimit(prefab, staleDuration: shortStale);
            for (int i = 0; i < count; i++) list.Add(PPool.Get(prefab));
            for (int i = 0; i < count; i++) PPool.Put(list[i]);
            list.Clear();

            //  act
            PPool.Expire(prefab, true);
            int available = PPool.GetAvailable(prefab);

            //  assert
            Assert.AreEqual(0, available, "Pool did not expire unused objects");
        }

        #endregion
        #region Edge cases

        [Test]
        public void Get_ReturnsNullWhenAtMaxLimit()
        {
            //  setup
            PPool.Clear(prefab);

            //	arrange
            int count = 9;
            List<GameObject> g = new List<GameObject>();
            PPool.SetLimit(prefab, poolSize: count);

            //	act
            for (int i = 0; i < count + 1; i++)
            {
                GameObject go = PPool.Get(prefab);
                if (go) g.Add(go);
            }

            //	assert
            Assert.AreEqual(count, g.Count, "Pool created wrong number of objects");
        }

        [Test]
        public void SetNoStaleDuration_RemovesTimestamps()
        {
            //  setup
            PPool.Clear(prefab);

            //  arrange
            List<GameObject> list = new List<GameObject>();
            float duration = 0;    //  seconds
            int count = 9;

            //  act (create objects with timestamps)           
            PPool.SetLimit(prefab, staleDuration: duration);
            for (int i = 0; i < count; i++) list.Add(PPool.Get(prefab));
            for (int i = 0; i < count; i++) PPool.Put(list[0]);
            list.Clear();

            //  act
            //  Use SetStale(unlimited), which should remove timestamps.
            PPool.SetLimit(prefab, staleDuration: PPool.UNLIMITED);
            int available = PPool.GetAvailable(prefab);

            //  assert
            Assert.AreEqual(count, available, "Wrong number available");
        }

        [Test]
        public void SetNoQuantityLimit_RemovesLimit()
        {
            //  setup
            PPool.Clear(prefab);

            //  arrange
            int count = 9;
            int limit = count - 1;
            List<GameObject> list = new List<GameObject>();

            //  act
            PPool.SetLimit(prefab, poolSize: limit);
            for (int i = 0; i < count; i++)
            {
                GameObject g = PPool.Get(prefab);
                if (g != null) list.Add(g);
            }
            int createdWhileLimted = list.Count;
            int countWhileLimited = PPool.GetInUse(prefab);

            PPool.SetLimit(prefab, poolSize: PPool.UNLIMITED);
            for (int i = 0; i < count; i++)
            {
                GameObject g = PPool.Get(prefab);
                if (g != null) list.Add(g);
            }
            int createUnlimited = list.Count;
            int countUnlimited = PPool.GetInUse(prefab);

            //  assert
            Assert.AreEqual(limit, createdWhileLimted, "Wrong number created while limited");
            Assert.AreEqual(limit, countWhileLimited, "Pool reports wrong amount created while limited");
            Assert.AreEqual(limit + count, createUnlimited, "Unlimited object creation count incorrect");
            Assert.AreEqual(limit + count, countUnlimited, "Pool reports wrong amount created while unlimited");
        }

        #endregion
        #region Diagnostics

        Stopwatch stopwatch = new Stopwatch();

        /// <summary>
        /// 2017-8-10
        /// </summary>
        /// <param name="duration">seconds</param>
        public void Delay(float duration)
        {
            stopwatch.Reset();
            stopwatch.Start();
            while (stopwatch.ElapsedMilliseconds < duration * 1000) ;
            stopwatch.Stop();
        }

        /// <summary>
        /// 2017-8-10
        /// Echos 'ticks' to console.
        /// </summary>
        /// <param name="duration">seconds</param>
        public void DelayWithLogging(float duration)
        {
            for(int i = 0; i < duration; i++)
            {
                stopwatch.Reset();
                stopwatch.Start();
                while (stopwatch.ElapsedMilliseconds < 1000) ;
                stopwatch.Stop();
                Debug.Log("Tick " + i + " seconds");
            }

            float remainder = duration - Mathf.FloorToInt(duration);

            stopwatch.Reset();
            stopwatch.Start();
            while (stopwatch.ElapsedMilliseconds < remainder * 1000) ;
            stopwatch.Stop();
            Debug.Log("Tick +" + remainder + " seconds");
        }

        #endregion
    }
}