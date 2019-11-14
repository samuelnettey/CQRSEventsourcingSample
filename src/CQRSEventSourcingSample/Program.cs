using System;
using System.Collections;
using System.Collections.Generic;

namespace CQRSEventSourcingSample
{

    public class Person
    {
        private int age;
        private readonly EventBroker eventBroker;

        

        public Person(EventBroker eventBroker)
        {
            this.eventBroker = eventBroker;
            eventBroker.Commands += BrokerOnCommands;
            eventBroker.Queries += BrokerOnQueries;
        }

        private void BrokerOnQueries(object sender, Query query)
        {
            var aq = query as AgeQuery;
            if (aq != null && aq.Target == this)
            {
                aq.Result = age;
            }
        }



        private void BrokerOnCommands(object sender, Command command)
        {
            var cac = command as ChangeAgeCommand;
            if(cac != null && cac.Target == this)
            {
                eventBroker.AllEvents.Add(new AgeChangeEvent(this, age, cac.Age));
                age = cac.Age;
            }
        }
    }



    public class EventBroker
    {
        public IList<Event> AllEvents = new List<Event>();
        public event EventHandler<Command> Commands;
        public event EventHandler<Query> Queries;

        public void Command(Command c)
        {
            Commands?.Invoke(this, c);
        }

        public T Query<T>(Query query)
        {
            Queries?.Invoke(this, query);
            return (T)query.Result;
        }
    }

    public class Query
    {
        public object Result;

    }

    public class Event
    {
    }

    public class AgeChangeEvent : Event
    {
        public AgeChangeEvent(Person target, int oldValue, int newValue)
        {
            Target = target;
            OldValue = oldValue;
            NewValue = newValue;
        }

        public override string ToString()
        {
            return $"Age changed from {OldValue} to {NewValue}";
        }

        public Person Target { get; }
        public int OldValue { get; }
        public int NewValue { get; }
    }
    public class Command : EventArgs
    {

    }


    public class AgeQuery: Query
    {

        public Person Target;
    }

    public class ChangeAgeCommand :Command
    {
        public readonly int Age;

        public ChangeAgeCommand(Person target, int age)
        {
            Target = target;
           Age = age;
        }

        public Person Target { get; }
    }


    class Program
    {
        static void Main(string[] args)
        {
            var eventBroker = new EventBroker();
            var person = new Person(eventBroker);

            eventBroker.Command(new ChangeAgeCommand(person, 33));
            foreach (var e in eventBroker.AllEvents)
            {
                Console.WriteLine(e);
            }



            int age = eventBroker.Query<int>(new AgeQuery() {Target=person });

            Console.WriteLine(age);
            Console.ReadKey();
        }
    }
}

   