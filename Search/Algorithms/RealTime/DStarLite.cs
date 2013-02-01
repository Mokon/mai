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

  public class DStarLite<SS, DS> : Algorithm<SS, DS>
    where SS : StaticState<SS, DS>
    where DS : DynamicState<SS, DS> {

    private StateVisualizer<SS, DS> sv;

    public DStarLite( Heuristic<SS, DS> H, Transformer<SS, DS> Transformer,
      StateVisualizer<SS, DS> sv ) {
      this.Transformer = Transformer;
      this.H = H;
      this.sv = sv;
    }

    private Transformer<SS, DS> Transformer;
    protected Heuristic<SS, DS> H;
    private Dictionary<string, Metric> RHS;
    private Dictionary<string, Metric> G;
    private PriorityQueue<SvenKey, DS> U;
    private Metric km;
    private DS Sgoal;
    private DS Sstart;
    private SS ss;
    private Operator<SS, DS>[] Ops;

    private void init( Goal<SS, DS> Goal, SS ss, DS Sstart, Operator<SS, DS>[] Actions ) {
      RHS = new Dictionary<string, Metric>( );
      G = new Dictionary<string, Metric>( );
      this.Sgoal = Transformer.Transform( Goal, Sstart );
      this.Sstart = Sstart;
      this.ss = ss;
      this.Ops = Actions;
    }

    private Metric h( DS Sstart, DS Sgoal ) {
      return H.H( ss, Sstart, Sgoal, Ops );
    }

    public Metric c( DS s1, DS s2 ) {
      if ( s1.IsInvalid( ss, false ) || s2.IsInvalid( ss, false ) ) {
        return Metric.Infinity;
      } else {
        return h( s1, s2 );
      }
    }

    private Metric rhs( DS ds ) {
      Metric r;
      if ( RHS.TryGetValue( ds.ToString( ), out r ) ) {
        return r;
      } else {
        return Metric.PositiveInfinity;
      }
    }

    private void rhs( DS ds, Metric value ) {
      string key = ds.ToString( );
      if ( RHS.ContainsKey( key ) ) {
        RHS.Remove( key );
      }
      RHS.Add( key, value );
    }

    private Metric g( DS ds ) {
      Metric r;
      if ( G.TryGetValue( ds.ToString( ), out r ) ) {
        return r;
      } else {
        return Metric.PositiveInfinity;
      }
    }

    private void g( DS ds, Metric value ) {
      string key = ds.ToString( );
      if ( G.ContainsKey( key ) ) {
        G.Remove( key );
      }
      G.Add( key, value );
    }

    private SvenKey CalculateKey( DS s ) {
      return new SvenKey( ) {
        k1 = Min( g( s ), rhs( s ) ) + h( Sstart, s ) + km,
        k2 = Min( g( s ), rhs( s ) )
      };
    }

    private void Initialize( ) {
      U = new PriorityQueue<SvenKey, DS>( );
      km = Metric.Zero;
      // for all s E S rhs(s) = g(s) = inf ; // Implicit
      rhs( Sgoal, Metric.Zero );
      U.Insert( Sgoal, new SvenKey( ) {
        k1 = h( Sstart, Sgoal ),
        k2 = Metric.Zero
      } );
    }

    private void UpdateVertex( DS u ) {
      bool there = U.Contains( u );
      if ( g( u ) != rhs( u ) && there ) {
        U.Update( u, CalculateKey( u ) );
      } else if ( g( u ) != rhs( u ) && !there ) {
        U.Insert( u, CalculateKey( u ) );
      } else if ( g( u ) == rhs( u ) && there ) {
        U.Remove( u );
      }
    }

    private void ComputeShortestPath( ) {
      while ( TopKey( ) < CalculateKey( Sstart ) || rhs( Sstart ) > g( Sstart ) ) {
        this.Expansions++;
#if OpenGl
        if ( sv != null ) {
          this.sv.VisualizeAlg( ss, ss.InitialDynamicState, new OpenGLStateVisualizer.OpenGlVisulizationData( ) {
            G = G, RHS = this.RHS, U = this.U as PriorityQueue<SvenKey, GenericGridWorldDynamicState>
          } );
        }
#endif
        var u = U.Top( );
        var kold = TopKey( );
        var knew = CalculateKey( u );
        if ( kold < knew ) {
          U.Update( u, knew );
        } else if ( g( u ) > rhs( u ) ) {
          g( u, rhs( u ) );
          U.Remove( u );
          foreach ( var s in Pred( u ) ) {
            this.Generations++;
            if ( !s.Equals( Sgoal ) ) {
              rhs( s, Min( rhs( s ), c( s, u ) + g( u ) ) );
            }
            UpdateVertex( s );
          }
        } else {
          var gold = g( u );
          g( u, Metric.PositiveInfinity );
          foreach ( var s in PredUnionSelf( u ) ) {
            this.Generations++;
            if ( rhs( s ) == ( c( s, u ) + gold ) ) {
              if ( !s.Equals( Sgoal ) ) {
                rhs( s, Min( sp => c( s, sp ) + g( sp ), Succ( s ) ) );
              }
            }
            UpdateVertex( s );
          }
        }
      }
    }

    public override IEnumerable<Path<SS, DS>> Compute( SS ss, DS ds,
      Goal<SS, DS> Goal, Operator<SS, DS>[] Actions ) {

      init( Goal, ss, ds, Actions );

      var Slast = Sstart;
      Initialize( );
      ComputeShortestPath( );
      while ( !Sstart.Equals( Sgoal ) ) {
        if ( rhs( Sstart ).Equals( Metric.PositiveInfinity ) ) {
          yield return null;
        } else {
          yield return new Path<SS, DS>( ExtractPath( ).Actions.First.Value );
          Sstart = ArgMin( sp => c( Sstart, sp ) + g( sp ), Succ( Sstart ) );
        }
        if ( ss.HasRecentChanges() ) {
          km += h( Slast, Sstart );
          Slast = Sstart;
          foreach ( var u in ss.Changes ) {
            foreach ( var v in Succ( u ) ) {
              UpdateDirectedEdge( u, v );
              UpdateDirectedEdge( v, u );
            }
          }
          ComputeShortestPath( );
        }
      }
      yield break;
    }

    private Path<SS, DS> ExtractPath( ) {
      var acts = new LinkedList<Operator<SS, DS>>( );
      DS state = Sstart;
      DS next;

      while ( !state.Equals( Sgoal ) ) {
        next = ArgMin( sp => c( state, sp ) + g( sp ), Succ( state ) );
        foreach ( var action in Ops ) {
          if ( action.IsValidOn( state, ss, false ) && action.PerformOn( state, ss ).First( ).Equals( next ) ) {
            acts.AddLast( action );
            break;
          }
        }
        state = next;
        break;
      }
      return new Path<SS, DS>( (DS)null ) {
        Actions = acts
      };
    }

    private void UpdateDirectedEdge( DS u, DS v ) {
      Metric cold = c( u, v ).Equals( Metric.Infinity ) ? h( u, v ) : Metric.Infinity;
      if ( cold > c( u, v ) ) {
        if ( !u.Equals( Sgoal ) ) {
          rhs( u, Min( rhs( u ), c( u, v )+ g( v ) ) );
        }
      } else if ( rhs( u ) == cold + g( v ) ) {
        if ( !u.Equals( Sgoal ) ) {
          rhs( u, Min( sp => c( u, sp ) + g( sp ), Succ( u ) ) );
        }
      }
      UpdateVertex( u );
    }

    private Metric Min( Func<DS, Metric> func, params DS[] args ) {
      Metric min = Metric.PositiveInfinity;
      foreach ( DS d in args ) {
        Metric v = func( d );
        if ( v < min ) {
          min = v;
        }
      }
      return min;
    }

    private Metric Min( params Metric[] args ) {
      Metric min = Metric.PositiveInfinity;
      foreach ( Metric d in args ) {
        if ( d < min ) {
          min = d;
        }
      }
      return min;
    }

    private DS ArgMin( Func<DS, Metric> func, params DS[] args ) {
      DS arg = null;
      Metric min = Metric.PositiveInfinity;
      foreach ( DS d in args ) {
        Metric v = func( d );
        if ( v <= min ) {
          min = v;
          arg = d;
        }
      }
      return arg;
    }

    private DS[] Pred( DS u ) {
      return u.Neighbors( Ops, ss, false ).ToArray( );
    }

    private DS[] PredUnionSelf( DS u ) {
      var _ = u.Neighbors( Ops, ss, false );
      _.AddFirst( u );
      return _.ToArray( );
    }

    private DS[] Succ( DS u ) {
      return u.Neighbors( Ops, ss, false ).ToArray( );
    }

    private SvenKey TopKey( ) {
      var _ = U.TopKey( );
      if ( _ == null ) {
        _ = new SvenKey( ) {
          k1 = Metric.PositiveInfinity,
          k2 = Metric.PositiveInfinity
        };
      }
      return _;
    }

  }
}
