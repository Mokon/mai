/*
 * This file is part of the MAI framework, distributed under The M License.
 * Copyright © 2008-2010, David Bond, All Rights Resevered
 */
using System.Collections.Generic;
using System.IO;
using Net.Mokon.Utilities;
using Net.Mokon.Edu.Unh.CS.AI.Search.Utility;

namespace Net.Mokon.Edu.Unh.CS.AI.Harness {

  /// <summary>
  /// A mode for converting HOG format to gwmap format.
  /// </summary>
  public class ConverterMode : Mode {

    public override string GetHelp( ) {
      return "Converts a HOG formated file to the GWMAP MAI format.";
    }

    /// <summary>
    /// Runs the Mode.
    /// </summary>
    /// <param name="args"></param>
    public override void Run( string[] args ) {
      List<string> files = Files.GetFilesRecursive( args[1] );
      foreach ( var file in files ) {
        if ( !file.EndsWith( ".gwmap" ) ) {
          TextWriter fOutMap;
          if ( File.Exists( file + ".gwmap" ) ) {
            fOutMap = new StreamWriter( new FileStream( file +
            ".gwmap", FileMode.Truncate, FileAccess.Write ) );
          } else {
            fOutMap = new StreamWriter( new FileStream( file +
          ".gwmap", FileMode.CreateNew, FileAccess.Write ) );
          }
          var s = new StreamReader( new FileStream( file, FileMode.Open, FileAccess.Read ) );
          try {
            fOutMap.Write( MapConverter.Convert( s ) );
            System.Console.WriteLine( "\tWriting... (" + file + ")" );
          } catch {
          }
          fOutMap.Close( );
          s.Close( );
        }
      }
    }

    /// <summary>
    /// Gets the name of the mode.
    /// </summary>
    /// <returns></returns>
    public override string Name( ) {
      return "converter";
    }

  }

}
