using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace GCTestApp.Convertors
{
    /// <summary>
    /// Конвертер видимости объекта.
    /// </summary>
    public sealed class BooleanToVisibilityHideConverter : IValueConverter
    {
        #region Public Methods

        /// <summary>
        /// Прямое преобразование.
        /// </summary>
        /// <param name="value">Исходное значение.</param>
        /// <param name="targetType">Целевой тип.</param>
        /// <param name="parameter">Дополнительный параметр.</param>
        /// <param name="culture">Культура контекста преобразования.</param>
        /// <returns>Результата конвертации.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b && b)
            {
                return Visibility.Visible;
            }

            return Visibility.Hidden;
        }

        /// <summary>
        /// Обратное преобразование.
        /// </summary>
        /// <param name="value">Исходное значение.</param>
        /// <param name="targetType">Целевой тип.</param>
        /// <param name="parameter">Дополнительный параметр.</param>
        /// <param name="culture">Культура контекста преобразования.</param>
        /// <returns>Результата конвертации.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility && visibility == Visibility.Visible)
            {
                return true;
            }

            return false;
        }

        #endregion
    }


    /// <summary>
    /// Инвертированный конвертер видимости объекта.
    /// </summary>
    public sealed class InverseBooleanToVisibilityConverter : IValueConverter
    {
        #region Публичные методы

        /// <summary>
        /// Прямое преобразование.
        /// </summary>
        /// <param name="value">Исходное значение.</param>
        /// <param name="targetType">Целевой тип.</param>
        /// <param name="parameter">Дополнительный параметр.</param>
        /// <param name="culture">Культура контекста преобразования.</param>
        /// <returns>Результата конвертации.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if ((bool)value)
                {
                    return Visibility.Collapsed;
                }

                return Visibility.Visible;
            }
            catch (Exception)
            {
                return Visibility.Visible;
            }


        }

        /// <summary>
        /// Обратное преобразование.
        /// </summary>
        /// <param name="value">Исходное значение.</param>
        /// <param name="targetType">Целевой тип.</param>
        /// <param name="parameter">Дополнительный параметр.</param>
        /// <param name="culture">Культура контекста преобразования.</param>
        /// <returns>Результата конвертации.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Visibility visibility = (Visibility)value;

            if (visibility == Visibility.Collapsed)
            {
                return true;
            }

            return false;
        }

        #endregion
    }
}
