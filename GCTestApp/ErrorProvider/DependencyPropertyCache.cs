using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;

namespace GCTestApp.ErrorProvider
{
    /// <summary>
    /// The dependency property cache.
    /// </summary>
    internal sealed class DependencyPropertyCache
    {
        #region Fields

        /// <summary>
        /// The lazy instance.
        /// </summary>
        private static readonly Lazy<DependencyPropertyCache> LazyInstance = new Lazy<DependencyPropertyCache>(() => new DependencyPropertyCache());

        /// <summary>
        /// The cache.
        /// </summary>
        private readonly Dictionary<Type, List<DependencyProperty>> _cache;

        #endregion

        #region .ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyPropertyCache"/> class.
        /// </summary>
        public DependencyPropertyCache()
        {
            _cache = new Dictionary<Type, List<DependencyProperty>>();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets Instance.
        /// </summary>
        public static DependencyPropertyCache Instance
        {
            get { return LazyInstance.Value; }
        }

        /// <summary>
        /// Get dependency properties for type.
        /// </summary>
        /// <param name="type">
        /// The current type.
        /// </param>
        public IEnumerable<DependencyProperty> this[Type type]
        {
            get
            {
                List<DependencyProperty> dependencyProperties;

                if (!_cache.TryGetValue(type, out dependencyProperties))
                {
                    dependencyProperties = new List<DependencyProperty>();

                    var members = type.GetMembers(BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
                    foreach (var member in members)
                    {
                        DependencyProperty dp = null;

                        if (member.MemberType == MemberTypes.Field)
                        {
                            var field = (FieldInfo)member;
                            if (typeof(DependencyProperty).IsAssignableFrom(field.FieldType))
                            {
                                dp = (DependencyProperty)field.GetValue(null);
                            }
                        }
                        else if (member.MemberType == MemberTypes.Property)
                        {
                            var prop = (PropertyInfo)member;
                            if (typeof(DependencyProperty).IsAssignableFrom(prop.PropertyType))
                            {
                                dp = (DependencyProperty)prop.GetValue(null, null);
                            }
                        }

                        if (dp != null)
                        {
                            dependencyProperties.Add(dp);
                        }
                    }

                    _cache[type] = dependencyProperties;
                }

                return dependencyProperties;
            }
        }

        #endregion
    }
}
