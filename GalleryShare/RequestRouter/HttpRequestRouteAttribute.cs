using System;

namespace GalleryShare.RequestRouter
{
	/// <summary>
	/// Defines a class to be a http reqeust router that's part of the given routing group.
	/// </summary>
	/// <remarks>
	/// This class is an attribute. Please don't try to inherit from it.
	/// To create a new request router, please implement GalleryShare.RequestRouter.IRequestRoute, not this class.
	/// </remarks>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public sealed class HttpRequestRoute : Attribute
	{
		public string RoutingGroup;

		public HttpRequestRoute(string inRoutingGroup)
		{
			RoutingGroup = inRoutingGroup;
		}
	}
}
