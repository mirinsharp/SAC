using System;
using System.Collections.Generic;
using LeagueSharp;

namespace SCommon.Packet
{
    public static class PacketHandler
    {
        private class FunctionList : List<Action<byte[]>>
        {
            public FunctionList()
                : base()
            {

            }
        }

        private static FunctionList[] s_opcodeMap;

        static PacketHandler()
        {
            s_opcodeMap = new FunctionList[256];
            for (int i = 0; i < 256; i++)
                s_opcodeMap[i] = new FunctionList();

            Game.OnProcessPacket += Game_OnProcessPacket;
        }

        public static void Register(byte opcode, Action<byte[]> fn)
        {
            s_opcodeMap[opcode].Add(fn);
        }

        public static void Unregister(byte opcode, Action<byte[]> fn)
        {
            s_opcodeMap[opcode].Remove(fn);
        }

        public static void Clear(byte opcode)
        {
            s_opcodeMap[opcode].Clear();
        }

        private static void Game_OnProcessPacket(GamePacketEventArgs args)
        {
            foreach (var fn in s_opcodeMap[args.PacketData[0]])
            {
                if (fn != null)
                    fn(args.PacketData);
            }
        }
    }
}
