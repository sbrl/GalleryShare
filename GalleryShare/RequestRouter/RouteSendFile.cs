using System;
using System.Threading.Tasks;
using System.Net;
using System.Drawing;
using System.IO;

namespace GalleryShare.RequestRouter
{
	[HttpRequestRoute("GalleryShare")]
	public class RouteSendFile : IRequestRoute
	{
		static MimeSharp.Mime mimeDB = new MimeSharp.Mime();


		Size thumbnailSize = new Size(300, 200);

		public int Priority { get; } = 5;

		public RouteSendFile()
		{
		}

		public void SetParentServer(GalleryServer inParentServer)
		{
		}

		/// <remarks>
		/// The SendFile route can only handle requests to paths that are a valid file on disk.
		/// </remarks>
		public bool CanHandle(string rawUrl, string requestedPath)
		{
			if (File.Exists(requestedPath))
				return true;
			return false;
		}

		public async Task HandleRequestAsync(HttpListenerContext cycle, string requestedPath)
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

