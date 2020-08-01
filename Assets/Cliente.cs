using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class Client
{
    public static int dataBufferSize = 4096;

    public int id;
    public Player player;
    public TCP tcp;
    public UDP udp;

    public Client(int _clientId)
    {
        id = _clientId;
        tcp = new TCP(id);
        udp = new UDP(id);
    }

    public class TCP
    {
        public TcpClient socket;

        private readonly int id;
        private NetworkStream stream;
        private Packet receivedData;
        private byte[] receiveBuffer;

        public TCP(int _id)
        {
            id = _id;
        }
        /// prepara socket de conexion
        /// <parametro name="_socket">paquete a enviar.</param>
        public void Connect(TcpClient _socket)
        {
            socket = _socket;
            socket.ReceiveBufferSize = dataBufferSize;
            socket.SendBufferSize = dataBufferSize;

            stream = socket.GetStream();

            receivedData = new Packet();
            receiveBuffer = new byte[dataBufferSize];

            stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);

            ServerSend.Welcome(id, "Conexión Exitosa!");
        }

        /// enviar data al cliente via TCP
        /// <parametro name="_packet">paquete a enviar.</param>
        public void SendData(Packet _packet)
        {
            try
            {
                if (socket != null)
                {
                    stream.BeginWrite(_packet.ToArray(), 0, _packet.Length(), null, null);
                }
            }
            catch (Exception _ex)
            {
                Debug.Log($"Error enviando data al jugador {id} via TCP: {_ex}");
            }
        }

        /// <summary>recibiendo data.</summary>
        private void ReceiveCallback(IAsyncResult _result)
        {
            try
            {
                int _byteLength = stream.EndRead(_result);
                if (_byteLength <= 0)
                {
                    Server.clients[id].Disconnect();
                    return;
                }

                byte[] _data = new byte[_byteLength];
                Array.Copy(receiveBuffer, _data, _byteLength);

                receivedData.Reset(HandleData(_data)); // Reset data si esta ya fue recibida
                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
            }
            catch (Exception _ex)
            {
                Debug.Log($"Error receiving TCP data: {_ex}");
                Server.clients[id].Disconnect();
            }
        }

        /// <summary>recibir data.</summary>
        /// <param name="_data">recibir data.</param>
        private bool HandleData(byte[] _data)
        {
            int _packetLength = 0;

            receivedData.SetBytes(_data);

            if (receivedData.UnreadLength() >= 4)
            {
                // Verificar si cliente recibio data
                _packetLength = receivedData.ReadInt();
                if (_packetLength <= 0)
                {
                    // verifica si paquete esta vacio
                    return true; // Reset receivedData para permitir reusar
                }
            }

            while (_packetLength > 0 && _packetLength <= receivedData.UnreadLength())
            {
                // mientras paquete contenga data y no exceda length permitido
                byte[] _packetBytes = receivedData.ReadBytes(_packetLength);
                ThreadManager.ExecuteOnMainThread(() =>
                {
                    using (Packet _packet = new Packet(_packetBytes))
                    {
                        int _packetId = _packet.ReadInt();
                        Server.packetHandlers[_packetId](id, _packet); // llamada de metodo para manejo de paquete
                    }
                });

                _packetLength = 0; // Reset length del paquete
                if (receivedData.UnreadLength() >= 4)
                {
                    // If clientes reciben data de otros paquetes
                    _packetLength = receivedData.ReadInt();
                    if (_packetLength <= 0)
                    {
                        // If packet esta vacio
                        return true; // Reset receivedData para permitir reusar
                    }
                }
            }

            if (_packetLength <= 1)
            {
                return true; // Reset receivedData para permitir reusar
            }

            return false;
        }

        /// <summary>limpia y cerrar conexiones TCP.</summary>
        public void Disconnect()
        {
            socket.Close();
            stream = null;
            receivedData = null;
            receiveBuffer = null;
            socket = null;
        }
    }

    public class UDP
    {
        public IPEndPoint endPoint;

        private int id;

        public UDP(int _id)
        {
            id = _id;
        }

        /// <summary>Innicializa Nueva conexion CLient UDP.</summary>
        /// <param name="_endPoint">IPEndPoint instancia la nueva conexion.</param>
        public void Connect(IPEndPoint _endPoint)
        {
            endPoint = _endPoint;
        }

        /// <summary>enviar data al cliente via UDP.</summary>
        /// <param name="_packet">packet que se va a enviar.</param>
        public void SendData(Packet _packet)
        {
            Server.SendUDPData(endPoint, _packet);
        }

        /// <summary>preparar metodo de manejo de paquete.</summary>
        /// <param name="_packetData">packet contiene la data recibida.</param>
        public void HandleData(Packet _packetData)
        {
            int _packetLength = _packetData.ReadInt();
            byte[] _packetBytes = _packetData.ReadBytes(_packetLength);

            ThreadManager.ExecuteOnMainThread(() =>
            {
                using (Packet _packet = new Packet(_packetBytes))
                {
                    int _packetId = _packet.ReadInt();
                    Server.packetHandlers[_packetId](id, _packet); // llamada de metodo para manejo de paquete
                }
            });
        }

        /// <summary>limpiar conexion UDP.</summary>
        public void Disconnect()
        {
            endPoint = null;
        }
    }

    /// <summary>envia informacion a clientes conectados sobre nuevo jugador.</summary>
    /// <param name="_playerName">nombre de usuario.</param>
    public void SendIntoGame(string _playerName)
    {
        player = NetworkManager.instance.InstantiatePlayer();
        player.Initialize(id, _playerName);


        foreach (Client _client in Server.clients.Values)
        {
            if (_client.player != null)
            {
                if (_client.id != id)
                {
                    ServerSend.SpawnPlayer(id, _client.player);
                }
            }
        }


        foreach (Client _client in Server.clients.Values)
        {
            if (_client.player != null)
            {
                ServerSend.SpawnPlayer(_client.id, player);
            }
        }

        foreach (ItemSpawner _itemSpawner in ItemSpawner.spawners.Values)
        {
            ServerSend.CreateItemSpawner(id, _itemSpawner.spawnerId, _itemSpawner.transform.position, _itemSpawner.hasItem);
        }

        foreach (Enemy _enemy in Enemy.enemies.Values)
        {
            ServerSend.SpawnEnemy(id, _enemy);
        }
    }

    /// <summary>desconexion y stop de trafico.</summary>
    private void Disconnect()
    {
        Debug.Log($"{tcp.socket.Client.RemoteEndPoint} Desconexión Exitosa.");

        ThreadManager.ExecuteOnMainThread(() =>
        {
            UnityEngine.Object.Destroy(player.gameObject);
            player = null;
        });

        tcp.Disconnect();
        udp.Disconnect();

        ServerSend.PlayerDisconnected(id);
    }
}
