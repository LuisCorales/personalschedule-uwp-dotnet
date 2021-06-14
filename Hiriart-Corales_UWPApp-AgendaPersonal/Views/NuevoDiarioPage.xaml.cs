using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Hiriart_Corales_UWPApp_AgendaPersonal.ViewModels;
using Hiriart_Corales_UWPApp_AgendaPersonal.Core.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Windows.UI.Popups;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Hiriart_Corales_UWPApp_AgendaPersonal.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class NuevoDiarioPage : Page
    {
        ObservableCollection<Evento> Eventos = new ObservableCollection<Evento>();//Lista para guardar los eventos que se ponen en el combobox
        public NuevoDiarioPage()
        {
            this.InitializeComponent();                      
        }

        private void VolverBoton_Click(object sender, RoutedEventArgs e)
        {
            //Llama a la vista DiarioPage
            this.Frame.Navigate(typeof(DiarioPage));
        }

        private async void AyudaBoton_Click(object sender, RoutedEventArgs e)
        {
            var ayuda = new MessageDialog("Utilice el simbolo de guardado para añadir una entrada \n" +
                "de dario, la flecha hacia la izquirda le permite volver a la pantalla principal de Diarios");
            ayuda.Title = "Información";
            await ayuda.ShowAsync();
        }

        private async void CrearBoton_Click(object sender, RoutedEventArgs e)//async para poder mostrar el message dialog
        {
            if (Validar())//Si estan llenos los campos, se crea el diario
            {
                List<int?> ids = new List<int?>();//Lista para IDs de evento
                foreach (Evento evento in this.eventosListBox.SelectedItems)//Saca los ID de los eventos seleccionados
                {
                    ids.Add(evento.EventoID);
                }
                bool exito = DiarioViewModel.CreateDiario((App.Current as App).ConnectionString,
                    (DateTimeOffset)this.fechaCalendarDatePicker.Date, this.contenidoTextBox.Text, ids);
                if (exito)//Quita los datos que se escribieron apra hacer mas entradas
                {
                    var errorBase = new MessageDialog("Entrada de diario guardada con éxito");
                    errorBase.Title = "Información";
                    await errorBase.ShowAsync();
                    this.Eventos.Clear();
                    this.fechaCalendarDatePicker.Date = null;
                    this.contenidoTextBox.Text = "";
                }
                else//Notifica que no se pudo crear la entrada por un error con la base
                {
                    var errorBase = new MessageDialog("Ha ocurrido un error con la base de datos, \nno se puede ingresar el diario");
                    errorBase.Title = "Error";
                    await errorBase.ShowAsync();
                }
            }
            else//Si no estan llenos los campos adecuados, no se puede crear la entrada, se notifica
            {
                var faltanDatos = new MessageDialog("No se han ingresado todos los datos, intente \nde nuevo");
                faltanDatos.Title = "Error";
                await faltanDatos.ShowAsync();
            }
        }

        private void FechaCalendarDatePicker_DateChanged(CalendarDatePicker sender, CalendarDatePickerDateChangedEventArgs args)
        {
            if (fechaCalendarDatePicker.Date!=null)//Si es null, no se debe ejecutar en null sino hay errores para llenar eventos
            {
                LlenarEventos();//Llena el comboBox con los eventos de la Fecha seleccionada
            }          
        }

        private void LlenarEventos()
        {
            this.Eventos = EventosViewModel.ReadEventos((App.Current as App).ConnectionString);//Obtener eventos
            ObservableCollection<Evento> eventosOtraFecha = new ObservableCollection<Evento>();//Lista para guardar eventos de la fecha del diario
            //Se filtran los eventos, solo los eventos de la fecha en que se quiere hacer la entrada
            foreach (Evento evento in Eventos)
            {
                DateTimeOffset soloDateView = (DateTimeOffset)fechaCalendarDatePicker.Date;//Fecha que tiene el picker
                DateTime fechaView = soloDateView.Date;
                if (!evento.Fecha.Date.Equals(fechaView))//Si no es la fecha seleccionada, poner en una lista de no deseados
                {
                    eventosOtraFecha.Add(evento);
                }
            }
            foreach (var evento in eventosOtraFecha)//Quitar no deseados del ItemsSource
            {
                this.Eventos.Remove(evento);
            }
            this.eventosListBox.ItemsSource = Eventos;
        }

        private bool Validar()//Valida si fecha y contendo estan llenos
        {
            if(!String.IsNullOrEmpty(this.contenidoTextBox.Text) && this.fechaCalendarDatePicker.Date!=null)
            {
                return true;//Si se llenaron los campos
            }
            else
            {
                return false;//No se llenaron los campos
            }
        }
    }
}
