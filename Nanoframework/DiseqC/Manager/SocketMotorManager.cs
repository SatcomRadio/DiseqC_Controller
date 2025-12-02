using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace DiseqC.Manager
{
    internal class SocketMotorManager
    {
        private readonly RotorManager _rotorMgr;
        private readonly Socket _socket;

        private double _lastAz;
        private double _lastEl;

        public SocketMotorManager(RotorManager rotorMgr)
        {
            _rotorMgr = rotorMgr;
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _socket.Bind(new IPEndPoint(IPAddress.Any, 5000));
        }

        public void Start()
        {
            _socket.Listen(10);
            var acceptConnectionsThread = new Thread(AcceptConnections);
            acceptConnectionsThread.Start();

            Debug.WriteLine($"Raw socket server is up and running, connect on: ws://localIP:5000");

        }

        public void AcceptConnections()
        {
            while (true)
            {
                var clientSocket = _socket.Accept();
                Debug.WriteLine("Client connected.");

                var clientThread = new Thread(() => HandleClient(clientSocket));
                clientThread.Start();
            }
        }

        private void HandleClient(Socket clientSocket)
        {
            using (clientSocket)
            {
                while (true)
                {
                    var buffer = new byte[1024];

                    // Receive data
                    var bytesRead = clientSocket.Receive(buffer);
                    var message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Debug.WriteLine($"Received: {message}");

                    if (message.StartsWith("P"))
                    {
                        var splitStr = message.Split(' ');
                        _lastAz = double.Parse(splitStr[1]);
                        _lastEl = double.Parse(splitStr[2]);

                        var finalAz = 0;
                        if (_lastAz < 270 && _lastAz > 90)
                            finalAz = (int)_lastAz - 180;

                        _rotorMgr.TrackAndGoToAngle(finalAz);

                        Debug.WriteLine($"Moving to: {finalAz}");
                        clientSocket.Send(Encoding.UTF8.GetBytes("0"));
                    }

                    else switch (message)
                    {
                        case "S":
                            clientSocket.Send(Encoding.UTF8.GetBytes("0"));
                            _rotorMgr.StopTracking();
                            break;
                        case "p":
                            clientSocket.Send(Encoding.UTF8.GetBytes($"{_lastAz}\n{_lastEl}\n"));
                            break;
                        default:
                            clientSocket.Send(Encoding.UTF8.GetBytes("0"));
                            break;
                        }
                }
            }
        }
    }
}
