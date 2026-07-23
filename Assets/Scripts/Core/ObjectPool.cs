using System.Collections.Generic;
using UnityEngine;

namespace Zzinmak.Core
{
    public class ObjectPool : MonoBehaviour
    {
        public static ObjectPool Instance { get; private set; }

        private readonly Dictionary<GameObject, Queue<GameObject>> pools = new Dictionary<GameObject, Queue<GameObject>>();
        private readonly Dictionary<GameObject, GameObject> instanceSourcePrefab = new Dictionary<GameObject, GameObject>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        public GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            if (!pools.TryGetValue(prefab, out var queue))
            {
                queue = new Queue<GameObject>();
                pools[prefab] = queue;
            }

            GameObject instance;
            if (queue.Count > 0)
            {
                instance = queue.Dequeue();
                instance.transform.SetPositionAndRotation(position, rotation);
                instance.SetActive(true);
            }
            else
            {
                instance = Instantiate(prefab, position, rotation, transform);
                instanceSourcePrefab[instance] = prefab;
            }

            return instance;
        }

        public void Return(GameObject instance)
        {
            instance.SetActive(false);

            if (instanceSourcePrefab.TryGetValue(instance, out var prefab))
            {
                if (!pools.TryGetValue(prefab, out var queue))
                {
                    queue = new Queue<GameObject>();
                    pools[prefab] = queue;
                }

                queue.Enqueue(instance);
            }
            else
            {
                Destroy(instance);
            }
        }

        public void Prewarm(GameObject prefab, int count)
        {
            var temp = new List<GameObject>(count);
            for (int i = 0; i < count; i++)
            {
                temp.Add(Get(prefab, Vector3.zero, Quaternion.identity));
            }

            foreach (var obj in temp)
            {
                Return(obj);
            }
        }
    }
}
