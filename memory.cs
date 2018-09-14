using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

public static class memory
{
	public struct MEMORY_BASIC_INFORMATION64
	{
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

	public static bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer)
	{
		int lpNumberOfBytesRead = 0;
		return ReadProcessMemory(hProcess, lpBaseAddress, lpBuffer, lpBuffer.Length, ref lpNumberOfBytesRead);
	}

	private static List<int> byte_find(byte[] src, byte[] pattern)
	{
		List<int> list = new List<int>();
		if (src.Length < pattern.Length)
		{
			return list;
		}
		for (int i = 0; i < src.Length - pattern.Length + 1; i++)
		{
			bool flag = true;
			for (int j = 0; j < pattern.Length; j++)
			{
				if (src[i + j] != pattern[j])
				{
					flag = false;
				}
			}
			if (flag)
			{
				list.Add(i);
			}
		}
		return list;
	}

	private static int byte_find_first(byte[] src, byte?[] pattern)
	{
		new List<int>();
		if (src.Length < pattern.Length)
		{
			return -1;
		}
		for (int i = 0; i < src.Length - pattern.Length + 1; i++)
		{
			bool flag = true;
			for (int j = 0; j < pattern.Length; j++)
			{
				if (pattern[j].HasValue && src[i + j] != pattern[j])
				{
					flag = false;
				}
			}
			if (flag)
			{
				return i;
			}
		}
		return -1;
	}

	public static ulong[] find_patterns(Process proc, IntPtr start_from, IntPtr end_at, List<byte?[]> patterns) {
		IntPtr intPtr = start_from;
		ulong[] array = new ulong[patterns.Count];
		int num = patterns.Count;
		do
		{
			if (VirtualQueryEx(proc.Handle, intPtr, out MEMORY_BASIC_INFORMATION64 lpBuffer, (uint)Marshal.SizeOf(typeof(MEMORY_BASIC_INFORMATION64))) > 0 && lpBuffer.RegionSize != 0)
			{
				byte[] array2 = new byte[(uint)lpBuffer.RegionSize];
				ReadProcessMemory(proc.Handle, (IntPtr)(long)lpBuffer.BaseAddress, array2);
				for (int i = 0; i < patterns.Count; i++)
				{
					if (array[i] == 0)
					{
						int num2 = byte_find_first(array2, patterns[i]);
						if (num2 > 0)
						{
							array[i] = lpBuffer.BaseAddress + (uint)num2;
							num--;
						}
					}
				}
			}
			intPtr = (IntPtr)(long)(lpBuffer.BaseAddress + lpBuffer.RegionSize);
		}
		while ((ulong)(long)intPtr < (ulong)(long)end_at && num > 0);
		return array;
	}
}
