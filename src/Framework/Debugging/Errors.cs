using System;
using System.Runtime.CompilerServices;

public class Debugger
{
    public static void Assert(bool value, string message = "", [CallerMemberName]string memberName = "")
    {
        if (!value)
        {
            if (!message.IsEmpty())
                Log.outFatal(LogFilter.Server, message);

            throw new Exception(memberName);
        }
    }
}
