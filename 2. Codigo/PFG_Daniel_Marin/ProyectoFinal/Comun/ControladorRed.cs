
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

		private Action FuncionAlRecibir;

		public static int Enviar(string IP, ushort PORT, string Mensaje)
		{
			Socket destino = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

			int intentosDeConexion =
			Enviar_ConectarConDestino(destino, IP, PORT);
			Enviar_EnviarMensaje(destino, Mensaje);
			Enviar_RecibirConfirmacion(destino);
			Enviar_CerrarSockets(destino);

			return intentosDeConexion;
		}

		public ControladorRed(string IP, ushort PORT, Action FuncionAlRecibir)
		{
			Servidor.Bind(new IPEndPoint(IPAddress.Parse(IP), PORT));
			this.FuncionAlRecibir = FuncionAlRecibir;
		}

		public void EmpezarRecibir()
		{
			Servidor.Listen(0);
            Servidor.BeginAccept(Servidor_NuevaConexion, null);
		}

		public void PararRecibir()
		{
			Servidor.Shutdown(SocketShutdown.Both);
			Servidor.Close();
		}

		#region Enviar (Funciones Privadas)

		private static int Enviar_ConectarConDestino(Socket Destino, string IP, ushort PORT)
		{
			int intentosDeConexion = 0;

			while(!Destino.Connected)
			{
				intentosDeConexion++;

				try
				{
					Destino.Connect(new IPEndPoint(IPAddress.Parse(IP), PORT));
				}
				catch(SocketException) { }
			}

			return intentosDeConexion;
		}

		private static void Enviar_EnviarMensaje(Socket Destino, string Mensaje)
		{
			byte[] data = Encoding.ASCII.GetBytes(Mensaje);

			Destino.Send(data);
		}

		private static /*bool*/ void Enviar_RecibirConfirmacion(Socket Destino)
		{
			byte[] buffer = new byte[MAX_BUFFER_SIZE];

			Destino.ReceiveTimeout = 30 * 1000; // 30s

            /* int bytesRecibidos = */ Destino.Receive(buffer);

            // if(bytesRecibidos == 0) return;

            // byte[] data = new byte[bytesRecibidos];
            // Array.Copy(buffer, data, bytesRecibidos);
            // string respuestaDestino = Encoding.ASCII.GetString(data);

			// if(!respuestaDestino.Equals(OK)) return false;
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
            Socket nuevaConexion;

            try { nuevaConexion = Servidor.EndAccept(AR); } catch (ObjectDisposedException) { return; }

			nuevaConexion.BeginReceive(Buffer, 0, MAX_BUFFER_SIZE, SocketFlags.None, Servidor_Recibir, nuevaConexion);

			Servidor.BeginAccept(Servidor_NuevaConexion, null);
        }

		private void Servidor_Recibir(IAsyncResult AR)
        {
            Socket nuevaConexion = (Socket)AR.AsyncState;
            int numeroBytesRecibidos;

            try { numeroBytesRecibidos = nuevaConexion.EndReceive(AR); } catch (SocketException) { nuevaConexion.Close(); return; }

            byte[] bufferRecibido = new byte[numeroBytesRecibidos];
            Array.Copy(Buffer, bufferRecibido, numeroBytesRecibidos);

            string textoRecibido = Encoding.ASCII.GetString(bufferRecibido);

			byte[] data = Encoding.ASCII.GetBytes("OK");
            nuevaConexion.Send(data);

			FuncionAlRecibir();
        }

		#endregion
	}
}
