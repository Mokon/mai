/*
 * This file is part of the MAI framework, distributed under The M License.
 * Copyright © 2008-2010, David Bond, All Rights Resevered
 */
using System.IO;
using System.Linq;
using Net.Mokon.Utilities.CommandLine;

namespace Net.Mokon.Edu.Unh.CS.AI.Harness {

  public class HelpMode : Mode {

    public override string GetHelp( ) {
      return "Display help information about the MAI framework.";
    }

    public HelpMode( ) {
      base.CLH = new CommandLineHandler( );
    }

    public Mode[] Modes {
      get;
      set;
    }

    /// <summary>
    /// Takes a playlist and scales it by the given width and height.
    /// </summary>
    public override void Run( string[] args ) {
      if ( args.Length <= 1 ) { 
        System.Console.WriteLine( "Supported Modes are:" );
        foreach ( Mode m in Modes ) {
          System.Console.WriteLine( "\t{0}: {1}", m.Name( ), m.GetHelp( ) );
        }
      } else if ( args[1].Equals( "all" ) ) {
        foreach ( Mode M in Modes ) {
          PrintModeHelp( M );
        }
      } else {
        var M = Modes.FirstOrDefault( x => args[1].Equals( x.Name( ) ) );
        if ( M == null ) {
          System.Console.WriteLine( "Invalid Mode" );
          System.Console.WriteLine( "Supported Modes are:" );
          foreach ( Mode m in Modes ) {
            System.Console.WriteLine( "\t" + m.Name( ) );
          }
        } else {
          PrintModeHelp( M );
        }
      }


    }

    private void PrintModeHelp( Mode M ) {
      System.Console.WriteLine( "Help for Mode {0}", M.Name() );
      System.Console.WriteLine( "\t{0}: {1}", M.Name( ), M.GetHelp( ) );
      System.Console.WriteLine( SearchHarness.Divider.Replace('-', '.') );
      System.Console.WriteLine( "Command Line Flags for Mode {0}", M.Name( ) );
      if ( M.CLH == null ) {
        System.Console.WriteLine( "\t{0} does not use a command line handler.", M.Name( ) );
      } else {
        foreach ( CommandLineFlag f in M.CLH.Flags ) {
          string[] FlagIdentifiers = f.FlagIdentifiers.ToArray( );
          System.Console.Write( "\t" );
          foreach ( string fid in FlagIdentifiers ) {
            System.Console.Write( "{0}, ", fid );
          }
          System.Console.WriteLine( );
          System.Console.WriteLine( "\t\t{0}", f.GetHelp( ) );
        }
      }
      System.Console.WriteLine( SearchHarness.Divider );
    }

    /// <summary>
    /// Gets the name of the mode.
    /// </summary>
    /// <returns></returns>
    public override string Name( ) {
      return "help";
    }

  }

}
