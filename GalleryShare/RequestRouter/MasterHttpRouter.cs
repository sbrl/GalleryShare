using System;
using System.Net;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Claims;
using System.Linq;
using System.Diagnostics;

namespace GalleryShare.RequestRouter
{
	public delegate string UrlPathTransformer(string rawUrl);

	public class MasterHttpRouter
	{
		public bool DebugMode = true;

		List<IRequestRoute> requestRoutes = new List<IRequestRoute>();
		UrlPathTransformer urlTransformer;

		public UrlPathTransformer UrlTransformer
		{
			get { return urlTransformer; }
			set { urlTransformer = value; }
		}

		public MasterHttpRouter(GalleryServer parentServer, string routingGroup)
		{
			// Add the default route
			requestRoutes.Add(new RouteDefault());
			// Search for and add the rest of the routes
			foreach(Type currentType in getRequestRouters(routingGroup))
			{
				IRequestRoute nextRoute = (IRequestRoute)Activator.CreateInstance(currentType);
				nextRoute.SetParentServer(parentServer);
				requestRoutes.Add(nextRoute);
			}

			// Sort the request reoutes by priority
			requestRoutes.Sort((routeA, routeB) => -routeA.Priority.CompareTo(routeB.Priority));
		}

		public async Task RouteRequest(HttpListenerContext cycle)
		{
			foreach(IRequestRoute currentRoute in requestRoutes)
			{
				if (DebugMode) Console.Write("Trying {0} (Priority {1}) - ", currentRoute.GetType(), currentRoute.Priority);
				string transformedUrl = UrlTransformer(cycle.Request.RawUrl);

				if (!currentRoute.CanHandle(cycle.Request.RawUrl, transformedUrl))
				{
					if(DebugMode) Console.WriteLine("false. Trying next route.");
					continue;
				}
				if(DebugMode) Console.WriteLine("true. Sending to route.");
				await currentRoute.HandleRequestAsync(cycle, transformedUrl);
				break;
			}
		}

		private IEnumerable<Type> getRequestRouters(string routingGroup)
		{
			foreach(Type requestRoute in Assembly.GetCallingAssembly().GetExportedTypes().Where(cType =>
				cType.GetInterfaces().Contains(typeof(IRequestRoute))))
			{
				foreach(HttpRequestRoute attr in requestRoute.GetCustomAttributes())
				{
					if(attr.RoutingGroup == routingGroup)
					{
						yield return requestRoute;
						break;
					}
				}
			}
		}
	}
}

