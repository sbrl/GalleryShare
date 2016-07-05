using System;
using System.Net;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.IO;

namespace GalleryShare
{
	class GalleryServer 
	{
		HttpListener server = new HttpListener();
		string prefix;
		int port;

		public int Port { get { return port; } }


		public GalleryServer(int inPort)
		{
			port = inPort;

			prefix = string.Format("http://*:{0}/", Port);
			server.Prefixes.Add(prefix);
		}

		/// <summary>
		/// Synchronously starrts the server listening for requests.
		/// </summary>
		public void StartSync()
		{
			Task.WaitAll(Start());
		}

		/// <summary>
		/// Asynchronously starts the server listening for requests.
		/// </summary>
		public async Task Start()
		{
			server.Start();
			Console.WriteLine("Listening for requests on {0}.", prefix);
			Console.WriteLine("Browser url: http://localhost:{0}/", Port);

			while (true)
			{
				Utilities.ForgetTask(Handle(await server.GetContextAsync()));
			}
		}

		/// <summary>
		/// Handles the specified Http request.
		/// </summary>
		/// <param name="cycle">The Http request to handle.</param>
		private async Task Handle(HttpListenerContext cycle)
		{
			StreamWriter responseData = new StreamWriter(cycle.Response.OutputStream);

			await responseData.WriteLineAsync(string.Format("You requested {0}", cycle.Request.RawUrl));

			responseData.Close();
			cycle.Response.Close();

			Console.WriteLine("[{0}] [{1}] [{2}] {3} {4}",
				DateTime.Now.ToString("hh:mm tt"),
				cycle.Request.RemoteEndPoint,
				cycle.Response.StatusCode,
				cycle.Request.HttpMethod,
				cycle.Request.RawUrl
			);
		}
	}
}

