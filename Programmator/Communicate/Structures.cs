using System;
using System.Runtime.InteropServices;

namespace Programmator.Communicate
{
	public static class Structures
	{
		public struct FlashVersion
		{
			public byte ToNetAddress;
			public byte FromNetAddress;
			public byte LengthMessage;
			public Enums.Commands Command;

			public ushort VersionBootLoader;
			public uint AddressUserCodeBegin;
			public uint AddressUserCodeEnd;
			public uint AddressEepromBegin;
			public uint AddressEepromEnd;
			public ushort Machine;
			public ushort NumberMachine;
			public ushort TypeCpu;

			public ushort Crc;
		}

		public struct FlashReadWrite
		{
			public byte ToNetAddress;
			public byte FromNetAddress;
			public byte LengthMessage;
			public Enums.Commands Command;

			public ushort rsrv;

			public uint AddressBegin;
			public uint AddressOffset;
			public uint LengthData;
			/// <summary>
			/// максимальная длина 115 слов
			/// </summary>
			//[MarshalAs(UnmanagedType.ByValArray, SizeConst = 115)]
			//public UInt16[] Data;
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 230)]
			public byte[] Data;

			public ushort Crc;
		}

		public struct FlashErase
		{
			public byte ToNetAddress;
			public byte FromNetAddress;
			public byte LengthMessage;
			public Enums.Commands Command;

			public uint AddressBegin;
			public uint AddressEnd;

			public ushort Task;
			public ushort Crc;
		}

		public struct FlashRead
		{
			public byte ToNetAddress;
			public byte FromNetAddress;
			public byte LengthMessage;
			public Enums.Commands Command;

			public uint AddressBegin;
			public uint AddressEnd;
			public ushort Task;

			public ushort Crc;
		}


		public struct FlashResult
		{
			public byte ToNetAddress;
			public byte FromNetAddress;
			public byte LengthMessage;
			public Enums.Commands Command;

			public byte Task;
			public byte Status;

			public ushort Crc;
		}

		public struct FlashDefault
		{
			public byte ToNetAddress;
			public byte FromNetAddress;
			public byte LengthMessage;
			public Enums.Commands Command;

			public byte State1;
			public byte State2;

			public ushort Crc;
		}
	}

	public static class CalculateCrc
	{
		public static ushort CreateCRC(object obj)
		{
			int len = Marshal.SizeOf(obj);
			byte[] arr = new byte[len];
			IntPtr ptr = Marshal.AllocHGlobal(len);
			Marshal.StructureToPtr(obj, ptr, true);
			Marshal.Copy(ptr, arr, 0, len);
			Marshal.FreeHGlobal(ptr);

			return CreateCRC(arr, (ushort)len);
		}

		/// <summary>
		/// Нахождение контрольной суммы для массива данных
		/// </summary>
		/// <param name="Arr">Массив, с байтовыми элементами</param>
		/// <param name="Length">Длина массива (ВАЖНО: С учетом контрольной суммы)</param>
		/// <returns>Контрольную сумму</returns>
		/// 
		public static ushort CreateCRC(byte[] Arr, ushort Length)
		{
			ushort Result = 0xFFFF;
			ushort B = 0;

			ushort Len = (ushort)(Length - 2);
			for (ushort i = 0; i <= Len - 1; i++)
			{
				B = Convert.ToUInt16(Arr[i]);
				Result = Convert.ToUInt16((Result & 0xFF00) + (B ^ (Result & 0x00FF)));
				for (int j = 1; j <= 8; j++)
				{
					if ((Result & 0x0001) != 0)
					{
						Result = Convert.ToUInt16((Result >> 1) ^ 0xA001);
					}
					else
					{
						Result = Convert.ToUInt16((Result >> 1));
					}
				}
			}
			return Result;
		}

	}

	#region Function Converters

	/// <summary>
	/// Преобразование массива байтового в структуру и наобороот, а также нахождение длины структуы в байтах
	/// </summary>
	public static class StructConvert
	{
		/// <summary>
		/// Преобразование структуры в байтовый массив 
		/// </summary>
		/// <param name="obj">указывается сама структура для трансформации</param>
		/// <returns>байтовый массив</returns>
		public static byte[] StructureToByteArray(object obj, int len)
		{
			//Int32 len = Marshal.SizeOf(obj);
			byte[] arr = new byte[len];//len
			IntPtr ptr = Marshal.AllocHGlobal(len);
			Marshal.StructureToPtr(obj, ptr, true);
			Marshal.Copy(ptr, arr, 0, len);
			Marshal.FreeHGlobal(ptr);
			return arr;
		}

		/// <summary>
		/// Преобразование структуры в байтовый массив 
		/// </summary>
		/// <param name="obj">указывается сама структура для трансформации</param>
		/// <returns>байтовый массив</returns>
		public static byte[] StructureToByteArray(object obj)
		{
			int len = Marshal.SizeOf(obj);
			byte[] arr = new byte[len];
			IntPtr ptr = Marshal.AllocHGlobal(len);
			Marshal.StructureToPtr(obj, ptr, true);
			Marshal.Copy(ptr, arr, 0, len);
			Marshal.FreeHGlobal(ptr);
			return arr;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="bytearray"></param>
		/// <param name="obj"></param>
		public static void ByteArrayToStructure(byte[] bytearray, ref object obj)
		{
			int len = Marshal.SizeOf(obj);
			IntPtr i = Marshal.AllocHGlobal(len);
			Marshal.Copy(bytearray, 0, i, len);
			obj = Marshal.PtrToStructure(i, obj.GetType());
			Marshal.FreeHGlobal(i);
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="bytearray"></param>
		/// <param name="obj"></param>
		public static void ByteArrayToStructure(byte[] bytearray, ref object obj, int len)
		{
			//Int32 len = Marshal.SizeOf(obj);
			IntPtr i = Marshal.AllocHGlobal(len);
			Marshal.Copy(bytearray, 0, i, len);
			obj = Marshal.PtrToStructure(i, obj.GetType());
			Marshal.FreeHGlobal(i);
		}

		/// <summary>
		/// Нахождение размера структуры в байтах
		/// </summary>
		/// <param name="obj">Структура размер которой определяется</param>
		/// <returns></returns>
		public static byte LengthStruct(object obj)
		{
			return (byte)Marshal.SizeOf(obj);
		}
	}
	#endregion
}
