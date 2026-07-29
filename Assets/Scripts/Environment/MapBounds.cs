using UnityEngine;

namespace Zzinmak.Environment
{
    public class MapBounds : MonoBehaviour
    {
        [SerializeField] private BoxCollider2D northWall;
        [SerializeField] private BoxCollider2D southWall;
        [SerializeField] private BoxCollider2D eastWall;
        [SerializeField] private BoxCollider2D westWall;
        [SerializeField] private float margin = 0.5f;

        public static MapBounds Instance { get; private set; }

        private Rect interior;

        private void Awake()
        {
            Instance = this;
            RecomputeBounds();
        }

        public void RecomputeBounds()
        {
            float minX = westWall != null ? westWall.bounds.max.x : float.NegativeInfinity;
            float maxX = eastWall != null ? eastWall.bounds.min.x : float.PositiveInfinity;
            float minY = southWall != null ? southWall.bounds.max.y : float.NegativeInfinity;
            float maxY = northWall != null ? northWall.bounds.min.y : float.PositiveInfinity;

            minX += margin;
            maxX -= margin;
            minY += margin;
            maxY -= margin;

            if (minX > maxX)
            {
                float mid = (minX + maxX) * 0.5f;
                minX = maxX = mid;
            }
            if (minY > maxY)
            {
                float mid = (minY + maxY) * 0.5f;
                minY = maxY = mid;
            }

            interior = Rect.MinMaxRect(minX, minY, maxX, maxY);
        }

        public Vector2 ClampToInterior(Vector2 position)
        {
            return new Vector2(
                Mathf.Clamp(position.x, interior.xMin, interior.xMax),
                Mathf.Clamp(position.y, interior.yMin, interior.yMax));
        }

        public Vector2 RandomPointInInterior()
        {
            return new Vector2(
                Random.Range(interior.xMin, interior.xMax),
                Random.Range(interior.yMin, interior.yMax));
        }
    }
}
