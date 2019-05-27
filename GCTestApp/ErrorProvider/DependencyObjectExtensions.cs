using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace GCTestApp.ErrorProvider
{
    /// <summary>
    /// Класс предоставляет методы для поиска элементов по типу в дереве контролов.
    /// </summary>
    public static class DependencyObjectExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TBehavior"></typeparam>
        /// <param name="dependencyObject"></param>
        /// <param name="dependencyProperty"></param>
        /// <param name="creator"></param>
        /// <returns></returns>
        public static TBehavior GetOrCreateBehavior<TBehavior>(this DependencyObject dependencyObject, DependencyProperty dependencyProperty, Func<DependencyObject, TBehavior> creator) where TBehavior : class
        {
            var behavior = dependencyObject.GetValue(dependencyProperty) as TBehavior;
            if (behavior == null)
            {
                behavior = creator(dependencyObject);
                dependencyObject.SetValue(dependencyProperty, behavior);
            }

            return behavior;
        }

        /// <summary>
        /// Ищет родителя элемент заданного типа
        /// </summary>
        /// <typeparam name="T">
        /// Тип искомого объекта.
        /// </typeparam>
        /// <param name="child">
        /// The child.
        /// </param>
        /// <returns>
        /// Искомый объект или Null, если не найден.
        /// </returns>
        public static T FindParent<T>(this DependencyObject child) where T : class
        {
            DependencyObject parentObject = null;

            if (child is Visual || child is Visual3D)
            {
                try
                {
                    parentObject = VisualTreeHelper.GetParent(child);
                }
                catch (InvalidOperationException)
                {
                    try
                    {
                        parentObject = LogicalTreeHelper.GetParent(child);
                    }
                    catch (InvalidOperationException)
                    {
                    }
                }
            }
            else
            {
                try
                {
                    parentObject = LogicalTreeHelper.GetParent(child);
                }
                catch (InvalidOperationException)
                {
                }
            }

            if (parentObject == null)
            {
                // VisualTreeHelper иногда не находит родителя, поэтому пытаемся ему помочь =)
                var element = child as FrameworkElement;
                if (element != null)
                    parentObject = element.Parent;
            }
            if (parentObject == null)
                return null;

            var parent = parentObject as T;
            return parent ?? FindParent<T>(parentObject);
        }

        /// <summary>
        /// Ищет родителя элемент заданного типа или самого себя, если тип соответствует
        /// </summary>
        /// <typeparam name="T">
        /// Тип искомого объекта.
        /// </typeparam>
        /// <param name="child">
        /// The child.
        /// </param>
        /// <returns>
        /// Искомый объект или Null, если не найден.
        /// </returns>
        public static T FindParentOrSelf<T>(this DependencyObject child) where T : class
        {
            return (child as T) ?? child.FindParent<T>();
        }

        /// <summary>
        /// Найти наивысшего родителя в иерархии (не имеющего родительских элементов)
        /// </summary>
        /// <param name="child"></param>
        /// <returns></returns>
        public static DependencyObject FindTopParent(this DependencyObject child)
        {
            for (var next = child; next != null; next = child.FindParent<FrameworkElement>())
            {
                child = next;
            }
            return child;
        }

        /// <summary>
        /// Ищет всех детей заданого типа
        /// </summary>
        /// <typeparam name="T">
        /// Тип искомого объекта.
        /// </typeparam>
        /// <param name="obj">
        /// The parent.
        /// </param>
        /// <returns>
        /// Список искомых объектов.
        /// </returns>
        public static IEnumerable<DependencyObject> FindAllChildren<T>(this DependencyObject obj)
        {
            return FindAllChildren(obj, new TypeFindSpecification<T>());
        }

        public static IEnumerable<T> FindChilds<T>(this DependencyObject parent) where T : DependencyObject
        {
            if (parent == null)
                return null;

            var foundChilds = new List<T>();

            var childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (var i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                var childType = child as T;
                if (childType == null)
                {
                    foundChilds.AddRange(FindChilds<T>(child));
                }
                else
                {
                    foundChilds.Add((T)child);
                }
            }

            return foundChilds;
        }

        /// <summary>
        /// Ищет child элемент заданного типа.
        /// </summary>
        /// <param name="parent">
        /// The parent.
        /// </param>
        /// <typeparam name="T">
        /// Тип искомого объекта.
        /// </typeparam>
        /// <returns>
        /// Искомый объект или Null, если не найден.
        /// </returns>
        public static T FindChild<T>(this DependencyObject parent) where T : class //DependencyObject
        {
            if (parent == null)
                return null;

            T foundChild = null;

            var childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (var i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                var childType = child as T;
                if (childType == null)
                {
                    foundChild = FindChild<T>(child);
                    if (foundChild != null)
                        break;
                }
                else
                {
                    foundChild = (T)(object)child;
                    break;
                }
            }

            return foundChild;
        }

        /// <summary>
        /// Ищет всех детей 
        /// </summary>
        /// <param name="obj">
        /// The parent.
        /// </param>
        /// <param name="findSpecification">Спецификация поиска</param>
        /// <param name="excludeSpecification">Спецификация исключаемых элементов (вглубь их поиск не производится)</param>
        /// <returns>
        /// Список искомых объектов.
        /// </returns>
        public static IEnumerable<DependencyObject> FindAllChildren(this DependencyObject obj, IFindSpecification findSpecification, IFindSpecification excludeSpecification = null)
        {
            if (obj != null)
            {
                if (excludeSpecification != null && excludeSpecification.IsValidCondition(obj))
                {
                    yield break;
                }

                var isEqual = findSpecification.IsValidCondition(obj);

                if (isEqual)
                {
                    yield return obj;
                }

                for (var i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
                {
                    foreach (var child in FindAllChildren(VisualTreeHelper.GetChild(obj, i), findSpecification, excludeSpecification).Where(child => child != null))
                    {
                        yield return child;
                    }
                }
            }
        }

        /// <summary>
        /// Поиск строкового представления для текущего объекта.
        /// Если src равен null, то вернет null.
        /// Если src имеет тип string, то вернет src.
        /// Если src имеет тип TextBlock, то вернет значение свойства TextBlock.Text
        /// Иначе ищет первый дочерний TextBlock и возвращает значение свойства TextBlock.Text
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public static string FindNestedText(this object src)
        {
            if (src != null)
            {
                var text = src as string;
                if (text != null)
                    return text;

                var dobj = src as DependencyObject;
                if (dobj != null)
                {
                    var tb = (dobj as TextBlock) ?? dobj.FindChild<TextBlock>();
                    if (tb != null)
                        return tb.Text;
                }
            }

            return null;
        }
    }
}
