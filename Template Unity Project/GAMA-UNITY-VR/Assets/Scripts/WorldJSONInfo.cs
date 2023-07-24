using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class WorldJSONInfo
{
    public List<AgentInfo> agents;
    public List<int> position;


    public static WorldJSONInfo CreateFromJSON(string jsonString)
    {
        return JsonUtility.FromJson<WorldJSONInfo>(jsonString);
    }

}

[System.Serializable]
public class AgentInfo
{
    public List<int> v;

}