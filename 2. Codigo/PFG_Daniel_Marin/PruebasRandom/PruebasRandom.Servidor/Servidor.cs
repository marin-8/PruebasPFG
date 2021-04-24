
using System;
using System.Net;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Diagnostics;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using ProyectoFinal.Comun;

namespace PruebasRandom
{
	public partial class Servidor : Form
	{
		private string ServerIP;
		private const int PORT = 1600;

		private ControladorRed servidor;

		private readonly ClienteControls[] ClientesControls = new ClienteControls[3];

		private readonly List<InfoCliente> InfoClientes = new();

		public Servidor()
		{
			InitializeComponent();

			FormClosing += new FormClosingEventHandler(Form1_Closed);

			Enviar1.Click += new EventHandler( (s,e) => EnviarMensaje(1) );
			Enviar2.Click += new EventHandler( (s,e) => EnviarMensaje(2) );
			Enviar3.Click += new EventHandler( (s,e) => EnviarMensaje(3) );

			ClientesControls[0] = new(){ IP=IP1, ListaMensajes=ListaMensajes1, MensajeEnviar=MensajeEnviar1, Enviar=Enviar1 };
			ClientesControls[1] = new(){ IP=IP2, ListaMensajes=ListaMensajes2, MensajeEnviar=MensajeEnviar2, Enviar=Enviar2 };
			ClientesControls[2] = new(){ IP=IP3, ListaMensajes=ListaMensajes3, MensajeEnviar=MensajeEnviar3, Enviar=Enviar3 };
		}

		private static List<AdaptadorDeRed> GetAdaptadoresDeRedDisponibles()
		{
			List<AdaptadorDeRed> adaptadoresDeRedDisponibles = new();

			foreach(NetworkInterface networkInterface in NetworkInterface.GetAllNetworkInterfaces())
			{
				if((networkInterface.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 ||
					networkInterface.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
				   && networkInterface.OperationalStatus == OperationalStatus.Up)
				{
					AdaptadorDeRed nuevoAdaptadorDeRedDisponible = new() { Name=networkInterface.Name };

					foreach (UnicastIPAddressInformation ip in networkInterface.GetIPProperties().UnicastAddresses)
					{
						if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
						{
							nuevoAdaptadorDeRedDisponible.IPs.Add(ip.Address.ToString());
						}
					}

					adaptadoresDeRedDisponibles.Add(nuevoAdaptadorDeRedDisponible);
				}  
			}

			return adaptadoresDeRedDisponibles;
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			var IPList = from _ in Dns.GetHostEntry(Dns.GetHostName()).AddressList where _.ToString().Contains('.') select _;
			var HostName = Dns.GetHostEntry(Dns.GetHostName()).HostName;
			var NetworkInterfaces = from networkAdapter in NetworkInterface.GetAllNetworkInterfaces() select networkAdapter;

			ServerIP = (from ip in GetAdaptadoresDeRedDisponibles() where ip.IPs[0].ToString().Contains("192") select ip.IPs[0]).First();
			
			IPServer.Text= $"{ServerIP} : {PORT}";

			servidor = new ControladorRed(ServerIP, PORT, CuandoRecibe, true);
		}

		private void Form1_Closed(object sender, EventArgs e)
		{
			servidor.Cerrar();
		}

		private void EnviarMensaje(int cliente)
		{
			//  TODO - Peta cuando intento enviar al móvil de mi madre

			string mensajeEnviar = ClientesControls[cliente-1].MensajeEnviar.Text;

			ControladorRed.Enviar
			(
				ClientesControls[cliente-1].IP.Text,
				PORT,
				ClientesControls[cliente-1].MensajeEnviar.Text
			);

			ClientesControls[cliente-1].ListaMensajes.Items.Add($"S > {mensajeEnviar}");
		}

		private void CuandoRecibe(string ipCliente, string mensaje)
		{
			if(InfoClientes.Count < 3)
			{
				Invoke(new Action(() =>
				{ 
					if(!InfoClientes.Select(ic => ic.IP).Contains(ipCliente))
					{
						InfoClientes.Add(new InfoCliente() { IP=ipCliente });

						ClientesControls[InfoClientes.Count-1].IP.Text = ipCliente;
						ClientesControls[InfoClientes.Count-1].ListaMensajes.Items.Add($"C > {mensaje}");

						ClientesControls[InfoClientes.Count-1].MensajeEnviar.Enabled = true;
						ClientesControls[InfoClientes.Count-1].Enviar.Enabled = true;
					}
					else
					{
						ClientesControls[InfoClientes.Select(ic => ic.IP).ToList().IndexOf(ipCliente)].ListaMensajes.Items.Add($"C > {mensaje}");
					}
				}));
			}
		}
	}

	public struct InfoCliente
	{
		public string IP;
		public ushort PORT;
	}

	public struct ClienteControls
	{
		public TextBox IP;

		public ListBox ListaMensajes;
		public TextBox MensajeEnviar;
		public Button Enviar;
	}

	public class AdaptadorDeRed
	{
		public string Name;
		public List<string> IPs = new();
	}
}
