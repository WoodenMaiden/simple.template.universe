
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Diagnostics;

public class GAMAGeometryExportUI : EditorWindow
{
    public string ip = "localhost";
    public int port = 8000;
    public float GamaCRSCoefX = 1.0f;
    public float GamaCRSCoefY = 1.0f;
    public float GamaCRSOffsetX = 0.0f;
    public float GamaCRSOffsetY = 0.0f;

    private GAMAGeometryExport exporter;
   
    private const string _helpText = "Cannot find GameObjects to export!";
    private static Rect _helpRect = new Rect(0f, 0f, 600f, 300f);
    private static Vector2 _windowsMinSize = Vector2.one * 600f;
    private static Rect _listRect = new Rect(Vector2.zero, _windowsMinSize);
    SerializedObject _objectSO = null;
    ReorderableList _listRE = null;
    GameObjectListUIG _gameObjects;

    

     
    void OnEnable()
    {
        _gameObjects = FindAnyObjectByType<GameObjectListUIG>();

        if (!_gameObjects) 
        {
            return;
        }
        _objectSO = new SerializedObject(_gameObjects);
        _listRE = new ReorderableList(_objectSO, _objectSO.FindProperty("objectsToExport"), true, true, true, true);

        _listRE.drawHeaderCallback = (rect) => EditorGUI.LabelField(rect, "Game Objects");
        _listRE.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            rect.y += 2f;
            rect.height = EditorGUIUtility.singleLineHeight;

            GUIContent objectLabel = new GUIContent("GameObject {" + index + "}");
            EditorGUI.PropertyField(rect, _listRE.serializedProperty.GetArrayElementAtIndex(index), objectLabel);
        };
    }

    private void OnGUI()
    {
        if (_objectSO == null)
        {
            EditorGUI.HelpBox(_helpRect, _helpText, MessageType.Warning);
            return;
        }
        else if (_objectSO != null)
        {
            _objectSO.Update();
            _listRE.DoList(_listRect);
            _objectSO.ApplyModifiedProperties();
        }
        GUILayout. Space(_listRE.GetHeight ( ) + 30f) ;
        GUILayout. Label( "Please select Game Objects to export");
        GUILayout. Space(30f) ;
        
        GUILayout.Space(10);
        ip = EditorGUILayout.TextField("IP: ", ip);
        GUILayout.Space(10);
        port = int.Parse(EditorGUILayout.TextField("Port: ", port + ""));
        GUILayout.Space(10);
        GamaCRSCoefX = float.Parse(EditorGUILayout.TextField("X-scaling: ", GamaCRSCoefX + ""));
        GUILayout.Space(10);
        GamaCRSCoefY = float.Parse(EditorGUILayout.TextField("Z-scaling: ", GamaCRSCoefY + ""));
        GUILayout.Space(10);
        GamaCRSOffsetX = float.Parse(EditorGUILayout.TextField("X-Offset: ", GamaCRSOffsetX + ""));
        GUILayout.Space(10);
        GamaCRSOffsetY = float.Parse(EditorGUILayout.TextField("Z-Offset: ", GamaCRSOffsetY + ""));

        GUILayout.Space(30);

        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(10);


        if (GUILayout. Button( "Export geometries" ))
        {
            //EditorUtility.DisplayDialog("Exporting of Geometries",
             //     "Waiting for exporting geometries to GAMA", "Ok");

            exporter = FindAnyObjectByType<GAMAGeometryExport>();

           
            exporter.ManageGeometries(_gameObjects.GetList(), ip, port, GamaCRSCoefX, GamaCRSCoefY, GamaCRSOffsetX, GamaCRSOffsetY);

            //Close();
        }
        GUILayout.Space(30f);
        if (GUILayout. Button("Cancel" )) {
            Close();
        }
        GUILayout. Space(30f) ;
        EditorGUILayout. EndHorizontal();
    }

    private void OnInspectorUpdate()
    {
        Repaint();
    }

    
}