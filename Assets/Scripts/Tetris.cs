using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// decisioning for where and how to drop a sequence of pieces to minimize height at end of game
public class Tetris
{
    private TetrisPiece[] pieces = new TetrisPiece[]
    {
        new TetrisPiece(
            new byte[] { 0, 1, 0, 1 }, 
            new byte[][] { new byte[] { 0, 0, 0, 0 }, new byte[] { 0 } },
            new byte[][] { new byte[] { 1, 1, 1, 1 }, new byte[] { 4 } },
            new sbyte[][] { new sbyte[] { 0, 0, 0 }, new sbyte[] { } }, 
            new sbyte[] { 0, -2 },
            new sbyte[] { 0, 2 }),
        new TetrisPiece(
            new byte[] { 0, 1, 2, 3 },
            new byte[][] { new byte[] { 0, 0, 0 }, new byte[] { 0, 2 }, new byte[] { 1, 1, 0 }, new byte[] { 0, 0 } },
            new byte[][] { new byte[] { 2, 1, 1 }, new byte[] { 3, 1 }, new byte[] { 1, 1, 2 }, new byte[] { 1, 3 } },
            new sbyte[][] { new sbyte[] { 0, 0 }, new sbyte[] { 2 }, new sbyte[] { 0, -1 }, new sbyte[] { 0 } },
            new sbyte[] { 0, 0, 0, 0 },
            new sbyte[] { 0, 0, 0, 0 }),
        new TetrisPiece(
            new byte[] { 0, 1, 2, 3 },
            new byte[][] { new byte[] { 0, 0, 0 }, new byte[] { 0, 0 }, new byte[] { 0, 1, 1 }, new byte[] { 2, 0 } },
            new byte[][] { new byte[] { 1, 1, 2 }, new byte[] { 3, 1 }, new byte[] { 2, 1, 1 }, new byte[] { 1, 3 } },
            new sbyte[][] { new sbyte[] { 0, 0 }, new sbyte[] { 0 }, new sbyte[] { 1, 0 }, new sbyte[] { -2 } },
            new sbyte[] { 0, 0, 0, 0 },
            new sbyte[] { 0, 0, 0, 0 }),
        new TetrisPiece(
            new byte[] { 0, 0, 0, 0 },
            new byte[][] { new byte[] { 0, 0 } },
            new byte[][] { new byte[] { 2, 2 } },
            new sbyte[][] { new sbyte[] { 0 } },
            new sbyte[] { 0 },
            new sbyte[] { 0 }),
        new TetrisPiece(
            new byte[] { 0, 1, 0, 1 },
            new byte[][] { new byte[] { 0, 0, 1 }, new byte[] { 1, 0 } },
            new byte[][] { new byte[] { 1, 2, 1 }, new byte[] { 2, 2 } },
            new sbyte[][] { new sbyte[] { 0, 1 }, new sbyte[] { -1 } },
            new sbyte[] { 0, 0 },
            new sbyte[] { 0, 0 }),
        new TetrisPiece(
            new byte[] { 0, 1, 2, 3 },
            new byte[][] { new byte[] { 0, 0, 0 }, new byte[] { 0, 1 }, new byte[] { 1, 0, 1 }, new byte[] { 1, 0 } },
            new byte[][] { new byte[] { 1, 2, 1 }, new byte[] { 3, 1 }, new byte[] { 1, 2, 1 }, new byte[] { 1, 3 } },
            new sbyte[][] { new sbyte[] { 0, 0 }, new sbyte[] { 1 }, new sbyte[] { -1, 1 }, new sbyte[] { -1 } },
            new sbyte[] { 0, -1, 0, 0 },
            new sbyte[] { 0, 0, 0, 1 }),
        new TetrisPiece(
            new byte[] { 0, 1, 0, 1 },
            new byte[][] { new byte[] { 1, 0, 0 }, new byte[] { 0, 1 } },
            new byte[][] { new byte[] { 1, 2, 1 }, new byte[] { 2, 2 } },
            new sbyte[][] { new sbyte[] { -1, 0 }, new sbyte[] { 1 } },
            new sbyte[] { 0, 0 },
            new sbyte[] { 0, 0 }),
    };

