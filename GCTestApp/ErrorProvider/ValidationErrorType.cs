using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTestApp.ErrorProvider
{
    /// <summary>
    /// Тип ошибки валидации
    /// </summary>
    public enum ValidationErrorType
    {
        /// <summary>
        /// Нет ошибки
        /// </summary>
        None = 0,
        /// <summary>
        /// Предупреждение
        /// </summary>
        Warning = 1,
        /// <summary>
        /// Ошибка на сервере
        /// </summary>
        Server = 2,
        /// <summary>
        /// Ошибка
        /// </summary>
        Error = 3

    }
}
