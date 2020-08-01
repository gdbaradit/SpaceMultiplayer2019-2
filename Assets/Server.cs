using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class Server
{
    public static int MaxPlayers { get; private set; }
    public static int Port { get; private set; }
    public static Dictionary<int, Client> clients = new Dictionary<int, Client>();
    public delegate void PacketHandler(int _fromClient, Packet _packet);
    public static Dictionary<int, PacketHandler> packetHandlers;

    private static TcpListener tcpListener;
    private static UdpClient udpListener;

    /// <summary>Inicicar srvidor.</summary>
    /// <param name="_maxPlayers">maximo de jugadres simultaneos.</param>
    /// <param name="_port">puerto del servidor.</param>
    public static void Start(int _maxPlayers, int _port)
    {
        MaxPlayers = _maxPlayers;
        Port = _port;

        Debug.Log("Iniciando Servidor...");
        InitializeServerData();

        tcpListener = new TcpListener(IPAddress.Any, Port);
        tcpListener.Start();
        tcpListener.BeginAcceptTcpClient(TCPConnectCallback, null);

        udpListener = new UdpClient(Port);
        udpListener.BeginReceive(UDPReceiveCallback, null);

        Debug.Log($"Servidor iniciado con exito en Puerto: {Port}.");
    }

    /// <summary>Handles new TCP connections.</summary>
    private static void TCPConnectCallback(IAsyncResult _result)
    {
        TcpClient _client = tcpListener.EndAcceptTcpClient(_result);
        tcpListener.BeginAcceptTcpClient(TCPConnectCallback, null);
        Debug.Log($"Conexión entrante {_client.Client.RemoteEndPoint}...");

        for (int i = 1; i <= MaxPlayers; i++)
        {
            if (clients[i].tcp.socket == null)
            {
                clients[i].tcp.Connect(_client);
                return;
            }
        }

        Debug.Log($"{_client.Client.RemoteEndPoint} Fallo al conectar: Servidor lleno!");
    }

    /// <summary>recibiendo data UDP.</summary>
    private static void UDPReceiveCallback(IAsyncResult _result)
    {
        try
        {
            IPEndPoint _clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
            byte[] _data = udpListener.EndReceive(_result, ref _clientEndPoint);
            udpListener.BeginReceive(UDPReceiveCallback, null);

            if (_data.Length < 4)
            {
                return;
            }

            using (Packet _packet = new Packet(_data))
            {
                int _clientId = _packet.ReadInt();

                if (_clientId == 0)
                {
                    return;
                }

                if (clients[_clientId].udp.endPoint == null)
                {
                    // si es una conexion nueva
                    clients[_clientId].udp.Connect(_clientEndPoint);
                    return;
                }

                if (clients[_clientId].udp.endPoint.ToString() == _clientEndPoint.ToString())
                {
                    // verifica cliente
                    clients[_clientId].udp.HandleData(_packet);
                }
            }
        }
        catch (Exception _ex)
        {
            Debug.Log($"Error recibiendo data UDP: {_ex}");
        }
    }

    /// <summary>envia paquetes al punto de llegada via UDP.</summary>
    /// <param name="_clientEndPoint">punto de llegada.</param>
    /// <param name="_packet">paquete a enviar.</param>
    public static void SendUDPData(IPEndPoint _clientEndPoint, Packet _packet)
    {
        try
        {
            if (_clientEndPoint != null)
            {
                udpListener.BeginSend(_packet.ToArray(), _packet.Length(), _clientEndPoint, null, null);
            }
        }
        catch (Exception _ex)
        {
            Debug.Log($"Error enviando datos para: {_clientEndPoint} via UDP: {_ex}");
        }
    }

    /// <summary>inicializa datos en el servidor.</summary>
    private static void InitializeServerData()
    {
        for (int i = 1; i <= MaxPlayers; i++)
        {
            clients.Add(i, new Client(i));
        }

        packetHandlers = new Dictionary<int, PacketHandler>()
        {
            { (int)ClientPackets.welcomeReceived, ServerHandle.WelcomeReceived },
            { (int)ClientPackets.playerMovement, ServerHandle.PlayerMovement },
            { (int)ClientPackets.playerShoot, ServerHandle.PlayerShoot },
            { (int)ClientPackets.playerThrowItem, ServerHandle.PlayerThrowItem }
        };
        Debug.Log("Paquetes inicializados.");
    }

    public static void Stop()
    {
        tcpListener.Stop();
        udpListener.Close();
    }
}
