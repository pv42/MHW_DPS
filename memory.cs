using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

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

	[DllImport("kernel32.dll")]
	private static extern int VirtualQueryEx(IntPtr hProcess, IntPtr lpAddress, out MEMORY_BASIC_INFORMATION64 lpBuffer, uint dwLength);

	[DllImport("kernel32.dll")]
	public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);

	public static bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer) {
		int lpNumberOfBytesRead = 0;
		return ReadProcessMemory(hProcess, lpBaseAddress, lpBuffer, lpBuffer.Length, ref lpNumberOfBytesRead);
	}

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
				ReadProcessMemory(proc.Handle, (IntPtr)(long)lpBuffer.BaseAddress, memory);
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
}
