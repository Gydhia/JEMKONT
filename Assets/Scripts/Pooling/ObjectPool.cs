using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DownBelow.Pools
{
    public abstract class ObjectPool<ObjectPoolType> : MonoBehaviour, IObjectPool
        where ObjectPoolType : MonoBehaviour, IPoolable
    {
        [Header("Object Pool Element")]
        [Tooltip("REQUIRED ; The GameObject that holds the poolable objects")]
        public GameObject PoolContainer;
        [Header("Object Pool Element")]
        [Tooltip("REQUIRED ; The GameObject that holds the poolable objects")]
        private Transform _poolContainerTransform;

        [Header("Object Pool Settings")]
        [Tooltip("REQUIRED ; The poolable prefab to instantiate ")]
        public ObjectPoolType ObjectPoolPrefab;
        [Tooltip("REQUIRED ; The number of minimum poolable objects which we should always have in the pool")]
        public int ForesightNumber = 20;

        [Header("Runtime Values")]
        [ShowInInspector]
        public Stack<ObjectPoolType> ObjectsPool;

        /// <summary> The coroutine used to add asynchronously the poolable objects</summary>
        private Coroutine _addingCoroutine;
        [ShowInInspector]
        [ReadOnly]
        private int _nbToAdd = 0;
        /// <summary>
        /// Number of object created by the pool since its creation
        /// </summary>
        [ShowInInspector]
        [ReadOnly]
        public int ObjectCreated
        {
            get;
            private set;
        }
        /// <summary>
        /// Number of object served by the pool since its creation
        /// </summary>
        [ShowInInspector]
        [ReadOnly]
        public int ObjectGiven
        {
            get;
            private set;
        }

        private void Awake()
        {
            // init the stack with a memory size equal to double the Foresight
            this.ObjectsPool = new Stack<ObjectPoolType>(this.ForesightNumber * 2);

            if (this.PoolContainer == null)
                this.PoolContainer = this.gameObject;

            this._poolContainerTransform = this.PoolContainer.transform;
        }

        public void InitPool(int nbOfObjects, int foresightNumber)
        {
            this.ForesightNumber = foresightNumber;
            InitPool(nbOfObjects);
        }

        public void InitPool(int nbOfObjects)
        {
            this.ObjectsPool ??= new Stack<ObjectPoolType>();
            
            this.CheckPoolState(true);
        }

        /// <summary>
        /// Will resize the pooled objects stack ; create the pooled objects but not init them
        /// </summary>
        /// <param name="nbToAdd">Number of pooled object that needs to be added to the array</param>
        /// <param name="isAsync">Do we need it now or do we just prepare for future insertion</param>
        public void AskForMore(int nbToAdd, bool isUrgent = false)
        {
            if (nbToAdd <= 0)
                return;

            if (!isUrgent)
            {
                // Increment the nb of pooled objects to add used in the coroutine
                this._nbToAdd += nbToAdd;
                // If the coroutine isn't started, execute it until there aren't anymore objects to add
                if (this._addingCoroutine == null)
                {
                    this._addingCoroutine = StartCoroutine(FillPoolAsync());
                }
            }
            else
            {
                // in case of any FillPoolAsync ongoing, decrement the number of this urgent call
                if (this._nbToAdd > 0)
                    this._nbToAdd = this._nbToAdd - nbToAdd < 0 ? 0 : nbToAdd;

                FillPool(nbToAdd);
            }
        }

        /// <summary>
        /// Fill the pool instantly with new instantiated pooled object
        /// </summary>
        /// <param name="nbToAdd">Number of pooled objects to add</param>
        public void FillPool(int nbToAdd)
        {
            for (int i = 0; i < nbToAdd; i++)
            {
                ObjectsPool.Push(CreatePooled());
                ObjectCreated++;
            }
        }
        /// <summary>
        /// Fill the pool asynchronously with new instantiated pooled object
        /// </summary>
        public IEnumerator FillPoolAsync()
        {
            while (_nbToAdd > 0)
            {
                ObjectsPool.Push(CreatePooled());
                ObjectCreated++;
                _nbToAdd--;
                yield return null;
            }
            _addingCoroutine = null;
        }

        /// <summary>Need to be override to create the right pooled object</summary> 
        protected virtual ObjectPoolType CreatePooled()
        {
            ObjectPoolType pooled = Instantiate(ObjectPoolPrefab, this._poolContainerTransform);
            pooled.Pool = this;
            pooled.gameObject.SetActive(false);
            return pooled;
        }

        /// <summary>
        /// Return an available object from the pool
        /// </summary>
        /// <returns></returns>
        public virtual ObjectPoolType GetPooled()
        {
            this.ObjectGiven++;
            if (ObjectsPool.Count > 0)
            {
                ObjectPoolType objectPool = ObjectsPool.Pop();
                objectPool.Pooled = true;
                return objectPool;
            }
            else
            {
                AskForMore(5, true);
                return GetPooled();
            }
        }

        public bool TryReleaseToPool(object poolObject)
        {
            return this.TryReleasePooled((ObjectPoolType)poolObject);
        }

        /// <summary>
        /// When a pooled object becomes unused, use this function to disable it and add it to the pool
        /// </summary>
        /// <param name="poolObject"></param>
        public virtual bool TryReleasePooled(ObjectPoolType poolObject)
        {
            bool ok = false;
            if (poolObject != null && poolObject.Pooled)
            {
                ObjectsPool.Push(poolObject);
                poolObject.Pooled = false;
                poolObject.gameObject.SetActive(false);
                poolObject.transform.SetParent(this._poolContainerTransform, true);
                poolObject.DisableFromPool();

                ok = true;
            }
            else
            {
                Debug.LogWarning("You're trying to release a pooled object already released " + poolObject.name);
            }

            // ask if the object is null a second time because it could have been destroyed during DisableFromPool()
            if (poolObject != null)
                poolObject.gameObject.SetActive(false);

            return ok;
        }

        /// <summary>
        /// Used to destroy a certain range of the objects pool.
        /// </summary>
        /// <param name="rangeToDelete"></param>
        public void DestroyMultiplePooled(int rangeToDelete)
        {
            if (this.ObjectsPool.Count < rangeToDelete)
                rangeToDelete = this.ObjectsPool.Count;

            for (int i = 0; i < rangeToDelete; i++)
            {
                ObjectPoolType obj = this.ObjectsPool.Pop();

                obj.DisableFromPool();
                obj.gameObject.SetActive(false);
                Destroy(obj.gameObject);
            }
        }

        /// <summary>
        /// To be called to check if we got enough objects in stack. Will delete or add poolable objects according to the <see cref="ForesightNumber"/>
        /// </summary>
        public bool CheckPoolState(bool urgent = false)
        {
            if (this.ObjectsPool.Count < this.ForesightNumber)
                AskForMore(this.ForesightNumber - this.ObjectsPool.Count, urgent);

            return true;
            // TO DO ; think about an improvement to save memory in case of huge requests
            //else if(ObjectsPool.Count > ForesightNumber) {
            //    //if (_addingCor != null) {
            //    //    StopCoroutine(_addingCor);
            //    //    _addingCor = null;
            //    //    _nbToAdd = 0;
            //    //}
            //    //this.DestroyMultiplePooled(ForesightNumber - ObjectsPool.Count);
            //}
        }
    }
}
