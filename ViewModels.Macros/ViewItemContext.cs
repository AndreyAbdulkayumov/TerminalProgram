using Core.Models.Settings.DataTypes;
using ReactiveUI;
using ViewModels.Macros.DataTypes;

namespace ViewModels.Macros;

public class ViewItemContext<T1, T2> : IMacrosContext
{
    private readonly MacrosContent<T1, T2> _content;

    public ViewItemContext(MacrosContent<T1, T2> content)
    {
        _content = content;
    }

    public MacrosViewItemData CreateContext()
    {
        var contentForSend = MacrosHelper.GetWithAdditionalData(_content);

        Action action = () =>
        {
            if (contentForSend.Commands == null)
            {
                return;
            }

            MessageBus.Current.SendMessage(contentForSend);
        };

        return new MacrosViewItemData(contentForSend.MacrosName ?? string.Empty, action);
    }
}
