using Framework.Constants.Network;
using Game.Networking;
using Game.Networking.Packets.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Networking
{
    public partial class WorldSession
    {
        [LoginPacketHandler(ClientOpcodes.MSG_SELECT_SERVER)]
        void HandleSelectServer(SelectServerPacket selectServer)
        {
            var data = new SelectServerResponsePacket();
            SendPacket(data);
        }
    }
}
