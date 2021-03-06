﻿using System;
using System.Collections.Generic;

namespace Rivets
{
	public interface IAppLinkTarget
	{
		Uri Url { get; set; }
		string AppName { get;set; }
	}

	public class AndroidAppLinkTarget : IAppLinkTarget
	{
		public Uri Url { get;set; }
		public string AppName { get;set; }
		public string Class { get;set; }
		public string Package { get;set; }
	}

	public class WindowsPhoneAppLinkTarget : IAppLinkTarget
	{
		public Uri Url { get;set; }
		public string AppName { get;set; }
		public string AppId { get;set; }
	}

	public class IOSAppLinkTarget : IAppLinkTarget
	{
		public Uri Url { get;set; }
		public string AppName { get;set; }
		public string AppStoreId { get;set; }
	}

	public class IPhoneAppLinkTarget : IOSAppLinkTarget
	{
	}

	public class IPadAppLinkTarget : IOSAppLinkTarget
	{
	}
}

