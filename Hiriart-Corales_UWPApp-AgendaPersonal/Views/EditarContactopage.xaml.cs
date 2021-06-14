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
    public sealed partial class EditarContactoPage : Page
    {
        Contacto seleccionadoContactoPage;//Variable global de lo que se selecciono en la view principal
        protected override void OnNavigatedTo(NavigationEventArgs e)//Metodo a usar en caso de pasar parametros durante la navegacion
        {
            if (!e.Parameter.Equals(null))//No continuar si es null, habrian errores
            {
                this.seleccionadoContactoPage = (Contacto)e.Parameter;

                //Llenar los campos con los datos ya conocidos de la entrada seleccionada
                this.nombreTextBox.Text = this.seleccionadoContactoPage.Nombre;
                this.apellidoTextBox.Text = this.seleccionadoContactoPage.Apellido;
                this.telefonoTextBox.Text = this.seleccionadoContactoPage.Telefono;
                this.emailTextBox.Text = this.seleccionadoContactoPage.Email==null?"":this.seleccionadoContactoPage.Email;//Porque puede ser null
                this.organizacionTextBox.Text = this.seleccionadoContactoPage.Organizacion == null ? "" : this.seleccionadoContactoPage.Organizacion;
                this.fechaCalendarDatePicker.Date = this.seleccionadoContactoPage.FechaNacimiento.Date;
                this.infoTextBox.Text = this.seleccionadoContactoPage.InformacionAdicional == null ? "" : this.seleccionadoContactoPage.InformacionAdicional;
            }
            else//Volver a la pantalla principal si es null, quiere decir que no se selecciono nada
            {
                this.Frame.Navigate(typeof(DiarioPage));
            }
        }

        public EditarContactoPage()
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
            var ayuda = new MessageDialog("Utilice el simbolo de guardado para guardar los cambios a un contacto, \n" +
                "la flecha hacia la izquirda le permite volver a la pantalla principal de Contacto");
            ayuda.Title = "Información";
            await ayuda.ShowAsync();
        }

        private async void EditarBoton_Click(object sender, RoutedEventArgs e)//async para poder mostrar el message dialog
        {
            if (Validar())//Si estan llenos los campos, se crea el diario
            {            
                bool exito = ContactosViewModel.UpdateContacto((App.Current as App).ConnectionString, this.seleccionadoContactoPage.ContactoID,
                    this.nombreTextBox.Text, this.apellidoTextBox.Text, this.telefonoTextBox.Text, this.emailTextBox.Text, this.organizacionTextBox.Text,
                    (DateTimeOffset)this.fechaCalendarDatePicker.Date, this.infoTextBox.Text);
                if (exito)//Quita los datos que se escribieron para hacer mas entradas
                {
                    var errorBase = new MessageDialog("Se ha editado el contacto, puede seguir editandolo \n" +
                        "o volver a la pantalla principal de Contactos");
                    errorBase.Title = "Contacto editado correctamente";
                    await errorBase.ShowAsync();
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
