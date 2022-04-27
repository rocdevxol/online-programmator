using System;
using System.Threading;
using System.Windows.Threading;

namespace Programmator.Communicate
{
	public class Messages
	{
		private readonly byte NetAddressDevice, NetAddressPc;
		private Device Device { get; set; }

		private Enums.Commands commandOut;

		private Enums.FunctionalMode Function { get; set; }

		private ushort TaskToProcess;

		public Messages(byte netAddressDevice, byte netAddressPc, Device device)
		{
			NetAddressDevice = netAddressDevice;
			NetAddressPc = netAddressPc;
			Device = device;
			commandOut = Enums.Commands.StatusSystem;
			TaskToProcess = 0;
		}

        public void SetFunction(Enums.FunctionalMode function)
        {
            Function = function;
			TaskToProcess = 1;
        }

		public Enums.FunctionalMode GetFunction()
		{
			return Function;
		}


		#region Подготовка сообщений
        public byte[] PrepareMessage(bool answered)
        {
            byte[] array = null;
			Enums.Commands command = Enums.Commands.ZeroCommand;
			ushort task = TaskToProcess;
			try
            {
                switch (Function)
                {
                    case Enums.FunctionalMode.Disable:
						command = Enums.Commands.StatusSystem;
						break;
					case Enums.FunctionalMode.VersionSoftware:
						command = Enums.Commands.VersionSoft;
						break;
                    case Enums.FunctionalMode.EraseEeprom:
						Device.StatusProgress = Enums.StatusProgress.Inderterminate;

						command = Enums.Commands.EraseFlash;
						Device.AddressBegin = Device.InfoDevice.AddressEepromBegin;
						Device.AddressEnd = Device.InfoDevice.AddressEepromEnd;
						break;
                    case Enums.FunctionalMode.EraseFlash:
						Device.StatusProgress = Enums.StatusProgress.Inderterminate;

						command = Enums.Commands.EraseFlash;
						Device.AddressBegin = Device.InfoDevice.AddressUserCodeBegin;
						Device.AddressEnd = Device.InfoDevice.AddressUserCodeEnd;
						break;
                    case Enums.FunctionalMode.EraseRegion:
						Device.StatusProgress = Enums.StatusProgress.Inderterminate;

						command = Enums.Commands.EraseFlash;
						Device.AddressBegin = Device.Region.BeginAddress;
						Device.AddressEnd = Device.Region.EndAddress;
						break;
                    case Enums.FunctionalMode.ProgramEeprom:
					case Enums.FunctionalMode.ProgramFlash:
					case Enums.FunctionalMode.ProgramRegion:
						if (Device.DataToWrite == null) break;
						if (Device.DataToWrite.Count == 0) break;
						Device.StatusProgress = Enums.StatusProgress.Work;
						command = Enums.Commands.ProgramVerifyFlash;
						if (task == 0 && !answered)
							task = 2;
						if (Device.DataToWrite.Count - Device.AddressOffset == 0)
						{
							Device.StatusProgress = Enums.StatusProgress.Finish;
							Function = Enums.FunctionalMode.Disable;
							command = Enums.Commands.ZeroCommand;
							MainWindow.Logger.Trace("Запись завершена");
						}
						break;
					case Enums.FunctionalMode.ReadEeprom:
					case Enums.FunctionalMode.ReadFlash:
					case Enums.FunctionalMode.ReadRegion:
						if (task == 0 && !answered)
							task = 2;
						command = Enums.Commands.ReadFlash;
                        break;
					case Enums.FunctionalMode.UpdateEeprom:
						command = Enums.Commands.FinishProgramFlash;
						break;
					case Enums.FunctionalMode.RebootDevice:
						command = Enums.Commands.RebootDevice;
						break;

                }

//				if (command == Enums.Commands.ZeroCommand)
//					command = commandOut;
                switch (command)
                {

                    case Enums.Commands.ZeroCommand:
                        array = prepareStatusSystem();
                        break;
                    case Enums.Commands.VersionSoft:
                        array = prepareVersionSoft();
                        break;
                    case Enums.Commands.StatusSystem:
                        array = prepareStatusSystem();
                        break;
                    case Enums.Commands.ReadFlash:
                        array = prepareReadFlash(task);
                        break;
                    case Enums.Commands.EraseFlash:
                        array = prepareEraseFlash(task);
                        break;
                    case Enums.Commands.ProgramVerifyFlash:
                        array = prepareProgramVerifyFlash(task);
                        break;
                    case Enums.Commands.FinishProgramFlash:
						array = prepareFinishProgramFlash();
						break;
                    case Enums.Commands.RebootDevice:
						array = prepareRebootDevice();
						break;
                }

            }
            catch (Exception ex)
            {
                MainWindow.Logger.Error(ex);
            }
            return array;
        }

