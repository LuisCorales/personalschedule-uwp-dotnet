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
    /// 
    public sealed partial class EditarEventoPage : Page
    {
        Evento seleccionadoContactoPage;//Variable global de lo que se selecciono en la view principal
        protected override void OnNavigatedTo(NavigationEventArgs e)//Metodo a usar en caso de pasar parametros durante la navegacion
        {
            if (!e.Parameter.Equals(null))//No continuar si es null, habrian errores
            {
                this.seleccionadoContactoPage = (Evento)e.Parameter;

                //Llenar los campos con los datos ya conocidos de la entrada seleccionada
                this.fechaCalendarDatePicker.Date = seleccionadoContactoPage.Fecha.Date;
                this.inicioTimePicker.SelectedTime = seleccionadoContactoPage.Inicio.TimeOfDay;
                this.finTimePicker.SelectedTime = seleccionadoContactoPage.Fin.TimeOfDay;
                this.tituloTextBox.Text = seleccionadoContactoPage.Titulo;
                this.ubicacionTextBox.Text = seleccionadoContactoPage.Ubicacion;
                this.descripcionTextBox.Text = seleccionadoContactoPage.Descripcion;
                this.serieCheckBox.IsChecked = seleccionadoContactoPage.EsSerie;
                List<string> dias = new List<string>();
                if ((bool)this.serieCheckBox.IsChecked)
                {
                    foreach (string dia in seleccionadoContactoPage.Dias.Split(","))
                    {
                        dias.Add(dia);
                    }
                }
                this.diasListBox.ItemsSource = dias;
                this.diasListBox.SelectAll();
                inicializaDias(dias);//Lenar los demas dias que no van a estar en el dato leido
                this.contactosListBox.ItemsSource = ContactosViewModel.ReadContactos((App.Current as App).ConnectionString);
                if (seleccionadoContactoPage.NotificacionID!=0)
                { 
                    this.notifiListBox.ItemsSource = EventosViewModel.NotificacionRelacionada((App.Current as App).ConnectionString, seleccionadoContactoPage.NotificacionID);
                }
                if (seleccionadoContactoPage.MemoID != 0)
                {
                    this.memoListBox.ItemsSource = EventosViewModel.MemoRelacionado((App.Current as App).ConnectionString, seleccionadoContactoPage.MemoID);
                }

                if (!seleccionadoContactoPage.EsSerie)
                    this.diasListBox.IsEnabled = false;
            }
            else//Volver a la pantalla principal si es null, quiere decir que no se selecciono nada
            {
                this.Frame.Navigate(typeof(EventosPage));
            }
        }

        private List<string> inicializaDias(List<string> dias)
        {
            List<string> todosDias = new List<string>();
            todosDias.Add("Domingo");
            todosDias.Add("Lunes");
            todosDias.Add("Martes");
            todosDias.Add("Miércoles");
            todosDias.Add("Jueves");
            todosDias.Add("Viernes");
            todosDias.Add("Sábado");

            foreach (string dia in todosDias)
            {
                if (!dias.Contains(dia))
                    dias.Add(dia);
            }

            return dias;
        }

        public EditarEventoPage()
        {
            this.InitializeComponent();                      
        }

        private void VolverBoton_Click(object sender, RoutedEventArgs e)
        {
            //Llama a la vista ContactoPage
            this.Frame.Navigate(typeof(EventosPage));
        }

        private async void AyudaBoton_Click(object sender, RoutedEventArgs e)
        {
            var ayuda = new MessageDialog("Utilice el simbolo de guardado para guardar los cambios, \n" +
                "la flecha hacia la izquirda le permite volver a la pantalla principal de Eventos");
            ayuda.Title = "Información";
            await ayuda.ShowAsync();
        }

        private async void EditarBoton_Click(object sender, RoutedEventArgs e)//async para poder mostrar el message dialog
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
                Memo memo = (Memo)this.memoListBox.SelectedItem;
                Notificacion notif = (Notificacion)this.notifiListBox.SelectedItem;
                if (this.memoListBox.SelectedItem==null)
                    memo = new Memo();
                if (this.notifiListBox.SelectedItem==null)
                    notif = new Notificacion();

                bool exito = EventosViewModel.UpdateEvento((App.Current as App).ConnectionString, seleccionadoContactoPage.EventoID, notif.NotificacionID, memo.MemoID, 
                (DateTimeOffset)this.fechaCalendarDatePicker.Date, (TimeSpan)this.inicioTimePicker.SelectedTime,
                    (TimeSpan)this.finTimePicker.SelectedTime, this.tituloTextBox.Text, this.descripcionTextBox.Text,
                    this.ubicacionTextBox.Text, (bool)this.serieCheckBox.IsChecked, dias, contactos);
                if (exito)//Quita los datos que se escribieron para hacer mas entradas
                {
                    var errorBase = new MessageDialog("Se ha editado el evento, puede seguir editandolo \n" +
                        "o volver a la pantalla principal de Eventos");
                    errorBase.Title = "Contacto editado correctamente";
                    await errorBase.ShowAsync();
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
            if (this.fechaCalendarDatePicker.Date != null && !this.inicioTimePicker.SelectedTime.Equals(null) &&
                !this.finTimePicker.SelectedTime.Equals(null) && !String.IsNullOrEmpty(this.tituloTextBox.Text) &&
                !String.IsNullOrEmpty(this.descripcionTextBox.Text) && !String.IsNullOrEmpty(this.ubicacionTextBox.Text))
            {
                return true;//Si se llenaron los campos
            }
            else
            {
                return false;//No se llenaron los campos
            }
        }

        private void siEsEvento(object sender, RoutedEventArgs e)
        {
            this.diasListBox.IsEnabled = true;
        }

        private void noEsEvento(object sender, RoutedEventArgs e)
        {
            this.diasListBox.IsEnabled = false;
        }
    }
}
