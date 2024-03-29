﻿using Framework.Constants.Network;
using LoginServer.Networking;
using LoginServer.Networking.Packets.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoginServer.Networking
{
    public partial class LoginSession
    {
        [LoginPacketHandler(ClientOpcodes.MSG_SELECT_SERVER)]
        void HandleSelectServer(SelectServerPacket selectServer)
        {
            var data = new SelectServerResponsePacket();
            SendPacket(data);
        }
    }
}
