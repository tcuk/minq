﻿using System;
using System.Collections.Generic;
using System.Linq;
using ScapiItem = global::Sitecore.Data.Items.Item;
using ScapiTemplateFieldItem = Sitecore.Data.Items.TemplateFieldItem;
using ScapiStandardValuesManager = Sitecore.Data.StandardValuesManager;
using ScapiVersionCollection = global::Sitecore.Collections.VersionCollection;
using ScapiItemManager = global::Sitecore.Data.Managers.ItemManager;
using ScapiVersionComparer = global::Sitecore.Data.VersionComparer;
using ScapiVersion = global::Sitecore.Data.Version;
using ScapiLinkManager = global::Sitecore.Links.LinkManager;
using ScapiUrlOptions = global::Sitecore.Links.UrlOptions;
using ScapaLanguageEmbedding = global::Sitecore.Links.LanguageEmbedding;

namespace Minq.Sitecore
{
	/// <summary>
	/// Provides a Sitecore item based on the <see ref="ISitecoreItem" /> interface.
	/// </summary>
	public class SitecoreItem : ISitecoreItem
	{
		private ScapiItem _scapiItem;
		private IDictionary<string, ISitecoreField> _fields;
		private SitecoreItem _parent;

		/// <summary>
		/// Initializes the class based on a <see cref="SitecoreItemKey"/>.
		/// </summary>
		/// <param name="sitecoreItem">The Sitecore item</param>
		public SitecoreItem(ScapiItem sitecoreItem)
		{
			_scapiItem = sitecoreItem;
		}

		/// <summary>
		/// Gets the <see cref="SitecoreItemKey" /> used to uniquely identify the item.
		/// </summary>
		public SitecoreItemKey Key
		{
			get
			{
				return new SitecoreItemKey(_scapiItem.ID.Guid, _scapiItem.Language.Name, _scapiItem.Database.Name);
			}
		}

		public SitecoreUrl Url
		{
			get
			{
				ScapiUrlOptions options = new ScapiUrlOptions
				{
					AddAspxExtension = ScapiLinkManager.AddAspxExtension,
					AlwaysIncludeServerUrl = true,
					LanguageEmbedding = ScapiLinkManager.LanguageEmbedding,
					LowercaseUrls = true,
					EncodeNames = true,
					UseDisplayName = ScapiLinkManager.UseDisplayName
				};

				if (ScapaLanguageEmbedding.Never != ScapiLinkManager.LanguageEmbedding)
				{
					options.Language = _scapiItem.Language;
				}

				string url = ScapiLinkManager.GetItemUrl(_scapiItem, options);
				
				return new SitecoreUrl(url);
			}
		}

		/// <summary>
		/// Gets the name of this item.
		/// </summary>
		public string Name
		{
			get
			{
				return _scapiItem.Name;
			}
		}

		/// <summary>
		/// Gets the versions for this item.
		/// </summary>
		public int[] Versions
		{
			get
			{
				ScapiVersionCollection versions = ScapiItemManager.GetVersions(_scapiItem);

				if (versions != null)
				{
					ScapiVersionComparer comparer = new ScapiVersionComparer();

					return versions
						.OrderBy<ScapiVersion, ScapiVersion>(version => version, comparer)
						.Select(version => version.Number)
						.ToArray();
				}

				return new int[0];
			}
		}

		/// <summary>
		/// Get the languages that this item exists in.
		/// </summary>
		public string[] Languages
		{
			get
			{
				return ScapiItemManager.GetContentLanguages(_scapiItem)
					.Select(language => language.Name)
					.ToArray();
			}
		}

		/// <summary>
		/// Gets all the Sitecore fields defined for this item
		/// based on its template definition.
		/// </summary>
		public IDictionary<string, ISitecoreField> FieldDictionary
		{
			get
			{
				if (_fields == null)
				{
					_fields = GetFieldDictionary();
				}

				return _fields;
			}
		}

		private Dictionary<string, ISitecoreField> GetFieldDictionary()
		{
			Dictionary<string, ISitecoreField> fields = new Dictionary<string, ISitecoreField>(StringComparer.OrdinalIgnoreCase);

			foreach (ScapiTemplateFieldItem scapiTemplateFieldItem in _scapiItem.Template.Fields)
			{
				string name = scapiTemplateFieldItem.Name;

				fields[name] = new SitecoreField(_scapiItem.Fields[name]);
			}

			return fields;
        }


		/// <summary>
		/// Gets all the Sitecore children defined for this item.
		/// </summary>
		public IEnumerable<ISitecoreItem> Children
		{
			get
			{
				foreach (ScapiItem scapiItem in _scapiItem.Children)
				{
					if (scapiItem != null)
					{
						yield return new SitecoreItem(scapiItem);
					}
				}
			}
		}

		public ISitecoreItem Parent
		{
			get
			{
				if (_scapiItem.Parent != null)
				{
					if (_parent == null)
					{
						_parent = new SitecoreItem(_scapiItem.Parent);
					}
				}

				return _parent;
			}
		}

		public SitecoreTemplateKey TemplateKey
		{
			get
			{
				return new SitecoreTemplateKey(_scapiItem.TemplateID.Guid, _scapiItem.Database.Name);
			}
		}
	}
}
