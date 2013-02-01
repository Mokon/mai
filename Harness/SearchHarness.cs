/*
 * This file is part of the MAI framework, distributed under The M License.
 * Copyright © 2008-2010, David Bond, All Rights Resevered
 */
using System;
using System.Linq;

namespace Net.Mokon.Edu.Unh.CS.AI.Harness {

  /// <summary>
  /// A harness for testing real time search.
  /// </summary>
  public static class SearchHarness {

    /// <summary>
    /// The main method which parses out the modes to run.
    /// </summary>
    /// <param name="args"></param>
    public static void Main( string[] args ) {
      var HelpMode = new HelpMode( );
      HelpMode.Modes = new Mode[] {
        new DemoMode( ),
        new BatchMode( ),
        new OrganizeMode( ),
        new MakePlayListMode( ),
        new BatchSingleMode( ),
        new BatchModeThreaded( ),
        new DemoPlaylistMode( ),
        new MakeRandomMaps( ),
        new MakeScale( ),
        new MakeChokepointMaps( ),
        new MakeRoomsMaps( ),
        new TimeTestMode( ),
        HelpMode,
      };
      bool Err = false;
      System.Console.WriteLine(
        "Welcome to MAI, a framework for AI development by David Bond." );
      System.Console.WriteLine( "Distributed under The M License." );
      System.Console.WriteLine(
        "Copyright © 2008-2010, David Bond, All Rights Resevered" );
      System.Console.WriteLine( SearchHarness.Divider );
      try {
        if ( args.Length >= 1 ) {
          var M = HelpMode.Modes.FirstOrDefault( x => args[0].Equals( x.Name( ) ) );
          if ( M == null ) {
            System.Console.WriteLine( "Invalid Mode" );
            Err = true;
          } else {
            M.Run( args );
          }
        } else {
          System.Console.WriteLine( "No Mode Given" );
          Err = true;
        }
        if ( Err ) {
          System.Console.WriteLine( "Supported Modes are:" );
          foreach ( Mode m in HelpMode.Modes ) {
            System.Console.WriteLine( "\t" + m.Name( ) );
          }
          System.Console.WriteLine( SearchHarness.Divider );
        } else {
          System.Console.WriteLine( SearchHarness.Divider );
          System.Console.WriteLine( "Mode [{0}] Run Completed Successfully", args[0] );
          System.Console.WriteLine( SearchHarness.Divider );
        }
      } catch ( Exception e ) {
        System.Console.WriteLine( SearchHarness.Divider );
        System.Console.WriteLine( "Mode [{0}] Run Error Detected", args[0] );
        System.Console.WriteLine( "The Error was [{0}]", e.Message );
        System.Console.WriteLine( "Printing a stack trace" );
        System.Console.WriteLine( SearchHarness.Divider );
        System.Console.WriteLine( e );
        System.Console.WriteLine( SearchHarness.Divider );
      }
    }

    internal static readonly string Divider =
      "----------------------------------------------------------------------";

  }

}
