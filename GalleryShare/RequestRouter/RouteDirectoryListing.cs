using System;
using System.Net;
using System.Xml;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace GalleryShare.RequestRouter
{
	[HttpRequestRoute("GalleryShare")]
	public class RouteDirectoryListing : IRequestRoute
	{
		GalleryServer parentServer;

		public int Priority { get; } = 5;

		public RouteDirectoryListing()
		{
		}

		public bool CanHandle(string rawUrl, string requestedPath)
		{
			if(Directory.Exists(requestedPath))
				return true;
			return false;
		}

		public void SetParentServer(GalleryServer inParentServer)
		{
			parentServer = inParentServer;
		}

		public async Task HandleRequestAsync(HttpListenerContext cycle, string requestedPath)
		{
			cycle.Response.ContentType = "application/xml";

			List<string> dirFiles = new List<string>(Directory.GetFiles(requestedPath));
			List<string> dirDirectories = new List<string>(Directory.GetDirectories(requestedPath));

			XmlWriterSettings writerSettings = new XmlWriterSettings();
			writerSettings.Async = true;
			writerSettings.Indent = true;
			writerSettings.IndentChars = "\t";
			XmlWriter xmlData = XmlWriter.Create(cycle.Response.OutputStream, writerSettings);

			await xmlData.WriteStartDocumentAsync();
			await xmlData.WriteProcessingInstructionAsync("xml-stylesheet", "type=\"text/xsl\" href=\"/!Transform-DirListing.xslt\"");
			await xmlData.WriteStartElementAsync(null, "DirectoryListing", null);
			await xmlData.WriteElementStringAsync(null, "CurrentDirectory", null, Uri.EscapeDataString(cycle.Request.RawUrl));
			await xmlData.WriteStartElementAsync(null, "Contents", null);

			foreach (string directoryName in dirDirectories)
			{
				await xmlData.WriteStartElementAsync(null, "ListingEntry", null);
				await xmlData.WriteAttributeStringAsync(null, "Type", null, "Directory");

				await xmlData.WriteElementStringAsync(null, "Name", null, Uri.EscapeDataString("/" + directoryName.Substring(parentServer.ServingDirectory.Length)));
				await xmlData.WriteElementStringAsync(null, "ItemCount", null, Directory.GetFileSystemEntries(directoryName).Length.ToString());

				await xmlData.WriteEndElementAsync();
			}
			foreach (string filename in dirFiles)
			{
				await xmlData.WriteStartElementAsync(null, "ListingEntry", null);
				await xmlData.WriteAttributeStringAsync(null, "Type", null, "File");

						await xmlData.WriteElementStringAsync(null, "Name", null, Uri.EscapeDataString("/" + filename.Substring(parentServer.ServingDirectory.Length)));

				await xmlData.WriteEndElementAsync();
			}

			await xmlData.WriteEndDocumentAsync();
			await xmlData.FlushAsync();
		}
	}
}

