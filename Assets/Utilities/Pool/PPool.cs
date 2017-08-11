using UnityEngine;
using System.Collections.Generic;
using System.Text;
using PLib.General;
using System.Linq;
using System.Collections;
using Random = UnityEngine.Random;

namespace PLib.Pooling
{
    /// <summary>
    /// 2017-8-8
    /// Manager object for GameObject pooling. 
    /// This manages a dictionary of individual Pools for each type of object. 
    /// Prefab objects are distinguished by their Hashcode.
    /// 
    /// To use:
    ///     instead of "Instantiate(prefab)", use "PPool.Get(prefab)"
    ///     instead of "Destroy(item)", use "PPool.Put(item)"
    ///     
    /// That's it. Everything else is managed behind the scenes.
    /// </summary>
    public static class PPool
    {
        #region Data Libraries
        //////////////////////////
        //	Data Libraries		//
        //////////////////////////

        public const int UNLIMITED = -1;
        private static Dictionary<int, IPool> pools = new Dictionary<int, IPool>();

        /// <summary>
        /// 2017-8-4
        /// This finds and returns the appropriate object pool based on the name hash of the provided game object.
        /// If there is no pool for this game object, then one will be created.
        /// </summary>
        private static IPool GetPool(GameObject prefab)
        {
            //  get the hash of the prefab's name
            int hash = prefab.GetHashCode();

            //  when there is not already a pool for this item, create one
            if (!pools.ContainsKey(hash))
            {
                pools.Add(hash, new Pool(prefab));
            }
            return pools[hash];
        }

        #endregion
        #region Manager API
        //////////////////////
        //	Public API		//
        //////////////////////

        /// <summary>
        /// 2016-2-17
        /// Returns an instance of the provided prefab from the appropriate pool.
        /// </summary>
        /// <param name="prefab"></param>
        /// <returns></returns>
        public static GameObject Get(GameObject prefab)
        {
            return (GameObject)(GetPool(prefab) as Pool).Get();
        }

        /// <summary>
        /// 2016-5-8
        /// Returns an instance of the provided prefab from the appropriate pool.
        /// </summary>
        /// <param name="prefab"></param>
        /// <returns></returns>
        public static GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            GameObject item = Get(prefab);
            if (item)
            {
                item.transform.position = position;
                item.transform.rotation = rotation;
            }
            return item;
        }

        /// <summary>
        /// 2017-8-4
        /// 
        /// PoolSize limits the maximum number of items in the pool for the provided prefab.
        /// If the limit provided is UNLIMITED (-1), then the pool has no limit.
        /// 
        /// StaleDuration limits the maximum time an object is maintained when not in use. Object that
        /// are not used within this duration are Destroyed.
        /// 
        /// Side effects:
        ///     When an object pool does not exist for the prefab parameter, one is created automatically.
        /// </summary>
        public static void SetLimit(GameObject prefab, int? poolSize = UNLIMITED, float? staleDuration = UNLIMITED)
        {
            IPool pool = GetPool(prefab);
            if (poolSize != null) pool.MaxSize((int)poolSize);
            if (staleDuration != null) pool.StaleDuration((int)staleDuration);
        }

        /// <summary>
        /// 2017-8-4
        /// Lets the pool manager know to recycle the item. The appropriate pool is notified and the object
        /// is reset and deactivated.
        /// 
        /// WARNING: if the item was renamed, this will probably fail (depending on whether GetHashCode()
        ///     uses the object name to generate the hashcode. When an item's hashcode cannot be found in
        ///     the pool, the item is destroyed and the return value is false.
        /// </summary>
        public static bool Put(GameObject item)
        {
            bool result = false;

            foreach (var p in pools)
            {
                IPool pool = p.Value;
                result = pool.Put(item);
                if (result) break;
            }

            if (!result)
            {
                if (Application.isEditor)
                {
                    MonoBehaviour.DestroyImmediate(item);
                }
                else
                {
                    MonoBehaviour.Destroy(item);
                }
            }

            return result;
        }

        /// <summary>
        /// 2017-8-4
        /// DEPRECATED. Use Put() instead.
        /// </summary>
        public static bool Return(GameObject item)
        {
            return Put(item);
        }

        /// <summary>
        /// 2017-8-3
        /// Returns the number of unused items in the 'available' pool for
        /// the given prefab. Returns -1 when this prefab does not have 
        /// a Pool object.
        /// </summary>
        public static int GetAvailable(GameObject prefab)
        {
            int result = -1;
            int hash = prefab.GetHashCode();
            if (pools.ContainsKey(hash))
            {
                result = GetPool(prefab).Available();
            }
            return result;
        }

