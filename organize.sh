#!/bin/sh
export OutputOrgPrefix=Output/Output-Org-
export OutputExtension=.csv

export OutOrgFileName=$OutputOrgPrefix$2$OutputExtension

mono -O=all heuristic_search_harness.bin organize -i $1 -o $OutOrgFileName