        private byte[] prepareVersionSoft()
        {
            Structures.FlashResult buffer = new Structures.FlashResult();

            buffer.ToNetAddress = NetAddressDevice;
            buffer.FromNetAddress = NetAddressPc;
            buffer.LengthMessage = StructConvert.LengthStruct(buffer);
            buffer.Command = Enums.Commands.VersionSoft;

            buffer.Task = 0;
            buffer.Status = 0;

            buffer.Crc = CalculateCrc.CreateCRC(buffer);
            byte[] array = StructConvert.StructureToByteArray(buffer);
            return array;
        }

        private byte[] prepareStatusSystem()
		{
			Structures.FlashResult buffer = new Structures.FlashResult();

			buffer.ToNetAddress = NetAddressDevice;
			buffer.FromNetAddress = NetAddressPc;
			buffer.LengthMessage = StructConvert.LengthStruct(buffer);
			buffer.Command = Enums.Commands.StatusSystem;

			buffer.Task = 0;
			buffer.Status = 0;

			buffer.Crc = CalculateCrc.CreateCRC(buffer);
			byte[] array = StructConvert.StructureToByteArray(buffer);
			return array;
		}

		private byte[] prepareEraseFlash(ushort task)
		{
			Structures.FlashErase buffer = new Structures.FlashErase();
			buffer.ToNetAddress = NetAddressDevice;
			buffer.FromNetAddress = NetAddressPc;
			buffer.LengthMessage = StructConvert.LengthStruct(buffer);
			buffer.Command = Enums.Commands.EraseFlash;

			buffer.AddressBegin = Device.AddressBegin;
			buffer.AddressEnd = Device.AddressEnd;

			buffer.Task = task;

			if (TaskToProcess == 1) TaskToProcess = 0;

			buffer.Crc = CalculateCrc.CreateCRC(buffer);
			byte[] array = StructConvert.StructureToByteArray(buffer);
			return array;
		}

		private byte[] prepareProgramVerifyFlash(ushort task)
		{
			Structures.FlashReadWrite buffer = new Structures.FlashReadWrite();
			buffer.ToNetAddress = NetAddressDevice;
			buffer.FromNetAddress = NetAddressPc;
			buffer.LengthMessage = StructConvert.LengthStruct(buffer);
			buffer.Command = Enums.Commands.ProgramVerifyFlash;

			if (task == 0 || task == 1) // Новые данные
			{
				Device.AddressOffset += Device.LengthSend;

				int delta = (int)Device.DataToWrite.Count - (int)Device.AddressOffset;
				if (delta > 230)
					Device.LengthSend = 230;
				else if (delta != 0)
					Device.LengthSend = (uint)delta;
				else // Переданы все данные
				{
					Device.LengthSend = 0;

				}

				byte[] data = Device.DataToWrite.GetRange((int)Device.AddressOffset, (int)Device.LengthSend).ToArray();
				if (data.Length < 230) Array.Resize(ref data, 230);
				Device.DataArray = data;
			}
			else if (task == 3) // Ошибка
			{

			}
			else // task = 2, Повтор данных
			{

			}


			buffer.AddressBegin = Device.AddressBegin;
			buffer.AddressOffset = Device.AddressOffset;
			buffer.LengthData = Device.LengthSend / 2;
			buffer.Data = Device.DataArray;
			buffer.rsrv = 0;

			buffer.Crc = CalculateCrc.CreateCRC(buffer);
			byte[] array = StructConvert.StructureToByteArray(buffer);
			return array;
		}

