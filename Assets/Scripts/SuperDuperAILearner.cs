using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SuperDuperAILearner : MonoBehaviour
{
    public TextAsset sequenceSource;
    [Header("Params")]
    public float correctShape = 2.0f;
    public float badShape = -5.0f;
    public float sideShape = 0.25f;
    public float clearLine = 3.0f;
    public float height = -1.5f;

    private float[][] param = new float[][] 
    { 
        new float[5] { 2, -3, 1.25f, 4, -1.5f },
        new float[5] { 2, -3, 1.25f, 2, -1.5f },
        new float[5] { 2, -2, 0.75f, 2, -2.0f },
        new float[5] { 3, -2, 1.25f, 4, -2.0f },
        new float[5] { 3, -4, 1.75f, 4, -1.5f },
        new float[5] { 2, -2, 1.25f, 3, -1.5f },
        new float[5] { 3, -3, 1.25f, 5, -2.0f },
        new float[5] { 2, -3, 1.25f, 4, -1.0f }
    };

    private Tetris game;

    private int fi = 1;
    private List<int> res = new List<int>();
    private List<float[]> resParams = new List<float[]>();

    private bool stop = false;

    private void Start()
    {
        //TestSimulate(InitSequence());
        while (File.Exists(GetDataFileName(fi)))
        {
            fi++;
        }
        for (int i = 0; i < 48; i++)
        {
            Simulate2(InitSequence());
        }
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

    private void TestSimulate(byte[] sequence)
    {
        correctShape = 1.0f;
        badShape = -1.0f;
        sideShape = 0.25f;
        clearLine = 1.0f;
        height = -1.0f;
        game = new Tetris(sequence, correctShape, badShape, sideShape, clearLine, height);
        //game = new Tetris(sequence, 1, -1, 0.25f, 1, -1);
        game.CalculateDrops();

        Debug.Log($"{game.MaxHeight},{correctShape},{badShape},{sideShape},{clearLine},{height}");
        return;
        correctShape = 1.0f;
        badShape = -1.0f;
        sideShape = 0.25f;
        clearLine = 1.0f;
        height = -0.5f;
        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                for (int k = 0; k < 4; k++)
                {
                    for (int l = 0; l < 5; l++)
                    {
                        for (int m = 0; m < 4; m++)
                        {
                            game = new Tetris(sequence, correctShape, badShape, sideShape, clearLine, height);
                            game.CalculateDrops();

                            Debug.Log($"{game.MaxHeight},{correctShape},{badShape},{sideShape},{clearLine},{height}");
                            height -= 0.5f;
                        }
                        clearLine += 1.0f;
                        height = -0.5f;
                    }
                    sideShape += 0.5f;
                    clearLine = 1.0f;
                    height = -0.5f;
                }
                badShape -= 1.0f;
                sideShape = 0.25f;
                clearLine = 1.0f;
                height = -0.5f;
            }

            correctShape += 1.0f;
            badShape = -1.0f;
            sideShape = 0.25f;
            clearLine = 1.0f;
            height = -0.5f;
        }
    }
    private void Simulate(byte[] sequence)
    {
        correctShape = 1.0f;
        badShape = -1.0f;
        sideShape = 0.25f;
        clearLine = 1.0f;
        height = -0.5f;
        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                for (int k = 0; k < 4; k++)
                {
                    for (int l = 0; l < 5; l++)
                    {
                        for (int m = 0; m < 4; m++)
                        {
                            game = new Tetris(sequence, correctShape, badShape, sideShape, clearLine, height);
                            game.CalculateDrops();

                            Debug.Log($"{game.MaxHeight},{correctShape},{badShape},{sideShape},{clearLine},{height}");
                            if (game.MaxHeight == 1) { stop = true; }
                            res.Add(game.MaxHeight);
                            resParams.Add(new float[] { correctShape, badShape, sideShape, clearLine, height });
                            height -= 0.5f;
                        }
                        clearLine += 1.0f;
                        height = -0.5f;
                    }
                    sideShape += 0.5f;
                    clearLine = 1.0f;
                    height = -0.5f;
                }
                badShape -= 1.0f;
                sideShape = 0.25f;
                clearLine = 1.0f;
                height = -0.5f;
            }

            correctShape += 1.0f;
            badShape = -1.0f;
            sideShape = 0.25f;
            clearLine = 1.0f;
            height = -0.5f;
        }
        // save test result to file
        StreamWriter writer = File.CreateText(GetDataFileName(fi));
        for (int n = 0; n < res.Count; n++)
        {
            writer.WriteLine($"{res[n]},{string.Join(",", resParams[n])}");
        }
        writer.Close();
        StreamWriter writer2 = File.CreateText(GetSequenceFileName(fi));
        for (int n = 0; n < sequence.Length; n++)
        {
            writer2.WriteLine(sequence[n]);
        }
        writer2.Close();
        res.Clear();
        resParams.Clear();
        fi++;
    }
    private void Simulate2(byte[] sequence)
    {
        for (float i = 2; i <= 3.05; i += 0.25f)
        {
            for (float j = -4; j <= -1.95; j += 0.25f)
            {
                for (float k = 0.75f; k <= 1.8; k += 0.25f)
                {
                    for (float l = 2; l <= 5.05; l += 0.25f)
                    {
                        for (float m = -2.5f; m <= -1.45; m += 0.25f)
                        {
                            game = new Tetris(sequence, i, j, k, l, m);
                            game.CalculateDrops();

                            Debug.Log($"{game.MaxHeight},{i},{j},{k},{l},{m}");
                            if (game.MaxHeight == 1) { stop = true; }
                            res.Add(game.MaxHeight);
                            resParams.Add(new float[] { i, j, k, l, m });
                        }
                    }
                }
            }
        }
        // save test result to file
        StreamWriter writer = File.CreateText(GetDataFileName(fi));
        for (int n = 0; n < res.Count; n++)
        {
            writer.WriteLine($"{res[n]},{string.Join(",", resParams[n])}");
        }
        writer.Close();
        StreamWriter writer2 = File.CreateText(GetSequenceFileName(fi));
        for (int n = 0; n < sequence.Length; n++)
        {
            writer2.WriteLine(sequence[n]);
        }
        writer2.Close();
        res.Clear();
        resParams.Clear();
        fi++;
    }

    private string GetDataFileName(int i)
    {
        return $"Assets/Data2/data{i:0000000}.txt";
    }
    private string GetSequenceFileName(int i)
    {
        return $"Assets/Data2/seq{i:0000000}.txt";
    }
}
