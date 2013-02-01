/*
 * This file is part of the MAI framework, distributed under The M License.
 * Copyright © 2008-2010, David Bond, All Rights Resevered
 */
using Net.Mokon.Edu.Unh.CS.AI.GridWorld;
using Net.Mokon.Edu.Unh.CS.AI.Search;
using Net.Mokon.Utilities;


#if OpenGl
using Tao.FreeGlut;
using Tao.OpenGl;

namespace Net.Mokon.Edu.Unh.CS.AI.Harness {

  /// <summary>
  /// A faster, simple visulizer.
  /// </summary>
  public class SonicOpenGLStateVisualizer :
    StateVisualizer<GenericGridWorldStaticState, GenericGridWorldDynamicState> {

    private GenericGridWorldStaticState StaticState;

    private GenericGridWorldDynamicState DyanmicState;

    private const float winWid = 100.0f, winHeight = 100.0f;

    public SonicOpenGLStateVisualizer( ) {
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
      Glut.glutKeyboardFunc( new Glut.KeyboardCallback( keyboard ) );
      //new Thread( new ThreadStart( Glut.glutMainLoop ) ).Start( );
      Gl.glEnable( Gl.GL_BLEND );
      Gl.glBlendFunc( Gl.GL_SRC_ALPHA, Gl.GL_ONE_MINUS_SRC_ALPHA );


    }

    private bool text = false;
    private bool nextAction = true;
    private bool nextPlanning = true;
    private bool continuous = false;
    private bool fast_continuous = false;
    private void keyboard( byte key, int x, int y ) {
      System.Console.WriteLine( " Key Pressed : {0}", key );
      switch ( (char)key ) {
        case 'c':
          continuous = !continuous;
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
        case 't':
          text = !text;
          break;
        case (char)27:
          System.Environment.Exit( 0 );
          break;
      }
    }
    
    private OpenGLStateVisualizer.OpenGlVisulizationData vd;

    /// <summary>
    /// This function draws the screen.
    /// </summary>
    public void redraw( ) {
      lock ( this ) {
        if ( StaticState != null && DyanmicState != null ) {
          Unit u = DyanmicState.FindUnit( 0 );
          float x = 0.0f, y = 0.0f,
            xInc = winWid/StaticState.Width,
            yInc = winHeight/StaticState.Height;
          string dss = text && vd.DS != null ? vd.DS.ToString( ) : null;

          // Go through all tiles
          for ( int yT = (int)StaticState.Height - 1 ; yT >= 0 ; yT-- ) {
            x= 0.0f;
            for ( int xT = 0 ; xT < StaticState.Width ; xT++ ) {
              string s = "<X: " + xT + ", Y: " + yT + " >";
              bool close = Distance.OctileDistance( xT, u.X, yT, u.Y ) <= u.LOS;

              float r, g, b, a;
              r = g = b = a = 0;
              a = 1f;
              if ( close ) {
                r = 0.5f;
              }
              bool passable = StaticState.Tiles[yT][xT].IsPassable( null, true );
              if ( passable ) {
                g = 1;
              }
              if ( text && vd != null && vd.RHS != null && vd.G != null ) {
                Metric fr = rhs( s );
                Metric fg = this.g( s );
                if ( fr != fg ) {
                  if ( double.IsInfinity( fr.ToDouble()  ) || double.IsInfinity( fg.ToDouble() ) ) {
                    g = 0.7f;
                  } else {
                    g = 0.5f;
                  }
                }
              }
              
              if ( dss != null && dss.Equals( s ) ) {
                r = 1;
                g = 0;
                b = 0;
              }

              if ( !StaticState.Tiles[yT][xT].BeenExplored ) {
                b = 0.5f;
              }
              Gl.glColor4f( r, g, b, a );
              Gl.glPushMatrix( );
              Gl.glTranslatef( x, y, 0.0f );
              Gl.glRectf( 0, 0, xInc, yInc );
              Gl.glPopMatrix( );
              x+=xInc;
            }
            y+=yInc;
          }

          Gl.glPushMatrix( );
          Gl.glColor3f( 0, 0, 1.0f );

          Gl.glTranslatef( u.X*( winWid/StaticState.Width ), winHeight-( u.Y+1 )*( winHeight/StaticState.Height ), 0.0f );
          Gl.glRectf( 0, 0, xInc, yInc );
          Gl.glPopMatrix( );
          Gl.glPushMatrix( );
          Gl.glColor3f( 1.0f, 0, 0 );
          Gl.glTranslatef( u.DstX*( winWid/StaticState.Width ), winHeight-( u.DstY+1 )*( winHeight/StaticState.Height ), 0.0f );
          Gl.glRectf( 0, 0, xInc, yInc );
          Gl.glPopMatrix( );
        }
      }
      Glut.glutSwapBuffers( );
    }

    public override void Visualize( GenericGridWorldStaticState StaticState,
      GenericGridWorldDynamicState DyanmicState ) {
      lock ( this ) {
        this.DyanmicState = DyanmicState;
        this.StaticState = StaticState;
      }
      while ( !nextAction && !nextPlanning ) {
        Glut.glutPostRedisplay( );
        Glut.glutMainLoopEvent( );
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
        if ( this.vd != null ) {
          OL = vd.OL;
        }
        this.vd = (OpenGLStateVisualizer.OpenGlVisulizationData)AlgData;
        if ( this.vd != null && this.vd.OL == null ) {
          vd.OL = OL;
        }
      }
      while ( !nextAction && !nextPlanning ) {
        if ( continuous && fast_continuous ) {
          break;
        }
        Glut.glutPostRedisplay( );
        Glut.glutMainLoopEvent( );
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


    private Metric rhs( string ds ) {
      Metric r;
      if ( vd.RHS.TryGetValue( ds, out r ) ) {
        return r;
      } else {
        return Metric.PositiveInfinity;
      }
    }

  }

}
#else
namespace Net.Mokon.Edu.Unh.CS.AI.GridWorld {

  public class SonicOpenGLStateVisualizer :
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