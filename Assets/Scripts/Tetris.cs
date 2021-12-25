using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// decisioning for where and how to drop a sequence of pieces to minimize height at end of game
public class Tetris
{
    private TetrisPiece[] pieces = new TetrisPiece[7]
    {
        new TetrisPiece(new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0), new Vector2Int(3, 0), 
            new PreferedSpot(null, new sbyte[] { 4 }, 1), new PreferedSpot(new sbyte[] { 4 }, null, 1), new PreferedSpot(null, new sbyte[] { 0, 0, 0 }, 0)),
        new TetrisPiece(new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0), new Vector2Int(0, 1), 
            new PreferedSpot(null, new sbyte[] { 2 }, 1), new PreferedSpot(null, new sbyte[] { 0, -1 }, 2), new PreferedSpot(null, new sbyte[] { 0, 0 }, 0), new PreferedSpot(null, new sbyte[] { 0, 3 }, 3), new PreferedSpot(null, new sbyte[] { 0 }, 3)), 
        new TetrisPiece(new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0), new Vector2Int(2, 1),
            new PreferedSpot(null, new sbyte[] { -2 }, 3), new PreferedSpot(null, new sbyte[] { 1, 0 }, 2), new PreferedSpot(null, new sbyte[] { 0, 0 }, 0), new PreferedSpot(-3, 0, 1), new PreferedSpot(null, new sbyte[] { 0 }, 1)),
        new TetrisPiece(new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(1, 1), 
            new PreferedSpot(null, new sbyte[] { 0 }, 0)),
        new TetrisPiece(new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(1, 1), new Vector2Int(2, 1),
            new PreferedSpot(null, new sbyte[] { 0, 1 }, 0), new PreferedSpot(null, new sbyte[] { -1 }, 1)),
        new TetrisPiece(new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0), new Vector2Int(1, 1),
            new PreferedSpot(null, new sbyte[] { -1, 1 }, 2), new PreferedSpot(null, new sbyte[] { 0, 0 }, 0), new PreferedSpot(null, new sbyte[] { 1 }, 1), new PreferedSpot(null, new sbyte[] { -1 }, 3)),
        new TetrisPiece(new Vector2Int(1, 0), new Vector2Int(2, 0), new Vector2Int(0, 1), new Vector2Int(1, 1),
            new PreferedSpot(null, new sbyte[] { -1, 0 }, 0), new PreferedSpot(null, new sbyte[] { 1 }, 1))
    };

    private byte[] sequence;
    public PieceDrop[] drops;
    public float[] points;
    private int maxHeight = 0;

    private bool[,] map = new bool[10, 4001];
    private List<Vector2Int> holes = new List<Vector2Int>();

    public Tetris(byte[] sequence)
    {
        this.sequence = sequence;
        this.drops = new PieceDrop[sequence.Length];
        this.points = new float[sequence.Length];
    }

    // wish to minimize
    private int GetMapScore()
    {
        int score = 0;
        for (byte i = 1; i < 10; i++)
        {
            score += GetSurface(i) - 1;
        }
        return score;
    }
    private float GetColumnScore(byte column, TetrisPiece piece, out byte orientation)
    {
        if(column >= 10 - piece.Width + 1) { orientation = 0; return -1; }
        PreferedSpot pref = new PreferedSpot();
        float maxScore = -1;
        for (int i = 0; i < piece.PreferenceCount; i++)
        {
            float score = 0;
            PreferedSpot p = piece.GetPreference(i);
            for (int j = 0; j < p.left.Length; j++)
            {
                if (p.left[j] == GetSurface((byte)(column + j)))
                {
                    score += (piece.PreferenceCount - i) / p.left.Length;
                }
            }
            for (byte j = 0; j < p.right.Length; j++)
            {
                if (p.right[j] == GetSurface((byte)(column + j + 1)))
                {
                    score += (piece.PreferenceCount - i) / p.right.Length;
                }
            }
            if(score >= 0)
            {
                if(score > maxScore)
                {
                    maxScore = score;
                    pref = p;
                }
            }
        }
        orientation = pref.orientation;
        return maxScore;
    }

    private int GetSurface(byte column)
    {
        // |    0  1  2  3  4  5  6  7  8  9    |
        // -1000 s1 s2 s3 s4 s5 s6 s7 s8 s9 1000
        if (column == 0)
        {
            return -1000;
        }
        else if (column == 10)
        {
            return 1000;
        }
        else
        {
            //Debug.Log(column + " = " + (GetHeight(column) - GetHeight((byte)(column - 1))));
            return GetHeight(column) - GetHeight((byte)(column - 1));
        }
    }
    private int GetHeight(byte column)
    {
        for (int y = maxHeight; y >= 0; y--)
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

    public void CalculateDrops()
    {
        for (int i = 0; i < sequence.Length; i++)
        {
            byte bestColumn = 0;
            byte bestOrientation = 0;
            float bestScore = 0;
            List<float> scores = new List<float>();
            for (byte c = 0; c < 10; c++)
            {
                float score = GetColumnScore(c, pieces[sequence[i]], out byte orientation);
                scores.Add(score);
                if (score > bestScore)
                {
                    bestColumn = c;
                    bestOrientation = orientation;
                    bestScore = score;
                }
            }
            Debug.Log(i + ":" + sequence[i] + " = " + string.Join(", ", scores));
            points[i] = bestScore;
            Drop(i, new PieceDrop(sequence[i], bestOrientation, bestColumn));
        }
    }
    public void Drop(int i, PieceDrop drop)
    {
        drops[i] = drop;
        TetrisPiece rotated = new TetrisPiece(pieces[drop.Piece], drop.Orientation);
        byte[] mins = rotated.MinSolids();

        int dropY = 0;
        for (byte c = 0; c < mins.Length; c++)
        {
            dropY = Mathf.Max(GetHeight((byte)(drop.X + c)) - mins[c], dropY);
        }

        for (byte s = 0; s < 4; s++)
        {
            Vector2Int target = new Vector2Int(drop.X, dropY);
            Vector2Int index = new Vector2Int(target.x + rotated[s].x, target.y + rotated[s].y);
            map[index.x, index.y] = true;
            maxHeight = Mathf.Max(maxHeight, index.y + 1);
        }

        // clear rows
        for (int y = 0; y < maxHeight; y++)
        {
            if (IsRowFull(y))
            {
                // clear line y
                for (int yn = y + 1; yn < maxHeight; yn++)
                {
                    for (int c = 0; c < 10; c++)
                    {
                        map[c, yn - 1] = map[c, yn];
                        map[c, yn] = false;
                    }
                }
                y--;
                maxHeight--;
            }
        }
    }

    public TetrisPiece this[byte i]
    {
        get { return pieces[i]; }
    }
}

