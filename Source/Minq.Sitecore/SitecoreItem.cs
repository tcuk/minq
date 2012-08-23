﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScapiItem = global::Sitecore.Data.Items.Item;
using ScapiTemplateFieldItem = Sitecore.Data.Items.TemplateFieldItem;

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
		/// <param name="key">The <see cref="SitecoreItemKey" /> used to uniquely identify the item</param>
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
					_fields = new Dictionary<string, ISitecoreField>(StringComparer.OrdinalIgnoreCase);

					foreach (ScapiTemplateFieldItem scapiTemplateFieldItem in _scapiItem.Template.Fields)
					{
						string name = scapiTemplateFieldItem.Name;

						_fields[name] = new SitecoreField(_scapiItem.Fields[name]);
					}
				}

				return _fields;
			}
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
				if (_parent == null)
				{
					_parent = new SitecoreItem(_scapiItem.Parent);
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