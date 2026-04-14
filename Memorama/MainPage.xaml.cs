namespace Memorama
{
    public partial class MainPage : ContentPage
    {
        private readonly string[] cartasIMG =
        {
        "img1_mem.jpeg", "img2_mem.jpeg", "img3_mem.jpeg",
        "img4_mem.jpeg", "img5_mem.jpeg", "img6_mem.jpeg"
        };
        private int maxPuntos;

        private bool cartasDeshabilitadas = false;
        private Grid cartaVolteada1 = null;
        private Grid cartaVolteada2 = null;
        private int puntos = 0;

        public MainPage()
        {
            InitializeComponent();
            maxPuntos = cartasIMG.Length * 100;
            IniciarMemorama();
        }

        private void IniciarMemorama()
        {
            puntos = 0;
            ActualizarPuntos();
            cartasDeshabilitadas = false;
            cartaVolteada1 = null;
            cartaVolteada2 = null;

            GenerarTablero();
        }

        private void GenerarTablero()
        {
            // Preparar y mezclar el arreglo de nombres de imágenes
            var listaCartas = cartasIMG.Concat(cartasIMG).ToList();
            MezclarArray(listaCartas);

            // Se envia la lista al XAML, el flexlayout creará automáticamente las cartas basándose en el DataTemplate
            BindableLayout.SetItemsSource(Tablero, listaCartas);
        }

        private async void ManejarClicCarta(object sender, EventArgs e)
        {
            var cartaClickeada = (Grid)sender;

            // Se obtiene la cara frontal (es el segundo elemento en el XAML, índice 1)
            var caraFrontal = (Image)cartaClickeada.Children[1];

            // Validaciones: Si el juego está pausado, o si se toca la misma carta, o si ya está destapada
            if (cartasDeshabilitadas || cartaClickeada == cartaVolteada1 || caraFrontal.IsVisible)
            {
                return;
            }

            // Se voltea la carta
            await AnimarVolteo(cartaClickeada, mostrarFrente: true);

            if (cartaVolteada1 == null)
            {
                cartaVolteada1 = cartaClickeada;
            }
            else
            {
                cartaVolteada2 = cartaClickeada;
                cartasDeshabilitadas = true;
                await ComprobarCoincidencia();
            }
        }

        private async Task ComprobarCoincidencia()
        {
            // El valor a comparar se guarda en el ClassId directamente desde el XAML
            if (cartaVolteada1.ClassId == cartaVolteada2.ClassId)
            {
                puntos += 100;
                ActualizarPuntos();
                ResetearTurno();

                if (puntos == maxPuntos)
                {
                    await Task.Delay(500);
                    await DisplayAlert("¡Ganaste!", "¡Has encontrado todos los pares!", "Jugar de nuevo");
                    IniciarMemorama();
                }
            }
            else
            {
                // Se espera un poco y se ocultan las cartas
                await Task.Delay(800);

                _ = AnimarVolteo(cartaVolteada1, mostrarFrente: false);
                _ = AnimarVolteo(cartaVolteada2, mostrarFrente: false);

                ResetearTurno();
            }
        }

        private void ResetearTurno()
        {
            cartaVolteada1 = null;
            cartaVolteada2 = null;
            cartasDeshabilitadas = false;
        }

        private void MezclarArray(List<string> lista)
        {
            Random rng = new Random();
            int n = lista.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                string value = lista[k];
                lista[k] = lista[n];
                lista[n] = value;
            }
        }

        private void ActualizarPuntos()
        {
            PuntosLabel.Text = $"Puntos: {puntos}";
        }

        private async Task AnimarVolteo(Grid carta, bool mostrarFrente)
        {
            var cartaBack = (Image)carta.Children[0];
            var cartaFront = (Image)carta.Children[1];

            // Efecto 3D de volteo
            await carta.RotateYTo(90, 150);
            cartaBack.IsVisible = !mostrarFrente;
            cartaFront.IsVisible = mostrarFrente;
            await carta.RotateYTo(mostrarFrente ? 180 : 0, 150);
        }
    }
}
