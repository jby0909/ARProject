using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Flexible chunk‐based “UI Tile Grid” streamer using UGUI, with pooling and scale‐in/scale‐out animations.
/// 
/// • You can set gridWidth×gridHeight freely (ideally odd numbers, so there’s a clear “center” tile).  
/// • tileSize determines the pixel size of each tile.  
/// • animDuration controls how long it takes for a recycled tile to scale‐out (1→0) and then scale‐in (0→1).
/// 
/// 동작 원리:
/// 1. gridWidth×gridHeight 만큼의 Image 프리팹을 화면 전체를 덮는 container(RectTransform) 아래에 미리 인스턴스화합니다.
/// 2. 마우스의 현재 “타일 좌표” (마우스 픽셀 위치 ÷ tileSize 정수부) 가 항상 배열의 [centerIndexX, centerIndexY] 위치에 있도록 유지합니다.
/// 3. Update()에서 마우스가 한 타일만큼 움직이면( dx 또는 dy == ±1 ), 
///    반대쪽 행(row) 또는 열(column) 전체를 재활용(pool)하고:
///      • 재활용할 타일들을 먼저 기존 위치에서 Scale 1→0으로 사라지게 하고,  
///      • Scale이 0이 되면 위치(anchoredPosition)를 새로 들어올 위치로 스냅한 뒤,  
///      • 그 위치에서 Scale 0→1로 나타나게 합니다.  
///    이 과정을 통해 매번 객체를 생성/파괴하지 않고 기존 풀을 재활용합니다.
/// 4. 만약 마우스가 한 번에 두 칸 이상(tileSize 두 배) 이동하면( dx 또는 dy 절댓값 > 1 ),  
///    배열 전체를 RepositionAllTiles()으로 리셋합니다.
/// 
/// 사용 방법:
/// 1. Canvas를 “Screen Space – Overlay” 모드로 만든 뒤, 그 아래에 빈 GameObject 하나를 만들고 RectTransform만 남깁니다.
///    Inspector에서 Anchor Min=(0,0), Anchor Max=(0,0), Pivot=(0,0), Anchored Position=(0,0)로 설정하세요.
/// 2. 해당 RectTransform을 이 스크립트의 “container” 필드에 연결합니다.
/// 3. UI Image가 붙은 프리팹(tilePrefab) 하나를 준비합니다.
///    - Image 컴포넌트(어떤 스프라이트든 상관없음)  
///    - RectTransform: Anchor Min=(0,0), Anchor Max=(0,0), Pivot=(0.5,0.5)  
///    - 스크립트가 실행되면 sizeDelta와 localScale을 덮어씁니다.
/// 4. 빈 GameObject(예: “ChunkStreamerUGUI”)를 만들고, 이 스크립트를 붙입니다.
///    • Inspector에서 tilePrefab에 프리팹을 드래그  
///    • container에 풀스크린 RectTransform을 드래그  
///    • gridWidth, gridHeight (가로×세로 타일 개수), tileSize(픽셀), animDuration(애니메이션 시간)을 설정  
/// 5. Play 모드에서 마우스를 움직이면, 항상 마우스가 위치한 타일이 배열의 가운데( centerIndexX, centerIndexY )가 됩니다.
///    • 타일들은 필요에 따라 재활용되며, 사라질 때와 나타날 때 스케일 풀링 애니메이션을 가집니다.
/// 
/// 주의:
/// - gridWidth나 gridHeight가 0 또는 1로 설정되어도 동작하지만, 적당히 3 이상(특히 홀수)로 설정하는 걸 권장합니다.
/// - animDuration을 너무 짧게 설정하면 애니메이션이 보이지 않을 수 있고, 너무 길게 하면 반응이 둔해집니다.
/// </summary>
public class UI_ChunkStreamer : MonoBehaviour
{
    [Header("References")]
    [Tooltip("UI Image 프리팹 (RectTransform: AnchorMin/Max=(0,0), Pivot=(0.5,0.5)")]
    public GameObject tilePrefab;

    [Tooltip("화면 전체를 덮는 RectTransform (AnchorMin/Max=(0,0), Pivot=(0,0))")]
    public RectTransform container;

    [Header("Grid Settings")]
    [Tooltip("가로 타일 개수 (홀수를 권장)")]
    public int gridWidth = 3;

    [Tooltip("세로 타일 개수 (홀수를 권장)")]
    public int gridHeight = 3;

    [Tooltip("각 타일의 한 변 크기 (픽셀 단위)")]
    public int tileSize = 100;

    [Header("Animation Settings")]
    [Tooltip("재활용되는 타일이 사라졌다가 나타나는 전체(Scale‐out + Scale‐in) 시간 (초)")]
    public float animDuration = 0.3f;

