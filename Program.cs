using System;
using System.Windows;

namespace AutoArchiveX
{
    public static class Program
    {
        [STAThread]
        public static void Main()
        {
            try
            {
                Application app = new Application();
                app.Run(new MainWindow());
            }
            catch (Exception ex)
            {
                MessageBox.Show("A critical error occurred while running AutoArchive_X:\n\n" + ex.ToString(),
                    "AutoArchive_X - Critical Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}