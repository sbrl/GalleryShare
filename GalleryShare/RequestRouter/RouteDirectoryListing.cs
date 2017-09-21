using System;
using System.Net;
using System.Xml;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Drawing;

namespace GalleryShare.RequestRouter
{
	[HttpRequestRoute("GalleryShare")]
	public class RouteDirectoryListing : IRequestRoute
	{
		GalleryServer parentServer;

		public int Priority { get; } = 5;

		public Size thumbnailSize { get; private set; } = new Size(300, 200);

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
			if (cycle.Request.QueryString["type"] == "thumbnail")
			{
				await ThumbnailGenerator.SendThumbnailPng(requestedPath, thumbnailSize, cycle);
				return;
			}
			
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
			await xmlData.WriteElementStringAsync(null, "CurrentDirectory", null, cycle.Request.RawUrl);
			await xmlData.WriteStartElementAsync(null, "Contents", null);

			foreach (string directoryName in dirDirectories)
			{
				await xmlData.WriteStartElementAsync(null, "ListingEntry", null);
				await xmlData.WriteAttributeStringAsync(null, "Type", null, "Directory");

				string relativePath = directoryName.Substring(parentServer.ServingDirectory.Length);
				await xmlData.WriteElementStringAsync(null, "Name", null, escapePath("/" + relativePath.TrimStart("/".ToCharArray())));
				await xmlData.WriteElementStringAsync(null, "DisplayName", null, relativePath.Substring(relativePath.LastIndexOf("/") + 1).TrimStart("/".ToCharArray()));
				await xmlData.WriteElementStringAsync(null, "ItemCount", null, Directory.GetFileSystemEntries(directoryName).Length.ToString());

				await xmlData.WriteEndElementAsync();
			}
			foreach (string filename in dirFiles)
			{
				await xmlData.WriteStartElementAsync(null, "ListingEntry", null);
				await xmlData.WriteAttributeStringAsync(null, "Type", null, "File");

				string relativePath = filename.Substring(parentServer.ServingDirectory.Length);
				await xmlData.WriteElementStringAsync(null, "Name", null, escapePath("/" + relativePath.TrimStart("/".ToCharArray())));
				await xmlData.WriteElementStringAsync(null, "DisplayName", null, relativePath.Substring(relativePath.LastIndexOf("/") + 1).TrimStart("/".ToCharArray()));

				await xmlData.WriteEndElementAsync();
			}

			await xmlData.WriteEndDocumentAsync();
			await xmlData.FlushAsync();
		}

		/// <summary>
		/// Escapes a path to make it safe for sending to a browser.
		/// Does not escape forward slashes ('/').
		/// </summary>
		/// <param name="path">The path to escape.</param>
		/// <returns>An escaped version of the given path.</returns>
		private string escapePath(string path)
		{
			return Uri.EscapeDataString(path).Replace("%2F", "/");
		}
	}
}

