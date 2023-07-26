
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
    public float offsetYBackgroundGeom = 0.0f;

    private PolygonGenerator polyGen;

    private GAMAGeometry geoms;

    private bool continueProcess = true;
    public GAMAGeometryLoader(string ip_, int port_, float x, float y, float YOffset)
    {
        ip = ip_;
        port = port_;
        GamaCRSCoefX = x;
        GamaCRSCoefY = y;
        offsetYBackgroundGeom = YOffset;
        ConnectToTcpServer();


        while (continueProcess) {
            generateGeom();

        }
    }

    private void generateGeom()
    {
        if (parameters != null && geoms != null)
        {

            if (polyGen == null) polyGen = new PolygonGenerator(converter, offsetYBackgroundGeom);
            polyGen.GeneratePolygons(geoms);
            continueProcess = false;
        }
    }

    protected override void ManageMessage(string mes)
    {
        print(mes);
        if (mes.Contains("precision"))
        {
            parameters = ConnectionParameter.CreateFromJSON(mes);
            converter = new CoordinateConverter(parameters.precision, GamaCRSCoefX, GamaCRSCoefY);

            SendMessageToServer("ok\n");

        }
        else if (mes.Contains("points"))
        {
            geoms = GAMAGeometry.CreateFromJSON(mes);
            
        }
    }
}