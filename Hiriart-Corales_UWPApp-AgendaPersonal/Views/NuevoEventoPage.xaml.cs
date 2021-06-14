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
    public sealed partial class NuevoEventoPage : Page
    {
        public NuevoEventoPage()
        {
            this.InitializeComponent();
            this.diasListBox.ItemsSource = inicializaDias();
            this.contactosListBox.ItemsSource = ContactosViewModel.ReadContactos((App.Current as App).ConnectionString);
            this.diasListBox.IsEnabled = false;
        }

        private List<string> inicializaDias()
        {
            List<string> dias = new List<string>();
            dias.Add("Domingo");
            dias.Add("Lunes");
            dias.Add("Martes");
            dias.Add("Miércoles");
            dias.Add("Jueves");
            dias.Add("Viernes");
            dias.Add("Sábado");
            return dias;
        }

        private void VolverBoton_Click(object sender, RoutedEventArgs e)
        {
            //Llama a la vista ContactoPage
            this.Frame.Navigate(typeof(EventosPage));
        }

        private async void AyudaBoton_Click(object sender, RoutedEventArgs e)
        {
            var ayuda = new MessageDialog("Utilice el simbolo de guardado para añadir un evento, \n" +
                "la flecha hacia la izquirda le permite volver a la pantalla principal de Eventos");
            ayuda.Title = "Información";
            await ayuda.ShowAsync();
        }

        private async void CrearBoton_Click(object sender, RoutedEventArgs e)//async para poder mostrar el message dialog
        {
            if (Validar())//Si estan llenos los campos, se crea el diario
            {
                string dias = "";
                List<int?> contactos = new List<int?>();
                if (this.diasListBox.IsEnabled)
                {
                    foreach (string dia in this.diasListBox.SelectedItems)
                    {
                        if (dias.Equals(""))
                        {
                            dias += dia;
                        }
                        else
                        {
                            dias += ", " + dia;
                        }

                    }
                }                
                foreach (Contacto contacto in this.contactosListBox.SelectedItems)
                {
                    contactos.Add(contacto.ContactoID);
                }

                bool exito = EventosViewModel.CreateEvento((App.Current as App).ConnectionString, (DateTimeOffset)this.fechaCalendarDatePicker.Date,
                    (TimeSpan)this.inicioTimePicker.SelectedTime, (TimeSpan)this.finTimePicker.SelectedTime, this.tituloTextBox.Text, this.descripcionTextBox.Text,
                    this.ubicacionTextBox.Text, (bool)this.serieCheckBox.IsChecked, dias, contactos);
                if (exito)//Quita los datos que se escribieron para hacer mas entradas
                {
                    var errorBase = new MessageDialog("Evento creado con éxito");
                    errorBase.Title = "Información";
                    await errorBase.ShowAsync();
                    this.fechaCalendarDatePicker.Date = null;
                    this.inicioTimePicker.SelectedTime = null;
                    this.finTimePicker.SelectedTime = null;
                    this.tituloTextBox.Text = "";
                    this.ubicacionTextBox.Text = "";
                    this.descripcionTextBox.Text = "";
                    this.serieCheckBox.IsChecked = false;
                    this.diasListBox.ItemsSource = null;
                    this.diasListBox.ItemsSource = inicializaDias();
                    this.contactosListBox.ItemsSource = ContactosViewModel.ReadContactos((App.Current as App).ConnectionString);
                }
                else//Notifica que no se pudo crear la entrada por un error con la base
                {
                    var errorBase = new MessageDialog("Ha ocurrido un error con la base de datos, \nno se puede ingresar el evento");
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

        private bool Validar()//Valida si fecha y contendo estan llenos
        {
            if(this.fechaCalendarDatePicker.Date != null && this.inicioTimePicker.SelectedTime!=null &&
                this.finTimePicker.SelectedTime!=null && !String.IsNullOrEmpty(this.tituloTextBox.Text) &&
                !String.IsNullOrEmpty(this.descripcionTextBox.Text) && !String.IsNullOrEmpty(this.ubicacionTextBox.Text))
            {
                return true;//Si se llenaron los campos
            }
            else
            {
                return false;//No se llenaron los campos
            }
        }

        private void siEsSerie(object sender, RoutedEventArgs e)
        {
            this.diasListBox.IsEnabled = true;
        }

        private void noEsSerie(object sender, RoutedEventArgs e)
        {
            this.diasListBox.IsEnabled = false;
        }
    }
}
