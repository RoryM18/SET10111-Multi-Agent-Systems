using ActressMas;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Serialization;

//Using the ActressMAS library create an application that will simulate news dissemination.
//Create an Agent called Person, then create a population of Person agents
//named p1, p2....p100 etc  When initialising a Person, create a random list of 5 "friend" agents (ie p1,p4,p45,p99,p72).
//Create an Act method that will pass on any message received to each of the friends.
//Create a Source agent that will create a message string that it sends to a random group of Person agents,
//each recipient should now forward the message to each of their friends.

namespace Practical1
{
    public class Program
    {
        public static List<string> randomFriends = new List<string>();

        private static void Main(string[] args)
        {
            var env = new EnvironmentMas(parallel: false, randomOrder: false);

            int friendCount = 5;

            var list = new List<Person>();

            Random random = new Random();

            for (int i = 1; i <= 100; i++)
            {
                Person population = new Person();
                env.Add(population, $"p{i}");

                list.Add(population);
            }

            // Generate randomFriends list here
            while (randomFriends.Count < friendCount)
            {
                int randomIndex = random.Next(0, list.Count);
                var randomFriend = list[randomIndex].Name;

                if (!randomFriends.Contains(randomFriend))
                {
                    randomFriends.Add(randomFriend);
                }
            }

            foreach (var friend in randomFriends)
            {
                Console.WriteLine($"{friend} is in a list of 5");
            }

            var source = new Source();
            env.Add(source, "Source");

            env.Start();
        }
    }

    public class Person : Agent
    {
        private bool hasReceivedNews = false;
        public override void Setup()
        {
            Console.WriteLine($"{Name} is starting");
        }

        public override void ActDefault()
        {
        }

        public override void Act(Message message)
        {
            if (!hasReceivedNews)
            {
                Console.WriteLine($"{Name} has received {message.Content} from {message.Sender}");
                hasReceivedNews = true;

                // Forward the news to one random friend
                Random random = new Random();
                int randomIndex = random.Next(0, Program.randomFriends.Count);
                var randomFriend = Program.randomFriends[randomIndex];

                Send(randomFriend, message.Content);
            }
        }
    }

    public class Source : Agent
    {
        public override void Setup()
        {
            Console.WriteLine($"{Name} is starting");
            Console.WriteLine($"{Name} is sending news to a recipient");
        }

        public override void ActDefault()
        {
            // Send news to someone in the group
            Random random = new Random();
            int randomIndex = random.Next(0, Program.randomFriends.Count);
            var randomRecipient = Program.randomFriends[randomIndex];

            Send(randomRecipient, "News");

            Stop();
        }

        public override void Act(Message message)
        {
        }
    }
}

