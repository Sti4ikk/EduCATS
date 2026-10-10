using System;
using System.Collections.Concurrent;
using EduCATS.Fonts;
using Microsoft.Maui.Controls;

namespace EduCATS.Helpers.Forms.Styles
{
	/// <summary>
	/// Text styles.
	/// </summary>
	/// <remarks>
	/// Label and button styles are shared: a new one was created for every
	/// label of every list cell. The key holds the current font and size,
	/// so changing them in the settings gives new styles.
	/// </remarks>
	public static class AppStyles
	{
		static readonly ConcurrentDictionary<(Type type, double size, string font, bool bold), Style> _styles =
			new ConcurrentDictionary<(Type type, double size, string font, bool bold), Style>();

		/// <summary>
		/// Entry style (a new one each time: pages add their own setters to it).
		/// </summary>
		public static Style GetEntryStyle(NamedSize size = NamedSize.Medium, bool bold = false) =>
			createStyle(
				typeof(Entry), Entry.FontSizeProperty, Entry.FontFamilyProperty, Entry.FontAttributesProperty,
				FontSizeController.GetSize(size, typeof(Entry)), FontsController.GetCurrentFont(bold), bold);

		/// <summary>
		/// Shared button style: must not be modified.
		/// </summary>
		public static Style GetButtonStyle(NamedSize size = NamedSize.Medium, bool bold = false) =>
			getSharedStyle(typeof(Button), Button.FontSizeProperty, Button.FontFamilyProperty, Button.FontAttributesProperty, size, bold);

		/// <summary>
		/// Shared label style: must not be modified.
		/// </summary>
		public static Style GetLabelStyle(NamedSize size = NamedSize.Medium, bool bold = false) =>
			getSharedStyle(typeof(Label), Label.FontSizeProperty, Label.FontFamilyProperty, Label.FontAttributesProperty, size, bold);

		static Style getSharedStyle(
			Type type,
			BindableProperty fontSizeProperty,
			BindableProperty fontFamilyProperty,
			BindableProperty fontAttributesProperty,
			NamedSize size,
			bool bold)
		{
			var fontSize = FontSizeController.GetSize(size, type);
			var font = FontsController.GetCurrentFont(bold);

			return _styles.GetOrAdd((type, fontSize, font, bold), _ =>
				createStyle(type, fontSizeProperty, fontFamilyProperty, fontAttributesProperty, fontSize, font, bold));
		}

		static Style createStyle(
			Type type,
			BindableProperty fontSizeProperty,
			BindableProperty fontFamilyProperty,
			BindableProperty fontAttributesProperty,
			double fontSize,
			string font,
			bool bold)
		{
			return new Style(type) {
				Setters = {
					getSetter(fontSizeProperty, fontSize),
					getSetter(fontFamilyProperty, font),
					getSetter(fontAttributesProperty, bold ? FontAttributes.Bold : FontAttributes.None)
				}
			};
		}

		static Setter getSetter(BindableProperty property, object value)
		{
			return new Setter {
				Property = property,
				Value = value
			};
		}
	}
}
