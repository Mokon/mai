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
  /// This is an algorithm which implements BRTS.
  /// </summary>
  public class BRTS<SS, DS> : RealTimeAlgorithm<SS, DS>
    where SS : StaticState<SS, DS>
    where DS : DynamicState<SS, DS> {

    /// <summary>
    /// A transformer for the goal.
    /// </summary>
    private Transformer<SS, DS> Transformer;

    /// <summary>
    /// Whether or not to sort the open list every step.
    /// </summary>
    private bool Sort;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="BestExpresion">The best expression for sorting elements
    /// in the open list.</param>
    /// <param name="Limit">The limit bound. Default to 0 if no bound
    /// wanted.</param>
    /// <param name="UseClosedList">Whether or not a closed list vs upward
    /// travesal loop detection will be used to detect cycles.</param>
    public BRTS( Heuristic<SS, DS> HeuristicSS, bool Sort,
      Transformer<SS, DS> Transformer ) {
      this.Transformer = Transformer;
      this.HeuristicSS = HeuristicSS;
      this.Sort = Sort;
    }

    /// <summary>
    /// The heursitic function for BRTS.
    /// </summary>
    protected Heuristic<SS, DS> HeuristicSS;


    /// <summary>
    /// The compute method which uses greedy search.
    /// </summary>
    /// <param name="StaticState">A static state to work on.</param>
    /// <param name="DynamicState">A dynamic state to work on.</param>
    /// <param name="Goal">The goal state(s) to search for.</param>
    /// <returns>Computes a path.</returns>
    public override IEnumerable<Path<SS, DS>> Compute( SS StaticState,
      DS DynamicState, Goal<SS, DS> Goal, Operator<SS, DS>[] Actions ) {
      HashSet<DS> DeadEndList = new HashSet<DS>( );
      InStateGoal<SS, DS> FakeGoal = new InStateGoal<SS, DS>( DynamicState );
      DS GoalState = Transformer.Transform( Goal, DynamicState );
      uint IterationExpansions = 0;
      ExtendedDictionary<DS, DS> tree = new ExtendedDictionary<DS, DS>( x => null );
      ExtendedDictionary<DS, Metric> G = new ExtendedDictionary<DS, Metric>( k => Metric.Infinity );
      G.Set( DynamicState, Metric.Zero );
      PriorityQueue<Metric, DS> OpenList = new PriorityQueue<Metric, DS>( );
      HashSet<DS> ClosedList = new HashSet<DS>( );
      OpenList.Enqueue( Metric.Zero, GoalState );
      while ( true ) {
        if ( OpenList.IsEmpty( ) ) {
          throw new PathNotFoundException( "Path Not Found" );
        }
        var Cur = OpenList.DequeueNode( );
        ClosedList.Add( Cur.Second );
        if ( FakeGoal.IsSatisfiedBy( StaticState, Cur.Second ) ) {
          LinkedList<Operator<SS, DS>> Operators = new LinkedList<Operator<SS,
            DS>>( );
          foreach ( var a in Algorithm<SS, DS>.ExtractPath( StaticState.InitialDynamicState,
            Cur.Second, tree, Actions, StaticState ).Actions ) {
            Operators.AddFirst( new ReverseOperator<SS, DS>( a ) );
          }
          yield return new Path<SS, DS>( (DS)null ) {
            Actions = Operators
          };
          yield break;
        } else {
          this.Expansions++;
          if ( ++IterationExpansions >= ComputationLimit ) {
            IEnumerable<DS> dss = DynamicState.Expand( Actions, StaticState,
              true ).Where(
              ds => !DeadEndList.Contains( ds ) );
            if ( dss.All( ds => HeuristicSS.H( StaticState, ds, Cur.Second,
              Actions ) >
              HeuristicSS.H( StaticState, DynamicState, Cur.Second, Actions ) )
              ) {
              DeadEndList.Add( DynamicState );
            }
            if ( !dss.Any( ) ) {
              dss = DynamicState.Expand( Actions, StaticState, true );
            }
            DS s = dss.ArgMin(
              ds => HeuristicSS.H( StaticState, ds, Cur.Second, Actions ) );
            var p = Algorithm<SS, DS>.ExtractPath( StaticState.InitialDynamicState,
            Cur.Second, tree, Actions, StaticState );
            yield return p;
            DynamicState = p.Actions.First( ).PerformOn( DynamicState,
              StaticState ).First( );
            if ( OpenList.Contains( DynamicState ) ) {
              DS st;
              do {
                st = OpenList.Dequeue( );
              } while ( !st.Equals( DynamicState ) ); //TODO add method for this.
              LinkedList<Operator<SS, DS>> Operators = 
                new LinkedList<Operator<SS, DS>>( );
              foreach ( var a in Algorithm<SS, DS>.ExtractPath( StaticState.InitialDynamicState,
            st, tree, Actions, StaticState ).Actions ) {
                Operators.AddFirst( new ReverseOperator<SS, DS>( a ) );
              }
              yield return new Path<SS, DS>( (DS)null ) {
                Actions = Operators
              };
              yield break;
            }
            if ( ClosedList.Contains( DynamicState ) ) {
              LinkedList<Operator<SS, DS>> Operators = 
                new LinkedList<Operator<SS, DS>>( );
              var pgot = Algorithm<SS, DS>.ExtractPath( StaticState.InitialDynamicState,
            ClosedList.First( ds => ds.Equals(
                DynamicState ) ), tree, Actions, StaticState );
              foreach ( var a in pgot.Actions ) {
                Operators.AddFirst( new ReverseOperator<SS, DS>( a ) );
              }
              yield return new Path<SS, DS>( (DS)null ) {
                Actions = Operators
              };
              yield break;
            }
            FakeGoal = new InStateGoal<SS, DS>( DynamicState );
            IterationExpansions = 0;
            OpenList.Enqueue( Cur.First, Cur.Second );
            if ( Sort ) {
              /*PriorityQueue<Metric, DS> sortedOpen = 
                new PriorityQueue<Metric, DS>( );
              DS x;
              while ( ( x = OpenList.Dequeue( ) ) !=  null ) {
                sortedOpen.Enqueue( G.Lookup( Cur.Second )  + e.LastOperator.Cost( Cur.Second ), x );
              }
              OpenList = sortedOpen;
               */
            }
          } else {
            foreach ( DS e in Cur.Second.Expand( Actions, StaticState, true
              ) ) {
              if ( !ClosedList.Contains( e ) && !OpenList.Contains( e ) ) {
                Metric cost = G.Lookup( Cur.Second )  + e.LastOperator.Cost( Cur.Second );
                if ( G.Lookup( e ) > cost ) {
                  G.Set( e, cost );
                  tree.Set( e, Cur.Second );
                  OpenList.Enqueue( cost, e );
                }
              }
              this.Generations++;
            }
          }
        }
      }
    }

  }

}
