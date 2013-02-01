/*
 * This file is part of the MAI framework, distributed under The M License.
 * Copyright © 2008-2010, David Bond, All Rights Resevered
 */
using System;
using System.Linq;
using System.Collections.Generic;
using Net.Mokon.Utilities;
using Net.Mokon.Edu.Unh.CS.AI.Search.Goals;
using Net.Mokon.Edu.Unh.CS.AI.Search.Heuristics;

namespace Net.Mokon.Edu.Unh.CS.AI.Search.Algorithms.RealTime {

  /// <summary>
  /// This is an algorithm which implements the greedy search alg.
  /// </summary>
  public class TBAStar2<SS, DS> : RealTimeAlgorithm<SS, DS>
    where SS : StaticState<SS, DS>
    where DS : DynamicState<SS, DS> {

    /// <summary>
    /// The best expression for sorting elements in the open list.
    /// </summary>
    protected Func<SS, DS, Goal<SS, DS>, Operator<SS, DS>[], Metric>
      BestExpresion;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="BestExpresion">The best expression for sorting elements
    /// in the open list.</param>
    /// <param name="Limit">The limit bound. Default to 0 if no bound
    /// wanted.</param>
    /// <param name="UseClosedList">Whether or not a closed list vs upward
    /// travesal loop detection will be used to detect cycles.</param>
    public TBAStar2( Func<SS, DS, Goal<SS, DS>, Operator<SS, DS>[],
      Metric> BestExpresion, float r, int c ) {
      this.BestExpresion = BestExpresion;
      this.r = r;
      this.c = c;
      Alg = new BestFirstSearch<SS, DS>( BestExpresion );
    }


    public TBAStar2( Heuristic<SS, DS> H, Transformer<SS, DS> Transformer,
             float r, int c, StateVisualizer<SS, DS> sv ) {
      this.r = r;
      this.c = c;
      Alg = new LPAStar<SS, DS>( H, Transformer, sv, ReconstructPathStartingPoint.UTop);
    }

    private float r;
    private int c;
    private RealTimeAlgorithm<SS, DS> Alg;

    public bool traceBack( ref Path<SS, DS> pathNew, ref Path<SS, DS> pathNewTemp, DS loc, int Nt, DS Start ) {
      if ( !pathNew.rdy ) {
        pathNew.rdy = true;
        pathNewTemp = new Path<SS, DS>( (DS)null );

        pathNew.StatesEnumerator = pathNew.States.Reverse( ).GetEnumerator( );
        pathNew.ActionsEnumerator = pathNew.Actions.Reverse( ).GetEnumerator( );
      }
      int count = 0;
      int times = 0;
      bool result = false;
      while ( count <= Nt ) {
        times++;
        bool NotDoneTrace = pathNew.StatesEnumerator.MoveNext( ) && pathNew.ActionsEnumerator.MoveNext( );
        if ( NotDoneTrace ) {
          if ( pathNew.StatesEnumerator.Current.Equals( loc ) ) {
            pathNewTemp.States.AddFirst( pathNew.StatesEnumerator.Current );
            pathNewTemp.StateHashTable.Add( pathNew.StatesEnumerator.Current );
            pathNew.rdy = false;
            result = true;
            break;
          }
          pathNewTemp.PushFirst( pathNew.ActionsEnumerator.Current, pathNew.StatesEnumerator.Current, pathNew.StatesEnumerator.Current );
          continue;
        } else {
          pathNewTemp.States.AddFirst( Start );
          pathNewTemp.StateHashTable.Add( Start );
          pathNew.rdy = false;
          result = true;
          break;
        }
      }
      uint k = this.Expansions;
      this.Expansions += (uint)Math.Ceiling( (double)( ( (double)times ) /
              ( (double)c ) ) );
      if ( this.Expansions - k == 0 ) {
        throw new Exception( );
      }
      return result;
    }

    public override IEnumerable<Path<SS, DS>> Compute( SS StaticState, DS DynamicState,
      Goal<SS, DS> Goal, Operator<SS, DS>[] Actions ) {
      bool solutionFound = false;
      bool solutionFoundAndTraced = false;
      bool doneTrace = true;
      DS loc = DynamicState;
      Path<SS, DS> pathNew = null;
      Path<SS, DS> pathFollow = null;
      Path<SS, DS> pathNewBuild = null;
      Path<SS, DS> pathTaken = new Path<SS, DS>( DynamicState );
      Alg.SetComputationLimit( Convert.ToInt32( ComputationLimit * r ) );
      var searchStep = Alg.Compute( StaticState, DynamicState, Goal, Actions
        ).GetEnumerator( );
      Alg.SetComputationLimit( Math.Min( Convert.ToInt32( ComputationLimit * r ),
        ( ComputationLimit - Convert.ToInt32( ComputationLimit * r ) ) * c ) );
      while ( !Goal.IsSatisfiedBy( StaticState, loc ) ) {
        if ( !solutionFound ) {
          solutionFound = !searchStep.MoveNext( );
          Alg.SetComputationLimit( Convert.ToInt32( ComputationLimit * r ) );
        }
        if ( !solutionFoundAndTraced ) {
          if ( doneTrace ) {
            pathNew = searchStep.Current;
          }
          doneTrace = traceBack( ref pathNew, ref pathNewBuild, loc, ( ComputationLimit -
                      Convert.ToInt32( ComputationLimit * r ) ) * c, DynamicState );
          if ( doneTrace && pathNewBuild.ActionCount != 0 ) {
            pathFollow = pathNewBuild;
            if ( Goal.IsSatisfiedBy( StaticState, pathFollow.States.Last.Value ) ) {
              solutionFoundAndTraced = true;
            }
          }
        }
        var locO = loc;
        Operator<SS, DS> op;
        if ( pathFollow == null ) {
          yield return null;
        } else {
          if ( pathFollow.StateHashTable.Contains( loc ) ) {
            if ( pathFollow.Actions.Count != 0 &&
                     pathFollow.Actions.First.Value.IsValidOn( loc, StaticState, false ) ) {
              var result = pathFollow.popFront( );
              loc = pathFollow.States.First.Value;
              op = result.Second;
              pathTaken.Push( op, locO, loc );
            } else {
              op = new ReverseOperator<SS, DS>( pathTaken.Actions.Last.Value );
              loc = op.PerformOn( locO, StaticState ).First( );
              pathTaken.Push( op, locO, loc );
            }
          } else {
            if ( !loc.Equals( DynamicState ) ) {
              pathTaken.StateHashTable.Remove( pathTaken.States.Last.Value );
              pathTaken.States.RemoveLast( );
              op = new ReverseOperator<SS, DS>( pathTaken.Actions.Last.Value );
              pathTaken.Actions.RemoveLast( );
              loc = pathTaken.States.Last.Value;

            } else {
              op = new ReverseOperator<SS, DS>( pathTaken.Actions.Last.Value );
              loc = op.PerformOn( locO, StaticState ).First( );
              pathTaken.Push( op, locO, loc );
            }
          }
          if ( !op.IsValidOn( locO, StaticState, false ) ) {
            System.Console.Error.WriteLine( "ERROR" );
          }
          var p = new Path<SS, DS>( (DS)null );
          p.Push( op, loc, null );
          yield return p;
        }
      }
      this.Expansions += this.Alg.Expansions;
      this.Generations += this.Alg.Generations;
      yield break;
    }


  }
}
