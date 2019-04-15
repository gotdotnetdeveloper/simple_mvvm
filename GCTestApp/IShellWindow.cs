using System.Windows.Controls;

namespace GCTestApp
{
    /// <summary>
    /// Интерфейс основного окна приложения.
    /// </summary>
    public interface IShellWindow
    {
        /// <summary>
        /// Базовый фрейм приложения.
        /// </summary>
        ContentControl ShellFrame { get; }
    }
}