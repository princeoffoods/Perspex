﻿// -----------------------------------------------------------------------
// <copyright file="ItemsControl.cs" company="Steven Kirk">
// Copyright 2014 MIT Licence. See licence.md for more information.
// </copyright>
// -----------------------------------------------------------------------

namespace Perspex.Controls
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Perspex.Collections;
    using Perspex.Controls.Generators;
    using Perspex.Controls.Presenters;
    using Perspex.Controls.Primitives;
    using Perspex.VisualTree;

    public class ItemsControl : TemplatedControl, ILogical
    {
        [SuppressMessage("Microsoft.StyleCop.CSharp.NamingRules", "SA1202:ElementsMustBeOrderedByAccess", Justification = "Needs to be before or a NullReferenceException is thrown.")]
        private static readonly ItemsPanelTemplate DefaultPanel =
            new ItemsPanelTemplate(() => new StackPanel { Orientation = Orientation.Vertical });

        public static readonly PerspexProperty<IEnumerable> ItemsProperty =
            PerspexProperty.Register<ItemsControl, IEnumerable>("Items");

        public static readonly PerspexProperty<ItemsPanelTemplate> ItemsPanelProperty =
            PerspexProperty.Register<ItemsControl, ItemsPanelTemplate>("ItemsPanel", defaultValue: DefaultPanel);

        private ItemContainerGenerator itemContainerGenerator;

        private PerspexReadOnlyListView<IVisual, ILogical> logicalChildren;

        private ItemsPresenter presenter;

        public ItemsControl()
        {
            this.GetObservableWithHistory(ItemsProperty).Subscribe(this.ItemsChanged);
        }

        public ItemContainerGenerator ItemContainerGenerator
        {
            get
            {
                if (this.itemContainerGenerator == null)
                {
                    this.itemContainerGenerator = this.CreateItemContainerGenerator();
                }

                return this.itemContainerGenerator;
            }
        }

        public IEnumerable Items
        {
            get { return this.GetValue(ItemsProperty); }
            set { this.SetValue(ItemsProperty, value); }
        }

        public ItemsPanelTemplate ItemsPanel
        {
            get { return this.GetValue(ItemsPanelProperty); }
            set { this.SetValue(ItemsPanelProperty, value); }
        }

        IReadOnlyPerspexList<ILogical> ILogical.LogicalChildren
        {
            get
            {
                if (this.logicalChildren == null)
                {
                    this.logicalChildren = new PerspexReadOnlyListView<IVisual, ILogical>(
                        new PerspexList<IVisual>(), 
                        x => (ILogical)x);
                }

                return this.logicalChildren;
            }
        }

        protected virtual ItemContainerGenerator CreateItemContainerGenerator()
        {
            return new ItemContainerGenerator(this);
        }

        protected override void OnTemplateApplied()
        {
            if (this.logicalChildren != null)
            {
                this.logicalChildren.Dispose();
                this.logicalChildren = null;
            }

            this.presenter = this.FindTemplateChild<ItemsPresenter>("itemsPresenter");

            if (this.presenter != null)
            {
                var panel = (IVisual)this.presenter.GetVisualChildren().Single();
                this.logicalChildren = new PerspexReadOnlyListView<IVisual, ILogical>(
                    panel.VisualChildren,
                    x => (ILogical)x);
            }
        }

        private void ItemsChanged(Tuple<IEnumerable, IEnumerable> value)
        {
            INotifyPropertyChanged inpc = value.Item1 as INotifyPropertyChanged;

            if (inpc != null)
            {
                inpc.PropertyChanged -= this.ItemsPropertyChanged;
            }

            if (value.Item2 == null || !value.Item2.OfType<object>().Any())
            {
                this.Classes.Add(":empty");
            }
            else
            {
                this.Classes.Remove(":empty");
            }

            inpc = value.Item2 as INotifyPropertyChanged;

            if (inpc != null)
            {
                inpc.PropertyChanged += this.ItemsPropertyChanged;
            }
        }

        private void ItemsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Count")
            {
                if (((IList)sender).Count == 0)
                {
                    this.Classes.Add(":empty");
                }
                else
                {
                    this.Classes.Remove(":empty");
                }
            }
        }
    }
}
