using System;
using System.Collections.Generic;
using System.Text;
using LoginServer;
using LoginServer.Server;

public static class Global
{
    public static LoginManager LoginMgr { get { return LoginManager.Instance; } }
}