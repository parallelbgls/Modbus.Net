using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Technosoftware.DaAeHdaClient.Utilities;

namespace Technosoftware.DaAeHdaClient.Com.Utilities
{
    /// <summary>
    /// The result of DCOM watchdog
    /// </summary>
    public enum DCOMWatchdogResult
    {
        /// <summary>
        /// Watchdog has not been set/there is no result
        /// </summary>
        None = 0,
        /// <summary>
        /// The Set/Reset cycle was manually completed i.e. the DCOM call did not timeout
        /// </summary>
        Completed,
        /// <summary>
        /// No Reset call occurred with the timeout period thus the current DCOM call was automatically cancelled
        /// </summary>
        TimedOut,
        /// <summary>
        /// The current DCOM call was manually cancelled
        /// </summary>
        ManuallyCancelled
    }

    /// <summary>
    /// Watchdog mechanism to allow for cancellation of DCOM calls. Note that this mechanism will only work for a STA thread apartment - the thread on which
    /// the watchdog is Set and DCOM calls are made have to be the same thread and the thread apartment model has to be set to STA.
    /// </summary>
    public static class DCOMCallWatchdog
    {
        #region Fields
        private const int DEFAULT_TIMEOUT_SECONDS = 10;

        private static object watchdogLock_ = new object();
        private static uint watchDogThreadID_;
        private static bool isCancelled_;
        private static TimeSpan timeout_ = TimeSpan.Zero; //disabled by default
        private static Task watchdogTask_;
        private static DCOMWatchdogResult lastWatchdogResult_ = DCOMWatchdogResult.None;
        private static DateTime setStart_;
        
        #endregion

        #region Properties

        /// <summary>
        /// The result of the last watchdog set/reset operation
        /// </summary>
        public static DCOMWatchdogResult LastWatchdogResult
        {
            get { return lastWatchdogResult_; }
        }

        /// <summary>
        /// The current native thread ID on which the watchdog has been enabled
        /// </summary>
        public static uint WatchDogThreadID
        {
            get => watchDogThreadID_;
        }

        /// <summary>
        /// Indicates if the watchdog mechanism is active or not
        /// </summary>
        public static bool IsEnabled
        {
            get => timeout_ != TimeSpan.Zero;
        }

        /// <summary>
        /// Indicates if the watchdog has been set and is busy waiting for a call completion Reset to be called or a timeout to occur.
        /// </summary>
        public static bool IsSet
        {
            get => WatchDogThreadID != 0;
        }

        /// <summary>
        /// Indicates if the watchdog was cancelled due to a timeout
        /// </summary>
        public static bool IsCancelled
        {
            get => isCancelled_;
        }

