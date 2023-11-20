using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace Bercetech.Games.Fleepas
{
    public class ObjectPoolUI : MonoBehaviour
    {
        // Define parameters
        // Defining a static shared instance variable so other scripts can access to the object pool
        private static ObjectPoolUI _sharedInstace;
        public static ObjectPoolUI SharedInstace => _sharedInstace;
        private List<GameObject> _pooledObjects;
        public List<GameObject> PooledObjects => _pooledObjects;
        // Group of parameters that may change between object pools
        [Serializable]
        public struct ObjectPoolItem
        {
            public GameObject ObjectToPool;
            public int AmountToPool;
            // Boolean to keep increasing the pool in case we reach the limite in GetPooledObject
            public bool ShouldExpand;
            public bool InstantiateActive;
            public bool SetParent;

            public ObjectPoolItem(GameObject objectToPool, int amountToPool, bool shouldExpand, bool instantiateActive, bool setParent)
            {
                ObjectToPool = objectToPool;
                AmountToPool = amountToPool;
                ShouldExpand = shouldExpand;
                InstantiateActive = instantiateActive;
                SetParent = setParent;
            }
        }

        [SerializeField]
        private Transform _gameObjectsParentTransform;
        [SerializeField]
        private List<ObjectPoolItem> _itemsToPool;
        public static List<ObjectPoolItem> ItemsToPool;

        // Defining a Touch Stream
        private static Signal _poolObjectsGenerated = new Signal();
        public static Signal PoolObjectsGenerated => _poolObjectsGenerated;



        void Awake()
        {
            if (_sharedInstace != null && _sharedInstace != this)
            {
                Destroy(this.gameObject);
            }
            else
            {
                _sharedInstace = this;
            }

            ItemsToPool = _itemsToPool;
        }

        // Instantiating amount of objects during start.
        void Start()
        {
            _pooledObjects = new List<GameObject>();
            foreach (ObjectPoolItem item in _itemsToPool)
            {
                for (int i = 0; i < item.AmountToPool; i++)
                {
                    GameObject tmp = Instantiate(item.ObjectToPool);
                    if (item.SetParent)
                        tmp.transform.SetParent(_gameObjectsParentTransform, false);
                    tmp.SetActive(item.InstantiateActive);
                    _pooledObjects.Add(tmp);
                }
            }

            // Throwing event when instantiation is finished
            _poolObjectsGenerated.Fire();
        }

        // Call this function to access objects from the pool
        // Returns "null" in case all objects are active in the hierarchy
        public GameObject GetPooledObject(string tag)
        {
            GameObject nextObject = _pooledObjects.Where(t => !t.activeInHierarchy & t.tag == tag).FirstOrDefault();

            if (nextObject == null)
            {
                // Expanding the pool in case all the objects are already active
                var pool = _itemsToPool.Where(t => t.ObjectToPool.tag == tag & t.ShouldExpand).FirstOrDefault();
                if (pool.ObjectToPool != null)
                {
                    GameObject obj = Instantiate(pool.ObjectToPool);
                    if (pool.SetParent)
                        obj.transform.parent = _gameObjectsParentTransform;
                    obj.SetActive(pool.InstantiateActive);
                    _pooledObjects.Add(obj);
                    return obj;
                }
            }
            // nextObject may be null
            return nextObject;
        }

        // Call this function to access objects from the pool
        // Returns "null" in case all objects are active in the hierarchy
        public GameObject GetPooledObjectByName(string name)
        {
            GameObject nextObject = _pooledObjects.Where(t => !t.activeInHierarchy & t.name == name+"(Clone)").FirstOrDefault();

            if (nextObject == null)
            {
                // Expanding the pool in case all the objects are already active
                var pool = _itemsToPool.Where(t => t.ObjectToPool.name == name & t.ShouldExpand).FirstOrDefault();
                if (pool.ObjectToPool != null)
                {
                    GameObject obj = Instantiate(pool.ObjectToPool);
                    if (pool.SetParent)
                        obj.transform.parent = _gameObjectsParentTransform;
                    obj.SetActive(pool.InstantiateActive);
                    _pooledObjects.Add(obj);
                    return obj;
                }
            }
            // nextObject may be null
            return nextObject;
        }

        // Call this function to access objects from the pool
        // Returns "null" in case all objects are active in the hierarchy
        public ObjectPoolItem[] GetPooledObjectNamesWithTag(string tag)
        {
            return _itemsToPool.Where(t => t.ObjectToPool.tag == tag).Distinct().ToArray();
        }


        public void DestroyPooledObjectByName(string name)
        {
            GameObject[] objects = _pooledObjects.Where(t => t.name == name + "(Clone)").ToArray();
            foreach (GameObject objectToDestroy in objects) {
                _pooledObjects.Remove(objectToDestroy);
                Destroy(objectToDestroy);
            }
        }

        public void ChangePooledObject(string objectToChangeTag, GameObject newObject)
        {
            // Get the current item in the pool
            ObjectPoolItem itemToChange = _itemsToPool.Where(t => t.ObjectToPool.tag == objectToChangeTag).FirstOrDefault();
            // Destroy it from the pool
            DestroyPooledObjectByName(itemToChange.ObjectToPool.name);
            // Create new object in the pool with the same parameters as the original item
            for (int i = 0; i < itemToChange.AmountToPool; i++)
            {
                GameObject tmp = Instantiate(newObject);
                if (itemToChange.SetParent)
                    tmp.transform.SetParent(_gameObjectsParentTransform, false);
                tmp.SetActive(itemToChange.InstantiateActive);
                _pooledObjects.Add(tmp);
            }
            // Create new ObjectPoolItem to Add to ItemsToPool and remove the old one
            _itemsToPool = _itemsToPool.Where(t => t.ObjectToPool.tag != objectToChangeTag).ToList();
            var newObjectPoolItem = new ObjectPoolItem(newObject, itemToChange.AmountToPool, itemToChange.ShouldExpand,
                itemToChange.InstantiateActive, itemToChange.SetParent);
            _itemsToPool.Add(newObjectPoolItem);
        }

    }
}