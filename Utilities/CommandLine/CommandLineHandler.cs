/*
 * This file is part of the Mokon.Net Utilties Package,
 * distributed under The M License.
 * Copyright © 2008-2010, David Bond, All Rights Resevered
 */
using System;
using System.Collections.Generic;
using System.Linq;

namespace Net.Mokon.Utilities.CommandLine {

  /// <summary>
  /// A class for parsing the command line.
  /// </summary>
  public class CommandLineHandler {
    public CommandLineFlag[] Flags {
      get;
      private set;
    }
    public CommandLineHandler( params CommandLineFlag[] Flags ) {
      this.Flags = Flags;
      foreach ( CommandLineFlag Flag in Flags ) {
        Flag.Verify( this );
      }
    }

    public void AddFlag( CommandLineFlag Flag ) {
      List<CommandLineFlag> FlagsNew = new List<CommandLineFlag>( this.Flags );
      FlagsNew.Add( Flag );
      this.Flags = FlagsNew.ToArray( );
      Flag.Verify( this );
    }

    public void AddFlags( params CommandLineFlag[] Flags ) {
      List<CommandLineFlag> FlagsNew = new List<CommandLineFlag>( this.Flags );
      foreach ( CommandLineFlag Flag in Flags ) {
        FlagsNew.Add( Flag );
      }
      this.Flags = FlagsNew.ToArray( );
      foreach ( CommandLineFlag Flag in Flags ) {
        Flag.Verify( this );
      }
    }

    public int NumMatches( string Flag ) {
      int NumMatches =  0;
      foreach ( CommandLineFlag F in Flags ) {
        if ( F.IsMatch( Flag ) ) {
          NumMatches++;
        }
      }
      return NumMatches;
    }

    public void Parse( string[] args ) {
      for ( int i = 1 ; i < args.Length ; i++ ) {
        if ( args[i].StartsWith( "-" ) ) {
          string Flag = args[i].TrimStart( '-' );
          bool Match = false;
          try {
            foreach ( CommandLineFlag F in Flags ) {
              Match = F.IsMatch( Flag );
              if ( Match ) {
                try {
                  F.Parse( args, ref i );
                  break;
                } catch ( System.MissingFieldException e ) {
                  System.Console.Error.WriteLine(
                    "Mising Field ( Field = {0} ) ( Message = {1})",
                    F.FlagIdentifiers.First(), e.Message );
                }
                
              }
            }
            if ( !Match ) {
              throw new CommandLineException( "Unknown Flag [" + Flag + "]" );
            }
          } catch ( Exception e ) {
            throw new CommandLineException(
              "Flag Parse Exception [" + e.Message + "]", e );
          }
        }
      }

    }
  }

}
