using FoodyGo.Controllers;
using FoodyGo.Mapping;
using FoodyGo.Services;
using FoodyGo.Services.GPS;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static FoodyGo.Services.GooglePlacesService;

namespace FoodyGo.Managers
{
    public class PlacesMarkersManager : MonoBehaviour
    {
        [SerializeField] PlaceMarkerController _markerPrefab;
        [SerializeField] GooglePlacesService _googlePlacesService;
        [SerializeField] GPSLocationService _gpsLocationService;
        [SerializeField] int _markerCount = 10;
        List<PlaceMarkerController> _markers;

        private void Awake()
        {
            _markers = new List<PlaceMarkerController>(_markerCount);
        }

        IEnumerator Start()
        {
            yield return new WaitUntil(() => _gpsLocationService.isReady);
            RefreshMarkers();
        }

        public void RefreshMarkers()
        {
            _googlePlacesService.SerachNearbyRequest(_gpsLocationService.latitude,
                                                        _gpsLocationService.longitude,
                                                        200,
                                                        new List<string> { "restaurant" },
                                                        null,
                                                        null,
                                                        null,
                                                        10,
                                                        "DISTANCE",
                                                        "ko",
                                                        "KR",
                                                        PlacesFields.DisplayName | PlacesFields.Types | PlacesFields.FormattedAddress | PlacesFields.Location,
                                                        RespawnMarkers
                                                     );
        }

        void RespawnMarkers(IEnumerable<(string name, double latitude, double longtitude)> places)
        {
            for(int i = _markers.Count - 1; i >= 0; i--)
            {
                Destroy(_markers[i]);
                _markers.RemoveAt(i);
            }

            foreach (var place in places)
            {
                PlaceMarkerController marker = Instantiate(_markerPrefab);
                marker.RefreshPlace(place.name);
                marker.transform.position = new Vector3(GoogleMapUtils.LonToUnityX(place.longtitude, _gpsLocationService.mapOrigin.longitude,
                    _gpsLocationService.mapTileZoomLevel), 0f, GoogleMapUtils.LatToUnityY(place.latitude,
                    _gpsLocationService.mapOrigin.latitude, _gpsLocationService.mapTileZoomLevel));
                _markers.Add(marker);
            }
        }
    }
}

