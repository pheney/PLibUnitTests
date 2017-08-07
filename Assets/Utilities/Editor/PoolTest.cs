//#define UNIT_TEST

using UnityEngine;
using System.Collections.Generic;
using NUnit.Framework;
using PLib.Pooling;
using System;
using Random = UnityEngine.Random;

namespace PLib
{    
    #if UNIT_TEST
    public class Debug
    {
        public static void Log(string s) { Console.WriteLine(s); }
        public static void LogWarning(string s) { Console.WriteLine(s); }
        public static void LogError(string s) { Console.WriteLine(s); }
    }

    public class MonoBehaviour {

        void Instantiate() { }
        void Destroy() { }
        void DestroyImmediate() { }
    }
    #endif
}
namespace PoolTest {

    [TestFixture]
    public class PoolTest
    {
        GameObject prefab;

        [SetUp]
        public void Setup()
        {
            prefab = GameObject.CreatePrimitive(PrimitiveType.Cube);
        }

        [Test]
        public void Get_EmptyPoolInstantiatesObject()
        {
            //  setup
            PPool.Clear();

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
            PPool.Clear();

            //  arrange
            GameObject instance, sameObject;
            int count = 9;

            //  act
            for (int i = 0; i < count; i++)
            {
                instance = PPool.Get(prefab);
                PPool.Put(instance);
                instance = null;
            }
            sameObject = PPool.Get(prefab);
            bool isActive = sameObject.activeSelf;
            bool nameMatch = prefab.name.Equals(sameObject.name);
            int availableCount = PPool.GetAvailable(prefab);
            int usedCount = PPool.GetInUse(prefab);

            //  assert
            Assert.IsNotNull(prefab, "Prefab is null");
            Assert.IsNotNull(sameObject, "Second instance is null");
            Assert.AreNotSame(prefab, sameObject, "Prefab and second instance are the same object");
            Assert.IsTrue(isActive, "Second instanced object is not active");
            Assert.IsTrue(nameMatch, "Prefab and second instanced object have different names {0} <> {1}",
                prefab.name, sameObject.name);
            Assert.AreEqual(0, availableCount, "Pool should have 0 available objects");
            Assert.AreEqual(1, usedCount, "Pool should have 1 object in use");
        }

        [Test]
        public void Get_AssignsTransformData()
        {
            //  setup
            PPool.Clear();

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
            PPool.Clear();

            //  arrange
            GameObject instance = PPool.Get(prefab);
            int available = PPool.GetAvailable(prefab);

            //  act
            PPool.Put(instance);
            int result = PPool.GetAvailable(prefab);

            //  assert
            Assert.AreEqual(available + 1, result, "Available object count incorrect");
        }

        [Test]
        public void Put_DestroysNonPoolObject()
        {
            //  setup
            PPool.Clear();

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
            PPool.Clear();

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
            PPool.Clear();

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
            PPool.Clear();

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
            PPool.Clear();

            //  arrange
            float duration = 0.1f;
            PPool.SetLimit(prefab, staleDuration: duration);

            //  act
            GameObject instance = PPool.Get(prefab);
            PPool.Put(instance);
            //  TODO -- Wait for "duration" seconds
            int available = PPool.GetAvailable(prefab);

            //  assert
            Assert.AreEqual(0, available, "Pool kept objects past expiration");
        }

        [Test]
        public void Prewarm_CreatesObjects()
        {

        }

        [Test]
        public void Cull_ReducesObjectCount()
        {

        }

        [Test]
        public void Clear_DestroysAllObjects()
        {

        }

        [Test]
        public void Expire_ReducesObjectCount()
        {

        }
        
        [Test]
        public void Get_ReturnsNullWhenAtMaxLimit()
        {
            //	arrange
            PPool.Clear(true);
            GameObject prefab = GameObject.CreatePrimitive(PrimitiveType.Cube);
            prefab.name = "Prefab";
            int limit = 10;

            //	act
            PPool.SetLimit(prefab, limit);
            List<GameObject> instances = new List<GameObject>();
            for (int i = 0; i < limit; i++)
            {
                instances.Add(PPool.Get(prefab));
            }
            GameObject nullObject = PPool.Get(prefab);

            //	assert
            Assert.IsNotNull(prefab, "Prefab is null");
            Assert.AreEqual(limit, instances.Count,
                "Failed to created correct number of instances {0} <> {1}", limit, instances.Count);
            Assert.IsNull(nullObject, "Last object should have been null");
        }

        [Test]
        public void SetLimit_LimitsTotalObjectsCreated()
        {
            //  Arrange
            GameObject g = GameObject.CreatePrimitive(PrimitiveType.Cube);
            int limit = 3;
            List<GameObject> items = new List<GameObject>();

            //  Act
            PPool.SetLimit(g, limit);
            for (int i = 0; i < limit + 1; i++)
            {
                items.Add(PPool.Get(g));
            }

            //  Assert
            Assert.AreEqual(limit, items.Count);
            Assert.AreEqual(0, PPool.GetAvailable(g));
        }

