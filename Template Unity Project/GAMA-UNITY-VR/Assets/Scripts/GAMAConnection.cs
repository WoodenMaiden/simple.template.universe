using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net.Sockets;
using System.Threading;
using System.Timers;
using TMPro;
public class GlobalTest : TCPConnector
{

 
  
    public GameObject Player; 

    public GameObject Ground;

    public List<GameObject> Agents;

    //optional: rotation, Y-translation and Size scale to apply to the prefabs correspoding to the different species of agents
    public List<float> rotations = new List<float> { 90.0f, 90.0f, 0.0f };
    public List<float> rotationsCoeff = new List<float> { 1, 1, 0.0f };
    public List<float> YValues = new List<float> { -0.9f, -0.9f, 0.15f };
    public List<float> Sizefactor = new List<float> { 0.3f, 0.3f, 1.0f };

    // optional: define a scale between GAMA and Unity for the location given
    public float GamaCRSCoefX = 1.0f;
    public float GamaCRSCoefY = 1.0f;
    public float GamaCRSOffsetX = 0f;
    public float GamaCRSOffsetY = 0f;


    //Y scale for the ground
    public float groundY = 1.0f;

    //Y-offset to apply to the background geometries
    public float offsetYBackgroundGeom = 0.1f;



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

    private CoordinateConverter converter;

    private PolygonGenerator polyGen;

    // Start is called before the first frame update
    void Start()
    {
        DisplayMessage("Start");
       agentMapList = new List<Dictionary<int, GameObject>>();
        foreach (GameObject i in Agents) 
        {
            agentMapList.Add(
                new Dictionary<int, GameObject>());
        }
        Debug.Log("START WORLD");
        DisplayMessage("IP: " + ip + " port: " + port);
        ConnectToTcpServer();
    }




    private void Update()
    {
        //DisplayMessage("Update");
        if (text != null && message != null)
        {
            if (message.Contains("agents"))
            {
                DisplayMessage("Received agents: " + message.Length);
            }
        }
    }
    public int cpt = 0;
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
        cpt++;
       // DisplayMessage("parameters: " + parameters + " cpt:"  + cpt);

        if (parameters != null && geoms != null)
        {
            foreach (GAMAGeometry g in geoms)
            {
                if (polyGen == null) polyGen = new PolygonGenerator(converter, offsetYBackgroundGeom);
                polyGen.GeneratePolygons(g);
            }
            geoms = null;
        } 
        if (parameters != null && Ground != null && !defineGroundSize)
        {
            Vector3 ls = converter.fromGAMACRS(parameters.world[0], parameters.world[1]);
            if (ls.z < 0)
                ls.z = -ls.z;
            if (ls.x < 0)
                ls.x = -ls.x;
            ls.y = groundY;
            Ground.transform.localScale = ls;

            Vector3 ps = converter.fromGAMACRS(parameters.world[0]/2, parameters.world[1]/2);
            ps.y = -groundY;

            Ground.transform.position = ps;
            defineGroundSize = true;
            if (Player != null)
            {
                Vector3 pos = converter.fromGAMACRS(parameters.position[0], parameters.position[1]);
                Player.transform.position = pos;
            }
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

            SendMessageToServer("ready");

            return;
        }
       
        if (Player != null && playerPositionUpdate && parameters != null)
        {
           Player.transform.position = converter.fromGAMACRS(parameters.position[0], parameters.position[1]);
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

   
    private void SendPlayerPosition()
    {
        Vector2 vF = new Vector2(Camera.main.transform.forward.x, Camera.main.transform.forward.z);
        Vector2 vR = new Vector2(transform.forward.x, transform.forward.z);
        vF.Normalize();
        vR.Normalize();
        float c = vF.x * vR.x + vF.y * vR.y;
        float s = vF.x * vR.y - vF.y * vR.x;

        double angle = ((s > 0) ? -1.0 : 1.0) * (180 / Math.PI) * Math.Acos(c) * parameters.precision;

        List<int> p = converter.toGAMACRS(Player.transform.position); 
        SendMessageToServer("{\"position\":[" + p[0] + "," + p[1] + "],\"rotation\": " + (int)angle + "}");
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

            
             Vector3 pos = converter.fromGAMACRS(pi.v[2], pi.v[3]);
             pos.y = YValues[speciesIndex];
            float rot = rotationsCoeff[speciesIndex] * (pi.v[4] / parameters.precision) + rotations[speciesIndex];
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


   

    protected override void ManageMessage(string mes)
    {
        if (mes.Contains("precision"))
        {
            parameters = ConnectionParameter.CreateFromJSON(mes);
            converter = new CoordinateConverter(parameters.precision, GamaCRSCoefX, GamaCRSCoefY, GamaCRSOffsetX, GamaCRSOffsetY);
            SendMessageToServer("ok");
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

    }

    
}
