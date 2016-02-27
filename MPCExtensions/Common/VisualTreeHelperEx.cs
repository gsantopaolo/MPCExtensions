using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace MPCExtensions.Common
{
    public static class VisualTreeHelperEx
    {
        #region child
        /// <summary>
        /// Gets the first child object in the visual tree that is of the templated type.
        /// </summary>
        /// <typeparam name="T">The type of the child that is wanted</typeparam>
        /// <param name="obj">The object whose child hierarchy should be searched.</param>
        /// <returns>The first object in the hierarchy below that is of the specified type</returns>
        public static T GetChildObject<T>(DependencyObject obj) where T : class
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child is T)
                    return child as T;
                else
                {
                    child = GetChildObject<T>(child) as DependencyObject;
                    if (child is T)
                        return child as T;
                }
            }
            return null;
        }

        public static List<T> GetChildObjects<T>(DependencyObject obj) where T : class
        {
            List<T> coll = new List<T>();
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child is T)
                    coll.Add(child as T);
                else
                {
                    child = GetChildObject<T>(child) as DependencyObject;
                    if (child is T)
                        coll.Add(child as T);
                }
            }
            return coll;
        }

        /// <summary>
        /// Finds a Child of a given item in the visual tree. 
        /// Usage: foundTextBox = WPFHelper.FindChild<TextBox>(Application.Current.MainWindow, "myTextBoxName");
        /// </summary>
        /// <param name="parent">A direct parent of the queried item.</param>
        /// <typeparam name="T">The type of the queried item.</typeparam>
        /// <param name="childName">x:Name or Name of child. </param>
        /// <returns>The first parent item that matches the submitted type parameter. 
        /// If not matching item can be found, a null parent is being returned.
        /// </returns>
        public static T FindChild<T>(DependencyObject parent, string childName) where T : DependencyObject
        {
            // Confirm parent and childName are valid.   
            if (parent == null) return null;
            T foundChild = null;
            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                // If the child is not of the request child type child    
                T childType = child as T;
                if (childType == null)
                {
                    // recursively drill down the tree      
                    foundChild = FindChild<T>(child, childName);

                    // If the child is found, break so we do not overwrite the found child.       
                    if (foundChild != null) break;
                }
                else if (!string.IsNullOrEmpty(childName))
                {
                    var frameworkElement = child as FrameworkElement;
                    // If the child's name is set for search      
                    if (frameworkElement != null && frameworkElement.Name == childName)
                    {
                        // if the child's name is of the request name        
                        foundChild = (T)child;
                        break;
                    }
                }
                else
                {
                    // child element found.      
                    foundChild = (T)child;
                    break;
                }
            }
            return foundChild;
        }

        /// <summary>
        /// Gets the first child object in the visual tree that is of the templated type.
        /// </summary>
        /// <typeparam name="T">The type of the child that is wanted</typeparam>
        /// <param name="obj">The object whose child hierarchy should be searched.</param>
        /// <returns>The first object in the hierarchy below that is of the specified type</returns>
        public static T FindGenericChild<T>(DependencyObject obj) where T : class
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child is T)
                    return child as T;
                else
                {
                    child = FindGenericChild<T>(child) as DependencyObject;
                    if (child is T)
                        return child as T;
                }
            }
            return null;
        }
        #endregion

        #region parent
        /// <summary>
        /// Gets the first parent object in the visual tree that is of the templated type.
        /// </summary>
        /// <typeparam name="T">The type of the parent that is wanted</typeparam>
        /// <param name="obj">The object whose parent hierarchy should be searched.</param>
        /// <param name="exactTypeMatch">if set to <c>true</c> only objects of the exact type (i.e. not classes inheriting from it) is returned.</param>
        /// <returns>The first object in the hierarchy above that is of the specified type (or subtype, depending on the <paramref name="exactTypeMatch"/>)</returns>
        public static T FindParent<T>(DependencyObject obj, bool exactTypeMatch) where T : DependencyObject
        {
            try
            {
                while (obj != null &&
                    (exactTypeMatch ? (obj.GetType() != typeof(T)) : !(obj is T)))
                {
                    
                        obj = VisualTreeHelper.GetParent(obj) as DependencyObject;
                }
                return obj as T;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the first parent object in the visual tree that is of the templated type.
        /// </summary>
        /// <typeparam name="T">The type of the parent that is wanted</typeparam>
        /// <param name="obj">The object whose parent hierarchy should be searched.</param>
        /// <returns>The first object in the hierarchy above that is of the specified type</returns>
        public static T FindParent<T>(DependencyObject obj) where T : DependencyObject
        {
            return FindParent<T>(obj, false);
        }


        /// <summary>
        /// Finds a parent of a given control/item on the visual tree.
        /// </summary>
        /// <typeparam name="T">Type of Parent</typeparam>
        /// <param name="child">Child whose parent is queried</param>
        /// <returns>Returns the first parent item that matched the type (T), if no match found then it will return null</returns>
        public static T TryFindParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);

            if (parentObject == null)
                return null;

            T parent = parentObject as T;

            if (parent != null)
                return parent;
            else
                return TryFindParent<T>(parentObject);
        }
        #endregion

        public static UIElement FindRoot(UIElement obj, bool isScatterView)
        {
           
            UIElement parent = VisualTreeHelper.GetParent(obj) as UIElement;

            if (parent == null )
                return obj;
            else if (isScatterView && parent is ContentPresenter /*&& ((FrameworkElement)parent).Name == "PART_PANEL"*/)
                return parent;
            else return FindRoot(parent, isScatterView);
        }
    }
}
