using UniRx;
using System;


namespace Bercetech.Games.Fleepas
{

    /// <summary>
    /// Simple signal for receiving/sending messages to disconnected components in the application
    /// Serves purpose for decoupling logic without getting into Zenject memory/CPU issues
    /// </summary>
    public class Signal : IObservable<Unit>
    {
        #region [Fields]

        private readonly Subject<Unit> _signalStream = new Subject<Unit>();

        #endregion

        public void Fire() { _signalStream.OnNext(Unit.Default); }

        public IDisposable Subscribe(IObserver<Unit> observer) { return _signalStream.Subscribe(observer.OnNext); }
    }

    /// <summary>
    /// Typed signal that passes typed data via stream same way as signal does
    /// </summary>
    public class Signal<T> : IObservable<T>
    {
        #region [Fields]

        private readonly Subject<T> _signalStream = new Subject<T>();

        #endregion

        public void Fire(T data) { _signalStream.OnNext(data); }

        public IDisposable Subscribe(IObserver<T> observer) { return _signalStream.Subscribe(observer.OnNext); }

    }
}

