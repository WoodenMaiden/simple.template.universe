
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class GAMAGeometryLoader : TCPConnector
{

    private ConnectionParameter parameters = null;
    private CoordinateConverter converter;
    // optional: define a scale between GAMA and Unity for the location given
    public float GamaCRSCoefX = 1.0f;
    public float GamaCRSCoefY = 1.0f;
    public float GamaCRSOffsetX = 0.0f;
    public float GamaCRSOffsetY = 0.0f;
    public float offsetYBackgroundGeom = 0.0f;

    private PolygonGenerator polyGen;

    private GAMAGeometry geoms;

    private bool continueProcess = true;
    

    public void GenerateGeometries(string ip_, int port_, float x, float y, float ox, float oy, float YOffset)
    {
        ip = ip_;
        port = port_;
        GamaCRSCoefX = x;
        GamaCRSCoefY = y;
        GamaCRSOffsetX = ox;
        GamaCRSOffsetY = oy;
        offsetYBackgroundGeom = YOffset;
        ConnectToTcpServer();

        continueProcess = true;
        while (continueProcess) {
            Debug.Log("continueProcess:" + continueProcess);

            generateGeom();

        }
    }

    private void generateGeom()
    {

        Debug.Log("parameters:" + parameters + " geoms:" + geoms);
        if (parameters != null && converter != null && geoms != null)
        {
            Debug.Log("ici:" + converter);

            if (polyGen == null) polyGen = new PolygonGenerator(converter, offsetYBackgroundGeom);
            polyGen.GeneratePolygons(geoms);
            continueProcess = false;
        }
    }

    protected override void ManageMessage(string mes)
    {
        Debug.Log("mes:" + mes);
        if (mes.Contains("precision"))
        {
            parameters = ConnectionParameter.CreateFromJSON(mes);
            converter = new CoordinateConverter(parameters.precision, GamaCRSCoefX, GamaCRSCoefY, GamaCRSOffsetX, GamaCRSOffsetY);

            SendMessageToServer("ok\n");


        }
       

        else if (mes.Contains("points"))
        {
            geoms = GAMAGeometry.CreateFromJSON(mes);

            
        }
    }
}