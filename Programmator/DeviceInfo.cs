using Programmator.Communicate;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Programmator
{
	public class DeviceInfo : INotifyPropertyChanged
	{
		/// <summary>
		/// Сетевой адрес устройства
		/// </summary>
		public byte DeviceAddress { get => Get<byte>(); set => Set<byte>(value); }

		/// <summary>
		/// Версия загрузчика
		/// </summary>
		public ushort VersionBootloader { get => Get<ushort>(); set => Set<ushort>(value); }

		/// <summary>
		/// Идентификация машины по коду
		/// </summary>
		public Enums.Machine Machine { get => Get<Enums.Machine>(); set => Set<Enums.Machine>(value); }

		/// <summary>
		/// Порядковый номер машины
		/// </summary>
		public ushort NumberMachine { get => Get<ushort>(); set => Set<ushort>(value); }

		///
		/// Порядковый номер машины
		/// </summary>
		public Enums.CpuIdentificator TypeCpu { get => Get<Enums.CpuIdentificator>(); set => Set<Enums.CpuIdentificator>(value); }

		/// <summary>
		/// Начало кода основной программы
		/// </summary>
		public uint AddressUserCodeBegin { get => Get<uint>(); set => Set<UInt32>(value); }

		/// <summary>
		/// Окончание кода основной программы
		/// </summary>
		public uint AddressUserCodeEnd { get => Get<uint>(); set => Set<UInt32>(value); }

		/// <summary>
		/// Начало EEPROM
		/// </summary>
		public uint AddressEepromBegin { get => Get<uint>(); set => Set<UInt32>(value); }

		/// <summary>
		/// Окончание EEPROM
		/// </summary>
		public uint AddressEepromEnd { get => Get<uint>(); set => Set<UInt32>(value); }

		public DeviceInfo()
		{
			DeviceAddress = 0;
			VersionBootloader = 0;
			Machine = Enums.Machine.UNKNOWN;
			NumberMachine = 0;
			TypeCpu = Enums.CpuIdentificator.UndefinedCpu;
			AddressUserCodeBegin = 0;
			AddressUserCodeEnd = 0;
			AddressEepromBegin = 0;
			AddressEepromEnd = 0;
		}

		#region Bindable PropertyChanged
		private readonly Dictionary<string, object> _properties = new Dictionary<string, object>();

		/// <summary>
		/// Gets the value of a property
		/// <typeparam name="T"></typeparam>
		/// <param name="name"></param>
		/// <returns></returns>
		protected T Get<T>([CallerMemberName] string name = null)
		{
			if (_properties.TryGetValue(name, out object value))
			{
				return value == null ? default(T) : (T)value;
			}

			return default(T);
		}

		/// <summary>
		/// Sets the value of a property
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="value"></param>
		/// <param name="name"></param>
		protected void Set<T>(T value, [CallerMemberName] string name = null)
		{
			if (Equals(value, Get<T>(name)))
			{
				return;
			}

			_properties[name] = value;
			OnPropertyChanged(name);
		}

		[field: NonSerialized]
		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
		#endregion
	}
}
