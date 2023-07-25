using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Timers;
using TMPro;
public class GlobalTest : MonoBehaviour
{

    public string ip = "192.168.0.186";
    public int port = 8000;

  
    public GameObject Player;

    public GameObject Ground;

    public List<GameObject> Agents;

    //optional: rotation, Y-translation and Size scale to apply to the prefabs correspoding to the different species of agents
    public List<float> rotations = new List<float> { 90.0f, 90.0f, 0.0f };
    public List<float> YValues = new List<float> { -0.9f, -0.9f, 0.15f };
    public List<float> Sizefactor = new List<float> { 0.3f, 0.3f, 1.0f };

    // optional: define a scale between GAMA and Unity for the location given
    public float GamaCRSCoefX = 1.0f;
    public float GamaCRSCoefY = 1.0f;


    //Y scale for the ground
    public float groundY = 1.0f;

    //Y-offset to apply to the background geometries
    public float offsetYBackgroundGeom = 0.1f;


    //optional, used for debugging
    public TMP_Text text = null;

    private List<Dictionary<int, GameObject>> agentMapList ;

    private TcpClient socketConnection;
    private Thread clientReceiveThread;

    private bool initialized = false;
    private bool playerPositionUpdate = false;

    private string message ="";

    private bool defineGroundSize = false;

    private static bool receiveInformation = true;

    private static bool timerFinish = false;

    private WorldJSONInfo infoWorld = null;

    private ConnectionParameter parameters = null;

    private List<GAMAGeometry> geoms;

    private static System.Timers.Timer aTimer;

    // Start is called before the first frame update
    void Start()
    {
        agentMapList = new List<Dictionary<int, GameObject>>();
        foreach (GameObject i in Agents) 
        {
            agentMapList.Add(
                new Dictionary<int, GameObject>());
        }
        Debug.Log("START WORLD");
        EstablishConnection();
    }

    public void EstablishConnection()
    {
        ConnectToTcpServer();
    }


    Vector2 fromGAMACRS2D(int x, int y)
    {
        return new Vector2((GamaCRSCoefX * x) / parameters.precision,(GamaCRSCoefY * y) / parameters.precision);
    }
    Vector3 fromGAMACRS(int x, int y)
    {
        return new Vector3((GamaCRSCoefX * x ) / parameters.precision, 0.0f, (GamaCRSCoefY * y) / parameters.precision);
    }

    List<int> toGAMACRS(Vector3 pos)
    {
        List<int> position = new List<int>();
        position.Add((int)(pos.x / GamaCRSCoefX * parameters.precision));
        position.Add((int)(pos.z/ GamaCRSCoefY * parameters.precision));

        return position;
    }

    


    private void Update()
    {
        if (text != null && message != null)
        {
            if (message.Contains("agents"))
            {
                DisplayMessage("Received agents: " + message.Length);
            }
        }
    }

    // Specify what you want to happen when the Elapsed event is raised.
    private static void OnTimedEvent(object source, ElapsedEventArgs e)
    {
        receiveInformation = true;
        timerFinish = true;
        aTimer.Stop();
        aTimer = null;

    }

