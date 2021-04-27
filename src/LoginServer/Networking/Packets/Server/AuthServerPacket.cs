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
            _loginPacket.WriteByte(reason);
            _loginPacket.WriteByte(code);
        }
        public byte reason { set; get; }
        public byte code { set; get; }
    }
}