		private byte[] prepareReadFlash(ushort task)
		{
			Structures.FlashRead buffer = new Structures.FlashRead();
			buffer.ToNetAddress = NetAddressDevice;
			buffer.FromNetAddress = NetAddressPc;
			buffer.LengthMessage = StructConvert.LengthStruct(buffer);
			buffer.Command = Enums.Commands.ReadFlash;

			buffer.Task = task;
			if (Function == Enums.FunctionalMode.ReadFlash)
			{
				buffer.AddressBegin = Device.InfoDevice.AddressUserCodeBegin;
				buffer.AddressEnd = Device.InfoDevice.AddressUserCodeEnd;
			}
			else if (Function == Enums.FunctionalMode.ReadEeprom)
			{
				buffer.AddressBegin = Device.InfoDevice.AddressEepromBegin;
				buffer.AddressEnd = Device.InfoDevice.AddressEepromEnd;
			}
			else if (Function == Enums.FunctionalMode.ReadRegion)
			{
				buffer.AddressBegin = Device.Region.BeginAddress;
				buffer.AddressEnd = Device.Region.EndAddress;
			}

			buffer.Crc = CalculateCrc.CreateCRC(buffer);
			byte[] array = StructConvert.StructureToByteArray(buffer);
			return array;
		}

		private byte[] prepareFinishProgramFlash()
		{
			Structures.FlashResult buffer = new Structures.FlashResult();

			buffer.ToNetAddress = NetAddressDevice;
			buffer.FromNetAddress = NetAddressPc;
			buffer.LengthMessage = StructConvert.LengthStruct(buffer);
			buffer.Command = Enums.Commands.FinishProgramFlash;

			buffer.Task = 0;
			buffer.Status = 0;

			buffer.Crc = CalculateCrc.CreateCRC(buffer);
			byte[] array = StructConvert.StructureToByteArray(buffer);
			return array;
		}

		private byte[] prepareRebootDevice()
		{
			Structures.FlashResult buffer = new Structures.FlashResult();

			buffer.ToNetAddress = NetAddressDevice;
			buffer.FromNetAddress = NetAddressPc;
			buffer.LengthMessage = StructConvert.LengthStruct(buffer);
			buffer.Command = Enums.Commands.RebootDevice;

			buffer.Task = 0;
			buffer.Status = 0;

			buffer.Crc = CalculateCrc.CreateCRC(buffer);
			byte[] array = StructConvert.StructureToByteArray(buffer);
			return array;
		}

		#endregion


		#region Чтение сообщений

		public bool ReadMessage(byte[] array)
		{
			bool result = false;
			try
			{
				ushort length = array[2];
				if (length == 0) return false;
				ushort crcGetter = GetWorld(array[length - 2], array[length - 1]);
				ushort crcCalc = CalculateCrc.CreateCRC(array, (ushort)(length));
				if (crcCalc != crcGetter)
					return false;
				result = ReadInputData(array);
			}
			catch (Exception ex)
			{
				MainWindow.Logger.Error(ex);
			}

			return result;
		}

		public bool ReadInputData(byte[] array)
		{
			try
			{
				Enums.Commands command = (Enums.Commands)array[3];
				switch (command)
				{
					case Enums.Commands.VersionSoft:
						readVersionSoft(array);
						break;
					case Enums.Commands.StatusSystem:
						readStatusSystem(array);
						break;
					case Enums.Commands.EraseFlash:
						readEraseFlash(array);
						break;
					case Enums.Commands.ProgramVerifyFlash:
						readProgramVerifyFlash(array);
						break;
					case Enums.Commands.ReadFlash:
						readReadFlash(array);
						break;
					case Enums.Commands.FinishProgramFlash:
						readFinishProgramFlash(array);
						break;
					case Enums.Commands.RebootDevice:
						readRebootDevice(array);
						break;
				}
			}
			catch (Exception ex)
			{
				MainWindow.Logger.Error(ex);
				return false;
			}
			return true;
		}

