using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OliverBeebe.UnityUtilities.Runtime.Pooling
{
    [CreateAssetMenu(menuName = "Oliver Utilities/Pooling/GameObject Pool")]
    public class GameObjectPool : ScriptableObject
    {
        [SerializeField] private Poolable prefab;
        [SerializeField] private bool setActiveOnRetrieval;

        private static Transform allParent;
        private Transform heirarchyParent;

        private Transform currentParent;

        private ObjectPool<Poolable> pool;
        private ObjectPool<Poolable> Pool => pool ??= new();

        protected virtual Transform SpawnHeirarchyParent() => new GameObject().transform;

        private void Initialize()
        {
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += (from, to) => Clear(); 

            if (allParent == null)
            {
                allParent = new GameObject("GameObject Pools").transform;
            }

            heirarchyParent = SpawnHeirarchyParent();
            heirarchyParent.parent = allParent;
            heirarchyParent.name = name;
        }

        private Poolable Create()
        {
            if (heirarchyParent == null)
            {
                Initialize();
            }

            return Instantiate(prefab, currentParent);
        }

        private void Destroy(Poolable poolable)
        {
            if (poolable != null && poolable.gameObject != null)
            {
                Destroy(poolable.gameObject);
            }
        }

        public void Generate(int count) => Generate(count, heirarchyParent);
        public void Generate(int count, Transform parent)
        {
            currentParent = parent;
            Pool.Generate(count, Create);
        }

        public Poolable Retrieve() => Retrieve(heirarchyParent);
        public Poolable Retrieve(Transform parent)
        {
            currentParent = parent;
            var poolable = Pool.Retrieve(Create);

            poolable.Retrieve();
            poolable.Returned += OnReturned;

            if (setActiveOnRetrieval)
            {
                poolable.gameObject.SetActive(true);
            }

            return poolable;
        }

        public void RetrieveAll() => Pool.RetrieveAll();
        
        private void OnReturned(Poolable poolable)
        {
            poolable.Returned -= OnReturned;

            if (setActiveOnRetrieval)
            {
                poolable.gameObject.SetActive(false);
            }

            Pool.Return(poolable);
        }
        
        public void Return(Poolable poolable)
        {
            poolable.Return();
        }

        public void ReturnAll()
        {
            foreach (var poolable in Pool.ActiveObjects)
            {
                poolable.Return();
            }
        }

        public void Clear() => Pool.Clear(Destroy);
    }
}
