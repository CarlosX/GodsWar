using Framework.Constants.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoginServer.Networking.Packets.Server
{
    public class ServerListPacket : ServerPacket
    {
        public ServerListPacket() : base(ServerOpcodes.MSG_UNK2) { }
        public override void Write()
        {
            var data = new byte[]
            {


                /* LAST LOGIN SERVER IS ALWAYS THE FIRST ONE */
                //0x39, 0x00, // unk - Packet Length
                //0x18, 0x27,// server entry identification constant?
                0x41,0x67,0x61,0x74,0x68,0x61,0x20,0x28,0x4c,0x6f,0x77,0x20,0x45,0x78,0x70,0x29, 0x00, // Server Name
                0xC4, 0xB6, // unk
                0xDD, 0x41, // unk
                0x00, 0x00, 0x00, 0x00, 0x28, 0xB6, // unk
                0xDD, 0x41, // unk
                0x44, 0xB6, // unk
                0xDD, 0x41, 0xC1, 0x5A, // unk
                0x41, 0x00, 0x44, 0xB6, // unk
                0xDD, 0x41, 0x15, 0x05, // unk
                0x00, // unk
                0x00, // unk
                0x01, // id
                0x00, // recomendado ? 1 : 0
                0x23, 0x00, 0x0F, // spot?
                0x01,
                0x00, // end list
                0x00,
            };

            _loginPacket.WriteBytes(data);
        }
    }

    public class SelectServerPacket : ClientPacket
    {
        public SelectServerPacket(LoginPacket packet) : base(packet) { }
        public override void Read()
        {

        }
    }
}
