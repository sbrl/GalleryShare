using System;
using System.Net;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using System.Xml;
using System.Reflection;

using System.Drawing;
using GalleryShare.RequestRouter;

namespace GalleryShare
{
	public class GalleryServer
	{
		int port;
		string servingDirectory = Environment.CurrentDirectory;

		HttpListener server = new HttpListener();
		MasterHttpRouter router;
		string prefix;

		Dictionary<string, string> pathReplacements = new Dictionary<string, string>()
		{
			{ "%20", " " }
		};

		public int Port { get { return port; } }
		public string ServingDirectory { get { return servingDirectory; } }

		public GalleryServer(string inServingDirectory, int inPort)
		{
			port = inPort;
			servingDirectory = inServingDirectory;

			string homeDir = Environment.GetEnvironmentVariable("HOME");
			if (homeDir != null)
				servingDirectory = servingDirectory.Replace("~", homeDir);

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
			Console.Write("Setting up router...");
			router = new MasterHttpRouter(this, "GalleryShare");
			router.UrlTransformer = GetFullReqestedPath;
			Console.WriteLine("done.");

			server.Start();
			Console.WriteLine("Listening for requests on {0}.", prefix);
			Console.WriteLine("Serving from {0}. Browser url: http://localhost:{1}/", servingDirectory, Port);

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
			IPEndPoint remoteEndpoint = cycle.Request.RemoteEndPoint;
			try
			{
				await router.RouteRequest(cycle);
				logCycle(cycle);
			}
			catch(Exception error)
			{
				Console.WriteLine("[{0}] [{1}] [Error] {2} ({3})",
					DateTime.Now.ToString("hh:m tt"),
					remoteEndpoint,
					cycle.Request.RawUrl,
					error.Message
				);
			}
			finally
			{
				cycle.Response.Close();
			}
		}

		private string GetFullReqestedPath(string rawUrl)
		{
			string result = Path.GetFullPath(Path.Combine(servingDirectory, "." + rawUrl));
			if(result.IndexOf("?") != -1)
				result = result.Substring(0, result.IndexOf("?"));
			foreach (KeyValuePair<string, string> replacePair in pathReplacements)
				result = result.Replace(replacePair.Key, replacePair.Value);
			return result;
		}

		private void logCycle(HttpListenerContext cycle)
		{
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

