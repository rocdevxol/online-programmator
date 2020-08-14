using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;

namespace Programmator.Communicate
{
    public class SerialPortCommunication
    {
        private SerialPort serialPort;
        private Messages messages;
        private Thread serialThread { get; set; }
        private bool isClosed { get; set; }
        public bool IsEnabled { get; set; }

		private bool isAnswered;

        public SerialPortCommunication(Messages messages, string serialPortName)
        {
            try
            {
                this.messages = messages;
                serialPort = new SerialPort(serialPortName, 115200, Parity.None, 8, StopBits.One);
                serialThread = new Thread(new ThreadStart(ThreadSerial));
            }
            catch (UnauthorizedAccessException ex)
            {
                MessageBox.Show(ex.Message, ex.Source);
            }
        }

        public void Start()
        {
            if (IsEnabled) return;
            if (!serialThread.IsAlive)
            {
                serialThread.Start();
            }
            IsEnabled = true;
        }

        public void Stop()
        {
            if (!IsEnabled) return;
            isClosed = true;
            if (serialThread != null)
            {
                if (serialThread.IsAlive)
                {
                    serialThread.Join();
                }
            }
            IsEnabled = false;
        }

        private void ThreadSerial()
        {
            try
            {
                if (!serialPort.IsOpen)
                    serialPort.Open();
                serialPort.ReadTimeout = 500;
                serialPort.WriteTimeout = 500;

                while (!isClosed)
                {
                    byte[] array = messages.PrepareMessage(isAnswered);
                    serialPort.DiscardInBuffer();
                    serialPort.Write(array, 0, array.Length);
                    Thread.Sleep(50);
					int lengthRead = 0;
                    byte[] arrayRead = ReadArray(ref lengthRead);
                    if (lengthRead > 0)
                        isAnswered = messages.ReadMessage(arrayRead);
                }
            }
            catch (Exception ex)
            {
                MainWindow.Logger.Error(ex);
            }
            finally
            {
                if (serialPort.IsOpen)
                    serialPort.Close();
            }
        }

        private byte[] ReadArray(ref int length)
        {
            byte[] arrayRead = null;
			length = 0;
            try
            {
                arrayRead = new byte[255];
                length = serialPort.Read(arrayRead, 0, 255);
                if (length == 0)
                    return null;
            }
            catch (TimeoutException ex)
            {
                //MainWindow.Logger.Error(ex);
            }
            return arrayRead;
        }
    }

}
