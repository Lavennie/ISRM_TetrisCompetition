using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// decisioning for where and how to drop a sequence of pieces to minimize height at end of game
public class Tetris
{
    private TetrisPiece[] pieces = new TetrisPiece[7]
    {
        new TetrisPiece(new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0), new Vector2Int(3, 0)),
        new TetrisPiece(new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0), new Vector2Int(0, 1)),
        new TetrisPiece(new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0), new Vector2Int(2, 1)),
        new TetrisPiece(new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(1, 1)),
        new TetrisPiece(new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(1, 1), new Vector2Int(2, 1)),
        new TetrisPiece(new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0), new Vector2Int(1, 1)),
        new TetrisPiece(new Vector2Int(1, 0), new Vector2Int(2, 0), new Vector2Int(0, 1), new Vector2Int(1, 1))
    };

    private byte[] sequence;

    public Tetris(byte[] sequence)
    {
        this.sequence = sequence;
    }

    public PieceDrop[] CalculateDrops()
    {
        return new PieceDrop[]
        {
            new PieceDrop(0, 1, 0),
            new PieceDrop(0, 1, 0),
            new PieceDrop(3, 0, 1),
            new PieceDrop(3, 0, 1),
            new PieceDrop(3, 0, 1),
            new PieceDrop(3, 0, 1),
            new PieceDrop(3, 0, 3),
            new PieceDrop(3, 0, 3),
            new PieceDrop(3, 0, 3),
            new PieceDrop(3, 0, 3),
            new PieceDrop(3, 0, 5),
            new PieceDrop(3, 0, 5),
            new PieceDrop(3, 0, 5),
            new PieceDrop(3, 0, 5),
            new PieceDrop(3, 0, 7),
            new PieceDrop(3, 0, 7),
            new PieceDrop(3, 0, 7),
            new PieceDrop(3, 0, 7),
            new PieceDrop(2, 2, 3),
            new PieceDrop(0, 1, 9),
            new PieceDrop(4, 1, 8),
        };
        PieceDrop[] result = new PieceDrop[sequence.Length];
        for (int i = 0; i < sequence.Length; i++)
        {
            byte o = (byte)Random.Range(0, 4);
            result[i] = new PieceDrop(sequence[i], o, (byte)Random.Range(0, 11 - new TetrisPiece(pieces[sequence[i]], o).Width));
        }
        return result;
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

    public TetrisPiece(Vector2Int solid1, Vector2Int solid2, Vector2Int solid3, Vector2Int solid4)
    {
        solid = new Vector2Int[4] { solid1, solid2, solid3, solid4 };
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

    public Vector2Int this[byte i] { get { return solid[i]; } }

    public override string ToString()
    {
        return string.Join(", ", solid);
    }
}