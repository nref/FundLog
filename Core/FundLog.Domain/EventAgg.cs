using System.Reactive.Subjects;
using FundLog.Model.Events;

namespace FundLog.Domain;

public interface IEventAgg
{
  public IObservable<Event> Published { get; }
  void Publish(Event e);
}

public class EventAgg : IEventAgg
{
  public IObservable<Event> Published => _published;
  private readonly ISubject<Event> _published = new Subject<Event>();

  public void Publish(Event e) => _published.OnNext(e);
}
