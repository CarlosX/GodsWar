using Framework.Configuration;
using Framework.Constants;
using Framework.Networking;
using LoginServer.Server;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace LoginServer.Networking
{
    public class LoginSocketManager : SocketManager<LoginSocket>
    {
        public override bool StartNetwork(string bindIp, int port, int threadCount)
        {
            _tcpNoDelay = ConfigMgr.GetDefaultValue("Network.TcpNodelay", true);

            Log.outDebug(LogFilter.Misc, "Max allowed socket connections {0}", ushort.MaxValue);

            // -1 means use default
            _socketSendBufferSize = ConfigMgr.GetDefaultValue("Network.OutKBuff", -1);

            if (!base.StartNetwork(bindIp, port, threadCount))
                return false;

            _instanceAcceptor = new AsyncAcceptor();
            if (!_instanceAcceptor.Start(bindIp, LoginConfig.GetIntValue(LoginCfg.PortInstance)))
            {
                Log.outError(LogFilter.Network, "StartNetwork failed to start instance AsyncAcceptor");
                return false;
            }

            _instanceAcceptor.AsyncAcceptSocket(OnSocketOpen);

            return true;
        }

        public override void StopNetwork()
        {
            _instanceAcceptor.Close();
            base.StopNetwork();

            _instanceAcceptor = null;
        }

        public override void OnSocketOpen(Socket sock)
        {
            // set some options here
            try
            {
                if (_socketSendBufferSize >= 0)
                    sock.SendBufferSize = _socketSendBufferSize;

                // Set TCP_NODELAY.
                sock.NoDelay = _tcpNoDelay;
            }
            catch (SocketException ex)
            {
                Log.outException(ex);
                return;
            }

            base.OnSocketOpen(sock);
        }

        AsyncAcceptor _instanceAcceptor;
        int _socketSendBufferSize;
        bool _tcpNoDelay;
    }
}
