using Framework.Constants;
using Framework.Constants.Network;
using Framework.Cryptography;
using Framework.Database;
using Framework.IO;
using Framework.Logging;
using Framework.Networking;
using Game.Networking.Packets.Client;
using Game.Networking.Packets.Server;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace Game.Networking
{
    public class WorldSocket : SocketBase
    {
        static readonly int HeaderSize = 2;

        SocketBuffer _headerBuffer;
        SocketBuffer _packetBuffer;
        long _LastPingTime;
        uint _OverSpeedPings;
        object _worldSessionLock = new();
        WorldSession _loginSession;
        LoginCrypt _loginCrypt;
        uint hashPointRecv = 0;
        uint hashPointSend = 0;

        AsyncCallbackProcessor<QueryCallback> _queryProcessor = new();

        public WorldSocket(Socket socket) : base(socket)
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

        public override bool Update()
        {
            if (!base.Update())
                return false;

            _queryProcessor.ProcessReadyCallbacks();

            return true;
        }

        public override void Accept()
        {
            Console.WriteLine("Accept");
            _packetBuffer.Resize(0);
            _packetBuffer.Reset();
            //AsyncReadWithCallback(InitializeHandler);
            AsyncRead();
        }


        bool ReadHeader()
        {
            PacketHeader header = new();
            header.Read(_headerBuffer.GetData());

            _packetBuffer.Resize(header.Size - HeaderSize);
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
                /*if (_packetBuffer.GetRemainingSpace() > 0)
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
                }*/
                ByteBuffer buffer = new(_packetBuffer.GetData());
                _packetBuffer.Resize(0);
                _packetBuffer.Reset();
                AsyncRead();
                return;
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
                byte[] tmpBuff = new byte[args.BytesTransferred];

                Buffer.BlockCopy(args.Buffer, 0, tmpBuff, 0, args.BytesTransferred);
                if (!_loginCrypt.Decrypt(ref tmpBuff, ref hashPointRecv))
                {
                    Log.outError(LogFilter.Network, $"WorldSocket.ReadData(): client {GetRemoteIpAddress()} failed to decrypt packet");
                    return;
                }

                //LogHex.HexDump(tmpBuff, "", 16, 20);

                if (_headerBuffer.GetRemainingSpace() > 0)
                {
                    // need to receive the header
                    int readHeaderSize = Math.Min(args.BytesTransferred - currentReadIndex, _headerBuffer.GetRemainingSpace());
                    _headerBuffer.Write(tmpBuff, currentReadIndex, readHeaderSize);
                    currentReadIndex += readHeaderSize;

                    if (_headerBuffer.GetRemainingSpace() > 0)
                        break; // Couldn't receive the whole header this time.

                    // We just received nice new header
                    if (!ReadHeader())
                    {
                        CloseSocket();
                        return;
                    }
                }
                else
                {
                    Log.outInfo(LogFilter.Server, "Error Read Header");
                }

                // We have full read header, now check the data payload
                if (_packetBuffer.GetRemainingSpace() > 0)
                {
                    // need more data in the payload
                    int readDataSize = Math.Min(args.BytesTransferred - currentReadIndex, _packetBuffer.GetRemainingSpace());
                    _packetBuffer.Write(tmpBuff, currentReadIndex, readDataSize);
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

            WorldPacket packet = new(tmpBuff);
            _packetBuffer.Reset();

            if (packet.GetOpcode() >= (int)ClientOpcodes.Max)
            {
                Log.outError(LogFilter.Network, $"LoginSocket.ReadData(): client {GetRemoteIpAddress()} sent wrong opcode (opcode: {packet.GetOpcode()})");
                Log.outError(LogFilter.Network, $"Data: {_packetBuffer.GetData().ToHexString()}");
                return ReadDataHandlerResult.Error;
            }

            LogHex.HexDump(tmpBuff, "Recv", 16);

            //PacketLog.Write(packet.GetData(), packet.GetOpcode(), GetRemoteIpAddress(), _connectType, true);

            ClientOpcodes opcode = (ClientOpcodes)packet.GetOpcode();

            Log.outInfo(LogFilter.Server, "Opcode {0:X} {1}", opcode, opcode);

            switch (opcode)
            {
                case ClientOpcodes.MSG_LOGIN_GAMESERVER:
                    {
                        AuthSession auth = new(packet);
                        auth.Read();
                        HandleAuthSession(auth);
                        return ReadDataHandlerResult.WaitingForQuery;
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


        public void HandleAuthSession(AuthSession _packet)
        {
            /*PreparedStatement stmt = DB.Login.GetPreparedStatement(LoginStatements.SEL_ACCOUNT_INFO_BY_USERNAME);
            stmt.AddValue(0, _packet.username);

            _queryProcessor.AddCallback(DB.Login.AsyncQuery(stmt).WithCallback(HandleAuthSessionCallback, _packet));*/
            HandleAuthSessionCallback(_packet, null);
        }

        private void HandleAuthSessionCallback(AuthSession _packet, SQLResult result)
        {
            /*// Stop if the account is not found
            if (result.IsEmpty())
            {
                Log.outError(LogFilter.Network, "HandleAuthSession: Sent Auth Response (unknown account).");
                CloseSocket();
                return;
            }

            InvalidCredentialPacket authServer = new();
            AccountInfo account = new(result.GetFields());
            
            if (!account.Password.Equals(_packet.password))
            {
                Log.outInfo(LogFilter.Server, $"Password: {account.Password} !== {_packet.password}");
                authServer.reason = ResponseCodes.ERROR_PASSWORD;
                authServer.code = 0;
                SendPacket(authServer);
                CloseSocket();
                return;
            }*/

            InvalidCredentialPacket authServer = new();

            _loginSession = new WorldSession(1, "carlosx", this);
            /*
             * 0: el ID no está registrado; 
             * 1: el inicio de sesión es exitoso; 
             * 2: inicio de sesión repetido; 
             * 3: error de contraseña; 
             * 4: error de versión 
             */
            authServer.reason = ResponseCodes.SESSION_SUCCESS;
            authServer.code = 0;

            //SendPacket(authServer);
            Global.WorldMgr.AddSession(_loginSession);

            var ret = new ServerAuthResponsePacket();
            SendPacket(ret);

            var ret2 = new CharacterInfoResponsePacket();
            SendPacket(ret2);

            /*var serverList = new ServerListPacket();
            SendPacket(serverList);*/

            AsyncRead();
        }

        public void SendPacket(ServerPacket packet)
        {
            if (!IsOpen())
                return;

            //packet.LogPacket(_loginSession);
            packet.WritePacketData();

            var data = packet.GetData();
            //LogHex.HexDump(data, "SendPacket");
            ServerOpcodes opcode = packet.GetOpcode();
            //PacketLog.Write(data, (uint)opcode, GetRemoteIpAddress(), _connectType, false);

            short packetSize = (short)data.Length;

            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteUInt16((ushort)opcode);
            buffer.WriteBytes(data);
            packetSize += 4 /*size+opcode*/;

            data = buffer.GetData();

            PacketHeader header = new();
            header.Size = packetSize;

            ByteBuffer byteBuffer = new();
            header.Write(byteBuffer);
            byteBuffer.WriteBytes(data);

            byte[] tmpBuff = byteBuffer.GetData();
            LogHex.HexDump(tmpBuff, "SendPacket1");
            _loginCrypt.Encrypt(ref tmpBuff, ref hashPointSend);
            LogHex.HexDump(tmpBuff, "SendPacket2");
            AsyncWrite(tmpBuff);
        }

        public void SendPacket(byte[] packet)
        {
            _loginCrypt.Encrypt(ref packet, ref hashPointSend);
            AsyncWrite(packet);
        }
    }

    enum ReadDataHandlerResult
    {
        Ok = 0,
        Error = 1,
        WaitingForQuery = 2
    }

    class AccountInfo
    {
        public AccountInfo(SQLFields fields)
        {
            Id = fields.Read<uint>(0);
            Username = fields.Read<string>(1);
            Password = fields.Read<string>(2);
        }
        public uint Id;
        public string Username;
        public string Password;
    }
}
