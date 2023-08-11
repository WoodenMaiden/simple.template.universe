using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameObjectListUIG : MonoBehaviour
{
    [HideInInspector]
    public GameObject[] objectsToExport = new GameObject[] {};

    public GameObject[] GetList() {
        return objectsToExport;
    }

}
