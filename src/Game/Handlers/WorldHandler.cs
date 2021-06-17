using Framework.Constants.Network;
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
        [LoginPacketHandler(ClientOpcodes.MSG_ENTER_GAME)]
        void HandleEnterGame(SelectServerPacket selectServer)
        {
            var data = new EnterGameResponsePacket();
            SendPacket(data);

            
        }

        [LoginPacketHandler(ClientOpcodes.MSG_CLIENT_READY)]
        void HandleClientReady(SelectServerPacket selectServer)
        {
            var data2 = new GameClientReadyResponsePacket();
            SendPacket(data2);
        }
    }
}
