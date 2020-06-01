using System.Windows.Media;

namespace Forge.Application.Infrastructure.Internal
{
    using MaterialDesignThemes.Wpf;

    internal static class ThemeExtensions
    {
        public static void SetPrimaryColor(this ITheme theme, string primaryColor)
        {
            theme.SetPrimaryColor(ToColor(primaryColor));
        }

        public static void SetSecondaryColor(this ITheme theme, string accentColor)
        {
            theme.SetSecondaryColor(ToColor(accentColor));
        }

        private static Color ToColor(string hex)
        {
            return (Color) ColorConverter.ConvertFromString(hex);
        }
    }

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
            theme.SetBaseTheme(this.DarkMode ? Theme.Dark : Theme.Light);
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
                    theme.SetPrimaryColor(this.DarkModePrimary);
                }

                if (this.DarkModeAccent != null)
                {
                    theme.SetSecondaryColor(this.DarkModeAccent);
                }
            }
            else
            {
                if (this.LightModePrimary != null)
                {
                    theme.SetPrimaryColor(this.LightModePrimary);
                }

                if (this.LightModeAccent != null)
                {
                    theme.SetSecondaryColor(this.LightModeAccent);
                }
            }
        }
    }
}