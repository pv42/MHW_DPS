using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

public static class memory {
	public struct MEMORY_BASIC_INFORMATION64 {
		public ulong BaseAddress;
		public ulong AllocationBase;
		public int AllocationProtect;
		public int __alignment1;
		public ulong RegionSize;
		public int State;
		public int Protect;
		public int Type;
		public int __alignment2;
	}

    private static byte[] mem_8B = new byte[8];
    private static byte[] mem_4B = new byte[4];

    [DllImport("kernel32.dll")]
	private static extern int VirtualQueryEx(IntPtr hProcess, IntPtr lpAddress, out MEMORY_BASIC_INFORMATION64 lpBuffer, uint dwLength);

	[DllImport("kernel32.dll")]
	public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);


    // find a pattern in a byte array, returns a list of all matches start indices 
	private static List<int> byte_find(byte[] src, byte[] pattern) {
		List<int> list = new List<int>();
		if (src.Length < pattern.Length) {
			return list;
		}
		for (int i = 0; i < src.Length - pattern.Length + 1; i++) {
			bool flag = true;
			for (int j = 0; j < pattern.Length; j++) {
				if (src[i + j] != pattern[j]) {
					flag = false;
                    break;
				}
			}
			if (flag) {
				list.Add(i);
			}
		}
		return list;
	}


    // find a pattern in a byte array, returns the first matchs start index or -1 of no match was found
    private static int byte_find_first(byte[] src, byte?[] pattern)	{
		new List<int>();
		if (src.Length < pattern.Length){
			return -1;
		}
		for (int i = 0; i < src.Length - pattern.Length + 1; i++) {
			bool flag = true;
			for (int j = 0; j < pattern.Length; j++) {
				if (pattern[j].HasValue && src[i + j] != pattern[j]) {
					flag = false;
				}
			}
			if (flag) {
				return i;
			}
		}
		return -1;
	}

    /**
     * finds memory patterns in a process 
     * @param proc        process handle of the process to search
     * @param start_from  lower search boundary address
     * @param end_at      upper search boundary address
     * @param patters     patterns to search for
     * @return array of found patters addressses
     */
	public static ulong[] find_patterns(Process proc, IntPtr start_from, IntPtr end_at, List<byte?[]> patterns) {
		IntPtr intPtr = start_from;
		ulong[] results = new ulong[patterns.Count];
		int remaining_not_found = patterns.Count;
		do {
			if (VirtualQueryEx(proc.Handle, intPtr, out MEMORY_BASIC_INFORMATION64 lpBuffer, (uint)Marshal.SizeOf(typeof(MEMORY_BASIC_INFORMATION64))) > 0 && lpBuffer.RegionSize != 0) {
				byte[] memory = new byte[(uint)lpBuffer.RegionSize];
				read_bytes(proc.Handle, (IntPtr)(long)lpBuffer.BaseAddress, memory);
				for (int i = 0; i < patterns.Count; i++) {
					if (results[i] == 0) {
						int match = byte_find_first(memory, patterns[i]);
						if (match > 0) {
							results[i] = lpBuffer.BaseAddress + (uint)match;
							remaining_not_found--;
						}
					}
				}
			}
			intPtr = (IntPtr)(long)(lpBuffer.BaseAddress + lpBuffer.RegionSize);
		} while ((ulong)(long)intPtr < (ulong)(long)end_at && remaining_not_found > 0);
		return results;
	}

    // reads a bytearray from a process at a given address, the lenght is the lenght of the buffer given to write to
    public static bool read_bytes(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer) {
        int lpNumberOfBytesRead = 0;
        return ReadProcessMemory(hProcess, lpBaseAddress, lpBuffer, lpBuffer.Length, ref lpNumberOfBytesRead);
    }

    // reads an uint64 from a process at a given address
    public static ulong read_ulong(IntPtr hProcess, IntPtr lpBaseAddress) {
        read_bytes(hProcess, lpBaseAddress, mem_8B);
        return BitConverter.ToUInt64(mem_8B, 0);
    }

    // reads an uint64 from a process at a given address
    public static long read_long(IntPtr hProcess, IntPtr lpBaseAddress) {
        read_bytes(hProcess, lpBaseAddress, mem_8B);
        return BitConverter.ToInt64(mem_8B, 0);
    }

    // reads an uint32 from a process at a given address
    public static uint read_uint(IntPtr hProcess, IntPtr lpBaseAddress) {
        read_bytes(hProcess, lpBaseAddress, mem_4B);
        return BitConverter.ToUInt32(mem_4B, 0);
    }

    // reads a float (or single) from a process at a given address
    public static float read_float(IntPtr hProcess, IntPtr lpBaseAddress) {
        read_bytes(hProcess, lpBaseAddress, mem_4B);
        return BitConverter.ToSingle(mem_4B, 0);
    }

    // read a null terminated string from a process at a given address
    public static string read_string(IntPtr hProcess, IntPtr lpBaseAddress, uint max_lengh) {
        string str;
        byte[] mem = new byte[max_lengh];
        memory.read_bytes(hProcess, lpBaseAddress, mem);
        int str_len = Array.FindIndex(mem, (byte x) => x == 0); // find '\0' String terminator
        if (str_len <= 0) {
            str = "";
        } else {
            Array.Resize(ref mem, str_len);
            str = Encoding.UTF8.GetString(mem);
        }
        return str;
    }
}
