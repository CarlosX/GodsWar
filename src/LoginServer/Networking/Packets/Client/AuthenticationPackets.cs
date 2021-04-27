using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoginServer.Networking.Packets.Client
{
    public class AuthSession : ClientPacket
    {
        public AuthSession(LoginPacket packet) : base(packet) { }
        public override void Read()
        {
            
        }
    }
}
