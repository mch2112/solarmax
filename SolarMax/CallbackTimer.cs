using System;
using System.Runtime.InteropServices;

namespace SolarMax;

public sealed partial class CallbackTimer : IDisposable
{
    IntPtr timerHandle; // Handle to the timer.

    #region Win32 TimerQueueTimer Functions

    [DllImport("kernel32.dll")]
    static extern bool CreateTimerQueueTimer(
        out IntPtr phNewTimer,          // phNewTimer - Pointer to a handle; this is an out value
        IntPtr TimerQueue,              // TimerQueue - Timer queue handle. For the default timer queue, NULL
        TimerDelegate Callback,         // Callback - Pointer to the callback function
        IntPtr Parameter,               // Parameter - Value passed to the callback function
        uint DueTime,                   // DueTime - Time (milliseconds), before the timer is set to the signaled state for the first time 
        uint Period,                    // Period - Timer period (milliseconds). If zero, timer is signaled only once
        uint Flags                      // Flags - One or more of the next values (table taken from MSDN):
                                        // WT_EXECUTEINTIMERTHREAD 	The callback function is invoked by the timer thread itself. This flag should be used only for short tasks or it could affect other timer operations.
                                        // WT_EXECUTEINIOTHREAD 	The callback function is queued to an I/O worker thread. This flag should be used if the function should be executed in a thread that waits in an alertable state.

                                        // The callback function is queued as an APC. Be sure to address reentrancy issues if the function performs an alertable wait operation.
                                        // WT_EXECUTEINPERSISTENTTHREAD 	The callback function is queued to a thread that never terminates. This flag should be used only for short tasks or it could affect other timer operations.

                                        // Note that currently no worker thread is persistent, although no worker thread will terminate if there are any pending I/O requests.
                                        // WT_EXECUTELONGFUNCTION 	Specifies that the callback function can perform a long wait. This flag helps the system to decide if it should create a new thread.
                                        // WT_EXECUTEONLYONCE 	The timer will be set to the signaled state only once.
        );

    [DllImport("kernel32.dll")]
    static extern bool DeleteTimerQueueTimer(
        IntPtr timerQueue,              // TimerQueue - A handle to the (default) timer queue
        IntPtr timer,                   // Timer - A handle to the timer
        IntPtr completionEvent          // CompletionEvent - A handle to an optional event to be signaled when the function is successful and all callback functions have completed. Can be NULL.
        );


    [DllImport("kernel32.dll")]
    static extern bool DeleteTimerQueue(IntPtr TimerQueue);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool CloseHandle(IntPtr hObject);

    #endregion

    public delegate void TimerDelegate(IntPtr lpParameter, bool timerOrWaitFired); 

    public CallbackTimer()
    {
    }

    public void Create(uint FirstCallbackMsec, uint PeriodMsec, TimerDelegate CallbackDelegate)
    {
        IntPtr param = IntPtr.Zero;

        bool success = CreateTimerQueueTimer(out timerHandle,
                                            IntPtr.Zero, // Default timer queue. IntPtr.Zero is just a constant value that represents a null pointer.
                                            CallbackDelegate,
                                            param,
                                            FirstCallbackMsec,
                                            PeriodMsec,
                                            (uint)Flag.WT_EXECUTEINIOTHREAD);

        if (!success)
            throw new Exception("Error creating timer");
    }

    public void Delete()
    {
        DeleteTimerQueueTimer( IntPtr.Zero, // TimerQueue - A handle to the (default) timer queue
                               timerHandle,  // Timer - A handle to the timer
                               IntPtr.Zero  // CompletionEvent - A handle to an optional event to be signaled when the function is successful and all callback functions have completed. Can be NULL.
                              );
    }

    private enum Flag
    {
        WT_EXECUTEDEFAULT = 0x00000000,
        WT_EXECUTEINIOTHREAD = 0x00000001,
        //WT_EXECUTEINWAITTHREAD       = 0x00000004,
        WT_EXECUTEONLYONCE = 0x00000008,
        WT_EXECUTELONGFUNCTION = 0x00000010,
        WT_EXECUTEINTIMERTHREAD = 0x00000020,
        WT_EXECUTEINPERSISTENTTHREAD = 0x00000080,
        //WT_TRANSFER_IMPERSONATION    = 0x00000100
    }

    public void Dispose()
    {
        this.Delete();
    }
}
