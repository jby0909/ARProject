using UnityEngine;
using System;

namespace FoodyGo.Mapping
{
    /// <summary>
    /// GoogleMapUtils 클래스는 구글 맵의 Web Mercator 투영 방식을 구현한 유틸리티 메서드를 제공합니다.
    /// - 위도/경도(°) ↔ Mercator 픽셀 좌표(zoom=21 기준) 변환
    /// - Mercator 픽셀 좌표 ↔ 위도/경도 역변환
    /// - 주어진 픽셀 단위 오프셋만큼 위도/경도 를 이동시키는 메서드
    /// - 타일 이미지 픽셀 크기와 Unity Units 간의 배율을 계산하는 메서드
    /// 
    /// 이 클래스는 구글 Static Maps API 또는 자체 타일 서버와 함께 사용하여 
    /// 위도/경도를 머카토르 기반 픽셀 좌표로 변환하고, 필요한 배율을 산출하는 데 쓰입니다.
    /// </summary>
    public class GoogleMapUtils
    {
        // 구글 머카토르 픽셀 좌표계의 중심 오프셋 (2^28 / 2)
        static double GOOGLE_OFFSET = 268435456.0;

        // 머카토르 반지름 (GOOGLE_OFFSET / π)
        static double GOOGLE_OFFSET_RADIUS = 85445659.44705395;

        // 라디안 ↔ 도 변환 상수 (π / 180)
        static double MATHPI_180 = Math.PI / 180.0;

        // 경도(°)를 라디안으로 변환 후 픽셀 X 좌표로 곱해줄 상수 (GOOGLE_OFFSET_RADIUS * π/180)
        static private double preLonToX1 = GOOGLE_OFFSET_RADIUS * (Math.PI / 180.0);


        /// <summary>
        /// 경도(lon, °)를 “zoom = 21” 머카토르 픽셀 X 좌표(정수)로 변환합니다.
        /// 픽셀 중심을 GOOGLE_OFFSET로 한 뒤, 경도(rad) × 반지름을 더한 값을 반올림하여 반환합니다.
        /// </summary>
        public static int LonToX(double lon)
        {
            return (int)Math.Round(GOOGLE_OFFSET + preLonToX1 * lon);
        }

        /// <summary>
        /// 위도(lat, °)를 “zoom = 21” 머카토르 픽셀 Y 좌표(정수)로 변환합니다.
        /// 공식을 사용하여 y = OFFSET ? R * ln[ tan(π/4 + lat(rad)/2 ) ] 값을 반올림해 반환합니다.
        /// </summary>
        public static int LatToY(double lat)
        {
            double latRad = lat * MATHPI_180;
            double mercN = Math.Log((1.0 + Math.Sin(latRad)) / (1.0 - Math.Sin(latRad))) / 2.0;
            return (int)Math.Round(GOOGLE_OFFSET - GOOGLE_OFFSET_RADIUS * mercN);
        }

        /// <summary>
        /// “zoom = 21” 머카토르 픽셀 X 좌표를 경도(lon, °)로 역변환합니다.
        /// (x ? OFFSET) / 반지름 = lon(rad), → °로 변환해 반환합니다.
        /// </summary>
        public static double XToLon(double x)
        {
            return ((Math.Round(x) - GOOGLE_OFFSET) / GOOGLE_OFFSET_RADIUS) * 180.0 / Math.PI;
        }

        /// <summary>
        /// “zoom = 21” 머카토르 픽셀 Y 좌표를 위도(lat, °)로 역변환합니다.
        /// lat(rad) = π/2 ? 2 * atan(exp((y ? OFFSET)/R)) 공식을 사용해 라디안 위도를 계산한 후, °로 변환해 반환합니다.
        /// </summary>
        public static double YToLat(double y)
        {
            double x = (Math.Round(y) - GOOGLE_OFFSET) / GOOGLE_OFFSET_RADIUS;
            double latRad = (Math.PI / 2.0) - 2.0 * Math.Atan(Math.Exp(x));
            return latRad * 180.0 / Math.PI;
        }

