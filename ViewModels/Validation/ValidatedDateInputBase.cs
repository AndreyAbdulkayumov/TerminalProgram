using ReactiveUI;
using System.Collections;
using System.ComponentModel;

namespace ViewModels.Validation
{
    public class ValidatedDateInputBase : ReactiveObject, INotifyDataErrorInfo
    {
        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

        private bool _hasErrors;

        public bool HasErrors
        {
            set => this.RaiseAndSetIfChanged(ref _hasErrors, value);
            get => _hasErrors;
        }

        protected readonly Dictionary<string, List<string>> _errors = new();

        protected void OnErrorsChanged(string propertyName)
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }

        public IEnumerable GetErrors(string? propertyName)
        {
            HasErrors = _errors.Any();

            if (!string.IsNullOrEmpty(propertyName) && _errors.TryGetValue(propertyName, out var errors))
            {
                return errors;
            }

            return Enumerable.Empty<string>();
        }
    }
}
