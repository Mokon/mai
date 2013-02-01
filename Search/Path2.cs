/*
 * This file is part of the MAI framework, distributed under The M License.
 * Copyright © 2008-2010, David Bond, All Rights Resevered
 */
using System;
using System.Collections.Generic;
using System.Linq;
using Net.Mokon.Utilities;

namespace Net.Mokon.Edu.Unh.CS.AI.Search
{

    /// <summary>
    /// An class which represents a set of actions which can be take upon
    /// a state space to get from an initial state to a goal state. A path does
    /// not store the orginal state and therefore if one wishes to recreate this
    /// the initial state must be stored elsewhere.
    /// </summary>
    public class Path2<SS, DS>
        where SS : StaticState<SS, DS>
        where DS : DynamicState<SS, DS>
    {
      public override int GetHashCode( ) {
        return base.GetHashCode( );
      }

        public IEnumerable<Path2<SS, DS>> GetIEnumerable()
        {
            return new Path2<SS, DS>[] { this };
        }

        /// <summary>
        /// A list of the state/action pairs to follow this path. These should be in the order
        /// front to last in order of time. In other words the first action in the
        /// list is the first action to take upon the first state in the list.
        /// </summary>
        internal LinkedList<STuple<DS, Operator<SS, DS>>> Actions;

        /// <summary>
        /// A dictionary which maps a state to a state/action element in Actions.
        /// </summary>
        internal Dictionary<DS, LinkedListNode<STuple<DS, Operator<SS, DS>>>> States;

        /// <summary>
        /// Constructor of a path.
        /// </summary>
        /// <param name="Actions">The actions to perform the path.</param>
        public Path2(DS InitialState)
        {
            LinkedListNode<STuple<DS, Operator<SS, DS>>> node;

            this.Actions = new LinkedList<STuple<DS, Operator<SS, DS>>>();
            this.States = new Dictionary<DS, LinkedListNode<STuple<DS, Operator<SS, DS>>>>();
            this.cost = Metric.Zero;

            if (InitialState != null)
            {
                node = this.Actions.AddFirst(new STuple<DS, Operator<SS, DS>>(InitialState, null));
                this.States.Add(InitialState, node);
            }
        }

        public Path2(Operator<SS, DS> o)
            : this((DS)null)
        {
            this.Actions.AddFirst(new STuple<DS, Operator<SS, DS>>(null, o));
        }

        /// <summary>
        /// Add an action to the end of the path.
        /// </summary>
        /// <param name="Action"></param>
        public void Push(Operator<SS, DS> Action, DS ToPush, DS newState)
        {
            LinkedListNode<STuple<DS, Operator<SS, DS>>> node;

            this.Actions.Last.Value.Second = Action;
            node = this.Actions.AddLast(new STuple<DS, Operator<SS, DS>>(newState, null));
            this.States.Add(newState, node);

            this.cost += Action.Cost(ToPush);
        }

        /// <summary>
        /// Add an action to the end of the path.
        /// </summary>
        /// <param name="Action"></param>
        public void PushFirst(Operator<SS, DS> Action, DS ToPush, DS newState)
        {
            LinkedListNode<STuple<DS, Operator<SS, DS>>> node;

            node = this.Actions.AddFirst(new STuple<DS, Operator<SS, DS>>(ToPush, Action));
            this.States.Add(ToPush, node);
            this.cost += Action.Cost(ToPush);
        }

        /// <summary>
        /// Converts the path into a simple human readable string.
        /// </summary>
        /// <returns>A string representation of the path.</returns>
        public override string ToString()
        {
            if (this.Actions.Any())
            {
                return (from x in Actions
                        select x.Second.ToString() + "\r\n").
                         Aggregate((x, y) => y + x);
            }
            else
            {
                return "<Empty>";
            }
        }

        /// <summary>
        /// Returns a clone of the path.
        /// </summary>
        /// <returns>A clone.</returns>
        public virtual object Clone()
        {
            Path2<SS, DS> Path = this.MemberwiseClone() as Path2<SS, DS>;

            Path.Actions = new LinkedList<STuple<DS, Operator<SS, DS>>>();
            foreach (var e in this.Actions)
            {
                Path.Actions.AddLast(e);
            }

            Path.States = new Dictionary<DS, LinkedListNode<STuple<DS, Operator<SS, DS>>>>();
            foreach (KeyValuePair<DS, LinkedListNode<STuple<DS, Operator<SS, DS>>>> kvp in this.States)
            {
                Path.States.Add(kvp.Key, kvp.Value);
            }

            return Path;
        }

        /// <summary>
        /// Pops the front of the path.
        /// </summary>
        /// <returns></returns>
        public STuple<DS, Operator<SS, DS>> popFront()
        {
            DS firstDS = this.Actions.First.Value.First;
            Operator<SS, DS> firstA = this.Actions.First.Value.Second;

            this.Actions.RemoveFirst();
            this.States.Remove(firstDS);

            this.cost -= firstA.Cost(firstDS);

            return new STuple<DS, Operator<SS, DS>>(firstDS, firstA);
        }

        /// <summary>
        /// Pops the back of the path.
        /// </summary>
        /// <returns></returns>
        public STuple<DS, Operator<SS, DS>> popBack()
        {
            DS lastDS = this.Actions.Last.Value.First;
            Operator<SS, DS> lastA = this.Actions.Last.Value.Second;

            this.Actions.RemoveLast();
            this.States.Remove(lastDS);

            this.cost -= this.Actions.Last.Value.Second.Cost(this.Actions.Last.Value.First);
            this.Actions.Last.Value.Second = null;

            return new STuple<DS, Operator<SS, DS>>(lastDS, lastA);
        }

        public override bool Equals(object obj)
        {
            return this.ToString().Equals(obj.ToString());
        }

        /// <summary>
        /// Peeks at the front of the path.
        /// </summary>
        /// <returns></returns>
        public STuple<DS, Operator<SS, DS>> peekFront()
        {
            DS firstDS = this.Actions.First.Value.First;
            Operator<SS, DS> firstA = this.Actions.First.Value.Second;
            return new STuple<DS, Operator<SS, DS>>(firstDS, firstA);
        }

        /// <summary>
        /// Peeks at the back of the path.
        /// </summary>
        /// <returns></returns>
        public STuple<DS, Operator<SS, DS>> peekBack()
        {
            DS lastDS = this.Actions.Last.Value.First;
            Operator<SS, DS> lastA = this.Actions.Last.Value.Second;
            return new STuple<DS, Operator<SS, DS>>(lastDS, lastA);
        }

        /// <summary>
        /// Book keeping values for path work.
        /// </summary>
        internal bool rdy = false;
        internal IEnumerator<DS> StatesEnumerator = null;
        internal IEnumerator<Operator<SS, DS>> ActionsEnumerator = null;

        /// <summary>
        /// Computes the cost of the path based on the action costs.
        /// </summary>
        /// <returns>The cost of the path.</returns>
        public Metric Cost
        {
            get
            {
                return this.cost;
            }
        }

        /// <summary>
        /// The internal cost.
        /// </summary>
        private Metric cost;

        /// <summary>
        /// Computes the cost of the path based on the action costs.
        /// </summary>
        /// <returns>The cost of the path.</returns>
        public int ActionCount
        {
            get
            {
                return this.Actions.Count;
            }
        }

    }

}
