using System;
using System.Net;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.IO;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using System.Xml;
using System.Reflection;

namespace GalleryShare
{
	class GalleryServer 
	{
		HttpListener server = new HttpListener();
		string prefix;
		int port;

		string servingDirectory = Environment.CurrentDirectory;

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
			if(cycle.Request.RawUrl == @"/!Transform-DirListing.xslt")
			{
				/*string[] resNames = Assembly.GetExecutingAssembly().GetManifestResourceNames();
				foreach (string resName in resNames)
					Console.WriteLine(resName);*/
				cycle.Response.ContentType = "text/xsl";
				byte[] xsltData = await Utilities.GetEmbeddedResourceContent(@"GalleryShare.XSLT.DirectoryListing.xslt");
				cycle.Response.OutputStream.WriteAsync(xsltData, 0, xsltData.Length);
				cycle.Response.Close();
			}
			string requestedPath = Path.GetFullPath(Path.Combine(servingDirectory, "." + cycle.Request.RawUrl));

			if (!File.Exists(requestedPath) && !Directory.Exists(requestedPath))
			{
				await sendMessage(cycle, 404, "Error: File or directory '{0}' not found.", requestedPath);
				logCycle(cycle);
				return;
			}

			FileAttributes reqPathAttrs = File.GetAttributes(requestedPath);

			StreamWriter responseData = new StreamWriter(cycle.Response.OutputStream);

			if(reqPathAttrs.HasFlag(FileAttributes.Directory))
			{
				List<string> dirFiles = new List<string>(Directory.GetFiles(requestedPath));
				List<string> dirDirectories = new List<string>(Directory.GetDirectories(requestedPath));

				cycle.Response.ContentType = "text/xml";
				
				await responseData.FlushAsync();
				XmlWriterSettings writerSettings = new XmlWriterSettings();
				writerSettings.Async = true;
				XmlWriter xmlData = XmlWriter.Create(cycle.Response.OutputStream, writerSettings);

				await xmlData.WriteStartDocumentAsync();
				await xmlData.WriteProcessingInstructionAsync("xsl-stylesheet", "type=\"text/xsl\" href=\"/!Transform-DirListing.xslt\"");
				await xmlData.WriteStartElementAsync(null, "DirectoryListing", null);

				foreach (string directoryname in dirDirectories)
				{
					await xmlData.WriteStartElementAsync(null, "ListingEntry", null);
					await xmlData.WriteAttributeStringAsync(null, "Type", null, "Directory");

					await xmlData.WriteElementStringAsync(null, "Name", null, directoryname);

					// TODO: Write out the number of items in directory
					// TODO: Write out thumbnail url

					await xmlData.WriteEndElementAsync();
				}
				foreach (string filename in dirFiles)
				{
					await xmlData.WriteStartElementAsync(null, "ListingEntry", null);
					await xmlData.WriteAttributeStringAsync(null, "Type", null, "File");

					await xmlData.WriteElementStringAsync(null, "Name", null, filename);

					// TODO: Write out thumbnail url

					await xmlData.WriteEndElementAsync();
				}

				await xmlData.WriteEndDocumentAsync();
				await xmlData.FlushAsync();
			}

			responseData.Close();
			cycle.Response.Close();

			logCycle(cycle);
		}

		private async Task sendMessage(HttpListenerContext cycle, int statusCode, string message, params object[] paramObjects)
		{
			StreamWriter responseData = new StreamWriter(cycle.Response.OutputStream);

			cycle.Response.StatusCode = statusCode;
			await responseData.WriteLineAsync(string.Format(message, paramObjects));
			responseData.Close();
			cycle.Response.Close();
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

