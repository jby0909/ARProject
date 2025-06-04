using System;

namespace FoodyGo.Services.GPS
{
    /// <summary>
    /// 맵 위치 데이터
    /// </summary>
    [Serializable]
    public struct MapLocation
    {
        public MapLocation(double latitude, double longtitude)
        {
            this.latitude = latitude;
            this.longitude = longtitude;
        }

        public double latitude; // 위도
        public double longitude; // 경도
    }
}