        /// <summary>
        /// 2017-8-3
        /// Returns the number of "in use" items in the pool for
        /// the given prefab. Returns -1 when this prefab does not have 
        /// a Pool object.
        /// </summary>
        public static int GetInUse(GameObject prefab)
        {
            int result = -1;
            int hash = prefab.GetHashCode();
            if (pools.ContainsKey(hash))
            {
                result = GetPool(prefab).InUse();
            }
            return result;
        }

        /// <summary>
        /// 20178-8-3
        /// Prewarms the object pool. Prewarming is spread over a number
        /// of seconds equal to "duration." Default is 3 seconds.
        /// </summary>
        public static void Prewarm(GameObject prefab, int count, float duration = 1)
        {
            GetPool(prefab).Prewarm(count, duration);
        }

        /// <summary>
        ///	2017-8-7
        /// Clears the pool. Destroys all objects in the pool, then
        ///	deletes the pool as well. This can be expensive.
        /// </summary>
        public static void Clear(GameObject prefab)
        {
            GetPool(prefab).Clear();
        }

        /// <summary>
        /// 2017-8-4
        /// Destroys all unused items in excess of the each item pool's 
        /// maximum capacity. Maximum capacity is set using SetLimit().
        /// </summary>
        public static void Cull(GameObject prefab, bool immediate = false)
        {
            GetPool(prefab).Cull(immediate);
        }

        /// <summary>
        /// 2017-8-4
        /// Destroys all unused items that are stale. Stale items are
        /// objects that have been unused longer than Stale Duration.
        /// Stale Duration is set using SetLimit().
        /// </summary>
        public static void Expire(GameObject prefab, bool immediate = false)
        {
            GetPool(prefab).Expire(immediate);
        }

        #endregion

        /// <summary>
        /// 2017-8-4
        /// A pool for re-using GameObjects. Do not use this with POCOs. The creation/destruction
        /// procedure is not compatible between Unity objects and POCOs. Use PocoPool for POCOs.
        /// 
        ///     Use new Pool(prefab) to create a pool
        ///     Use Get(prefab) instead of Instantiate(prefab)
        ///     Use Put(item) instead of Destroy(item)
        ///     
        /// Optional "stale" duration -- any "available" items older than this
        ///     are automatically culled. This keeps memory usage down, at the
        ///     expensive of occassional GC.
        /// </summary>
        private class Pool : IPool
        {
            #region Data

            private GameObject prefab;
            protected int maxObjects;
            protected Dictionary<int, float> recycleTime;
            protected float staleDuration;
            protected List<GameObject> available;
            protected List<GameObject> inUse;

            #endregion

            /// <summary>
            /// 2017-8-7
            /// Prefab is the item the pool will manage.
            /// </summary>
            public Pool(GameObject prefab)
            {
                this.recycleTime = new Dictionary<int, float>();
                this.available = new List<GameObject>();
                this.inUse = new List<GameObject>();
                this.prefab = prefab;
                this.MaxSize(UNLIMITED);
                this.StaleDuration(UNLIMITED);
            }

            #region Basic Operations

            /// <summary>
            /// 2017-8-7
            /// Get a GameObject from the pool.
            /// The GameObject will be set active.
            /// The GameObject has been reset via broadcast message "Reset" and "Start".
            /// 
            /// Position will be (0,0,0), rotation will be (0,0,0)
            /// Scale is unaffected.
            /// </summary>
            public object Get()
            {
                //  when the number of items in use is at max, return null
                if (maxObjects != UNLIMITED && maxObjects <= inUse.Count) return null;

                GameObject item = null;

                //  grab an available item, or create a new one if neccessary
                if (available.Count > 0)
                {
                    //  grab an available item
                    item = this.available[0];
                    this.available.RemoveAt(0);
                    item.transform.position = Vector3.zero;
                    item.transform.rotation = Quaternion.identity;
                }
                else
                {
                    //  create a new item
                    item = CreateInstance();
                }
                this.inUse.Add(item);
                item.SetActive(true);
                item.BroadcastMessage("Reset", SendMessageOptions.DontRequireReceiver);
                item.BroadcastMessage("Start", SendMessageOptions.DontRequireReceiver);
                RemoveTimestamp(item);
                return item;
            }

