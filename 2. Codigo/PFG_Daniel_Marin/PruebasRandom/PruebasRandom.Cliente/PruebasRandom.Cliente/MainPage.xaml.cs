﻿
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using ProyectoFinal.Comun;

namespace PruebasRandom.Cliente
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class MainPage : ContentPage
	{
		private readonly Random random = new();

		private ControladorRed servidor;

		private readonly ObservableCollection<ListView_Mensaje_Cell> Mensajes = new();

		public MainPage()
		{
			InitializeComponent();

			ListaMensajes.ItemsSource = Mensajes;

			ushort PORT = ushort.Parse(ControladorRed.Enviar("192.168.1.136", 1600, "PedirSiguientePuertoDisponible"));

			servidor = new ControladorRed(Global.GetMiIP_Xamarin(), PORT, CuandoRecibe, true);
		}

		private void EnviarClicked(object sender, EventArgs args)
		{
			//string mensaje = MensajeEnviar.Text;

			//if (mensaje.Equals("")) return;
			//if (mensaje.Length > 24) mensaje = mensaje.Substring(0, 24);

			//byte[] data = Encoding.ASCII.GetBytes(mensaje);

			//Socket svr = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			//svr.Connect(IPAddress.Parse(IP.Text), int.Parse(PORT.Text));

			//svr.Send(data, 0, data.Length, SocketFlags.None);

			ControladorRed.Enviar(IP.Text, ushort.Parse(PORT.Text), MensajeEnviar.Text);
			Mensajes.Add(new ListView_Mensaje_Cell($"C > {MensajeEnviar.Text}"));
		}

		private void CuandoRecibe(string ipServidor, ushort puertoServidor, string mensaje)
		{
			Device.BeginInvokeOnMainThread (() =>
			{
				Mensajes.Add(new ListView_Mensaje_Cell($"S > {mensaje}"));
			});
		}

		private void Scroll_ListaMensajes_Final()
		{
			var ultimoMensaje = ListaMensajes.ItemsSource.Cast<object>().Last();
			ListaMensajes.ScrollTo(ultimoMensaje, ScrollToPosition.End, false);
		}
	}

	public class ListView_Mensaje_Cell
	{
		public string Mensaje { get; set; } = string.Empty;
		public string FechaHora { get; set; } = string.Empty;

		public ListView_Mensaje_Cell(string Mensaje)
		{
			this.Mensaje = Mensaje;
			FechaHora = DateTime.Now.ToString("dd/MM/yyyy H:mm:ss");
		}
	}
}