/*
 * This file is part of the Mokon.Net Utilties Package,
 * distributed under The M License.
 * Copyright © 2008-2010, David Bond, All Rights Resevered
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Net.Mokon.Utilities.CommandLine {

  /// <summary>
  /// A command line flag class.
  /// </summary>
  public class CommandLineFlag {
    protected readonly object FieldHolder;
    public HashSet<string> FlagIdentifiers {
      get;
      private set;
    }
    protected readonly FieldInfo FieldInfo;
    private String HelpInfo;
    public CommandLineFlag( object FieldHolder, String HelpInfo, String Field,
      params string[] FlagIdentifiers ) {
      this.FieldHolder = FieldHolder;
      this.HelpInfo = HelpInfo;
      this.FieldInfo = FieldHolder.GetType( ).GetField( Field,
        BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public );
      if ( FlagIdentifiers.Length == 0 ) {
        throw new ArgumentException( "No Flag Identifiers Given" );
      }
      this.FlagIdentifiers = new HashSet<string>( FlagIdentifiers );
    }

    public virtual void Parse( string[] args, ref int index ) {
      if ( FieldInfo.FieldType.Equals( typeof( int ) ) ) {
        FieldInfo.SetValue( FieldHolder, int.Parse( args[++index] ) );
      } else if ( FieldInfo.FieldType.Equals( typeof( long ) ) ) {
        FieldInfo.SetValue( FieldHolder, long.Parse( args[++index] ) );
      } else if ( FieldInfo.FieldType.Equals( typeof( bool ) ) ) {
        FieldInfo.SetValue( FieldHolder, bool.Parse( args[++index] ) );
      } else if ( FieldInfo.FieldType.Equals( typeof( double ) ) ) {
        FieldInfo.SetValue( FieldHolder, double.Parse( args[++index] ) );
      } else if ( FieldInfo.FieldType.Equals( typeof( string ) ) ) {
        FieldInfo.SetValue( FieldHolder, args[++index] );
      } else if ( FieldInfo.FieldType.Equals( typeof( TextWriter ) ) ) {
        FieldInfo.SetValue( FieldHolder, new StreamWriter(
          new FileStream( args[++index], FileMode.Create, FileAccess.Write ) ) );
      } else if ( FieldInfo.FieldType.Equals( typeof( TextReader ) ) ) {
        FieldInfo.SetValue( FieldHolder, new StreamReader(
          new FileStream( args[++index], FileMode.Open, FileAccess.Read ) ) );
      } else {
        throw new CommandLineException( "Unsupported Field Type [" + FieldInfo.FieldType.Name + "]" );
      }
    }
    public bool IsMatch( string arg ) {
      return FlagIdentifiers.Contains( arg );
    }

    public void Verify( CommandLineHandler CommandLineHandler ) {
      foreach ( string Flag in this.FlagIdentifiers ) {
        if ( CommandLineHandler.NumMatches( Flag ) != 1 ) {
          throw new CommandLineException( "Multiple flags with same Idenifier." );
        }
      }
    }

    public string GetHelp( ) {
      return this.HelpInfo;
    }
  }

  public class AppendAutoFlushTextWriterFlag : CommandLineFlag {
    public AppendAutoFlushTextWriterFlag( object FieldHolder, String HelpInfo, String Field,
      params string[] FlagIdentifiers ) :
      base( FieldHolder, HelpInfo, Field, FlagIdentifiers ) {
    }

    public override void Parse( string[] args, ref int index ) {
      if ( FieldInfo.FieldType.Equals( typeof( TextWriter ) ) ) {
        StreamWriter OutSW  = new StreamWriter(
          new FileStream( args[++index], FileMode.Append, FileAccess.Write ) );
        OutSW.AutoFlush = true;
        FieldInfo.SetValue( FieldHolder, OutSW );
      } else {
        throw new CommandLineException( "Invalid Field Type" );
      }
    }
  }

  public class TruncOrCreateNewAutoFlushTextWriterFlag : CommandLineFlag {
    public TruncOrCreateNewAutoFlushTextWriterFlag( object FieldHolder,
      String HelpInfo, String Field,
      params string[] FlagIdentifiers ) :
      base( FieldHolder, HelpInfo, Field, FlagIdentifiers ) {
    }

    public override void Parse( string[] args, ref int index ) {
      if ( FieldInfo.FieldType.Equals( typeof( TextWriter ) ) ) {
        index++ ;
        StreamWriter OutSW ;
              if ( File.Exists( args[index] ) ) {
                OutSW = new StreamWriter(
                  new FileStream( args[index], FileMode.Truncate, FileAccess.Write ) );
              } else {
                OutSW = new StreamWriter(
                  new FileStream( args[index], FileMode.CreateNew, FileAccess.Write ) );
              }
              OutSW.AutoFlush = true;
        FieldInfo.SetValue( FieldHolder, OutSW );
      } else {
        throw new CommandLineException( "Invalid Field Type" );
      }
    }
  }

}