public struct PieceDrop
{
    private byte piece;
    private byte orientation;
    private byte x;

    public PieceDrop(byte piece, byte orientation, byte x)
    {
        this.piece = piece;
        this.orientation = orientation;
        this.x = x;
    }

    public byte Piece { get { return piece; } }
    public byte Orientation { get { return orientation; } }
    public byte X { get { return x; } }
}
public struct TetrisPiece
{
    private Vector2Int[] solid;
    private int maxDimension; // TODO: use (maxDimension / 2, maxDimension / 2) as pivot for rotating piece
    private PreferedSpot[] preferences;

    public TetrisPiece(Vector2Int solid1, Vector2Int solid2, Vector2Int solid3, Vector2Int solid4)
    {
        solid = new Vector2Int[4] { solid1, solid2, solid3, solid4 };
        Vector2Int min = new Vector2Int(10, 10);
        Vector2Int max = new Vector2Int(-10, -10);
        for (int i = 0; i < solid.Length; i++)
        {
            min = Vector2Int.Min(solid[i], min);
            max = Vector2Int.Min(solid[i], max);
        }
        this.maxDimension = Mathf.Max(max.x - min.x, max.y - min.y);
        this.preferences = new PreferedSpot[0];
    }
    public TetrisPiece(Vector2Int solid1, Vector2Int solid2, Vector2Int solid3, Vector2Int solid4, params PreferedSpot[] preferences)
    {
        solid = new Vector2Int[4] { solid1, solid2, solid3, solid4 };
        Vector2Int min = new Vector2Int(10, 10);
        Vector2Int max = new Vector2Int(-10, -10);
        for (int i = 0; i < solid.Length; i++)
        {
            min = Vector2Int.Min(solid[i], min);
            max = Vector2Int.Min(solid[i], max);
        }
        this.maxDimension = Mathf.Max(max.x - min.x, max.y - min.y);
        this.preferences = preferences;
    }
    public TetrisPiece(TetrisPiece source, byte orientation) : this(source.solid[0], source.solid[1], source.solid[2], source.solid[3])
    {
        Vector2Int min = Vector2Int.zero;
        for (int i = 0; i < solid.Length; i++)
        {
            solid[i] = Vector2Int.RoundToInt(Quaternion.Euler(0, 0, -90 * orientation) * (Vector2)solid[i]);
            min = Vector2Int.Min(min, solid[i]);
        }
        for (int i = 0; i < solid.Length; i++)
        {
            solid[i] -= min;
        }
    }

    public byte[] MinSolids()
    {
        byte[] mins = new byte[Width];
        for (int i = 0; i < mins.Length; i++)
        {
            mins[i] = byte.MaxValue;
        }
        for (int i = 0; i < solid.Length; i++)
        {
            mins[solid[i].x] = (byte)Mathf.Min(mins[solid[i].x], solid[i].y);
        }
        return mins;
    }

    public PreferedSpot GetPreference(int i)
    {
        return preferences[i];
    }

    public byte Width
    {
        get
        {
            byte width = 0;
            foreach (var item in solid)
            {
                width = (byte)Mathf.Max(width, item.x + 1);
            }
            return width;
        }
    }
    public int PreferenceCount { get { return preferences.Length; } }

    public Vector2Int this[byte i] { get { return solid[i]; } }

    public override string ToString()
    {
        return string.Join(", ", solid);
    }
}
public struct PreferedSpot
{
    // 0, 1, 2, 3, 4, 5+
    public sbyte[] left;
    public sbyte[] right;
    public byte orientation;

    public PreferedSpot(sbyte[] left, sbyte[] right, byte orientation)
    {
        this.left = (left != null) ? left : new sbyte[0];
        this.right = (right != null) ? right : new sbyte[0];
        this.orientation = orientation;
    }
    public PreferedSpot(sbyte left, sbyte right, byte orientation)
    {
        this.left = new sbyte[] { left };
        this.right = new sbyte[] { right };
        this.orientation = orientation;
    }

    public override string ToString()
    {
        return string.Join(", ", left) + " | " + orientation + " | " + string.Join(", ", right);
    }
}