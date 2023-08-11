using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

using System.Net.Sockets;
using System.Threading;
using System.Text;
using TMPro;

public abstract class TCPConnector : MonoBehaviour
{
    public string ip = "localhost";
    public int port = 8000;
    public string endMessageSymbol = "&&&";
    private TcpClient socketConnection;
    private Thread clientReceiveThread;

    //optional, used for debugging
    public TMP_Text text = null;

    public void DisplayMessage(string message)
    {
        if (text != null)
            text.SetText(DateTime.Today + " - " + message);
    }

    protected void SendMessageToServer(string clientMessage)
    {
       //  Debug.Log("SendMessageToServer: " + clientMessage);

        if (socketConnection == null)
        {
            return;
        }
        try
        {
            // Get a stream object for writing. 			
            NetworkStream stream = socketConnection.GetStream();
           // stream.Flush();
            if (stream.CanWrite)
            {
                // Convert string message to byte array.                 
                byte[] clientMessageAsByteArray = Encoding.ASCII.GetBytes(clientMessage + "\n");

                // Write byte array to socketConnection stream.                 
                stream.Write(clientMessageAsByteArray, 0, clientMessageAsByteArray.Length);
                //stream.Flush();
                  Debug.Log("Client sent his message - should be received by server");
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
        //DisplayMessage("ListenForData");

        try
        {
           // Debug.Log("ListenForData : " + ip + "  " + port);

            socketConnection = new TcpClient(ip, port);
            
            //Debug.Log("socketConnection: " + socketConnection);

            //DisplayMessage("socketConnection: " + socketConnection);

            SendMessageToServer("connected");
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
                       // Debug.Log("serverMessage:" + serverMessage);
                        stream.Flush();
                        fullMessage += serverMessage;
                        if (fullMessage.Contains(endMessageSymbol))
                        {
                          //  Debug.Log("fullMessage:" + fullMessage);

                            string[] messages = fullMessage.Split(endMessageSymbol);
                          //  Debug.Log("messages.Length:" + messages.Length);
                            for (int i = 0; i < messages.Length - 1; i++)
                            {
                                string mes = messages[i];
                            //    Debug.Log("mes:" + mes);
                                ManageMessage(mes);
                            }
                            
                            fullMessage = messages[messages.Length - 1] != null ? messages[messages.Length - 1] : "";
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
        DisplayMessage("ConnectToTcpServer");

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
