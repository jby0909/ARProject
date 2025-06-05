using UnityEngine;

namespace FoodyGo.Mapping
{
    /// <summary>
    /// 맵의 경계
    /// </summary>
    public struct MapEnvelope
    {
        private float lon1;
        private float lon2;
        private float lat1;
        private float lat2;

        public MapEnvelope(float lon1, float lat1, float lon2, float lat2)
        {
            this.lon1 = lon1;
            this.lon2 = lon2;
            this.lat1 = lat1;
            this.lat2 = lat2;
        }

        /// <summary>
        /// 특정 위치가 현재 맵 범위 내에 있는지 여부 확인
        /// </summary>
        /// <param name="loc"> 특정 위치 </param>
        /// <returns> true : 맵 범위 내에 위치함 </returns>
        public bool Contains(MapLocation loc)
        {
            var xMin = Mathf.Min(lon1, lon2);
            var xMax = Mathf.Max(lon1, lon2);
            var yMin = Mathf.Min(lat1, lat2);
            var yMax = Mathf.Max(lat1, lat2);

            if ((loc.longitude >= xMin) &&
                (loc.longitude <= xMax) &&
                (loc.latitude >= yMin) &&
                (loc.latitude <= yMax)) return true;
            else return false;
        }
    }
}