    public readonly float POINTS_CORRECT_SHAPE_MUL = 2.0f;
    public readonly float POINTS_BAD_SHAPE_MUL = -5.0f;
    public readonly float POINTS_SIDE_SHAPE_MUL = 0.25f;
    public readonly float POINTS_CLEAR_LINE_MUL = 3.0f;
    public readonly float POINTS_HEIGHT_MUL = -1.5f;

    private byte[] sequence;
    public PieceDrop[] drops;
    public float[] points;
    private int maxHeight = 0;

    private bool[,] map = new bool[10, 4001];

    public Tetris(byte[] sequence, float correctShapeMul, float badShapeMul, float sideShapeMul, float clearLineMul, float heightMul)
    {
        this.POINTS_CORRECT_SHAPE_MUL = correctShapeMul;
        this.POINTS_BAD_SHAPE_MUL = badShapeMul;
        this.POINTS_SIDE_SHAPE_MUL = sideShapeMul;
        this.POINTS_CLEAR_LINE_MUL = clearLineMul;
        this.POINTS_HEIGHT_MUL = heightMul;

        this.sequence = sequence;
        this.drops = new PieceDrop[sequence.Length];
        this.points = new float[sequence.Length];
    }

    public int GetSurface(byte column)
    {
        // |     0  1  2  3  4  5  6  7  8  9     |
        //  s0:-4 s1 s2 s3 s4 s5 s6 s7 s8 s9 s10:4
        if (column == 0)
        {
            return -4;
        }
        else if (column == 10)
        {
            return 4;
        }
        else
        {
            return GetHeight(column) - GetHeight((byte)(column - 1));
        }
    }
    public int GetHeight(byte column)
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
    private bool WillRowBeFull(int y, byte p, byte o, byte px, int py)
    {
        for (int x = 0; x < 10; x++)
        {
            if (!map[x, y] && !pieces[p].HasSolid((sbyte)(x - px), (sbyte)(y - py), o))
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
            //string debug = "";
            SimulatePiece(i, out byte x, out int y, out byte o, out float points);
            //Debug.Log(sequence[i] + " (" + bestO + ") = " + bestPoints + " -> " + new Vector2Int(bestX, bestY));
            //Debug.Log(new Vector2Int(bestX, bestY) + " -> " + debug);
            Drop(i, o, x, y);
        }
    }
    public bool SimulateDrop(byte x, byte o, byte p, out float points, out int dropY)
    {
        points = 0;
        dropY = 0;
        TetrisPiece piece = pieces[p];

        if (x + piece.Width(o) - 1 >= 10) { return false; };

        for (byte c = 0; c < piece.Width(o); c++)
        {
            dropY = Mathf.Max(GetHeight((byte)(x + c)) - piece.Min(c, o), dropY);
        }

        // add points for row clears
        for (int t = 0; t < 4; t++)
        {
            if (WillRowBeFull(dropY + t, p, o, x, dropY))
            {
                points += POINTS_CLEAR_LINE_MUL;
            }
        }
        // deduct points for higher height
        points += dropY * POINTS_HEIGHT_MUL;
        // add/deduct points for following shape (not leaving holes)
        points += piece.Points(this, x, dropY, o);


        //Debug.Log("SIMULATE: " + new Vector2Int(x, dropY) + " " + points + " =   " + debug);
        return true;
    }
    public void Drop(int i, byte o, byte x, int y)
    {
        drops[i] = new PieceDrop(sequence[i], o, x);

        int newMaxHeight = 0;
        // update map and max height
        for (byte c = 0; c < pieces[sequence[i]].Width(o); c++)
        {
            for (int r = 0; r < pieces[sequence[i]].Height(c, o); r++)
            {
                map[x + c, y + pieces[sequence[i]].Min(c, o) + r] = true;
                newMaxHeight = Mathf.Max(newMaxHeight, y + pieces[sequence[i]].Min(c, o) + r + 1);
            }
        }

        // clear rows
        for (int r = 0; r < newMaxHeight; r++)
        {
            if (IsRowFull(r))
            {
                // clear line r
                for (int rn = r + 1; rn < maxHeight; rn++)
                {
                    for (int c = 0; c < 10; c++)
                    {
                        map[c, rn - 1] = map[c, rn];
                        map[c, rn] = false;
                    }
                }
                r--;
                newMaxHeight--;
            }
        }
        maxHeight = Mathf.Max(maxHeight, newMaxHeight);
    }

