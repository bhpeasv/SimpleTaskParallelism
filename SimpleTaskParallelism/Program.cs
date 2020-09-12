using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleTaskParallelism
{
    class Program
    {
        const int CANCELATION_TIME = 6000;

        static void Main(string[] args)
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            CancellationToken token = cts.Token;

            Console.WriteLine($"Canceling after {CANCELATION_TIME / 1000d} seconds\n");
            cts.CancelAfter(CANCELATION_TIME);

            //Task t1 = Task.Factory.StartNew(() => UnitOfWork(5000, token), token);
            //Task t2 = Task.Factory.StartNew(() => UnitOfWork(10000, token), token);

            try
            {
                Parallel.Invoke(
                    new ParallelOptions() { CancellationToken = token },
                    () => UnitOfWork(5000, token),
                    () => UnitOfWork(10000, token)
                );
                //Task.WaitAll(t1, t2);
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine(ex.GetType());
                if (ex is AggregateException ae)
                {
                    ae.Handle((ie) =>
                    {
                        Console.WriteLine($"    {ie.Message}");
                        return true;
                    });
                }
                else
                {
                    Console.WriteLine(ex.Message);
                }
            }

            Console.WriteLine();
            //Console.WriteLine($"Task{t1.Id}.Status = {t1.Status}");
            //Console.WriteLine($"Task{t2.Id}.Status = {t2.Status}");
        }

        static void UnitOfWork(int delay, CancellationToken token)
        {
            Console.WriteLine($"Task{Task.CurrentId} executing for {delay / 1000d} seconds");

            Stopwatch sw = Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < delay)
            {
                if (token.IsCancellationRequested)
                {
                    Console.WriteLine("Canceling task" + Task.CurrentId);
                    token.ThrowIfCancellationRequested();
                    //return;
                    //throw new OperationCanceledException();
                }
            }
            Console.WriteLine($"Task{Task.CurrentId} Done!");
        }
    }
}
