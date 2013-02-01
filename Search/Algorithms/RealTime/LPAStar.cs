/*
 * This file is part of the MAI framework, distributed under The M License.
 * Copyright © 2008-2010, David Bond, All Rights Resevered
 */
using System;
using System.Linq;
using System.Collections.Generic;
using Net.Mokon.Utilities;
using Net.Mokon.Edu.Unh.CS.AI.Search.Algorithms.RealTime;
using Net.Mokon.Edu.Unh.CS.AI.GridWorld;
using Net.Mokon.Edu.Unh.CS.AI.Harness;
using Net.Mokon.Edu.Unh.CS.AI.Search.Goals;
using Net.Mokon.Edu.Unh.CS.AI.Search.Heuristics;

namespace Net.Mokon.Edu.Unh.CS.AI.Search.Algorithms.RealTime 
{

    public class LPAStar<SS, DS> : RealTimeAlgorithm<SS, DS>
        where SS : StaticState<SS, DS>
        where DS : DynamicState<SS, DS>
    {

        private StateVisualizer<SS, DS> sv;

        #region constructor
        public LPAStar(Heuristic<SS, DS> H, Transformer<SS, DS> Transformer,
          StateVisualizer<SS, DS> sv, ReconstructPathStartingPoint rpsp)
        {
            this.Transformer = Transformer;
            this.H = H;
            this.sv = sv;
            this.rpsp = rpsp;
        }

        private Transformer<SS, DS> Transformer;
        protected Heuristic<SS, DS> H;
        #endregion

        #region valueFunctions

        /*private void pp(DS x, DS y)
        {
            string key = x.ToString();
            if (PP.ContainsKey(key))
            {
                PP.Remove(key);
            }
            PP.Add(key, y);
        }

        private DS pp(DS x)
        {
            DS y;
            if (PP.TryGetValue(x.ToString(), out y))
            {
                return y;
            }
            else
            {
                return null;
            }
        } */

        private Metric h( DS Sstart, DS Sgoal )
        {
            return H.H(ss, Sstart, Sgoal, Actions);
        }

        public Metric c( DS s1, DS s2 )
        {
          Metric r;
            if (C.TryGetValue(s1.ToString() + s2.ToString(), out r))
            {
                return r;
            }
            else
            {
                return h(s1, s2);
            }
        }

        private void c( DS s1, DS s2, Metric value )
        {
            string key = s1.ToString() + s2.ToString();
            if (C.ContainsKey(key))
            {
                C.Remove(key);
            }
            C.Add(key, value);
        }

        private Metric rhs( DS ds )
        {
          Metric r;
            if (RHS.TryGetValue(ds.ToString(), out r))
            {
                return r;
            }
            else
            {
              return Metric.PositiveInfinity;
            }
        }

        private void rhs( DS ds, Metric value )
        {
            string key = ds.ToString();
            if (RHS.ContainsKey(key))
            {
                RHS.Remove(key);
            }
            RHS.Add(key, value);
        }

        private Metric g( DS ds )
        {
          Metric r;
            if (G.TryGetValue(ds.ToString(), out r))
            {
                return r;
            }
            else
            {
              return Metric.PositiveInfinity;
            }
        }

        private void g( DS ds, Metric value )
        {
            string key = ds.ToString();
            if (G.ContainsKey(key))
            {
                G.Remove(key);
            }
            G.Add(key, value);
        }
        #endregion

        #region variables
        private Dictionary<string, Metric> RHS;
        private Dictionary<string, Metric> G;
        private Dictionary<string, Metric> C;
        private Dictionary<string, DS> PP;
        private PriorityQueue<SvenKey, DS> U;
        private DS Sgoal;
        private DS Sstart;
        private SS ss;
        private Operator<SS, DS>[] Actions;

        private void init(Goal<SS, DS> Goal, SS ss, DS Sstart, Operator<SS, DS>[] Actions)
        {
          RHS = new Dictionary<string, Metric>( );
          G = new Dictionary<string, Metric>( );
          C = new Dictionary<string, Metric>( );
            PP = new Dictionary<string, DS>();
            this.Sgoal = Transformer.Transform(Goal, Sstart);
            this.Sstart = Sstart;
            this.ss = ss;
            this.Actions = Actions;
        }
        #endregion

        #region LPAStarFunctions

        private SvenKey CalculateKey(DS s)
        {
            return new SvenKey()
              {
                  k1 = Min(g(s), rhs(s)) + h(s, Sgoal),
                  k2 = Min(g(s), rhs(s))
              };
        }

        private void Initialize()
        {
            U = new PriorityQueue<SvenKey, DS>();
            // for all s E S rhs(s) = g(s) = inf ; // Implicit
            rhs( Sstart, Metric.Zero);
            U.Insert(Sstart, CalculateKey(Sstart));
        }

        private void UpdateVertex(DS u)
        {
            //if (!u.Equals(Sstart))
            if (!u.Equals(Sstart))
            {
                rhs(u, Min(up => g(up) + c(up, u), Pred(u).ToArray()));
            }
            if (U.Contains(u))
            { // NOTE! Contains may not be a O(1) op
                U.Remove(u);
            }
            if (g(u) != rhs(u))
            {
                /* update parent pointer */
                //pp(u, ArgMin(up => g(up) + c(up, u), Pred(u).ToArray()));
                U.Insert(u, CalculateKey(u));
            }
        }

        private LinkedList<DS> Pred(DS u)
        {
            return u.Expand(Actions, ss, false);
        }

        /*private LinkedList<DS> RemoveLoops(Path<SS, DS> path, LinkedList<DS> pred)
        {
            var ret = new LinkedList<DS>();
            foreach (var p in pred)
            {
                if (path.StateHashTable.Contains(p))
                {
                    continue;
                }
                ret.AddLast(p);
            }
            return ret;
        }*/

        

        private LinkedList<DS> Succ(DS u)
        {
            return u.Expand(Actions, ss, false);
        }

        private LinkedList<DS> SuccUnionSelf(DS u)
        {
            var _ = Succ(u);
            _.AddFirst(u);
            return _;
        }

        private SvenKey TopKey()
        {
            var _ = U.TopKey();
            if (_ == null)
            {
                _ = new SvenKey()
                  {
                      k1 = Metric.PositiveInfinity,
                      k2 = Metric.PositiveInfinity
                  };
            }
            return _;
        }

        private void ComputeShortestPath(ref int ComputationLimit)
        {
            while (ComputationLimit != 0 && (TopKey() < CalculateKey(Sgoal) || rhs(Sgoal) != g(Sgoal)))
            {
#if OpenGl
              if ( this.sv != null ) {
                this.sv.VisualizeAlg( ss, ss.InitialDynamicState, new OpenGLStateVisualizer.OpenGlVisulizationData( ) {
                  G = G,
                  RHS = this.RHS,
                  U = this.U as PriorityQueue<SvenKey, GenericGridWorldDynamicState>,
                  PP = this.PP as Dictionary<string, GenericGridWorldDynamicState>
                } );
              }
#endif
                var u = U.Dequeue();

#if DEBUG
                System.Console.Error.WriteLine("U.Top                 " + u.ToString());
#endif

                if (g(u) > rhs(u))
                {
                    g(u, rhs(u));

                    foreach (var s in Succ(u))
                    {
                        UpdateVertex(s);
                    }
                }
                else
                {
                  g( u, Metric.PositiveInfinity );
                    foreach (var s in SuccUnionSelf(u))
                    {
                        UpdateVertex(s);
                    }
                }

                ComputationLimit--;
            }
        }

        private Metric Min( Func<DS, Metric> func, params DS[] args )
        {
          Metric min = Metric.PositiveInfinity;
            foreach (DS d in args)
            {
              Metric v = func( d );
                if (v < min)
                {
                    min = v;
                }
            }
            return min;
        }

        private Metric Min( params Metric[] args )
        {
          Metric min = Metric.PositiveInfinity;
          foreach ( Metric d in args )
            {
                if (d < min)
                {
                    min = d;
                }
            }
            return min;
        }
        private DS ArgMin( Func<DS, Metric> func, params DS[] args )
        {
            DS arg = null;
            Metric min = Metric.PositiveInfinity;
            foreach (DS d in args)
            {
              Metric v = func( d );
                if (v <= min)
                {
                    min = v;
                    arg = d;
                }
            }
            if (arg == null)
            {
                return null;
            }
            return arg;
        }

        #endregion

        #region main
        public override IEnumerable<Path<SS, DS>> Compute(SS ss, DS ds,
                                                           Goal<SS, DS> Goal, Operator<SS, DS>[] Actions)
        {
            int n;

            init(Goal, ss, ds, Actions);

            Initialize();
            while (true)
            {
                n = this.ComputationLimit;
                ComputeShortestPath(ref n);

                setComputationDone(ComputationLimit - n);

                if ( n != 0 && g( Sgoal ) == Metric.PositiveInfinity )
                {
                    throw new PathNotFoundException("No path found");
                }

                yield return ReconstructPath(n);

                if (ss.HasRecentChanges())
                {
                    foreach (var u in ss.Changes)
                    {
                        foreach (var v in Succ(u))
                        {
                            UpdateDirectedEdge(u, v);
                            UpdateDirectedEdge(v, u);
                        }

                    }
                }
            }
        #endregion
        }

        private ReconstructPathStartingPoint rpsp;

        private Path<SS, DS> ReconstructPath(int n)
        {
            HashSet<DS> Visted = new HashSet<DS>();
            Path<SS, DS> ret = new Path<SS, DS>((DS)null);
            bool foundAction;
            DS state ;
            DS next;

            if (n != 0)
            {
                state = Sgoal;
            }
            else
            {
                switch (rpsp)
                {
                    case ReconstructPathStartingPoint.UTop:
                        state = U.Top();
                        break;
                    case ReconstructPathStartingPoint.BestH:
                        state = ArgMin(up => h(up, Sgoal), U.ToArray());
                        break;
                    case ReconstructPathStartingPoint.Alternate:
                        if (U.Top().GetHashCode() % 2 == 0)
                        {
                            state = U.Top();
                        }
                        else
                        {
                            state = ArgMin(up => h(up, Sgoal), U.ToArray());
                        }
                        break;
                    default:
                        state = U.Top();
                        break;
                }
            }

#if DEBUG
            System.Console.Error.WriteLine("STATE: {0} G:{1} RHS:{2} N:{3}", state, g(state), rhs(state), n);
#endif

            next = null;

            while (!state.Equals(Sstart))
            {
                if (Visted.Contains(state))
                {
                    return null;
                }
                else
                {
                    Visted.Add(state);
                }

                next = ArgMin(up => g(up) + c(up, state), Pred(state).ToArray());

                if (next == null)
                {
                    return null;
                }

#if DEBUG
                System.Console.Error.WriteLine("state: " + state.ToString() + "next: " + next.ToString());
#endif

                foundAction = false;
                foreach (var action in Actions)
                {
                  try {
                    if ( action.PerformOn( next, ss ).First( ).Equals( state ) ) {
                      ret.PushFirst( action, next, state );
                      foundAction = true;
                      break;
                    }
                  } catch (ArgumentException) {
                    continue;
                  }
                }

                if (!foundAction)
                {
                    return null;
                }

                state = next;
#if OpenGl
                if ( this.sv != null ) {
                  this.sv.VisualizeAlg( ss, ss.InitialDynamicState,
                    new OpenGLStateVisualizer.OpenGlVisulizationData( ) {
                      G = G,
                      RHS = this.RHS,
                      U = this.U as PriorityQueue<SvenKey,
                          GenericGridWorldDynamicState>,
                      DS = next as GenericGridWorldDynamicState,
                      PP = this.PP as Dictionary<string, GenericGridWorldDynamicState>
                    } );
                }
#endif
            }

            ret.States.AddFirst( Sstart );
            ret.StateHashTable.Add( Sstart );
            
            return ret;
        }

        private void UpdateDirectedEdge(DS u, DS v)
        {
            if (u.IsInvalid(ss, false) || v.IsInvalid(ss, false))
            {
              c( u, v, Metric.PositiveInfinity ); // Domain Specific Knownledge
            }
            else
            {
                c(u, v, h(u, v));
            }
            UpdateVertex(v);
        }
    }

    public enum ReconstructPathStartingPoint
    {
        UTop,
        BestH,
        Alternate
    }
}
