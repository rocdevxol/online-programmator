using Microsoft.Win32;
using NLog;
using NLog.Targets;
using Programmator.Communicate;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Programmator
{
	/// <summary>
	/// Логика взаимодействия для MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public static Logger Logger;
		private MemoryTarget target;
		private DispatcherTimer timer;

		public static Device Device { get; set; }
		private Messages Messages;
		private SerialPortCommunication Communication { get; set; }

		private string[] serialNames;
		private List<string> tList;

		private bool update;

		public MainWindow()
		{
			InitializeComponent();
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			Logger = LogManager.GetCurrentClassLogger();
			//target = new MemoryTarget();
			//target.Layout = @"${date:format=HH\:mm\:ss}: ${message}";
			//NLog.Config.SimpleConfigurator.ConfigureForTargetLogging(target, LogLevel.Trace);

			Device = new Device();
			Messages = new Messages(0xB1, 0x90, Device);
			Device.PropertyChanged += Device_PropertyChanged;
			LayoutRoot.DataContext = Device;

			SearchComPort();

			timer = new DispatcherTimer();
			timer.Interval = new TimeSpan(2500000); // 100ms
			timer.Tick += Timer_Tick;
			timer.Start();
		}

		private void Timer_Tick(object sender, EventArgs e)
		{
			//if (!update) return;
			//update = false;
			UpdateForm();
		}

		private void Window_Closed(object sender, EventArgs e)
		{
			Communication?.Stop();
		}

		private void UpdateForm()
		{
			switch (Device.StatusProgress)
			{
				case Communicate.Enums.StatusProgress.Disable:
					progressBarStatus.Visibility = Visibility.Hidden;
					progressBarStatus.IsIndeterminate = false;
					progressBarStatus.Foreground = new SolidColorBrush(Color.FromArgb(255, 6, 176, 37));
					taskBarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.None;
					break;
				case Communicate.Enums.StatusProgress.Finish:
					progressBarStatus.Visibility = Visibility.Hidden;
					progressBarStatus.IsIndeterminate = false;
					progressBarStatus.Foreground = new SolidColorBrush(Color.FromArgb(255, 6, 176, 37));
					taskBarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.None;
					break;
				case Communicate.Enums.StatusProgress.Alarm:
					progressBarStatus.Visibility = Visibility.Visible;
					progressBarStatus.IsIndeterminate = false;
					progressBarStatus.Foreground = new SolidColorBrush(Color.FromArgb(255, 220, 60, 20));
					taskBarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Error;
					break;
				case Communicate.Enums.StatusProgress.Inderterminate:
					progressBarStatus.Visibility = Visibility.Visible;
					progressBarStatus.IsIndeterminate = true;
					progressBarStatus.Foreground = new SolidColorBrush(Color.FromArgb(255, 6, 176, 37));
					taskBarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Indeterminate;
					break;
				case Communicate.Enums.StatusProgress.Work:
					progressBarStatus.Visibility = Visibility.Visible;
					progressBarStatus.IsIndeterminate = false;
					progressBarStatus.Foreground = new SolidColorBrush(Color.FromArgb(255, 6, 176, 37));
					taskBarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Normal;
					break;
			}

			switch (Messages?.GetFunction())
			{
				case Enums.FunctionalMode.Disable:
					break;
				case Enums.FunctionalMode.EraseEeprom:
					break;
				case Enums.FunctionalMode.EraseFlash:
					break;
				case Enums.FunctionalMode.EraseRegion:
					break;
				case Enums.FunctionalMode.ProgramEeprom:
				case Enums.FunctionalMode.ProgramFlash:
				case Enums.FunctionalMode.ProgramRegion:
					textBlockSendedData.Text = Device.AddressOffset.ToString("X8");

					double percent = (double)Device.AddressOffset * 100.0 / (double)Device.DataToWrite.Count;
					progressBarStatus.Value = percent;
					taskBarItemInfo.ProgressValue = percent / 100;
					break;
				case Enums.FunctionalMode.ReadEeprom:
					textBlockLength.Text = Device.EepromData.Count.ToString("X8");
					break;
				case Enums.FunctionalMode.ReadFlash:
					textBlockLength.Text = Device.FlashData.Count.ToString("X8");
					break;
				case Enums.FunctionalMode.ReadRegion:
					textBlockLength.Text = Device.RegionData.Count.ToString("X8");
					break;
				case Enums.FunctionalMode.RebootDevice:
					break;
				case Enums.FunctionalMode.UpdateEeprom:
					break;
				case Enums.FunctionalMode.VersionSoftware:
					break;
			}
		}

		private void Device_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			update = true;
		}

		private void MenuItemOpenFile_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog ofd = new OpenFileDialog();
			ofd.Filter = "Бинарные файлы|*.bin*";
			Boolean? res = ofd.ShowDialog();
			if (res != true) return;

			FileStream stream = new FileStream(ofd.FileName, FileMode.Open, FileAccess.Read);
			int leng = (int)stream.Length;
			byte[] buffer = new byte[stream.Length];
			stream.Read(buffer, 0, leng);
			stream.Close();
			Device.DataToWrite = buffer.ToList();

			if (Communication != null)
			{
				if (Communication.IsEnabled)
				{
					menuItemWriteFlash.IsEnabled = true;
					menuItemWriteEeprom.IsEnabled = true;
					menuItemWriteRegion.IsEnabled = true;
				}
			}

			Logger.Trace("Количество прочитанных байт: {0} [{0:X8}]", Device.DataToWrite.Count);
			textBlockLength.Text = Device.DataToWrite.Count.ToString("X8");

		}

		private void MenuItemSaveFileFlash_Click(object sender, RoutedEventArgs e)
		{
			if (Device.FlashData == null || Device.FlashData.Count == 0)
			{
				MessageBox.Show("Данные остутсвуют, произведите считывание данных с системы", this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}
			SaveFileDialog sfd = new SaveFileDialog();
			sfd.Filter = "Бинарные файлы|*.bin";
			String name = String.Empty;

			name = String.Format("flash_{0}.bin", Device.InfoDevice.NumberMachine);

			sfd.FileName = name;
			Boolean? res = sfd.ShowDialog();
			if (res != true) return;

			FileStream stream = new FileStream(sfd.FileName, FileMode.CreateNew, FileAccess.ReadWrite);
			int leng = Device.FlashData.Count;
			byte[] buffer = Device.FlashData.ToArray();
			stream.Write(buffer, 0, leng);
			stream.Close();
			Logger.Trace("Сохранение файла FLASH: {0} [{0:X8}]", leng);
		}

		private void MenuItemSaveFileEeprom_Click(object sender, RoutedEventArgs e)
		{
			if (Device.EepromData == null || Device.EepromData.Count == 0)
			{
				MessageBox.Show("Данные остутсвуют, произведите считывание данных с системы", this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}
			SaveFileDialog sfd = new SaveFileDialog();
			sfd.Filter = "Бинарные файлы|*.bin";
			String name = String.Empty;

			name = String.Format("eeprom_{0}.bin", Device.InfoDevice.NumberMachine);

			sfd.FileName = name;
			Boolean? res = sfd.ShowDialog();
			if (res != true) return;

			FileStream stream = new FileStream(sfd.FileName, FileMode.CreateNew, FileAccess.ReadWrite);
			int leng = Device.EepromData.Count;
			byte[] buffer = Device.EepromData.ToArray();
			stream.Write(buffer, 0, leng);
			stream.Close();

			Logger.Trace("Сохранение файла EEPROM: {0} [{0:X8}]", leng);
		}

		private void MenuItemExit_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}

		private void MenuItemConnect_Click(object sender, RoutedEventArgs e)
		{
			Messages.SetFunction(Enums.FunctionalMode.VersionSoftware);
			Communication = new SerialPortCommunication(Messages, serialNames[comboBoxSerialPorts.SelectedIndex]);
			Communication?.Start();

			menuItemDisconnect.IsEnabled = true;
			menuItemConnect.IsEnabled = false;
			menuItemUserCode.IsEnabled = true;
			menuItemRewriteEeprom.IsEnabled = true;
			menuItemEraseFlash.IsEnabled = true;
			menuItemReadFlash.IsEnabled = true;
			menuItemEraseEeprom.IsEnabled = true;
			menuItemReadEeprom.IsEnabled = true;
			menuItemEraseRegion.IsEnabled = true;
			menuItemReadRegion.IsEnabled = true;
			menuItemOpenFile.IsEnabled = true;

			menuItemWriteFlash.IsEnabled = false;
			menuItemWriteRegion.IsEnabled = false;
			menuItemWriteEeprom.IsEnabled = false;


			if (Device.DataToWrite == null) return;
			if (Device.DataToWrite.Count == 0) return;

			menuItemWriteFlash.IsEnabled = true;
			menuItemWriteRegion.IsEnabled = true;
			menuItemWriteEeprom.IsEnabled = true;


		}

		private void MenuItemDisconnect_Click(object sender, RoutedEventArgs e)
		{
			Communication?.Stop();

			menuItemDisconnect.IsEnabled = false;
			menuItemConnect.IsEnabled = true;
			menuItemUserCode.IsEnabled = false;
			menuItemRewriteEeprom.IsEnabled = false;
			menuItemEraseFlash.IsEnabled = false;
			menuItemReadFlash.IsEnabled = false;
			menuItemWriteFlash.IsEnabled = false;
			menuItemEraseEeprom.IsEnabled = false;
			menuItemReadEeprom.IsEnabled = false;
			menuItemWriteEeprom.IsEnabled = false;
			menuItemEraseRegion.IsEnabled = false;
			menuItemReadRegion.IsEnabled = false;
			menuItemWriteRegion.IsEnabled = false;
			menuItemOpenFile.IsEnabled = false;

		}

		private void MenuItemUserCode_Click(object sender, RoutedEventArgs e)
		{

		}

		private void MenuItemRewriteEeprom_Click(object sender, RoutedEventArgs e)
		{
			Logger.Trace("Обновление EEPROM");
			Messages.SetFunction(Enums.FunctionalMode.UpdateEeprom);
			//Device.Function = Enums.FunctionalMode.ProgramEeprom;
			//task = new Object[] { 0x01 };
		}

		private void MenuItemEraseFlash_Click(object sender, RoutedEventArgs e)
		{
			Logger.Trace("Очистка FLASH");
			Messages.SetFunction(Enums.FunctionalMode.EraseFlash);
			//task = new Object[] { device.AddressUserCode, device.AddressEeprom, 0 };
		}

		private void MenuItemReadFlash_Click(object sender, RoutedEventArgs e)
		{
			Device.FlashData.Clear();
			Logger.Trace("Чтение FLASH");
			Messages.SetFunction(Enums.FunctionalMode.ReadFlash);
		}

		private void MenuItemWriteFlash_Click(object sender, RoutedEventArgs e)
		{
			Device.AddressBegin = Device.InfoDevice.AddressUserCodeBegin;
			Device.AddressOffset = 0;
			Device.LengthSend = 0;

			Logger.Trace("Запись FLASH");
			Messages.SetFunction(Enums.FunctionalMode.ProgramFlash);
		}

		private void MenuItemEraseEeprom_Click(object sender, RoutedEventArgs e)
		{
			Logger.Trace("Очистка EEPROM");
			Messages.SetFunction(Enums.FunctionalMode.EraseEeprom);
			//task = new Object[] { device.AddressUserCode, device.AddressEeprom, 0 };
		}

		private void MenuItemReadEeprom_Click(object sender, RoutedEventArgs e)
		{
			Device.EepromData.Clear();
			Logger.Trace("Чтение EEPROM");
			Messages.SetFunction(Enums.FunctionalMode.ReadEeprom);
		}

		private void MenuItemWriteEeprom_Click(object sender, RoutedEventArgs e)
		{
			Device.AddressBegin = Device.InfoDevice.AddressEepromBegin;
			Device.AddressOffset = 0;
			Device.LengthSend = 0;

			Logger.Trace("Запись EEPROM");
			Messages.SetFunction(Enums.FunctionalMode.ProgramEeprom);
		}

		private void MenuItemEraseRegion_Click(object sender, RoutedEventArgs e)
		{




			Logger.Trace("Очистка Области");
			Messages.SetFunction(Enums.FunctionalMode.EraseRegion);
			//task = new Object[] { device.AddressUserCode, device.AddressEeprom, 0 };
		}

		private void MenuItemReadRegion_Click(object sender, RoutedEventArgs e)
		{


			Device.RegionData.Clear();
			Logger.Trace("Чтение Области");
			Messages.SetFunction(Enums.FunctionalMode.ReadRegion);
		}

		private void MenuItemWriteRegion_Click(object sender, RoutedEventArgs e)
		{
			Device.AddressBegin = Device.InfoDevice.AddressEepromBegin;
			Device.AddressOffset = 0;
			Device.LengthSend = 0;

			Logger.Trace("Запись Области");
			Messages.SetFunction(Enums.FunctionalMode.ProgramRegion);
		}

		private void ButtonRefreshSerial_Click(object sender, RoutedEventArgs e)
		{
			SearchComPort();
		}

		/// <summary>
		/// Поиск Последовательных портов в системе
		/// </summary>
		private void SearchComPort()
		{
			using (var searcher = new ManagementObjectSearcher("SELECT * FROM WIN32_SerialPort"))
			{
				serialNames = SerialPort.GetPortNames();
				var ports = searcher.Get().Cast<ManagementBaseObject>().ToList();
				tList = (from n in serialNames
						 join p in ports on n equals p["DeviceID"].ToString()
						 select n + " - " + p["Caption"]).ToList();
			}
			comboBoxSerialPorts.ItemsSource = null;
			if (serialNames.Length > 0)
			{
				//comboBoxSerialPorts.ItemsSource = tList;
				comboBoxSerialPorts.ItemsSource = serialNames;
			}
			comboBoxSerialPorts.SelectedIndex = 0;
		}

		private void MenuItemSelectRegion_Click(object sender, RoutedEventArgs e)
		{
			WindowRegion windowRegion = new WindowRegion(Device.Region);
		}
	}
}
