using System;
using GalleryShare.RequestRouter;
using System.Threading.Tasks;
using System.Net;
using System.IO;

namespace GalleryShare
{
	public class RouteDefault : IRequestRoute
	{
		public int Priority { get; } = 0;

		public string DefaultResponse { get; set; } = "Error: 404 - No route was found to handle the specified url.\n";

		public RouteDefault()
		{
		}

		public bool CanHandle(string rawUrl, string requestedPath)
		{
			return true;
		}

		public void SetParentServer(GalleryServer inParentServer)
		{
		}

		public async Task HandleRequestAsync(HttpListenerContext cycle, string requestedPath)
		{
			cycle.Response.StatusCode = 404;
			cycle.Response.ContentType = "text/plain";
			cycle.Response.ContentLength64 = DefaultResponse.Length;

			StreamWriter responseData = new StreamWriter(cycle.Response.OutputStream) { AutoFlush = true };
			await responseData.WriteLineAsync(DefaultResponse);
		}
	}
}

