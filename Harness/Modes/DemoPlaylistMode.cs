/*
 * This file is part of the MAI framework, distributed under The M License.
 * Copyright © 2008-2010, David Bond, All Rights Resevered
 */
using System;
using System.Collections.Generic;
using System.IO;
using Net.Mokon.Edu.Unh.CS.AI.GridWorld;
using Net.Mokon.Utilities;
using Net.Mokon.Utilities.CommandLine;
using System.Linq;

namespace Net.Mokon.Edu.Unh.CS.AI.Harness {

  /// <summary>
  /// A mode for demoing an algorithm.
  /// </summary>
  public class DemoPlaylistMode : Mode {

    public override string GetHelp( ) {
      return "Runs demo mode in in infinite loop on a playlist until canceled.";
    }

    public DemoPlaylistMode( ) {
      base.CLH = new CommandLineHandler( );
    }

    private class PlaylistEntry {
      public string File;
      public string Opl;
    }

    /// <summary>
    /// Runs the Mode.
    /// </summary>
    /// <param name="args"></param>
    public override void Run( string[] args ) {

      DemoMode dm = new DemoMode( );

      dm.ParseArgs( args );
      List<Results> Resultss = new List<Results>( );
      List<PlaylistEntry> Playlist = new List<PlaylistEntry>( );
      if ( Directory.Exists( dm.file ) || dm.file.EndsWith( ".xml" ) || dm.file.EndsWith( ".gwmap" ) ) {
        var Files = Net.Mokon.Utilities.Files.GetFilesRecursive( dm.file ).Where(
           file => ( file.EndsWith( ".gwmap" ) || file.EndsWith( ".xml" ) ) 
           && !file.Contains( "KEY" ) ) ;
        foreach ( string file in Files ) {
          Playlist.Add( new PlaylistEntry( ) {
            File = file, Opl = "100000"
          } );
        }
      } else {
        StreamReader pl = new StreamReader( new FileStream(
         dm.file, FileMode.Open, FileAccess.Read ) );
        string pll;

        while ( ( pll = pl.ReadLine( ) ) != null ) {
          var ppls =  pll.Split( );
          if ( ppls.Length == 2 ) {
            Playlist.Add( new PlaylistEntry( ) {
              File = "../../" + ppls[0], Opl = ppls[1]
            } );
          } else if ( ppls.Length == 1 ) {
            Playlist.Add( new PlaylistEntry( ) {
              File = "../../" +  ppls[0], Opl = null
            } );
          } else {
            break;
          }
        }
        pl.Close( );
      }


      while ( Playlist.Count != 0 ) {
        foreach ( PlaylistEntry ple in Playlist ) {
          dm.SetFilename( "" + ple.File );
          if ( ple.Opl == null ) {
            dm.Solve = true;
          } else {
            dm.Solve = false;
            dm.OptimalPathLenth = new Metric( double.Parse( ple.Opl ) );
          }
          dm.Run( args );
          Resultss.Add( dm.Results );
          dm.Resultss = Resultss;
        }
      }


    }

    /// <summary>
    /// Gets the name of the mode.
    /// </summary>
    /// <returns></returns>
    public override string Name( ) {
      return "demo-playlist";
    }

  }

}
