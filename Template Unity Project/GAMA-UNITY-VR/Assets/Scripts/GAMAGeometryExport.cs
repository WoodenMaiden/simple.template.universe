
using UnityEngine;
public class GAMAGeometryExport : TCPConnector
{

    private ConnectionParameter parameters = null;
    // optional: define a scale between GAMA and Unity for the location given
    public float GamaCRSCoefX = 1.0f;
    public float GamaCRSCoefY = 1.0f;
    public float GamaCRSOffsetX = 0.0f;
    public float GamaCRSOffsetY = 0.0f;

    private bool continueProcess = true;
    private bool ok = false;


    public void ManageGeometries(GameObject[] objectsToSend_, string ip_, int port_, float x, float y, float ox, float oy)
    {
        Debug.Log("ManageGeometries");
        ok = false;
        ip = ip_;
        port = port_;
        GamaCRSCoefX = x;
        GamaCRSCoefY = y;
        GamaCRSOffsetX = ox;
        GamaCRSOffsetY = oy;
        parameters = null;
        ConnectToTcpServer();
        continueProcess = true;

        System.Threading.Thread.Sleep(5000);


        while (continueProcess)
        {
            ExportGeoms(objectsToSend_);

      }
    }

    
    private void ExportGeoms(GameObject[] objectsToSend)
    {
        if (parameters != null && objectsToSend != null)
        {
            string message = "";
            CoordinateConverter converter = new CoordinateConverter(parameters.precision, GamaCRSCoefX, GamaCRSCoefY, GamaCRSOffsetX, GamaCRSOffsetY);
            UnityGeometry ug = new UnityGeometry(objectsToSend, converter);
            message = ug.ToJSON();
            SendMessageToServer(message + "\n");
            if (ok)
                continueProcess = false;

        }
    }

    protected override void ManageMessage(string mes)
    {
        if (parameters == null && mes.Contains("precision"))
        {
           
            parameters = ConnectionParameter.CreateFromJSON(mes);
            
            SendMessageToServer("ok\n");

        } else if (mes.Contains("ok"))
        {
            Debug.Log("received OK");
            ok = true;
        }
       
    }
}