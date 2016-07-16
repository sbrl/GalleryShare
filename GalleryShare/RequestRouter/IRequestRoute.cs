using System;
using System.Threading.Tasks;
using System.Net;
using System.Dynamic;

namespace GalleryShare.RequestRouter
{
	public interface IRequestRoute
	{
		/// <summary>
		/// The priority of the request route.
		/// Higher priority routes will always be chosen to handle requests over lower priority ones.
		/// Note that only 1 route may handle any given request.
		/// </summary>
		int Priority { get; }

		void SetParentServer(GalleryServer inGalleryServer);

		/// <summary>
		/// Works out whether the request route hander can handle a request to the given path.
		/// </summary>
		/// <param name="urlPath">The path to check to see if it can be handled.</param>
		/// <returns>Whether the request route can handle a request to the given path.</returns>
		bool CanHandle(string rawUrl, string requestedPath);
		/// <summary>
		/// Handles a HTTP request asynchronously.
		/// Note that the master request router will close the request for you, so you don't need to bother.
		/// </summary>
		/// <param name="cycle">The Http request to handle.</param>
		/// <param name="requestedPath">The transformed url path of the given request.</param>
		Task HandleRequestAsync(HttpListenerContext cycle, string requestedPath);
	}
}

