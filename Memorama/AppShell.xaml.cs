using Memorama.Views;

namespace Memorama
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            // Registramos la ruta para poder navegar a ella pasando parámetros
            Routing.RegisterRoute(nameof(GamePage), typeof(GamePage));
        }
    }
}
