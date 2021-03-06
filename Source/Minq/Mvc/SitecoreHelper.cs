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
		private ISitecorePageMode _pageMode;

		public SitecoreHelper(ViewDataDictionary<TModel> viewData, ISitecoreMarkupStrategy markupStrategy, ISitecorePageMode pageMode)
		{
			_viewData = viewData;
			_markupStrategy = markupStrategy;
			_pageMode = pageMode;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="TAlternative"></typeparam>
		/// <param name="alternative"></param>
		/// <returns></returns>
		public SitecoreHelper<TAlternative> Helper<TAlternative>(TAlternative alternative)
		{
			return new SitecoreHelper<TAlternative>(new ViewDataDictionary<TAlternative>(alternative), _markupStrategy, _pageMode);
		}

		/// <summary>
		/// Returns the correct markup for a Sitecore field for each property in the object that is represented by the specified expression.
		/// </summary>
		/// <typeparam name="TProperty">The type of the value.</typeparam>
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

		public IHtmlString PageEditorFieldFor<TProperty>(Expression<Func<TModel, TProperty>> expression, object htmlAttributes = null)
		{
			return PageEditorFieldFor(expression, HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
		}

		private IHtmlString PageEditorFieldFor<TProperty>(Expression<Func<TModel, TProperty>> expression, IDictionary<string, object> htmlAttributes)
		{
			return IfPageEditor(FieldFor(expression, htmlAttributes));
		}

		public IHtmlString HiddenFor<TProperty>(Expression<Func<TModel, TProperty>> expression, object htmlAttributes = null)
		{
			return HiddenFor(expression, HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
		}

		private IHtmlString HiddenFor<TProperty>(Expression<Func<TModel, TProperty>> expression, IDictionary<string, object> htmlAttributes)
		{
			if (_pageMode.IsPageEditor)
			{
				return FieldFor(expression, htmlAttributes);
			}

			SitecoreFieldMetadata fieldMetadata = SitecoreFieldMetadata.FromLambdaExpression<TModel, TProperty>(expression, _viewData);

			string text = _markupStrategy.GetFieldValue(fieldMetadata);

			TagBuilder tagBuilder = new TagBuilder("input");

			tagBuilder.MergeAttributes<string, object>(htmlAttributes);
			tagBuilder.MergeAttribute("type", HtmlHelper.GetInputTypeString(InputType.Hidden));
			tagBuilder.MergeAttribute("value", HttpUtility.HtmlAttributeEncode(text));

			return new MvcHtmlString(tagBuilder.ToString(TagRenderMode.SelfClosing));
		}

		/// <summary>
		/// Returns submit button markup for a Sitecore field.
		/// </summary>
		/// <typeparam name="TProperty">The type of the value.</typeparam>
		/// <param name="expression">An expression that identifies the object that contains the properties to render.</param>
		/// <param name="htmlAttributes">An object that contains the HTML attributes to set for the element.</param>
		/// <returns>Submit button markup for the Sitecore field.</returns>
		public IHtmlString SubmitFor<TProperty>(Expression<Func<TModel, TProperty>> expression, object htmlAttributes = null)
		{
			return SubmitFor(expression, HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
		}

		private IHtmlString SubmitFor<TProperty>(Expression<Func<TModel, TProperty>> expression, IDictionary<string, object> htmlAttributes)
		{
			if (_pageMode.IsPageEditor)
			{
				return FieldFor(expression, htmlAttributes);
			}

			SitecoreFieldMetadata fieldMetadata = SitecoreFieldMetadata.FromLambdaExpression<TModel, TProperty>(expression, _viewData);

			string text = _markupStrategy.GetFieldValue(fieldMetadata);

			TagBuilder tagBuilder = new TagBuilder("input");

			tagBuilder.MergeAttributes<string, object>(htmlAttributes);
			tagBuilder.MergeAttribute("type", "submit");
			tagBuilder.MergeAttribute("value", HttpUtility.HtmlAttributeEncode(text));

			return new MvcHtmlString(tagBuilder.ToString(TagRenderMode.SelfClosing));
		}

		private bool IsAuthoring
		{
			get
			{
				return _pageMode.IsPageEditor;
			}
		}

		/// <summary>
		/// A HTML filter than that outputs the given string if you are accessing the Sitecore authoring environment.
		/// </summary>
		/// <param name="htmlString">The HTML string to output.</param>
		/// <returns>The HTML string if you are accessing the Sitecore authoring environment; null otherwise.</returns>
		public IHtmlString IfAuthoring(IHtmlString htmlString)
		{
			if (IsAuthoring)
			{
				return htmlString;
			}

			return null;
		}

		/// <summary>
		/// A HTML filter than that outputs the given string if you are accessing the Sitecore authoring environment.
		/// </summary>
		/// <param name="htmlPredicate">The HTML rendering predicate to generate the output.</param>
		/// <returns>The HTML string if you are accessing the Sitecore authoring environment; null otherwise.</returns>
		public IHtmlString IfAuthoring(Func<object, object> htmlPredicate)
		{
			if (IsAuthoring)
			{
				object helperResult = htmlPredicate(null);

				if (helperResult != null)
				{
					return new HtmlString(helperResult.ToString());
				}
			}

			return null;
		}

		/// <summary>
		/// A HTML filter than that outputs the given string if you are accessing the Sitecore authoring environment.
		/// </summary>
		/// <param name="htmlPredicate">The HTML rendering predicate to generate the output.</param>
		/// <returns>The HTML string if you are accessing the Sitecore authoring environment; null otherwise.</returns>
		public IHtmlString IfAuthoring<THtmlString>(Func<THtmlString> htmlPredicate)
			where THtmlString : IHtmlString
		{
			return IfAuthoring(htmlPredicate());
		}

		private bool IsDelivering
		{
			get
			{
				return _pageMode.IsNormal || _pageMode.IsPreview;
			}
		}
		
		public IHtmlString IfDelivering(IHtmlString htmlString)
		{
			if (IsDelivering)
			{
				return htmlString;
			}

			return null;
		}

		public IHtmlString IfDelivering(Func<object, object> htmlPredicate)
		{
			if (IsDelivering)
			{
				object helperResult = htmlPredicate(null);

				if (helperResult != null)
				{
					return new HtmlString(helperResult.ToString());
				}
			}

			return null;
		}

		public IHtmlString IfDelivering<THtmlString>(Func<THtmlString> htmlPredicate)
			where THtmlString : IHtmlString
		{
			return IfDelivering(htmlPredicate());
		}

		private bool IsPreviewing
		{
			get
			{
				return _pageMode.IsPreview;
			}
		}

		public IHtmlString IfPreviewing(IHtmlString htmlString)
		{
			if (IsPreviewing)
			{
				return htmlString;
			}

			return null;
		}

		public IHtmlString IfPreviewing(Func<object, object> htmlPredicate)
		{
			if (IsPreviewing)
			{
				object helperResult = htmlPredicate(null);

				if (helperResult != null)
				{
					return new HtmlString(helperResult.ToString());
				}
			}

			return null;
		}

		public IHtmlString IfPreviewing<THtmlString>(Func<THtmlString> htmlPredicate)
			where THtmlString : IHtmlString
		{
			return IfPreviewing(htmlPredicate());
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

		public IHtmlString ResponsiveImageFor<TProperty>(Expression<Func<TModel, TProperty>> expression, object htmlAttributes = null)
		{
			return ResponsiveImageFor(expression, HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
		}

		private IHtmlString ResponsiveImageFor<TProperty>(Expression<Func<TModel, TProperty>> expression, IDictionary<string, object> htmlAttributes)
		{
			if (_pageMode.IsPageEditor)
			{
				return ImageFor(expression, htmlAttributes);
			}
			else
			{
				SitecoreFieldMetadata fieldMetadata = SitecoreFieldMetadata.FromLambdaExpression<TModel, TProperty>(expression, _viewData);

				SitecoreFieldAttributeDictionary fieldAttributes = SitecoreFieldAttributeDictionary.FromAttributes(htmlAttributes);

				ISitecoreFieldMarkup markup = _markupStrategy.GetFieldMarkup(fieldMetadata, fieldAttributes);

				string html = markup.GetHtml(null);

				html = SitecoreFieldMarkupParser.StripAttribute(html, "width");
				html = SitecoreFieldMarkupParser.StripAttribute(html, "height");

				return new HtmlString(html);
			}
		}

		public IHtmlString PageEditor(Func<object, object> htmlPredicate, object htmlAttributes = null)
		{
			return PageEditor(htmlPredicate, HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
		}

		private IHtmlString PageEditor(Func<object, object> htmlPredicate, IDictionary<string, object> htmlAttributes)
		{
			SitecoreFieldAttributeDictionary editorAttributes = SitecoreFieldAttributeDictionary.FromAttributes(htmlAttributes);

			SitecoreEditorMetadata metadata = SitecoreEditorMetadata.FromModel<TModel>(_viewData.Model);

			ISitecoreEditorMarkup markup = _markupStrategy.GetEditorMarkup(metadata, editorAttributes);

			object helperResult = htmlPredicate(_viewData.Model);

			if (helperResult != null)
			{
				return new HtmlString(markup.GetHtml(helperResult.ToString()));
			}

			return new HtmlString(markup.GetHtml(null));
		}

		#region Obsolete
		[Obsolete("USe IfPageEditor instead of IfAuthoring.")]
		public IHtmlString IfPageEditor(IHtmlString htmlString)
		{
			if (_pageMode.IsPageEditor)
			{
				return htmlString;
			}

			return null;
		}

		[Obsolete("USe IfPageEditor instead of IfAuthoring.")]
		public IHtmlString IfPageEditor(Func<object, object> htmlPredicate)
		{
			if (_pageMode.IsPageEditor)
			{
				object helperResult = htmlPredicate(null);

				if (helperResult != null)
				{
					return new HtmlString(helperResult.ToString());
				}
			}

			return null;
		}

		[Obsolete("USe IfPageEditor instead of IfAuthoring.")]
		public IHtmlString IfPageEditor<THtmlString>(Func<THtmlString> htmlPredicate)
			where THtmlString : IHtmlString
		{
			return IfPageEditor(htmlPredicate());
		}

		[Obsolete("USe IfDelivering instead of IfNormalPage.")]
		public IHtmlString IfNormalPage(IHtmlString htmlString)
		{
			if (_pageMode.IsNormal)
			{
				return htmlString;
			}

			return null;
		}

		[Obsolete("USe IfDelivering instead of IfNormalPage.")]
		public IHtmlString IfNormalPage(Func<object, object> htmlPredicate)
		{
			if (_pageMode.IsNormal)
			{
				object helperResult = htmlPredicate(null);

				if (helperResult != null)
				{
					return new HtmlString(helperResult.ToString());
				}
			}

			return null;
		}

		[Obsolete("USe IfDelivering instead of IfNormalPage.")]
		public IHtmlString IfNormalPage<THtmlString>(Func<THtmlString> htmlPredicate)
			where THtmlString : IHtmlString
		{
			return IfNormalPage(htmlPredicate());
		}

		[Obsolete("Use PageEditor method instead. This method will be phased out.")]
		public IHtmlString Editor(Func<object, object> htmlPredicate, object htmlAttributes = null)
		{
			return PageEditor(htmlPredicate, htmlAttributes);
        }
		#endregion
	}
}
