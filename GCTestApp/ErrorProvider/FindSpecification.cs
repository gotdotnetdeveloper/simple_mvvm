using System.Linq;
using System.Windows;

namespace GCTestApp.ErrorProvider
{
    ///<summary>
    /// Спецификация поиска
    ///</summary>
    public interface IFindSpecification
    {
        ///<summary>
        /// Возвращает true, если условия воиска верно
        ///</summary>
        ///<param name="obj"></param>
        ///<returns></returns>
        bool IsValidCondition(DependencyObject obj);
    }

    ///<summary>
    /// Спецификация соответствия по типу
    ///</summary>
    public class TypeFindSpecification<TType> : IFindSpecification
    {
        #region IFindSpecification Members

        public virtual bool IsValidCondition(DependencyObject obj)
        {
            return typeof(TType) == obj.GetType();
        }

        #endregion
    }

    ///<summary>
    /// Спецификация соответствия по базовому типу
    ///</summary>
    public class BaseTypeSpecification<TBaseType> : IFindSpecification where TBaseType : class
    {
        #region IFindSpecification Members

        public bool IsValidCondition(DependencyObject obj)
        {
            return obj is TBaseType;
        }

        #endregion
    }

    ///<summary>
    /// Спецификация соответствия по базовому типу
    ///</summary>
    public class TypeOrBaseTypeSpecification<TType> : IFindSpecification where TType : class
    {
        #region IFindSpecification Members

        public virtual bool IsValidCondition(DependencyObject obj)
        {
            var baseTypeSpecification = new BaseTypeSpecification<TType>();
            var typeFindSpecification = new TypeFindSpecification<TType>();
            return typeFindSpecification.IsValidCondition(obj) || baseTypeSpecification.IsValidCondition(obj);
        }

        #endregion
    }


    ///<summary>
    /// Спецификация соответствия по реализуемому интерфейсу
    ///</summary>
    public class BaseInterfaseFindSpecification<TBaseInterfaseType> : IFindSpecification
    {
        #region IFindSpecification Members

        public bool IsValidCondition(DependencyObject obj)
        {
            return obj.GetType().GetInterfaces().Contains(typeof(TBaseInterfaseType));
        }

        #endregion
    }
}