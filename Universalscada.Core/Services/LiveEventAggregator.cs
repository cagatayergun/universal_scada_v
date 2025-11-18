// Universalscada.Core/Services/LiveEventAggregator.cs
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Universalscada.Models;

namespace Universalscada.Core.Services
{
    /// <summary>
    /// PLC Polling hizmeti ile UI/API köprüsü arasında canlı durum güncellemelerini
    /// ileten olay toplayıcı (Event Aggregator) hizmeti.
    /// </summary>
    public class LiveEventAggregator
    {
        // Thread-safe koleksiyon: Abone olan metotları tutar.
        private readonly ConcurrentBag<Func<FullMachineStatus, Task>> _subscribers = new ConcurrentBag<Func<FullMachineStatus, Task>>();

        /// <summary>
        /// FullMachineStatus güncellemelerine abone olmak için kullanılır.
        /// </summary>
        public IDisposable Subscribe(Func<FullMachineStatus, Task> handler)
        {
            _subscribers.Add(handler);
            return new DisposableSubscription(() => Unsubscribe(handler));
        }

        /// <summary>
        /// FullMachineStatus nesnesini tüm abonelere asenkron olarak yayınlar (Publish).
        /// </summary>
        public async Task Publish(FullMachineStatus status)
        {
            // Yayınlama işlemi sırasında bir hata olsa bile diğer abonelerin etkilenmemesi için
            // her aboneyi ayrı ayrı Task.Run ile çalıştırıyoruz.
            var publishingTasks = _subscribers.Select(handler =>
            {
                return Task.Run(async () =>
                {
                    try
                    {
                        await handler(status);
                    }
                    catch (Exception ex)
                    {
                        // TODO: Loglama yapılmalı
                        Console.WriteLine($"Event Aggregator hatası: {ex.Message}");
                    }
                });
            }).ToList();

            await Task.WhenAll(publishingTasks);
        }

        private void Unsubscribe(Func<FullMachineStatus, Task> handler)
        {
            // Thread-safe bir koleksiyondan öğe kaldırmak ConcurrentBag'de zordur, 
            // performanstan ödün vererek yeni bir koleksiyon oluşturup filtreleriz.
            // Daha yüksek performans için ConcurrentQueue/Dictionary kullanılabilir.
            // Basitçe: _subscribers = new ConcurrentBag<Func<FullMachineStatus, Task>>(_subscribers.Except(new[] { handler }));
        }

        // Abonelikten çıkmayı kolaylaştıran yardımcı sınıf
        private class DisposableSubscription : IDisposable
        {
            private readonly Action _unsubscribeAction;
            public DisposableSubscription(Action unsubscribeAction) => _unsubscribeAction = unsubscribeAction;
            public void Dispose() => _unsubscribeAction?.Invoke();
        }
    }
}