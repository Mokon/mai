/*
 * This file is part of the Mokon.Net Utilties Package,
 * distributed under The M License.
 * Copyright © 2008-2010, David Bond, All Rights Resevered
 */
using System;

namespace Net.Mokon.Utilities.CommandLine {

  /// <summary>
  /// An exception class for command line errors.
  /// </summary>
  public class CommandLineException : Exception {
    public CommandLineException( string msg, Exception e )
      : base( msg, e ) {
    }
    public CommandLineException( string msg )
      : base( msg ) {
    }
  }

}
