﻿/**************************************************************************
 *                                                                        *
 *  Website:     https://github.com/florinleon/ActressMas                 *
 *  Description: Computation of the Shapley value for agents using        *
 *               the ActressMas framework                                 *
 *  Copyright:   (c) 2018, Florin Leon                                    *
 *                                                                        *
 *  This program is free software; you can redistribute it and/or modify  *
 *  it under the terms of the GNU General Public License as published by  *
 *  the Free Software Foundation. This program is distributed in the      *
 *  hope that it will be useful, but WITHOUT ANY WARRANTY; without even   *
 *  the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR   *
 *  PURPOSE. See the GNU General Public License for more details.         *
 *                                                                        *
 **************************************************************************/

using ActressMas;
using System;
using System.Collections.Generic;

namespace Shapley
{
    public class ManagerAgent : Agent
    {
        private string[] agentBidders;
        private List<string> bids;
        private Task currentTask;
        private int taskCount = 0;

        public ManagerAgent()
        {
            agentBidders = new string[Utils.NoBids];
        }

        public override void Setup()
        {
            GenerateNewTask();
        }

        private void GenerateNewTask()
        {
            currentTask = new Task();
            taskCount++;
            SelectAgents();
            Console.WriteLine("Selected: {0} {1} {2}", agentBidders[0], agentBidders[1], agentBidders[2]);

            for (int i = 0; i < Utils.NoBids; i++)
                Send(agentBidders[i], "task " + currentTask.ToString());

            bids = new List<string>();
        }

        private void SelectAgents()
        {
            int noAgents = this.Environment.Agents.Count;

            agentBidders[0] = "";
            while ((agentBidders[0] == "") || !agentBidders[0].StartsWith("worker"))
            {
                agentBidders[0] = this.Environment.Agents[Utils.RandNoGen.Next(noAgents)].Name;
            }

            agentBidders[1] = "";
            while (((agentBidders[1] == "") || (agentBidders[1] == agentBidders[0])) || !agentBidders[1].StartsWith("worker"))
            {
                agentBidders[1] = this.Environment.Agents[Utils.RandNoGen.Next(noAgents)].Name;
            }

            agentBidders[2] = "";
            while ((((agentBidders[2] == "") || (agentBidders[2] == agentBidders[0])) || (agentBidders[2] == agentBidders[1])) || !agentBidders[2].StartsWith("worker"))
            {
                agentBidders[2] = this.Environment.Agents[Utils.RandNoGen.Next(noAgents)].Name;
            }
        }

        public override void Act(Message message)
        {
            try
            {
                Console.WriteLine("\t[{1} -> {0}]: {2}", this.Name, message.Sender, message.Content);

                string action;
                string parameters;
                Utils.ParseMessage(message.Content, out action, out parameters);

                switch (action)
                {
                    case "time":
                        HandleTime(parameters);
                        break;

                    case "results":
                        HandleResults(parameters);
                        break;

                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void HandleTime(string timeEstimate)
        {
            bids.Add(timeEstimate);
            if (bids.Count == Utils.NoBids)
                AnalyzeCoalitions();
        }

        private void HandleResults(string results)
        {
            string[] toks = results.Split();
            for (int i = 0; i < Utils.NoBids; i++)
            {
                double r = currentTask.Price * Convert.ToDouble(toks[i]);
                Send(agentBidders[i], Utils.Str("reward", r));
            }
            if (taskCount >= Utils.NoTasks)
            {
                Console.WriteLine("Manager has finished");

                foreach (Agent a in this.Environment.Agents)
                {
                    if (a.Name.StartsWith("worker"))
                        Send(a.Name, "report");
                }

                Stop();
            }
            else
            {
                GenerateNewTask();
            }
        }

        private void AnalyzeCoalitions()
        {
            int[,] timeToSolve = new int[Utils.NoBids, Utils.NoAttributes];

            for (int i = 0; i < Utils.NoBids; i++)
            {
                string[] toks = bids[i].Split(new char[0]);
                for (int j = 0; j < Utils.NoAttributes; j++)
                    timeToSolve[i, j] = Convert.ToInt32(toks[j]);
            }

            int v1 = (timeToSolve[0, 0] + timeToSolve[0, 1]) + timeToSolve[0, 2];
            int v2 = (timeToSolve[1, 0] + timeToSolve[1, 1]) + timeToSolve[1, 2];
            int v3 = (timeToSolve[2, 0] + timeToSolve[2, 1]) + timeToSolve[2, 2];
            int v12 = (Math.Min(timeToSolve[0, 0], timeToSolve[1, 0]) + Math.Min(timeToSolve[0, 1], timeToSolve[1, 1])) + Math.Min(timeToSolve[0, 2], timeToSolve[1, 2]);
            int v13 = (Math.Min(timeToSolve[0, 0], timeToSolve[2, 0]) + Math.Min(timeToSolve[0, 1], timeToSolve[2, 1])) + Math.Min(timeToSolve[0, 2], timeToSolve[2, 2]);
            int v23 = (Math.Min(timeToSolve[1, 0], timeToSolve[2, 0]) + Math.Min(timeToSolve[1, 1], timeToSolve[2, 1])) + Math.Min(timeToSolve[1, 2], timeToSolve[2, 2]);
            int v123 = (Min3(timeToSolve[0, 0], timeToSolve[1, 0], timeToSolve[2, 0]) + Min3(timeToSolve[0, 1], timeToSolve[1, 1], timeToSolve[2, 1])) + Min3(timeToSolve[0, 2], timeToSolve[1, 2], timeToSolve[2, 2]);

            Send("shapley", Utils.Str("calculate", v1, v2, v3, v12, v13, v23, v123));
        }

        private int Min3(int x1, int x2, int x3)
        {
            return Math.Min(x1, Math.Min(x2, x3));
        }
    }
}