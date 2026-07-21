using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Library.UI.Services
{
    public class ToastService : IToastService
    {
        private readonly List<ToastMessage> _messages = new();
        private const int AutoDismissMilliseconds = 4000;

        public IReadOnlyList<ToastMessage> Messages => _messages;
        public event Action? OnChange;

        public void ShowSuccess(string message) => Add(message, ToastType.Success);
        public void ShowError(string message) => Add(message, ToastType.Error);
        public void ShowInfo(string message) => Add(message, ToastType.Info);
        public void ShowWarning(string message) => Add(message, ToastType.Warning);

        private void Add(string message, ToastType type)
        {
            var toast = new ToastMessage { Text = message, Type = type };
            _messages.Add(toast);
            OnChange?.Invoke();

            // Belirli bir süre sonra otomatik kaldır (fire-and-forget)
            _ = AutoRemoveAsync(toast.Id);
        }

        private async Task AutoRemoveAsync(Guid id)
        {
            await Task.Delay(AutoDismissMilliseconds);
            Remove(id);
        }

        public void Remove(Guid id)
        {
            var item = _messages.FirstOrDefault(m => m.Id == id);
            if (item != null)
            {
                _messages.Remove(item);
                OnChange?.Invoke();
            }
        }
    }
}