using UnityEngine;
using System.Collections.Generic;
using System.Text;
using PLib.General;
using System.Threading;
using System;
using System.Linq;

namespace PLib.Pooling
{
    /// <summary>
    /// 2017-8-7
    /// 
    /// Manager object for GameObject pooling. 
    /// This manages a dictionary of individual Pools for each type of object. Objects
    /// are distinguished by their Hashcode.
    /// 
    /// To use with GameObjects:
    ///     instead of "Instantiate(prefab)", simply use "PPool.Get(prefab)"
    ///     instead of "Destroy(item)", simply use "PPool.Put(item)"
    ///     
    /// That's it. Everything else is managed behind the scenes.
    /// </summary>
    public static class PPool
    {
        #region Data Libraries
        //////////////////////////
        //	Data Libraries		//
        //////////////////////////

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

        public const int UNLIMITED = -1;

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
            GameObject item = (GameObject)(GetPool(prefab) as Pool).Get();
            item.transform.position = position;
            item.transform.rotation = rotation;
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
            int hash = item.GetHashCode();

            IPool pool;
            if (pools.TryGetValue(hash, out pool))
            {
                pool.Put(item);
                result = true;
            }
            else
            {
                MonoBehaviour.Destroy(item);
                result = false;
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
        public static void Prewarm(GameObject prefab, int count, float duration = 3)
        {
            GetPool(prefab).Prewarm(count, duration);
        }

        /// <summary>
        ///	2017-8-7
        /// Clears the pool. Destroys all objects in the pool, then
        ///	deletes the pool as well. This can be expensive.
        /// </summary>
        public static void Clear(GameObject prefab, bool immediate = false)
        {
            GetPool(prefab).Clear(immediate);
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

            private GameObject model;
            protected int max;
            protected Dictionary<int, float> staleTime;
            protected float stale;
            protected List<GameObject> available;
            protected List<GameObject> inUse;

            #endregion

            /// <summary>
            /// Prefab is the item the pool will manage.
            /// </summary>
            public Pool(GameObject prefab)
            {
                this.staleTime = new Dictionary<int, float>();
                this.available = new List<GameObject>();
                this.inUse = new List<GameObject>();
                this.model = prefab;
                this.MaxSize(UNLIMITED);
                this.StaleDuration(UNLIMITED);
            }

            #region Basic Operations

            /// <summary>
            /// 2016-2-17
            /// Get a GameObject from the pool.
            /// The GameObject will be set active.
            /// The GameObject has been reset via broadcast message "Reset" and "Start".
            /// 2016-5-16
            /// Position will be (0,0,0), rotation will be (0,0,0)
            /// Scale is unaffected.
            /// </summary>
            /// <returns></returns>
            public object Get()
            {
                //  when the number of items in use is at max, return null
                if (max > 0 && inUse.Count == max) return null;

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
                    item = MonoBehaviour.Instantiate(model);
                    item.name = model.name;
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
            /// </summary>
            public void Put(object item)
            {
                if (item == null) return;
                GameObject g = (GameObject)item;
                g.SetActive(false);
                this.inUse.Remove(g);
                this.available.Add(g);
                SetRecycleTime(g);
            }

            /// <summary>
            ///	2016-5-9
            /// Clears this instance. Destroys all game objects maintained
            ///	by this object.
            /// </summary>
            public void Clear(bool immediate = false)
            {
                foreach (GameObject g in this.available)
                    if (immediate)
                    {
                        MonoBehaviour.DestroyImmediate(g);
                    }
                    else
                    {
                        MonoBehaviour.Destroy(g);
                    }
                foreach (GameObject g in this.inUse)
                    if (immediate)
                    {
                        MonoBehaviour.DestroyImmediate(g);
                    }
                    else
                    {
                        MonoBehaviour.Destroy(g);
                    }
            }

            #endregion
            #region Option: Pre-generation of objects

            /// <summary>
            /// 2017-8-3
            /// Pre-instantiates a number of items for the pool.
            /// The generation is distributed over [duration] seconds.
            /// Prewarm will never create more than one item per frame.
            /// </summary>
            public void Prewarm(int count, float duration)
            {
                if (count <= 0) return;
                if (duration <= 0) return;

                float delay = Mathf.Min(1 / 60f, duration) / count;

                PrewarmImmediate(count);

                //  Preserved, but commented because Task isn't working.
                //TimeSpan timeout = TimeSpan.FromSeconds(delay);                
                //Task t = Task.Run(() =>
                //{
                //    //  Create the things.
                //    //  Create no more than [count] things.
                //    for (int i = 0; i < count; i++)
                //    {
                //        PrewarmImmediate(1);
                //        Thread.Sleep(timeout);
                //    }
                //});
                Debug.LogWarning("Pool.Prewarm() does not implement any sort of delay.");
            }

            /// <summary>
            /// 2017-8-3
            /// Pre-instantiates a number of items for the pool.
            /// The generation is done immediately, during a single frame.
            /// </summary>
            /// <param name="count"></param>
            private void PrewarmImmediate(int count = 1)
            {
                int total = this.available.Count + this.inUse.Count;
                for (int i = 0; i < count && ((total + i) < max || max == UNLIMITED); i++)
                {
                    this.Put(MonoBehaviour.Instantiate(model));
                }
            }

            #endregion
            #region Option: Quantity-based Destruction

            /// <summary>
            /// 2017-8-4
            /// Sets the maximum pool size for this GameObject.
            /// Use -1 to indicate unlimited pool size.
            /// </summary>
            public void MaxSize(int size)
            {
                int originalSize = this.max;
                this.max = size;
                if (this.max < originalSize && size != UNLIMITED) Cull();
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
                if (this.max == UNLIMITED) return;

                //TimeSpan timeout = TimeSpan.FromSeconds(0.1f);
                //Task t = Task.Run(() =>
                //{
                //  loop until the pool is culled to the appropriate amount
                for (int i = this.max; i < (this.available.Count + this.inUse.Count)
                        && this.max != UNLIMITED; i++)
                {
                    CullImmediate(1);

                    //if (immediate) continue;

                    //  wait a few frames
                    //Thread.Sleep(timeout);
                }
                //});
                Debug.LogWarning("Pool.Cull() does not implement any sort of delay.");
            }

            /// <summary>
            /// 2018-8-2
            /// Immediately culles the number of objects from the available pool.
            /// This can be expensive, depending on the cull count.
            /// </summary>
            private void CullImmediate(int count = 1)
            {
                //  if the pool size is set to 'infinite' then abort culling
                if (this.max == UNLIMITED) return;

                //  loop until the pool is culled to the appropriate amount
                for (int i = 0; i < count && this.available.Count > 0; i++)
                {
                    GameObject g = this.available[0];
                    this.available.RemoveAt(0);
                    MonoBehaviour.Destroy(g);
                }
            }

            #endregion
            #region Option: Time-based Destruction

            /// <summary>
            /// 2017-8-2
            /// Sets a "stale" duration for unused objects. Any object that remains
            /// unused longer than this time is removed from the pool. Removing objects
            /// from the pool is counter to the pupose of using a pool, so the stale
            /// duration should be relatively high to prevent GC.
            /// </summary>
            public void StaleDuration(float duration)
            {
                float original = this.stale;
                this.stale = duration;

                //  Set to "no limit"
                if (this.stale == UNLIMITED)
                {
                    //  Empty the stale timestamps
                    this.staleTime.Clear();
                }
                else
                {
                    //  Stale time reduced
                    if (this.stale < original)
                    {
                        //  Check if any existing timestamp are now stale
                        Expire();
                    }

                    //  Ensure all available items have a stale timestamp
                    foreach (GameObject g in this.available)
                    {
                        if (this.staleTime.ContainsKey(g.GetHashCode())) continue;
                        SetRecycleTime(g);
                    }
                }
            }

            /// <summary>
            /// 2017-8-4
            /// Enters a time stamp for the GameObject that indicates when the
            /// object was recycled (returned to this pool).
            /// </summary>
            private void SetRecycleTime(GameObject item)
            {
                if (this.stale == UNLIMITED) return;
                staleTime.Add(item.GetHashCode(), Time.time);
            }

            /// <summary>
            /// Returns the expiration time (in seconds) for the item.
            /// Returns -1 when the Pool object is set to UNLIMITED stale times.
            /// Returns -1 when the item is in use (thus, not in the pool).
            /// </summary>
            private float GetExpireTime(GameObject item)
            {
                if (this.stale == UNLIMITED) return -1;
                if (this.inUse.Contains(item)) return -1;
                return staleTime[item.GetHashCode()] + this.stale;
            }

            /// <summary>
            /// 2017-8-2
            /// Removes the GameObject's "stale" time stamp from the library.
            /// This should be called whenever an object is requested and
            /// whenever an item is destroyed.
            /// </summary>
            private void RemoveTimestamp(GameObject item)
            {
                if (this.stale == UNLIMITED) return;
                int id = item.GetHashCode();
                if (!staleTime.ContainsKey(id)) return;
                staleTime.Remove(id);
            }

            /// <summary>
            /// 2017-8-4
            /// Indicates if the provided item has expired.
            /// </summary>
            private bool IsExpired(GameObject item)
            {
                if (this.stale == UNLIMITED) return false;

                int id = item.GetHashCode();
                if (!this.staleTime.ContainsKey(id)) return false;

                return Time.time > GetExpireTime(item);
            }

            /// <summary>
            /// 2017-8-2
            /// Destroys any GameObjects that is not currently 
            /// in use, and has not been used for a long time (stale time).
            /// </summary>
            public void Expire(bool immediate = false)
            {
                if (this.stale == UNLIMITED) return;

                //TimeSpan timeout = TimeSpan.FromSeconds(0.1f);
                //Task t = Task.Run(() =>
                //{
                //  loop indefinitely, until all stale items are destroyed
                while (this.staleTime.Count > 0)
                {
                    //  Convert the dictionary of expired times to a sortable object
                    List<KeyValuePair<int, float>> sortedDict = this.staleTime.ToList();

                    //  sort stale times by value
                    sortedDict.Sort((l, r) => { return l.Value.CompareTo(r.Value); });

                    //  When the first timestamp in the sorted stale list has
                    //  not expired, then exit because nothing else has expired either.
                    if ((sortedDict[0].Value + this.stale) > Time.time) return;

                    ExpireImmediate(1);

                    //if (immediate) continue;

                    //  wait a few frames
                    //Thread.Sleep(timeout);
                }
                //});
                Debug.LogWarning("Pool.Expire() does not implement any sort of delay.");
            }

            /// <summary>
            /// 2017-8-4
            /// Destroys up to "count" number of available (unused) items that
            /// have exceeded their stale time.
            /// </summary>
            private void ExpireImmediate(int count = 1)
            {
                //  Iterate the available list.
                //  Exit when "count" items have been expired OR
                //      the beginning of list is reached.
                for (int i = this.available.Count, expired = 0; i > 0 && expired < count; i--)
                {
                    int index = i - 1;
                    GameObject g = this.available[index];

                    if (Time.time < GetExpireTime(g)) continue;

                    this.staleTime.Remove(g.GetHashCode());
                    this.available.RemoveAt(index);
                    MonoBehaviour.Destroy(g);
                    expired++;
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
                b.Append("Prefab: " + model.ToString());
                b.Append(" (Max pool size: " + max + ")\n");
                b.Append(" (Stale duration: " + this.stale + ")\n");
                b.Append("Available: (" + available.Count + ")\n");
                b.Append(available.ContentsToString(","));
                b.Append("Active: (" + inUse.Count + ")\n");
                b.Append(inUse.ContentsToString(","));
                return b.ToString();
            }

            #endregion
        }
    }

    #region Pool Interface

    public interface IPool
    {
        #region Basic Operations

        object Get();
        void Put(object item);
        void Clear(bool immediate);

        #endregion
        #region Option: Pre-generation of objects

        void Prewarm(int count, float duration);

        #endregion
        #region Option: Quantity-based Destruction

        void MaxSize(int size);
        void Cull(bool immediate);

        #endregion
        #region Option: Time-based Destruction

        void StaleDuration(float duration);
        void Expire(bool immediate);

        #endregion
        #region Diagnostic

        int Available();
        int InUse();

        #endregion
    }

    #endregion
}