﻿using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;


#if __ANDROID__
using Android.Content;
#endif

namespace Rivets
{
	public class AppLinkNavigator : IAppLinkNavigation
	{
		public AppLinkNavigator ()
		{
		}
			
		static AppLinkNavigator()
		{
			DefaultResolver = new HttpClientAppLinkResolver ();
		}

		public static IAppLinkResolver DefaultResolver { get; set; }

		public async Task<NavigationResult> Navigate (Uri url, AppLinkData appLinkData)
		{
			var appLink = await DefaultResolver.ResolveAppLinks (url);

			if (appLink != null)
				return await Navigate (appLink, appLinkData);

			return NavigationResult.Failed;
		}

		public async Task<NavigationResult> Navigate (string url, AppLinkData appLinkData)
		{
			var uri = new Uri (url);
			return await Navigate(uri, appLinkData);
		}

		#if PORTABLE
		public async Task<NavigationResult> Navigate (AppLink appLink, AppLinkData appLinkData)
		{
			throw new NotSupportedException ("You can't run this from the Portable Library.  Reference a platform Specific Library Instead");
		}
		#elif __IOS__
		public async Task<NavigationResult> Navigate (AppLink appLink, AppLinkData appLinkData)
		{
			return NavigationResult.Failed;
		}
		#elif __ANDROID__
		public async Task<NavigationResult> Navigate (AppLink appLink, AppLinkData appLinkData)
		{
			var context = Android.App.Application.Context;
			var pm = context.PackageManager;

			Intent eligibleTargetIntent = null;
			foreach (var t in appLink.Targets) {
				var target = t as AndroidAppLinkTarget;

				if (target == null)
					continue;

				Intent targetIntent = new Intent (Intent.ActionView);

				if (target.Url != null)
					targetIntent.SetData (Android.Net.Uri.Parse(target.Url.ToString()));
				else
					targetIntent.SetData (Android.Net.Uri.Parse(appLink.SourceUrl.ToString()));

				targetIntent.SetPackage (target.Package);

				if (target.Class != null)
					targetIntent.SetClassName (target.Package, target.Class);

				targetIntent.PutExtra ("al_applink_data", Newtonsoft.Json.JsonConvert.SerializeObject(appLinkData));

				var resolved = pm.ResolveActivity (targetIntent, Android.Content.PM.PackageInfoFlags.MatchDefaultOnly);
				if (resolved != null) {
					eligibleTargetIntent = targetIntent;
					break;
				}
			}

			if (eligibleTargetIntent != null) {
				context.StartActivity (eligibleTargetIntent);
				return NavigationResult.App;
			}

			// Fall back to the web if it's available
			if (appLink.WebUrl != null) {
				var appLinkDataJson = string.Empty;
				try {
					appLinkDataJson = Newtonsoft.Json.JsonConvert.SerializeObject (appLinkData);
				} catch (Exception e) {
					Console.WriteLine (e);
					return NavigationResult.Failed;
				}

				var builder = new UriBuilder (appLink.WebUrl);
				var query = System.Web.HttpUtility.ParseQueryString (builder.Query);
				query ["al_applink_data"] = appLinkDataJson;
				builder.Query = query.ToString ();
				var webUrl = builder.ToString ();
				Intent launchBrowserIntent = new Intent (Intent.ActionView, Android.Net.Uri.Parse(webUrl));
				context.StartActivity (launchBrowserIntent);
				return NavigationResult.Web;
			}

			return NavigationResult.Failed;
		}
		#else
		public async Task<NavigationResult> Navigate (AppLink appLink, AppLinkData appLinkData)
		{
			if (appLink.WebUrl != null) {
				System.Diagnostics.Process.Start(appLink.WebUrl.ToString());
				return NavigationResult.Web;
			}
			
			return NavigationResult.Failed;
		}
		#endif
	}	
}

