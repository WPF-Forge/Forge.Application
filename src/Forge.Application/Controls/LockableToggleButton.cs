namespace Forge.Application.Controls
{
    using System.Windows;
    using System.Windows.Controls.Primitives;

    internal class LockableToggleButton : ToggleButton
    {
        public static readonly DependencyProperty LockToggleProperty =
            DependencyProperty.Register("LockToggle", typeof(bool), typeof(LockableToggleButton),
                new UIPropertyMetadata(false));

        public LockableToggleButton()
        {
            this.SetResourceReference(StyleProperty, typeof(ToggleButton));
        }

        public bool LockToggle
        {
            get { return (bool)this.GetValue(LockToggleProperty); }
            set { this.SetValue(LockToggleProperty, value); }
        }

        protected override void OnToggle()
        {
            if (!this.LockToggle)
            {
                base.OnToggle();
            }
        }
    }
}
