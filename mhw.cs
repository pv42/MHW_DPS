using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

public static class mhw
{
	public static bool is_wegame_build = false;

	public static long loc1 = -1L;

	public static long loc2 = -1L;

	public static long loc3 = -1L;

	public static long loc4 = -1L;

	[DllImport("kernel32.dll")]
	private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);

	private static bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer) {
		int lpNumberOfBytesRead = 0;
		return ReadProcessMemory(hProcess, lpBaseAddress, lpBuffer, lpBuffer.Length, ref lpNumberOfBytesRead);
	}

	public static ulong read_ulong(IntPtr hProcess, IntPtr lpBaseAddress) {
		byte[] array = new byte[8];
		ReadProcessMemory(hProcess, lpBaseAddress, array);
		return BitConverter.ToUInt64(array, 0);
	}

	public static uint read_uint(IntPtr hProcess, IntPtr lpBaseAddress) {
		byte[] array = new byte[4];
		ReadProcessMemory(hProcess, lpBaseAddress, array);
		return BitConverter.ToUInt32(array, 0);
	}

	public static int dword_to_int(ref byte[] array) {
		return array[0] + (array[1] << 8) + (array[2] << 16) + (array[3] << 24);
	}

	private static ulong asm_func1(Process proc, ulong rcx, uint edx) {
		uint num = read_uint(proc.Handle, (IntPtr)loc1);
		ulong num2 = rcx;
		rcx = (ulong)((long)(num & edx) * 88L);
		return num2 + 72 + rcx;
	}

	public static int[] get_team_dmg(Process proc) {
		int[] array = new int[4];
		byte[] array2 = new byte[4];
		byte[] array3 = new byte[8];
		ReadProcessMemory(proc.Handle, (IntPtr)loc2, array3);
		ulong num = BitConverter.ToUInt64(array3, 0) + 26288;
		ReadProcessMemory(proc.Handle, (IntPtr)loc3, array3);
		ulong rcx = BitConverter.ToUInt64(array3, 0);
		for (int i = 0; i < 4; i++)
		{
			ReadProcessMemory(proc.Handle, (IntPtr)((long)num + 4L * (long)i), array2);
			uint edx = read_uint(proc.Handle, (IntPtr)((long)num + 4L * (long)i));
			ulong num2 = asm_func1(proc, rcx, edx);
			if (num2 != 0)
			{
				ReadProcessMemory(proc.Handle, (IntPtr)(long)(num2 + 72), array3);
				ulong num3 = BitConverter.ToUInt64(array3, 0);
				if (num3 != 0)
				{
					bool num4 = ReadProcessMemory(proc.Handle, (IntPtr)(long)(num3 + 72), array2);
					int num5 = dword_to_int(ref array2);
					if (num4 && num5 >= 0 && num5 <= 1048575)
					{
						array[i] = num5;
					}
				}
			}
		}
		return array;
	}

	public static int get_player_seat_id(Process proc) {
		uint num = read_uint(proc.Handle, (IntPtr)loc4);
		uint num2 = read_uint(proc.Handle, (IntPtr)(num + (is_wegame_build ? 1376 : 600)));
		int result = -1;
		if (num2 > 4096)
		{
			uint num3 = read_uint(proc.Handle, (IntPtr)(long)(num2 + 16));
			if (num3 != 0)
			{
				result = (int)read_uint(proc.Handle, (IntPtr)(num3 + (is_wegame_build ? 49396 : 49132)));
			}
		}
		return result;
	}

	public static string[] get_team_player_names(Process proc) {
		string[] names = new string[4];
		byte[] array2 = new byte[40];
		int num = (int)read_uint(proc.Handle, (IntPtr)loc4) + (is_wegame_build ? 349621 : 346693);
		for (int i = 0; i < 4; i++)
		{
			Array.Resize(ref array2, 40);
			ReadProcessMemory(proc.Handle, (IntPtr)(num + 33 * i), array2);
			int num2 = Array.FindIndex(array2, (byte x) => x == 0);
			if (num2 == 0 || num2 == -1)
			{
				names[i] = "";
			}
			else
			{
				Array.Resize(ref array2, num2);
				names[i] = Encoding.UTF8.GetString(array2);
			}
		}
		return names;
	}
}
