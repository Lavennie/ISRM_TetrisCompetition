using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DropManager : MonoBehaviour
{
    public Transform grid;
    public GameObject squarePrefab;
    public GameObject[] piecePrefabs;
    public Camera camera;
    public float speed = 1.0f;

    private Tetris game;
    private bool[,] map = new bool[10, 4000];
    private GameObject[,] mapGo = new GameObject[10, 4000];
    private GameObject droppingPiece;
    private Vector3 dropTarget;

    private int score;

    private void Awake()
    {
        camera.transform.position = new Vector3(0, CamYOffset, -10);
        game = new Tetris(InitSequence());
        game.CalculateDrops();
        StartCoroutine(Play(game.drops));
    }
    private byte[] InitSequence()
    {
        byte[] result = new byte[10];
        for (int i = 0; i < result.Length; i++)
        {
            result[i] = (byte)Random.Range(0, 7);
        }
        return result;
    }

    private void Update()
    {
        if (droppingPiece != null)
        {
            droppingPiece.transform.localPosition = Vector3.MoveTowards(droppingPiece.transform.localPosition, dropTarget, speed);
            if (droppingPiece.transform.localPosition == dropTarget)
            {
                droppingPiece.transform.GetChild(4).SetParent(transform);
                Destroy(droppingPiece.gameObject);
                droppingPiece = null;
            }
        }

        if (MaxHeight > 14)
        {
            transform.position = Vector3.MoveTowards(transform.position,
                new Vector3(transform.position.x, -MaxHeight + CamYOffset, 0), 0.2f);
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position,
                new Vector3(transform.position.x, 0.5f, 0), 0.2f);
        }
        grid.position = new Vector3(grid.position.x, Mathf.Repeat(transform.position.y - 0.5f, 1.0f) - 1, 0);
    }

    private IEnumerator Play(PieceDrop[] sequence)
    {
        int k = 0;
        foreach (var drop in sequence)
        {
            Drop(drop, game.points[k]);
            k++;
            Color pieceColor = droppingPiece.transform.GetChild(0).GetComponent<SpriteRenderer>().color;
            yield return new WaitUntil(() => droppingPiece == null);
            // insert piece into map
            TetrisPiece rotated = new TetrisPiece(game[drop.Piece], drop.Orientation);
            for (byte i = 0; i < 4; i++)
            {
                Vector2Int target = Vector2Int.RoundToInt(dropTarget);
                Vector2Int index = new Vector2Int(target.x + rotated[i].x, target.y + rotated[i].y);
                map[index.x, index.y] = true;
                mapGo[index.x, index.y] = Instantiate(squarePrefab, transform.position + (Vector3)(Vector2)index, Quaternion.identity, transform);
                mapGo[index.x, index.y].GetComponent<SpriteRenderer>().color = pieceColor;
            }

            // clear rows
            for (int y = 0; y < MaxHeight; y++)
            {
                if (IsRowFull(y))
                {
                    Debug.Log("CLEAR <color=red>" + y + "</color>");
                    // clear line y
                    for (int x = 0; x < 10; x++)
                    {
                        Destroy(mapGo[x, y]);
                        mapGo[x, y] = null;
                    }
                    int maxHeight = MaxHeight;
                    for (int j = y + 1; j < maxHeight; j++)
                    {
                        for (int x = 0; x < 10; x++)
                        {
                            map[x, j - 1] = map[x, j];
                            map[x, j] = false;
                            mapGo[x, j]?.transform.Translate(0, -1, 0, Space.World);
                            mapGo[x, j - 1] = mapGo[x, j];
                            mapGo[x, j] = null;
                        }
                    }
                    y--;
                }
            }

            score = Mathf.Max(score, MaxHeight);
        }

        Debug.Log("FINISHED with score: <color=yellow>" + score + "</color>");
    }
    private void Drop(PieceDrop drop, float points)
    {
        TetrisPiece rotated = new TetrisPiece(game[drop.Piece], drop.Orientation);
        droppingPiece = Instantiate(piecePrefabs[drop.Piece], transform);

        droppingPiece.transform.localPosition = new Vector3(drop.X, Mathf.Ceil(MaxHeight + 2.0f * CamYOffset));
        droppingPiece.transform.localRotation = Quaternion.Euler(0, 0, -90 * drop.Orientation);
        // displace squares
        Vector2Int min = Vector2Int.zero;
        foreach (Transform square in droppingPiece.transform)
        {
            min = Vector2Int.Min(Vector2Int.RoundToInt(square.position - droppingPiece.transform.position), min);
        }
        foreach (Transform square in droppingPiece.transform)
        {
            square.Translate((Vector2)(-min), Space.World);
        }
        droppingPiece.transform.GetChild(4).GetComponent<TextMeshPro>().text = points.ToString();
        // -----------------------------
        byte[] mins = rotated.MinSolids();

        int dropY = 0;
        for (int i = 0; i < mins.Length; i++)
        {
            dropY = Mathf.Max(GetHeight((byte)(drop.X + i)) - mins[i], dropY);
        }
        dropTarget = new Vector3(drop.X, dropY);
    }

    private int GetHeight(byte column)
    {
        for (int y = MaxHeight; y >= 0; y--)
        {
            if (map[column, y])
            {
                return y + 1;
            }
        }
        return 0;
    }
    private bool IsRowFull(int row)
    {
        for (int i = 0; i < 10; i++)
        {
            if (!map[i, row])
            {
                return false;
            }
        }
        return true;
    }

    private int MaxHeight
    {
        get
        {
            for (int y = 0; y < 4000; y++)
            {
                bool clearLine = true;
                for (int x = 0; x < 10; x++)
                {
                    if (map[x, y])
                    {
                        clearLine = false;
                        break;
                    }
                }
                if (clearLine)
                {
                    return y;
                }
            }
            return 4000;
        }
    }
    private float CamYOffset { get { return camera.orthographicSize; } }
}
