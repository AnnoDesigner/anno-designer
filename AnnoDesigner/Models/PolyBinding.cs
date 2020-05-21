using System.Windows;
using System.Windows.Input;

namespace AnnoDesigner.Models
{
    /// <summary>
    /// Implementation of the <see cref="InputBinding"/> <see langword="abstract"/> class. This binding can be used with any class
    /// that implements <see cref="InputGesture"/>.
    /// </summary>
    public class PolyBinding<T> : InputBinding, IDescriptiveHotkeyBinding where T: InputGesture
    {
        public PolyBinding() { }
        public PolyBinding(ICommand command) : this(command, null, null) { }
        public PolyBinding(ICommand command, T gesture) : this(command, gesture, null) { }
        public PolyBinding(ICommand command, T gesture, string description) : base(command, gesture)
        {
            Description = description;
        }

        public static readonly DependencyProperty GestureProperty = DependencyProperty.Register("Gesture", typeof(string), typeof(PolyBinding<T>));
        public new T Gesture
        {
            get { return (T)GetValue(GestureProperty); }
            set { SetValue(GestureProperty, value); }
        }

        public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register("Description", typeof(string), typeof(PolyBinding<T>));
        public string Description
        {
            get { return (string)GetValue(DescriptionProperty); }
            set { SetValue(DescriptionProperty, value); }
        }
    }
}
