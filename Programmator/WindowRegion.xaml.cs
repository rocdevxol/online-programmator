using System.Windows;

namespace Programmator
{
	/// <summary>
	/// Логика взаимодействия для WindowRegion.xaml
	/// </summary>
	public partial class WindowRegion : Window
	{
		public Region region;

		public WindowRegion(Region reg)
		{
			InitializeComponent();
			region = new Region(reg.BeginAddress, reg.EndAddress);
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			textBoxBeginAddress.Text = string.Format("{0:X8}", region.BeginAddress);
			textBoxEndAddress.Text = string.Format("{0:X8}", region.EndAddress);
		}

		private void buttonOk_Click(object sender, RoutedEventArgs e)
		{
			region.BeginAddress = uint.Parse(textBoxBeginAddress.Text, System.Globalization.NumberStyles.HexNumber);
			region.EndAddress = uint.Parse(textBoxEndAddress.Text, System.Globalization.NumberStyles.HexNumber);

			DialogResult = true;
			Close();
		}

		private void buttonCancel_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = false;
			Close();
		}
	}
}
