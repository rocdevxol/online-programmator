using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Programmator
{
    public class Device : INotifyPropertyChanged
    {

		private uint addressBegin;
		private uint addressEnd;
		private uint addressOffset;
		private uint lengthSend;
		private byte[] dataArray;

		private List<byte> eepromData;
		private List<byte> flashData;
		private List<byte> regionData;

		private List<byte> dataToWrite;
		private DeviceInfo infoDevice;
		private Region region;
		private Communicate.Enums.StatusProgress statusProgress;

		public uint AddressBegin
		{
			get => addressBegin;
			set
			{
				if (addressBegin != value)
				{
					addressBegin = value;
					NotifyPropertyChanged();
				}
			}
		}

        public uint AddressEnd
		{
			get => addressEnd;
			set
			{
				if (addressEnd != value)
				{
					addressEnd = value;
					NotifyPropertyChanged();
				}
			}
		}

		public uint AddressOffset
		{
			get => addressOffset;
			set
			{
				if (addressOffset != value)
				{
					addressOffset = value;
					NotifyPropertyChanged();
				}
			}
		}

		public uint LengthSend
		{
			get => lengthSend;
			set
			{
				if (lengthSend != value)
				{
					lengthSend = value;
					NotifyPropertyChanged();
				}
			}
		}

		public byte[] DataArray
		{
			get => dataArray;
			set
			{
				if (dataArray != value)
				{
					dataArray = value;
					NotifyPropertyChanged();
				}
			}
		}


		public List<byte> EepromData
		{
			get => eepromData;
			set
			{
				if (eepromData != value)
				{
					eepromData = value;
					NotifyPropertyChanged();
				}
			}
		}

		public List<byte> FlashData
		{
			get => flashData;
			set
			{
				if (flashData != value)
				{
					flashData = value;
					NotifyPropertyChanged();
				}
			}
		}

		public List<byte> RegionData
		{
			get => regionData;
			set
			{
				if (regionData != value)
				{
					regionData = value;
					NotifyPropertyChanged();
				}
			}
		}


		public List<byte> DataToWrite
		{
			get => dataToWrite;
			set
			{
				if (dataToWrite != value)
				{
					dataToWrite = value;
					NotifyPropertyChanged();
				}
			}
		}


		public DeviceInfo InfoDevice
		{
			get => infoDevice;
			set
			{
				if (infoDevice != value)
				{
					infoDevice = value;
					NotifyPropertyChanged();
				}
			}
		}


		public Region Region
		{
			get => region;
			set
			{
				if (region != value)
				{
					region = value;
					NotifyPropertyChanged();
				}
			}
		}


		public Communicate.Enums.StatusProgress StatusProgress
		{
			get => statusProgress;
			set
			{
				if (statusProgress != value)
				{
					statusProgress = value;
					NotifyPropertyChanged();
				}
			}
		}


		public Device()
        {
            AddressBegin = 0;
            AddressEnd = 0;
            AddressOffset = 0;
            LengthSend = 0;
            DataArray = null;
            EepromData = new List<byte>();
            FlashData = new List<byte>();
            RegionData = new List<byte>();
            DataToWrite = new List<byte>();
            InfoDevice = new DeviceInfo();
			Region = new Region();
            StatusProgress = Communicate.Enums.StatusProgress.Disable;
        }
		/*
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
		*/
		#region Events
		[field: NonSerialized]
		public event PropertyChangedEventHandler PropertyChanged;
		protected void OnPropertyChanged(string propertyName)
		{
			//if (PropertyChanged != null)
			//   PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		protected void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
		#endregion
	}
}
