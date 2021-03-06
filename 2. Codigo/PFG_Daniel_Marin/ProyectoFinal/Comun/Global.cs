
using System;
using System.Net;
using System.Linq;
using System.Collections.Generic;
using System.Net.NetworkInformation;

namespace ProyectoFinal.Comun
{
	public static class Global
	{
		public static List<AdaptadorDeRed> GetAdaptadoresDeRedDisponibles()
		{
			List<AdaptadorDeRed> adaptadoresDeRedDisponibles = new List<AdaptadorDeRed>();

			foreach(NetworkInterface networkInterface in NetworkInterface.GetAllNetworkInterfaces())
			{
				if((networkInterface.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 ||
					networkInterface.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
				   && networkInterface.OperationalStatus == OperationalStatus.Up)
				{
					AdaptadorDeRed nuevoAdaptadorDeRedDisponible = new AdaptadorDeRed() { Nombre=networkInterface.Name };

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

		public static string GetMiIP_Windows()
		{
			return (from ip in GetAdaptadoresDeRedDisponibles() where ip.IPs[0].ToString().Contains("192") select ip.IPs[0]).First();
		}

		public static string GetMiIP_Xamarin()
		{
			return Dns.GetHostAddresses(Dns.GetHostName())
				   .Where(IP => IP.ToString().Contains("192.168"))
				   .First()
				   .ToString();
		}
	}
}
