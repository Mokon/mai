/*
 * This file is part of the MAI framework, distributed under The M License.
 * Copyright © 2008-2010, David Bond, All Rights Resevered
 */
using System.Threading;
using Net.Mokon.Edu.Unh.CS.AI.GridWorld;
using Net.Mokon.Edu.Unh.CS.AI.Search;
using Net.Mokon.Edu.Unh.CS.AI.Search.Heuristics;
using Net.Mokon.Edu.Unh.CS.AI.Search.Algorithms.RealTime;
using Net.Mokon.Utilities;
using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using System.Diagnostics;

#if OpenGl
using Tao.FreeGlut;
using Tao.OpenGl;
#if ScreenShot
using System.Drawing;
using System.Drawing.Imaging;
#endif

namespace Net.Mokon.Edu.Unh.CS.AI.Harness {

  /// <summary>
  /// The visualizer for the similator.
  /// </summary>
  public class OpenGLStateVisualizer :
    StateVisualizer<GenericGridWorldStaticState, GenericGridWorldDynamicState> {

    /// <summary>
    /// Animates the final scene and resets the visulizer.
    /// </summary>
    public override void Cleanup( ) {
      this.End = true;
      Glut.glutPostRedisplay( );
      Glut.glutMainLoopEvent( );
      Gl.glFinish( );
      this.End = false;
      this.vd = null;
      if ( !NoVis ) {
        Thread.Sleep( SleepTime );
      }
    }

    int SleepTime = 3000;

#if ScreenShot
    /// <summary>
    /// Takes a screenshot of the current screen pixels.
    /// </summary>
    /// <returns></returns>
    public Bitmap TakeScreenshot( ) {
      int w = Glut.glutGet( Glut.GLUT_SCREEN_WIDTH );
      int h = Glut.glutGet( Glut.GLUT_SCREEN_HEIGHT );
      Bitmap bmp = new Bitmap( w, h );
      BitmapData data =  bmp.LockBits( new Rectangle( 0, 0, w, h ),
        ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb );
      Gl.glReadPixels( 0, 0, w, h, Gl.GL_BGR, Gl.GL_UNSIGNED_BYTE, data.Scan0 );
      Gl.glFinish( );
      bmp.UnlockBits( data );
      bmp.RotateFlip( RotateFlipType.RotateNoneFlipY );
      return bmp;
    }
#endif

    private GenericGridWorldStaticState StaticState;

    private GenericGridWorldDynamicState DyanmicState;

    public bool Global = true;

    public OpenGLStateVisualizer( ) {
      int i = 0;
      Glut.glutInit( ref i, null );
      Glut.glutInitDisplayMode( Glut.GLUT_RGBA | Glut.GLUT_DOUBLE );

      Glut.glutCreateWindow( "Map" );
      Glut.glutPositionWindow( 0, 0 );
      Glut.glutFullScreen( );

      Gl.glClearColor( 0.0f, 0.0f, 0.0f, 1.0f );
      Gl.glMatrixMode( Gl.GL_PROJECTION );
      Gl.glLoadIdentity( );
      Gl.glOrtho( 0.0f, winWid, 0.0f, winHeight, -100.0f, 100.0f );
      Gl.glMatrixMode( Gl.GL_MODELVIEW );
      Gl.glLoadIdentity( );
      Glut.glutIdleFunc( new Glut.IdleCallback( redraw ) );
      Glut.glutDisplayFunc( new Glut.DisplayCallback( redraw ) );
      Glut.glutSpecialFunc( new Glut.SpecialCallback( keyboard_special ) );
      Glut.glutKeyboardFunc( new Glut.KeyboardCallback( keyboard ) );
      Global = true;
    }

    public void Presentation( bool Doors, bool states, bool Unknown ) {
      presentation_mode = states;
      AgentGoalSizeRatio = 3;
      DebugViewPort = false;
      this.Doors = Doors;
      viewPortXOrigin = viewPortXOriginFull;
      viewPortYOrigin =  viewPortYOriginFull;
      viewPortXWidth =  viewPortXWidthFull;
      viewPortYHeight =  viewPortYHeightFull;
      this.Unknown = Unknown;
      fast_continuous = true;
      continuous = true;
    }

    private void keyboard_special( int key, int x, int y ) {
      switch ( key ) {
        case Glut.GLUT_KEY_F1: // Presentation Mode
          Presentation( true, true, false );
          break;
        case Glut.GLUT_KEY_F2:
          presentation_mode = !presentation_mode;
          break;
        case (char)Glut.GLUT_KEY_RIGHT:
          this.viewXAdjustment++;
          break;
        case (char)Glut.GLUT_KEY_LEFT:
          this.viewXAdjustment--;
          break;
        case (char)Glut.GLUT_KEY_UP:
          this.viewYAdjustment--;
          break;
        case (char)Glut.GLUT_KEY_DOWN:
          this.viewYAdjustment++;
          break;
      }
      if ( kbsh != null ) {
        kbsh( key, x, y );
      }
    }

    private bool Doors = false;
    private bool presentation_mode = false;
    private bool text = false;
    private bool nextAction = true;
    private bool nextPlanning = true;
    private bool continuous = false;
    private bool fast_continuous = false;
    private bool parent_pointers = false;
    private bool PriorityQueue = false;
    private bool LockedList = false;
    private bool Changes = false;
    private bool Grid = false;
    private bool Times = false;
    private bool Unknown = true;
    private bool NoVis = false;
    private bool HeuristicEstimate = false;
    private bool DebugViewPort = true;

#if ScreenShot
    private bool TakeScreenShot = false;
    private bool Record = false;
    private bool PauseRecord = false;
    private bool path = false;
    private long avi;
    private int avii;
#endif

    private float AgentGoalSizeRatio = 1;

    private void keyboard( byte key, int x, int y ) {
      System.Console.WriteLine( " Key Pressed : {0}", key );
      switch ( (char)key ) {
        case '.':
          if ( AgentGoalSizeRatio < 1000 ) {
            AgentGoalSizeRatio += 0.5f;
          }
          break;
        case ',':
          if ( AgentGoalSizeRatio > 0 ) {
            AgentGoalSizeRatio -= 0.5f;
          }
          break;
        case ']':
          if ( SleepTime < 10000 ) {
            SleepTime += 100;
          }
          break;
        case '[':
          if ( SleepTime > 0 ) {
            SleepTime -= 100;
          }
          break;
        case '-':
          if ( zoom < 1 ) {
            zoom += 0.01f;
          }
          break;
        case '=':
          if ( zoom > 0.01 ) {
            zoom -= 0.01f;
          }
          break;
        case 't':
          Times = !Times;
          break;
        case 'o':
          NoVis = !NoVis;
          break;
        case 'd':
          Changes = !Changes;
          break;
        case 'h':
          HeuristicEstimate = !HeuristicEstimate;
          break;
        case 'q':
          PriorityQueue = !PriorityQueue;
          break;
        case 'k':
          parent_pointers = !parent_pointers;
          break;
        case 'j':
          Doors = !Doors;
          break;
        case 'l':
          LockedList = !LockedList;
          break;
        case 'c':
          continuous = !continuous;
          break;
        case 'u':
          Unknown = !Unknown;
          break;
        case 'g':
          Grid = !Grid;
          break;
        case 'f':
          fast_continuous = !fast_continuous;
          break;
        case 'a':
          nextAction = true;
          break;
        case 'p':
          nextPlanning = true;
          break;
        case 'P':
          path = !path;
          break;
        case 'v':
          text = !text;
          break;
#if ScreenShot
        case 'r':
          this.Record = !this.Record;
          if ( Record ) {
            avi =  System.DateTime.Now.Ticks;
            avii = 0;
            Directory.CreateDirectory( @"Videos/Video"+avi + "/" );
          }
          break;
        case 's':
          this.TakeScreenShot = true;
          break;
#endif
        case 'm':
          DebugViewPort = !DebugViewPort;
          if ( DebugViewPort ) {
            viewPortXOrigin = viewPortXOriginDebug;
            viewPortYOrigin =  viewPortYOriginDebug;
            viewPortXWidth =  viewPortXWidthDebug;
            viewPortYHeight =  viewPortYHeightDebug;
          } else {
            viewPortXOrigin = viewPortXOriginFull;
            viewPortYOrigin =  viewPortYOriginFull;
            viewPortXWidth =  viewPortXWidthFull;
            viewPortYHeight =  viewPortYHeightFull;
          }
          break;
        case (char)27:
          System.Environment.Exit( 0 );
          break;
      }

      if ( kbh != null ) {
        kbh( key, x, y );
      }
    }

    public class OpenGlVisulizationData {
      public Dictionary<string, Metric> G;

      public Dictionary<string, Metric> HAdjusted;

      public Dictionary<string, Metric> RHS;

      public PriorityQueue<SvenKey, GenericGridWorldDynamicState> U;

      public PriorityQueue<Metric, GenericGridWorldDynamicState> OL;
      public HashSet<GenericGridWorldDynamicState> List;

      public RealTimeDStarLite<GenericGridWorldStaticState, GenericGridWorldDynamicState>.USample US;

      public GenericGridWorldDynamicState DS;

      public GenericGridWorldDynamicState DS2;

      public HashSet<GenericGridWorldDynamicState> LL;

      public Dictionary<string, GenericGridWorldDynamicState> PP;
    }

    private OpenGlVisulizationData vd;


    Metric gMax, gMin, hMax, hMin;
    int hitsMax;

    private const float winWid = 100.0f, winHeight = 100.0f;
    private const float viewPortXOriginDebug = 15;
    private const float viewPortYOriginDebug = 8;
    private const float viewPortXWidthDebug =  85;
    private const float viewPortYHeightDebug = 92;

    private const float viewPortXOriginFull = 0;
    private const float viewPortYOriginFull = 0;
    private const float viewPortXWidthFull =  100;
    private const float viewPortYHeightFull = 100;

    private float viewPortXOrigin = viewPortXOriginDebug;
    private float viewPortYOrigin =  viewPortYOriginDebug;
    private float viewPortXWidth =  viewPortXWidthDebug;
    private float viewPortYHeight =  viewPortYHeightDebug;
    private float zoom = 1;

    private int viewXAdjustment = 0;
    private int viewYAdjustment = 0;

    private readonly static double NanoSecPerTick = ( 1000L )*( 1000L )*( 1000L ) / Stopwatch.Frequency;
    private static double ToTime( double Time ) {
      return Math.Round( Time/NanoSecPerTick/1000d/1000d, 4 );
    }

    /// <summary>
    /// This function draws the screen.
    /// </summary>
    public void redraw( ) {
      if ( NoVis ) {
        return;
      }
      lock ( this ) {
        if ( StaticState != null && DyanmicState != null ) {
          /* Initilization ****************************************************/

          // Find Unit Current Location
          Unit u = DyanmicState.FindUnit( 0 );

          int viewXCenter = u.X + viewXAdjustment;
          int viewYCenter = u.Y + viewYAdjustment;

          int viewXWidth =(int)( zoom * StaticState.Width );
          int viewYHeight = (int)( zoom * StaticState.Height );
          int viewXOrigin = (int)( viewXCenter-viewXWidth/2.0f );
          int viewYOrigin = (int)( viewYCenter-viewYHeight/2.0f );

          if ( viewXOrigin < 0 ) {
            viewXOrigin = 0;
          } else if ( viewXOrigin + viewXWidth >= StaticState.Width ) {
            viewXOrigin = (int)StaticState.Width - viewXWidth;
          }

          if ( viewYOrigin < 0 ) {
            viewYOrigin = 0;
          } else if ( viewYOrigin + viewYHeight >= StaticState.Height ) {
            viewYOrigin = (int)StaticState.Height - viewYHeight;
          }

          // Init Increments
          float x = 0;
          float y = 0;
          float xInc = viewPortXWidth/viewXWidth;
          float yInc = viewPortYHeight/viewYHeight;
          Gl.glColor3f( 1, 1, 1 );
          if ( this.DebugViewPort ) {
            Gl.glRasterPos2f( 0, 80 );
            Glut.glutBitmapString( Glut.GLUT_BITMAP_8_BY_13,
             "Key Commands\n\n" +
           "=: Zoom Out\n" +
           "-: Zoom In\n" +
           "t: Trace Hits Path " + ( this.Times ? "*" : "" )  + "\n" +
           "P: Trace Path " + ( this.path ? "*" : "" )  + "\n" +
           "q: Draw Global Search" + ( this.PriorityQueue ? "*" : "" )  + "\n" +
           "k: Draw Parent Pointers" + ( this.parent_pointers ? "*" : "" )  + "\n" +
           "l: Draw Local Search" + ( this.LockedList ? "*" : "" )  + "\n" +
           "c: Step Continuous Planning" + ( this.fast_continuous ? "*" : "" )  + "\n" +
           "f: Step Continuous Action" + ( this.continuous ? "*" : "" )  + "\n" +
           "a: Step Next Action" + ( this.nextAction ? "*" : "" )  + "\n" +
           "a: Heurisitic Estimate" + ( this.HeuristicEstimate ? "*" : "" )  + "\n" +
           "p: Step Next Planning" + ( this.nextPlanning ? "*" : "" )  + "\n" +
           "v: Draw Values" + ( this.text ? "*" : "" )  + "\n" +
#if ScreenShot
           "r: Record Video" + ( this.Record ? "*" : "" )  + "\n" +
           "s: Take ScreenShot" + ( this.TakeScreenShot ? "*" : "" )  + "\n" +
#endif
           "g: Draw Grid" + ( this.Grid ? "*" : "" )  + "\n" +
           "u: Agent's Viewpoint" + ( this.Unknown ? "*" : "" )  + "\n" +
           "d: Draw Agent VP Changes" + ( this.Changes ? "*" : "" )  + "\n" +
           "ESC: quit\n\n\n" +

           "Graphics Info\n\n" +
           "viewXOrigin: " + viewXOrigin + "\n" +
           "viewYOrigin: " + viewYOrigin + "\n" +
           "xInc: " + Math.Round( xInc, 2 ) + "\n" +
           "yInc: " + Math.Round( yInc, 2 ) + "\n" +
           "viewXWidth: " + viewXWidth + "\n" +
           "viewYHeight: " + viewYHeight + "\n\n\n" +

           "Step Info\n\n" +
           "Unit X: " + u.X + "\n" +
           "Unit Y: " + u.Y + "\n" +
           "Generations: " + Generations + "\n" +
           "Mean Expansions: " + Math.Round( MeanExpansions, 2 ) + "\n" +
           "Expansions: " + Expansions + "\n" +
           "Computation Time: " + ToTime( ComputationTime ) + "ms\n" +
           "Max Comp. Time: " + ToTime( MaxComputationTime ) + "ms\n" +
           "Avg. Comp. Time: " + ToTime( AverageComputationTime ) + "ms\n" +
           "Current Path Length: " + Math.Round( PathLength, 2 ) + "\n" +
           "Sub-Optimality: " + Math.Round( PathLength/OptimalPathLength, 2 ) + " times A*\n" +           
           "Max Memory Usage: " + MaxMemoryUsage/1024/1024 + "mb\n\n\n" +

           "General Info\n\n" +
           "Dest X: " + u.DstX + "\n" + 
           "Dest Y:" + u.DstY + "\n" +
           "Zoom: " + zoom + "\n" +
           "Sleep Time: " + this.SleepTime + "\n" +
           "Computation Limit: " + ComputationLimit + "\n" +
           "Optimal Path Length: " + Math.Round( OptimalPathLength, 2 ) + "\n" +
           ( Resultss != null && Resultss.Where( k => k!= null ).Count( ) > 0  ?
              "Average Path Sub-Optimality " + Math.Round( Resultss.Where( k => k!= null ).Average( t => t.SubOptimality ), 2 ) + "\n" +
              "Trials Run " + Resultss.Count + "\n" 
              : " " ) +
           "Algorithm: " + Alg + "\n" +
           "File: " + File +  "\n\n" );
          }

          // Find UTop
          var ut = vd == null || vd.U == null || vd.U.IsEmpty( ) ? null : vd.U.Top( );
          string UTop = ut == null ? "" : ut.ToString( );

          // Compute G Max
          if ( vd != null && vd.G != null && vd.RHS != null ) {
            this.gValues( );
            //this.rhsValues( );
          }

          if ( vd != null && vd.HAdjusted != null ) {
            this.hValues( );
          }

          // Computes Hits Max
          if ( End || Times ) {
            hitsMax = 0;
            foreach ( var row in StaticState.Tiles ) {
              foreach ( var tile in row ) {
                if ( hitsMax < tile.Hits ) {
                  hitsMax = tile.Hits;
                }
              }
            }
          }


          HashSet<GenericGridWorldDynamicState> ChangesHash = new HashSet<GenericGridWorldDynamicState>( );
          if ( StaticState.Changes != null ) {
            foreach ( var e in StaticState.Changes ) {
              ChangesHash.Add( e );
            }
          }
          if ( vd != null && vd.OL != null ) {
            vd.List = new HashSet<GenericGridWorldDynamicState>( );
            vd.OL.AddToHashSet( vd.List );
          }

          /* Tiles ************************************************************/
          for ( int yT = viewYHeight + viewYOrigin -1 ;
            yT >= viewYOrigin ; yT-- ) {
            x= 0;
            for ( int xT = viewXOrigin ;
              xT < viewXWidth + viewXOrigin ; xT++ ) {
              string s = "<X: " + xT + ", Y: " + yT + " >";
              GenericGridWorldDynamicState DS = new GenericGridWorldDynamicState( );
              Unit U = new Unit( xT, yT, StaticState.Height,
                StaticState.Width, u.DstX, u.DstY, u.LOS, 0 );
              DS.AddWorldObject( U );

              bool close = Distance.OctileDistance( xT, u.X, yT, u.Y ) <= u.LOS;
              bool passable = StaticState.Tiles[yT][xT].IsPassable( StaticState, Unknown );
              bool beenExplorer = StaticState.Tiles[yT][xT].BeenExplored;
              bool isChoke = StaticState.Tiles[yT][xT] is ChokeTile<GenericGridWorldStaticState, GenericGridWorldDynamicState>;

              /* Square Bckgrd ************************************************/
              float r = 0, g = 0, b = 0, a = 1;

              if ( isChoke ) {
                r = 0.9f;
                g = 0.5f;
                b = 0.5f;
              } else if ( close && passable ) {
                r = 1f;
                g = 1;
                b = 1;
              } else if ( close && !passable ) {
                r = 0;
                g = 0;
                b = 0;
              } else if ( beenExplorer && passable ) {
                r = 0.99f;
                g = 0.99f;
                b = 0.99f;
              } else if ( beenExplorer && !passable ) {
                r = 0;
                g = 0;
                b = 0;
              } else if ( !beenExplorer && passable ) {
                r = 0.95f;
                g = 0.95f;
                b = 0.95f;
              } else if ( !beenExplorer && !passable ) {
                r = 0.3f;
                g = 0.3f;
                b = 0.3f;
              }

              if ( !End && vd != null ) {
                if ( vd.DS != null && vd.DS.ToString( ).Equals( s ) ) {
                  r = 1;
                  g = 0;
                  b = 0;
                }
                if ( vd.DS2 != null && vd.DS2.ToString( ).Equals( s ) ) {
                  r = 1;
                  g = 1;
                  b = 1;
                }
              }

              Gl.glColor4f( r, g, b, a );
              Gl.glPushMatrix( );
              Gl.glTranslatef( viewPortXOrigin + x, viewPortYOrigin + y, 0.0f );
              Gl.glRectf( 0, 0, xInc, yInc );
              Gl.glPopMatrix( );

              /* Circle Frgrd **************************************************/
              bool c = false;

              //Presentation Mode
              if ( presentation_mode && vd != null ) {
                // Agent Centered Search Frontier
                if ( !c && vd.List != null && vd.List.Contains( DS ) ) {
                  r = 0.0f;
                  g = 1.0f;
                  b = 1.0f;
                  DrawCircle( r, g, b, xInc, yInc, viewPortXOrigin + x,
                    viewPortYOrigin + y, ( xInc+yInc )/4.5f, 6 );
                  c = true;
                }

                // Global Search
                if ( !c && vd.RHS != null && vd.G != null ) {
                  Metric fr = rhs( s );
                  Metric fg = this.g( s );

                  // Inconsitent States
                  if ( !fr.Equals( fg ) ) {
                    r = 0.0f;
                    g = 0.0f;
                    b = 1.0f;
                    DrawCircle( r, g, b, xInc, yInc, viewPortXOrigin + x,
                    viewPortYOrigin + y, ( xInc+yInc )/4.5f, 6 );
                    c = true;
                  }

                  // Consisent States
                  if ( !c &&!double.IsInfinity( fr.ToDouble( ) ) || !double.IsInfinity( fg.ToDouble( ) ) ) {
                    r = 0.7f;
                    g = 0.0f;
                    b = 0.0f;
                    DrawCircle( r, g, b, xInc, yInc, viewPortXOrigin + x,
                    viewPortYOrigin + y, ( xInc+yInc )/4.5f, 6 );
                    c = true;
                  }
                }

                // Local Search Cached H Values
                if ( !c && vd.HAdjusted != null ) {
                  Metric m = hAdjusted( s );
                  if ( !m.Equals( Metric.Zero ) ) {
                    r = 1.0f;
                    g = 1.0f;
                    b = 0.0f;
                    DrawCircle( r, g, b, xInc, yInc, viewPortXOrigin + x,
                      viewPortYOrigin + y, ( xInc+yInc )/4.5f, 6 );
                    c = true;
                  }

                }
              }

              // Draw Locked List
              if ( vd != null ) {
                if ( !c&& vd.LL != null && LockedList && vd.LL.Contains( DS ) ) {
                  r = 1f;
                  g = 1f;
                  b = 0.0f;
                  DrawCircle( r, g, b, xInc, yInc, viewPortXOrigin + x,
                    viewPortYOrigin + y, ( xInc+yInc )/4.5f, 6 );
                  c = true;
                }
              }
              // Draw Grid Labels
              if ( !c && Grid && viewXWidth < 25 ) {
                Gl.glColor3f( 1, 0, 0 );
                Gl.glRasterPos2f( viewPortXOrigin+x, y+viewPortYOrigin+yInc/2 );
                Glut.glutBitmapString( Glut.GLUT_BITMAP_HELVETICA_10, xT + ", " + yT );
              }

              // Draw Heuristic Estimate Values
              if ( !c && HeuristicEstimate && viewXWidth < 25 ) {
                double Est = Distance.OctileDistance( xT, U.DstX, yT, U.DstY );
                Gl.glColor3f( 1, 0, 0 );
                Gl.glRasterPos2f( viewPortXOrigin+x+xInc/2, y+viewPortYOrigin+yInc/2 );
                Glut.glutBitmapString( Glut.GLUT_BITMAP_HELVETICA_10, Math.Round( Est, 2 ).ToString( ) );
              }

              // Draw Path/Hits
              if ( !c && ( End || Times || path ) && StaticState.Tiles[yT][xT].Hits != 0 ) {
                float cVal = (float)StaticState.Tiles[yT][xT].Hits/hitsMax;
                r = cVal;
                g = 1- cVal;
                b = 0;
                if ( path ) {
                  r = 1;
                  g = 0;
                }
                DrawCircle( r, g, b, xInc, yInc, viewPortXOrigin + x,
                  viewPortYOrigin + y, ( xInc+yInc )/4.5f, 6 );
                c = true;
              }

              // Draw PQ (Open list for global search)
              if ( vd != null ) {
                if ( !c && vd.RHS != null && vd.G != null ) {
                  Metric fr = rhs( s );
                  Metric fg = this.g( s );

                  if ( PriorityQueue ) {
                    if ( !fr.Equals( fg ) ) {
                      if ( vd.U.Contains( s ) ) {
                        r = 0.5f;
                        g = 0.5f;
                        b = 0.5f;
                      } else {
                        r = 1;
                        g = 1;
                        b = 0;
                      }
                      DrawCircle( r, g, b, xInc, yInc, viewPortXOrigin + x,
                        viewPortYOrigin + y, ( xInc+yInc )/4.5f, 6 );
                      c = true;
                    }
                  }

                  // Draw non inconsistent states
                  if ( text && !c ) {
                    if ( !double.IsInfinity( fr.ToDouble( ) ) 
                        || !double.IsInfinity( fg.ToDouble( ) ) ) {
                      if ( s.Equals( UTop ) ) {
                        r = 0;
                        g = 1;
                        b =1;
                      } else if ( vd.US != null && vd.US.Contains( s ) ) {
                        r = 0;
                        g = 0;
                        b =0.5f;
                      } else {
                        float cVal = (float)( ( fg-gMin )/( gMax-gMin ) ).ToDouble( );
                        r = cVal;
                        g = cVal;
                        b = 0;
                      }
                      DrawCircle( r, g, b, xInc, yInc, viewPortXOrigin + x,
                        viewPortYOrigin + y, ( xInc+yInc )/4.5f, 6 );
                      c = true;
                    }

                    if ( text && viewXWidth < 25 ) {
                      Gl.glColor3f( 1, 0, 0 );
                      Gl.glRasterPos2f( viewPortXOrigin+x, y+viewPortYOrigin+yInc/2 );
                      Glut.glutBitmapString( Glut.GLUT_BITMAP_HELVETICA_10,
                       "rhs: " + Math.Round( fr.ToDouble( ), 2 ) + "\r\ng: " + Math.Round( fg.ToDouble( ), 2 ) );
                    }
                  }

                  // Draw Changes
                  if ( !c && Changes && StaticState.Changes != null 
                    && ChangesHash.Contains( DS ) ) {
                    DrawCircle( 0.95f, 0.95f, 0.95f, xInc, yInc, viewPortXOrigin + x,
                      viewPortYOrigin + y, ( xInc+yInc )/4.5f, 6 );
                  }

                  // Draw PP
                  if ( !c && parent_pointers && vd.PP != null ) {
                    GenericGridWorldDynamicState ppds;
                    vd.PP.TryGetValue( s, out ppds );
                    if ( ppds != null ) {
                      Unit unit = ppds.FindUnit( 0 );
                      float dx = ( StaticState.Width-xT )-( StaticState.Width-unit.X ),
                        dy = ( yT )-( unit.Y );

                      Gl.glColor4f( 1, 1, 0, 1 );
                      Gl.glPushMatrix( );
                      Gl.glTranslatef( viewPortXOrigin + x+xInc/2,
                        viewPortYOrigin + y+yInc/2, 0.0f );
                      float deg = (float)( ( dx>=0?-1:1 )*
                      System.Math.Acos( dy/System.Math.Sqrt( dx*dx + dy*dy ) )
                      * 180.0f /(float)System.Math.PI );
                      Gl.glRotatef( deg, 0.0f, 0.0f, 1.0f );
                      Gl.glBegin( Gl.GL_TRIANGLES );
                      float tSize = xInc / 4;
                      Gl.glVertex2f( -1.0f*tSize, -1.0f*tSize );
                      Gl.glVertex2f( 1.0f*tSize, -1.0f*tSize );
                      Gl.glVertex2f( 0.0f*tSize, 2.5f*tSize );
                      Gl.glEnd( );
                      Gl.glPopMatrix( );
                    }
                  }
                }

                // Draw HAdjusted Values! (Cached h values)
                if ( !c && text && vd.HAdjusted != null ) {
                  Metric m = hAdjusted( s );
                  if ( !m.Equals( Metric.Zero ) ) {
                    float cVal = (float)( ( m-hMin )/( hMax-hMin ) ).ToDouble( );
                    r = cVal;
                    g = cVal;
                    b = 0;
                    DrawCircle( r, g, b, xInc, yInc, viewPortXOrigin + x,
                      viewPortYOrigin + y, ( xInc+yInc )/4.5f, 6 );
                    c = true;
                  }

                  if ( HeuristicEstimate && viewXWidth < 25 && !m.Equals( Metric.Zero ) ) {
                    Gl.glColor3f( 1, 0, 0 );
                    Gl.glRasterPos2f( viewPortXOrigin+x + xInc/2, y+viewPortYOrigin+yInc/2 );
                    Glut.glutBitmapString( Glut.GLUT_BITMAP_HELVETICA_10,
                      Math.Round( m.ToDouble( ), 2 ).ToString( ) );
                  }
                }

                // Draw List (This is local search frontier for RTD*)
                if ( !c && LockedList && vd.List != null && vd.List.Contains( DS ) ) { 
                  r = 0.7f;
                  g = 0.7f;
                  b = 0.7f;
                  DrawCircle( r, g, b, xInc, yInc, viewPortXOrigin + x,
                    viewPortYOrigin + y, ( xInc+yInc )/4.5f, 6 );
                  c = true;
                }
              }
              x+=xInc;
            }
            y+=yInc;
          }

          if ( Doors ) {
            y = 0;
            // Detect and Mark Doors
            for ( int yT = viewYHeight + viewYOrigin -1 ;
              yT >= viewYOrigin ; yT-- ) {
              x = 0;
              for ( int xT = viewXOrigin ;
                xT < viewXWidth + viewXOrigin ; xT++ ) {
                bool n, so, w, e;
                try {
                  n = StaticState.Tiles[yT-1][xT].IsPassable( StaticState, Unknown );
                } catch ( IndexOutOfRangeException ) {
                  n = true;
                }

                try {
                  so = StaticState.Tiles[yT+1][xT].IsPassable( StaticState, Unknown );
                } catch ( IndexOutOfRangeException ) {
                  so = true;
                }

                try {
                  w = StaticState.Tiles[yT][xT-1].IsPassable( StaticState, Unknown );
                } catch ( IndexOutOfRangeException ) {
                  w = true;
                }
                try {
                  e = StaticState.Tiles[yT][xT+1].IsPassable( StaticState, Unknown );
                } catch ( IndexOutOfRangeException ) {
                  e = true;
                }

                bool wedoor = !w && !e;
                bool nsdoor = !n && !so;

                bool passable = StaticState.Tiles[yT][xT].IsPassable( StaticState, Unknown );
                bool beenExplorer = StaticState.Tiles[yT][xT].BeenExplored;

                if ( wedoor || nsdoor ) {
                  if ( beenExplorer ) {
                    float r = 0, g = 0, b = 0;
                    if ( passable ) {
                      r = 0.0f;
                      g = 0.96f;
                      b = 0.0f;
                      DrawCircle( r, g, b, xInc, yInc, viewPortXOrigin + x,
                        viewPortYOrigin + y, ( xInc+yInc )/1.3f, 12 );
                    }
                  }
                }
                x+=xInc;
              }
              y+=yInc;
            }
          }

          // Draw Agent and Goal
          if ( viewXOrigin <= u.X && viewXOrigin + viewXWidth > u.X 
            && viewYOrigin <= u.Y && viewYOrigin + viewYHeight > u.Y ) {
            DrawCircle( 0, 0, 1, xInc, yInc, viewPortXOrigin + ( -viewXOrigin +u.X )*( xInc ),
              viewPortYOrigin + ( viewYHeight + viewYOrigin -1 - u.Y )*( yInc ), AgentGoalSizeRatio*( xInc+yInc )/4, 25 );
          }
          if ( viewXOrigin <= u.DstX && viewXOrigin + viewXWidth > u.DstX 
            && viewYOrigin <= u.DstY && viewYOrigin + viewYHeight > u.DstY ) {
            DrawCircle( 1, 0, 0, xInc, yInc, viewPortXOrigin + ( -viewXOrigin +u.DstX )*( xInc ),
              viewPortYOrigin + ( viewYHeight + viewYOrigin -1 - u.DstY )*( yInc ), AgentGoalSizeRatio*( xInc+yInc )/4, 25 );
          } else if ( viewXOrigin <= u.DstX && viewXOrigin + viewXWidth > u.DstX ) {
            DrawCircle( 1, 0, 0, xInc, yInc, viewPortXOrigin + ( -viewXOrigin +u.DstX )*( xInc ), viewPortYOrigin, 25, 2 );

          } else if ( viewYOrigin <= u.DstY && viewYOrigin + viewYHeight > u.DstY ) {

          }
          //if ( Global ) {
          //  DrawCircle( 0, 0, 1, xInc, yInc, 2f, 2f, AgentGoalSizeRatio*( xInc+yInc )/4, 25 );
          //} else {
          //  DrawCircle( 1, 0, 0, xInc, yInc, 2f, 2f, AgentGoalSizeRatio*( xInc+yInc )/4, 25 );
          //}
        }
      }


#if ScreenShot
      if ( Record && !PauseRecord ) {
        Bitmap bmp = this.TakeScreenshot( );
        bmp.Save( @"Videos/Video"+avi + "/" + avii++ + ".Png",
          System.Drawing.Imaging.ImageFormat.Png );
        bmp.Dispose( );
      }
      if ( TakeScreenShot ) {
        Bitmap bmp = this.TakeScreenshot( );
        bmp.Save( @"Screenshots/screenshot" + System.DateTime.Now.Ticks + 
          ".Png", System.Drawing.Imaging.ImageFormat.Png );
        bmp.Dispose( );
        this.TakeScreenShot = false;
      }
#endif
      Glut.glutSwapBuffers( );
      Gl.glClear( Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT
        | Gl.GL_ACCUM_BUFFER_BIT | Gl.GL_STENCIL_BUFFER_BIT );
    }

    private bool End = false;

    public override void Visualize( GenericGridWorldStaticState StaticState,
      GenericGridWorldDynamicState DyanmicState ) {
      lock ( this ) {
        this.DyanmicState = DyanmicState;
        this.StaticState = StaticState;
      }
#if ScreenShot
      bool first = true;
#endif
      while ( !nextAction && !nextPlanning ) {
#if ScreenShot
        if ( !first ) {
          this.PauseRecord = true;
        }
#endif
        Glut.glutPostRedisplay( );
        Glut.glutMainLoopEvent( );
        Gl.glFinish( );
#if ScreenShot
        if ( !first ) {
          this.PauseRecord = false;
        } else {
          first = false;
        }
#endif
        if ( continuous ) {
          break;
        }
      }
      nextAction = false;
      nextPlanning = false;
    }

    public override void VisualizeAlg( GenericGridWorldStaticState StaticState,
      GenericGridWorldDynamicState DynamicState, object AlgData ) {
      lock ( this ) {
        PriorityQueue<Metric, GenericGridWorldDynamicState> OL = null;
        Dictionary<string, Metric> HAdjusted = null;
        GenericGridWorldDynamicState DS2 = null;
        HashSet<GenericGridWorldDynamicState> List = null;
        OpenGlVisulizationData AD = (OpenGlVisulizationData)AlgData;
        if ( AD.OL != null ) {
          OL = AD.OL;
        } else if ( vd != null && vd.OL != null ) {
          OL = vd.OL;
        }

        if ( AD.List != null ) {
          List = AD.List;
        } else if ( vd != null && vd.List != null ) {
          List = vd.List;
        }

        if ( AD.HAdjusted != null ) {
          HAdjusted = AD.HAdjusted;
        } else if ( vd != null && vd.HAdjusted != null ) {
          HAdjusted = vd.HAdjusted;
        }

        if ( AD.DS2 != null ) {
          DS2 = AD.DS2;
        } else if ( vd != null && vd.DS2 != null ) {
          DS2 = vd.DS2;
        }

        if ( OL != null || DS2 != null || HAdjusted != null || List != null ) {
          if ( this.vd == null ) {
            this.vd = new OpenGlVisulizationData( );
          }
        } else {
          this.vd = (OpenGlVisulizationData)AlgData;
        }

        if ( OL != null ) {
          vd.OL = OL;
        }

        if ( DS2 != null ) {
          vd.DS2 = DS2;
        }

        if ( HAdjusted != null ) {
          vd.HAdjusted = HAdjusted;
        }

        if ( List != null ) {
          vd.List = List;
        }
      }
#if ScreenShot
      bool first = true;
#endif
      while ( !nextAction && !nextPlanning ) {
        if ( continuous && fast_continuous ) {
          break;
        }
#if ScreenShot
        if ( !first ) {
          this.PauseRecord = true;
        }
#endif
        Glut.glutPostRedisplay( );
        Glut.glutMainLoopEvent( );
        Gl.glFinish( );
#if ScreenShot
        if ( !first ) {
          this.PauseRecord = false;
        } else {
          first = false;
        }
#endif
        if ( continuous ) {
          break;
        }
      }
      nextPlanning = false;
    }

    private Metric g( string ds ) {
      Metric r;
      if ( vd.G.TryGetValue( ds, out r ) ) {
        return r;
      } else {
        return Metric.PositiveInfinity;
      }
    }


    private void gValues( ) {
      gMax = Metric.NegativeInfinity;
      gMin = Metric.PositiveInfinity;
      foreach ( var v in vd.G.Values ) {
        if ( v > gMax && !double.IsInfinity( v.ToDouble( ) ) ) {
          gMax = v;
        }
        if ( v < gMin && !double.IsInfinity( v.ToDouble( ) ) ) {
          gMin = v;
        }
      }
    }

    private void hValues( ) {
      hMax = Metric.NegativeInfinity;
      hMin = Metric.PositiveInfinity;
      foreach ( var v in vd.HAdjusted.Values ) {
        if ( v > hMax && !double.IsInfinity( v.ToDouble( ) ) ) {
          hMax = v;
        }
        if ( v < hMin && !double.IsInfinity( v.ToDouble( ) ) ) {
          hMin = v;
        }
      }
    }

    /*private void rhsValues( ) {
      rhsMax = Metric.NegativeInfinity;
      rhsMin = Metric.PositiveInfinity;
      foreach ( var v in vd.RHS.Values ) {
        if ( v > rhsMax && !double.IsInfinity( v.ToDouble( ) ) ) {
          rhsMax = v;
        }
        if ( v < rhsMin && !double.IsInfinity( v.ToDouble( ) ) ) {
          rhsMin = v;
        }
      }
    }*/

    private Metric rhs( string ds ) {
      Metric r;
      if ( vd.RHS.TryGetValue( ds, out r ) ) {
        return r;
      } else {
        return Metric.PositiveInfinity;
      }
    }

    private Metric hAdjusted( string ds ) {
      Metric r;
      if ( vd.HAdjusted.TryGetValue( ds, out r ) ) {
        return r;
      } else {
        return Metric.Zero;
      }
    }

    private void DrawCircle( float r, float g, float b, float xInc, float yInc, float x, float y, float radius, int slices ) {
      Gl.glPushMatrix( );
      Gl.glColor3f( r, g, b );
      Gl.glTranslatef( x+xInc/2, y+yInc/2, 0.0f );
      Glut.glutSolidSphere( radius, slices, 2 );
      Gl.glPopMatrix( );
    }

  }

}
#else
namespace Net.Mokon.Edu.Unh.CS.AI.GridWorld {

  public class OpenGLStateVisualizer :
    StateVisualizer<GenericGridWorldStaticState, GenericGridWorldDynamicState> {
    public override void Visualize( GenericGridWorldStaticState StaticState, GenericGridWorldDynamicState DynamicState ) {
      throw new System.NotImplementedException( );
    }
    public override void VisualizeAlg( GenericGridWorldStaticState StaticState, GenericGridWorldDynamicState DynamicState, object AlgData ) {
      throw new System.NotImplementedException( );
    }
  }

}
#endif

/*
if ( StaticState.Height < 100 ) {
                    Gl.glColor3f( 1, 0, 0 );
                    Gl.glRasterPos2f( x, y+yInc/2 );
                    if ( s.Equals( UTop ) ) {
                      Glut.glutBitmapString( Glut.GLUT_BITMAP_HELVETICA_18, "TOP" );
                    } else {
                      Glut.glutBitmapString( Glut.GLUT_BITMAP_HELVETICA_10, xT + ", " + yT );
                    }
                    Gl.glColor3f( 1, 1, 1 );
                    Gl.glRasterPos2f( x, y );
                    Glut.glutBitmapString( Glut.GLUT_BITMAP_8_BY_13, fr + " " + fg );
                  } else
*/