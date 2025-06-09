using FoodyGo.Services.GoogleMaps;
using FoodyGo.Services.GPS;
using UnityEngine;

namespace FoodyGo.Mapping
{
    public class GoogleMapTile : MonoBehaviour
    {
        [Header("Map Settings")]
        [Tooltip("줌 레벨")]
        [Range(1, 20)]
        public int zoomLevel = 15;

        [Range(64, 1024)]
        [Tooltip("맵 텍스처 사이즈")]
        public int size = 640;

        [Tooltip("월드 맵 원점")]
        public MapLocation worldCenterLocation;

        [Header("Tile Settings")]
        [Tooltip("타일링을 위한 오프셋")]
        public Vector2Int tileOffset;

        [Tooltip("오프셋 적용한 맵의 중심 위치")]
        public MapLocation tileCenterLocation;

        [Header("Map Services")]
        public GoogleStaticMapService googleStaticMapService;

        [Header("GPS Services")]
        public GPSLocationService gpsLocationService;
       
        private Renderer _renderer;

        private void Awake()
        {
            _renderer = GetComponent<Renderer>();
        }

        public void RefreshMapTile()
        {
            //오프셋에 따른 중심위치 계산
            tileCenterLocation.latitude = GoogleMapUtils.adjustLatByPixels(worldCenterLocation.latitude, (int)(size * tileOffset.y), zoomLevel);
            tileCenterLocation.longitude = GoogleMapUtils.adjustLonByPixels(worldCenterLocation.longitude, (int)(size * tileOffset.x), zoomLevel);

            //맵 텍스처 요청
            googleStaticMapService.LoadMap(tileCenterLocation.latitude, tileCenterLocation.longitude, zoomLevel, new Vector2(size, size), OnMapLoaded);
        }

        private void OnMapLoaded(Texture2D texture)
        {
            if(_renderer.material.mainTexture != null)
            {
                Destroy(_renderer.material.mainTexture);
            }

            _renderer.material.mainTexture = texture;
        }
    }
}
