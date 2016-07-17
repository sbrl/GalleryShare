using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;

namespace GalleryShare.RequestRouter
{
	[HttpRequestRoute("GalleryShare")]
	public class RouteSpecialFile : IRequestRoute
	{
		public int Priority { get; } = 5;

		/// <summary>
		/// A dictionary mapping special file names to their actual filenames and content types.
		/// </summary>
		Dictionary<string, SpecialFileEntry> specialFileMap = new Dictionary<string, SpecialFileEntry>()
		{
			{ "Transform-DirListing.xslt", new SpecialFileEntry(@"GalleryShare.Embed.DirectoryListing.xslt", "text/xsl") },
			{ "Theme.css", new SpecialFileEntry(@"GalleryShare.Embed.Theme.css", "text/css") },
			{ "images/Background-Texture.png", new SpecialFileEntry(@"GalleryShare.Embed.Background-Texture.png", "image/png") },
			{ "images/Background-Caption.png", new SpecialFileEntry(@"GalleryShare.Embed.Background-Caption.png", "image/png") },
			{ "images/Badge-License.svg", new SpecialFileEntry(@"GalleryShare.Embed.Badge-License.svg", "image/svg+xml") }
		};

		public RouteSpecialFile()
		{
		}

		public void SetParentServer(GalleryServer inParentServer)
		{
		}

		public bool CanHandle(string rawUrl, string requestedPath)
		{
			if (rawUrl.StartsWith("/!"))
				return true;
			return false;
		}

		public async Task HandleRequestAsync(HttpListenerContext cycle, string requestedPath)
		{
			string specialFileName = cycle.Request.RawUrl.Substring(2);
			string outputFileName = string.Empty;

			if(specialFileMap.ContainsKey(specialFileName))
			{
				outputFileName = specialFileMap[specialFileName].FileName;
				cycle.Response.ContentType = specialFileMap[specialFileName].ContentType;
			}

			if (outputFileName == string.Empty)
			{
				await Utilities.SendMessage(cycle, 404, "Error: Unknown special file '{0}' requested.", specialFileName);
				return;
			}

			byte[] xsltData = await Utilities.GetEmbeddedResourceContent(outputFileName);
			await cycle.Response.OutputStream.WriteAsync(xsltData, 0, xsltData.Length);
		}
	}

	class SpecialFileEntry 
	{
		public string FileName;
		public string ContentType;


		public SpecialFileEntry(string inFileName, string inContentType)
		{
			FileName = inFileName;
			ContentType = inContentType;
		}
	}
}
