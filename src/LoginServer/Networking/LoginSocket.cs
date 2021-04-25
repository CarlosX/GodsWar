using Framework.Constants.Network;
using Framework.Cryptography;
using Framework.Database;
using Framework.IO;
using Framework.Logging;
using Framework.Networking;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace LoginServer.Networking
{
    public class LoginSocket : SocketBase
    {
        static readonly int HeaderSize = 2;

        SocketBuffer _headerBuffer;
        SocketBuffer _packetBuffer;
        long _LastPingTime;
        uint _OverSpeedPings;
        object _worldSessionLock = new();
        LoginSession _loginSession;
        LoginCrypt _loginCrypt;

        AsyncCallbackProcessor<QueryCallback> _queryProcessor = new();

        public LoginSocket(Socket socket) : base(socket)
        {
            _headerBuffer = new SocketBuffer(HeaderSize);
            _packetBuffer = new SocketBuffer(0);
            _loginCrypt = new LoginCrypt();

            _loginCrypt.Initialize();
        }

        public override void Dispose()
        {
            _loginSession = null;
            _queryProcessor = null;

            base.Dispose();
        }

        public override void Accept()
        {
            Console.WriteLine("Accept");
            _packetBuffer.Resize(32000);
            _packetBuffer.Reset();
            //AsyncReadWithCallback(InitializeHandler);
            AsyncRead();
        }

        bool ReadHeader()
        {
            PacketHeader header = new();
            header.Read(_headerBuffer.GetData());

            _packetBuffer.Resize(header.Size);
            return true;
        }

        void InitializeHandler(SocketAsyncEventArgs args)
        {
            if (args.SocketError != SocketError.Success)
            {
                CloseSocket();
                return;
            }

            if (args.BytesTransferred > 0)
            {
                if (_packetBuffer.GetRemainingSpace() > 0)
                {
                    // need to receive the header
                    int readHeaderSize = Math.Min(args.BytesTransferred, _packetBuffer.GetRemainingSpace());
                    _packetBuffer.Write(args.Buffer, 0, readHeaderSize);

                    if (_packetBuffer.GetRemainingSpace() > 0)
                    {
                        // Couldn't receive the whole header this time.
                        AsyncReadWithCallback(InitializeHandler);
                        return;
                    }

                    //ByteBuffer buffer = new(_packetBuffer.GetData());

                    _packetBuffer.Resize(0);
                    _packetBuffer.Reset();
                    HandleSendAuthSession();
                    AsyncRead();
                    return;
                }
            }
        }

        void HandleSendAuthSession()
        {
            Console.WriteLine("HandleSendAuthSession");
        }

        public override void ReadHandler(SocketAsyncEventArgs args)
        {
            if (!IsOpen())
                return;

            int currentReadIndex = 0;
            while (currentReadIndex < args.BytesTransferred)
            {
                _packetBuffer.Resize(args.BytesTransferred - currentReadIndex);

                // We have full read header, now check the data payload
                if (_packetBuffer.GetRemainingSpace() > 0)
                {
                    // need more data in the payload
                    int readDataSize = Math.Min(args.BytesTransferred - currentReadIndex, _packetBuffer.GetRemainingSpace());
                    _packetBuffer.Write(args.Buffer, currentReadIndex, readDataSize);
                    currentReadIndex += readDataSize;

                    if (_packetBuffer.GetRemainingSpace() > 0)
                        break; // Couldn't receive the whole data this time.
                }

                // just received fresh new payload
                ReadDataHandlerResult result = ReadData();
                _headerBuffer.Reset();
                if (result != ReadDataHandlerResult.Ok)
                {
                    if (result != ReadDataHandlerResult.WaitingForQuery)
                        CloseSocket();

                    return;
                }
            }

            AsyncRead();

        }

        ReadDataHandlerResult ReadData()
        {
            byte[] tmpBuff = _packetBuffer.GetData();
            if (!_loginCrypt.Decrypt(ref tmpBuff))
            {
                Log.outError(LogFilter.Network, $"WorldSocket.ReadData(): client {GetRemoteIpAddress()} failed to decrypt packet");
                return ReadDataHandlerResult.Error;
            }

            LogHex.HexDump(tmpBuff, "");

            PacketHeader header = new();
            header.Read(tmpBuff);

            LoginPacket packet = new(tmpBuff);
            _packetBuffer.Reset();

            if (packet.GetOpcode() >= (int)ClientOpcodes.Max)
            {
                Log.outError(LogFilter.Network, $"LoginSocket.ReadData(): client {GetRemoteIpAddress()} sent wrong opcode (opcode: {packet.GetOpcode()})");
                Log.outError(LogFilter.Network, $"Data: {_packetBuffer.GetData().ToHexString()}");
                return ReadDataHandlerResult.Error;
            }

            //PacketLog.Write(packet.GetData(), packet.GetOpcode(), GetRemoteIpAddress(), _connectType, true);

            ClientOpcodes opcode = (ClientOpcodes)packet.GetOpcode();

            if (!header.IsValidSize())
            {
                Log.outError(LogFilter.Network, $"LoginSocket.ReadHeaderHandler(): client {GetRemoteIpAddress()} sent malformed packet (size: {header.Size})");
                return ReadDataHandlerResult.Error;
            }

            switch (opcode)
            {
                case ClientOpcodes.MSG_LOGIN:
                    {
                        HandleLogin(packet);
                        break;
                    }

                default:
                    lock (_worldSessionLock)
                    {
                        if (_loginSession == null)
                        {
                            Log.outError(LogFilter.Network, $"ProcessIncoming: Client not authed opcode = {opcode}");
                            return ReadDataHandlerResult.Error;
                        }

                        if (!PacketManager.ContainsHandler(opcode))
                        {
                            Log.outError(LogFilter.Network, $"No defined handler for opcode {opcode} sent by {_loginSession.GetPlayerInfo()}");
                            break;
                        }

                        // Our Idle timer will reset on any non PING opcodes on login screen, allowing us to catch people idling.
                        _loginSession.ResetTimeOutTime(false);

                        // Copy the packet to the heap before enqueuing
                        _loginSession.QueuePacket(packet);
                    }
                    break;
            }

            return ReadDataHandlerResult.Ok;
        }


        public void HandleLogin(LoginPacket packet)
        {
            ByteBuffer packet1 = new();
            packet1.WriteUInt16(0x06);
            packet1.WriteUInt16(0x03);
            packet1.WriteBit(0);
            packet1.WriteBit(1);

            AsyncWrite(packet1.GetData());
        }

        public void SendPacket(ServerPacket packet)
        {
            if (!IsOpen())
                return;

            //packet.LogPacket(_loginSession);
            //packet.WritePacketData();

            var data = packet.GetData();
            ServerOpcodes opcode = packet.GetOpcode();
            //PacketLog.Write(data, (uint)opcode, GetRemoteIpAddress(), _connectType, false);

            short packetSize = (short)data.Length;

            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteUInt16((ushort)opcode);
            buffer.WriteBytes(data);
            packetSize += 2 /*opcode*/;

            data = buffer.GetData();

            PacketHeader header = new();
            header.Size = packetSize;
            _loginCrypt.Encrypt(ref data);

            ByteBuffer byteBuffer = new();
            header.Write(byteBuffer);
            byteBuffer.WriteBytes(data);

            AsyncWrite(byteBuffer.GetData());
        }
    }

    enum ReadDataHandlerResult
    {
        Ok = 0,
        Error = 1,
        WaitingForQuery = 2
    }
}
