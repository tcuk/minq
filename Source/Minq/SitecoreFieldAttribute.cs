﻿using System;
using System.Reflection;

namespace Minq
{
	/// <summary>
	/// When applied to the member of a type, specifies that the member is a Sitecore field and is serializable by the Sitecore engine.
	/// </summary>
	public sealed class SitecoreFieldAttribute : Attribute
	{
		private string _name;

		/// <summary>
		/// Creates a new instance of the <see cref="SitecoreFieldAttribute" />.
		/// </summary>
		/// <param name="name">The name of the Sitecore field.</param>
		public SitecoreFieldAttribute(string name)
		{
			if (String.IsNullOrEmpty(name))
			{
				throw new ArgumentNullException("name");
			}

			_name = name;
		}

		/// <summary>
		/// Gets the name of the Sitecore field the member represents.
		/// </summary>
		public string Name
		{
			get
			{
				return _name ?? "";
			}
		}

		/// <summary>
		/// Retrieves the <see cref="SitecoreFieldAttribute" /> for a given property.
		/// </summary>
		/// <param name="type">The type of the object to get the property for.</param>
		/// <param name="propertyName">The property to get the <see cref="SitecoreFieldAttribute" /> for.</param>
		/// <returns>The <see cref="SitecoreFieldAttribute" />, or null if not found.</returns>
		public static SitecoreFieldAttribute GetItemFieldAttribute(Type type, string propertyName)
		{
			PropertyInfo propertyInfo = type.GetProperty(propertyName, BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

			if (propertyInfo != null)
			{
				return (SitecoreFieldAttribute)Attribute.GetCustomAttribute(propertyInfo, typeof(SitecoreFieldAttribute));
			}
			
			return null;
		}
	}
}
