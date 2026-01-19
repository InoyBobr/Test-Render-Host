using System.Timers;

public class CounterService
{
    private int _value;
    public int Value => _value;

    public CounterService()
    {
        var timer = new Timer(60_000);
        timer.Elapsed += (s, e) => Reset();
        timer.AutoReset = true;
        timer.Start();
    }

    public void Increment() => _value++;
    public void Reset() => _value = 0;
}
