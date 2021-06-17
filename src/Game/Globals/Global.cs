using System;
using System.Collections.Generic;
using System.Text;
using Game;
using Game.Server;

public static class Global
{
    public static WorldManager WorldMgr { get { return WorldManager.Instance; } }
}