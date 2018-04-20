using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Canon_VB_M42
{
    public static class extention
    {
        public static IObservable<List<T>> ObserveWithBuffer<T>(this IObservable<T> source, IScheduler scheduler)
        {
            return Observable.Create<List<T>>(observer =>
            {
                Notification<List<T>> outsideNotification = null;
                var gate = new object();
                bool active = false;
                var cancelable = new MultipleAssignmentDisposable();
                var disposable = source.Materialize().Subscribe(thisNotification =>
                {
                    bool wasNotAlreadyActive;
                    lock (gate)
                    {
                        wasNotAlreadyActive = !active;
                        active = true;
                        if (outsideNotification == null) outsideNotification = Notification.CreateOnNext(new List<T>());
                        outsideNotification.Value.Add(thisNotification.Value);
                    }

                    if (wasNotAlreadyActive)
                    {
                        cancelable.Disposable = scheduler.Schedule(self =>
                        {
                            Notification<List<T>> localNotification = null;
                            lock (gate)
                            {
                                localNotification = outsideNotification;
                                outsideNotification = null;
                            }
                            localNotification.Accept(observer);
                            bool hasPendingNotification = false;
                            lock (gate)
                            {
                                hasPendingNotification = active = (outsideNotification != null);
                            }
                            if (hasPendingNotification)
                            {
                                self();
                            }
                        });
                    }
                });
                return new CompositeDisposable(disposable, cancelable);
            });
        }

    }
}
