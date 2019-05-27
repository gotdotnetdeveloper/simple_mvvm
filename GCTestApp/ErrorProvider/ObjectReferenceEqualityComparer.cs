using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace GCTestApp.ErrorProvider
{
    /// <summary>
    /// Сравнение объектов по ссылке на объект.
    /// </summary>
    public class ObjectReferenceEqualityComparer : EqualityComparer<object>
    {
        #region Fields

        // ReSharper disable StaticFieldInGenericType
        private static IEqualityComparer<object> _defaultComparer;
        // ReSharper restore StaticFieldInGenericType

        #endregion

        #region Properties

        /// <summary>
        /// 
        /// </summary>
        public new static IEqualityComparer<object> Default
        {
            get
            {
                return _defaultComparer ?? (_defaultComparer = new ObjectReferenceEqualityComparer());
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public override bool Equals(object x, object y)
        {
            return ReferenceEquals(x, y);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override int GetHashCode(object obj)
        {
            return RuntimeHelpers.GetHashCode(obj);
        }

        #endregion
    }
}
