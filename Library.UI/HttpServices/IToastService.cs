using System;
using System.Collections.Generic;

namespace Library.UI.Services
{
    public enum ToastType
    {
        Success,
        Error,
        Info,
        Warning
    }

    public class ToastMessage
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Text { get; set; } = string.Empty;
        public ToastType Type { get; set; } = ToastType.Info;
    }

    /// <summary>
    /// Uygulama genelinde başarı / hata / bilgi bildirimlerini (toast) yönetmek için kullanılan servis.
    /// MainLayout içindeki ToastContainer bileşeni bu servisi dinler.
    /// </summary>
    public interface IToastService
    {
        event Action? OnChange;
        IReadOnlyList<ToastMessage> Messages { get; }

        void ShowSuccess(string message);
        void ShowError(string message);
        void ShowInfo(string message);
        void ShowWarning(string message);
        void Remove(Guid id);
    }
}