//#define UNIT_TEST

using System.Collections.Generic;
using NUnit.Framework;
using PLib.Pooling;
using System;
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
    public class PoolPocoTest
    {
        private class TestClass
        {
            public int iValue;
            public string sValue;
            public TestClass() { }
            public TestClass(int i, string s) { iValue = i; s = sValue; }
        }
        TestClass prefab;

        #region Default standard behavior

        [SetUp]
        public void Setup()
        {
            prefab = new TestClass(9, "ok");
        }

        [Test]
        public void Get_EmptyPoolInstantiatesObject()
        {
            //  setup
            PPocoPool.Clear<TestClass>();

            //  arrange
            TestClass instance;

            //  act
            instance = PPocoPool.Get<TestClass>();
            bool uniqueHash = prefab.GetHashCode() != instance.GetHashCode();

            //  assert
            Assert.IsNotNull(prefab, "Prefab is null");
            Assert.IsNotNull(instance, "Instance is null");
            Assert.AreNotSame(prefab, instance, "Prefab and instance are the same object");
            Assert.IsTrue(uniqueHash, "Prefab and Instanced object have same hashCode {0}", prefab.GetHashCode());
        }

        [Test]
        public void Get_PoolProvidesExistingObject()
        {
            //  setup
            PPocoPool.Clear<TestClass>();

            //  arrange
            TestClass first, second;
            int firstHash, secondHash;

            //  act
            first = PPocoPool.Get<TestClass>();
            firstHash = first.GetHashCode();
            PPocoPool.Put(first);

            second = PPocoPool.Get<TestClass>();
            secondHash = second.GetHashCode();
            
            //  assert
            Assert.AreEqual(firstHash, secondHash, "Hashcodes do not match");
        }

        [Test]
        public void Put_ReturnsObjectToPool()
        {
            //  setup
            PPocoPool.Clear<TestClass>();

            //  arrange
            TestClass instance = PPocoPool.Get<TestClass>();
            int available = PPocoPool.GetAvailable<TestClass>();

            //  act
            bool putResult = PPocoPool.Put(instance);
            int result = PPocoPool.GetAvailable<TestClass>();

            //  assert
            Assert.IsTrue(putResult, "Pool indicated object was not reclaimed");
            Assert.AreEqual(available + 1, result, "Available object count incorrect");
        }

        [Test]
        public void Put_DestroysNonPoolObject()
        {
            //  setup
            PPocoPool.Clear<TestClass>();

            //  arrange
            Exception nonPoolObject = new Exception();

            //  act
            bool result = PPocoPool.Put(nonPoolObject);

            //  assert
            Assert.IsFalse(result, "Pool indicated object was reclaimed.");
        }

        [Test]
        public void GetAvailable_IndicatesUnusedQuantity()
        {
            //  setup
            PPocoPool.Clear<TestClass>();

            //  arrange
            int count = 3;
            TestClass[] gos = new TestClass[count];

            //  act
            for (int i = 0; i < count; i++)
            {
                gos[i] = PPocoPool.Get<TestClass>();
            }
            int resultForEmpty = PPocoPool.GetAvailable<TestClass>();

            for (int i = 0; i < count; i++)
            {
                PPocoPool.Put(gos[i]);
            }
            int resultForReclaimed = PPocoPool.GetAvailable<TestClass>();

            //  assert
            Assert.AreEqual(0, resultForEmpty, "Pool not empty");
            Assert.AreEqual(count, resultForReclaimed, "Pool missing reclaimed objects");
        }

        [Test]
        public void GetInUse_IndicatesUsedQuantity()
        {
            //  setup
            PPocoPool.Clear<TestClass>();

            //  arrange
            int count = 3;
            TestClass[] gos = new TestClass[count];

            //  act
            for (int i = 0; i < count; i++)
            {
                gos[i] = PPocoPool.Get<TestClass>();
            }
            int resultForUsed = PPocoPool.GetInUse<TestClass>();

            for (int i = 0; i < count; i++)
            {
                PPocoPool.Put(gos[i]);
            }
            int resultForReclaimed = PPocoPool.GetInUse<TestClass>();

            //  assert
            Assert.AreEqual(count, resultForUsed, "Pool has wrong number of active objects");
            Assert.AreEqual(0, resultForReclaimed, "Pool has wrong number of active objects");
        }

        [Test]
        public void SetLimit_AssignsCreationLimit()
        {
            //  setup
            PPocoPool.Clear<TestClass>();

            //  arrange
            int limit = 3;
            PPocoPool.SetLimit<TestClass>(poolSize: limit);

            //  act
            TestClass[] g = new TestClass[limit + 1];
            for (int i = 0; i < limit + 1; i++)
            {
                g[i] = PPocoPool.Get<TestClass>();
            }

            //  assert
            Assert.IsNull(g[limit], "Pool created objects in excess of assigned limit");
        }

        [Test]
        public void SetLimit_AssignsPersistenceLimit()
        {
            //  setup
            PPocoPool.Clear<TestClass>();

            //  arrange
            float duration = 0f;
            PPocoPool.SetLimit<TestClass>(staleDuration: duration);

            //  act
            TestClass instance = PPocoPool.Get<TestClass>();
            PPocoPool.Put(instance);
            //  Using a duration of '0' should trigger the time-based culling.
            int available = PPocoPool.GetAvailable<TestClass>();

            //  assert
            Assert.AreEqual(0, available, "Pool kept objects past expiration");
        }

        [Test]
        public void Prewarm_CreatesObjects()
        {
            //  setup
            PPocoPool.Clear<TestClass>();

            //  arrange
            int count = 9;
            float duration = 7; //  seconds

            //  act
            PPocoPool.Prewarm<TestClass>(count, duration);
            Delay(duration * 0.5f);
            int initial = PPocoPool.GetAvailable<TestClass>();
            Delay(duration);
            int complete = PPocoPool.GetAvailable<TestClass>();

            //  assert
            Assert.IsTrue(initial < count, "Partial generaton contains too many objects");
            Assert.AreEqual(count, complete, "Complete generation contains wrong number of objects");
        }

        [Test]
        public void Cull_ReducesObjectCount()
        {
            //  setup
            PPocoPool.Clear<TestClass>();

            //  arrange
            int count = 9;
            int reduced = count - 1;
            List<TestClass> list = new List<TestClass>();
            for (int i = 0; i < count; i++) list.Add(PPocoPool.Get< TestClass>());
            for (int i = 0; i < count; i++) PPocoPool.Put(list[0]);
            list.Clear();

            //  act
            PPocoPool.SetLimit<TestClass>(poolSize: reduced);
            int available = PPocoPool.GetAvailable<TestClass>();

            //  assert
            Assert.AreEqual(reduced, available, "Available objects does not match expected count");
        }

        [Test]
        public void Clear_DestroysAllObjects()
        {
            //  setup
            PPocoPool.Clear<TestClass>();

            //  arrange
            int count = 9;

            //  act
            PPocoPool.Prewarm<TestClass>(count);
            PPocoPool.Clear<TestClass>();
            int total = PPocoPool.GetAvailable<TestClass>() + PPocoPool.GetInUse<TestClass>();

            //  assert
            Assert.AreEqual(0, total, "Pool contains objects");
        }

        [Test]
        public void Expire_ReducesObjectCount()
        {
            //  setup
            PPocoPool.Clear<TestClass>();

            //  arrange
            float longStale = 9;
            float shortStale = 1;
            int count = 7;
            PPocoPool.SetLimit<TestClass>(staleDuration: longStale);
            PPocoPool.Prewarm<TestClass>(count, duration: 0);

            //  act
            PPocoPool.SetLimit<TestClass>(staleDuration: shortStale);
            Delay(shortStale + 0.1f);
            PPocoPool.Expire<TestClass>();
            int available = PPocoPool.GetAvailable<TestClass>();

            //  assert
            Assert.AreEqual(0, available, "Pool did not expire unused objects");
        }

        #endregion
        #region Edge cases

        [Test]
        public void Get_ReturnsNullWhenAtMaxLimit()
        {
            //  setup
            PPocoPool.Clear<TestClass>();

            //	arrange
            int count = 9;
            List<TestClass> g = new List<TestClass>();
            PPocoPool.SetLimit<TestClass>(poolSize: count);

            //	act
            for (int i = 0; i < count + 1; i++)
            {
                TestClass go = PPocoPool.Get<TestClass>();
                if (go != null) g.Add(go);
            }

            //	assert
            Assert.AreEqual(count, g.Count, "Pool created wrong number of objects");
        }

        [Test]
        public void SetNoStaleDuration_RemovesTimestamps()
        {
            //  setup
            PPocoPool.Clear<TestClass>();

            //  arrange
            float duration = 1;    //  seconds
            PPocoPool.SetLimit<TestClass>(staleDuration: duration);
            int count = 9;

            //  act            
            PPocoPool.Prewarm<TestClass>(count);
            PPocoPool.SetLimit<TestClass>(staleDuration: PPocoPool.UNLIMITED);
            Delay(duration + 0.1f);
            int available = PPocoPool.GetAvailable<TestClass>();

            //  assert
            Assert.AreEqual(count, available, "Wrong number available");
        }

        [Test]
        public void SetNoQuantityLimit_RemovesLimit()
        {
            //  setup
            PPocoPool.Clear<TestClass>();

            //  arrange
            int count = 9;
            int limit = count - 1;
            List<TestClass> list = new List<TestClass>();

            //  act
            PPocoPool.SetLimit<TestClass>(poolSize: limit);
            PPocoPool.Prewarm<TestClass>(count);
            int countWhileLimited = PPocoPool.GetAvailable<TestClass>();
            PPocoPool.SetLimit<TestClass>(poolSize: PPool.UNLIMITED);
            PPocoPool.Prewarm<TestClass>(count);
            int countUnlimited = PPocoPool.GetAvailable<TestClass>();

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