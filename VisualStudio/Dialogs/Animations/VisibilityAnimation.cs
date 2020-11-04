using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Animation;

namespace LaubPlusCo.VisualStudio.HelixTemplates.Dialogs.Animations
{
  public class VisibilityAnimation
  {
    public enum AnimationType
    {
      None,
      Fade
    }

    private const int AnimationDuration = 300;

    private static readonly Dictionary<FrameworkElement, bool> _hookedElements =
        new Dictionary<FrameworkElement, bool>();

    public static AnimationType GetAnimationType(DependencyObject obj)
    {
      return (AnimationType)obj.GetValue(AnimationTypeProperty);
    }

    public static void SetAnimationType(DependencyObject obj, AnimationType value)
    {
      obj.SetValue(AnimationTypeProperty, value);
    }

    public static readonly DependencyProperty AnimationTypeProperty =
        DependencyProperty.RegisterAttached(
            "AnimationType",
            typeof(AnimationType),
            typeof(VisibilityAnimation),
            new FrameworkPropertyMetadata(AnimationType.None,
                new PropertyChangedCallback(OnAnimationTypePropertyChanged)));

    private static void OnAnimationTypePropertyChanged(
        DependencyObject dependencyObject,
        DependencyPropertyChangedEventArgs e)
    {
      if (!(dependencyObject is FrameworkElement frameworkElement))
      {
        return;
      }
      if (GetAnimationType(frameworkElement) == AnimationType.None)
      {
        UnHookVisibilityChanges(frameworkElement);
        return;
      }
      HookVisibilityChanges(frameworkElement);
    }

    private static void HookVisibilityChanges(FrameworkElement frameworkElement)
    {
      _hookedElements.Add(frameworkElement, false);
    }

    private static void UnHookVisibilityChanges(FrameworkElement frameworkElement)
    {
      if (_hookedElements.ContainsKey(frameworkElement))
      {
        _hookedElements.Remove(frameworkElement);
      }
    }

    static VisibilityAnimation()
    {
      UIElement.VisibilityProperty.AddOwner(
          typeof(FrameworkElement),
          new FrameworkPropertyMetadata(
              Visibility.Visible,
              VisibilityChanged,
              CoerceVisibility));
    }

    private static void VisibilityChanged(
        DependencyObject dependencyObject,
        DependencyPropertyChangedEventArgs e)
    {
    }

    private static object CoerceVisibility(
        DependencyObject dependencyObject,
        object baseValue)
    {
      if (!(dependencyObject is FrameworkElement frameworkElement)
          || !(baseValue is Visibility visibility)
          || visibility == frameworkElement.Visibility
          || !IsHookedElement(frameworkElement)
          || UpdateAnimationStartedFlag(frameworkElement))
      {
        return baseValue;
      }

      var animation = new DoubleAnimation
      {
        Duration = new Duration(TimeSpan.FromMilliseconds(AnimationDuration))
      };

      animation.Completed += (sender, eventArgs) =>
      {
        SetVisibility(frameworkElement, visibility);

      };

      if (visibility == Visibility.Collapsed || visibility == Visibility.Hidden)
      {
        animation.From = 1.0;
        animation.To = 0.0;
      }
      else
      {
        animation.From = 0.0;
        animation.To = 1.0;
      }

      frameworkElement.BeginAnimation(UIElement.OpacityProperty, animation);

      return Visibility.Visible;
    }

    private static void SetVisibility(FrameworkElement frameworkElement, Visibility visibility)
    {
      if (visibility == Visibility.Visible)
      {
        UpdateAnimationStartedFlag(frameworkElement);
        return;
      }

      if (BindingOperations.IsDataBound(frameworkElement, UIElement.VisibilityProperty))
      {
        Binding bindingValue = BindingOperations.GetBinding(frameworkElement, UIElement.VisibilityProperty);
        BindingOperations.SetBinding(frameworkElement, UIElement.VisibilityProperty, bindingValue);
        return;
      }

      frameworkElement.Visibility = visibility;
    }

    private static bool IsHookedElement(FrameworkElement frameworkElement)
    {
      return _hookedElements.ContainsKey(frameworkElement);
    }

    private static bool UpdateAnimationStartedFlag(FrameworkElement frameworkElement)
    {
      var animationStarted = (bool)_hookedElements[frameworkElement];
      _hookedElements[frameworkElement] = !animationStarted;
      return animationStarted;
    }
  }
}
