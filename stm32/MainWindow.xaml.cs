using System;
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
            serialPort = new SerialPort("COM1", 115200, Parity.None, 8, StopBits.One);
            serialPort.DataReceived += SerialPort_DataReceived;
            serialPort.Open();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button clickedButton = (Button)sender;

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
                message[8] = CalculateCRC(message);

                // Wysyłanie wiadomości
                serialPort.Write(message, 0, message.Length);
            }
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
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
    }
}
