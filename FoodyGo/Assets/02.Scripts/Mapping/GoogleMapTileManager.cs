using FoodyGo.Services.GoogleMaps;
using FoodyGo.Services.GPS;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

namespace FoodyGo.Mapping
{
    /// <summary>
    /// MapTile 생성, 갱신, 제거 등의 관리
    /// GPS 데이터가 범위를 벗어날 때 타일맵 확장및 반대방향 타일맵 삭제
    /// </summary>
    public class GoogleMapTileManager : MonoBehaviour
    {
        public bool isInitialized { get; private set; }

        [Header("Configuration")]
        [SerializeField] GoogleStaticMapService _googleStaticMapService;
        [SerializeField] GPSLocationService _gpsLocationService;
        [SerializeField] GoogleMapTile _mapTilePrefab;
        [SerializeField] Transform _mapTilesParent;

        [Header("Debug")]
        Vector2Int _currentCenterTileCoord;

        [Header("Managed mapTiles")]
        GoogleMapTile[,] _mapTiles = new GoogleMapTile[3, 3];
        readonly int[] TILE_OFFSETS = { -1, 0, 1 };

        IEnumerator Start()
        {
            yield return new WaitUntil(() => _gpsLocationService.isReady);
            InitializeTiles();
            isInitialized = true;
        }

        /// <summary>
        /// 현재 GPS 기반으로 중심 타일 인덱스 계산
        /// 3x3 배열로 MapTile들 생성
        /// </summary>
        void InitializeTiles()
        {
            _currentCenterTileCoord = CalcTileCoordinate(_gpsLocationService.mapCenter);
            CreateTiles(_currentCenterTileCoord);
        }

        void CreateTiles(Vector2Int center)
        {
            // 중심 인덱스 기준으로 모든 방향 타일들 인덱스 계산
            for(int i = 0; i < TILE_OFFSETS.Length; i++)
            {
                for(int j = 0; j < TILE_OFFSETS.Length; j++)
                {
                    Vector2Int coord = new Vector2Int(center.x + TILE_OFFSETS[i], center.y + TILE_OFFSETS[j]);

                    GoogleMapTile tile = Instantiate(_mapTilePrefab, _mapTilesParent);
                    tile.tileOffset = new Vector2Int(i - 1, j - 1);
                    tile.googleStaticMapService = _googleStaticMapService;
                    tile.zoomLevel = _gpsLocationService.mapTileZoomLevel;
                    tile.gpsLocationService = _gpsLocationService;
                    tile.name = $"MapTile_{coord.x}_{coord.y}";
                    tile.transform.position = CalcWorldPosition(coord);
                    tile.RefreshMapTile();
                    _mapTiles[i, j] = tile;
                }
            }
        }

        public Vector3 GetCenterTileWorldPosition()
        {
            return CalcWorldPosition(_currentCenterTileCoord);
        }

        /// <summary>
        /// 타일 인덱스로 게임 월드 포지션 산출
        /// </summary>
        /// <param name="coord">타일 인덱스</param>
        /// <returns>월드 위치</returns>
        Vector3 CalcWorldPosition(Vector2Int coord)
        {
            float spacing = 10f; 
            return new Vector3(-coord.x * spacing, 0f, coord.y * spacing);
        }

        /// <summary>
        /// 특정 위도, 경도에 해당하는 MapTile의 인덱스를 계산 
        /// </summary>
        /// <param name="center">MapTile을 그릴 위도경도 중심</param>
        /// <returns>MapTile 인덱스</returns>
        Vector2Int CalcTileCoordinate(MapLocation center)
        {
            // 메르카토르 픽셀 좌표(zoom = 21)
            int pixelX21 = GoogleMapUtils.LonToX(center.longitude);
            int pixelY21 = GoogleMapUtils.LatToY(center.latitude);

            // GoogleMap zoomlevel 1 당 2배씩 값이 작아지기 때문에 (공식문서 참조)
            // ZoomLevel 차이만큼 오른쪽으로 Bit-Shift 하면 원하는 픽셀 값을 구할 수 있다.
            int shift = 21 - _gpsLocationService.mapTileZoomLevel;
            int pixelX = pixelX21 >> shift;
            int pixelY = pixelY21 >> shift;

            // MapTile 당 픽셀수로 나누면 인덱스 구할 수 있음
            return new Vector2Int(Mathf.RoundToInt(pixelX / (float)_gpsLocationService.mapTileSizePixels), 
                                  Mathf.RoundToInt(pixelY / (float)_gpsLocationService.mapTileSizePixels));
        }
    }
}
