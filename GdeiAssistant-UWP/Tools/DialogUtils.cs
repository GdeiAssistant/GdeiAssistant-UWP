using System;
using Windows.UI.Popups;

namespace GdeiAssistant.Tools
{
    public class DialogUtils
    {

        public static async void ShowMessageDialog(string title,string content)
        {
            var messageDialog = new MessageDialog(content) { Title = title };
            messageDialog.Commands.Add(new UICommand("确定"));
            await messageDialog.ShowAsync();
        }

        public static async void ShowMessageDialog(string title,string content, UICommandInvokedHandler yesUICommandInvokedHandler
            , UICommandInvokedHandler noUICommandInvokedHandler)
        {
            var messageDialog = new MessageDialog(content) { Title = title };
            messageDialog.Commands.Add(new UICommand("确定", yesUICommandInvokedHandler));
            messageDialog.Commands.Add(new UICommand("取消",noUICommandInvokedHandler));
            await messageDialog.ShowAsync();
        }

    }
}
