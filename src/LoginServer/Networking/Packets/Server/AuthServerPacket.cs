using Framework.Constants;
using Framework.Constants.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoginServer.Networking.Packets.Server
{
    public class AuthServerPacket : ServerPacket
    {
        public AuthServerPacket() : base(ServerOpcodes.MSG_LOGIN_RETURN_INFO) { }

        public override void Write()
        {
            _loginPacket.WriteByte(0);
            _loginPacket.WriteByte(result);
        }

        public byte result { set; get; }
    }

    public class InvalidCredentialPacket : ServerPacket
    {
        public InvalidCredentialPacket() : base(ServerOpcodes.MSG_INVALID_CREDENTIAL) { }
        public override void Write()
        {
            _loginPacket.WriteByte((byte)reason);
            _loginPacket.WriteByte(code);
        }
        public ResponseCodes reason { set; get; }
        public byte code { set; get; }
    }

    public class LoginReturnInfoResponsePacket : ServerPacket
    {
        public LoginReturnInfoResponsePacket() : base(ServerOpcodes.MSG_RESPONSE_GAMESERVER) { }
        public override void Write()
        {
            _loginPacket.WriteByte(0x13);
            _loginPacket.WriteString("127.0.0.1", 23);

            var data = new byte[]
            {
                0xA0, 0xDE, 0x72, 0x68,
                0xC2, 0xE2, 0x42, 0x00, 0x00, 0xDF, 0x72, 0x68, 0x58, 0x1B, 0x00, 0x00, 0x00, 0x35, 0x62, 0x34,
                0x34, 0x32, 0x35, 0x38, 0x31, 0x34, 0x32, 0x62, 0x65, 0x37, 0x38, 0x64, 0x39, 0x36, 0x38, 0x30,
                0x32, 0x37, 0x35, 0x34, 0x61, 0x00, 0x72, 0x68
            };
            _loginPacket.WriteBytes(data);
        }
    }
}
