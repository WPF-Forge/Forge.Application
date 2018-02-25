namespace Forge.Application.Infrastructure.Internal
{
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
            new PaletteHelper().SetLightDark(this.DarkMode);
        }

        public void RefreshPalette()
        {
            var paletteHelper = new PaletteHelper();
            if (this.DarkMode)
            {
                if (this.DarkModePrimary != null)
                {
                    paletteHelper.ReplacePrimaryColor(this.DarkModePrimary);
                }

                if (this.DarkModeAccent != null)
                {
                    paletteHelper.ReplaceAccentColor(this.DarkModeAccent);
                }
            }
            else
            {
                if (this.LightModePrimary != null)
                {
                    paletteHelper.ReplacePrimaryColor(this.LightModePrimary);
                }

                if (this.LightModeAccent != null)
                {
                    paletteHelper.ReplaceAccentColor(this.LightModeAccent);
                }
            }
        }
    }
}
