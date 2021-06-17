using Framework.Util;

namespace Game.Networking.Packets.Client
{
    public class AuthSession : ClientPacket
    {
        public AuthSession(WorldPacket packet) : base(packet) { }
        public override void Read()
        {
            /*string usernameShift = _worldPacket.ReadString(32);
            password = _worldPacket.ReadString(32);
            _worldPacket.ReadBytes(4);
            string clientMac = _worldPacket.ReadString(32);
            _worldPacket.ReadBytes(32);
            uint unk3 = _worldPacket.ReadUInt32();
            StringShift shift = new();
            username = shift.Parser(usernameShift);
            Log.outInfo(LogFilter.Server, $"username: {username}");
            Log.outInfo(LogFilter.Server, $"passwordMD5: {password}");
            Log.outInfo(LogFilter.Server, $"MAC: {clientMac}");
            Log.outInfo(LogFilter.Server, $"unk3: {unk3}");*/
        }

        #region variables
        public string username { set; get; }
        public string password { set; get; }
        #endregion
    }
}
