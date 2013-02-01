/*
 * This file is part of the Mokon.Net Utilties Package,
 * distributed under The M License.
 * Copyright © 2008-2010, David Bond, All Rights Resevered
 */
using System;
using System.Collections.Generic;
using System.IO;

namespace Net.Mokon.Utilities {

  /// <summary>
  /// A static class for file maniplualtion functions.
  /// </summary>
  public static class Files {

    /// <summary>
    /// A utility method to get files recursively given a dir.
    /// </summary>
    /// <param name="rootDir"></param>
    /// <returns></returns>
    public static List<string> GetFilesRecursive( String rootDir ) {
      List<string> files = new List<string>( );
      if ( File.Exists( rootDir ) ) {
        files.Add( rootDir );
        return files;
      }
      Stack<string> dirs = new Stack<string>( );
      dirs.Push( rootDir );
      while ( dirs.Count > 0 ) {
        string dir = dirs.Pop( );
        files.AddRange( Directory.GetFiles( dir, "*.*" ) );
        foreach ( string dn in Directory.GetDirectories( dir ) ) {
          dirs.Push( dn );
        }
      }
      return files;
    }

  }

}
