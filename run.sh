#!/bin/sh
export CompLimitMin=1
export CompLimitMax=1200
export CompLimitMethod=exp-1.5
export CompLimitIncrement=25
export MinPathLength=10
export MaxPathLength=200
export CeilingRatio=50
export Directory=maps/game/instance/battleground/
export LOS=7
export MaxThreads=1
# export Algs="1 RealTimeD*Lite"
# export Algs="4 D*Lite RealTimeD*Lite RealTimeD*Lite-Null RealTimeD*Lite-Dead "
# export Algs="15 D*Lite TBA*-LPA* RealTimeD*Lite RealTimeD*Lite-Null RealTimeD*Lite-USample RealTimeD*Lite-DeadAdjusted\
#	RealTimeD*Lite-USampleDeadAdjusted RealTimeD*Lite-Adjusted RealTimeD*Lite-Dead BRTA* TBA*\
#	BRTwA*-Weight=2 RealTimeD*Lite-Weight=2 RealTimeD*Lite-Null-Weight=2 RealTimeD*Lite-Dead-Weight=2"
# export Algs="10 D*Lite RealTimeD*Lite RealTimeD*Lite-Null RealTimeD*Lite-USample RealTimeD*Lite-DeadAdjusted\
#        RealTimeD*Lite-USampleDeadAdjusted RealTimeD*Lite-Adjusted RealTimeD*Lite-Dead BRTA* TBA*"
# export Algs="2 RealTimeD*Lite RealTimeD*Lite-Null"
export Algs="9 D*Lite RealTimeD*Lite RealTimeD*Lite-Null RealTimeD*Lite-USample\
       RealTimeD*Lite-Adjusted RealTimeD*Lite-Dead BRTA* TBA* RealTimeD*Lite-AStar-Ratio=0.2"

export TSTAMP=`date +%s`
export OutputRawPrefix=Output/Output-Raw-
export OutputStatusPrefix=Output/Output-Status-
export OutputOrgPrefix=Output/Output-Org-
export OutputExtension=.csv
export OutputStatusExtension=.txt

export OutRawFileName=$OutputRawPrefix$TSTAMP$OutputExtension
export OutStatusFileName=$OutputStatusPrefix$TSTAMP$OutputStatusExtension
export OutOrgFileName=$OutputOrgPrefix$TSTAMP$OutputExtension

nohup mono -O=all heuristic_search_harness.bin batch -clmin $CompLimitMin -clm $CompLimitMax -cli $CompLimitIncrement \
        -clcr $CeilingRatio -minpl $MinPathLength -maxpl $MaxPathLength -d $Directory -a $Algs \
        -o $OutRawFileName -clmethod $CompLimitMethod -LOS $LOS -mt $MaxThreads > $OutStatusFileName
if [[ $? == 0 ]] ; then
	nohup mono -O=all heuristic_search_harness.bin organize -i $OutRawFileName -o $OutOrgFileName
	../bin/mono -O=all heuristic_search_harness.bin organize -i $OutRawFileName -o $OutOrgFileName
fi