    private void SimulatePiece(int i, out byte dropX, out int dropY, out byte dropO, out float points)
    {
        points = float.MinValue;
        dropX = 0;
        dropY = 0;
        dropO = 0;
        for (byte x = 0; x < 10; x++)
        {
            for (byte o = 0; o < 4; o++)
            {
                if (SimulateDrop(x, o, sequence[i], out float tempPoints, out int tempDropY))
                {
                    //debug += points + ", ";
                    if (tempPoints > points)
                    {
                        points = tempPoints;
                        dropO = o;
                        dropX = x;
                        dropY = tempDropY;
                    }
                }
            }
            //debug += " | ";
        }
        if (points == float.MinValue)
        {
            Debug.LogError(new System.ArgumentException("OOPS! Something went wrong >:("));
        }
    }

    public TetrisPiece this[byte i]
    {
        get { return pieces[i]; }
    }

    public int MaxHeight { get { return maxHeight; } }
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

    public override string ToString()
    {
        return $"{piece} ({orientation}) -> {x}";
    }

    public byte Piece { get { return piece; } }
    public byte Orientation { get { return orientation; } }
    public byte X { get { return x; } }
}
public class TetrisPiece
{
    private byte[] oTOi;
    private byte[][] mins;
    private byte[][] height;
    private sbyte[][] derivative;
    private sbyte[] left, right;

    public TetrisPiece(byte[] orientationToIndex, byte[][] mins, byte[][] height, sbyte[][] derivative, sbyte[] left, sbyte[] right)
    {
        this.oTOi = orientationToIndex;
        this.mins = mins;
        this.height = height;
        this.derivative = derivative;
        this.left = left;
        this.right = right;
    }

    public byte Min(byte x, byte o)
    {
        return mins[oTOi[o]][x];
    }
    public byte Width(byte o)
    {
        return (byte)mins[oTOi[o]].Length;
    }
    public byte Height(byte x, byte o)
    {
        return height[oTOi[o]][x];
    }
    public bool HasSolid(sbyte x, sbyte y, byte o)
    {
        if (x < 0 || x >= Width(o)) { return false; }
        return y - mins[oTOi[o]][x] < height[oTOi[o]][x];
    }
    public float Points(Tetris map, byte x, int y, byte o)
    {
        return ShapePointsBase(map, x, y, o) + ShapePointsSide(map, x, y, o);
    }

