﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Minq.Mocks
{
	public class MockSitecoreMediaGateway : ISitecoreMediaGateway
	{
		private IDictionary<SitecoreItemKey, ISitecoreMedia> _items = new Dictionary<SitecoreItemKey, ISitecoreMedia>(new SitecoreItemKeyComparer());

		public void AddMedia(ISitecoreMedia media)
		{
			_items[media.Key] = media;
		}

		public ISitecoreMedia GetMedia(string keyOrPath, string languageName, string databaseName)
		{
			ISitecoreMedia media;

			Guid guid;

			if (Guid.TryParse(keyOrPath, out guid))
			{
				if (_items.TryGetValue(new SitecoreItemKey(guid, languageName, databaseName), out media))
				{
					return media;
				}
				else
				{
					throw new MockSitecoreItemGatewayException(String.Format("Sitecore item {0} does not exist", keyOrPath));
				}
			}
			else
			{
				throw new MockSitecoreItemGatewayException(String.Format("Path lookup not supported in mocking yet {0}", keyOrPath));
			}
		}
	}
}