using Core.Models.Settings.DataTypes;
using ReactiveUI;
using ViewModels.Macros.DataTypes;

namespace ViewModels.Macros
{
    public class ViewItemContext<T> : IMacrosContext
    {
        private readonly MacrosContent<T> _content;

        public ViewItemContext(MacrosContent<T> content)
        {
            _content = content;
        }

        public MacrosViewItemData CreateContext()
        {
            Action action = () =>
            {
                if (_content.Commands == null)
                {
                    return;
                }

                MessageBus.Current.SendMessage(_content.Commands);
            };

            return new MacrosViewItemData(_content.MacrosName ?? string.Empty, action);
        }
    }
}