            /// <summary>
            /// Put a GameObject back into the pool.
            /// The GameObject will be set inactive.
            /// Returns true if the object is reclaimed.
            /// Returns false if the object does not belong to this pool.
            /// Returns false if the object is null;
            /// </summary>
            public bool Put(object item)
            {
                if (item == null) return false;
                GameObject g = (GameObject)item;
                if (!this.inUse.Contains(g) && !this.available.Contains(g)) return false;
                g.SetActive(false);
                this.inUse.Remove(g);
                this.available.Add(g);
                SetTimestamp(g);
                return true;
            }

            /// <summary>
            ///	2016-5-9
            /// Clears this instance. Destroys all game objects maintained
            ///	by this object.
            /// </summary>
            public void Clear()
            {
                while (this.Available() > 0)
                {
                    GameObject g = this.available[0];
                    this.available.RemoveAt(0);
                    DestroyInstance(g);
                }
                while (this.InUse() > 0)
                {
                    GameObject g = this.inUse[0];
                    this.inUse.RemoveAt(0);
                    DestroyInstance(g);
                }
            }

            /// <summary>
            /// 2017-8-9
            /// Handles actual instantiation of game objects.
            /// </summary>
            private GameObject CreateInstance()
            {
                GameObject item = MonoBehaviour.Instantiate(prefab);
                item.name = prefab.name;
                return item;
            }

            /// <summary>
            /// 2017-8-8
            /// Handles actual destruction of game objects. Works in editor
            /// so unit tests can be conducted.
            /// </summary>
            /// <param name="instance"></param>
            private void DestroyInstance(GameObject instance)
            {
                if (Application.isEditor) MonoBehaviour.DestroyImmediate(instance);
                else MonoBehaviour.Destroy(instance);
            }

            #endregion
            #region Option: Pre-generation of objects

            /// <summary>
            /// 2017-8-7
            /// Pre-instantiates a number of items for the pool.
            /// The generation is distributed over [duration] seconds.
            /// Prewarm will never create more than one item per frame.
            /// </summary>
            public void Prewarm(int count, float duration)
            {
                if (count <= 0) return;

                float delay = Mathf.Max(1 / 60f, duration / count);

                PCoroutine.CreateCoroutineRunner(PrewarmEnumerator(count, delay), prefab.name);
            }

            /// <summary>
            /// 2017-8-8
            /// Coroutine that executes PrewarmImmediate over time.
            /// </summary>
            /// <param name="count">Quantity of items to instantiate</param>
            /// <param name="delay">Amount of time (seconds) to spread instantiation across</param>
            private IEnumerator PrewarmEnumerator(int count, float delay)
            {
                for (int i = 0; i < count; i++)
                {
                    PrewarmImmediate(1);

                    //  wait a few frames
                    yield return new WaitForSeconds(delay);
                }

                for (int i = 0; i < count; i++) Put(this.inUse[0]);
            }

            /// <summary>
            /// 2017-8-3
            /// Pre-instantiates a number of items for the pool.
            /// The generation is done immediately, during a single frame.
            /// Will not exceed item limit, if set.
            /// </summary>
            /// <param name="count"></param>
            private void PrewarmImmediate(int count = 1)
            {
                int total = -1;

                //  Generate the required number of items, 
                //  subject to the maxObjects limit (if it exists).
                for (int i = 0; i < count && (this.maxObjects > total || this.maxObjects == UNLIMITED); i++)
                {
                    this.inUse.Add(CreateInstance());
                    total = this.available.Count + this.inUse.Count;
                }
            }

            #endregion
            #region Option: Quantity-based Destruction

            /// <summary>
            /// 2017-8-8
            /// Sets the maximum pool size for this GameObject.
            /// Use PPool.UNLIMITED (-1) to indicate unlimited pool size.
            /// 
            /// When the new size is fewer than the existing number of items
            /// in the pool, the pool will automatically begins to cull excess
            /// items as they are recycled until the pool size is reduced to
            /// the new maximum.
            /// </summary>
            public void MaxSize(int size)
            {
                int originalSize = this.maxObjects;
                this.maxObjects = size;
                if (this.maxObjects != UNLIMITED) Cull();
            }

            /// <summary>
            /// 2017-8-4
            /// Ensures the pool has not exceeded the maximum pool size.
            /// This only needs to be called if the pool's maximum size has been reduced,
            /// and only if the pool was used extensively (or prewarmed) prior to the reduction.
            /// The culling occurs as objects are Returned to the pool -- so no
            /// objects will be destroyed while still in use.
            /// </summary>
            public void Cull(bool immediate = false)
            {
                //  if the pool size is set to 'infinite' then do nothing
                if (this.maxObjects == UNLIMITED) return;

                PCoroutine.CreateCoroutineRunner(CullEnumerator(immediate));
            }

