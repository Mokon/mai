#!/bin/sh
export CompLimitMin=1
export CompLimitMax=1001
export CompLimitMethod=arr
export CompLimitIncrement=25
export CeilingRatio=50
export LOS=7
export MaxThreads=1
export PlayList=Playlist/playlist-battleground.txt
# export Algs="1 RealTimeD*Lite"
# export Algs="4 D*Lite RealTimeD*Lite RealTimeD*Lite-Null RealTimeD*Lite-Dead "
# export Algs="15 D*Lite TBA*-LPA* RealTimeD*Lite RealTimeD*Lite-Null RealTimeD*Lite-USample RealTimeD*Lite-DeadAdjusted\
#	RealTimeD*Lite-USampleDeadAdjusted RealTimeD*Lite-Adjusted RealTimeD*Lite-Dead BRTA* TBA*\
#	BRTwA*-Weight=2 RealTimeD*Lite-Weight=2 RealTimeD*Lite-Null-Weight=2 RealTimeD*Lite-Dead-Weight=2"
# export Algs="10 D*Lite RealTimeD*Lite RealTimeD*Lite-Null RealTimeD*Lite-USample RealTimeD*Lite-DeadAdjusted\
#        RealTimeD*Lite-USampleDeadAdjusted RealTimeD*Lite-Adjusted RealTimeD*Lite-Dead BRTA* TBA*"
# export Algs="2 RealTimeD*Lite RealTimeD*Lite-Null"
export Algs="4 RealTimeTBA*-LPA*-UTop RealTimeTBA*-LPA*-BestH RealTimeTBA*-LPA*-UTop-ShortcutDetection RealTimeTBA*-LPA*-BestH-ShortcutDetection" 
export TSTAMP=`date +%s`
if [[ ! -e Output ]] ; then
  mkdir Output
fi
export OutputFolder=Output/Output-$TSTAMP/
mkdir $OutputFolder
export OutputStatusPrefix=Output-Status-
export OutputRaw=Output-Raw
export OutputOrg=Output-Org
export OutputCsvExtension=.csv
export OutputTxtExtension=.txt


exec 3<$PlayList
while read -u 3 Filename OptimalCost
do
FILENAME=`echo $Filename | sed -e 's/\//-/g'`
nohup mono -O=all heuristic_search_harness.bin batch-single -clmin $CompLimitMin -clm $CompLimitMax -cli $CompLimitIncrement \
	-clcr $CeilingRatio -O $OptimalCost -a $Algs -i $Filename \
	-o $OutputFolder$OutputRaw$FILENAME$OutputCsvExtension -clmethod $CompLimitMethod -LOS $LOS -mt $MaxThreads \
     > $OutputFolder$OutputStatusPrefix$FILENAME$OutputTxtExtension
if [[ $? == 0 ]] ; then
  cat $OutputFolder$OutputRaw$OutputCsvExtension $OutputFolder$OutputRaw$FILENAME$OutputCsvExtension > tmp
  more tmp > $OutputFolder$OutputRaw$OutputCsvExtension 
  rm -f tmp
fi
done

nohup mono -O=all heuristic_search_harness.bin organize -i $OutputFolder$OutputRaw$OutputCsvExtension  -o $OutputFolder$OutputOrg$OutputCsvExtension 
