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

using System.Drawing;

namespace GalleryShare
{
	enum OutputFunction
	{
		None,
		SpecialFile,
		DirectoryListing,
		SendFile
	}

	class GalleryServer 
	{
		static MimeSharp.Mime mimeDB = new MimeSharp.Mime();

		int port;
		string servingDirectory = Environment.CurrentDirectory;
		Size thumbnailSize = new Size(300, 200);

		HttpListener server = new HttpListener();
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
			OutputFunction outFunction = OutputFunction.None;

			if (cycle.Request.RawUrl.StartsWith("/!"))
				outFunction = OutputFunction.SpecialFile;

			string requestedPath = GetFullReqestedPath(cycle.Request.RawUrl);
			if (Directory.Exists(requestedPath))
				outFunction = OutputFunction.DirectoryListing;
			if (File.Exists(requestedPath))
				outFunction = OutputFunction.SendFile;
			
			switch(outFunction)
			{
				case OutputFunction.SpecialFile:
					await sendSpecialFile(cycle);
					break;
				
				case OutputFunction.DirectoryListing:
					cycle.Response.ContentType = "application/xml";
					await sendDirectoryListing(cycle.Response.OutputStream, cycle.Request.RawUrl, requestedPath);
					break;
				
				case OutputFunction.SendFile:
					await sendFile(cycle, requestedPath);
					break;
				
				default:
					await sendMessage(cycle, 404, "Error: File or directory '{0}' not found.", requestedPath);
					break;
			}

			logCycle(cycle);
			cycle.Response.Close();
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

		private async Task sendMessage(HttpListenerContext cycle, int statusCode, string message, params object[] paramObjects)
		{
			StreamWriter responseData = new StreamWriter(cycle.Response.OutputStream);

			cycle.Response.StatusCode = statusCode;
			await responseData.WriteLineAsync(string.Format(message, paramObjects));
			/*responseData.Close();
			cycle.Response.Close();*/
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

		private async Task sendDirectoryListing(Stream outgoingData, string rawUrl, string requestedPath)
		{
			List<string> dirFiles = new List<string>(Directory.GetFiles(requestedPath));
			List<string> dirDirectories = new List<string>(Directory.GetDirectories(requestedPath));

			XmlWriterSettings writerSettings = new XmlWriterSettings();
			writerSettings.Async = true;
			writerSettings.Indent = true;
			writerSettings.IndentChars = "\t";
			XmlWriter xmlData = XmlWriter.Create(outgoingData, writerSettings);

			await xmlData.WriteStartDocumentAsync();
			await xmlData.WriteProcessingInstructionAsync("xml-stylesheet", "type=\"text/xsl\" href=\"/!Transform-DirListing.xslt\"");
			await xmlData.WriteStartElementAsync(null, "DirectoryListing", null);
			await xmlData.WriteElementStringAsync(null, "CurrentDirectory", null, rawUrl);
			await xmlData.WriteStartElementAsync(null, "Contents", null);

			foreach (string directoryName in dirDirectories)
			{
				await xmlData.WriteStartElementAsync(null, "ListingEntry", null);
				await xmlData.WriteAttributeStringAsync(null, "Type", null, "Directory");

				await xmlData.WriteElementStringAsync(null, "Name", null, "/" + directoryName.Substring(servingDirectory.Length));
				await xmlData.WriteElementStringAsync(null, "ItemCount", null, Directory.GetFileSystemEntries(directoryName).Length.ToString());

				// TODO: Write out thumbnail url

				await xmlData.WriteEndElementAsync();
			}
			foreach (string filename in dirFiles)
			{
				await xmlData.WriteStartElementAsync(null, "ListingEntry", null);
				await xmlData.WriteAttributeStringAsync(null, "Type", null, "File");

				await xmlData.WriteElementStringAsync(null, "Name", null, "/" + filename.Substring(servingDirectory.Length));

				await xmlData.WriteEndElementAsync();
			}

			await xmlData.WriteEndDocumentAsync();
			await xmlData.FlushAsync();
		}

		private async Task sendSpecialFile(HttpListenerContext cycle)
		{
			string specialFileName = cycle.Request.RawUrl.Substring(2);
			string outputFileName = string.Empty;

			switch(specialFileName)
			{
				case "Transform-DirListing.xslt":
					cycle.Response.ContentType = "text/xsl";
					outputFileName = @"GalleryShare.Embed.DirectoryListing.xslt";
					break;
				case "Theme.css":
					cycle.Response.ContentType = "text/css";
					outputFileName = @"GalleryShare.Embed.Theme.css";
					break;
			}

			if (outputFileName == string.Empty)
			{
				await sendMessage(cycle, 404, "Error: Unknown special file '{0}' requested.", specialFileName);
				return;
			}

			/*string[] resNames = Assembly.GetExecutingAssembly().GetManifestResourceNames();
			foreach (string resName in resNames)
				Console.WriteLine(resName);*/
			byte[] xsltData = await Utilities.GetEmbeddedResourceContent(outputFileName);
			await cycle.Response.OutputStream.WriteAsync(xsltData, 0, xsltData.Length);
		}

		private async Task sendFile(HttpListenerContext cycle, string requestedPath)
		{
			if(cycle.Request.QueryString["type"] == "thumbnail")
			{
				// Send a thumbnail!
				Console.WriteLine("Sending thumbnail for '{0}'", requestedPath);
				await ThumbnailGenerator.SendThumbnailPng(requestedPath, thumbnailSize, cycle);
				return;
			}

			// Send the raw file

			cycle.Response.ContentType = mimeDB.Lookup(requestedPath);

			Stream fileData = File.OpenRead(requestedPath);
			await fileData.CopyToAsync(cycle.Response.OutputStream);
		}
	}
}

