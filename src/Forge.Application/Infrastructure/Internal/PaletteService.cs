namespace Forge.Application.Infrastructure.Internal
{
    using System.Windows.Media;
    using MaterialDesignThemes.Wpf;

    internal class PaletteService : IPaletteService
    {
        public bool DarkMode { get; set; }

        public string LightModePrimary { get; set; }

        public string LightModeAccent { get; set; }

        public string DarkModePrimary { get; set; }

        public string DarkModeAccent { get; set; }

        public void RefreshTheme()
        {
            var paletteHelper = new PaletteHelper();
            var theme = paletteHelper.GetTheme();
            IBaseTheme baseTheme = DarkMode ? new MaterialDesignDarkTheme() : (IBaseTheme)new MaterialDesignLightTheme();
            theme.SetBaseTheme(baseTheme);
            paletteHelper.SetTheme(theme);
        }

        public void RefreshPalette()
        {
            var paletteHelper = new PaletteHelper();
            var theme = paletteHelper.GetTheme();
            if (this.DarkMode)
            {
                if (this.DarkModePrimary != null)
                {
                    theme.SetPrimaryColor((Color)ColorConverter.ConvertFromString(this.DarkModePrimary));
                }

                if (this.DarkModeAccent != null)
                {
                    theme.SetSecondaryColor((Color)ColorConverter.ConvertFromString(this.DarkModeAccent));
                }
            }
            else
            {
                if (this.LightModePrimary != null)
                {
                    theme.SetPrimaryColor((Color)ColorConverter.ConvertFromString(this.LightModePrimary));
                }

                if (this.LightModeAccent != null)
                {
                    theme.SetSecondaryColor((Color)ColorConverter.ConvertFromString(this.LightModeAccent));
                }
            }

            paletteHelper.SetTheme(theme);
        }
    }
}
