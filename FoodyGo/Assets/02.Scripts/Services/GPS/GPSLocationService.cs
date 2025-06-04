using System;
using UnityEngine;

namespace FoodyGo.Services.GPS
{
    public class GPSLocationService : MonoBehaviour
    {
        private ILocationProvider _locationProvider;

        [Header("Simulation Settings (Editor Only)")]
        [SerializeField] bool _isSimulation;
        [SerializeField] Transform _simulationTarget;
        [SerializeField] MapLocation _simulationStartLocation = new MapLocation(37.4946, 127.0276056); // 시작위치 : 비트컴퓨터학원 으로 지정

        public double latitude { get; private set; }
        public double longitude { get; private set; }
        public double altitude { get; private set; }
        public float accuracy { get; private set; }
        public double timeStamp { get; private set; }

        public event Action onLocationUpdated;

        private void Awake()
        {
#if UNITY_EDITOR
            SimulatedLocationProvider simulatedLocationProvider = gameObject.AddComponent<SimulatedLocationProvider>();
            simulatedLocationProvider.target = _simulationTarget;
            simulatedLocationProvider.startLocation = _simulationStartLocation;
            _locationProvider = simulatedLocationProvider;
#else
            _locationProvider = gameObject.AddComponent<DeviceLocationProvider>();
#endif
        }

        private void OnEnable()
        {
            _locationProvider.onLocationUpdated += OnLocationUpdated;
            _locationProvider.StartService();
        }

        private void OnDisable()
        {
            _locationProvider.onLocationUpdated -= OnLocationUpdated;
            _locationProvider.StopService();
        }

        private void OnLocationUpdated(double newLatitude, double newLongitude, double newAltitude, float newAccuracy, double newTimeStamp)
        {
            latitude = newLatitude; 
            longitude = newLongitude;
            altitude = newAltitude;
            accuracy = newAccuracy;
            timeStamp = newTimeStamp;
        }
    }
}
