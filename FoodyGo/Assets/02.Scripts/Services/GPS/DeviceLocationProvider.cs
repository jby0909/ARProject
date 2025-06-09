using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Android;

namespace FoodyGo.Services.GPS
{
    public class DeviceLocationProvider : MonoBehaviour, ILocationProvider
    {
        public double latitude { get; private set; }

        public double longitude { get; private set; }

        public double altitude { get; private set; }

        public bool isRunning { get; private set; }

        public event Action<double, double, double, float, double> onLocationUpdated;

        private float _resendTime = 1.0f; // GPS 갱신 주기
        private float _desiredAccuracyInMeters = 5f;
        private float _updateAccuracyInMeters = 5f;
        private float _elapsedWaitTime = 0f; // GPS 초기화 타임아웃 경과시간
        private float _maxWaitTime = 10f; // GPS 초기화 타임아웃

        public void StartService()
        {
            StartCoroutine(C_RefreshGPSData());
        }

        public void StopService()
        {
            isRunning = false;
            StopAllCoroutines();
        }

        IEnumerator C_RefreshGPSData()
        {
            // 위치 정보 접근 권한 확인 후 요청
            if(Permission.HasUserAuthorizedPermission(Permission.FineLocation) == false)
            {
                Permission.RequestUserPermission(Permission.FineLocation);

                //요청 이후 권한 생길 때까지 대기
                yield return new WaitUntil(() => Permission.HasUserAuthorizedPermission(Permission.FineLocation));
            }

            if(Input.location.isEnabledByUser == false)
            {
                Debug.LogError("GPS 장치 꺼짐.");
                yield break;
            }

            Input.location.Start(_desiredAccuracyInMeters, _updateAccuracyInMeters);

            // 초기화 될 때까지 기다림
            while(Input.location.status == LocationServiceStatus.Initializing &&
                _elapsedWaitTime < _maxWaitTime)
            {
                yield return new WaitForSeconds(1.0f);
                _elapsedWaitTime += 1.0f;
            }

            if(Input.location.status == LocationServiceStatus.Failed)
            {
                // TODO : GPS 실행 실패시 예외처리
            }

            if(_elapsedWaitTime >= _maxWaitTime)
            {
                // TODO : 타임아웃 에러 띄우기
            }

            LocationInfo locationInfo = Input.location.lastData;
            isRunning = true;

            while(true)
            {
                locationInfo = Input.location.lastData;
                latitude = locationInfo.latitude;
                longitude = locationInfo.longitude;
                altitude = locationInfo.altitude;
                onLocationUpdated?.Invoke(latitude, longitude, altitude, locationInfo.horizontalAccuracy, locationInfo.timestamp);
                yield return new WaitForSeconds(_resendTime);
            }
        }
    }
}
