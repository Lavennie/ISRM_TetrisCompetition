using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SuperDuperAILearner : MonoBehaviour
{
    [Header("Params")]
    public float correctShape = 2.0f;
    public float badShape = -5.0f;
    public float sideShape = 0.25f;
    public float clearLine = 3.0f;
    public float height = -1.5f;

    private Tetris game;

    private int fi = 1;
    private List<int> res = new List<int>();
    private List<float[]> resParams = new List<float[]>();

    private void Start()
    {
        correctShape = 1.0f;
        badShape = -1.0f;
        sideShape = 0.25f;
        clearLine = 1.0f;
        height = -0.5f;
        for (int i = 0; i < 100; i++)
        {
            Simulate(InitSequence());
        }
    }
    private byte[] InitSequence()
    {
        byte[] result = new byte[1000];
        for (int i = 0; i < result.Length; i++)
        {
            result[i] = (byte)Random.Range(0, 7);
        }
        return result;
    }

    private void Simulate(byte[] sequence)
    {
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

                            Debug.Log($"{game.MaxHeight} -> {correctShape}, {badShape}, {sideShape}, {clearLine}, {height}");
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
            int[] r = res.ToArray();
            float[][] rp = resParams.ToArray();
            System.Array.Sort<int, float[]>(r, rp);
            StreamWriter writer = File.CreateText($"Assets/data 0{fi}.txt");
            for (int n = 0; n < r.Length; n++)
            {
                writer.WriteLine($"{r[n]} -> {string.Join(", ", rp[n])}");
            }
            writer.Close();
            res.Clear();
            resParams.Clear();
            fi++;

            correctShape += 1.0f;
            badShape = -1.0f;
            sideShape = 0.25f;
            clearLine = 1.0f;
            height = -0.5f;
        }
    }
}