		private void readVersionSoft(byte[] array)
		{
			Structures.FlashVersion buffer = new Structures.FlashVersion();
			object obj = new Structures.FlashVersion();
			StructConvert.ByteArrayToStructure(array, ref obj);
			buffer = (Structures.FlashVersion)obj;

			Device.InfoDevice.DeviceAddress = buffer.FromNetAddress;
			Device.InfoDevice.VersionBootloader = buffer.VersionBootLoader;
			Device.InfoDevice.Machine = (Enums.Machine)buffer.Machine;
			Device.InfoDevice.NumberMachine = buffer.NumberMachine;
			Device.InfoDevice.TypeCpu = (Enums.CpuIdentificator)buffer.TypeCpu;

			Device.InfoDevice.AddressUserCodeBegin = buffer.AddressUserCodeBegin;
			Device.InfoDevice.AddressUserCodeEnd = buffer.AddressUserCodeEnd;

			Device.InfoDevice.AddressEepromBegin = buffer.AddressEepromBegin;
			Device.InfoDevice.AddressEepromEnd = buffer.AddressEepromEnd;

			Function = Enums.FunctionalMode.Disable;
		}

		private void readStatusSystem(byte[] array)
		{
			Structures.FlashResult buffer = new Structures.FlashResult();
			object obj = new Structures.FlashResult();
			StructConvert.ByteArrayToStructure(array, ref obj);
			buffer = (Structures.FlashResult)obj;

			Enums.Status status;
/*			if (commandOut != Enums.Commands.StatusSystem)
			{
				if (commandOut == Enums.Commands.ReadFlash)
				{
					MainWindow.Logger.Info("Чтение EEPROM завершено");
					Device.StatusProgress = Enums.StatusProgress.Finish;
					// TODO разрешить работать кнопке сохранить ЕЕПРОМ
				}
				commandOut = Enums.Commands.ZeroCommand;
			}
*/			status = (Enums.Status)buffer.Status;
			if (status != Enums.Status.OK)
			{
				MainWindow.Logger.Error("Ошибка операции: {0}", status);
				commandOut = Enums.Commands.ZeroCommand;
				Device.StatusProgress = Enums.StatusProgress.Alarm;//Finish
				Function = Enums.FunctionalMode.Disable;
			}
		}

		private void readEraseFlash(byte[] array)
		{
			Structures.FlashResult buffer = new Structures.FlashResult();
			object obj = new Structures.FlashResult();
			StructConvert.ByteArrayToStructure(array, ref obj);
			buffer = (Structures.FlashResult)obj;

			Enums.Status status = (Enums.Status)buffer.Status;
			if (status != Enums.Status.OK)
			{
				MainWindow.Logger.Error("Ошибка операции: {0}", status);
				Device.StatusProgress = Enums.StatusProgress.Alarm;
				Function = Enums.FunctionalMode.Disable;
			}
			else
			{
				MainWindow.Logger.Trace("Очистка завершена");
				Device.StatusProgress = Enums.StatusProgress.Finish;
				Function = Enums.FunctionalMode.Disable;
			}

			commandOut = Enums.Commands.ZeroCommand;
		}

		private void readProgramVerifyFlash(byte[] array)
		{
			Structures.FlashResult buffer = new Structures.FlashResult();
			object obj = new Structures.FlashResult();
			StructConvert.ByteArrayToStructure(array, ref obj);
			buffer = (Structures.FlashResult)obj;

			Enums.Status status = (Enums.Status)buffer.Status;
			if (status != Enums.Status.OK)
			{
				MainWindow.Logger.Error("Ошибка операции: {0}", status);
				commandOut = Enums.Commands.ZeroCommand;
				Device.StatusProgress = Enums.StatusProgress.Alarm;
				Function = Enums.FunctionalMode.Disable;
			}
			//Device.AddressOffset += Device.LengthSend; // TODO uncomment
			if (TaskToProcess == 1) TaskToProcess = 0;
		}

