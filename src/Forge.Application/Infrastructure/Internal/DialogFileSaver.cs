namespace Forge.Application.Infrastructure.Internal
{
    using Microsoft.Win32;

    internal class DialogFileSaver : IFileSaver
    {
        public string GetFile(string fileName, string filter)
        {
            var saveFileDialog = new SaveFileDialog
            {
                FileName = fileName ?? string.Empty,
                Filter = filter
            };

            return saveFileDialog.ShowDialog() == true ? saveFileDialog.FileName : null;
        }
    }
}
