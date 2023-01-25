using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace DefineServerUtility
{
    class ConvertPacketFunc
    {
        public const short _maxByte = 1024;
        public const short _maxPerson = 20;

        public static Packet CreatePack(int protocolID, long uuid, int size, byte[] data)
        {
            Packet pack;
            pack._protocolID = protocolID;
            pack._targetID = uuid;
            pack._totalSize = (short)size;
            pack._datas = new byte[1002];
            if (data != null) // 로그인 Send_Login_Failed 일 땐 null
            {
                Array.Copy(data, pack._datas, data.Length);
            }
            return pack;
        }

        public static byte[] StructureToByteArray(object obj)
        {
            int dataSize = Marshal.SizeOf(obj);
            IntPtr buff = Marshal.AllocHGlobal(dataSize);
            Marshal.StructureToPtr(obj, buff, false);
            byte[] data = new byte[dataSize];
            Marshal.Copy(buff, data, 0, dataSize);
            Marshal.FreeHGlobal(buff);

            return data;
        }
        public static object ByteArrayToStructure(byte[] data, Type type, int size)
        {
            IntPtr buff = Marshal.AllocHGlobal(data.Length);
            Marshal.Copy(data, 0, buff, data.Length);
            object obj = Marshal.PtrToStructure(buff, type);
            Marshal.FreeHGlobal(buff);
            if (Marshal.SizeOf(obj) != size)
            {
                return null;
            }
            return obj;
        }
    }
}