        /// <summary>
        /// 주어진 경도(lon, °)를 픽셀 단위(delta)만큼 좌우로 이동한 뒤, 이동된 픽셀 값을 역변환해 새로운 경도(°)를 반환합니다.
        /// 내부적으로 LonToX로 픽셀 X 계산 → delta << (21 ? zoom) 연산 → XToLon 역변환을 수행합니다.
        /// </summary>
        public static double adjustLonByPixels(double lon, int delta, int zoom)
        {
            int px21 = LonToX(lon);
            int movedPx21 = px21 + (delta << (21 - zoom));
            return XToLon(movedPx21);
        }

        /// <summary>
        /// 주어진 위도(lat, °)를 픽셀 단위(delta)만큼 상하로 이동한 뒤, 이동된 픽셀 값을 역변환해 새로운 위도(°)를 반환합니다.
        /// 내부적으로 LatToY로 픽셀 Y 계산 → delta << (21 ? zoom) 연산 → YToLat 역변환을 수행합니다.
        /// </summary>
        public static double adjustLatByPixels(double lat, int delta, int zoom)
        {
            int py21 = LatToY(lat);
            int movedPy21 = py21 + (delta << (21 - zoom));
            return YToLat(movedPy21);
        }

        /// <summary>
        /// 주어진 위도(lat, °), 타일 이미지 픽셀 크기(tileSizePixels), 
        /// 그리고 “타일을 Unity Units로 보여줄 때의 크기(tileSizeUnits)” 정보를 바탕으로,
        /// 경도 방향(가로) 배율(Units / 1px)을 계산하여 반환합니다.
        /// 1) LatToY(lat) → y0
        /// 2) adjustLatByPixels(lat, tileSizePixels, zoom) → 오프셋 위도
        /// 3) LatToY(offset) → y1
        /// 4) pixelRange = y1 ? y0
        /// 5) tileSizeUnits / pixelRange = 1픽셀당 Unity Units → 반환
        /// </summary>
        public static double CalculateScaleX(double lat, int tileSizePixels, int tileSizeUnits, int zoom)
        {
            double latOffset = adjustLatByPixels(lat, tileSizePixels, zoom);
            int y0 = LatToY(lat);
            int y1 = LatToY(latOffset);
            double pixelRange = y1 - y0;
            return tileSizeUnits / pixelRange;
        }

        /// <summary>
        /// 주어진 경도(lon, °), 타일 이미지 픽셀 크기(tileSizePixels), 
        /// 그리고 “타일을 Unity Units로 보여줄 때의 크기(tileSizeUnits)” 정보를 바탕으로,
        /// 위도 방향(세로) 배율(Units / 1px)을 계산하여 반환합니다.
        /// 1) LonToX(lon) → x0
        /// 2) adjustLonByPixels(lon, tileSizePixels, zoom) → 오프셋 경도
        /// 3) LonToX(offset) → x1
        /// 4) pixelRange = x1 ? x0
        /// 5) tileSizeUnits / pixelRange = 1픽셀당 Unity Units → 반환
        /// </summary>
        public static double CalculateScaleY(double lon, int tileSizePixels, int tileSizeUnits, int zoom)
        {
            double lonOffset = adjustLonByPixels(lon, tileSizePixels, zoom);
            int x0 = LonToX(lon);
            int x1 = LonToX(lonOffset);
            double pixelRange = x1 - x0;
            return tileSizeUnits / pixelRange;
        }

        // Vector2 uv = new Vector2(
        //     (float)myMarker.pixelCoords.x / (float)renderer.material.mainTexture.width,
        //     1f - (float)myMarker.pixelCoords.y / (float)renderer.material.mainTexture.height
        // );
    }
}
