using System;

namespace GCTestApp.ErrorProvider
{
    /// <summary>
    /// Интерфейс вью.
    /// </summary>
    public interface IView
    {
        /// <summary>
        /// Событие закрытия вью-модели.
        /// </summary>
        event EventHandler Closed;
    }
}
