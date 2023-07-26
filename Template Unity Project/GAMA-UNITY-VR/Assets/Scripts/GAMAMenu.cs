using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


public class GAMAMenu : ScriptableObject
{

    public string ip = "192.168.0.186";
    public int port = 8000;


    private const string CustomMenuBasePath = "GAMA Menu/";
	private const string LoadGeometriesPath = CustomMenuBasePath + "Load geometries from GAMA";

	[MenuItem(LoadGeometriesPath)]
	private static void LoadGeometries()
	{
        CreateGeometryImportationWaitingDialog();

    }


    static void CreateGeometryImportationWaitingDialog()
    {
        GAMAGeometryLoaderUI window = CreateInstance<GAMAGeometryLoaderUI>();
        window.position = new Rect(Screen.width / 2, Screen.height / 2, 300, 400);
        window.ShowUtility();
    }
}