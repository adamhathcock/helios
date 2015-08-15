﻿using System;
using System.Threading;

namespace Helios.Util.TimedOps
{
    /// <summary>
    /// When triggered, ScheduledValue will update an underlying
    /// value to a new one after a period of time has elapsed
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ScheduledValue<T> : IDisposable
    {
        protected Timer SetTimer;

        public event EventHandler ScheduleFinished;

        public ScheduledValue(T initialValue)
        {
            Value = initialValue;
        }

        public ScheduledValue(T initialValue, T futureValue, TimeSpan timeToSet) : this(initialValue)
        {
            Schedule(futureValue, timeToSet);
        }

        protected T FutureValue;

        public T Value { get; set; }

        public bool IsScheduled { get; protected set; }

        /// <summary>
        /// Indicates that the future value was successfully set
        /// </summary>
        public bool WasSet { get; set; }

        public bool WasDisposed { get; set; }

        public void Cancel()
        {
            if (SetTimer == null || !IsScheduled) return;
            SetTimer.Dispose();
            IsScheduled = false;
        }

        public void Schedule(T futureValue, TimeSpan timeToSet)
        {
            FutureValue = futureValue;

            if (SetTimer == null)
            {
                SetTimer = new Timer(new TimerCallback(SetTimerOnElapsed), null, 0, (int)Math.Max(timeToSet.TotalMilliseconds, 1));
            }
            IsScheduled = true;
        }

        private void SetTimerOnElapsed(object sender)
        {
            Value = FutureValue;
            WasSet = true;
            IsScheduled = false;
            Cancel();
            InvokeScheduleFinished();
        }

        #region Events

        private void InvokeScheduleFinished()
        {
            var h = ScheduleFinished;
            if (h == null) return;
            h(this, new EventArgs());
        }

        #endregion

        #region Conversion

        public static implicit operator T(ScheduledValue<T> o)
        {
            return o.Value;
        }

        public static implicit operator ScheduledValue<T>(T o)
        {
            return new ScheduledValue<T>(o);
        }

        #endregion

        #region Object overloads

        public override bool Equals(object obj)
        {
            return Value.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        #endregion

        #region IDisposable Members

        public void Dispose(bool isDisposing)
        {
            if (SetTimer != null)
            {
                Cancel();
                SetTimer.Dispose();
            }

            WasDisposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
