﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Linq.Expressions;
using System.Reflection;
using System.Collections.Concurrent;
using System.Web;
using System.Globalization;
using System.Web.Routing;
using System.Web.WebPages;

namespace Minq.Mvc
{
	/// <summary>
	/// Represents support for Sitecore controls in an application.
	/// </summary>
	public class SitecoreHelper<TModel>
	{
		private ViewDataDictionary<TModel> _viewData;
		private ISitecoreMarkupStrategy _markupStrategy;

		public SitecoreHelper(ViewDataDictionary<TModel> viewData, ISitecoreMarkupStrategy markupStrategy)
		{
			_viewData = viewData;
			_markupStrategy = markupStrategy;
		}

		/// <summary>
		/// Returns the correct markup for a Sitecore field for each property in the object that is represented by the specified expression.
		/// </summary>
		/// <typeparam name="TProperty">The type of the value.</typeparam>
		/// <param name="htmlHelper">The HTML helper instance that this method extends.</param>
		/// <param name="expression">An expression that identifies the object that contains the properties to render.</param>
		/// <param name="htmlAttributes">An object that contains the HTML attributes to set for the element.</param>
		/// <returns>Markup for the Sitecore field.</returns>
		public IHtmlString FieldFor<TProperty>(Expression<Func<TModel, TProperty>> expression, object htmlAttributes = null)
		{
			return FieldFor(expression, HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
		}

		private IHtmlString FieldFor<TProperty>(Expression<Func<TModel, TProperty>> expression, IDictionary<string, object> htmlAttributes)
		{
			SitecoreFieldMetadata fieldMetadata = SitecoreFieldMetadata.FromLambdaExpression<TModel, TProperty>(expression, _viewData);

			SitecoreFieldAttributeDictionary fieldAttributes = SitecoreFieldAttributeDictionary.FromAttributes(htmlAttributes);

			ISitecoreFieldMarkup markup = _markupStrategy.GetFieldMarkup(fieldMetadata, fieldAttributes);

			return new HtmlString(markup.GetHtml(null));
		}

		/// <summary>
		/// Returns the correct markup for a Sitecore hyperlink field for each property in the object that is represented by the specified expression.
		/// </summary>
		/// <typeparam name="TProperty">The type of the value.</typeparam>
		/// <param name="expression">An expression that identifies the object that contains the properties to render.</param>
		/// <param name="htmlAttributes">An object that contains the HTML attributes to set for the element.</param>
		/// <returns>>Markup for the Sitecore hyperlink.</returns>
		public SitecoreFieldString<TModel> LinkFor<TProperty>(Expression<Func<TModel, TProperty>> expression, object htmlAttributes = null)
		{
			return LinkFor<TProperty>(expression, HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
		}

		private SitecoreFieldString<TModel> LinkFor<TProperty>(Expression<Func<TModel, TProperty>> expression, IDictionary<string, object> htmlAttributes)
		{
			SitecoreFieldMetadata fieldMetadata = SitecoreFieldMetadata.FromLambdaExpression<TModel, TProperty>(expression, _viewData);

			SitecoreFieldAttributeDictionary fieldAttributes = SitecoreFieldAttributeDictionary.FromAttributes(htmlAttributes);

			ISitecoreFieldMarkup markup = _markupStrategy.GetFieldMarkup(fieldMetadata, fieldAttributes);

			return new SitecoreFieldString<TModel>(markup);
		}

		public IHtmlString ImageFor<TProperty>(Expression<Func<TModel, TProperty>> expression, object htmlAttributes = null)
		{
			return ImageFor(expression, HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
		}

		private IHtmlString ImageFor<TProperty>(Expression<Func<TModel, TProperty>> expression, IDictionary<string, object> htmlAttributes)
		{
			SitecoreFieldMetadata fieldMetadata = SitecoreFieldMetadata.FromLambdaExpression<TModel, TProperty>(expression, _viewData);

			SitecoreFieldAttributeDictionary fieldAttributes = SitecoreFieldAttributeDictionary.FromImageAttributes(htmlAttributes);

			ISitecoreFieldMarkup markup = _markupStrategy.GetFieldMarkup(fieldMetadata, fieldAttributes);

			return new HtmlString(markup.GetHtml(null));
		}

		public IHtmlString Editor(Func<object, object> htmlPredicate, object htmlAttributes = null)
		{
			return Editor(htmlPredicate, HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
		}

		private IHtmlString Editor(Func<object, object> htmlPredicate, IDictionary<string, object> htmlAttributes)
		{
			SitecoreFieldAttributeDictionary editorAttributes = SitecoreFieldAttributeDictionary.FromAttributes(htmlAttributes);

			ISitecoreEditorMarkup markup = _markupStrategy.GetEditorMarkup(editorAttributes);

			HelperResult helperResult = htmlPredicate(_viewData.Model) as HelperResult;

			if (helperResult != null)
			{
				return new HtmlString(markup.GetHtml(helperResult.ToString()));
			}

			return new HtmlString(markup.GetHtml(null));
		}
	}
}
