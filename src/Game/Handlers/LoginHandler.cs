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
        [LoginPacketHandler(ClientOpcodes.MSG_LOGIN_RETURN_INFO)]
        void HandleLoginReturnInfo(SelectServerPacket selectServer)
        {
            var ret = new LoginReturnInfoResponsePacket();
            SendPacket(ret);
        }
    }
}
