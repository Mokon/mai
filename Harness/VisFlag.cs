/*
 * This file is part of the MAI framework, distributed under The M License.
 * Copyright © 2008-2010, David Bond, All Rights Resevered
 */
using System;
using Net.Mokon.Edu.Unh.CS.AI.GridWorld;
using Net.Mokon.Utilities.CommandLine;
using Net.Mokon.Edu.Unh.CS.AI.Search;

namespace Net.Mokon.Edu.Unh.CS.AI.Harness {

  /// <summary>
  /// A flag to load a gw visuliser.
  /// </summary>
  public class VisFlag : CommandLineFlag {
    public VisFlag( object FieldHolder, String HelpInfo, String Field,
      params string[] FlagIdentifiers ) :
      base( FieldHolder, HelpInfo, Field, FlagIdentifiers ) {
    }

    public override void Parse( string[] args, ref int index ) {
      if ( FieldInfo.FieldType.Equals( typeof(
        StateVisualizer<GenericGridWorldStaticState,
        GenericGridWorldDynamicState> ) ) ) {
        ++index;
        if ( args[index].Equals( "animate" ) ) {
          FieldInfo.SetValue( FieldHolder, new OpenGLStateVisualizer( ) );
        } else if ( args[index].Equals( "xml" ) ) {
          throw new NotSupportedException( );
        } else if ( args[index].Equals( "sonic" ) ) {
          FieldInfo.SetValue( FieldHolder, new SonicOpenGLStateVisualizer( ) );
        } else {
          throw new CommandLineException( "Unknown Visualizer" );
        }
      } else {
        throw new CommandLineException( "Invalid Field Type" );
      }
    }
  }

}
