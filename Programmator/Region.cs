using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Programmator
{
	public class Region : INotifyPropertyChanged
	{
		/// <summary>
		/// Начальный адрес области
		/// </summary>
		public uint BeginAddress { get => Get<uint>(); set => Set(value); }
		/// <summary>
		/// Конечный адрес области
		/// </summary>
		public uint EndAddress { get => Get<uint>(); set => Set(value); }

		public Region(uint beginAddress, uint endAddress)
		{
			BeginAddress = beginAddress;
			EndAddress = endAddress;
		}

		public Region() : this(0, 0) { }

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
