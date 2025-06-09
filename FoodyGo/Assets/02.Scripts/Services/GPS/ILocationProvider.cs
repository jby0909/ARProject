using System;
using UnityEngine;

namespace FoodyGo.Services.GPS
{
    public interface ILocationProvider
    {
        /// <summary>
        /// 동작중
        /// </summary>
        bool isRunning { get; }

        /// <summary>
        /// 위도
        /// </summary>
        double latitude { get; }

        /// <summary>
        /// 경도
        /// </summary>
        double longitude { get; }

        /// <summary>
        /// 고도
        /// </summary>
        double altitude { get; }

        /// <summary>
        /// latitude, longtitude, altitude, horizontalAccuracy, timestamp
        /// </summary>
        event Action<double, double, double, float, double> onLocationUpdated;

        /// <summary>
        /// 위치 데이터 갱신 시작
        /// </summary>
        void StartService();

        /// <summary>
        /// 위치 데이터 갱신 종료
        /// </summary>
        void StopService();
    }
}
