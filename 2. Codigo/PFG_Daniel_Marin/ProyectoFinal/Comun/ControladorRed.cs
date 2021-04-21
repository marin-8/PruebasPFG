
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace ProyectoFinal.Comun
{
	public static class ControladorRed
	{
		private const ushort MAX_BUFFER_SIZE = 300;

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
	}
}
