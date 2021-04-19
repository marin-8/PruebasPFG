
using System;
using System.Net;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Diagnostics;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Net.NetworkInformation;

namespace PruebasRandom
{
	public struct ClienteControls
	{
		public TextBox IP;
		public ListBox ListaMensajes;
		public TextBox MensajeEnviar;
	}

	public class AdaptadorDeRed
	{
		public string Name;
		public List<string> IPs = new();
	}

	public partial class Servidor : Form
	{
		private string ServerIP;
		private const int PORT = 100;

		private const int BUFFER_SIZE = 24;
		private readonly byte[] buffer = new byte[BUFFER_SIZE];

		private readonly Socket serverSocket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

		private readonly ClienteControls[] ClientesControls = new ClienteControls[3];

		public Servidor()
		{
			InitializeComponent();

			FormClosing += new FormClosingEventHandler(Form1_Closed);

			Enviar1.Click += new EventHandler( (s,e) => EnviarMensaje(1) );
			Enviar2.Click += new EventHandler( (s,e) => EnviarMensaje(2) );
			Enviar3.Click += new EventHandler( (s,e) => EnviarMensaje(3) );

			ClientesControls[0] = new(){ IP=IP1, ListaMensajes=ListaMensajes1, MensajeEnviar=MensajeEnviar1 };
			ClientesControls[1] = new(){ IP=IP2, ListaMensajes=ListaMensajes2, MensajeEnviar=MensajeEnviar2 };
			ClientesControls[2] = new(){ IP=IP3, ListaMensajes=ListaMensajes3, MensajeEnviar=MensajeEnviar3 };
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			var IPList = from _ in Dns.GetHostEntry(Dns.GetHostName()).AddressList where _.ToString().Contains('.') select _;
			var HostName = Dns.GetHostEntry(Dns.GetHostName()).HostName;
			var NetworkInterfaces = from networkAdapter in NetworkInterface.GetAllNetworkInterfaces() select networkAdapter;

			ServerIP = "127.0.0.1";

			ArrancarServidor(ServerIP);
		}

		private List<AdaptadorDeRed> GetAdaptadoresDeRedDisponibles()
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

		private void Form1_Closed(object sender, EventArgs e)
		{
			CerrarServidor();
		}

		private void ArrancarServidor(string IP)
		{
            serverSocket.Bind(new IPEndPoint(IPAddress.Parse(IP), PORT));

			Debug.WriteLine(((IPEndPoint)serverSocket.LocalEndPoint).Address);

            serverSocket.Listen(0);
            serverSocket.BeginAccept(AcceptCallback, null);
		}

		private void AcceptCallback(IAsyncResult AR)
        {
            Socket newClientSocket;

            try { newClientSocket = serverSocket.EndAccept(AR); } catch (ObjectDisposedException) { return; }

			newClientSocket.BeginReceive(buffer, 0, BUFFER_SIZE, SocketFlags.None, ReceiveCallback, newClientSocket);

			serverSocket.BeginAccept(AcceptCallback, null);
        }

		private void ReceiveCallback(IAsyncResult AR)
        {
            Socket socketCliente = (Socket)AR.AsyncState;
            int numeroBytesRecibidos;

            try { numeroBytesRecibidos = socketCliente.EndReceive(AR); } catch (SocketException) { socketCliente.Close(); return; }

            byte[] bufferRecibido = new byte[numeroBytesRecibidos];
            Array.Copy(buffer, bufferRecibido, numeroBytesRecibidos);
            string textoRecibido = Encoding.ASCII.GetString(bufferRecibido);

			Invoke(new Action( () => ListaMensajes1.Items.Add(textoRecibido) ));
			Debug.WriteLine(((IPEndPoint)socketCliente.RemoteEndPoint).Address);

            socketCliente.Shutdown(SocketShutdown.Both);
            socketCliente.Close();
        }

		private void CerrarServidor()
		{
            serverSocket.Close();
		}

		private void EnviarMensaje(int cliente)
		{
			string mensaje = ClientesControls[cliente-1].MensajeEnviar.Text;

			if (mensaje.Equals("")) return;

			if(mensaje.Length > 24) mensaje = mensaje.Substring(0, 24);

			Socket svr = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			svr.Connect(IPAddress.Parse(ClientesControls[cliente-1].IP.Text), PORT);

			byte[] data = Encoding.ASCII.GetBytes(mensaje);

			svr.Send(data, 0, data.Length, SocketFlags.None);

			svr.Shutdown(SocketShutdown.Both);
            svr.Close();
		}
	}
}
