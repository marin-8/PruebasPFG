
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace ProyectoFinal.Comun
{
	public class ControladorRed
	{
		private const ushort MAX_BUFFER_SIZE = 300;

		private readonly byte[] Buffer = new byte[MAX_BUFFER_SIZE];

		private readonly Socket Servidor = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

		private readonly Action<string,ushort,string> FuncionAlRecibir;

		private int SiguientePuertoLibre = 1601;

		public static string Enviar(string IP, ushort PORT, string Mensaje)
		{
			Socket destino = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

			/*int intentosDeConexion =*/ Enviar_ConectarConDestino(destino, IP, PORT);
			Enviar_EnviarMensaje(destino, Mensaje);
			string respuestaDelServidor = Enviar_RecibirRespuesta(destino);
			Enviar_CerrarSockets(destino);

			return respuestaDelServidor;
		}

		public ControladorRed(string IP, ushort PORT, Action<string,ushort,string> FuncionAlRecibir, bool EmpezarRecibir)
		{
			Servidor.Bind(new IPEndPoint(IPAddress.Parse(IP), PORT));
			this.FuncionAlRecibir = FuncionAlRecibir;
			
			if(EmpezarRecibir) this.EmpezarRecibir();
		}

		public void EmpezarRecibir()
		{
			Servidor.Listen(0);
            Servidor.BeginAccept(Servidor_NuevaConexion, null);
		}

		public void Cerrar()
		{
			if(Servidor.Connected) Servidor.Shutdown(SocketShutdown.Both);
			Servidor.Close();
		}

		#region Enviar (Funciones Privadas)

		private static /*int*/ void Enviar_ConectarConDestino(Socket Destino, string IP, ushort PORT)
		{
			// int intentosDeConexion = 0;

			while(!Destino.Connected)
			{
				// intentosDeConexion++;

				try
				{
					Destino.Connect(new IPEndPoint(IPAddress.Parse(IP), PORT));
				}
				catch(SocketException) { }
			}

			// return intentosDeConexion;
		}

		private static void Enviar_EnviarMensaje(Socket Destino, string Mensaje)
		{
			byte[] data = Encoding.ASCII.GetBytes(Mensaje);

			Destino.Send(data);
		}

		private static string Enviar_RecibirRespuesta(Socket Destino)
		{
			byte[] buffer = new byte[MAX_BUFFER_SIZE];

			Destino.ReceiveTimeout = 30 * 1000; // 30s

            int bytesRecibidos = Destino.Receive(buffer);

			if (bytesRecibidos == 0) return "";

			byte[] data = new byte[bytesRecibidos];
			Array.Copy(buffer, data, bytesRecibidos);
			string respuestaDestino = Encoding.ASCII.GetString(data);

			return respuestaDestino;
		}

		private static void Enviar_CerrarSockets(Socket Destino)
		{
			Destino.Shutdown(SocketShutdown.Both);
			Destino.Close();
		}

		#endregion

		#region Servidor (Funciones Privadas)

		private void Servidor_NuevaConexion(IAsyncResult AR)
        {
            Socket cliente;

            try { cliente = Servidor.EndAccept(AR); } catch (ObjectDisposedException) { return; }

			cliente.BeginReceive(Buffer, 0, MAX_BUFFER_SIZE, SocketFlags.None, Servidor_Recibir, cliente);

			Servidor.BeginAccept(Servidor_NuevaConexion, null);
        }

		private void Servidor_Recibir(IAsyncResult AR)
        {
            Socket cliente = (Socket)AR.AsyncState;
            int numeroBytesRecibidos;

            try { numeroBytesRecibidos = cliente.EndReceive(AR); } catch (SocketException) { cliente.Close(); return; }

            byte[] bufferRecibido = new byte[numeroBytesRecibidos];
            Array.Copy(Buffer, bufferRecibido, numeroBytesRecibidos);

            string mensajeRecibido = Encoding.ASCII.GetString(bufferRecibido);

			string respuesta;
			IPEndPoint clienteInfo = (IPEndPoint)cliente.RemoteEndPoint;
			ushort puertoCliente;

			if(mensajeRecibido.Equals("PedirSiguientePuertoDisponible"))
			{
				respuesta = SiguientePuertoLibre++.ToString();
				puertoCliente = ushort.Parse(respuesta);
			}
			else
			{
				respuesta = "OK";
				puertoCliente = (ushort)clienteInfo.Port;
			}

			byte[] data = Encoding.ASCII.GetBytes(respuesta);
            cliente.Send(data);

			string ipCliente = clienteInfo.Address.ToString();
			//ushort puertoCliente = (ushort)clienteInfo.Port;
			FuncionAlRecibir(ipCliente, puertoCliente, mensajeRecibido);
        }

		#endregion
	}
}
