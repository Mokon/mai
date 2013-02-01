/*
 * This file is part of the MAI framework, distributed under The M License.
 * Copyright © 2008-2010, David Bond, All Rights Resevered
 */
using System;
using Net.Mokon.Utilities;
using System.Threading;
using System.Collections.Generic;
using Net.Mokon.Edu.Unh.CS.AI.Search.Goals;
using Net.Mokon.Edu.Unh.CS.AI.Search.Heuristics;
using Net.Mokon.Edu.Unh.CS.AI.GridWorld;
using Net.Mokon.Edu.Unh.CS.AI.Harness;

#if OpenGl
using Tao.FreeGlut;
using Tao.OpenGl;

namespace Net.Mokon.Edu.Unh.CS.AI.Search.Algorithms {

  public class Human : Algorithm<GenericGridWorldStaticState, GenericGridWorldDynamicState> {

    //private string Name;

    private void keyboard_special( int key, int x, int y ) {
      switch ( key ) {
        case (char)Glut.GLUT_KEY_RIGHT:
          Operators.Enqueue( new MoveEast( 0 ) );
          break;
        case (char)Glut.GLUT_KEY_LEFT:
          Operators.Enqueue( new MoveWest( 0 ) );
          break;
        case (char)Glut.GLUT_KEY_UP:
          Operators.Enqueue( new MoveNorth( 0 ) );
          break;
        case (char)Glut.GLUT_KEY_DOWN:
          Operators.Enqueue( new MoveSouth( 0 ) );
          break;
      }
    }
    Queue<GridWorldOperator> Operators;

    private void keyboard( byte key, int x, int y ) {
      switch ( (char)key ) {
        case 'D':
          Operators.Enqueue( new MoveEast( 0 ) );
          break;
        case 'A':
          Operators.Enqueue( new MoveWest( 0 ) );
          break;
        case 'W':
          Operators.Enqueue( new MoveNorth( 0 ) );
          break;
        case 'X':
          Operators.Enqueue( new MoveSouth( 0 ) );
          break;
        case 'Q':
          Operators.Enqueue( new MoveNorthWest( 0 ) );
          break;
        case 'E':
          Operators.Enqueue( new MoveNorthEast( 0 ) );
          break;
        case 'Z':
          Operators.Enqueue( new MoveSouthWest( 0 ) );
          break;
        case 'C':
          Operators.Enqueue( new MoveSouthEast( 0 ) );
          break;
      }
    }


    private OpenGLStateVisualizer sv;
    public Human( OpenGLStateVisualizer sv ) {
      Operators = new Queue<GridWorldOperator>( );
      if ( sv != null ) {
        this.sv = sv;
        sv.kbh += new StateVisualizer<GenericGridWorldStaticState,
          GenericGridWorldDynamicState>.KeyBoardHandler( this.keyboard );
        sv.kbsh += new StateVisualizer<GenericGridWorldStaticState,
          GenericGridWorldDynamicState>.KeyBoardSpecialHandler( this.keyboard_special );
      }
      //Console.Write( "What is your name?: " );
      //Name = Console.ReadLine( );
    }

    public override IEnumerable<Path<GenericGridWorldStaticState,
        GenericGridWorldDynamicState>> Compute(
          GenericGridWorldStaticState StaticState,
          GenericGridWorldDynamicState DynamicState,
          Goal<GenericGridWorldStaticState, GenericGridWorldDynamicState> Goal,
          Operator<GenericGridWorldStaticState,
            GenericGridWorldDynamicState>[] Actions ) {
      while ( true ) {
        while ( Operators.Count == 0 ) {
          Thread.Sleep( 100 );
          if ( this.sv != null ) {
            sv.VisualizeAlg( StaticState, StaticState.InitialDynamicState, new OpenGLStateVisualizer.OpenGlVisulizationData( ) );
            Glut.glutPostRedisplay( );
            Glut.glutMainLoopEvent( );
            Gl.glFinish( );
          }
        }
        var op = Operators.Dequeue( );
        System.Console.WriteLine( "Got Op --> {0}", op );
        if ( op.IsValidOn( StaticState.InitialDynamicState, StaticState, false ) ) {
          System.Console.WriteLine( "Trying Op --> {0}", op );
          yield return GridWorldOperator.MakePath( op );
        }
      }
    }

  }

}
#endif