    // 내부 풀링용 배열
    private GameObject[,] tiles;
    private RectTransform[,] tileRTs;

    // 현재 “타일 좌표” (마우스 위치 기반)
    private int centerTileX;
    private int centerTileY;

    // 배열 상에서의 중심 인덱스
    private int centerIndexX;
    private int centerIndexY;

    private void Start()
    {
        // 최소 1 이상
        gridWidth = Mathf.Max(1, gridWidth);
        gridHeight = Mathf.Max(1, gridHeight);

        // 예: gridWidth=5 → centerIndexX = 2 (0,1,[2],3,4)
        centerIndexX = gridWidth / 2;
        centerIndexY = gridHeight / 2;

        // container가 화면 전체를 덮도록 세팅
        container.anchorMin = Vector2.zero;
        container.anchorMax = Vector2.zero;
        container.pivot = Vector2.zero;
        container.anchoredPosition = Vector2.zero;
        container.sizeDelta = new Vector2(Screen.width, Screen.height);

        // 풀링 배열 초기화
        tiles = new GameObject[gridWidth, gridHeight];
        tileRTs = new RectTransform[gridWidth, gridHeight];

        // gridWidth × gridHeight 만큼 프리팹 인스턴스화
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                GameObject go = Instantiate(tilePrefab, container);
                go.name = $"Tile_{x}_{y}";
                RectTransform rt = go.GetComponent<RectTransform>();

                // Anchor/Pivot 강제 세팅 (픽셀 단위 위치 지정용)
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.zero;
                rt.pivot = new Vector2(0.5f, 0.5f);

                // 크기를 tileSize × tileSize로 지정
                rt.sizeDelta = new Vector2(tileSize, tileSize);

                // 초기 스케일은 1
                rt.localScale = Vector3.one;

                tiles[x, y] = go;
                tileRTs[x, y] = rt;
            }
        }

        // 초기 중심 타일 좌표를 마우스 픽셀 위치 기준으로 계산
        Vector2 m = Input.mousePosition;
        centerTileX = Mathf.FloorToInt(m.x / tileSize);
        centerTileY = Mathf.FloorToInt(m.y / tileSize);

        // 처음에 모든 타일을 올바른 위치(화면 기준)로 배치
        RepositionAllTiles();
    }

    private void Update()
    {
        Vector2 m = Input.mousePosition;
        int newTileX = Mathf.FloorToInt(m.x / tileSize);
        int newTileY = Mathf.FloorToInt(m.y / tileSize);

        int dx = newTileX - centerTileX;
        int dy = newTileY - centerTileY;

        // 여전히 같은 타일 내라면 아무것도 하지 않음
        if (dx == 0 && dy == 0) return;

        // 한 번에 두 칸 이상 이동했다면, 풀 리셋
        if (Mathf.Abs(dx) > 1 || Mathf.Abs(dy) > 1)
        {
            centerTileX = newTileX;
            centerTileY = newTileY;
            RepositionAllTiles();
            return;
        }

        // X 축으로 1칸 이동: 왼쪽(-1) 또는 오른쪽(+1)
        if (dx == +1)
        {
            // 마우스가 오른쪽으로 한 칸 넘어감
            ShiftHorizontal(+1);
        }
        else if (dx == -1)
        {
            // 마우스가 왼쪽으로 한 칸 넘어감
            ShiftHorizontal(-1);
        }

        // Y 축으로 1칸 이동: 아래(-1) 또는 위(+1)
        if (dy == +1)
        {
            // 마우스가 위쪽으로 한 칸 넘어감
            ShiftVertical(+1);
        }
        else if (dy == -1)
        {
            // 마우스가 아래쪽으로 한 칸 넘어감
            ShiftVertical(-1);
        }
    }

    /// <summary>
    /// 배열 전체를, “현재 centerTileX/Y 기준”으로 재설정합니다.  
    /// 모든 타일을 올바른 anchoredPosition(픽셀 좌표)으로 배치하고 localScale을 1로 초기화합니다.
    /// </summary>
    private void RepositionAllTiles()
    {
        for (int yIdx = 0; yIdx < gridHeight; yIdx++)
        {
            for (int xIdx = 0; xIdx < gridWidth; xIdx++)
            {
                RectTransform rt = tileRTs[xIdx, yIdx];

                // 배열 내 (xIdx, yIdx)에 해당하는 “월드 타일 좌표” 계산
                int tileX = centerTileX + (xIdx - centerIndexX);
                int tileY = centerTileY + (yIdx - centerIndexY);

                // 월드 타일 좌표 → 화면 픽셀 좌표로 환산 (좌하단이 (0,0))
                float px = tileX * tileSize + (tileSize * 0.5f);
                float py = tileY * tileSize + (tileSize * 0.5f);

                rt.anchoredPosition = new Vector2(px, py);
                rt.localScale = Vector3.one;
            }
        }
    }

    /// <summary>
    /// X방향으로 한 칸 이동했을 때 호출됩니다.
    /// dir == +1 → 마우스가 오른쪽으로 넘어갔음. → 맨 왼쪽 열(oldX=0)을 재활용해서 맨 오른쪽(newXIdx=gridWidth-1)으로 옮김.
    /// dir == -1 → 마우스가 왼쪽으로 넘어갔음. → 맨 오른쪽 열(oldX=gridWidth-1)을 재활용해서 맨 왼쪽(newXIdx=0)으로 옮김.
    /// </summary>
    private void ShiftHorizontal(int dir)
    {
        // 재활용할 “이전 열” 인덱스
        int oldX = (dir == +1) ? 0 : (gridWidth - 1);
        // 재활용 타일을 옮길 “새 열” 인덱스
        int newXIdx = (dir == +1) ? (gridWidth - 1) : 0;

        // old 열의 모든 GameObject와 RectTransform을 임시 보관
        GameObject[] oldGOs = new GameObject[gridHeight];
        RectTransform[] oldRTs = new RectTransform[gridHeight];
        for (int y = 0; y < gridHeight; y++)
        {
            oldGOs[y] = tiles[oldX, y];
            oldRTs[y] = tileRTs[oldX, y];
        }

        // 중심 타일 좌표 업데이트 (X축)
        centerTileX += dir;

        // 배열 내 타일 참조만 한 칸씩 민다 (왼쪽 또는 오른쪽).
        if (dir == +1)
        {
            // 오른쪽으로 한 칸 이동했으므로, (1→0), (2→1), …, (gridWidth-1→gridWidth-2)
            for (int y = 0; y < gridHeight; y++)
            {
                for (int x = 0; x < gridWidth - 1; x++)
                {
                    tiles[x, y] = tiles[x + 1, y];
                    tileRTs[x, y] = tileRTs[x + 1, y];
                }
            }
        }
        else // dir == -1
        {
            // 왼쪽으로 한 칸 이동했으므로, (gridWidth-2→gridWidth-1), (gridWidth-3→gridWidth-2), …, (0→1)
            for (int y = 0; y < gridHeight; y++)
            {
                for (int x = gridWidth - 1; x > 0; x--)
                {
                    tiles[x, y] = tiles[x - 1, y];
                    tileRTs[x, y] = tileRTs[x - 1, y];
                }
            }
        }

        // 임시 보관해 둔 old 열을, 새 열(newXIdx) 위치에 넣는다.
        for (int y = 0; y < gridHeight; y++)
        {
            tiles[newXIdx, y] = oldGOs[y];
            tileRTs[newXIdx, y] = oldRTs[y];
        }

        // “남아있는” 타일들(재활용되지 않은 열 제외)에 대해서는 즉시 올바른 위치와 스케일(1)을 세팅한다.
        for (int yIdx = 0; yIdx < gridHeight; yIdx++)
        {
            for (int xIdx = 0; xIdx < gridWidth; xIdx++)
            {
                if (xIdx == newXIdx)
                    continue; // 재활용된 열은 애니메이션으로 처리할 예정이므로 일단 스킵

                RectTransform rt = tileRTs[xIdx, yIdx];
                rt.localScale = Vector3.one;

                int tileX = centerTileX + (xIdx - centerIndexX);
                int tileY = centerTileY + (yIdx - centerIndexY);
                float px = tileX * tileSize + (tileSize * 0.5f);
                float py = tileY * tileSize + (tileSize * 0.5f);
                rt.anchoredPosition = new Vector2(px, py);
            }
        }

        // 재활용된 열(oldX)의 타일들: 
        // 1) 기존 위치에서 Scale 1→0 애니메이션 → 
        // 2) 새 위치로 스냅 → 
        // 3) Scale 0→1 애니메이션
        float halfDur = animDuration * 0.5f;
        for (int yIdx = 0; yIdx < gridHeight; yIdx++)
        {
            RectTransform rt = oldRTs[yIdx];

            int tileXNew = centerTileX + (newXIdx - centerIndexX);
            int tileYNew = centerTileY + (yIdx - centerIndexY);
            Vector2 newAnchoredPos = new Vector2(
                tileXNew * tileSize + (tileSize * 0.5f),
                tileYNew * tileSize + (tileSize * 0.5f)
            );

            StartCoroutine(AnimateTileTransition(rt, newAnchoredPos, halfDur));
        }
    }

    /// <summary>
    /// Y방향으로 한 칸 이동했을 때 호출됩니다.
    /// dir == +1 → 마우스가 위로 넘어갔음. → 맨 아래(oldY=0) 행을 재활용해서 맨 위(newYIdx=gridHeight-1)로 옮김.
    /// dir == -1 → 마우스가 아래로 넘어갔음. → 맨 위(oldY=gridHeight-1) 행을 재활용해서 맨 아래(newYIdx=0)로 옮김.
    /// </summary>
    private void ShiftVertical(int dir)
    {
        // 재활용할 “이전 행” 인덱스
        int oldY = (dir == +1) ? 0 : (gridHeight - 1);
        // 재활용 타일을 옮길 “새 행” 인덱스
        int newYIdx = (dir == +1) ? (gridHeight - 1) : 0;

        // old 행의 모든 GameObject와 RectTransform을 임시 보관
        GameObject[] oldGOs = new GameObject[gridWidth];
        RectTransform[] oldRTs = new RectTransform[gridWidth];
        for (int x = 0; x < gridWidth; x++)
        {
            oldGOs[x] = tiles[x, oldY];
            oldRTs[x] = tileRTs[x, oldY];
        }

        // 중심 타일 좌표 업데이트 (Y축)
        centerTileY += dir;

        // 배열 내 타일 참조만 위/아래로 한 칸씩 민다.
        if (dir == +1)
        {
            // 위로 한 칸 이동했으므로, (1→0), (2→1), …, (gridHeight-1→gridHeight-2)
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight - 1; y++)
                {
                    tiles[x, y] = tiles[x, y + 1];
                    tileRTs[x, y] = tileRTs[x, y + 1];
                }
            }
        }
        else // dir == -1
        {
            // 아래로 한 칸 이동했으므로, (gridHeight-2→gridHeight-1), (gridHeight-3→gridHeight-2), …, (0→1)
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = gridHeight - 1; y > 0; y--)
                {
                    tiles[x, y] = tiles[x, y - 1];
                    tileRTs[x, y] = tileRTs[x, y - 1];
                }
            }
        }

        // 임시 보관해 둔 old 행을, 새 행(newYIdx) 위치에 넣는다.
        for (int x = 0; x < gridWidth; x++)
        {
            tiles[x, newYIdx] = oldGOs[x];
            tileRTs[x, newYIdx] = oldRTs[x];
        }

        // “남아있는” 타일들(재활용되지 않은 행 제외)에 대해서는 즉시 올바른 위치와 스케일(1)을 세팅한다.
        for (int yIdx = 0; yIdx < gridHeight; yIdx++)
        {
            if (yIdx == newYIdx)
                continue;

            for (int xIdx = 0; xIdx < gridWidth; xIdx++)
            {
                RectTransform rt = tileRTs[xIdx, yIdx];
                rt.localScale = Vector3.one;

                int tileX = centerTileX + (xIdx - centerIndexX);
                int tileY = centerTileY + (yIdx - centerIndexY);
                float px = tileX * tileSize + (tileSize * 0.5f);
                float py = tileY * tileSize + (tileSize * 0.5f);
                rt.anchoredPosition = new Vector2(px, py);
            }
        }

        // 재활용된 행(oldY)의 타일들: 
        // 1) 기존 위치에서 Scale 1→0 애니메이션 → 
        // 2) 새 위치로 스냅 → 
        // 3) Scale 0→1 애니메이션
        float halfDur = animDuration * 0.5f;
        for (int xIdx = 0; xIdx < gridWidth; xIdx++)
        {
            RectTransform rt = oldRTs[xIdx];

            int tileXNew = centerTileX + (xIdx - centerIndexX);
            int tileYNew = centerTileY + (newYIdx - centerIndexY);
            Vector2 newAnchoredPos = new Vector2(
                tileXNew * tileSize + (tileSize * 0.5f),
                tileYNew * tileSize + (tileSize * 0.5f)
            );

            StartCoroutine(AnimateTileTransition(rt, newAnchoredPos, halfDur));
        }
    }

    /// <summary>
    /// 하나의 타일에 대해
    /// 1) scale 1→0 (사라짐) 
    /// 2) anchoredPosition 스냅
    /// 3) scale 0→1 (나타남)
    /// 애니메이션을 수행합니다.
    /// </summary>
    private IEnumerator AnimateTileTransition(RectTransform rt, Vector2 newAnchoredPos, float halfDuration)
    {
        // 1) Scale‐out (1 → 0)
        float t = 0f;
        while (t < halfDuration)
        {
            float frac = t / halfDuration;
            rt.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, frac);
            t += Time.deltaTime;
            yield return null;
        }
        rt.localScale = Vector3.zero;

        // 2) 위치 스냅
        rt.anchoredPosition = newAnchoredPos;

        // 3) Scale‐in (0 → 1)
        t = 0f;
        while (t < halfDuration)
        {
            float frac = t / halfDuration;
            rt.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, frac);
            t += Time.deltaTime;
            yield return null;
        }
        rt.localScale = Vector3.one;
    }
}