            /// <summary>
            /// 2017-8-8
            /// Coroutine that executes CullImmediate() over time.
            /// </summary>
            /// <param name="immediate">Cull everything at once</param>
            private IEnumerator CullEnumerator(bool immediate)
            {
                //  Loop until the pool is culled to the appropriate amount.
                //  If the pool size is set to 'infinite' then abort culling.
                while (this.maxObjects < (this.available.Count + this.inUse.Count)
                    && this.maxObjects != UNLIMITED)
                {
                    CullImmediate(1);

                    if (immediate) continue;

                    //  wait a few frames
                    yield return new WaitForSeconds(Random.value + 0.1f);
                }
            }

            /// <summary>
            /// 2018-8-2
            /// Immediately culles the number of objects from the available pool.
            /// This can be expensive, depending on the cull count.
            /// </summary>
            private void CullImmediate(int count = 1)
            {
                //  loop until the pool is culled to the appropriate amount
                for (int i = 0; i < count && this.available.Count > 0; i++)
                {
                    GameObject g = this.available[0];
                    this.available.RemoveAt(0);
                    DestroyInstance(g);
                }
            }

            #endregion
            #region Option: Time-based Destruction

            /// <summary>
            /// 2017-8-11
            /// Sets a "stale" duration for unused objects. Any object that remains
            /// unused longer than this time is removed from the pool. Removing objects
            /// from the pool is counter to the pupose of using a pool, so the stale
            /// duration should be relatively high to minimize the impact of GC.
            /// </summary>
            public void StaleDuration(float duration)
            {
                #region Debug logging

                Debug.Log(string.Format("StaleDuration({0}) called", duration));
                Debug.Log(string.Format("Timestamp list size: {0}", this.recycleTime.Count));
                foreach (int id in this.recycleTime.Keys)
                {
                    Debug.Log(string.Format("Item {0} recycled at {1} ({2})",
                        id, this.recycleTime[id], IsExpired(id) ? "expired" : "ok"));
                }

                #endregion

                float original = this.staleDuration;
                this.staleDuration = duration;

                //  Set to "no limit"
                if (this.staleDuration == UNLIMITED)
                {
                    //  Remove all timestamps
                    this.recycleTime.Clear();
                    return;
                }

                if (this.staleDuration != UNLIMITED)
                {
                    //  Stale time reduced
                    if (this.staleDuration < original)
                    {
                        //  Check if any existing timestamp are now stale
                        Expire(true);
                    }

                    //  Ensure all available items have a timestamp
                    GameObject g;
                    for (int i = 0; i < this.available.Count; i++)
                    {
                        g = this.available[i];

                        //  skip items that already have a timestamp
                        if (this.recycleTime.ContainsKey(g.GetHashCode())) continue;
                        SetTimestamp(g);
                    }
                }
            }

            /// <summary>
            /// 2017-8-4
            /// Enters a time stamp for the GameObject that indicates when the
            /// object was recycled (returned to this pool).
            /// </summary>
            private void SetTimestamp(GameObject item)
            {
                if (this.staleDuration == UNLIMITED) return;
                recycleTime.Add(item.GetHashCode(), Time.time);
            }

            /// <summary>
            /// 2017-8-11    
            /// Returns the expiration time (in seconds) for the item.
            /// The expiration time is calculated as follows:
            ///     item recycle timestamp + current stale duration
            /// Returns -1 when the Pool object is set to UNLIMITED stale times.
            /// Returns -1 when the item does not have a recycle timestamp.
            /// </summary>
            /// <param name="item">An intance supplied by this recycler</param>
            /// <returns>The game time when this item will (or did) expire</returns>
            private float GetExpireTime(GameObject item)
            {
                return GetExpireTime(item.GetHashCode());
            }

            /// <summary>
            /// 2017-8-11
            /// Returns the expiration time (in seconds) for the item.
            /// The expiration time is calculated as follows:
            ///     item recycle timestamp + current stale duration
            /// Returns -1 when the Pool object is set to UNLIMITED stale times.
            /// Returns -1 when the item does not have a recycle timestamp.
            /// </summary>
            /// <param name="itemHashCode">The hashcode for an object intance supplied by this recycler</param>
            /// <returns>The game time when this item will (or did) expire</returns>
            private float GetExpireTime(int itemHashCode)
            {
                if (this.staleDuration == UNLIMITED) return -1;
                if (!this.recycleTime.ContainsKey(itemHashCode)) return -1;
                return recycleTime[itemHashCode] + this.staleDuration;
            }

