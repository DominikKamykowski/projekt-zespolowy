using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace stm32
{
    public partial class MainWindow : Window
    {
        private SerialPort serialPort;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            serialPort = new SerialPort("COM5", 115200, Parity.None, 8, StopBits.One);
            serialPort.DataReceived += SerialPort_DataReceived;
            serialPort.Open();
            Trace.WriteLine("GUI");
        }

        byte Calc_CRC_8(byte[] DataArray, int Length)
        {
            int i;
            byte CRC;

            CRC = 0;

            for (i = 0; i < Length; i++)
                CRC = CRC_8_TABLE[CRC ^ DataArray[i]];

            return CRC;
        }

        byte[] CRC_8_TABLE =
        {
             0x00, 0x07, 0x0e, 0x09, 0x1c, 0x1b, 0x12, 0x15, 0x38, 0x3f, 0x36, 0x31,
 0x24, 0x23, 0x2a, 0x2d, 0x70, 0x77, 0x7e, 0x79, 0x6c, 0x6b, 0x62, 0x65,
 0x48, 0x4f, 0x46, 0x41, 0x54, 0x53, 0x5a, 0x5d, 0xe0, 0xe7, 0xee, 0xe9,
 0xfc, 0xfb, 0xf2, 0xf5, 0xd8, 0xdf, 0xd6, 0xd1, 0xc4, 0xc3, 0xca, 0xcd,
 0x90, 0x97, 0x9e, 0x99, 0x8c, 0x8b, 0x82, 0x85, 0xa8, 0xaf, 0xa6, 0xa1,
 0xb4, 0xb3, 0xba, 0xbd, 0xc7, 0xc0, 0xc9, 0xce, 0xdb, 0xdc, 0xd5, 0xd2,
 0xff, 0xf8, 0xf1, 0xf6, 0xe3, 0xe4, 0xed, 0xea, 0xb7, 0xb0, 0xb9, 0xbe,
 0xab, 0xac, 0xa5, 0xa2, 0x8f, 0x88, 0x81, 0x86, 0x93, 0x94, 0x9d, 0x9a,
 0x27, 0x20, 0x29, 0x2e, 0x3b, 0x3c, 0x35, 0x32, 0x1f, 0x18, 0x11, 0x16,
 0x03, 0x04, 0x0d, 0x0a, 0x57, 0x50, 0x59, 0x5e, 0x4b, 0x4c, 0x45, 0x42,
 0x6f, 0x68, 0x61, 0x66, 0x73, 0x74, 0x7d, 0x7a, 0x89, 0x8e, 0x87, 0x80,
 0x95, 0x92, 0x9b, 0x9c, 0xb1, 0xb6, 0xbf, 0xb8, 0xad, 0xaa, 0xa3, 0xa4,
 0xf9, 0xfe, 0xf7, 0xf0, 0xe5, 0xe2, 0xeb, 0xec, 0xc1, 0xc6, 0xcf, 0xc8,
 0xdd, 0xda, 0xd3, 0xd4, 0x69, 0x6e, 0x67, 0x60, 0x75, 0x72, 0x7b, 0x7c,
 0x51, 0x56, 0x5f, 0x58, 0x4d, 0x4a, 0x43, 0x44, 0x19, 0x1e, 0x17, 0x10,
 0x05, 0x02, 0x0b, 0x0c, 0x21, 0x26, 0x2f, 0x28, 0x3d, 0x3a, 0x33, 0x34,
 0x4e, 0x49, 0x40, 0x47, 0x52, 0x55, 0x5c, 0x5b, 0x76, 0x71, 0x78, 0x7f,
 0x6a, 0x6d, 0x64, 0x63, 0x3e, 0x39, 0x30, 0x37, 0x22, 0x25, 0x2c, 0x2b,
 0x06, 0x01, 0x08, 0x0f, 0x1a, 0x1d, 0x14, 0x13, 0xae, 0xa9, 0xa0, 0xa7,
 0xb2, 0xb5, 0xbc, 0xbb, 0x96, 0x91, 0x98, 0x9f, 0x8a, 0x8d, 0x84, 0x83,
 0xde, 0xd9, 0xd0, 0xd7, 0xc2, 0xc5, 0xcc, 0xcb, 0xe6, 0xe1, 0xe8, 0xef,
 0xfa, 0xfd, 0xf4, 0xf3
        };

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button clickedButton = (Button)sender;
            Trace.WriteLine("test");
            // Włączanie/wyłączanie diod
            if (clickedButton == buttonLED1 || clickedButton == buttonLED2 || clickedButton == buttonLED3)
            {
                int ledNumber = int.Parse(clickedButton.Tag.ToString());
                bool isOn = (clickedButton.Background == Brushes.Green);

                // Tworzenie wiadomości protokołu
                byte[] message = new byte[9];
                message[0] = 0x60;
                message[1] = 0x60;
                message[2] = (byte)(ledNumber + 2);
                message[3] = (byte)(isOn ? 1 : 0);
                message[8] = Calc_CRC_8(message, message.Length-1);

                // Wysyłanie wiadomości
                serialPort.Write(message, 0, message.Length);
            }
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Trace.WriteLine(e);
            Slider changedSlider = (Slider)sender;
            int pwmNumber = int.Parse(changedSlider.Tag.ToString());
            byte pwmValue = (byte)changedSlider.Value;

            // Tworzenie wiadomości protokołu
            byte[] message = new byte[9];
            message[0] = 0x60;
            message[1] = 0x60;
            message[2] = (byte)(pwmNumber + 5);
            message[3] = pwmValue;
            message[8] = CalculateCRC(message);

            // Wysyłanie wiadomości
            serialPort.Write(message, 0, message.Length);
        }

        private byte CalculateCRC(byte[] message)
        {
            byte crc = 0;
            for (int i = 0; i < message.Length - 1; i++)
            {
                crc ^= message[i];
            }
            return crc;
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            Trace.WriteLine("Od stm");
            int bytesToRead = serialPort.BytesToRead;
            byte[] buffer = new byte[bytesToRead];
            serialPort.Read(buffer, 0, bytesToRead);

            // Analiza odebranych danych
            if (buffer[0] == 0x60 && buffer[1] == 0x60 && buffer.Length >= 10)
            {
                bool led1State = buffer[2] == 0x01;
                bool led2State = buffer[3] == 0x01;
                bool led3State = buffer[4] == 0x01;
                byte pwm1Value = buffer[5];
                byte pwm2Value = buffer[6];
                byte pwm3Value = buffer[7];
                bool buttonState = buffer[8] == 0x01;
                byte receivedCRC = buffer[9];
                byte calculatedCRC = CalculateCRC(buffer);

                if (receivedCRC == calculatedCRC)
                {
                    // Aktualizacja stanu interfejsu użytkownika
                    Dispatcher.Invoke(() =>
                    {
                        buttonLED1.Background = led1State ? Brushes.Green : Brushes.Red;
                        buttonLED2.Background = led2State ? Brushes.Green : Brushes.Red;
                        buttonLED3.Background = led3State ? Brushes.Green : Brushes.Red;
                        sliderPWM1.Value = pwm1Value;
                        sliderPWM2.Value = pwm2Value;
                        sliderPWM3.Value = pwm3Value;

                        // Obsługa stanu naciśnięcia przycisku
                        if (buttonState)
                        {
                            // Przycisk został naciśnięty
                        }
                        else
                        {
                            // Przycisk nie jest naciśnięty
                        }
                    });
                }
                else
                {
                    // Błąd sumy kontrolnej
                }
            }
            else
            {
                // Błędny format wiadomości
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Button clickedButton = (Button)sender;
            Trace.WriteLine("test");
            // Włączanie/wyłączanie diod
            //if (clickedButton == buttonLED1 || clickedButton == buttonLED2 || clickedButton == buttonLED3)
            //{
                //int ledNumber = int.Parse(clickedButton.Tag.ToString());
                //bool isOn = (clickedButton.Background == Brushes.Green);

                // Tworzenie wiadomości protokołu
                byte[] message = new byte[9];
                message[0] = 0x60;
                message[1] = 0x60;
                message[2] = /*(byte)(ledNumber + 2)*/ 0x01;
                message[3] = /*(byte)(isOn ? 1 : 0)*/ 0x01;
            message[4] = 0x01;
            message[5] = 0xFF;
            message[6] = 0xFF;
            message[7] = 0xFF;
                message[8] = CalculateCRC(message);

                // Wysyłanie wiadomości
                Trace.WriteLine(message[8]);
                serialPort.Write(message, 0, message.Length);
            //}
        }
    }
}
