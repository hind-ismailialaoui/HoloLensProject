using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class UdpReceiver : MonoBehaviour
{
    UdpClient udpClient;
    int port = 5005; // Correspond au port utilisé par la Raspberry Pi

    void Start()
    {
        // Initialisation du client UDP
        udpClient = new UdpClient(port);
        udpClient.BeginReceive(OnReceive, null);
        Debug.Log("Listening for UDP data...");
    }

    void OnReceive(IAsyncResult result)
    {
        // Réception des données
        IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, port);
        byte[] receivedData = udpClient.EndReceive(result, ref remoteEndPoint);
        string receivedText = Encoding.UTF8.GetString(receivedData);
        Debug.Log("Received data: " + receivedText);

        // Redémarrage de l'écoute
        udpClient.BeginReceive(OnReceive, null);
    }

    void OnDestroy()
    {
        // Fermer le client UDP en cas de destruction de l'objet
        udpClient.Close();
    }
}
