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
    public sealed partial class NuevoContactoPage : Page
    {
        public NuevoContactoPage()
        {
            this.InitializeComponent();                      
        }

        private void VolverBoton_Click(object sender, RoutedEventArgs e)
        {
            //Llama a la vista ContactoPage
            this.Frame.Navigate(typeof(ContactosPage));
        }

        private async void AyudaBoton_Click(object sender, RoutedEventArgs e)
        {
            var ayuda = new MessageDialog("Utilice el simbolo de guardado para añadir un contacto, \n" +
                "la flecha hacia la izquirda le permite volver a la pantalla principal de Contacto");
            ayuda.Title = "Información";
            await ayuda.ShowAsync();
        }

        private async void CrearBoton_Click(object sender, RoutedEventArgs e)//async para poder mostrar el message dialog
        {
            if (Validar())//Si estan llenos los campos, se crea el diario
            {            
                bool exito = ContactosViewModel.CreateContacto((App.Current as App).ConnectionString, this.nombreTextBox.Text, this.apellidoTextBox.Text,
                    this.telefonoTextBox.Text, this.emailTextBox.Text, this.organizacionTextBox.Text,
                    (DateTimeOffset)this.fechaCalendarDatePicker.Date, this.infoTextBox.Text);
                if (exito)//Quita los datos que se escribieron para hacer mas entradas
                {
                    var errorBase = new MessageDialog("Contacto guardado con éxito");
                    errorBase.Title = "Información";
                    await errorBase.ShowAsync();
                    this.nombreTextBox.Text = "";
                    this.apellidoTextBox.Text = "";
                    this.telefonoTextBox.Text = "";
                    this.emailTextBox.Text = "";
                    this.organizacionTextBox.Text = "";
                    this.fechaCalendarDatePicker.Date = null;
                    this.infoTextBox.Text = "";
                }
                else//Notifica que no se pudo crear la entrada por un error con la base
                {
                    var errorBase = new MessageDialog("Ha ocurrido un error con la base de datos, \nno se puede ingresar el contacto");
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
            if(!String.IsNullOrEmpty(this.nombreTextBox.Text) && !String.IsNullOrEmpty(this.apellidoTextBox.Text) &&
                !String.IsNullOrEmpty(this.telefonoTextBox.Text) && !String.IsNullOrEmpty(this.emailTextBox.Text) &&
                !String.IsNullOrEmpty(this.organizacionTextBox.Text) && this.fechaCalendarDatePicker.Date!=null &&
                !String.IsNullOrEmpty(this.infoTextBox.Text))
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