            /// <summary>
            /// 2017-8-2
            /// Removes the GameObject's "stale" time stamp from the library.
            /// This should be called whenever an object is requested and
            /// whenever an item is destroyed.
            /// </summary>
            private void RemoveTimestamp(GameObject item)
            {
                int id = item.GetHashCode();
                if (recycleTime.ContainsKey(id)) recycleTime.Remove(id);
            }

            /// <summary>
            /// 2017-8-11
            /// Indicates if the provided item has expired.
            /// Returns false if the Pool is set to UNLIMITED stale duration.
            /// Returns false if the item has no stale duration.
            /// Returns false if the item is not stale.
            /// Returns true if the item is stale.
            /// </summary>
            /// <param name="itemHashCode">The hashcode for an object intance supplied by this recycler</param>
            /// <returns>Whether the item is expired and should be destroyed</returns>
            private bool IsExpired(int itemHashCode)
            {
                if (this.staleDuration == UNLIMITED) return false;

                if (!this.recycleTime.ContainsKey(itemHashCode)) return false;

                return Time.time > GetExpireTime(itemHashCode);
            }

            /// <summary>
            /// 2017-8-2
            /// Destroys any GameObjects that is not currently 
            /// in use, and has not been used for a long time (stale time).
            /// </summary>
            public void Expire(bool immediate = false)
            {
                if (this.staleDuration == UNLIMITED) return;
                PCoroutine.CreateCoroutineRunner(ExpireEnumerator(immediate), prefab.name);
            }

            /// <summary>
            /// 2017-8-8
            /// Coroutine that executes ExpireImmediate() over time.
            /// </summary>
            /// <param name="immediate">Expire everything at once</param>
            private IEnumerator ExpireEnumerator(bool immediate)
            {
                //  Loop until all items with stale times are destroyed,
                //  OR until there are no items with timestamps.
                while (this.recycleTime.Count > 0)
                {
                    //  Convert the dictionary of expired times to a sortable object
                    List<KeyValuePair<int, float>> sortedDict = this.recycleTime.ToList();

                    //  sort entries by time
                    sortedDict.Sort((l, r) => { return l.Value.CompareTo(r.Value); });

                    //  When the first timestamp in the sorted stale list has
                    //  not expired, then exit because nothing else has expired either.
                    float currentTime = Time.time;
                    float expiretime = sortedDict[0].Value + this.staleDuration;
                    if (currentTime < expiretime) yield break;

                    ExpireImmediate(1);

                    if (immediate) continue;

                    //  wait a few frames
                    yield return new WaitForSeconds(Random.value+1);
                }
            }

            /// <summary>
            /// 2017-8-4
            /// Destroys up to "count" number of available (unused) items that
            /// have exceeded their stale time.
            /// </summary>
            private void ExpireImmediate(int count = 1)
            {
                //  Iterate the list of available objects.
                //  Remove expired items until the required count is reached
                //  OR until the end of the list is reached.
                for (int i = 0, expired = 0; i < this.available.Count && expired < count; /**/)
                {
                    GameObject g = this.available[i];

                    //  check expiration
                    if (Time.time < GetExpireTime(g))
                    {
                        //  item has not expired, so move to next
                        i++;
                        continue;
                    }
                    else
                    {
                        //  item has expired, so remove and destroy it
                        this.recycleTime.Remove(g.GetHashCode());
                        this.available.RemoveAt(i);
                        DestroyInstance(g);
                        expired++;
                    }
                }
            }

            #endregion
            #region Diagnostic

            public int Available() { return this.available.Count; }
            public int InUse() { return this.inUse.Count; }

            /// <summary>
            /// 2017-8-2
            /// Displays contents of pool, separated into "available"
            /// and "in use" groups.
            /// </summary>
            public new string ToString()
            {
                StringBuilder b = new StringBuilder();
                b.Append("Prefab: " + prefab.ToString());
                b.Append(" (Max pool size: " + maxObjects + ")\n");
                b.Append(" (Stale duration: " + this.staleDuration + ")\n");
                b.Append("Available: (" + available.Count + ")\n");
                b.Append(available.ContentsToString(","));
                b.Append("Active: (" + inUse.Count + ")\n");
                b.Append(inUse.ContentsToString(","));
                return b.ToString();
            }

            #endregion
        }
    }
}