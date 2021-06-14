using System;
using System.Collections.ObjectModel;
using Hiriart_Corales_UWPApp_AgendaPersonal.Core.Models;
using Hiriart_Corales_UWPApp_AgendaPersonal.ViewModels;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
using static Hiriart_Corales_UWPApp_AgendaPersonal.ViewModels.MemosViewModel;

namespace Hiriart_Corales_UWPApp_AgendaPersonal.Views
{
    public sealed partial class NuevoMemoPage : Page
    {
        ObservableCollection<Evento> Eventos = new ObservableCollection<Evento>();
        public NuevoMemoPage()
        {
            InitializeComponent();
        }

        private async void AyudaBoton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var ayuda = new MessageDialog("Utilice el simbolo de guardado para añadir una entrada \n" +
                "de dario, la flecha hacia la izquirda le permite volver a la pantalla principal de Memos");
            ayuda.Title = "Información";
            await ayuda.ShowAsync();
        }

        private void VolverBoton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            //Llama a la vista DiarioPage
            this.Frame.Navigate(typeof(MemosPage));
        }

        private async void GuardarBoton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if (Validar())
            {
                Evento evento = (Evento)this.eventosListBox.SelectedItem;
                if (this.eventosListBox.SelectedItem == null)
                    evento = new Evento();

                bool exito = CreateMemo((App.Current as App).ConnectionString, this.contenidoTextBox.Text,
                    evento.EventoID);
                if (exito)
                {
                    var ingresoExito = new MessageDialog("Se ha creado el memo");
                    ingresoExito.Title = "Información";
                    await ingresoExito.ShowAsync();
                    //Resetear interfaz
                    this.fechaCalendarDatePicker.Date = null;
                    Eventos.Clear();
                    this.eventosListBox.ItemsSource = Eventos;
                    this.contenidoTextBox.Text = "";
                }
                else
                {
                    var errorBase = new MessageDialog("Ha ocurrido un error con la base de datos, \nno se puede ingresar el memo");
                    errorBase.Title = "Error";
                    await errorBase.ShowAsync();
                }
            }
            else
            {
                var faltanDatos = new MessageDialog("No se han ingresado todos los datos, intente \nde nuevo");
                faltanDatos.Title = "Error";
                await faltanDatos.ShowAsync();
            }
        }

        private bool Validar()
        {
            if (this.fechaCalendarDatePicker.Date!=null && this.eventosListBox.SelectedItem!=null &&!String.IsNullOrEmpty(this.contenidoTextBox.Text))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void LeerEventos(CalendarDatePicker sender, CalendarDatePickerDateChangedEventArgs args)
        {
            if (this.fechaCalendarDatePicker.Date!=null)
            {
                this.eventosListBox.ItemsSource = EventosRelacionados((App.Current as App).ConnectionString, (DateTimeOffset)this.fechaCalendarDatePicker.Date);
            }
            
        }
    }
}
