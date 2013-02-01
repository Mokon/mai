/*

 * This file is part of the MAI framework, distributed under The M License.
 * Copyright © 2008-2010, David Bond, All Rights Resevered
 */
using System;
using System.Collections.Generic;
using System.Linq;
using Net.Mokon.Utilities;
using Net.Mokon.Edu.Unh.CS.AI.Harness;
using Net.Mokon.Edu.Unh.CS.AI.GridWorld;
using Net.Mokon.Edu.Unh.CS.AI.Search.Goals;
using Net.Mokon.Edu.Unh.CS.AI.Search.Heuristics;

namespace Net.Mokon.Edu.Unh.CS.AI.Search.Algorithms.RealTime {

  public class LRTAStar<SS, DS> : RealTimeAlgorithm<SS, DS>
    where SS : StaticState<SS, DS>
    where DS : DynamicState<SS, DS> {

    bool Static = true;

    public LRTAStar( Heuristic<SS, DS> Heuristic,
      bool Static, StateVisualizer<SS, DS> sv,
      Metric Weight, bool RTAStar ) {
      this.Heuristic = Heuristic;
      this.Static = Static;
      this.sv = sv;
      this.Weight = Weight;
      this.RTAStar = RTAStar;
    }

    private bool RTAStar;
    private StateVisualizer<SS, DS> sv;
    private Dictionary<string, Metric> HAdjusted;
    private Heuristic<SS, DS> Heuristic;
    private Metric Weight;

    private static Metric h( DS state, Dictionary<string, Metric> HAdjusted,
      Heuristic<SS, DS> Heuristic, Goal<SS, DS> Goal, Operator<SS, DS>[] Actions, SS ss ) {
      Metric r;
      if ( HAdjusted != null && HAdjusted.TryGetValue( state.ToString( ), out r ) ) {
        return r;
      } else {
        return Heuristic.H( ss, state, Goal, Actions );
      }
    }

    private static void h( DS state, Metric newValue, Dictionary<string, Metric> HAdjusted ) {
      if ( HAdjusted == null ) {
        return;
      }
      string key = state.ToString( );
      if ( HAdjusted.ContainsKey( key ) ) {
        HAdjusted.Remove( key );
      }
      HAdjusted.Add( key, newValue );
    }

    /// <summary>
    /// The compute method which uses greedy search.
    /// </summary>
    /// <param name="StaticState">A static state to work on.</param>
    /// <param name="DynamicState">A dynamic state to work on.</param>
    /// <param name="Goal">The goal state(s) to search for.</param>
    /// <returns>Computes a path.</returns>
    public override IEnumerable<Path<SS, DS>> Compute( SS StaticState,
      DS DynamicState, Goal<SS, DS> Goal, Operator<SS, DS>[] Actions ) {
      HAdjusted = new Dictionary<string, Metric>( );
      while ( true ) {
        yield return AStarLookAhead( StaticState.InitialDynamicState, StaticState,
          ComputationLimit, sv, ref Expansions, ref Generations, Static,
          Actions, HAdjusted, Heuristic, Goal, Weight, RTAStar );
      }
    }

    public static Path<SS, DS> AStarLookAheadNotSliced( DS Start, SS ss,
      int ComputationLimit, StateVisualizer<SS, DS> sv,
      ref uint Expansions, ref uint Generations, bool Static,
      Operator<SS, DS>[] Actions, Dictionary<string, Metric> HAdjusted,
      Heuristic<SS, DS> Heuristic, Goal<SS, DS> Goal, Metric Weight, bool RTAStar ) {

      if ( ComputationLimit <= 0 ) {
        throw new ArgumentException( "Computation Limit Invalid" );
      }

      DS Cur = null;

      Start.ResetPath( );
      int IterationExpansions = 0;
      PriorityQueue<Metric, DS> OpenList = new PriorityQueue<Metric, DS>( );
      HashSet<DS> ClosedList = new HashSet<DS>( );
      OpenList.Enqueue( h( Start, HAdjusted, Heuristic, Goal, Actions, ss ), Start );
      while ( true ) {
        if ( OpenList.IsEmpty( ) ) {
          Cur = null;
          break;
        }
#if OpenGl
        if ( sv != null ) {
          sv.VisualizeAlg( ss, ss.InitialDynamicState, new OpenGLStateVisualizer.OpenGlVisulizationData( ) {
            OL = OpenList as PriorityQueue<Metric, GenericGridWorldDynamicState>,
            HAdjusted = HAdjusted,
          } );
        }
#endif
        Cur = OpenList.Dequeue( );
        ClosedList.Add( Cur );
        if ( Goal.IsSatisfiedBy( ss, Cur ) ) {
          break;
        } else {
          if ( ++IterationExpansions > ComputationLimit ) {
            break;
          }
          Expansions++;
          foreach ( DS e in Cur.Expand( Actions, ss, Static ) ) {
            if ( !ClosedList.Contains( e ) && !OpenList.Contains( e ) ) {
              OpenList.Enqueue( Weight*h( e, HAdjusted, Heuristic, Goal, Actions, ss )
                + gComputer.G( ss, e, Goal ), e );
            }
            Generations++;
          }
        }
      }
      Metric Best = null;
      Operator<SS, DS> BestOp = null;
      Metric SecondBest = null;
      if ( Cur != null ) {
        Best = Weight*h( Cur, HAdjusted, Heuristic, Goal, Actions, ss )
                + gComputer.G( ss, Cur, Goal );
        BestOp = Cur.Path( ).Actions.First.Value;

        if ( !OpenList.IsEmpty( ) ) {
          Cur = OpenList.Dequeue( );
          SecondBest = Weight*h( Cur, HAdjusted, Heuristic, Goal, Actions, ss )
                + gComputer.G( ss, Cur, Goal );
        }
      }

      if ( RTAStar && SecondBest != null ) {
        if ( h( Start, HAdjusted, Heuristic, Goal, Actions, ss ) < SecondBest ) {
          h( Start, SecondBest, HAdjusted );
        }
      } else if ( ( RTAStar && SecondBest == null ) || Best == null ) {
        h( Start, new Metric( double.PositiveInfinity ), HAdjusted );
      } else if ( Best != null ) {
        if ( h( Start, HAdjusted, Heuristic, Goal, Actions, ss ) < Best ) {
          h( Start, Best, HAdjusted );
        }
      }
      if ( BestOp == null ) {
        return null;
      } else {
        return Operator<SS, DS>.MakePath( BestOp );
      }
    }


    public static Path<SS, DS> AStarLookAhead( DS Start, SS ss,
      int ComputationLimit, StateVisualizer<SS, DS> sv,
      ref uint Expansions, ref uint Generations, bool Static,
      Operator<SS, DS>[] Actions, Dictionary<string, Metric> HAdjusted,
      Heuristic<SS, DS> Heuristic, Goal<SS, DS> Goal, Metric Weight, bool RTAStar ) {

      if ( ComputationLimit <= 0 ) {
        throw new ArgumentException( "Computation Limit Invalid" );
      }

      DS[] Neighbors = Start.Expand( Actions, ss, Static ).ToArray( );
      int IterationExpansions = 1;
      Expansions++;

      Dictionary<DS, Metric> GNeighs = new Dictionary<DS, Metric>( );
      Dictionary<DS, Metric> HNeighs = new Dictionary<DS, Metric>( );
      Dictionary<DS, Operator<SS, DS>> OpNeighs 
        = new Dictionary<DS, Operator<SS, DS>>( );

      LinkedList<DS> LookAheadNeighbors = new LinkedList<DS>( );
      foreach ( DS Neighbor in Neighbors ) {
        GNeighs.Add( Neighbor, Neighbor.Path( ).Cost );
        OpNeighs.Add( Neighbor, Neighbor.Path( ).Actions.First.Value );
        Neighbor.ResetPath( );
        Metric HNeigh = null;
        if ( HAdjusted != null && HAdjusted.TryGetValue( Neighbor.ToString( ), out HNeigh ) ) {
          HNeighs.Add( Neighbor, HNeigh );
        } else {
          LookAheadNeighbors.AddFirst( Neighbor );
        }
      }

      Dictionary<DS, PriorityQueue<Metric, DS>> OpenLists = 
        new Dictionary<DS, PriorityQueue<Metric, DS>>( );
#if OpenGl
      HashSet<DS> OpenStates = new HashSet<DS>( );
#endif
      HashSet<DS> ClosedList = new HashSet<DS>( );
      ClosedList.Add( Start );
      foreach ( DS Neighbor in LookAheadNeighbors ) {
        var ol = new PriorityQueue<Metric, DS>( );
        ol.Enqueue( h( Neighbor, HAdjusted, Heuristic, Goal, Actions, ss ), Neighbor );
        OpenLists.Add( Neighbor, ol );
#if OpenGl
        OpenStates.Add( Neighbor );
#endif
      }

      while ( true ) {
        LinkedList<DS> OpenNeighbors = new LinkedList<DS>( LookAheadNeighbors );
        foreach ( DS Neighbor in LookAheadNeighbors ) {
          PriorityQueue<Metric, DS> OpenList;
          OpenLists.TryGetValue( Neighbor, out OpenList );
#if OpenGl
          if ( sv != null ) {
            sv.VisualizeAlg( ss, ss.InitialDynamicState, new OpenGLStateVisualizer.OpenGlVisulizationData( ) {
              List = OpenStates as HashSet<GenericGridWorldDynamicState>,
              HAdjusted = HAdjusted,
            } );
          }
#endif
          if ( OpenList.IsEmpty( ) ) {
            HNeighs.Add( Neighbor, new Metric( double.PositiveInfinity ) );
            OpenNeighbors.Remove( Neighbor );
            continue;
          }
          DS Cur = OpenList.Dequeue( );
#if OpenGl
          OpenStates.Remove( Cur );
#endif

          ClosedList.Add( Cur );
          if ( Goal.IsSatisfiedBy( ss, Cur ) ) {
            HNeighs.Add( Neighbor, Cur.Path( ).Cost );
            OpenNeighbors.Remove( Neighbor );
            continue;
          } else {
            if ( ++IterationExpansions > ComputationLimit ) {
              HNeighs.Add( Neighbor, Cur.Path( ).Cost 
                + Weight*h( Cur, HAdjusted, Heuristic, Goal, Actions, ss ) );
              OpenNeighbors.Remove( Neighbor );
              break;
            }
            Expansions++;
            foreach ( DS e in Cur.Expand( Actions, ss, Static ) ) {
              if ( !ClosedList.Contains( e ) && !OpenList.Contains( e ) ) {
                OpenList.Enqueue( Weight*h( e, HAdjusted, Heuristic, Goal, Actions, ss )
                + gComputer.G( ss, e, Goal ), e );
#if OpenGl
                OpenStates.Add( e );
#endif
              }
              Generations++;
            }
          }

        }
        LookAheadNeighbors = OpenNeighbors;
        if ( IterationExpansions > ComputationLimit || LookAheadNeighbors.Count == 0 ) {
          break;
        }
      }
      foreach ( DS Neighbor in LookAheadNeighbors ) {
        PriorityQueue<Metric, DS> OpenList;
        OpenLists.TryGetValue( Neighbor, out OpenList );
        if ( OpenList.IsEmpty( ) ) {
          HNeighs.Add( Neighbor, new Metric( double.PositiveInfinity ) );
        } else {
          DS Cur = OpenList.Dequeue( );
          HNeighs.Add( Neighbor, Cur.Path( ).Cost
          + Weight*h( Cur, HAdjusted, Heuristic, Goal, Actions, ss ) );
        }
      }

      Metric Best = null;
      Operator<SS, DS> BestOp = null;
      Metric SecondBest = null;

      foreach ( DS Neighbor in Neighbors ) {
        Metric HNeigh = null;
        Metric GNeigh = null;
        Operator<SS, DS> OpNeigh = null;
        HNeighs.TryGetValue( Neighbor, out HNeigh );
        GNeighs.TryGetValue( Neighbor, out GNeigh );
        OpNeighs.TryGetValue( Neighbor, out OpNeigh );
        if ( HNeigh != null ) {
          Metric FNeigh =  GNeigh + HNeigh;
          if ( Best == null || Best > FNeigh ) {
            BestOp = OpNeigh;
            Best = FNeigh;
          } else if ( SecondBest == null || SecondBest > FNeigh ) {
            SecondBest = FNeigh;
          }
        }
      }
      if ( RTAStar && SecondBest != null ) {
        if ( h( Start, HAdjusted, Heuristic, Goal, Actions, ss ) < SecondBest ) {
          h( Start, SecondBest, HAdjusted );
        }
      } else if ( ( RTAStar && SecondBest == null ) || Best == null ) {
        h( Start, new Metric( double.PositiveInfinity ), HAdjusted );
      } else if ( Best != null ) {
        if ( h( Start, HAdjusted, Heuristic, Goal, Actions, ss ) < Best ) {
          h( Start, Best, HAdjusted );
        }
      }
      if ( BestOp == null ) {
        return null;
      } else {
        return Operator<SS, DS>.MakePath( BestOp );
      }
    }


    public static Path<SS, DS> BreadthFirstLookAhead( DS Start, SS ss,
      int ComputationLimit, StateVisualizer<SS, DS> sv,
      ref uint Expansions, ref uint Generations, bool Static,
      Operator<SS, DS>[] Actions, Dictionary<string, Metric> HAdjusted,
      Heuristic<SS, DS> Heuristic, Goal<SS, DS> Goal, Metric Weight, bool RTAStar, Random Gen ) {
      Start.ResetPath( );

      if ( ComputationLimit <= 0 ) {
        throw new ArgumentException( "Computation Limit Invalid" );
      }
      DS[] Neighbors = Start.Expand( Actions, ss, Static ).ToArray( );
      Expansions++;
      Metric Best = null;
      Operator<SS, DS> BestOp = null;
      Metric SecondBest = null;

      foreach ( DS Neighbor in Neighbors ) {
        Metric GNeigh = Neighbor.Path( ).Cost;
        Operator<SS, DS> OpNeigh = Neighbor.Path( ).Actions.First.Value;
        Metric HNeigh = null;
        if ( HAdjusted == null || !HAdjusted.TryGetValue( Neighbor.ToString( ), out HNeigh ) ) {
          Neighbor.ResetPath( );
          Metric Alpha = new Metric( double.PositiveInfinity );
          Stack<DS> ToVisit = new Stack<DS>( );
          Stack<int> Depths = new Stack<int>( );
          HashSet<DS> ClosedList = new HashSet<DS>( );
          ToVisit.Push( Neighbor );
          Depths.Push( 0 );
          ClosedList.Add( Start );
          while ( ToVisit.Count > 0 ) {
#if OpenGl
            if ( sv != null ) {
              sv.VisualizeAlg( ss, ss.InitialDynamicState, new OpenGLStateVisualizer.OpenGlVisulizationData( ) {
                List = new HashSet<DS>( ToVisit ) as HashSet<GenericGridWorldDynamicState>,
                HAdjusted = HAdjusted,
              } );
            }
#endif
            DS Top = ToVisit.Pop( );
            int Depth = Depths.Pop( );
            ClosedList.Add( Top );
            Metric TVal = gComputer.G( ss, Top, Goal ) + Weight *
                  h( Top, HAdjusted, Heuristic, Goal, Actions, ss );
            if ( TVal >= Alpha ) {
              continue;
            }
            if ( HNeigh == null || HNeigh > TVal ) {
              HNeigh = TVal;
            }
            if ( ComputationLimit == Depth ) {
              Alpha = Metric.Min( Alpha, TVal );
            } else {
              Expansions++;
              foreach ( DS n in Top.Expand( Actions, ss, Static ) ) {
                Generations++;
                if ( !ClosedList.Contains( n ) && !ToVisit.Contains( n ) ) {
                  ToVisit.Push( n );
                  Depths.Push( Depth + 1 );
                }
              }
            }
          }
        }
        if ( HNeigh != null ) {
          Metric FNeigh =  GNeigh + HNeigh;
          if ( Best == null || Best > FNeigh ) {
            BestOp = OpNeigh;
            Best = FNeigh;
          } else if ( SecondBest == null || SecondBest > FNeigh ) {
            SecondBest = FNeigh;
          }
        }
      }
      if ( RTAStar && SecondBest != null ) {
        if ( h( Start, HAdjusted, Heuristic, Goal, Actions, ss ) < SecondBest ) {
          h( Start, SecondBest, HAdjusted );
        }
      } else if ( ( RTAStar && SecondBest == null ) || Best == null ) {
        h( Start, new Metric( double.PositiveInfinity ), HAdjusted );
      } else if ( Best != null ) {
        if ( h( Start, HAdjusted, Heuristic, Goal, Actions, ss ) < Best ) {
          h( Start, Best, HAdjusted );
        }
      }
      if ( BestOp == null ) {
        return null;
      } else {
        return Operator<SS, DS>.MakePath( BestOp );
      }
    }

  }

}
