using Framework.Util;
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
            string usernameShift = _loginPacket.ReadString(32);
            password = _loginPacket.ReadString(32);
            _loginPacket.ReadBytes(4);
            string clientMac = _loginPacket.ReadString(32);
            _loginPacket.ReadBytes(32);
            uint unk3 = _loginPacket.ReadUInt32();
            StringShift shift = new();
            string username = shift.Parser(usernameShift);
            Log.outInfo(LogFilter.Server, $"username: {username}");
            Log.outInfo(LogFilter.Server, $"passwordMD5: {password}");
            Log.outInfo(LogFilter.Server, $"MAC: {clientMac}");
            Log.outInfo(LogFilter.Server, $"unk3: {unk3}");
        }

        #region variables
        public string password { set; get; }
        #endregion
    }
}
