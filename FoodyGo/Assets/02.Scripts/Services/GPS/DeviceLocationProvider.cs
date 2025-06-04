using System;
using UnityEngine;

namespace FoodyGo.Services.GPS
{
    public class DeviceLocationProvider : MonoBehaviour, ILocationProvider
    {
        public double latitude => throw new NotImplementedException();

        public double longitude => throw new NotImplementedException();

        public double altitude => throw new NotImplementedException();

        public event Action<double, double, double, float, double> onLocationUpdated;

        public void StartService()
        {
            throw new NotImplementedException();
        }

        public void StopService()
        {
            throw new NotImplementedException();
        }
    }
}
