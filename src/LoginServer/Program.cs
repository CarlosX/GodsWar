using Framework.Configuration;
using Framework.Constants;
using Framework.Database;
using LoginServer.Networking;
using LoginServer.Server;
using System;
using System.Globalization;
using System.Threading;

namespace LoginServer
{
    class Program
    {
        const uint LoginSleep = 50;
        static void Main(string[] args)
        {
            //Set Culture
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionHandler;

            if (!ConfigMgr.Load("LoginServer.conf"))
                ExitNow();

            if (!StartDB())
                ExitNow();

            Global.LoginMgr.SetInitialLoginSettings();

            // Server startup begin
            uint startupBegin = Time.GetMSTime();

            // Launch the worldserver listener socket
            int loginPort = LoginConfig.GetIntValue(LoginCfg.PortLogin);
            string loginListener = ConfigMgr.GetDefaultValue("BindIP", "0.0.0.0");

            int networkThreads = ConfigMgr.GetDefaultValue("Network.Threads", 1);
            if (networkThreads <= 0)
            {
                Log.outError(LogFilter.Server, "Network.Threads must be greater than 0");
                ExitNow();
                return;
            }


            var LoginSocketMgr = new LoginSocketManager();
            if (!LoginSocketMgr.StartNetwork(loginListener, loginPort, networkThreads))
            {
                Log.outError(LogFilter.Network, "Failed to start Login Network");
                ExitNow();
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            uint startupDuration = Time.GetMSTimeDiffToNow(startupBegin);
            Log.outInfo(LogFilter.Server, "Login initialized in {0} minutes {1} seconds", (startupDuration / 60000), ((startupDuration % 60000) / 1000));

            LoginUpdateLoop();

            ExitNow();
        }

        static void LoginUpdateLoop()
        {
            uint realPrevTime = Time.GetMSTime();

            while (!Global.LoginMgr.IsStopped)
            {
                var realCurrTime = Time.GetMSTime();

                uint diff = Time.GetMSTimeDiff(realPrevTime, realCurrTime);
                Global.LoginMgr.Update(diff);
                realPrevTime = realCurrTime;

                uint executionTimeDiff = Time.GetMSTimeDiffToNow(realCurrTime);

                // we know exactly how long it took to update the world, if the update took less than WORLD_SLEEP_CONST, sleep for WORLD_SLEEP_CONST - world update time
                if (executionTimeDiff < LoginSleep)
                    Thread.Sleep((int)(LoginSleep - executionTimeDiff));
            }
        }

        static bool StartDB()
        {
            DatabaseLoader loader = new DatabaseLoader(DatabaseTypeFlags.Login);
            loader.AddDatabase(DB.Login, "Login");

            if (!loader.Load())
                return false;
            return true;
        }

        static void ExitNow()
        {
            Console.WriteLine("Halting process...");
            System.Threading.Thread.Sleep(10000);
            Environment.Exit(-1);
        }

        static void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;
            Log.outException(ex);
        }
    }
}
