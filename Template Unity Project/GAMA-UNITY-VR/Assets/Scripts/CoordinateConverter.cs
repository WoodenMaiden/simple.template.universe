using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoordinateConverter 
{

    // optional: define a scale between GAMA and Unity for the location given
    public float GamaCRSCoefX = 1.0f;
    public float GamaCRSCoefY = 1.0f;

    public int precision;

    public CoordinateConverter(int p, float x, float y)
    {
        precision = p;
        GamaCRSCoefX = x;
        GamaCRSCoefY = y;
    }
    public Vector2 fromGAMACRS2D(int x, int y )
    {
        return new Vector2((GamaCRSCoefX * x) / precision, (GamaCRSCoefY * y) / precision);
    }
    public Vector3 fromGAMACRS(int x, int y)
    {
        return new Vector3((GamaCRSCoefX * x) / precision, 0.0f, (GamaCRSCoefY * y) / precision);
    }

    public List<int> toGAMACRS(Vector3 pos)
    {
        List<int> position = new List<int>();
        position.Add((int)(pos.x / GamaCRSCoefX * precision));
        position.Add((int)(pos.z / GamaCRSCoefY * precision));

        return position;
    }


}
