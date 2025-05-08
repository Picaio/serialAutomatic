using System;
using System.IO.Ports;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Management; 

namespace conexionSerial
{
    public partial class Form1 : Form
    {
        SerialPort puertoActivo;
        bool graficar = false;
        int tiempo = 0;
        NotifyIcon notifyIcon;
        DateTime ultimaRespuestaArduino = DateTime.Now;
        System.Windows.Forms.Timer heartbeatTimer;

        ManagementEventWatcher watcherConectar;
        ManagementEventWatcher watcherDesconectar;
        public Form1()
        {
            InitializeComponent();
            IniciarDeteccionPlugAndPlay();

            // Crear NotifyIcon
            notifyIcon = new NotifyIcon();
            notifyIcon.Icon = SystemIcons.Information; // Obligatorio poner un ícono
            notifyIcon.Visible = true;
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            label1.Text = "Buscando dispositivos...";

            string deviceInfo = await DetectarDispositivoAsync();

            if (!string.IsNullOrEmpty(deviceInfo))
            {
                label1.Text = $"{deviceInfo} encontrado!";
                notifyIcon.ShowBalloonTip(
                    3000,
                    "Dispositivo Detectado",
                    $"{deviceInfo} conectado correctamente.",
                    ToolTipIcon.Info
                );
            }
            else
            {
                label1.Text = "No se encontró ESP32 ni Arduino.";
                notifyIcon.ShowBalloonTip(
                    3000,
                    "Error de detección",
                    "No se detectó ningún ESP32 ni Arduino.",
                    ToolTipIcon.Error
                );
            }
        }

        private async Task<string> DetectarDispositivoAsync()
        {
            // Cerrar el puerto anterior si está abierto
            if (puertoActivo != null)
            {
                try
                {
                    if (puertoActivo.IsOpen)
                    {
                        puertoActivo.WriteLine("RESET");
                        await Task.Delay(500); // Espera que Arduino lo procese
                        puertoActivo.DataReceived -= PuertoActivo_DataReceived;
                        puertoActivo.Close();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al cerrar puerto: " + ex.Message);
                }
                finally
                {
                    puertoActivo.Dispose();
                    puertoActivo = null;
                }
            }

            // Buscar entre todos los COM
            foreach (string portName in SerialPort.GetPortNames())
            {
                try
                {
                    puertoActivo = new SerialPort(portName, 115200);
                    puertoActivo.ReadTimeout = 500;
                    puertoActivo.WriteTimeout = 500;
                    puertoActivo.DataReceived -= PuertoActivo_DataReceived;
                    puertoActivo.DataReceived += PuertoActivo_DataReceived;
                    puertoActivo.Open();
                    await Task.Delay(1000);

                    int tiempoTotal = 5000;
                    int tiempoEscuchado = 0;

                    while (tiempoEscuchado < tiempoTotal)
                    {
                        await Task.Delay(500);
                        tiempoEscuchado += 500;

                        string incoming = puertoActivo.ReadExisting();

                        if (incoming.Contains("DEVICE:ESP32"))
                        {
                            puertoActivo.WriteLine("STOP");
                            return "ESP32 en " + portName;
                        }
                        else if (incoming.Contains("DEVICE:ARDUINO"))
                        {
                            puertoActivo.WriteLine("STOP");
                            return "Arduino en " + portName;
                        }
                    }

                    // Si no detecta el dispositivo, cerrar este puerto también
                    puertoActivo.DataReceived -= PuertoActivo_DataReceived;
                    puertoActivo.Close();
                }
                catch
                {
                    // Ignorar errores de puertos ocupados o inaccesibles
                }
            }

            puertoActivo = null;
            return null;
        }

        private void PuertoActivo_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                string linea = puertoActivo.ReadLine().Trim();

                if (linea == "HB_ARDUINO")
                {
                    ultimaRespuestaArduino = DateTime.Now;
                    return;
                }

                if (graficar && linea.StartsWith("$"))
                {
                    string dato = linea.Substring(1);
                    if (int.TryParse(dato, out int temperatura))
                    {
                        this.Invoke((MethodInvoker)delegate {
                            chart1.Series[0].Points.AddXY(tiempo, temperatura);
                            tiempo++;
                        });
                    }
                }
            }
            catch
            {
                // Manejo de errores de lectura
            }
        }
        private void button2_Click(object sender, EventArgs e)
        {
            notifyIcon.ShowBalloonTip(
               3000,
               "Detectado",
               "ESP32 o Arduino encontrado.",
               ToolTipIcon.Info
           );
        }

