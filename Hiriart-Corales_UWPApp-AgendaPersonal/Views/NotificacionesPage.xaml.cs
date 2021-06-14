using System;
using Hiriart_Corales_UWPApp_AgendaPersonal.Core.Models;
using Hiriart_Corales_UWPApp_AgendaPersonal.ViewModels;
using Windows.UI.Notifications;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
using static Hiriart_Corales_UWPApp_AgendaPersonal.ViewModels.NotificacionesViewModel;

namespace Hiriart_Corales_UWPApp_AgendaPersonal.Views
{
    public sealed partial class NotificacionesPage : Page
    {
        public NotificacionesViewModel ViewModel { get; } = new NotificacionesViewModel();

        public NotificacionesPage()
        {
            InitializeComponent();
            this.NoticiacionList.ItemsSource = ReadNotificaciones((App.Current as App).ConnectionString);
        }

        private void NuevoBoton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            //Llama a la vista de nuevaNotificacionPage
            this.Frame.Navigate(typeof(NuevaNotificacionPage));
            Actualizar();
        }

        private void EditarBoton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            //Llama a la vista de editarNotificacionPage
            if (NoticiacionList.SelectedItem != null)//Solo entrar si hay algo seleccionado para evitar errores
            {
                Notificacion seleccionado = (Notificacion)NoticiacionList.SelectedItem;//Obtener fecha de la entrada a borrar
                this.Frame.Navigate(typeof(EditarNotificacionPage), seleccionado);//Llamar a la vista pasando el item seleccionado
                Actualizar();
            }
        }

        private async void EliminarBoton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            //Abre ventana de confirmación para preguntar si de quiere borrar la entrada
            if (NoticiacionList.SelectedItem != null)//Solo entrar si hay algo seleccionado para evitar errores
            {
                Notificacion seleccionado = (Notificacion)NoticiacionList.SelectedItem;//Obtener fecha de la entrada a borrar
                ContentDialog confirmarBorrado = new ContentDialog//Crea la ventana con boton de confirmacion y cancelacion
                {
                    Title = "Borrar notificacion",
                    Content = "¿Está seguro de que quiere borrar la notificacion " + seleccionado.Titulo + "?",
                    PrimaryButtonText = "Confirmar",
                    CloseButtonText = "Cancelar"
                };
                ContentDialogResult respuesta = await confirmarBorrado.ShowAsync();
                //Si se da click a Confirmar se borra, sino no
                if (respuesta == ContentDialogResult.Primary)//El boton primario confirma el borrado
                {
                    DeleteNotificacion((App.Current as App).ConnectionString, seleccionado.NotificacionID);
                    Actualizar();

                    //Borrar toast
                    var notificador = ToastNotificationManager.CreateToastNotifier();//Necesaria para llamar metodo que devuelve las toast pendientes
                    var toastPendientes = notificador.GetScheduledToastNotifications();

                    foreach(var toast in toastPendientes){//Si es el id de la que se esta borrando, se borra
                        if (toast.Id.Equals("DHLC"+seleccionado.NotificacionID))
                        {
                            notificador.RemoveFromSchedule(toast);
                        }
                    }
                }
                else//Si no fue el primario, se le dio click al de Cancelar, o se salio de la ventana, entonces no se debería borrar
                {
                    //No se borra, se cancelo la accion
                }
            }
        }

        private void Actualizar()
        {
            NoticiacionList.ItemsSource = ReadNotificaciones((App.Current as App).ConnectionString);
        }

        private async void AyudaBoton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            //Abre ventana que explica como usar la barra de comandos
            var ayuda = new MessageDialog("De clic en el boton de + para añadir una entrada nueva, el ícono \n" +
                "del lapiz le permite editar la entrada que tenga seleccionada, el ícono \n" +
                "del basurero borra la entrada seleccionada, y el boton Actualizar recarga la página");
            ayuda.Title = "Información";
            await ayuda.ShowAsync();
        }

        private void ActualizarBoton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            Actualizar();
        }
    }
}
