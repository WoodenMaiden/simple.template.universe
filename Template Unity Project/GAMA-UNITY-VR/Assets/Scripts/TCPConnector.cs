using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

using System.Net.Sockets;
using System.Threading;
using System.Text;

public abstract class TCPConnector : MonoBehaviour
{
    public string ip = "192.168.0.186";
    public int port = 8000;
    private TcpClient socketConnection;
    private Thread clientReceiveThread;


    protected void SendMessageToServer(string clientMessage)
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

    protected virtual void ManageMessage(string message) { }

    protected void ListenForData()
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
                            ManageMessage(mes);
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


    protected void ConnectToTcpServer()
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


}