        private async void button3_Click(object sender, EventArgs e)
        {
            foreach (string portName in SerialPort.GetPortNames())
            {
                try
                {
                    using (SerialPort port = new SerialPort(portName, 115200))
                    {
                        port.Open();
                        await Task.Delay(1000); // Esperar que el puerto esté listo

                        // Mandar el comando RESET
                        port.WriteLine("RESET");
                        label1.Text = $"Se envió RESET a {portName}";
                        break;
                    }
                }
                catch
                {
                    // Si falla en un puerto, seguir intentando
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (puertoActivo != null && puertoActivo.IsOpen)
            {
                tiempo = 0;
                graficar = true;
                chart1.Series[0].Points.Clear();
                label1.Text = "Graficando temperatura...";
                puertoActivo.WriteLine("INICIO");
            }
            else
            {
                MessageBox.Show("Primero detecta y conecta un dispositivo.");
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            graficar = false;
            label1.Text = "Gráfico detenido.";
            puertoActivo.WriteLine("PARAR");
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            heartbeatTimer = new System.Windows.Forms.Timer();
            heartbeatTimer.Interval = 2000; // 2 segundos
            heartbeatTimer.Tick += HeartbeatTimer_Tick;
            heartbeatTimer.Start();
        }

        private void HeartbeatTimer_Tick(object sender, EventArgs e)
        {
            if (puertoActivo != null && puertoActivo.IsOpen)
            {
                try
                {
                    puertoActivo.WriteLine("HB_PC");

                    // ¿Se perdió el Arduino?
                    if ((DateTime.Now - ultimaRespuestaArduino).TotalSeconds > 6)
                    {
                        graficar = false;
                        label1.Text = "Arduino desconectado (timeout).";
                        notifyIcon.ShowBalloonTip(3000, "Desconexión", "Arduino no responde.", ToolTipIcon.Warning);
                    }
                }
                catch
                {
                    // Si hay error en puerto, detener
                }
            }
        }

        private void IniciarDeteccionPlugAndPlay()
        {
            // Detectar conexión
            watcherConectar = new ManagementEventWatcher(
                new WqlEventQuery("SELECT * FROM Win32_DeviceChangeEvent WHERE EventType = 2")
            );
            watcherConectar.EventArrived += async (s, e) =>
            {
                this.Invoke((MethodInvoker)async delegate
                {
                    label1.Text = "Nuevo dispositivo conectado. Buscando...";
                    string deviceInfo = await DetectarDispositivoAsync();
                    if (!string.IsNullOrEmpty(deviceInfo))
                    {
                        notifyIcon.ShowBalloonTip(3000, "Plug & Play", $"{deviceInfo} detectado automáticamente.", ToolTipIcon.Info);
                        label1.Text = $"{deviceInfo} encontrado por evento.";
                    }
                });
            };
            watcherConectar.Start();

            // Detectar desconexión
            watcherDesconectar = new ManagementEventWatcher(
                new WqlEventQuery("SELECT * FROM Win32_DeviceChangeEvent WHERE EventType = 3")
            );
            watcherDesconectar.EventArrived += (s, e) =>
            {
                this.Invoke((MethodInvoker)delegate
                {
                    label1.Text = "Dispositivo desconectado.";
                    notifyIcon.ShowBalloonTip(3000, "Desconexión", "Un dispositivo fue retirado.", ToolTipIcon.Warning);
                });
            };
            watcherDesconectar.Start();
        }

    }
}
