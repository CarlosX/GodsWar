using Framework.Constants.Network;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Game.Networking
{
    public static class PacketManager
    {
        public static void Initialize()
        {
            Assembly currentAsm = Assembly.GetExecutingAssembly();
            foreach (var type in currentAsm.GetTypes())
            {
                foreach (var methodInfo in type.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic))
                {
                    foreach (var msgAttr in methodInfo.GetCustomAttributes<LoginPacketHandlerAttribute>())
                    {
                        if (msgAttr == null)
                            continue;

                        if (msgAttr.Opcode == ClientOpcodes.Unknown)
                        {
                            Log.outError(LogFilter.Network, "Opcode {0} does not have a value", msgAttr.Opcode);
                            continue;
                        }

                        if (_clientPacketTable.ContainsKey(msgAttr.Opcode))
                        {
                            Log.outError(LogFilter.Network, "Tried to override OpcodeHandler of {0} with {1} (Opcode {2})", _clientPacketTable[msgAttr.Opcode].ToString(), methodInfo.Name, msgAttr.Opcode);
                            continue;
                        }

                        var parameters = methodInfo.GetParameters();
                        if (parameters.Length == 0)
                        {
                            Log.outError(LogFilter.Network, "Method: {0} Has no paramters", methodInfo.Name);
                            continue;
                        }

                        if (parameters[0].ParameterType.BaseType != typeof(ClientPacket))
                        {
                            Log.outError(LogFilter.Network, "Method: {0} has wrong BaseType", methodInfo.Name);
                            continue;
                        }

                        _clientPacketTable[msgAttr.Opcode] = new PacketHandler(methodInfo, msgAttr.Status, msgAttr.Processing, parameters[0].ParameterType);
                    }
                }
            }
        }

        public static bool TryPeek(this ConcurrentQueue<WorldPacket> queue, out WorldPacket result)
        {
            result = null;

            if (queue.IsEmpty)
                return false;

            if (!queue.TryPeek(out result))
                return false;

            return true;
        }

        public static PacketHandler GetHandler(ClientOpcodes opcode)
        {
            return _clientPacketTable.LookupByKey(opcode);
        }

        public static bool ContainsHandler(ClientOpcodes opcode)
        {
            return _clientPacketTable.ContainsKey(opcode);
        }

        static ConcurrentDictionary<ClientOpcodes, PacketHandler> _clientPacketTable = new();
    }

    public class PacketHandler
    {
        public PacketHandler(MethodInfo info, SessionStatus status, PacketProcessing processingplace, Type type)
        {
            methodCaller = (Action<WorldSession, ClientPacket>)GetType().GetMethod("CreateDelegate", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(type).Invoke(null, new object[] { info });
            sessionStatus = status;
            ProcessingPlace = processingplace;
            packetType = type;
        }

        public void Invoke(WorldSession session, WorldPacket packet)
        {
            if (packetType == null)
                return;

            using (var clientPacket = (ClientPacket)Activator.CreateInstance(packetType, packet))
            {
                clientPacket.Read();
                clientPacket.LogPacket(session);
                methodCaller(session, clientPacket);
            }
        }

        static Action<WorldSession, ClientPacket> CreateDelegate<P1>(MethodInfo method) where P1 : ClientPacket
        {
            // create first delegate. It is not fine because its 
            // signature contains unknown types T and P1
            Action<WorldSession, P1> d = (Action<WorldSession, P1>)method.CreateDelegate(typeof(Action<WorldSession, P1>));
            // create another delegate having necessary signature. 
            // It encapsulates first delegate with a closure
            return delegate (WorldSession target, ClientPacket p) { d(target, (P1)p); };
        }

        Action<WorldSession, ClientPacket> methodCaller;
        Type packetType;
        public PacketProcessing ProcessingPlace { get; private set; }
        public SessionStatus sessionStatus { get; private set; }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public sealed class LoginPacketHandlerAttribute : Attribute
    {
        public LoginPacketHandlerAttribute(ClientOpcodes opcode)
        {
            Opcode = opcode;
            Status = SessionStatus.Loggedin;
            Processing = PacketProcessing.ThreadUnsafe;
        }

        public ClientOpcodes Opcode { get; private set; }
        public SessionStatus Status { get; set; }
        public PacketProcessing Processing { get; set; }
    }
}
