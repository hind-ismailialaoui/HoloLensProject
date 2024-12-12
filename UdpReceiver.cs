using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System;

public class UdpReceiver : MonoBehaviour
{
    UdpClient udpClient;
    int localPort = 5005; // Assure-toi que ce port correspond à celui du script Python
    bool isListening = true;

    void Start()
    {
        try
        {
            // Initialisation du client UDP
            udpClient = new UdpClient(localPort);
            udpClient.BeginReceive(OnReceive, null);
            Debug.Log("Listening for UDP data...");
        }
        catch (SocketException ex)
        {
            Debug.LogError($"SocketException: {ex.Message}");
        }
    }

    void OnReceive(IAsyncResult result)
    {
        if (!isListening || udpClient == null)
            return;

        try
        {
            // Réception des données
            IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, localPort);
            byte[] receivedData = udpClient.EndReceive(result, ref remoteEndPoint);
            string receivedText = Encoding.UTF8.GetString(receivedData);
            Debug.Log("Received data: " + receivedText);

            // Conversion et instanciation des sphères
            if (float.TryParse(receivedText, out float distance))
            {
                Vector3 position = new Vector3(distance, 0, 0);
                Instantiate(pointPrefab, position, Quaternion.identity);
            }

            // Redémarrage de l'écoute
            udpClient.BeginReceive(OnReceive, null);
        }
        catch (ObjectDisposedException)
        {
            Debug.LogError("UDP Receive Error: Cannot access a disposed object.");
        }
        catch (SocketException ex)
        {
            Debug.LogError($"SocketException: {ex.Message}");
        }
    }

    void OnDestroy()
    {
        // Nettoyage du client UDP
        isListening = false;
        udpClient?.Close();
        udpClient = null;
    }
}
