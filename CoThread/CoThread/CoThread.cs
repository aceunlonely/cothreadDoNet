using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace CoThread
{
    public static class CoThread
    {
        static CoThread() {
            ThreadPool.SetMaxThreads(1000, 100);
        }
        private static int count = 0;

        private static ManualResetEvent resetEvent = new ManualResetEvent(false);

        public static ConcurrentDictionary<object, object> Map = new ConcurrentDictionary<object, object>();

        public static ConcurrentQueue<object> Queue = new ConcurrentQueue<object>();

        public static ConcurrentQueue<object> ExceptionQueue = new ConcurrentQueue<object>();

        private static Action<object> GetSwappedAction<T>() {
            return p => {
                CoParam cp = p as CoParam;
                try
                {
                    //todo
                    if (cp.ParamType == CoParamType.NONE)
                    {
                        cp.NoneAction();
                    }
                    else {
                        cp.OneParamAction(cp.OneParamAction);
                    }
                    //todo
                }
                catch (Exception ex)
                {
                    CoException cex = new CoException("error", ex);
                    //todo
                    CoThread.ExceptionQueue.Enqueue(cex);
                }
                finally {
                    if (Interlocked.Decrement(ref count) == 0)
                        resetEvent.Set();
                }
            };
        }

        public static void Add(Action action) {
            if (action == null) return;
            Interlocked.Increment(ref count);
            ThreadPool.QueueUserWorkItem(new WaitCallback(CoThread.GetSwappedAction<object>()), new CoParam()
            {
                OneParamAction = o=> { action(); },
                ParamType = CoParamType.NONE
            });
        }

        public static void Add(Action<dynamic> action,dynamic param ) {
            if (action == null) return;
            Interlocked.Increment(ref count);
            ThreadPool.QueueUserWorkItem(new WaitCallback(CoThread.GetSwappedAction<dynamic>()), new CoParam()
            {
                OneParamAction = (p => { action(p); }),
                ParamType = CoParamType.DYNAMIC,
                objectParam = param
            });
        }

        public static void Add<T>(Action<T> action, T param)
        {
            if (action == null) return;
            Interlocked.Increment(ref count);
            ThreadPool.QueueUserWorkItem(new WaitCallback(CoThread.GetSwappedAction<T>()), new CoParam() {
                OneParamAction = (p => { action((T)p); }),
                ParamType = CoParamType.TYPE,
                objectParam = param
            });
        }

        public static void BatchAdd<T>(Action<T> action, IEnumerable<T> paramList) {
            if (paramList == null) return;
            foreach (T p in paramList)
                Add(action, p);
        }

        public static void BatchAdd(Action<dynamic> action, IEnumerable<dynamic> paramList) {
            if (paramList == null) return;
            foreach (dynamic p in paramList)
                Add(action, p);
        }

        public static void BatchAdd(Action action, int count) {
            for (int i = 0; i < count; i++)
                Add(action);
        }

        /// <summary>
        /// 等待线程执行完
        /// </summary>
        public static void Wait() {
            try
            {
                resetEvent.WaitOne();
            }
            catch (Exception ex)
            {
               //todo
            }
            finally
            {
                resetEvent.Close();
                resetEvent = new ManualResetEvent(false);
            }
        }

        public static void ClearAll() {

        }
    }

    public class CoException : Exception {
        public CoException():base() { }

        public CoException(string message, Exception innerException) : base(message, innerException) { }
    }

    class CoParam {
        internal CoParamType ParamType { get; set; }
        internal object objectParam { get; set; }
        internal Action<object> OneParamAction { get; set; }
        internal Action NoneAction { get; set; }

    }

    enum CoParamType {
        NONE = 1,
        DYNAMIC =2,
        TYPE =3
    }
}
