//#define UNIT_TEST

using UnityEngine;
using System.Collections.Generic;
using NUnit.Framework;
using PLib.Pooling;
using System;

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
        [Test]
        public void Get_ReturnsInstantiatedObject()
        {
            //	arrange
            PPool.Clear(true);
            GameObject prefab = GameObject.CreatePrimitive(PrimitiveType.Cube);
            prefab.name = "Prefab";

            //	act
            GameObject instance = PPool.Get(prefab);
            bool isActive = instance.activeSelf;

            //	assert
            Assert.IsNotNull(prefab, "Prefab is null");
            Assert.IsNotNull(instance, "Instance is null");
            Assert.AreNotSame(prefab, instance, "Prefab and instance are the same object");
            Assert.IsTrue(isActive, "Instanced object is not active");
            Assert.IsTrue(prefab.name.Equals(instance.name),
                "Prefab and Instanced object have different names {0} <> {1}",
                prefab.name, instance.name);
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
}