namespace Memorama.Views
{
    public partial class ThemeSelectionPage : ContentPage
    {
        public ThemeSelectionPage()
        {
            InitializeComponent();
        }

        private async void OnThemeSelected(object sender, EventArgs e)
        {
            // Evitamos múltiples clics rápidos deshabilitando temporalmente el botón
            if (sender is Button btn)
            {
                btn.IsEnabled = false;

                string temaSeleccionado = btn.CommandParameter.ToString();

                // Navegamos a GamePage y pasamos el tema como parámetro en la URL
                await Shell.Current.GoToAsync($"{nameof(GamePage)}?SelectedTheme={temaSeleccionado}");

                btn.IsEnabled = true;
            }
        }
    }
}