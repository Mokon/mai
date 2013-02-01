/*
 * This file is part of the MAI framework, distributed under The M License.
 * Copyright © 2008-2010, David Bond, All Rights Resevered
 */
using System;
using System.Collections.Generic;
using System.Linq;
using Net.Mokon.Edu.Unh.CS.AI.GridWorld;
using Net.Mokon.Edu.Unh.CS.AI.Harness;
using Net.Mokon.Edu.Unh.CS.AI.Search.Goals;
using Net.Mokon.Edu.Unh.CS.AI.Search.Heuristics;
using Net.Mokon.Utilities;

namespace Net.Mokon.Edu.Unh.CS.AI.Search.Algorithms.RealTime {

  /// <summary>
  /// This is an algorithm which implements the greedy search alg.
  /// </summary>
  public class RealTimeTBAStar<SS, DS> : RealTimeAlgorithm<SS, DS>
    where SS : StaticState<SS, DS>
    where DS : DynamicState<SS, DS> {

    /// <summary>
    /// The best expression for sorting elements in the open list.
    /// </summary>
    protected Func<SS, DS, Goal<SS, DS>, Operator<SS, DS>[], Metric>
      BestExpresion;

    StateVisualizer<SS, DS> sv;

    public RealTimeTBAStar( Heuristic<SS, DS> H, Transformer<SS, DS> Transformer,
             float r, int c, StateVisualizer<SS, DS> sv,
             ReconstructPathStartingPoint rpsp, ShortcutDetection sd ) {
      this.r = r;
      this.c = c;
      this.sv = sv;
      Alg = new LPAStar<SS, DS>( H, Transformer, sv, rpsp );
      this.H = H;
      this.Transformer = Transformer;
      this.sd = sd;
    }

    private float r;
    private int c;
    private RealTimeAlgorithm<SS, DS> Alg;
    private DS Sstart;
    private DS Sgoal;
    private SS StaticState;
    private Transformer<SS, DS> Transformer;
    protected Heuristic<SS, DS> H;
    private Operator<SS, DS>[] Actions;

    public bool traceBack( ref Path<SS, DS> pathNew, ref Path<SS, DS> pathNewTemp,
        DS loc, int Nt, ref int remainingNt, DS Start, SS ss ) {
      int times;
      bool DoneTrace;

      DoneTrace = false;
      times = 0;

      if ( !pathNew.rdy ) {
        pathNew.rdy = true;
        pathNewTemp = new Path<SS, DS>( (DS)null );

        pathNew.StatesEnumerator = pathNew.States.Reverse( ).GetEnumerator( );
        pathNew.ActionsEnumerator = pathNew.Actions.Reverse( ).GetEnumerator( );
        pathNew.StatesEnumerator.MoveNext( );
        pathNew.ActionsEnumerator.MoveNext( );
      }

      while ( pathNew.StatesEnumerator.Current != null &&
                pathNew.ActionsEnumerator.Current != null &&
                !pathNew.StatesEnumerator.Current.Equals( loc ) &&
                !pathNew.StatesEnumerator.Current.Equals( Sstart ) &&
                times < Nt ) {
#if OpenGl
        if ( this.sv != null ) {
          this.sv.VisualizeAlg( ss, ss.InitialDynamicState,
            new OpenGLStateVisualizer.OpenGlVisulizationData( ) {
              DS2 = pathNew.StatesEnumerator.Current
                as GenericGridWorldDynamicState,
            } );
        }
#endif
        pathNewTemp.PushFirst( pathNew.ActionsEnumerator.Current,
          pathNew.StatesEnumerator.Current, pathNew.StatesEnumerator.Current );

        pathNew.StatesEnumerator.MoveNext( );
        pathNew.ActionsEnumerator.MoveNext( );
        times++;
      }

      if ( pathNew.StatesEnumerator.Current != null &&
                pathNew.StatesEnumerator.Current.Equals( loc ) ||
                pathNew.StatesEnumerator.Current.Equals( Sstart ) ) {
        pathNewTemp.States.AddFirst( pathNew.StatesEnumerator.Current );
        pathNewTemp.StateHashTable.Add( pathNew.StatesEnumerator.Current );
        pathNew.rdy = false;
        DoneTrace = true;
      }

      remainingNt = Nt - times;

      this.Expansions += (uint)Math.Ceiling( (double)( ( (double)times ) /
                    ( (double)c ) ) );
      return DoneTrace;
    }

    private ShortcutDetection sd;

    public override IEnumerable<Path<SS, DS>> Compute( SS StaticState,
      DS DynamicState, Goal<SS, DS> Goal, Operator<SS, DS>[] Actions ) {
      int Ne;
      int remainingNe;
      int Nt;
      int remainingNt;
      bool pathFollowIsValid = true;
      Metric pathFollowCost = Metric.Zero;
      Metric pathToTraceCost = Metric.Zero;
      Path<SS, DS> pathToTrace = null;
      Path<SS, DS> pathFollow = null;
      Path<SS, DS> pathNew = null;
      Path<SS, DS> pathTraced = null;
      Path<SS, DS> pathTaken = new Path<SS, DS>( DynamicState );
      bool doneTrace = true;
      DS loc = DynamicState;
      DS loc_last = loc;
      Operator<SS, DS> op = null;
      Operator<SS, DS> op_last = null;

      this.StaticState = StaticState;
      this.Sstart = DynamicState;
      this.Sgoal = Transformer.Transform( Goal, Sstart );
      this.Actions = Actions;

      Ne = Convert.ToInt32( ComputationLimit * r );
      if ( Ne <= 0 )
        Ne = 1;
      Nt = ( ComputationLimit - Convert.ToInt32( ComputationLimit * r ) ) * c;
      if ( Nt <= 0 )
        Nt = 1;

      var searchStep = Alg.Compute(
        StaticState, DynamicState, Goal, Actions ).GetEnumerator( );
      Alg.SetComputationLimit( Math.Min( Ne, Nt ) );

      while ( !Goal.IsSatisfiedBy( StaticState, loc ) ) {
        remainingNe = Alg.GetComputationLimit( );
        searchStep.MoveNext( );
        remainingNe -= Alg.GetComputationDone( );

        if ( searchStep.Current != null )
          pathNew = searchStep.Current;

        if ( doneTrace && pathNew != null && ( !pathFollowIsValid ||
                 ( pathToTrace == null || !pathNew.Equals( pathToTrace ) ) &&
                 ( pathToTrace == null || pathNew.Cost > pathToTraceCost ) ) ) {
          pathToTrace = pathNew;
          pathToTraceCost = pathToTrace.Cost;
          doneTrace = false;
        }

        remainingNt = Nt;
        if ( !doneTrace ) {
          doneTrace = traceBack( ref pathToTrace, ref pathTraced, loc, Nt,
            ref remainingNt, DynamicState, StaticState );
        }

        if ( doneTrace && ( !pathFollowIsValid || pathFollow == null 
            || pathToTraceCost > pathFollowCost ||
            ( pathFollow.Actions.Count != 0 &&
            !pathFollow.peekFront( ).Second.IsValidOn(
                loc, StaticState, true ) ) ) ) {
          pathFollow = pathTraced;
          pathFollowCost = pathToTraceCost;
          pathToTrace = null;
          pathFollowIsValid = true;
        }

        // shortcut detection
        if ( sd == ShortcutDetection.AStar &&
            ( remainingNe + ( remainingNt / c ) ) > 0 &&
            ( !pathFollow.StateHashTable.Contains( loc ) ) ) {
          Path<SS, DS> ret = null;
#if DEBUG
          System.Console.Error.WriteLine("expansions = {0}",
            remainingNe + (remainingNt / c));
#endif
          ret = AStarSearch( pathTraced, pathTaken, loc,
            remainingNe + ( remainingNt / c ) );

          if ( ret != null ) {
            pathFollow = ret;
            pathFollowIsValid = true;
          }
        }

        if ( pathFollow != null && pathFollow.StateHashTable.Contains( loc ) ) {
          while ( pathFollow.Actions.Count != 0 &&
              !pathFollow.peekFront( ).First.Equals( loc ) ) {
            pathFollow.popFront( );
          }
        }

        if ( pathFollow.Actions.Count == 0 || ( pathFollow.Actions.Count != 0 &&
            pathFollow.peekFront( ).First.Equals( loc ) &&
            !pathFollow.peekFront( ).Second.IsValidOn(
              loc, StaticState, true ) ) ) {
          pathFollowIsValid = false;
        }

        // Movement Choice
        if ( pathFollowIsValid &&
            pathFollow.Actions.Count != 0 &&
            pathFollow.peekFront( ).First.Equals( loc ) &&
            pathFollow.peekFront( ).Second.IsValidOn(
              loc, StaticState, true ) ) {
          op = pathFollow.popFront( ).Second;
          loc = op.PerformOn( loc_last, StaticState ).First( );
          pathTaken.Push( op, loc_last, loc );
        } else if ( !loc.Equals( DynamicState ) ) {
          op = new ReverseOperator<SS, DS>( pathTaken.popBack( ).Second );
          loc = op.PerformOn( loc_last, StaticState ).First( );
        } else {
          if ( pathTraced != null && pathTraced.Cost < pathToTraceCost ) {
            pathTraced = null;
            pathToTrace = null;
            doneTrace = true;
          }
          if ( op_last == null ) {
            op = null;
          } else {
            op = new ReverseOperator<SS, DS>( op_last );
            loc = op.PerformOn( loc_last, StaticState ).First( );
            pathTaken.Push( op, loc_last, loc );
          }
        }

        if ( op == null ) {
          Alg.SetComputationLimit( Ne );
          yield return null;
        } else {
#if DEBUG
            System.Console.Error.WriteLine("  loc = " + loc);
            System.Console.Error.WriteLine("  op = " + op);

            if (!op.IsValidOn(loc_last, StaticState, true) || loc.IsInvalid(StaticState))
            {
                System.Console.Error.WriteLine("ERROR");
            }
#endif
          loc_last = loc;
          op_last = op;

          var p = new Path<SS, DS>( (DS)null );
          p.Push( op, loc, null );

          Alg.SetComputationLimit( Ne );
          yield return p;
        }

#if DEBUG
        if (!loc.Equals(StaticState.InitialDynamicState)) {
            System.Console.Error.WriteLine("loc = " + loc.ToString() 
              + " true = " + StaticState.InitialDynamicState.ToString());
            System.Console.Error.WriteLine("");
        }
#endif
      }

      this.Expansions += this.Alg.Expansions;
      this.Generations += this.Alg.Generations;
      yield break;
    }

    private Path<SS, DS> AStarSearch( Path<SS, DS> pathFollow,
        Path<SS, DS> pathOld, DS loc, int Expansions ) {
      bool found;
      PriorityQueue<Metric, DS> OpenList = new PriorityQueue<Metric, DS>( );
      HashSet<DS> ClosedList = new HashSet<DS>( );
      DS intersection = null;
      Path<SS, DS> ret = new Path<SS, DS>( loc );
      Path<SS, DS> reverse = null;

      var Sgoal = loc.Clone( ) as DS;
      Sgoal.ResetPath( );

      // find last intersection point of pathFollow and pathOld
      foreach ( var p in pathFollow.States.Reverse( ) ) {
        if ( pathOld.StateHashTable.Contains( p ) ) {
          intersection = p;
          break;
        }
      }

      // insert all states on pathFollow from intersection
      // onwards into OpenList
      bool add = false;
      foreach ( var p in pathFollow.States ) {
        if ( p.Equals( intersection ) )
          add = true;
        if ( add ) {
          var c = p.Clone( ) as DS;
          c.ResetPath( );
          OpenList.Insert( c, this.H.H( StaticState, c, this.Sgoal, Actions ) );
        }
      }

      // run A* for Expansions iterations or until agent's
      // current position 'Sgoal' is found
      DS Cur = null;
      found = false;
      for ( int i = 0 ; i < Expansions && !OpenList.IsEmpty( ) ; i++ ) {
        Cur = OpenList.Dequeue( );
        ClosedList.Add( Cur );

        if ( Sgoal.Equals( Cur ) ) {
          found = true;
          break;
        }

        foreach ( DS e in Cur.Expand( Actions, StaticState, false ) ) {
          if ( !ClosedList.Contains( e ) ) {
            OpenList.Enqueue( e.Path( ).Cost 
              + H.H( StaticState, e, Sgoal, Actions ), e );
          }
          this.Generations++;
        }

        i++;
      }

      // if we didnt get to the agent's current location,
      // fail out
      if ( !found )
        return null;

      // iterate backwards over path path
      // going from agent's current location -> state on pathFollow
      reverse = Cur.Path( );
      reverse.StatesEnumerator = reverse.States.Reverse( ).GetEnumerator( );

      DS state, prev;
      Operator<SS, DS> op;

      state = null;
      reverse.StatesEnumerator.MoveNext( );
      prev = reverse.StatesEnumerator.Current;
      while ( reverse.StatesEnumerator.MoveNext( ) ) {
        state = reverse.StatesEnumerator.Current;

        op = null;
        foreach ( var action in Actions ) {
          if ( action.PerformOn(
              prev, StaticState ).First( ).Equals( state ) ) {
            op = action;
            break;
          }
        }

        ret.Push( op, prev, state );
        prev = state;
      }

      // append remaining part of pathTraced (everything after 'state')
      add = false;
      foreach ( var p in pathFollow.States ) {
        if ( p.Equals( state ) ) {
          add = true;
          prev = p;
          continue;
        }

        if ( add ) {
          state = p;

          op = null;
          foreach ( var action in Actions ) {
            if ( action.PerformOn(
                prev, StaticState ).First( ).Equals( state ) ) {
              op = action;
              break;
            }
          }

          ret.Push( op, prev, state );
          prev = state;
        }
      }

      return ret;
    }
  }

  public enum ShortcutDetection {
    None,
    AStar
  }

}
