
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

			servidor = new ControladorRed(Global.GetMiIP_Xamarin(), 1600, CuandoRecibe, true);
		}

		private void EnviarClicked(object sender, EventArgs args)
		{
			ControladorRed.Enviar(IP.Text, ushort.Parse(PORT.Text), MensajeEnviar.Text);

			Mensajes.Add(new ListView_Mensaje_Cell($"C > {MensajeEnviar.Text}"));
			Scroll_ListaMensajes_Final();
		}

		private void CuandoRecibe(string ipServidor, string mensaje)
		{
			Device.BeginInvokeOnMainThread (() =>
			{
				Mensajes.Add(new ListView_Mensaje_Cell($"S > {mensaje}"));
				Scroll_ListaMensajes_Final();
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