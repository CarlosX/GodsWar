using Framework.Constants.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Networking.Packets.Server
{
    public class ServerListPacket : ServerPacket
    {
        public ServerListPacket() : base(ServerOpcodes.MSG_UNK2) { }
        public override void Write()
        {
            _loginPacket.WriteString("Dev", 32);
            var data = new byte[]
            {
                /* LAST LOGIN SERVER IS ALWAYS THE FIRST ONE */
                //0x39, 0x00, // unk - Packet Length
                //0x18, 0x27,// server entry identification constant?
                /*0x44, 0x65, 0x76, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // Server Name
                0x00, 0x00,
                0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // unk
                0x00, 0x00, // unk
                0x00, 0x00, // unk
                0x00, */
                
                0x41, 0xC1, 0x5A, 0x41, // ip ?
                0x05, // id?
                
                0x44, 0xB6, // unk
                0xC0, 0x41, 
                
                0x15, // estrella
                
                0x04, // unk
                0x00, // unk
                0x00, // unk
                0x02, // id
                0x03, // recomendado ? 1 : 0
                0x00, 
                0x00, 
                0x0F,
                0x00,
                0x00, // end list
                0x00,
            };

            _loginPacket.WriteBytes(data);
        }
    }

    public class SelectServerPacket : ClientPacket
    {
        public SelectServerPacket(WorldPacket packet) : base(packet) { }
        public override void Read()
        {

        }
    }
}
