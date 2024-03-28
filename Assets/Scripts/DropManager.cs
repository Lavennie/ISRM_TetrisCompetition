using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;

public class DropManager : MonoBehaviour
{
    public TextAsset sequenceSource;
    public Transform grid;
    public GameObject squarePrefab;
    public GameObject[] piecePrefabs;
    new public Camera camera;
    [Range(0.0f, 20.0f)]
    public float cameraSpeed = 1.0f;
    [Range(0.0f, 10.0f)]
    public float speed = 1.0f;

    [Header("Params")]
    public float correctShape = 2.0f;
    public float badShape = -5.0f;
    public float sideShape = 0.25f;
    public float clearLine = 3.0f;
    public float height = -1.5f;

    private Tetris game;
    private bool[,] map = new bool[10, 4000];
    private GameObject[,] mapGo = new GameObject[10, 4000];
    private GameObject droppingPiece;
    private Vector3 dropTarget;

    private int score;

    private byte[] sequence;
    private Coroutine playing;

    public List<int> res = new List<int>();
    public List<float[]> resParams = new List<float[]>();

    private void Start()
    {
        sequence = InitSequence();

        /*correctShape = -5.0f;
        badShape = -5.0f;
        sideShape = -5.0f;
        clearLine = -5.0f;
        height = -5.0f;*/
        //1,-1,0.25,1,-1
        Play();
    }

    private void Play()
    {
        camera.transform.position = new Vector3(0, CamYOffset, -10);
        for (int i = 0; i < mapGo.GetLength(0); i++)
        {
            for (int j = 0; j < mapGo.GetLength(1); j++)
            {
                if (mapGo[i, j] != null)
                {
                    Destroy(mapGo[i, j]);
                }
            }
        }
        score = 0;
        map = new bool[10, 4000];
        mapGo = new GameObject[10, 4000];
        if (droppingPiece != null) { Destroy(droppingPiece); }
        droppingPiece = null;
        dropTarget = Vector3.zero;

        game = new Tetris(sequence, correctShape, badShape, sideShape, clearLine, height);
        game.CalculateDrops();
        Debug.Log("<color=magenta>" + $"{game.MaxHeight},{correctShape},{badShape},{sideShape},{clearLine},{height}" + "</color>");
        playing = StartCoroutine(Play(game.drops));
    }
    private byte[] InitSequence()
    {
        if (sequenceSource != null)
        {
            string[] lines = sequenceSource.text.Split('\n');
            byte[] result = new byte[lines.Length - ((string.IsNullOrEmpty(lines[lines.Length - 1])) ? 1 : 0)];
            for (int i = 0; i < result.Length - 1; i++)
            {
                result[i] = byte.Parse(lines[i]);
            }
            return result;
        }
        else
        {
            byte[] result = new byte[1000];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = (byte)Random.Range(0, 7);
            }
            return result;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (playing != null)
            {
                StopCoroutine(playing);
            }
            Play();
        }
        else if (Input.GetKeyDown(KeyCode.R))
        {
            if (playing != null)
            {
                StopCoroutine(playing);
            }
            sequence = InitSequence();
            Play();
        }

        if (droppingPiece != null)
        {
            droppingPiece.transform.localPosition = Vector3.MoveTowards(droppingPiece.transform.localPosition, dropTarget, speed);
            if (droppingPiece.transform.localPosition == dropTarget)
            {
                Destroy(droppingPiece.gameObject);
                droppingPiece = null;
            }
        }

        if (MaxHeight > 14)
        {
            transform.position = Vector3.MoveTowards(transform.position,
                new Vector3(transform.position.x, -MaxHeight + CamYOffset, 0), cameraSpeed);
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position,
                new Vector3(transform.position.x, 0.5f, 0), cameraSpeed);
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
            for (byte i = 0; i < game[drop.Piece].Width(drop.Orientation); i++)
            {
                Vector2Int target = Vector2Int.RoundToInt(dropTarget);
                for (byte j = 0; j < game[drop.Piece].Height(i, drop.Orientation); j++)
                {
                    Vector2Int index = new Vector2Int(target.x + i, target.y + game[drop.Piece].Min(i, drop.Orientation) + j);
                    map[index.x, index.y] = true;
                    mapGo[index.x, index.y] = Instantiate(squarePrefab, transform.position + (Vector3)(Vector2)index, Quaternion.identity, transform);
                    mapGo[index.x, index.y].GetComponent<SpriteRenderer>().color = pieceColor;
                }
            }
            /*TetrisPiece rotated = new TetrisPiece(game[drop.Piece], drop.Orientation);
            for (byte i = 0; i < 4; i++)
            {
                Vector2Int target = Vector2Int.RoundToInt(dropTarget);
                Vector2Int index = new Vector2Int(target.x + rotated[i].x, target.y + rotated[i].y);
                map[index.x, index.y] = true;
                mapGo[index.x, index.y] = Instantiate(squarePrefab, transform.position + (Vector3)(Vector2)index, Quaternion.identity, transform);
                mapGo[index.x, index.y].GetComponent<SpriteRenderer>().color = pieceColor;
            }*/

            // clear rows
            int clearTo = MaxHeight;
            int loopCount = 0;
            for (int y = 0; y < clearTo; y++)
            {
                if (loopCount > 10000)
                {
                    Debug.LogError(new System.StackOverflowException("Reached loop repeat limit"));
#if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
#endif
                }
                if (IsRowFull(y))
                {
                    Debug.Log("CLEAR <color=red>" + y + "</color>");
                    // clear line y
                    for (int x = 0; x < 10; x++)
                    {
                        map[x, y] = false;
                        Destroy(mapGo[x, y]);
                        mapGo[x, y] = null;
                    }
                    for (int j = y + 1; j < clearTo; j++)
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
                    clearTo--;
                }
                loopCount++;
            }

            score = Mathf.Max(score, MaxHeight);
        }

        Debug.Log("FINISHED with score: <color=yellow>" + score + "</color>");
        Debug.Log("FINISHED with score: <color=green>" + game.MaxHeight + "</color>");
    }
    private void Drop(PieceDrop drop, float points)
    {
        //TetrisPiece rotated = new TetrisPiece(game[drop.Piece], drop.Orientation);
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
        //droppingPiece.transform.GetChild(4).GetComponent<TextMeshPro>().text = points.ToString();
        // -----------------------------
        //byte[] mins = rotated.MinSolids();

        int dropY = 0;
        for (byte i = 0; i < game[drop.Piece].Width(drop.Orientation); i++)
        {
            dropY = Mathf.Max(GetHeight((byte)(drop.X + i)) - game[drop.Piece].Min(i, drop.Orientation), dropY);
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
