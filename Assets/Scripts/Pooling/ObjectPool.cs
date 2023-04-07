using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ObjectPool<ObjectPoolType> : MonoBehaviour
    where ObjectPoolType : MonoBehaviour, IPoolable
{
    [Header("Object Pool Element")]
    [Tooltip("REQUIRED ; The GameObject that holds the poolable objects")]
    public GameObject PoolContainer;

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
    }

    public void InitPool(int nbOfObjects)
    {
        // TO-DO : Use CheckPoolState() instead of the code below when it'll be able to cleanup the number of powerball
        int nbToCreate = nbOfObjects + this.ForesightNumber - this.ObjectsPool.Count;
        AskForMore(nbToCreate, true);
    }

    /// <summary>
    /// Will resize the pooled objects stack ; create the pooled objects but not init them
    /// </summary>
    /// <param name="nbToAdd">Number of pooled object that needs to be added to the array</param>
    /// <param name="isAsync">Do we need it now or do we just prepare for future insertion</param>
    public void AskForMore(int nbToAdd, bool isUrgent = false)
    {
        if (!isUrgent)
        {
            // Increment the nb of pooled objects to add used in the coroutine
            _nbToAdd += nbToAdd;
            // If the coroutine isn't started, execute it until there aren't anymore objects to add
            if (_addingCoroutine == null)
            {
                _addingCoroutine = StartCoroutine(FillPoolAsync());
            }
        } else
        {
            if (_nbToAdd > 0)
                _nbToAdd = _nbToAdd - nbToAdd < 0 ? 0 : nbToAdd;
            else
                CheckPoolState();

            FillPool(nbToAdd);
        }
    }

    /// <summary>
    /// Fill the pool instantly with new instantiated pooled object
    /// </summary>
    /// <param name="nbToAdd">Number of pooled objects to add</param>
    public void FillPool(int nbToAdd)
    {
        for (int i = 0;i < nbToAdd;i++)
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
        ObjectPoolType pooled = Instantiate(ObjectPoolPrefab, PoolContainer.transform);
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
            return ObjectsPool.Pop();
        } else
        {
            AskForMore(1, true);
            return GetPooled();
        }
    }

    /// <summary>
    /// When a pooled object becomes unused, use this function to disable it and add it to the pool
    /// </summary>
    /// <param name="poolObject"></param>
    public virtual void ReleasePooled(ObjectPoolType poolObject)
    {
        if (poolObject != null)
        {
            ObjectsPool.Push(poolObject);
            poolObject.DisableFromPool();
        }

        // ask if the object is null a second time because it could have been destroyed during DisableFromPool()
        if (poolObject != null)
            poolObject.gameObject.SetActive(false);

    }

    /// <summary>
    /// Used to destroy a certain range of the objects pool.
    /// </summary>
    /// <param name="rangeToDelete"></param>
    public void DestroyMultiplePooled(int rangeToDelete)
    {
        if (ObjectsPool.Count < rangeToDelete)
            rangeToDelete = ObjectsPool.Count;

        for (int i = 0;i < rangeToDelete;i++)
        {
            ObjectPoolType obj = ObjectsPool.Pop();

            obj.DisableFromPool();
            obj.gameObject.SetActive(false);
            Destroy(obj.gameObject);
        }
    }

    /// <summary>
    /// To be called to check if we got enough objects in stack. Will delete or add poolable objects according to the <see cref="ForesightNumber"/>
    /// </summary>
    public void CheckPoolState()
    {
        if (ObjectsPool.Count < ForesightNumber)
        {
            AskForMore(ForesightNumber - ObjectsPool.Count);
        }
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
