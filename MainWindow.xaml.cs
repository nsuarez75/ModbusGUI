﻿using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO.Ports;
using EasyModbus;

namespace ModbusGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            PopulateProtocolos();
            PopulateSerialPorts();
            PopulateStopBits();
            PopulateParidad();
            PopulateBaudios();
           
        }

        private ModbusServer modbusServer;
        System.Timers.Timer updateTimer = new System.Timers.Timer(5000); // Cambiar valores cada 5 segundos

        private void PopulateProtocolos()
        {
            comboBoxProtocolos.Items.Clear();
            comboBoxProtocolos.Items.Add("RTU");
            comboBoxProtocolos.Items.Add("TCP");

            comboBoxProtocolos.SelectedIndex = 0;
        }

        private void PopulateStopBits()
        {
            comboBoxStopBits.Items.Clear();
            comboBoxStopBits.Items.Add(1);
            comboBoxStopBits.Items.Add(2);

            comboBoxStopBits.SelectedIndex = 0;
        }

        private void PopulateParidad()
        {
            comboBoxParity.Items.Clear();
            comboBoxParity.Items.Add("None");
            comboBoxParity.Items.Add("Even");
            comboBoxParity.Items.Add("Odd");

            comboBoxParity.SelectedIndex = 0;
        }

        private void PopulateBaudios()
        {
            comboBoxBaudios.Items.Clear();
            comboBoxBaudios.Items.Add(300);
            comboBoxBaudios.Items.Add(1200);
            comboBoxBaudios.Items.Add(2400);
            comboBoxBaudios.Items.Add(4800);
            comboBoxBaudios.Items.Add(9600);
            comboBoxBaudios.Items.Add(14400);
            comboBoxBaudios.Items.Add(19200);
            comboBoxBaudios.Items.Add(38400);
            comboBoxBaudios.Items.Add(57600);
            comboBoxBaudios.Items.Add(115200);

            comboBoxBaudios.SelectedIndex = 4;

        }


        private void PopulateSerialPorts()
        {
            comboBoxPorts.Items.Clear(); // Limpiar elementos previos
            string[] ports = SerialPort.GetPortNames(); // Obtener puertos serie disponibles

            if (ports.Length > 0)
            {
                foreach (var port in ports)
                {
                    comboBoxPorts.Items.Add(port); // Agregar cada puerto al ComboBox
                }
                comboBoxPorts.SelectedIndex = 0; // Seleccionar el primer puerto como predeterminado
            }
            else
            {
                comboBoxPorts.Items.Add("No ports available"); // Mostrar mensaje si no hay puertos
                comboBoxPorts.SelectedIndex = 0;
            }
        }

        private void EncenderServidor()
        {

            string protocol = comboBoxProtocolos.SelectedItem.ToString(); // "RTU" o "TCP"

            if (protocol == "RTU")
            {
                // Inicializar ModbusServer en modo RTU
                modbusServer = new ModbusServer
                {
                    SerialPort = "COM4", // Cambiar por el puerto adecuado
                    Baudrate = 9600,
                    Parity = System.IO.Ports.Parity.None,
                    StopBits = System.IO.Ports.StopBits.One,
                    UnitIdentifier = 1, // Dirección del esclavo
                };
                // Iniciar el servidor
                modbusServer.Listen();
            }
            else
            {
                // Inicializar ModbusServer en modo TCP
                modbusServer = new ModbusServer
                {
                    Port = 502 // Puerto TCP estándar
                };
                // Iniciar el servidor
                modbusServer.Listen();
            }

            // Configurar actualización aleatoria de valores
            var random = new Random();
            updateTimer.Elapsed += (sender, e) =>
            {
                for (int i = 0; i < 65535; i++) // Cambiar valores en las primeras 100 direcciones
                {
                    modbusServer.holdingRegisters[i] = (short)random.Next(0, 1000);
                    modbusServer.inputRegisters[i] = (short)random.Next(0, 1000);
                    modbusServer.coils[i] = !modbusServer.coils[i];
                    modbusServer.discreteInputs[i] = !modbusServer.discreteInputs[i];

                }
            };
            updateTimer.AutoReset = true;
            updateTimer.Start();
          
        }

        private void PararServidor()
        {
            updateTimer.Stop();
            modbusServer.StopListening();
        }

        private void Iniciar(object sender, RoutedEventArgs e)
        {
            EncenderServidor();
            labelEstado.Content = "ON";
        }

        private void Parar(object sender, RoutedEventArgs e)
        {
            PararServidor();
            labelEstado.Content = "OFF";
                    }

        private void comboBoxProtocolos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Visibility visibility = comboBoxProtocolos.SelectedIndex == 0 ? Visibility.Visible : Visibility.Collapsed;
            LabelPorts.Visibility = visibility;
            LabelStopBits.Visibility = visibility;
            LabelParity.Visibility = visibility;
            LabelBaudios.Visibility = visibility;
            comboBoxPorts.Visibility = visibility;
            comboBoxStopBits.Visibility = visibility;
            comboBoxParity.Visibility = visibility;
            comboBoxBaudios.Visibility = visibility;
        }
    }
}