        [Test]
        public void ReducedLimit_CullsExcessObjects()
        {
            //	arrange
            PPool.Clear(true);
            GameObject prefab = GameObject.CreatePrimitive(PrimitiveType.Cube);
            prefab.name = "Prefab";
            int limit = 10;
            int reduced = 6;
            int nonNullcount = 0;

            //	act
            PPool.SetLimit(prefab, limit);
            List<GameObject> instances = new List<GameObject>();

            //  create and return the object limit
            //  so now they exist in the pool (but are all inactive)
            for (int i = 0; i < limit; i++)
            {
                instances.Add(PPool.Get(prefab));
            }

            for (int i = 0; i < limit; i++)
            {
                PPool.Return(instances[0]);
                instances.RemoveAt(0);
            }

            //  reduce the pool size
            //  this should cull the excess objects (actually Destroy() them)
            PPool.SetLimit(prefab, reduced);

            //  attempt to create the original limit of objects
            //  this should result in some null results
            for (int i = 0; i < limit; i++)
            {
                instances.Add(PPool.Get(prefab));
            }

            //  count the nulls
            //  the count should be "original limit" - "new limit"
            for (int i = 0; i < limit; i++)
            {
                nonNullcount += instances[i] != null ? 1 : 0;
            }

            //	assert
            Assert.IsNotNull(prefab, "Prefab is null");
            Assert.AreEqual(reduced, nonNullcount,
                "List contained more items than permitted by the pool {0} <> {1}", reduced, nonNullcount);
        }

        [Test]
        public void Return_RecyclesInstantiatedObject()
        {
            Get_ReturnsInstantiatedObject();

            //	arrange
            PPool.Clear(true);
            GameObject prefab = GameObject.CreatePrimitive(PrimitiveType.Cube);
            prefab.name = "Prefab";
            GameObject instance = PPool.Get(prefab);

            //	act
            PPool.Return(instance);
            bool isActive = instance.activeSelf;

            //	assert
            Assert.IsNotNull(prefab, "Prefab is null");
            Assert.IsNotNull(instance, "Instance is null");
            Assert.AreNotSame(prefab, instance, "Prefab and instance are the same object");
            Assert.IsFalse(isActive, "Instanced object is still active (" + instance.name + ")");
        }

        [Test]
        public void Return_DestroysObject()
        {
            //  Arrange
            GameObject g = GameObject.CreatePrimitive(PrimitiveType.Cube);

            //  Act
            PPool.Return(g);

            //  Assert
            Assert.IsNull(g);
        }

        [Test]
        public void Flush_DestroysAvailableObjects()
        {
            //  Arrange
            GameObject prefab = GameObject.CreatePrimitive(PrimitiveType.Cube);
            GameObject g = null;

            //  Act
            g = PPool.Get(prefab);
            PPool.Put(g);
            PPool.Clear(true);

            //  Assert
            Assert.IsNull(g);
            Assert.AreEqual(0, PPool.GetInUse(prefab), "GameObjects inuse when there shouldn't be.");
            Assert.AreEqual(0, PPool.GetAvailable(prefab), "GameObjects available when there shouldn't be.");
        }

        [Test]
        public void Return_ReclaimsObject()
        {
            //  Arrange
            GameObject g = GameObject.CreatePrimitive(PrimitiveType.Cube);

            //  Act
            PPool.Return(g);

            //  Assert
            Assert.IsNotNull(g);
            Assert.IsFalse(g.activeSelf);
        }

        [Test]
        public void SetNoStaleDuration_RemovesTimestamps()
        {
            //  Arrange
            GameObject prefab = GameObject.CreatePrimitive(PrimitiveType.Cube);
            float stale = 0.1f;

            //  Act
            PPool.SetLimit(prefab, staleDuration: stale);
            GameObject g = PPool.Get(prefab);
            PPool.Put(g);
            PPool.SetLimit(prefab, PPool.UNLIMITED);

            //  Assert
            Assert.IsTrue(false, "UnitTest not implemented");
        }

        [Test]
        public void SetStaleDuration_AddsTimestamp()
        {
            //  Arrange

            //  Act

            //  Assert
            Assert.IsTrue(false, "UnitTest not implemented");
        }

        [Test]
        public void ReduceStaleDuration_AutomaticallyCullsExpired()
        {
            //  Arrange

            //  Act

            //  Assert
            Assert.IsTrue(false, "UnitTest not implemented");
        }

        [Test]
        public void Prewarm_CreatesObjects()
        {
            //  Arrange
            GameObject g = GameObject.CreatePrimitive(PrimitiveType.Cube);
            int count = 10;
            float duration = 1;

            //  Act
            PPool.Prewarm(g, count, duration);

            //  Assert
            Assert.IsTrue(false, "UnitTest not implemented");
        }
    }

    [TestFixture]
    public class PoolPocoTest
    {

    }
}