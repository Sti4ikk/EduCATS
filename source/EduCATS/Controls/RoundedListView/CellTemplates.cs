using System;
using System.Collections.Generic;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace EduCATS.Controls.RoundedListView
{
	/// <summary>
	/// Lets <see cref="ViewCell"/>-based item templates (made for the
	/// deprecated <see cref="ListView"/>) be used in a <see cref="CollectionView"/>.
	/// </summary>
	public static class CellTemplates
	{
		/// <summary>
		/// Template creating a cell of the given type.
		/// </summary>
		public static DataTemplate FromCell(Type cellType) =>
			Adapt(new DataTemplate(cellType));

		/// <summary>
		/// Template creating a cell with the factory.
		/// </summary>
		public static DataTemplate FromCell(Func<object> createCell) =>
			Adapt(new DataTemplate(createCell));

		/// <summary>
		/// Make a template usable in a <see cref="CollectionView"/>:
		/// cells are hosted in <see cref="CellHost"/>, views are kept as is.
		/// </summary>
		public static DataTemplate Adapt(DataTemplate template)
		{
			if (template == null || template is DataTemplateSelector)
			{
				return template;
			}

			return new DataTemplate(() =>
			{
				var content = template.CreateContent();
				return content is ViewCell cell ? new CellHost(cell) : content;
			});
		}

		/// <summary>
		/// Make a template selector usable in a <see cref="CollectionView"/>.
		/// </summary>
		public static DataTemplateSelector Adapt(DataTemplateSelector selector) =>
			new AdaptingSelector(selector);

		/// <summary>
		/// Selector returning adapted templates.
		/// </summary>
		/// <remarks>
		/// The same adapted instance is returned for the same inner template:
		/// <see cref="CollectionView"/> recycles items by template.
		/// </remarks>
		class AdaptingSelector : DataTemplateSelector
		{
			readonly DataTemplateSelector _inner;
			readonly Dictionary<DataTemplate, DataTemplate> _adapted = new Dictionary<DataTemplate, DataTemplate>();

			public AdaptingSelector(DataTemplateSelector inner)
			{
				_inner = inner;
			}

			protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
			{
				var template = _inner.SelectTemplate(item, container);

				if (template == null)
				{
					return null;
				}

				if (!_adapted.TryGetValue(template, out var adapted))
				{
					adapted = Adapt(template);
					_adapted[template] = adapted;
				}

				return adapted;
			}
		}
	}

	/// <summary>
	/// Hosts the view of a <see cref="ViewCell"/> as a <see cref="CollectionView"/> item.
	/// </summary>
	/// <remarks>
	/// The binding context is passed to the cell itself too, so cells overriding
	/// <see cref="Cell.OnBindingContextChanged"/> keep working. Context actions
	/// (long-press menu of the ListView) become swipe items.
	/// </remarks>
	public class CellHost : ContentView
	{
		readonly ViewCell _cell;

		public CellHost(ViewCell cell)
		{
			_cell = cell;

			var view = cell.View;
			cell.View = null;

			Content = cell.ContextActions.Count == 0 ? view : createSwipeView(cell, view);
		}

		protected override void OnBindingContextChanged()
		{
			base.OnBindingContextChanged();
			_cell.BindingContext = BindingContext;
		}

		static SwipeView createSwipeView(ViewCell cell, View view)
		{
			var items = new SwipeItems { Mode = SwipeMode.Reveal };

			foreach (var menuItem in cell.ContextActions)
			{
				var swipeItem = new SwipeItem { BackgroundColor = Microsoft.Maui.Graphics.Color.FromArgb("#9E9E9E") };
				swipeItem.SetBinding(MenuItem.TextProperty, new Binding(nameof(MenuItem.Text), source: menuItem));
				swipeItem.Invoked += (sender, e) => ((IMenuItemController)menuItem).Activate();
				items.Add(swipeItem);
			}

			return new SwipeView { RightItems = items, Content = view };
		}
	}
}
