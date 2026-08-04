using UnityEngine;

namespace Zzinmak.Progression
{
    public class ExpDogSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject dogPrefab;
        [SerializeField] private float initialPickupRange = 2.5f;

        private ExpDog dog;

        public void Activate(float growthPercent)
        {
            if (dogPrefab == null) return;

            if (dog == null)
            {
                var dogObj = Instantiate(dogPrefab, transform.position, Quaternion.identity);
                dog = dogObj.GetComponent<ExpDog>();
                if (dog != null)
                {
                    dog.Initialize(transform, initialPickupRange);
                }
            }
            else
            {
                dog.IncreasePickupRangePercent(growthPercent);
            }
        }
    }
}
