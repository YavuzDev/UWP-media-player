using System;
using System.Threading.Tasks;
using Windows.UI.Popups;

namespace MediaPlayer.Util
{
    public class Alert
    {
        public static async Task SendAlert(string message)
        {
            var messageDialog = new MessageDialog(message);
            messageDialog.Commands.Add(new UICommand("Close", CloseAlert));

            await messageDialog.ShowAsync();
        }

        private static void CloseAlert(IUICommand command)
        {
            
        }
    }
}