    private float ShapePointsBase(Tetris map, byte x, int y, byte o)
    {
        if (x + derivative[oTOi[o]].Length >= 10) { return float.MinValue; }
        float score = 0;
        int d = y + mins[oTOi[o]][0] - map.GetHeight(x); // difference in derivatives
        for (byte j = 0; j < derivative[oTOi[o]].Length; j++)
        {
            d = map.GetSurface((byte)(x + j + 1)) - derivative[oTOi[o]][j] + d;
            if (d == 0)
            {
                score += map.POINTS_CORRECT_SHAPE_MUL;
            }
            else
            {
                score += Mathf.Abs(d) * map.POINTS_BAD_SHAPE_MUL;
            }
        }
        return score;
    }
    private float ShapePointsSide(Tetris map, byte x, int y, byte o)
    {
        int sL = map.GetSurface(x) + (y + mins[oTOi[o]][0] - map.GetHeight(x));
        int sR = map.GetSurface((byte)(x + mins[oTOi[o]].Length)) - (y + mins[oTOi[o]][mins[oTOi[o]].Length - 1] - map.GetHeight((byte)(x + mins[oTOi[o]].Length - 1)));
        return (Mathf.Max(0, -sL - left[oTOi[o]]) + Mathf.Max(0, sR - right[oTOi[o]])) * map.POINTS_SIDE_SHAPE_MUL;
    }
}
/*public struct TetrisPiece
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
    public PreferedSpot[] GetPreferences(byte orientation)
    {
        List<PreferedSpot> p = new List<PreferedSpot>();
        for (int i = 0; i < preferences.Length; i++)
        {
            if (preferences[i].orientation == orientation)
            {
                p.Add(preferences[i]);
            }
        }
        return p.ToArray();
    }

    public bool HasSquare(Vector2Int square)
    {
        for (int i = 0; i < solid.Length; i++)
        {
            if (solid[i] == square)
            {
                return true;
            }
        }
        return false;
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
}*/
/*public struct PreferedSpot
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
}*/
/*public class Tetris
{
    private TetrisPiece[] pieces = new TetrisPiece[7]
    {
        new TetrisPiece(new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0), new Vector2Int(3, 0), 
            new PreferedSpot(null, new sbyte[] { 4 }, 1), new PreferedSpot(new sbyte[] { -4 }, null, 1), new PreferedSpot(null, new sbyte[] { 0, 0, 0 }, 0)),
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

    [Header("Points per condition")]
    private const float POINTS_CORRECT_SHAPE_MUL = 3.0f;
    private const float POINTS_BAD_SHAPE_MUL = -2.0f;
    private const float POINTS_CLEAR_LINE_MUL = 5.0f;
    private const float POINTS_COVER_HOLE_MUL = -2.0f;
    private const float POINTS_HEIGHT_MUL = -1.0f;

    private byte[] sequence;
    public PieceDrop[] drops;
    public float[] points;
    private int maxHeight = 0;

    private bool[,] map = new bool[10, 4001];
    private List<Vector2Int>[] holes = new List<Vector2Int>[10];

    public Tetris(byte[] sequence)
    {
        this.sequence = sequence;
        this.drops = new PieceDrop[sequence.Length];
        this.points = new float[sequence.Length];

        for (int i = 0; i < holes.Length; i++)
        {
            holes[i] = new List<Vector2Int>();
        }
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
    private bool WillRowBeFull(int row, TetrisPiece piece, Vector2Int piecePos)
    {
        for (int i = 0; i < 10; i++)
        {
            if (!map[i, row] && !piece.HasSquare(new Vector2Int(i - piecePos.x, row - piecePos.y)))
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
            Vector2Int bestPos = Vector2Int.zero;
            TetrisPiece bestPiece = pieces[sequence[i]];
            byte bestOrientation = 0;
            float bestScore = 0;
            string debug = "";
            for (byte c = 0; c < 10; c++)
            {
                for (byte o = 0; o < 4; o++)
                {
                    if (SimulateDrop(new PieceDrop(sequence[i], o, c), out float score, out Vector2Int pos, out TetrisPiece piece))
                    {
                        debug += o + ":" + score + ", ";
                        if (score > bestScore)
                        {
                            bestColumn = c;
                            bestPos = pos;
                            bestPiece = piece;
                            bestOrientation = o;
                            bestScore = score;
                        }
                    }
                }
                debug += " | ";
            }
            //Debug.Log(sequence[i] + " (" + bestOrientation + ") = " + bestScore + " -> " + bestPos);
            //Debug.Log(bestPos + " -> " + debug);
            points[i] = bestScore;
            Drop(i, sequence[i], bestOrientation, bestPiece, bestPos);
        }
    }
    public bool SimulateDrop(PieceDrop drop, out float points, out Vector2Int pos, out TetrisPiece piece)
    {
        points = 0;
        pos = new Vector2Int(drop.X, 0);

        piece = new TetrisPiece(pieces[drop.Piece], drop.Orientation);
        if(drop.X + piece.Width - 1 >= 10) { return false; };
        PreferedSpot[] prefs = pieces[drop.Piece].GetPreferences(drop.Orientation);
        if (prefs.Length == 0) { return false; }

        byte[] mins = piece.MinSolids();
        for (byte c = 0; c < mins.Length; c++)
        {
            pos.y = Mathf.Max(GetHeight((byte)(drop.X + c)) - mins[c], pos.y);
        }

        string debug = "HOLES: ";
        // deduct points for covering hole
        for (int s = 0; s < piece.Width; s++)
        {
            points += holes[drop.X + s].Count * POINTS_COVER_HOLE_MUL;
            debug += holes[drop.X + s].Count * POINTS_COVER_HOLE_MUL + ", ";
        }
        debug += " / ROW: ";
        // add points for row clears
        for (int t = 0; t < 4; t++)
        {
            if (WillRowBeFull(pos.y + t, piece, pos))
            {
                points += POINTS_CLEAR_LINE_MUL;
                debug += POINTS_CLEAR_LINE_MUL + ", ";
            }
        }
        debug += " / HEIGHT:  ";
        // deduct points for higher height
        points += pos.y * POINTS_HEIGHT_MUL;
        debug += pos.y * POINTS_HEIGHT_MUL + " / SHAPE: ";
        // add/deduct points for following shape (not leaving holes)
        PreferedSpot pref = new PreferedSpot();
        float maxScore = float.MinValue;
        for (int i = 0; i < prefs.Length; i++)
        {
            debug += "P" + i + ":";
            float score = 0;
            int d = pos.y + mins[0] - GetHeight((byte)pos.x); // difference in derivatives
            for (int j = 0; j < prefs[i].left.Length; j++)
            {
                if (drop.X - j < 0) { continue; }
                if ((prefs[i].left[j] + d) <= -4 && GetSurface((byte)(drop.X - j)) <= -4)
                {
                    d = GetSurface((byte)(drop.X - j)) - prefs[i].left[j] + d;
                    score += POINTS_CORRECT_SHAPE_MUL;
                    debug += "L" + POINTS_CORRECT_SHAPE_MUL + ", ";
                }
                else
                {
                    d = GetSurface((byte)(drop.X - j)) - prefs[i].left[j] + d;
                    if (d == 0)
                    {
                        score += POINTS_CORRECT_SHAPE_MUL;
                        debug += "L" + POINTS_CORRECT_SHAPE_MUL + ", ";
                    }
                    else
                    {
                        score += Mathf.Abs(d) * POINTS_BAD_SHAPE_MUL;
                        debug += "L" + Mathf.Abs(d) * POINTS_BAD_SHAPE_MUL + ", ";
                    }
                }
            }
            d = pos.y + mins[0] - GetHeight((byte)pos.x);
            for (byte j = 0; j < prefs[i].right.Length; j++)
            {
                if (drop.X + j + 1 > 10) { continue; }
                if ((prefs[i].right[j] + d) >= 4 && GetSurface((byte)(drop.X + j + 1)) >= 4)
                {
                    d = GetSurface((byte)(drop.X + j + 1)) - prefs[i].right[j] + d;
                    score += POINTS_CORRECT_SHAPE_MUL;
                    debug += "R" + POINTS_CORRECT_SHAPE_MUL + ", ";
                }
                else
                {
                    d = GetSurface((byte)(drop.X + j + 1)) - prefs[i].right[j] + d;
                    if (d == 0)
                    {
                        score += POINTS_CORRECT_SHAPE_MUL;
                        debug += "R" + POINTS_CORRECT_SHAPE_MUL + ", ";
                    }
                    else
                    {
                        score += Mathf.Abs(d) * POINTS_BAD_SHAPE_MUL;
                        debug += "R" + Mathf.Abs(d) * POINTS_BAD_SHAPE_MUL + ", ";
                    }
                }
            }
            if (score > maxScore)
            {
                maxScore = score;
                pref = prefs[i];
            }
        }
        points += maxScore;
        if (drop.Piece == 2)
        {
            //Debug.Log("SIMULATE: " + pos + points + " = " + drop + "   " + debug);
        }
        return true;
    }
    public void Drop(int i, byte p, byte o, TetrisPiece piece, Vector2Int pos)
    {
        drops[i] = new PieceDrop(p, o, (byte)pos.x);
        //Vector2Int target = SimulateDrop(drop, out float points, out TetrisPiece piece);

        // update max height
        for (byte s = 0; s < 4; s++)
        {
            Vector2Int index = new Vector2Int(pos.x + piece[s].x, pos.y + piece[s].y);
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

    public override string ToString()
    {
        return $"{piece} ({orientation}) -> {x}";
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
    public PreferedSpot[] GetPreferences(byte orientation)
    {
        List<PreferedSpot> p = new List<PreferedSpot>();
        for (int i = 0; i < preferences.Length; i++)
        {
            if (preferences[i].orientation == orientation)
            {
                p.Add(preferences[i]);
            }
        }
        return p.ToArray();
    }

    public bool HasSquare(Vector2Int square)
    {
        for (int i = 0; i < solid.Length; i++)
        {
            if (solid[i] == square)
            {
                return true;
            }
        }
        return false;
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
}*/