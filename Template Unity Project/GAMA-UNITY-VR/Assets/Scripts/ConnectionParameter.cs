using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class ConnectionParameter
{
    public int precision;
    public double delay;
    public List<int> position;
    public List<int> world;
    public bool physics;


    public static ConnectionParameter CreateFromJSON(string jsonString)
    {
        return JsonUtility.FromJson<ConnectionParameter>(jsonString);
    }

}