using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace GCTestApp.ValidationLabel
{
    /// <summary>
    /// Описание элемента с отображением валидации.
    /// </summary>
    public class ValidationLabel : Control
    {
        static ValidationLabel()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ValidationLabel), new FrameworkPropertyMetadata(typeof(ValidationLabel)));
        }

        /// <summary>
        /// Элемент для валидации.
        /// </summary>
        public static readonly DependencyProperty AttachedElementProperty = DependencyProperty.Register("AttachedElement", typeof(FrameworkElement), typeof(ValidationLabel), new PropertyMetadata(null));
        /// <summary>
        /// Элемент для валидации.
        /// </summary>
        public FrameworkElement AttachedElement
        {
            get => (FrameworkElement)GetValue(AttachedElementProperty);
            set => SetValue(AttachedElementProperty, value);
        }

        /// <summary>
        /// Текст описания.
        /// </summary>
        public static readonly DependencyProperty LabelProperty = DependencyProperty.Register("Label", typeof(string), typeof(ValidationLabel), new PropertyMetadata(string.Empty));
        /// <summary>
        /// Текст описания.
        /// </summary>
        public string Label
        {
            get => (string)GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }
    }
}
