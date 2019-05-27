using System.Windows;
using System.Windows.Controls;

namespace GCTestApp.ErrorProvider
{
    /// <summary>
    /// Дополнтельные поведения Label
    ///     - Отслеживание, Required ли Target
    /// </summary>
    public static class LabelBehavior
    {
        /// <summary>
        /// Надо ли показывать метку Required (по умолч. False)
        /// </summary>
        public static readonly DependencyProperty ShowRequiredMarkProperty =
            DependencyProperty.RegisterAttached("ShowRequiredMark",
                typeof(bool), typeof(LabelBehavior),
                new UIPropertyMetadata(false, OnShowRequiredMarkChanged));

        private static readonly DependencyProperty IsTargetRequiredInternalProperty =
            DependencyProperty.RegisterAttached("IsTargetRequiredInternal",
                typeof(bool), typeof(LabelBehavior),
                new UIPropertyMetadata(false));

        private static readonly DependencyPropertyKey IsTargetRequiredPropertyKey =
            DependencyProperty.RegisterAttachedReadOnly("IsTargetRequired",
                typeof(bool), typeof(LabelBehavior), new UIPropertyMetadata(false));

        ///<summary>
        /// Сигнализирует о том, что поле, на которое ссылается метка, обязательно.
        ///</summary>
        public static readonly DependencyProperty IsTargetRequiredProperty =
            IsTargetRequiredPropertyKey.DependencyProperty;


        #region Attached Property ShowRequiredMark
        private static void OnShowRequiredMarkChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }

        ///<summary>
        /// Получить значение ShowRequiredMark
        ///</summary>
        ///<param name="label"></param>
        ///<returns></returns>
        public static bool GetShowRequiredMark(this Label label)
        {
            return (bool)label.GetValue(ShowRequiredMarkProperty);
        }

        ///<summary>
        /// Установить значение ShowRequiredMark
        ///</summary>
        ///<param name="label"></param>
        ///<param name="value"></param>
        public static void SetShowRequiredMark(this Label label, bool value)
        {
            label.SetValue(ShowRequiredMarkProperty, value);
            CalculateIsTargetRequired(label);
        }
        #endregion

        #region Attached Property IsTargetRequired
        ///<summary>
        /// Является ли поле, на которое ссылается метка, обязательным.
        ///</summary>
        ///<param name="lbl"></param>
        ///<returns></returns>
        public static bool GetIsTargetRequired(this Label lbl)
        {
            return (bool)lbl.GetValue(IsTargetRequiredProperty);
        }

        private static void SetIsTargetRequired(this Label lbl, bool value)
        {
            lbl.SetValue(IsTargetRequiredPropertyKey, value);
        }
        #endregion

        #region Attached Property IsTargetRequiredInternal
        internal static bool GetIsTargetRequiredInternal(this Label lbl)
        {
            return (bool)lbl.GetValue(IsTargetRequiredInternalProperty);
        }

        internal static void SetIsTargetRequiredInternal(this Label lbl, bool value)
        {
            lbl.SetValue(IsTargetRequiredInternalProperty, value);
            CalculateIsTargetRequired(lbl);
        }
        #endregion

        private static void CalculateIsTargetRequired(Label label)
        {
            label.SetIsTargetRequired(label.GetShowRequiredMark() && label.GetIsTargetRequiredInternal());
        }

    }
}
