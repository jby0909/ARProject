using UnityEngine;
using System;

namespace FoodyGo.Mapping
{
    /// <summary>
    /// GoogleMapUtils Ŭ������ ���� ���� Web Mercator ���� ����� ������ ��ƿ��Ƽ �޼��带 �����մϴ�.
    /// - ����/�浵(��) �� Mercator �ȼ� ��ǥ(zoom=21 ����) ��ȯ
    /// - Mercator �ȼ� ��ǥ �� ����/�浵 ����ȯ
    /// - �־��� �ȼ� ���� �����¸�ŭ ����/�浵 �� �̵���Ű�� �޼���
    /// - Ÿ�� �̹��� �ȼ� ũ��� Unity Units ���� ������ ����ϴ� �޼���
    /// 
    /// �� Ŭ������ ���� Static Maps API �Ǵ� ��ü Ÿ�� ������ �Բ� ����Ͽ� 
    /// ����/�浵�� ��ī�丣 ��� �ȼ� ��ǥ�� ��ȯ�ϰ�, �ʿ��� ������ �����ϴ� �� ���Դϴ�.
    /// </summary>
    public class GoogleMapUtils
    {
        // ���� ��ī�丣 �ȼ� ��ǥ���� �߽� ������ (2^28 / 2)
        static double GOOGLE_OFFSET = 268435456.0;

        // ��ī�丣 ������ (GOOGLE_OFFSET / ��)
        static double GOOGLE_OFFSET_RADIUS = 85445659.44705395;

        // ���� �� �� ��ȯ ��� (�� / 180)
        static double MATHPI_180 = Math.PI / 180.0;

        // �浵(��)�� �������� ��ȯ �� �ȼ� X ��ǥ�� ������ ��� (GOOGLE_OFFSET_RADIUS * ��/180)
        static private double preLonToX1 = GOOGLE_OFFSET_RADIUS * (Math.PI / 180.0);


        /// <summary>
        /// �浵(lon, ��)�� ��zoom = 21�� ��ī�丣 �ȼ� X ��ǥ(����)�� ��ȯ�մϴ�.
        /// �ȼ� �߽��� GOOGLE_OFFSET�� �� ��, �浵(rad) �� �������� ���� ���� �ݿø��Ͽ� ��ȯ�մϴ�.
        /// </summary>
        public static int LonToX(double lon)
        {
            return (int)Math.Round(GOOGLE_OFFSET + preLonToX1 * lon);
        }

        /// <summary>
        /// ����(lat, ��)�� ��zoom = 21�� ��ī�丣 �ȼ� Y ��ǥ(����)�� ��ȯ�մϴ�.
        /// ������ ����Ͽ� y = OFFSET ? R * ln[ tan(��/4 + lat(rad)/2 ) ] ���� �ݿø��� ��ȯ�մϴ�.
        /// </summary>
        public static int LatToY(double lat)
        {
            double latRad = lat * MATHPI_180;
            double mercN = Math.Log((1.0 + Math.Sin(latRad)) / (1.0 - Math.Sin(latRad))) / 2.0;
            return (int)Math.Round(GOOGLE_OFFSET - GOOGLE_OFFSET_RADIUS * mercN);
        }

        /// <summary>
        /// ��zoom = 21�� ��ī�丣 �ȼ� X ��ǥ�� �浵(lon, ��)�� ����ȯ�մϴ�.
        /// (x ? OFFSET) / ������ = lon(rad), �� �Ʒ� ��ȯ�� ��ȯ�մϴ�.
        /// </summary>
        public static double XToLon(double x)
        {
            return ((Math.Round(x) - GOOGLE_OFFSET) / GOOGLE_OFFSET_RADIUS) * 180.0 / Math.PI;
        }

        /// <summary>
        /// ��zoom = 21�� ��ī�丣 �ȼ� Y ��ǥ�� ����(lat, ��)�� ����ȯ�մϴ�.
        /// lat(rad) = ��/2 ? 2 * atan(exp((y ? OFFSET)/R)) ������ ����� ���� ������ ����� ��, �Ʒ� ��ȯ�� ��ȯ�մϴ�.
        /// </summary>
        public static double YToLat(double y)
        {
            double x = (Math.Round(y) - GOOGLE_OFFSET) / GOOGLE_OFFSET_RADIUS;
            double latRad = (Math.PI / 2.0) - 2.0 * Math.Atan(Math.Exp(x));
            return latRad * 180.0 / Math.PI;
        }

        /// <summary>
        /// �־��� �浵(lon, ��)�� �ȼ� ����(delta)��ŭ �¿�� �̵��� ��, �̵��� �ȼ� ���� ����ȯ�� ���ο� �浵(��)�� ��ȯ�մϴ�.
        /// ���������� LonToX�� �ȼ� X ��� �� delta << (21 ? zoom) ���� �� XToLon ����ȯ�� �����մϴ�.
        /// </summary>
        public static double adjustLonByPixels(double lon, int delta, int zoom)
        {
            int px21 = LonToX(lon);
            int movedPx21 = px21 + (delta << (21 - zoom));
            return XToLon(movedPx21);
        }

        /// <summary>
        /// �־��� ����(lat, ��)�� �ȼ� ����(delta)��ŭ ���Ϸ� �̵��� ��, �̵��� �ȼ� ���� ����ȯ�� ���ο� ����(��)�� ��ȯ�մϴ�.
        /// ���������� LatToY�� �ȼ� Y ��� �� delta << (21 ? zoom) ���� �� YToLat ����ȯ�� �����մϴ�.
        /// </summary>
        public static double adjustLatByPixels(double lat, int delta, int zoom)
        {
            int py21 = LatToY(lat);
            int movedPy21 = py21 + (delta << (21 - zoom));
            return YToLat(movedPy21);
        }

        /// <summary>
        /// �־��� ����(lat, ��), Ÿ�� �̹��� �ȼ� ũ��(tileSizePixels), 
        /// �׸��� ��Ÿ���� Unity Units�� ������ ���� ũ��(tileSizeUnits)�� ������ ��������,
        /// �浵 ����(����) ����(Units / 1px)�� ����Ͽ� ��ȯ�մϴ�.
        /// 1) LatToY(lat) �� y0
        /// 2) adjustLatByPixels(lat, tileSizePixels, zoom) �� ������ ����
        /// 3) LatToY(offset) �� y1
        /// 4) pixelRange = y1 ? y0
        /// 5) tileSizeUnits / pixelRange = 1�ȼ��� Unity Units �� ��ȯ
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
        /// �־��� �浵(lon, ��), Ÿ�� �̹��� �ȼ� ũ��(tileSizePixels), 
        /// �׸��� ��Ÿ���� Unity Units�� ������ ���� ũ��(tileSizeUnits)�� ������ ��������,
        /// ���� ����(����) ����(Units / 1px)�� ����Ͽ� ��ȯ�մϴ�.
        /// 1) LonToX(lon) �� x0
        /// 2) adjustLonByPixels(lon, tileSizePixels, zoom) �� ������ �浵
        /// 3) LonToX(offset) �� x1
        /// 4) pixelRange = x1 ? x0
        /// 5) tileSizeUnits / pixelRange = 1�ȼ��� Unity Units �� ��ȯ
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