		private void readReadFlash(byte[] array)
		{
			Structures.FlashReadWrite buffer = new Structures.FlashReadWrite();
			object obj = new Structures.FlashReadWrite();
			StructConvert.ByteArrayToStructure(array, ref obj);
			buffer = (Structures.FlashReadWrite)obj;

            if (Function == Enums.FunctionalMode.ReadEeprom)
            {
				if (buffer.LengthData == 0) // Чтение завершено
				{
					MainWindow.Logger.Trace("Чтение EEPROM завершено");
					Device.StatusProgress = Enums.StatusProgress.FinishReadEeprom;
					Function = Enums.FunctionalMode.Disable;
				}

				for (int i = 0; i < buffer.LengthData * 2; i++)
                {
                    Device.EepromData.Add(buffer.Data[i]);
                }
            }
            else if (Function == Enums.FunctionalMode.ReadFlash)
            {
                for (int i = 0; i < buffer.LengthData * 2; i++)
                {
                    Device.FlashData.Add(buffer.Data[i]);
                }

				if (buffer.LengthData == 0) // Чтение завершено
				{
					MainWindow.Logger.Trace("Чтение Flash завершено");
					Device.StatusProgress = Enums.StatusProgress.FinishReadFlash;
					Function = Enums.FunctionalMode.Disable;
				}
			}
			else if (Function == Enums.FunctionalMode.ReadRegion)
            {
                for (int i = 0; i < buffer.LengthData * 2; i++)
                {
                    Device.RegionData.Add(buffer.Data[i]);
                }

				if (buffer.LengthData == 0) // Чтение завершено
				{
					MainWindow.Logger.Trace("Чтение области завершено");
					Device.StatusProgress = Enums.StatusProgress.FinishReadRegion;
					Function = Enums.FunctionalMode.Disable;
				}
			}

			if (buffer.LengthData != 0)
				Device.StatusProgress = Enums.StatusProgress.Inderterminate;

			if (TaskToProcess == 1) TaskToProcess = 0;
		}

		private void readFinishProgramFlash(byte[] array)
		{
			Structures.FlashResult buffer = new Structures.FlashResult();
			object obj = new Structures.FlashResult();
			StructConvert.ByteArrayToStructure(array, ref obj);
			buffer = (Structures.FlashResult)obj;

			Enums.Status status = (Enums.Status)buffer.Status;
			if (status == Enums.Status.OK)
			{
				MainWindow.Logger.Info("Операция завершена");
				commandOut = Enums.Commands.ZeroCommand;
				Device.StatusProgress = Enums.StatusProgress.Alarm;
				Function = Enums.FunctionalMode.Disable;
			}
			//Device.AddressOffset += Device.LengthSend; // TODO uncomment
			if (TaskToProcess == 1) TaskToProcess = 0;
		}

		private void readRebootDevice(byte[] array)
		{
			Structures.FlashResult buffer = new Structures.FlashResult();
			object obj = new Structures.FlashResult();
			StructConvert.ByteArrayToStructure(array, ref obj);
			buffer = (Structures.FlashResult)obj;

			Enums.Status status = (Enums.Status)buffer.Status;
			if (status == Enums.Status.OK)
			{
				MainWindow.Logger.Info("Операция завершена");
				commandOut = Enums.Commands.ZeroCommand;
				Device.StatusProgress = Enums.StatusProgress.Alarm;
				Function = Enums.FunctionalMode.Disable;
			}
			//Device.AddressOffset += Device.LengthSend; // TODO uncomment
			if (TaskToProcess == 1) TaskToProcess = 0;
		}

		#endregion

		private byte LowPart(ushort value) => (byte)(value & 0x00FF);

		private byte HighPart(ushort value) => (byte)(value >> 8);

		private ushort GetWorld(byte low, byte high) => (ushort)((high << 8) + low);

	}
}
