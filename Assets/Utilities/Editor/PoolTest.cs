//#define UNIT_TEST

using UnityEngine;
using System.Collections.Generic;
using NUnit.Framework;
using PLib.Pooling;
using Random = UnityEngine.Random;
using System.Diagnostics;

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

            //  assert
            Assert.AreEqual(position, posResult, "Positions are not the same");
            Assert.AreEqual(rotation, qtResult, "Rotations are not the same");
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

            //  assert
            Assert.IsFalse(result, "Pool indicated object was reclaimed.");
            Assert.IsNull(nonPoolObject, "Object was not Destroyed");
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
            float duration = 1f;
            PPool.SetLimit(prefab, staleDuration: duration);

            //  act
            GameObject instance = PPool.Get(prefab);
            PPool.Put(instance);
            Delay(duration + 0.1f);
            int available = PPool.GetAvailable(prefab);

            //  assert
            Assert.AreEqual(0, available, "Pool kept objects past expiration");
        }

        [Test]
        public void Prewarm_CreatesObjects()
        {
            //  setup
            PPool.Clear(prefab);

            //  arrange
            int count = 9;
            float duration = 7; //  second

            //  act
            PPool.Prewarm(prefab, count, duration);
            Delay(duration * 0.5f);
            int initial = PPool.GetAvailable(prefab);
            Delay(duration);
            int complete = PPool.GetAvailable(prefab);

            //  assert
            Assert.IsTrue(initial<count, "Partial generaton contains too many objects");
            Assert.AreEqual(count, complete, "Complete generation contains wrong number of objects");
        }

        [Test]
        public void Cull_ReducesObjectCount()
        {
            //  setup
            PPool.Clear(prefab);

            //  arrange
            int count = 9;
            int reduced = count - 1;
            PPool.Prewarm(prefab, count);

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
            int count = 9;

            //  act
            PPool.Prewarm(prefab, count);
            PPool.Clear(prefab, true);
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
            float longStale = 9;
            float shortStale = 1;
            int count = 7;
            PPool.SetLimit(prefab, staleDuration: longStale);
            PPool.Prewarm(prefab, count, duration: 0);

            //  act
            PPool.SetLimit(prefab, staleDuration: shortStale);
            Delay(shortStale + 0.1f);
            PPool.Expire(prefab);
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
            PPool.Clear(prefab, true);

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
            float duration = 1;    //  seconds
            PPool.SetLimit(prefab, staleDuration: duration);
            int count = 9;

            //  act            
            PPool.Prewarm(prefab, count);
            PPool.SetLimit(prefab, staleDuration: PPool.UNLIMITED);
            Delay(duration + 0.1f);
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
            PPool.Prewarm(prefab, count);
            int countWhileLimited = PPool.GetAvailable(prefab);
            PPool.SetLimit(prefab, poolSize: PPool.UNLIMITED);
            PPool.Prewarm(prefab, count);
            int countUnlimited = PPool.GetAvailable(prefab);

            //  assert
            Assert.AreEqual(limit, countWhileLimited, "Object creation not correctly limited");
            Assert.AreEqual(count, countUnlimited, "Unlimited object creation count incorrect");
        }

        #endregion
        #region Diagnostics

        Stopwatch stopwatch = new Stopwatch();

        /// <summary>
        /// </summary>
        /// <param name="duration">seconds</param>
        public void Delay(float duration)
        {
            stopwatch.Reset();
            stopwatch.Start();
            while (stopwatch.ElapsedMilliseconds < duration * 1000) ;
            stopwatch.Stop();
        }

        #endregion
    }

}