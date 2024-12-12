using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class UdpReceiver : MonoBehaviour
{
    public GameObject pointPrefab;  // Cette ligne expose le prefab dans l'inspecteur

    UdpClient udpClient;
    int port = 5005;  // Port utilisé par la Raspberry Pi

    private float lastReceivedDistance = 0;

    void Start()
    {
        // Initialisation du client UDP
        udpClient = new UdpClient(port);
        udpClient.BeginReceive(OnReceive, null);
        Debug.Log("Listening for UDP data...");
    }

    void OnReceive(IAsyncResult result)
    {
        try
        {
            // Réception des données
            IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, port);
            byte[] receivedData = udpClient.EndReceive(result, ref remoteEndPoint);
            string receivedText = Encoding.UTF8.GetString(receivedData);

            if (float.TryParse(receivedText, out float receivedDistance))
            {
                lastReceivedDistance = receivedDistance;
                Debug.Log("Received distance: " + receivedDistance);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("UDP Receive Error: " + ex.Message);
        }

        // Redémarre l'écoute des messages UDP
        udpClient.BeginReceive(OnReceive, null);
    }

    void Update()
    {
        if (lastReceivedDistance > 0 && pointPrefab)
        {
            // Génère la position pour l'instanciation de la sphère
            Vector3 position = new Vector3(lastReceivedDistance, 0, 0);
            Instantiate(pointPrefab, position, Quaternion.identity);
        }
    }

    void OnDestroy()
    {
        if (udpClient != null)
        {
            udpClient.Close();  // Ferme correctement le client UDP
            Debug.Log("UDP client closed.");
        }
    }
}
