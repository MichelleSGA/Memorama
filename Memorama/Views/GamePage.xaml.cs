namespace Memorama.Views
{
    // QueryProperty intercepta el parámetro que enviamos desde ThemeSelectionPage
    [QueryProperty(nameof(SelectedTheme), "SelectedTheme")]
    public partial class GamePage : ContentPage
    {
        private string _selectedTheme = string.Empty;
        public string SelectedTheme
        {
            get => _selectedTheme;
            set
            {
                _selectedTheme = Uri.UnescapeDataString(value);
                CargarCartasPorTema(); // Se ejecuta al recibir el tema
            }
        }

        private string[] cartasIMG = [];
        private int maxPuntos;
        private bool cartasDeshabilitadas = false;
        private Grid cartaVolteada1;
        private Grid cartaVolteada2;
        private int puntos = 0;
        private string _fondoTema;
        public string FondoTema
        {
            get => _fondoTema;
            set
            {
                _fondoTema = value;
                OnPropertyChanged(); // Notifica al XAML que el valor cambió
            }
        }

        public GamePage()
        {
            InitializeComponent();
            // El juego NO se inicia aquí, se inicia en CargarCartasPorTema() una vez que recibe la propiedad
            // Establecemos el BindingContext para que el XAML pueda leer las propiedades de esta clase
            BindingContext = this;
        }

        private void CargarCartasPorTema()
        {
            // Asignar el paquete de imágenes según la elección del jugador
            switch (_selectedTheme)
            {
                case "TAKAX":
                    FondoTema = "fondo_takax.jpeg";
                    cartasIMG = new[] { "img1_mem.jpeg", "img2_mem.jpeg", "img3_mem.jpeg", "img4_mem.jpeg", "img5_mem.jpeg", "img6_mem.jpeg" };
                    break;
                case "Loteria":
                    FondoTema = "lot_7_reverso.png";
                    cartasIMG = new[] { "lot_1.png", "lot_2.png", "lot_3.png", "lot_4.png", "lot_5.png", "lot_6.png" };
                    break;
                case "Planetas":
                    FondoTema = "bg_planetas.jpg";
                    cartasIMG = new[] { "jupiter_carta.jpg", "marte_carta.jpg", "mercurio_carta.jpg", "saturno_carta.jpg", "tierra_carta.jpg", "venus_carta.jpg" };
                    break;
                default:
                    FondoTema = "fondo_takax.jpeg"; // Fallback seguro
                    cartasIMG = new[] { "img1_mem.jpeg", "img2_mem.jpeg", "img3_mem.jpeg", "img4_mem.jpeg", "img5_mem.jpeg", "img6_mem.jpeg" };
                    break;
            }

            maxPuntos = cartasIMG.Length * 100;
            IniciarMemorama();
        }

        private void IniciarMemorama()
        {
            puntos = 0;
            ActualizarPuntos();
            ResetearTurno(); // Limpia referencias anteriores

            // Forzar ocultamiento de cartas al reiniciar (limpia estado visual viejo)
            Tablero.Children.Clear();

            GenerarTablero();
        }

        private void GenerarTablero()
        {
            var listaCartas = cartasIMG.Concat(cartasIMG).ToList();
            MezclarArray(listaCartas);
            BindableLayout.SetItemsSource(Tablero, listaCartas);
        }

        private async void ManejarClicCarta(object sender, EventArgs e)
        {
            var cartaClickeada = (Grid)sender;
            var caraFrontal = (Image)cartaClickeada.Children[1];

            // 1. Validaciones: Agregamos cartaVolteada2 a la comprobación de seguridad
            if (cartasDeshabilitadas || cartaClickeada == cartaVolteada1 || cartaClickeada == cartaVolteada2 || caraFrontal.IsVisible)
                return;

            // 2. ASIGNACIÓN INMEDIATA ANTES DE LA ANIMACIÓN
            if (cartaVolteada1 == null)
            {
                cartaVolteada1 = cartaClickeada; // El juego ya sabe que esta carta está ocupada
                await AnimarVolteo(cartaClickeada, mostrarFrente: true);
            }
            else if (cartaVolteada2 == null)
            {
                cartaVolteada2 = cartaClickeada;
                cartasDeshabilitadas = true; // BLOQUEO INMEDIATO del resto del tablero

                await AnimarVolteo(cartaClickeada, mostrarFrente: true);

                // Ahora sí, comprobamos si son iguales
                await ComprobarCoincidencia();
            }
        }

        private async Task ComprobarCoincidencia()
        {
            if (cartaVolteada1.ClassId == cartaVolteada2.ClassId)
            {
                puntos += 100;
                ActualizarPuntos();
                ResetearTurno();

                if (puntos == maxPuntos)
                {
                    await Task.Delay(500);
                    bool jugarOtraVez = await DisplayAlert("¡Felicidades!", "¡Has completado este tablero!", "Volver a jugar", "Cambiar de tema");

                    if (jugarOtraVez)
                    {
                        IniciarMemorama();
                    }
                    else
                    {
                        await Shell.Current.GoToAsync("..");
                    }
                }
            }
            else
            {
                // Se espera un poco para que el jugador vea las cartas incorrectas
                await Task.Delay(800);

                // CAMBIO AQUÍ: Esperamos a que AMBAS animaciones terminen por completo
                await Task.WhenAll(
                    AnimarVolteo(cartaVolteada1, mostrarFrente: false),
                    AnimarVolteo(cartaVolteada2, mostrarFrente: false)
                );

                // El turno se resetea (y se desbloquea el tablero) HASTA que las cartas ya están boca abajo
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
                (lista[k], lista[n]) = (lista[n], lista[k]); // Sintaxis simplificada (Tuplas C# 7+)
            }
        }

        private void ActualizarPuntos() => PuntosLabel.Text = $"Puntos: {puntos}";

        private async Task AnimarVolteo(Grid carta, bool mostrarFrente)
        {
            var cartaBack = (Image)carta.Children[0];
            var cartaFront = (Image)carta.Children[1];

            await carta.RotateYTo(90, 150);
            cartaBack.IsVisible = !mostrarFrente;
            cartaFront.IsVisible = mostrarFrente;
            await carta.RotateYTo(mostrarFrente ? 180 : 0, 150);
        }

        // Sobrescribimos el ciclo de vida para destruir el estado de la partida si el usuario presiona "Back"
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            ResetearTurno();
            Tablero.Children.Clear(); // Libera la memoria de las vistas creadas al salir
        }
    }
}