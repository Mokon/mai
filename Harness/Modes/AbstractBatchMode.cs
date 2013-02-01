/*
 * This file is part of the MAI framework, distributed under The M License.
 * Copyright © 2008-2010, David Bond, All Rights Resevered
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Net.Mokon.Utilities;
using Net.Mokon.Utilities.CommandLine;
using Net.Mokon.Edu.Unh.CS.AI.GridWorld;
using Net.Mokon.Edu.Unh.CS.AI.Search;
using Net.Mokon.Edu.Unh.CS.AI.Search.Algorithms;

namespace Net.Mokon.Edu.Unh.CS.AI.Harness {

  /// <summary>
  /// Batch mode for running lots of algorithms at once quickly.
  /// </summary>
  public abstract class AbstractBatchMode : Mode {

    protected GridWorldLoader<GenericGridWorldStaticState,
      GenericGridWorldDynamicState> Loader = new GenericGridWorldLoader( );
    protected int computationLimitMax = 1001;
    protected int computationLimitMin = 1;
    protected int computationLimitIncrement = 10;
    protected Func<int, int, int> computationLimitMethod;
    protected double computationLimitCeilingRatio = int.MaxValue;
    protected double Weight = 1.0d;
    protected int IterationCountLimit = -1;
    protected string file= "./game";
    protected TextWriter Out = System.Console.Out;
    protected int LOS = 7;
    protected IEnumerable<string> Algorithms =
      new string[] { "TBA*", "BRTA*", "BRTwA*", "BRTA*Sorting" };
    protected int maxThreads = 0;
    protected bool Dynamic = false;
    protected int Seed = 0;
    protected double MinPathLength = 0;
    protected double MaxPathLength = double.MaxValue;
    protected int AStarMinExpansion = 0;
    protected int AStarMaxExpansion = int.MaxValue;
    protected bool Shuffle = true;

    protected static bool IsRealTime( String Alg ) {
      return new GenericAlgorithmFactory<GenericGridWorldStaticState,
        GenericGridWorldDynamicState>( ).SetRealTimeData( null, null, null,
        Metric.Zero, 0 ).GetAlgorithm( Alg, null, 0 ).IsRealTime( );
    }

    public AbstractBatchMode( ) {
      computationLimitMethod = 
      ( computationLimit, clindex ) =>
       computationLimit + computationLimitIncrement;
      base.CLH = new CommandLineHandler(
        new GridWorldLoaderFlag( this, "", "Loader", "l", "loader" ),
         new AlgsFlag( this, "The algorithms this batch mode should run with.", "Algorithms", "a", "algorithms" ),
         new CommandLineFlag( this, "The seed for dynamic maps.", "Seed", "seed" ),
         new CommandLineFlag( this, "The max computation limit.", "computationLimitMax", "clm", "computationLimitMax" ),
         new CLMethodFlag( this, "The method for increasing the computation limit.", "computationLimitMethod", "clmethod", "computationLimitMethod" ),
         new CommandLineFlag( this, "The min computatation limit.", "computationLimitMin", "clmin", "computationLimitMin" ),
         new CommandLineFlag( this, "The weight for algorithms that use a weight.", "Weight", "w", "weight" ),
         new CommandLineFlag( this, "The maximum number of iterations to do for this run.", "IterationCountLimit", "icl", "iterationCountLimit" ),
         new CommandLineFlag( this, "Whether or not to shuffle the iteration order.", "Shuffle", "s", "shuffle" ),
         new CommandLineFlag( this, "The max number of threads to use.", "maxThreads", "mt", "maxThreads" ),
         new CommandLineFlag( this, "The min A* path length.", "MinPathLength", "minpl", "minPathLength" ),
         new CommandLineFlag( this, "The max A* path length.", "MaxPathLength", "maxpl", "maxPathLength" ),
         new CommandLineFlag( this, "The min A* number of expansions.", "AStarMinExpansion", "minexp", "minExpansions" ),
         new CommandLineFlag( this, "The max A* number of expansions.", "AStarMaxExpansion", "maxexp", "maxExpansions" ),
         new CommandLineFlag( this, "Whether to change the world between steps.", "Dynamic", "dyn", "dynamic" ),
         new CommandLineFlag( this, "The LOS of the unit path finding.", "LOS", "LOS", "lineOfSight" ),
         new CommandLineFlag( this, "The computation limit increment.", "computationLimitIncrement", "cli", "computationLimitIncrement" ),
         new CommandLineFlag( this, "The computation limit ratio to exclude paths over a certain cost.",
           "computationLimitCeilingRatio", "clcr", "computationLimitCeilingRatio" ),
         new CommandLineFlag( this, "The base input file.", "file", "i", "input" )
         );
    }

  }
  public class AlgsFlag : CommandLineFlag {
    public AlgsFlag( object FieldHolder, String HelpInfo, String Field,
      params string[] FlagIdentifiers ) :
      base( FieldHolder, HelpInfo, Field, FlagIdentifiers ) {
    }

    public override void Parse( string[] args, ref int index ) {
      if ( FieldInfo.FieldType.Equals( typeof( IEnumerable<string> ) ) ) {
        int num = int.Parse( args[++index] );
        FieldInfo.SetValue( FieldHolder, args.Skip( index + 1 ).Take( num ).ToArray( ) );
        index += num;
      } else {
        throw new CommandLineException( "Invalid Field Type" );
      }
    }
  }

  public class CLMethodFlag : CommandLineFlag {
    public CLMethodFlag( object FieldHolder, String HelpInfo, String Field,
      params string[] FlagIdentifiers ) :
      base( FieldHolder, HelpInfo, Field, FlagIdentifiers ) {
    }

    public override void Parse( string[] args, ref int index ) {
      if ( FieldInfo.FieldType.Equals( typeof( Func<int, int, int> ) ) ) {
        string method = args[++index];
        Func<int, int, int> computationLimitMethod;
        if ( method.Equals( "?" ) ) {
          computationLimitMethod = ( computationLimit, clindex ) =>
            computationLimit == 1 ? 2 : computationLimit*computationLimit;
        } else if ( method.Equals( "inc" ) ) {
          computationLimitMethod = ( computationLimit, clindex ) =>
            computationLimit + ( (int)FieldHolder.GetType( ).GetField( "computationLimitIncrement" ).GetValue( FieldHolder ) );
        } else if ( method.Equals( "arr" ) ) {
          int[] arr = new int[] { 4, 8, 16, 32, 64, 128, 256, 512, 10000 };


          computationLimitMethod = ( computationLimit, clindex ) =>
            arr[clindex];
        } else if ( method.Equals( "org-arr" ) ) {
          //int[] arr = new int[] { 1, 5, 10, 20, 30, 40, 50, 100, 250, 500, 10000 };
          int[] arr = new int[] { 1, 2, 3, 5, 7, 10, 15, 20, 25, 30, 35, 40, 45, 50, 100, 250, 500, 1000, 10000 };
          computationLimitMethod = ( computationLimit, clindex ) =>
            arr[clindex];
        } else if ( method.StartsWith( "exp-" ) ) {
          double z = double.Parse( method.Substring( 4 ) );
          computationLimitMethod = ( computationLimit, clindex ) =>
            (int)Math.Pow( z, clindex );
        } else {
          throw new CommandLineException( "Unknown Computation Limit Method" );
        }
        FieldInfo.SetValue( FieldHolder, computationLimitMethod );
      } else {
        throw new CommandLineException( "Invalid Field Type" );
      }
    }

  }
  public class GridWorldLoaderFlag : CommandLineFlag {
    public GridWorldLoaderFlag( object FieldHolder, String HelpInfo, String Field,
      params string[] FlagIdentifiers ) :
      base( FieldHolder, HelpInfo, Field, FlagIdentifiers ) {
    }

    public override void Parse( string[] args, ref int index ) {
      if ( FieldInfo.FieldType.Equals( typeof( GridWorldLoader<GenericGridWorldStaticState, GenericGridWorldDynamicState> ) ) ) {
        string method = args[++index];
        GridWorldLoader<GenericGridWorldStaticState, GenericGridWorldDynamicState> Loader;
        if ( method.Equals( "gwmap" ) ) {
          Loader = new GenericGridWorldLoader( );
        } else if ( method.Equals( "hog" ) ) {
          Loader = new HOGGridWorldLoader( 0, 0, 0, 0 );
        } else if ( method.Equals( "xml" ) ) {
          Loader = new XMLGridWorldLoader( );
        } else {
          throw new CommandLineException( "Unknown Grid World Loader Type" );
        }
        FieldInfo.SetValue( FieldHolder, Loader );
      } else {
        throw new CommandLineException( "Invalid Field Type" );
      }
    }
  }

}