    void FixedUpdate()
    {
        if(parameters != null && geoms != null)
        {
            foreach (GAMAGeometry g in geoms)
            {
                GeneratePolygons(g);
            }
            geoms = null;
        } 
        if (parameters != null && Ground != null && !defineGroundSize)
        {
            Ground.transform.localScale = new Vector3(parameters.world[0], groundY, parameters.world[1]);
            Ground.transform.position = new Vector3(parameters.world[0]/2.0f, -groundY, parameters.world[1] / 2.0f);
            defineGroundSize = true;
            if (parameters.physics && Player != null)
            {
                if (Player.GetComponent<Rigidbody>() == null)
                {
                    Player.AddComponent<Rigidbody>();
                }
            } else
            {
                if (Player.GetComponent<Rigidbody>() != null)
                {
                    Destroy(Player.GetComponent<Rigidbody>());
                }
            }
        }
        if (timerFinish)
        {
            timerFinish = false;

            SendMessageToServer("ready\n");

            return;
        }
       
        if (Player != null && playerPositionUpdate && parameters != null)
        {
           Player.transform.position = fromGAMACRS(parameters.position[0], parameters.position[1]);
            playerPositionUpdate = false;
            receiveInformation = false;
            if (parameters.delay > 0)
            {
                aTimer = new System.Timers.Timer(parameters.delay);
                aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);

                aTimer.AutoReset = false;
                aTimer.Enabled = true;
            } else
            {
                receiveInformation = true;
            }
           

        }
        if (initialized && Player != null && receiveInformation)
        {
            SendPlayerPosition();
        }
        if (infoWorld != null && receiveInformation)
        {
            UpdateAgentList();

            infoWorld = null;

        }
    }

    private void DisplayMessage(string message)
    {
        text.SetText(DateTime.Today + " - " + message);
    }

    private void SendPlayerPosition()
    {
        Vector2 vF = new Vector2(Camera.main.transform.forward.x, Camera.main.transform.forward.z);
        Vector2 vR = new Vector2(transform.forward.x, transform.forward.z);
        vF.Normalize();
        vR.Normalize();
        float c = vF.x * vR.x + vF.y * vR.y;
        float s = vF.x * vR.y - vF.y * vR.x;

        double angle = ((s > 0) ? -1.0 : 1.0) * (180 / Math.PI) * Math.Acos(c) * parameters.precision;

        List<int> p = toGAMACRS(Player.transform.position); 
        SendMessageToServer("{\"position\":[" + p[0] + "," + p[1] + "],\"rotation\": " + (int)angle + "}\n");
    }

    private void UpdateAgentList()
    {
        if (infoWorld.position.Count == 2)
        {
            parameters.position = infoWorld.position;
            playerPositionUpdate = true;

        }
        foreach (Dictionary<int, GameObject> agentMap in agentMapList) { 
            foreach (GameObject obj in agentMap.Values)
            {
                obj.SetActive(false);
            }
        }

       foreach (AgentInfo pi in infoWorld.agents)
        {
            int speciesIndex = pi.v[0];
            GameObject Agent = Agents[speciesIndex];
            int id = pi.v[1];
            GameObject obj = null;
            Dictionary<int, GameObject> agentMap = agentMapList[speciesIndex];
            if (!agentMap.ContainsKey(id))
            {
                obj = Instantiate(Agent);
                float scale = Sizefactor[speciesIndex];
                obj.transform.localScale = new Vector3(scale, scale, scale);
                obj.SetActive(true);

                agentMap.Add(id, obj);
            }
            else
            {
                obj = agentMap[id];
            }

            
             Vector3 pos = fromGAMACRS(pi.v[2], pi.v[3]);
             pos.y = YValues[speciesIndex];
            float rot = - (pi.v[4] / parameters.precision) + rotations[speciesIndex];
            obj.transform.SetPositionAndRotation(pos,Quaternion.AngleAxis(rot, Vector3.up));

            obj.SetActive(true);


        }
        foreach (Dictionary<int, GameObject> agentMap in agentMapList)
        {
            List<int> ids = new List<int>(agentMap.Keys);
            foreach (int id in ids)
            {
                GameObject obj = agentMap[id];
                if (!obj.activeSelf)
                {
                    obj.transform.position = new Vector3(0, -100, 0);
                    agentMap.Remove(id);
                    GameObject.Destroy(obj);
                }
            }
        }
    }


    private void SendMessageToServer(string clientMessage)
    {
       // Debug.Log("SendMessageToServer: " + clientMessage);

        if (socketConnection == null)
        {
            return;
        }
        try
        {
            // Get a stream object for writing. 			
            NetworkStream stream = socketConnection.GetStream();
            if (stream.CanWrite)
            {
                // Convert string message to byte array.                 
                byte[] clientMessageAsByteArray = Encoding.ASCII.GetBytes(clientMessage);

                // Write byte array to socketConnection stream.                 
                stream.Write(clientMessageAsByteArray, 0, clientMessageAsByteArray.Length);
              //  Debug.Log("Client sent his message - should be received by server");
            }
        }
        catch (SocketException socketException)
        {
            Debug.Log("Socket exception: " + socketException);
        }
    }

    private void ListenForData()
    {
        try
        {
            Debug.Log("ListenForData : " + ip + "  " + port);

            socketConnection = new TcpClient(ip, port);
            Debug.Log("socketConnection: " + socketConnection);

            SendMessageToServer("connected\n");
            Byte[] bytes = new Byte[1024];
            string fullMessage = "";
            while (true)
            {
                using (NetworkStream stream = socketConnection.GetStream())
                {
                    int length;
                    // Read incomming stream into byte arrary. 					

                    while ((length = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {

                        var incommingData = new byte[length];
                        Array.Copy(bytes, 0, incommingData, 0, length);
                        // Convert byte array to string message. 						
                        string serverMessage = Encoding.ASCII.GetString(incommingData);
                        fullMessage += serverMessage;
                        if (fullMessage.Contains("\n"))
                        {
                            string[] messages = fullMessage.Split("\n");
                            string mes = messages[0];

                            if (mes.Contains("precision"))
                            {
                                parameters = ConnectionParameter.CreateFromJSON(mes);
                                SendMessageToServer("ok\n");
                                initialized = true;
                                playerPositionUpdate = true;

                            }
                            else if (mes.Contains("points"))
                            {
                                GAMAGeometry g = GAMAGeometry.CreateFromJSON(mes);
                                if (geoms == null)
                                {
                                    geoms = new List<GAMAGeometry>();
                                }
                                geoms.Add(g);


                            }

                            else if (mes.Contains("agents") && parameters != null)
                            {
                                infoWorld = WorldJSONInfo.CreateFromJSON(mes);

                            }
                            if (text != null)
                                message = mes;
                            fullMessage = messages.Length > 1 ? messages[1] : "";  
                        }
                    }
                }
            }
        }

        catch (SocketException socketException)
        {
            Debug.Log("Socket exception: " + socketException);
        }
    }


    private void ConnectToTcpServer()
    {
        try
        {
            Debug.Log("ConnectToTcpServer");

            clientReceiveThread = new Thread(new ThreadStart(ListenForData));
            clientReceiveThread.IsBackground = true;
            clientReceiveThread.Start();
        }
        catch (Exception e)
        {
            Debug.Log("On client connect exception " + e);
        }
    }


    public void GeneratePolygons(GAMAGeometry geom)
    {

        List<Vector2> pts = new List<Vector2>();
        int cpt = 0;
        for (int i = 0; i < geom.points.Count; i++)
        {
            GAMAPoint pt = geom.points[i];
            if (pt.c.Count < 2)
            {
                if (pts.Count > 2)
                {
                    print("cpt: " + cpt);
                    GeneratePolygon(pts.ToArray(), geom.heights[cpt], geom.hasColliders[cpt]);
                }
                pts = new List<Vector2>();
                cpt++;
            }
            else
            {
                pts.Add(fromGAMACRS2D(pt.c[0], pt.c[1]));
            }
            
            
        }
    }

    // Start is called before the first frame update
    void GeneratePolygon(Vector2[] MeshDataPoints, float extrusionHeight, bool isUsingCollider)
    {
        bool is3D = true;
        bool isUsingBottomMeshIn3D = false;
        bool isOutlineRendered = false;

        // create new GameObject (as a child)
        GameObject polyExtruderGO = new GameObject();
        //polyExtruderGO.transform.parent = this.transform;

        // reference to setup example poly extruder 
        PolyExtruder polyExtruder;

        // add PolyExtruder script to newly created GameObject and keep track of its reference
        polyExtruder = polyExtruderGO.AddComponent<PolyExtruder>();

        // global PolyExtruder configurations
        polyExtruder.isOutlineRendered = isOutlineRendered;
        Vector3 pos = polyExtruderGO.transform.position;
        pos.y += offsetYBackgroundGeom;
        polyExtruderGO.transform.position = pos;
        polyExtruder.createPrism(polyExtruderGO.name, extrusionHeight, MeshDataPoints, Color.grey, is3D, isUsingBottomMeshIn3D, isUsingCollider);
        if (isUsingCollider) {
            MeshCollider mc = polyExtruderGO.AddComponent<MeshCollider>();
            mc.sharedMesh = polyExtruder.surroundMesh;


        }
       


    }
}