        /// <summary>
        /// The watchdog timeout timespan
        /// </summary>
        public static TimeSpan Timeout
        {
            get => timeout_;            
            set
            {
                Enable(value);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Enables the Watchdog mechanism. This can be called from any thread and does not have to be the DCOM call originator thread.
        /// Uses the default call timeout.
        /// </summary>
        public static void Enable()
        {
            Enable(TimeSpan.FromSeconds(DEFAULT_TIMEOUT_SECONDS));
        }


        /// <summary>
        /// Enables the Watchdog mechanism. This can be called from any thread and does not have to be the DCOM call originator thread.
        /// </summary>
        /// <param name="timeout">The maximum time to wait for a DCOM call to succeed before it is cancelled. Note that DCOM will typically timeout
        /// between 1-2 minutes, depending on the OS</param>
        public static void Enable(TimeSpan timeout)
        {          
            if (timeout == TimeSpan.Zero)
            {
                timeout = TimeSpan.FromSeconds(DEFAULT_TIMEOUT_SECONDS);
            }

            lock (watchdogLock_)
            {
                timeout_ = timeout;
            }

            watchdogTask_ = Task.Run(() => WatchdogTask());
        }

        /// <summary>
        /// Disables the watchdog mechanism and stops any call cancellations.
        /// </summary>
        /// <returns>True if enabled and now disabled, otherwise false</returns>

        public static bool Disable()
        {
            lock (watchdogLock_)
            {
                if (IsEnabled)
                {
                    timeout_ = TimeSpan.Zero;

                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        
        /// <summary>
        /// Sets the watchdog timer active on the current thread. If Reset is not called within the timeout period, any current thread DCOM call will be cancelled. The
        /// calling thread must be the originator of the DCOM call and must be an STA thread.
        /// </summary>
        /// <returns>True if the watchdog set succeeds or was already set for the current thread, else false if the watchdog is not enabled.</returns>
        public static bool Set()
        {
            if (IsEnabled)
            {
                var apartmentState = Thread.CurrentThread.GetApartmentState();

                if (apartmentState != ApartmentState.STA)
                {
                    throw new InvalidOperationException("COM calls can only be cancelled on a COM STA apartment thread - use [STAThread] attibute or set the state of the thread on creation");
                }

                lock (watchdogLock_)
                {
                    var threadId = Interop.GetCurrentThreadId();

                    if (IsSet)
                    {
                        if (threadId != watchDogThreadID_)
                        {
                            throw new InvalidOperationException($"Attempt to set call cancellation on different thread [{threadId}] to where it was already enabled [{watchDogThreadID_}]");
                        }
                    }
                    else
                    {
                        isCancelled_ = false;
                        watchDogThreadID_ = 0;
                        lastWatchdogResult_ = DCOMWatchdogResult.None;

                        //enable DCOM call cancellation for duration of the watchdog
                        var hresult = Interop.CoEnableCallCancellation(IntPtr.Zero);

                        if (hresult == 0)
                        {
                            setStart_ = DateTime.UtcNow;
                            watchDogThreadID_ = threadId;
                           
                            Utils.Trace(Utils.TraceMasks.Information, $"COM call cancellation on thread [{watchDogThreadID_}] was set with timeout [{timeout_.TotalSeconds} seconds]");
                        }
                        else
                        {
                            throw new Exception($"Failed to set COM call cancellation (HResult = {hresult})");
                        }
                    }
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Refreshes the watchdog activity timer to now, effectively resetting the time to wait.
        /// </summary>
        /// <returns>True if the watchdog time was updated, else False if the watchdog timer is not Enabled or Set</returns>
        public static bool Update()
        {
            if (IsEnabled)
            {
                lock (watchdogLock_)
                {
                    if (IsSet)
                    {
                        setStart_ = DateTime.UtcNow;

                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Resets the watchdog timer for the current thread. This should be called after a DCOM call returns to indicate the call succeeded, and thus cancelling the
        /// watchdog timer.
        /// </summary>
        /// <returns>True if the watchdog timer was reset for the current thread, else False if the timer was not set for the thread of the watchdog is not enabled.</returns>
        public static bool Reset()
        {
            if (IsEnabled)
            {
                lock (watchdogLock_)
                {
                    if (IsSet)
                    {
                        var threadId = Interop.GetCurrentThreadId();

                        if (threadId == watchDogThreadID_)
                        {
                            if (!IsCancelled)
                            {
                                lastWatchdogResult_ = DCOMWatchdogResult.Completed;
                            }

                            watchDogThreadID_ = 0;
                            isCancelled_ = false;

                            //disable DCOM call cancellation 
                            var hresult = Interop.CoDisableCallCancellation(IntPtr.Zero);

                            Utils.Trace(Utils.TraceMasks.Information, $"COM call cancellation on thread [{watchDogThreadID_}] was reset [HRESULT = {hresult}]");
                        }
                        else
                        {
                            throw new Exception($"COM call cancellation cannot be reset from different thread [{threadId}] it was set on [{watchDogThreadID_}]");
                        }
                    }
                }

                return true;
            }
            else
            {
                return false;
            }
        }   

        /// <summary>
        /// Allows for manual cancellation of the current DCOM call
        /// </summary>
        /// <returns></returns>
        public static bool Cancel()
        {
            return Cancel(DCOMWatchdogResult.ManuallyCancelled);
        }

        /// <summary>
        /// Cancels the current DCOM call if there is one active
        /// </summary>
        /// <param name="reason"></param>
        /// <returns>The reason for the cancellation</returns>
        private static bool Cancel(DCOMWatchdogResult reason)
        {
            if (IsEnabled)
            {
                lock (watchdogLock_)
                {
                    if (!IsCancelled && IsSet)
                    {
                        isCancelled_ = true;

                        //cancel the current DCOM call immediately
                        var hresult = Interop.CoCancelCall(watchDogThreadID_, 0);

                        Utils.Trace(Utils.TraceMasks.Information, $"COM call on thread [{watchDogThreadID_}] was cancelled [HRESULT = {hresult}]");

                        lastWatchdogResult_ = reason;

                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// The Watchdog Task is a seperate thread that is activated when the Watchdog is enabled. It checks the time since the last Set was called and
        /// then cancels the current DCOM call automatically if Reset is not called within the timeout period.
        /// </summary>
        private static void WatchdogTask()
        {            
            while (IsEnabled)
            {
                try
                {
                    if (IsSet & !IsCancelled)
                    {
                        if (TimeElapsed(setStart_) >= timeout_)
                        {
                            Utils.Trace(Utils.TraceMasks.Information, $"Sync call watchdog for thread [{watchDogThreadID_}] timed out - cancelling current call...");

                            Cancel(DCOMWatchdogResult.TimedOut);
                        }                        
                    }
                }
                catch (Exception e)
                {
                    Utils.Trace(Utils.TraceMasks.Error, $"Error in Sync call watchdog thread : {e.ToString()}");
                }
                finally
                {
                    Thread.Sleep(1);
                }
            }
        }

        
        private static TimeSpan TimeElapsed(DateTime startTime)
        {
            var now = DateTime.UtcNow;

            startTime = startTime.ToUniversalTime();

            if (startTime > now)
            {
                return startTime - now;
            }
            else
            {
                return now - startTime;
            }
        }

        #endregion

    }       
}
